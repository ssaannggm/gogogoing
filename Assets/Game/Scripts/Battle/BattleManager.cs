// Assets/Game/Scripts/Battle/BattleManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Services;
using Game.Combat;
using Game.Data;
using Game.Runtime;

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
        [Tooltip("모든 유닛(아군, 적) 생성에 사용할 기본 프리팹")]
        [SerializeField] private GameObject _characterBasePrefab;
        [SerializeField] private Transform[] _allySpawnPoints;
        [SerializeField] private Transform[] _enemySpawnPoints;

        [Header("Flow Options")]
        public float endCheckDelay = 0.05f;
        public float timeLimitSeconds = 0f;

        [Header("Spawn Options")]
        public Vector3 spawnOffset = Vector3.zero;

        public BattleState State { get; private set; } = BattleState.None;
        public static BattleMetrics LastMetrics;

        readonly HashSet<UnitStats> _aliveAllies = new();
        readonly HashSet<UnitStats> _aliveEnemies = new();
        readonly Dictionary<UnitStats, System.Action> _deathHandlers = new();

        int _spawnAllies, _spawnEnemies, _deadAllies, _deadEnemies;
        float _startRealtime, _endRealtime;
        long _memBefore, _memAfter;

        Coroutine _timeLimitCo;
        System.Action<RunEnded> _runEndedHandler;

        // [추가] 싱글톤 인스턴스
        public static BattleManager Instance { get; private set; }

        private EncounterSO _currentEncounter; // [추가] 이번 전투 정보를 저장할 변수
        // [추가] Awake 함수
        void Awake()
        {
            // 싱글톤 인스턴스 설정
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
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

            _currentEncounter = encounter;
            State = BattleState.Spawning;

            // 1. 적군만 먼저 스폰합니다.
            var enemySOs = encounter.enemyUnits;
            StartCoroutine(SpawnTeam(enemySOs, _enemySpawnPoints, Team.Enemy, "[ENEMY]"));

            Debug.Log("[BattleManager] 전투 준비 완료. 적군 스폰됨.");
        }

        // [추가] Inventory UI의 '전투 시작' 버튼이 호출할 함수
        public void StartBattle()
        {
            if (State != BattleState.Spawning) return;

            // 2. RunManager로부터 최신 아군 파티 정보를 가져옵니다.
            var run = GameManager.I?.CurrentRun;
            if (run == null)
            {
                Debug.LogError("[BattleManager] 아군을 스폰할 수 없습니다: CurrentRun이 null입니다.");
                return;
            }

            // PartyMemberState에서 UnitSO 목록을 추출합니다.
            var allySOs = run.GetPartyAsUnitSOs();

            // 3. 아군을 스폰합니다.
            StartCoroutine(SpawnTeam(allySOs, _allySpawnPoints, Team.Ally, "[ALLY]"));

            // 4. 모든 스폰이 끝난 후 전투를 시작합니다.
            StartCoroutine(Co_StartFightingPhase());
        }
        private IEnumerator Co_StartFightingPhase()
        {
            // 모든 스폰 코루틴이 끝날 때까지 잠시 대기
            yield return new WaitForSeconds(0.1f);

            State = BattleState.Fighting;
            Debug.Log("[BattleManager] 전투 시작! Fighting Phase 돌입.");

            if (timeLimitSeconds > 0f)
                _timeLimitCo = StartCoroutine(Co_TimeLimit(timeLimitSeconds));
        }
        public void InitializeBattle(EncounterSO encounter, IReadOnlyList<UnitSO> allySOs)
        {
            if (State != BattleState.None)
            {
                Debug.LogWarning("[BattleManager] 이미 전투가 진행 중입니다. InitializeBattle 호출을 무시합니다.");
                return;
            }
            StopAllCoroutines();
            StartCoroutine(Co_StartBattle(encounter, allySOs));
        }

        public void AbortBattle()
        {
            if (State == BattleState.Ending) return;
            StopAllCoroutines();
            _timeLimitCo = null;
            StartCoroutine(Co_EndBattle(victory: false, reason: "Aborted"));
        }

        IEnumerator Co_StartBattle(EncounterSO encounter, IReadOnlyList<UnitSO> allySOs)
        {
            _spawnAllies = _spawnEnemies = _deadAllies = _deadEnemies = 0;
            _startRealtime = Time.realtimeSinceStartup;
            _memBefore = System.GC.GetTotalMemory(false);

            State = BattleState.Spawning;
            ClearRegistry();

            yield return SpawnTeam(allySOs, _allySpawnPoints, Team.Ally, "[ALLY]");

            // [수정] EncounterSO에 정의된 enemyUnits (UnitSO 배열)를 사용합니다. 이제 이 부분이 정상 동작합니다.
            yield return SpawnTeam(encounter.enemyUnits, _enemySpawnPoints, Team.Enemy, "[ENEMY]");

            State = BattleState.Fighting;

            if (timeLimitSeconds > 0f)
                _timeLimitCo = StartCoroutine(Co_TimeLimit(timeLimitSeconds));
        }

        IEnumerator SpawnTeam(IReadOnlyList<UnitSO> unitSOs, Transform[] points, Team team, string tag)
        {
            int unitCount = unitSOs?.Count ?? 0;
            if (unitCount == 0)
            {
                Debug.LogWarning($"{tag} 스폰 스킵: 유닛이 0명입니다.");
                yield break;
            }
            if (!_characterBasePrefab)
            {
                Debug.LogError($"{tag} 스폰 실패: Character Base Prefab이 지정되지 않았습니다!");
                yield break;
            }

            for (int i = 0; i < unitCount; i++)
            {
                var unitSO = unitSOs[i];
                if (!unitSO) { Debug.LogWarning($"{tag} null UnitSO at index {i}"); continue; }

                Transform sp = (points?.Length > 0) ? points[i % points.Length] : null;
                var pos = (sp ? sp.position : transform.position) + spawnOffset;
                var rot = sp ? sp.rotation : Quaternion.identity;

                // 1. 베이스 프리팹 생성
                var go = Instantiate(_characterBasePrefab, pos, rot);
                go.name = $"{tag}_{unitSO.name}_{i}";

                // 2. 적군일 경우 레이어 자동 변경
                if (team == Team.Enemy)
                {
                    int enemyLayer = LayerMask.NameToLayer("Enemy");
                    if (enemyLayer != -1) go.SetLayerRecursively(enemyLayer);
                    else Debug.LogError("'Enemy' 레이어가 Project Settings에 정의되지 않았습니다!");
                }

                go.SetActive(true);

                // 3. UnitSO 데이터로 기본 외형 적용
                var visualApplier = go.GetComponent<SpumVisualApplier>();
                if (visualApplier) visualApplier.ApplyVisuals(unitSO);

                // 4. UnitSO 데이터로 초기 장비 장착
                var loadout = go.GetComponent<UnitLoadout>();
                if (loadout)
                {
                    var equip = unitSO.defaultEquipment;
                    if (equip.rightHand) loadout.Equip(equip.rightHand);
                    if (equip.leftHand) loadout.Equip(equip.leftHand);
                    if (equip.helmet) loadout.Equip(equip.helmet);
                    if (equip.armor) loadout.Equip(equip.armor);
                }

                // 5. 필수 컴포넌트 확인 및 초기화
                var st = go.GetComponent<UnitStats>();
                var hp = go.GetComponent<Health>();
                if (!st || !hp)
                {
                    Debug.LogWarning($"[BattleManager] {go.name} 프리팹에 UnitStats/Health 컴포넌트가 없습니다.");
                    Destroy(go);
                    continue;
                }

                // UnitStats의 maxHp 값으로 Health 컴포넌트 초기화
                //hp.Initialize(Mathf.RoundToInt(st.maxHp));

                // 팀 설정
                st.team = team;

                // 6. 타겟 레이어 자동 설정
                var weapon = go.GetComponentInChildren<MeleeWeapon>(true);
                var targeter = go.GetComponent<Targeter>();
                if (weapon) weapon.UpdateTargetMaskByTeam(team);
                if (targeter) targeter.UpdateTargetMaskByTeam(team);

                // 7. 전투 시스템에 유닛 등록
                System.Action handler = () => OnUnitDeath(st);
                _deathHandlers[st] = handler;
                hp.OnDeath += handler;

                if (team == Team.Ally) { _aliveAllies.Add(st); _spawnAllies++; }
                else { _aliveEnemies.Add(st); _spawnEnemies++; }

                Debug.Log($"{tag} 스폰 -> {go.name} at {pos}");
                yield return null;
            }
        }

        // --- 이하 코드는 변경 사항 없습니다 ---

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

            BattleResult result;
            if (reason == "Draw" || reason == "Time limit") result = BattleResult.Draw;
            else result = victory ? BattleResult.Victory : BattleResult.Defeat;

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
    // BattleManager.cs 파일 맨 아래, namespace 밖이나 안에 추가
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 게임 오브젝트와 모든 자식 오브젝트의 레이어를 재귀적으로 변경합니다.
        /// </summary>
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