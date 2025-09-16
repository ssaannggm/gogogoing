using UnityEngine;
using TMPro;

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

    public void UpdateStats(UnitStats stats)
    {
        // stats�� null�̸� ��� �ؽ�Ʈ�� �ʱ�ȭ
        if (stats == null)
        {
            // ��� TextMeshProUGUI �ʵ带 ã�� �ʱ�ȭ�ϴ� ���� (���÷��� ��� ��)
            // �����ϰԴ� �� �ؽ�Ʈ�� "-"�� ����
            maxHpText.text = "-";
            attackPowerText.text = "-";
            // ...
            return;
        }

        // --- �ٽ� ���� ������Ʈ ---
        maxHpText.text = stats.CurrentMaxHp.ToString("F0");
        attackPowerText.text = stats.CurrentAttackPower.ToString("F0");
        defenseText.text = stats.CurrentDefense.ToString("F0");
        attackSpeedText.text = stats.CurrentAttackSpeed.ToString("F2");
        attackRangeText.text = stats.CurrentAttackRange.ToString("F1");
        moveSpeedText.text = stats.CurrentMoveSpeed.ToString("F1");

        // --- �� ���� ������Ʈ (����� ��쿡��) ---
        if (healthRegenText) healthRegenText.text = stats.CurrentHealthRegen.ToString("F1");
        if (lifeOnKillText) lifeOnKillText.text = stats.CurrentLifeOnKill.ToString("F0");
        if (omnivampText) omnivampText.text = stats.CurrentOmnivamp.ToString("F1") + "%";
        if (critChanceText) critChanceText.text = stats.CurrentCritChance.ToString("F1") + "%";
        if (critDamageText) critDamageText.text = stats.CurrentCritDamage.ToString("F0") + "%";
        if (damageIncreaseText) damageIncreaseText.text = stats.CurrentDamageIncrease.ToString("F1") + "%";
        if (magicResistText) magicResistText.text = stats.CurrentMagicResist.ToString("F0");
        if (evasionChanceText) evasionChanceText.text = stats.CurrentEvasionChance.ToString("F1") + "%";
        if (blockChanceText) blockChanceText.text = stats.CurrentBlockChance.ToString("F1") + "%";
        if (blockPowerText) blockPowerText.text = stats.CurrentBlockPower.ToString("F1") + "%";
        if (damageReductionText) damageReductionText.text = stats.CurrentDamageReduction.ToString("F1") + "%";
        if (abilityHasteText) abilityHasteText.text = stats.CurrentAbilityHaste.ToString("F0");
        if (tenacityText) tenacityText.text = stats.CurrentTenacity.ToString("F1") + "%";
    }
}