// Assets/Game/Scripts/Battle/BattleManager.cs (수정본 전체)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Services;
using Game.Combat;
using Game.Data;
using Game.Runtime;
using Game.Items; // EquipSlot 등을 사용하기 위해 추가

namespace Game.Battle
{
    public enum BattleState { None, Spawning, Fighting, Ending }

    public struct BattleMetrics
    {
        public bool valid;
        public int alliesSpawned, enemiesSpawned;
        public int alliesDied, enemiesDied;
        public float elapsedSeconds;
        public string endReason;
        public bool victory;
        public long managedMemBefore, managedMemAfter, managedMemDelta;
    }

    public sealed class BattleManager : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private GameObject _characterBasePrefab;
        [SerializeField] private Transform[] _allySpawnPoints;
        [SerializeField] private Transform[] _enemySpawnPoints;

        [Header("Flow Options")]
        public float endCheckDelay = 0.05f;
        public float timeLimitSeconds = 0f;

        public BattleState State { get; private set; } = BattleState.None;
        public static BattleMetrics LastMetrics;

        // --- 내부 변수들 ---
        private readonly HashSet<UnitStats> _aliveAllies = new();
        private readonly HashSet<UnitStats> _aliveEnemies = new();
        private readonly Dictionary<UnitStats, System.Action> _deathHandlers = new();
        private int _spawnAllies, _spawnEnemies, _deadAllies, _deadEnemies;
        private float _startRealtime, _endRealtime;
        private long _memBefore, _memAfter;
        private Coroutine _timeLimitCo;
        private System.Action<RunEnded> _runEndedHandler;

        public static BattleManager Instance { get; private set; }

