// Assets/Game/Scripts/Combat/CombatCalculator.cs (���� �Ϸ�� �ڵ�)
using UnityEngine;
using Game.Combat; // HitInfo, HitOutcome�� ����ϱ� ����

public static class CombatCalculator
{

    /// <summary>
    /// �����ڿ� ������� ������ ������� ���� HitInfo�� �����Ͽ� ��ȯ�մϴ�.
    /// </summary>
    public static HitInfo CalculateAttack(UnitStats attacker, UnitStats defender)
    {
        // --- 1. ��� ���� (ȸ�� > ����) ---
        if (Random.Range(0f, 100f) < defender.evasionChance)
        {
            return new HitInfo { outcome = HitOutcome.Evade, instigator = attacker.gameObject };
        }

        bool isBlocked = Random.Range(0f, 100f) < defender.blockChance;

        // --- 2. �⺻ ���ط� ��� (���ݷ� + ġ��Ÿ) ---
        float damage = attacker.attackPower;
        bool isCritical = Random.Range(0f, 100f) < attacker.critChance;
        if (isCritical)
        {
            damage *= (attacker.critDamage / 100f);
        }

        // --- 3. ���� ���ط� ���ʽ�/���� ���� ---
        damage *= (1 + attacker.damageIncrease / 100f);

        float defenseMultiplier = 100f / (100f + defender.defense);
        damage *= defenseMultiplier;

        damage *= (1 - defender.damageReduction / 100f);

        // [����] ���⿡ �����ߴٸ�, ���� ������ ���� �ƴ� ������� 'blockPower' ������ ����մϴ�.
        if (isBlocked)
        {
            damage *= (1 - defender.blockPower / 100f);
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