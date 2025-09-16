using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
using UnityEngine;
using Game.Battle;
using Game.Data;

public sealed class BattleSoakRunner : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private BattleManager _battleManager;

    [Header("Test Data")]
    [Tooltip("반복 테스트할 전투 데이터")]
    [SerializeField] private EncounterSO _testEncounter;

    // [수정] PartyPresetSO 대신 UnitSO 배열을 직접 사용합니다.
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

        if (!_battleManager)
        {
            Debug.LogError("[BattleSoakRunner] BattleManager가 지정되지 않았습니다!");
            enabled = false;
            return;
        }
        // [수정] _testPartyUnits가 비어있는지 확인합니다.
        if (!_testEncounter || _testPartyUnits == null || _testPartyUnits.Length == 0)
        {
            Debug.LogError("[BattleSoakRunner] Test Encounter 또는 Test Party Units가 지정되지 않았습니다!");
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
        for (int i = 0; i < _iterations; i++)
        {
            // [수정] _testParty.members 대신 _testPartyUnits 배열을 직접 전달합니다.
            _battleManager.InitializeBattle(_testEncounter, _testPartyUnits);

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