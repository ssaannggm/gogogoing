// Assets/Game/Scripts/Combat/CombatCalculator.cs (수정 완료된 코드)
using UnityEngine;
using Game.Combat; // HitInfo, HitOutcome을 사용하기 위해

public static class CombatCalculator
{

    /// <summary>
    /// 공격자와 방어자의 스탯을 기반으로 최종 HitInfo를 생성하여 반환합니다.
    /// </summary>
    public static HitInfo CalculateAttack(UnitStats attacker, UnitStats defender)
    {
        // --- 1. 방어 판정 (회피 > 막기) ---
        if (Random.Range(0f, 100f) < defender.evasionChance)
        {
            return new HitInfo { outcome = HitOutcome.Evade, instigator = attacker.gameObject };
        }

        bool isBlocked = Random.Range(0f, 100f) < defender.blockChance;

        // --- 2. 기본 피해량 계산 (공격력 + 치명타) ---
        float damage = attacker.attackPower;
        bool isCritical = Random.Range(0f, 100f) < attacker.critChance;
        if (isCritical)
        {
            damage *= (attacker.critDamage / 100f);
        }

        // --- 3. 최종 피해량 보너스/감소 적용 ---
        damage *= (1 + attacker.damageIncrease / 100f);

        float defenseMultiplier = 100f / (100f + defender.defense);
        damage *= defenseMultiplier;

        damage *= (1 - defender.damageReduction / 100f);

        // [수정] 막기에 성공했다면, 이제 고정된 값이 아닌 방어자의 'blockPower' 스탯을 사용합니다.
        if (isBlocked)
        {
            damage *= (1 - defender.blockPower / 100f);
        }

        // --- 4. 최종 HitInfo 생성 및 반환 ---
        return new HitInfo
        {
            outcome = isCritical ? HitOutcome.Crit : (isBlocked ? HitOutcome.Block : HitOutcome.Hit),
            amount = Mathf.Max(1, Mathf.RoundToInt(damage)),
            critical = isCritical,
            instigator = attacker.gameObject,
            damageType = DamageType.Physical // 지금은 물리 피해만 가정
        };
    }
}