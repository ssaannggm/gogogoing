using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Data;
using Game.Runtime;
using Game.Services;
using Game.Battle; // BattleManager에 접근하기 위해 추가

// ★ 네임스페이스를 사용하신다면, 본인 프로젝트에 맞게 추가/수정해주세요.
// namespace Game.UI { ... }

public class RewardChoiceUI : MonoBehaviour
{
    public static RewardChoiceUI Instance { get; private set; }

    [Header("UI 구성 요소")]
    [SerializeField] private GameObject rewardChoicePanel; // 선택지 전체를 감싸는 패널
    [SerializeField] private Button choiceButtonPrefab; // 선택지 버튼 프리팹 (버튼과 Text 자식 필요)
    [SerializeField] private Transform buttonContainer; // 버튼들이 생성될 부모 Transform

    private List<GameObject> _generatedButtons = new List<GameObject>();
    private bool _isChoiceMade = false; // 중복 선택 방지 플래그

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // UI가 씬을 넘나들며 유지되어야 한다면 이 코드의 주석을 해제하세요.
        }
        else
        {
            Destroy(gameObject);
        }

        rewardChoicePanel.SetActive(false); // 시작할 때 UI는 항상 숨겨져 있어야 합니다.
    }

    /// <summary>
    /// 보상 선택지 목록(RewardTableSO)을 받아 UI를 활성화하고 버튼들을 생성합니다.
    /// BattleManager가 이 함수를 호출합니다.
    /// </summary>
    public void ShowChoices(List<RewardTableSO> choices)
    {
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("표시할 보상 선택지가 없습니다.");
            // 보상이 없다면 즉시 전투를 종료시켜야 합니다.
            BattleManager.Instance.FinalizeBattleEnd(true, "Enemies wiped (No rewards)");
            return;
        }

        // 이전 버튼들 깨끗하게 삭제
        foreach (var button in _generatedButtons)
        {
            Destroy(button);
        }
        _generatedButtons.Clear();

        _isChoiceMade = false; // 선택 가능 상태로 초기화
        rewardChoicePanel.SetActive(true); // 패널 활성화

        // 각 선택지에 대한 버튼 생성
        foreach (var choiceTable in choices)
        {
            if (choiceButtonPrefab == null || buttonContainer == null)
            {
                Debug.LogError("RewardChoiceUI에 버튼 프리팹 또는 컨테이너가 연결되지 않았습니다!");
                return;
            }

            Button newButton = Instantiate(choiceButtonPrefab, buttonContainer);

            // (선택) RewardTableSO에 설명 필드(예: public string description;)를 추가하면
            // 아래 코드로 버튼에 보상 설명을 표시할 수 있습니다.
            // var buttonText = newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            // if (buttonText) buttonText.text = choiceTable.description;

            // 버튼 클릭 시 OnChoiceMade 함수를 해당 보상 테이블과 함께 호출하도록 리스너 연결
            newButton.onClick.AddListener(() => OnChoiceMade(choiceTable));

            _generatedButtons.Add(newButton.gameObject);
        }
    }

    /// <summary>
    /// 플레이어가 보상 버튼을 클릭했을 때 호출되는 함수입니다.
    /// </summary>
    private void OnChoiceMade(RewardTableSO chosenRewardTable)
    {
        if (_isChoiceMade) return;
        _isChoiceMade = true;

        // 1. 보상 생성
        Reward generatedReward = chosenRewardTable.GenerateReward();
        var runManager = GameManager.I.CurrentRun;

        // 2. RunManager에 보상 꾸러미를 '통째로' 전달
        if (runManager != null)
        {
            runManager.ApplyReward(generatedReward);
        }

        Debug.Log($"보상 선택 완료! (통합 함수 호출)");

        // 3. UI 닫고 다음 단계로
        rewardChoicePanel.SetActive(false);
        BattleManager.Instance.FinalizeBattleEnd(true, "Enemies wiped (Reward chosen)");
    }
}