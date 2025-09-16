// Assets/Game/Scripts/Dev/BattleSoakRunner.cs (최종 수정본)
using System.Collections;
using UnityEngine;
using Game.Battle;
using Game.Data;
using Game.Services; // GameManager를 사용하기 위해 추가
using Game.Runtime; // RunManager를 사용하기 위해 추가

public sealed class BattleSoakRunner : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private BattleManager _battleManager;

    [Header("Test Data")]
    [Tooltip("반복 테스트할 전투 데이터")]
    [SerializeField] private EncounterSO _testEncounter;

    [Tooltip("반복 테스트할 아군 파티 구성")]
    [SerializeField] private UnitSO[] _testPartyUnits;

    [Header("Options")]
    [SerializeField, Min(1)] private int _iterations = 10;
    [SerializeField] private float _delayBetween = 0.25f;

    private int _errorCount;
    private float _worstFrame;

    void OnEnable()
    {
        Application.logMessageReceived += OnLog;
        _worstFrame = 0f;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= OnLog;
    }

    void Start()
    {
        if (!_battleManager) _battleManager = FindFirstObjectByType<BattleManager>();

        if (!_battleManager || !_testEncounter || _testPartyUnits == null || _testPartyUnits.Length == 0)
        {
            Debug.LogError("[BattleSoakRunner] 테스트에 필요한 BattleManager, Encounter, 또는 Party Units가 지정되지 않았습니다!");
            enabled = false;
            return;
        }

        StartCoroutine(Co_Run());
    }

    void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            _errorCount++;
    }

    IEnumerator Co_Run()
    {
        // 테스트를 위해 현재 RunManager를 가져옵니다.
        var runManager = GameManager.I?.CurrentRun;
        if (runManager == null)
        {
            Debug.LogError("[BattleSoakRunner] 테스트를 실행할 수 없습니다: CurrentRun이 없습니다. 게임을 한 번 시작한 후 테스트 씬을 실행하세요.");
            yield break;
        }

        for (int i = 0; i < _iterations; i++)
        {
            // --- ✨ 여기가 핵심 수정 부분 ✨ ---
            // 1. RunManager에 테스트용 파티 정보를 설정합니다.
            runManager.SetPartyFromUnitSOs(_testPartyUnits);

            // 2. 새로운 전투 흐름에 맞춰 함수를 호출합니다.
            _battleManager.PrepareBattle(_testEncounter);
            _battleManager.StartBattle();
            // --- ✨ 수정 끝 ✨ ---

            while (_battleManager.State != BattleState.None)
            {
                if (Time.deltaTime > _worstFrame) _worstFrame = Time.deltaTime;
                yield return null;
            }

            var m = BattleManager.LastMetrics;
            Debug.Log($"[Soak] #{i + 1}/{_iterations} done: {(m.victory ? "V" : "D")} reason={m.endReason} " +
                      $"t={m.elapsedSeconds:0.000}s memΔ={m.managedMemDelta / 1024f:0.0}KB");

            yield return new WaitForSeconds(_delayBetween);
        }

        Debug.Log($"[Soak] Completed {_iterations} runs. errors={_errorCount} worstFrame={(1f / _worstFrame):0.0} FPS");
    }
}