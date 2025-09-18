using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Data;
using Game.Runtime;
using Game.Services;
using Game.Battle; // BattleManager�� �����ϱ� ���� �߰�

// �� ���ӽ����̽��� ����ϽŴٸ�, ���� ������Ʈ�� �°� �߰�/�������ּ���.
// namespace Game.UI { ... }

public class RewardChoiceUI : MonoBehaviour
{
    public static RewardChoiceUI Instance { get; private set; }

    [Header("UI ���� ���")]
    [SerializeField] private GameObject rewardChoicePanel; // ������ ��ü�� ���δ� �г�
    [SerializeField] private Button choiceButtonPrefab; // ������ ��ư ������ (��ư�� Text �ڽ� �ʿ�)
    [SerializeField] private Transform buttonContainer; // ��ư���� ������ �θ� Transform

    private List<GameObject> _generatedButtons = new List<GameObject>();
    private bool _isChoiceMade = false; // �ߺ� ���� ���� �÷���

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // UI�� ���� �ѳ���� �����Ǿ�� �Ѵٸ� �� �ڵ��� �ּ��� �����ϼ���.
        }
        else
        {
            Destroy(gameObject);
        }

        rewardChoicePanel.SetActive(false); // ������ �� UI�� �׻� ������ �־�� �մϴ�.
    }

    /// <summary>
    /// ���� ������ ���(RewardTableSO)�� �޾� UI�� Ȱ��ȭ�ϰ� ��ư���� �����մϴ�.
    /// BattleManager�� �� �Լ��� ȣ���մϴ�.
    /// </summary>
    public void ShowChoices(List<RewardTableSO> choices)
    {
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("ǥ���� ���� �������� �����ϴ�.");
            // ������ ���ٸ� ��� ������ ������Ѿ� �մϴ�.
            BattleManager.Instance.FinalizeBattleEnd(true, "Enemies wiped (No rewards)");
            return;
        }

        // ���� ��ư�� �����ϰ� ����
        foreach (var button in _generatedButtons)
        {
            Destroy(button);
        }
        _generatedButtons.Clear();

        _isChoiceMade = false; // ���� ���� ���·� �ʱ�ȭ
        rewardChoicePanel.SetActive(true); // �г� Ȱ��ȭ

        // �� �������� ���� ��ư ����
        foreach (var choiceTable in choices)
        {
            if (choiceButtonPrefab == null || buttonContainer == null)
            {
                Debug.LogError("RewardChoiceUI�� ��ư ������ �Ǵ� �����̳ʰ� ������� �ʾҽ��ϴ�!");
                return;
            }

            Button newButton = Instantiate(choiceButtonPrefab, buttonContainer);

            // (����) RewardTableSO�� ���� �ʵ�(��: public string description;)�� �߰��ϸ�
            // �Ʒ� �ڵ�� ��ư�� ���� ������ ǥ���� �� �ֽ��ϴ�.
            // var buttonText = newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            // if (buttonText) buttonText.text = choiceTable.description;

            // ��ư Ŭ�� �� OnChoiceMade �Լ��� �ش� ���� ���̺�� �Բ� ȣ���ϵ��� ������ ����
            newButton.onClick.AddListener(() => OnChoiceMade(choiceTable));

            _generatedButtons.Add(newButton.gameObject);
        }
    }

    /// <summary>
    /// �÷��̾ ���� ��ư�� Ŭ������ �� ȣ��Ǵ� �Լ��Դϴ�.
    /// </summary>
    private void OnChoiceMade(RewardTableSO chosenRewardTable)
    {
        if (_isChoiceMade) return;
        _isChoiceMade = true;

        // 1. ���� ����
        Reward generatedReward = chosenRewardTable.GenerateReward();
        var runManager = GameManager.I.CurrentRun;

        // 2. RunManager�� ���� �ٷ��̸� '��°��' ����
        if (runManager != null)
        {
            runManager.ApplyReward(generatedReward);
        }

        Debug.Log($"���� ���� �Ϸ�! (���� �Լ� ȣ��)");

        // 3. UI �ݰ� ���� �ܰ��
        rewardChoicePanel.SetActive(false);
        BattleManager.Instance.FinalizeBattleEnd(true, "Enemies wiped (Reward chosen)");
    }
}