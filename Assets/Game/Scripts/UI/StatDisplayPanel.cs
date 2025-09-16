// StatDisplayPanel.cs - 전체 코드
using UnityEngine;
using TMPro;
using Game.Data; // UnitSO, PartyMemberState 사용을 위해 추가
using Game.Services; // GameManager 사용을 위해 추가
using Game.Items; // StatMods 사용을 위해 추가

public class StatDisplayPanel : MonoBehaviour
{
    [Header("핵심 스탯 텍스트")]
    [SerializeField] private TextMeshProUGUI maxHpText;
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI attackRangeText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;

    [Header("상세 스탯 텍스트 (선택 사항)")]
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
    /// [수정됨] UnitSO와 PartyMemberState를 기반으로 스탯을 계산하고 UI를 업데이트합니다.
    /// </summary>
    public void UpdateStats(UnitSO unitSO, PartyMemberState memberState)
    {
        // 유효한 데이터가 없으면 모든 텍스트를 초기화하고 종료
        if (unitSO == null || memberState == null)
        {
            ClearAllText();
            return;
        }

        // --- 1. 스탯 계산 시작 ---
        // 기본 스탯 가져오기
        StatSet baseStats = unitSO.baseStats;

        // 아이템으로 인한 추가 스탯을 합산할 변수
        StatMods totalItemMods = new StatMods();

        // 데이터 카탈로그 참조 가져오기
        var dataCatalog = GameManager.I?.Data;
        if (dataCatalog == null)
        {
            Debug.LogError("DataCatalog를 찾을 수 없습니다!");
            return;
        }

        // 장착된 모든 아이템의 StatMods를 더하기
        foreach (var equippedItem in memberState.equippedItemIds)
        {
            var itemSO = dataCatalog.GetItemById(equippedItem.Value);
            if (itemSO != null)
            {
                totalItemMods = totalItemMods.Add(itemSO.statMods);
            }
        }

        // 최종 스탯 계산
        StatSet finalStats = baseStats.ApplyMods(totalItemMods);

        // --- 2. 계산된 스탯을 UI에 표시 ---
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