        // ★ 추가: 인벤토리 토글 게이트 캐싱
        private BattleInventoryToggle _invToggle;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            // 배틀 씬에서 한 번 캐싱
            _invToggle = FindObjectOfType<BattleInventoryToggle>(true);
            if (_invToggle == null)
                Debug.LogWarning("[BattleManager] BattleInventoryToggle을 씬에서 찾지 못했습니다. I 토글 연동이 비활성화됩니다.");
        }

        void OnEnable()
        {
            _runEndedHandler = _ => AbortBattle();
            EventBus.Subscribe(_runEndedHandler);
        }

        void OnDisable()
        {
            if (_runEndedHandler != null) EventBus.Unsubscribe(_runEndedHandler);
            _runEndedHandler = null;
        }

        public void PrepareBattle(EncounterSO encounter)
        {
            if (State != BattleState.None) return;

            _spawnAllies = _spawnEnemies = _deadAllies = _deadEnemies = 0;
            _startRealtime = Time.realtimeSinceStartup;
            _memBefore = System.GC.GetTotalMemory(false);

            State = BattleState.Spawning;
            ClearRegistry();

            // ★ 페이즈: 세팅 가능 모드
            _invToggle?.SetPhase(BattlePhase.Setup);

            StartCoroutine(SpawnEnemies(encounter.enemyUnits, _enemySpawnPoints));
            Debug.Log("[BattleManager] 전투 준비 완료. 적군 스폰됨. 아군 배치를 기다립니다.");
        }

        public void StartBattle()
        {
            if (State != BattleState.Spawning) return;

            StartCoroutine(SpawnAllies(_allySpawnPoints));
            StartCoroutine(Co_StartFightingPhase());
        }

        private IEnumerator SpawnAllies(Transform[] spawnPoints)
        {
            var run = GameManager.I.CurrentRun;
            if (run == null)
            {
                Debug.LogError("아군 스폰 실패: CurrentRun이 null입니다.");
                yield break;
            }

            var party = run.PartyState;
            for (int i = 0; i < party.Count; i++)
            {
                var memberState = party[i];
                var unitSO = GameManager.I.Data.GetUnitById(memberState.unitId);

                var unitStats = SpawnSingleUnit(unitSO, i, spawnPoints, Team.Ally, "[ALLY]");
                if (unitStats != null)
                {
                    unitStats.Initialize(unitSO, memberState);

                    // [추가] UnitLoadout을 찾아 시각/VFX 초기화
                    var loadout = unitStats.GetComponent<UnitLoadout>();
                    if (loadout) loadout.InitializeLoadout(memberState, GameManager.I.Data);

                    var hp = unitStats.GetComponent<Health>();
                    if (hp) hp.Initialize(Mathf.RoundToInt(unitStats.CurrentStats.maxHp));

                    _aliveAllies.Add(unitStats);
                    _spawnAllies++;
                }
                yield return null;
            }
        }

        private IEnumerator SpawnEnemies(IReadOnlyList<UnitSO> enemySOs, Transform[] spawnPoints)
        {
            for (int i = 0; i < enemySOs.Count; i++)
            {
                var unitSO = enemySOs[i];
                var unitStats = SpawnSingleUnit(unitSO, i, spawnPoints, Team.Enemy, "[ENEMY]");
                if (unitStats != null)
                {
                    var tempEnemyState = new PartyMemberState(unitSO.unitId);
                    var equip = unitSO.defaultEquipment;
                    if (equip.rightHand) tempEnemyState.equippedItemIds[EquipSlot.RightHand] = equip.rightHand.itemId;
                    if (equip.leftHand) tempEnemyState.equippedItemIds[EquipSlot.LeftHand] = equip.leftHand.itemId;
                    if (equip.helmet) tempEnemyState.equippedItemIds[EquipSlot.Helmet] = equip.helmet.itemId;
                    if (equip.armor) tempEnemyState.equippedItemIds[EquipSlot.Armor] = equip.armor.itemId;

                    unitStats.Initialize(unitSO, tempEnemyState);

                    // [추가] UnitLoadout을 찾아 시각/VFX 초기화
                    var loadout = unitStats.GetComponent<UnitLoadout>();
                    if (loadout) loadout.InitializeLoadout(tempEnemyState, GameManager.I.Data);

                    var hp = unitStats.GetComponent<Health>();
                    if (hp) hp.Initialize(Mathf.RoundToInt(unitStats.CurrentStats.maxHp));

                    _aliveEnemies.Add(unitStats);
                    _spawnEnemies++;
                }
                yield return null;
            }
        }

        private UnitStats SpawnSingleUnit(UnitSO unitSO, int index, Transform[] points, Team team, string tag)
        {
            if (!unitSO) { Debug.LogWarning($"{tag} null UnitSO at index {index}"); return null; }
            if (!_characterBasePrefab) { Debug.LogError($"{tag} 스폰 실패: Character Base Prefab이 없습니다!"); return null; }

            Transform sp = (points?.Length > 0) ? points[index % points.Length] : transform;
            var pos = sp.position;
            var rot = sp.rotation;

            var go = Instantiate(_characterBasePrefab, pos, rot);
            go.name = $"{tag}_{unitSO.name}_{index}";

            if (team == Team.Enemy)
            {
                int enemyLayer = LayerMask.NameToLayer("Enemy");
                if (enemyLayer != -1) go.SetLayerRecursively(enemyLayer);
                else Debug.LogError("'Enemy' 레이어가 없습니다!");
            }

            var st = go.GetComponent<UnitStats>();
            var hp = go.GetComponent<Health>();
            if (!st || !hp)
            {
                Debug.LogError($"[BattleManager] {go.name} 프리팹에 UnitStats나 Health가 없습니다.", go);
                Destroy(go);
                return null;
            }

            var visualApplier = go.GetComponent<SpumVisualApplier>();
            if (visualApplier) visualApplier.ApplyVisuals(unitSO);

            st.team = team;
            var weapon = go.GetComponentInChildren<MeleeWeapon>(true);
            var targeter = go.GetComponent<Targeter>();
            if (weapon) weapon.UpdateTargetMaskByTeam(team);
            if (targeter) targeter.UpdateTargetMaskByTeam(team);

            System.Action handler = () => OnUnitDeath(st);
            _deathHandlers[st] = handler;
            hp.OnDeath += handler;

            go.SetActive(true);
            Debug.Log($"{tag} 스폰 -> {go.name} at {pos}");
            return st;
        }

        private IEnumerator Co_StartFightingPhase()
        {
            yield return new WaitForSeconds(0.1f);
            State = BattleState.Fighting;

            // ★ 페이즈: 전투 진행(읽기전용)
            _invToggle?.SetPhase(BattlePhase.Running);

            Debug.Log("[BattleManager] 전투 시작! Fighting Phase 돌입.");
            if (timeLimitSeconds > 0f)
                _timeLimitCo = StartCoroutine(Co_TimeLimit(timeLimitSeconds));
        }

        public void AbortBattle()
        {
            if (State == BattleState.Ending) return;
            StopAllCoroutines();
            _timeLimitCo = null;

            // ★ 페이즈: 결과 단계로 전환
            _invToggle?.SetPhase(BattlePhase.Results);

            StartCoroutine(Co_EndBattle(victory: false, reason: "Aborted"));
        }

        void OnUnitDeath(UnitStats st)
        {
            bool wasAlly = _aliveAllies.Remove(st);
            bool wasEnemy = _aliveEnemies.Remove(st);
            if (wasAlly) _deadAllies++;
            if (wasEnemy) _deadEnemies++;
            StartCoroutine(Co_CheckEndSoon());
        }

        IEnumerator Co_CheckEndSoon()
        {
            if (State != BattleState.Fighting) yield break;
            yield return new WaitForSeconds(endCheckDelay);
            bool alliesDead = _aliveAllies.Count == 0;
            bool enemiesDead = _aliveEnemies.Count == 0;
            if (alliesDead && enemiesDead)
                StartCoroutine(Co_EndBattle(victory: false, reason: "Draw"));
            else if (enemiesDead)
                StartCoroutine(Co_EndBattle(victory: true, reason: "Enemies wiped"));
            else if (alliesDead)
                StartCoroutine(Co_EndBattle(victory: false, reason: "Allies wiped"));
        }

        IEnumerator Co_TimeLimit(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (State == BattleState.Fighting)
                StartCoroutine(Co_EndBattle(victory: false, reason: "Time limit"));
        }

        IEnumerator Co_EndBattle(bool victory, string reason)
        {
            if (State == BattleState.Ending) yield break;
            State = BattleState.Ending;

            // ★ 페이즈: 결과 단계로 전환
            _invToggle?.SetPhase(BattlePhase.Results);

            if (_timeLimitCo != null) { StopCoroutine(_timeLimitCo); _timeLimitCo = null; }
            FreezeUnits(_aliveAllies);
            FreezeUnits(_aliveEnemies);
            yield return CleanupUnits();
            _endRealtime = Time.realtimeSinceStartup;
            _memAfter = System.GC.GetTotalMemory(false);

            LastMetrics = new BattleMetrics
            {
                valid = true,
                alliesSpawned = _spawnAllies,
                enemiesSpawned = _spawnEnemies,
                alliesDied = _deadAllies,
                enemiesDied = _deadEnemies,
                elapsedSeconds = Mathf.Max(0f, _endRealtime - _startRealtime),
                endReason = reason,
                victory = victory,
                managedMemBefore = _memBefore,
                managedMemAfter = _memAfter,
                managedMemDelta = _memAfter - _memBefore
            };

            BattleResult result = (reason == "Draw" || reason == "Time limit") ? BattleResult.Draw : (victory ? BattleResult.Victory : BattleResult.Defeat);
            EventBus.Raise(new BattleEnded(GameManager.I?.CurrentRun, result));
            State = BattleState.None;

            Debug.Log($"[BattleManager] Ended. Victory={victory} ({reason}) " +
                      $"| t={LastMetrics.elapsedSeconds:0.000}s " +
                      $"| spawn A/E={_spawnAllies}/{_spawnEnemies} " +
                      $"| dead A/E={_deadAllies}/{_deadEnemies} " +
                      $"| memΔ={LastMetrics.managedMemDelta / 1024f:0.0} KB");
        }

        void FreezeUnits(HashSet<UnitStats> set)
        {
            if (set == null || set.Count == 0) return;
            var snapshot = new List<UnitStats>(set);
            foreach (var unit in snapshot)
            {
                if (!unit) continue;
                var ai = unit.GetComponentInParent<AIController>(); if (ai) ai.enabled = false;
                var rb = unit.GetComponentInParent<Rigidbody2D>(); if (rb) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }
                var anim = unit.GetComponentInChildren<Animator>(); if (anim) anim.speed = 0f;
            }
        }

        IEnumerator CleanupUnits()
        {
            foreach (var kv in _deathHandlers)
            {
                var st = kv.Key;
                if (!st) continue;
                var hp = st.GetComponent<Health>();
                if (hp != null) hp.OnDeath -= kv.Value;
            }
            _deathHandlers.Clear();
            foreach (var st in _aliveAllies) if (st) Destroy(st.gameObject);
            foreach (var st in _aliveEnemies) if (st) Destroy(st.gameObject);
            _aliveAllies.Clear();
            _aliveEnemies.Clear();
            yield return null;
        }

        void ClearRegistry()
        {
            _deathHandlers.Clear();
            _aliveAllies.Clear();
            _aliveEnemies.Clear();
        }
    }

    public static class GameObjectExtensions
    {
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
    }
}
