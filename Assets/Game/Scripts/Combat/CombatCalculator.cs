// CombatCalculator.cs - 최종 수정본 (복사/붙여넣기)
using UnityEngine;
using Game.Combat;

public static class CombatCalculator
{
    /// <summary>
    /// 공격자와 방어자의 스탯을 기반으로 최종 HitInfo를 생성하여 반환합니다.
    /// </summary>
    public static HitInfo CalculateAttack(UnitStats attacker, UnitStats defender)
    {
        // --- 1. 방어 판정 (회피 > 막기) ---
        // [수정] defender.evasionChance -> defender.CurrentStats.evasionChance
        if (Random.Range(0f, 100f) < defender.CurrentStats.evasionChance)
        {
            return new HitInfo { outcome = HitOutcome.Evade, instigator = attacker.gameObject };
        }

        // [수정] defender.blockChance -> defender.CurrentStats.blockChance
        bool isBlocked = Random.Range(0f, 100f) < defender.CurrentStats.blockChance;

        // --- 2. 기본 피해량 계산 (공격력 + 치명타) ---
        // [수정] attacker.attackPower -> attacker.CurrentStats.attackPower
        float damage = attacker.CurrentStats.attackPower;

        // [수정] attacker.critChance -> attacker.CurrentStats.critChance
        bool isCritical = Random.Range(0f, 100f) < attacker.CurrentStats.critChance;
        if (isCritical)
        {
            // [수정] attacker.critDamage -> attacker.CurrentStats.critDamage
            damage *= (attacker.CurrentStats.critDamage / 100f);
        }

        // --- 3. 최종 피해량 보너스/감소 적용 ---
        // [수정] attacker.damageIncrease -> attacker.CurrentStats.damageIncrease
        damage *= (1 + attacker.CurrentStats.damageIncrease / 100f);

        // [수정] defender.defense -> defender.CurrentStats.defense
        float defenseMultiplier = 100f / (100f + defender.CurrentStats.defense);
        damage *= defenseMultiplier;

        // [수정] defender.damageReduction -> defender.CurrentStats.damageReduction
        damage *= (1 - defender.CurrentStats.damageReduction / 100f);

        if (isBlocked)
        {
            // [수정] defender.blockPower -> defender.CurrentStats.blockPower
            damage *= (1 - defender.CurrentStats.blockPower / 100f);
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