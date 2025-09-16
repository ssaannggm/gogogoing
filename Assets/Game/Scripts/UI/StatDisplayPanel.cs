// StatDisplayPanel.cs - ��ü �ڵ�
using UnityEngine;
using TMPro;
using Game.Data; // UnitSO, PartyMemberState ����� ���� �߰�
using Game.Services; // GameManager ����� ���� �߰�
using Game.Items; // StatMods ����� ���� �߰�

public class StatDisplayPanel : MonoBehaviour
{
    [Header("�ٽ� ���� �ؽ�Ʈ")]
    [SerializeField] private TextMeshProUGUI maxHpText;
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI attackRangeText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;

    [Header("�� ���� �ؽ�Ʈ (���� ����)")]
    [SerializeField] private TextMeshProUGUI healthRegenText;
    [SerializeField] private TextMeshProUGUI lifeOnKillText;
    [SerializeField] private TextMeshProUGUI omnivampText;
    [SerializeField] private TextMeshProUGUI critChanceText;
    [SerializeField] private TextMeshProUGUI critDamageText;
    [SerializeField] private TextMeshProUGUI damageIncreaseText;
    [SerializeField] private TextMeshProUGUI magicResistText;
    [SerializeField] private TextMeshProUGUI evasionChanceText;
    [SerializeField] private TextMeshProUGUI blockChanceText;
    [SerializeField] private TextMeshProUGUI blockPowerText;
    [SerializeField] private TextMeshProUGUI damageReductionText;
    [SerializeField] private TextMeshProUGUI abilityHasteText;
    [SerializeField] private TextMeshProUGUI tenacityText;

    /// <summary>
    /// [������] UnitSO�� PartyMemberState�� ������� ������ ����ϰ� UI�� ������Ʈ�մϴ�.
    /// </summary>
    public void UpdateStats(UnitSO unitSO, PartyMemberState memberState)
    {
        // ��ȿ�� �����Ͱ� ������ ��� �ؽ�Ʈ�� �ʱ�ȭ�ϰ� ����
        if (unitSO == null || memberState == null)
        {
            ClearAllText();
            return;
        }

        // --- 1. ���� ��� ���� ---
        // �⺻ ���� ��������
        StatSet baseStats = unitSO.baseStats;

        // ���������� ���� �߰� ������ �ջ��� ����
        StatMods totalItemMods = new StatMods();

        // ������ īŻ�α� ���� ��������
        var dataCatalog = GameManager.I?.Data;
        if (dataCatalog == null)
        {
            Debug.LogError("DataCatalog�� ã�� �� �����ϴ�!");
            return;
        }

        // ������ ��� �������� StatMods�� ���ϱ�
        foreach (var equippedItem in memberState.equippedItemIds)
        {
            var itemSO = dataCatalog.GetItemById(equippedItem.Value);
            if (itemSO != null)
            {
                totalItemMods = totalItemMods.Add(itemSO.statMods);
            }
        }

        // ���� ���� ���
        StatSet finalStats = baseStats.ApplyMods(totalItemMods);

        // --- 2. ���� ������ UI�� ǥ�� ---
        maxHpText.text = finalStats.maxHp.ToString("F0");
        attackPowerText.text = finalStats.attackPower.ToString("F0");
        defenseText.text = finalStats.defense.ToString("F0");
        attackSpeedText.text = finalStats.attackSpeed.ToString("F2");
        attackRangeText.text = finalStats.attackRange.ToString("F1");
        moveSpeedText.text = finalStats.moveSpeed.ToString("F1");

        if (healthRegenText) healthRegenText.text = finalStats.healthRegen.ToString("F1");
        if (lifeOnKillText) lifeOnKillText.text = finalStats.lifeOnKill.ToString("F0");
        if (omnivampText) omnivampText.text = finalStats.omnivamp.ToString("F1") + "%";
        if (critChanceText) critChanceText.text = finalStats.critChance.ToString("F1") + "%";
        if (critDamageText) critDamageText.text = finalStats.critDamage.ToString("F0") + "%";
        if (damageIncreaseText) damageIncreaseText.text = finalStats.damageIncrease.ToString("F1") + "%";
        if (magicResistText) magicResistText.text = finalStats.magicResist.ToString("F0");
        if (evasionChanceText) evasionChanceText.text = finalStats.evasionChance.ToString("F1") + "%";
        if (blockChanceText) blockChanceText.text = finalStats.blockChance.ToString("F1") + "%";
        if (blockPowerText) blockPowerText.text = finalStats.blockPower.ToString("F1") + "%";
        if (damageReductionText) damageReductionText.text = finalStats.damageReduction.ToString("F1") + "%";
        if (abilityHasteText) abilityHasteText.text = finalStats.abilityHaste.ToString("F0");
        if (tenacityText) tenacityText.text = finalStats.tenacity.ToString("F1") + "%";
    }

    private void ClearAllText()
    {
        maxHpText.text = "-";
        attackPowerText.text = "-";
        defenseText.text = "-";
        attackSpeedText.text = "-";
        attackRangeText.text = "-";
        moveSpeedText.text = "-";

        if (healthRegenText) healthRegenText.text = "-";
        if (lifeOnKillText) lifeOnKillText.text = "-";
        if (omnivampText) omnivampText.text = "-";
        if (critChanceText) critChanceText.text = "-";
        if (critDamageText) critDamageText.text = "-";
        if (damageIncreaseText) damageIncreaseText.text = "-";
        if (magicResistText) magicResistText.text = "-";
        if (evasionChanceText) evasionChanceText.text = "-";
        if (blockChanceText) blockChanceText.text = "-";
        if (blockPowerText) blockPowerText.text = "-";
        if (damageReductionText) damageReductionText.text = "-";
        if (abilityHasteText) abilityHasteText.text = "-";
        if (tenacityText) tenacityText.text = "-";
    }
}