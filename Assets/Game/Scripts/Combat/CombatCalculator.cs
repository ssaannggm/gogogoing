// CombatCalculator.cs - ���� ������ (����/�ٿ��ֱ�)
using UnityEngine;
using Game.Combat;

public static class CombatCalculator
{
    /// <summary>
    /// �����ڿ� ������� ������ ������� ���� HitInfo�� �����Ͽ� ��ȯ�մϴ�.
    /// </summary>
    public static HitInfo CalculateAttack(UnitStats attacker, UnitStats defender)
    {
        // --- 1. ��� ���� (ȸ�� > ����) ---
        // [����] defender.evasionChance -> defender.CurrentStats.evasionChance
        if (Random.Range(0f, 100f) < defender.CurrentStats.evasionChance)
        {
            return new HitInfo { outcome = HitOutcome.Evade, instigator = attacker.gameObject };
        }

        // [����] defender.blockChance -> defender.CurrentStats.blockChance
        bool isBlocked = Random.Range(0f, 100f) < defender.CurrentStats.blockChance;

        // --- 2. �⺻ ���ط� ��� (���ݷ� + ġ��Ÿ) ---
        // [����] attacker.attackPower -> attacker.CurrentStats.attackPower
        float damage = attacker.CurrentStats.attackPower;

        // [����] attacker.critChance -> attacker.CurrentStats.critChance
        bool isCritical = Random.Range(0f, 100f) < attacker.CurrentStats.critChance;
        if (isCritical)
        {
            // [����] attacker.critDamage -> attacker.CurrentStats.critDamage
            damage *= (attacker.CurrentStats.critDamage / 100f);
        }

        // --- 3. ���� ���ط� ���ʽ�/���� ���� ---
        // [����] attacker.damageIncrease -> attacker.CurrentStats.damageIncrease
        damage *= (1 + attacker.CurrentStats.damageIncrease / 100f);

        // [����] defender.defense -> defender.CurrentStats.defense
        float defenseMultiplier = 100f / (100f + defender.CurrentStats.defense);
        damage *= defenseMultiplier;

        // [����] defender.damageReduction -> defender.CurrentStats.damageReduction
        damage *= (1 - defender.CurrentStats.damageReduction / 100f);

        if (isBlocked)
        {
            // [����] defender.blockPower -> defender.CurrentStats.blockPower
            damage *= (1 - defender.CurrentStats.blockPower / 100f);
        }

        // --- 4. ���� HitInfo ���� �� ��ȯ ---
        return new HitInfo
        {
            outcome = isCritical ? HitOutcome.Crit : (isBlocked ? HitOutcome.Block : HitOutcome.Hit),
            amount = Mathf.Max(1, Mathf.RoundToInt(damage)),
            critical = isCritical,
            instigator = attacker.gameObject,
            damageType = DamageType.Physical // ������ ���� ���ظ� ����
        };
    }
}