// Assets/Game/Scripts/Data/StatSet.cs (�� ����)
using System;
using Game.Items; // StatMods�� ����ϱ� ���� �߰�

namespace Game.Data
{
    [Serializable]
    public struct StatSet
    {
        public float maxHp;
        public float healthRegen;
        public float lifeOnKill;
        public float omnivamp;
        public float attackPower;
        public float attackSpeed;
        public float attackRange;
        public float critChance;
        public float critDamage;
        public float damageIncrease;
        public float defense;
        public float magicResist;
        public float evasionChance;
        public float blockChance;
        public float blockPower;
        public float damageReduction;
        public float moveSpeed;
        public float abilityHaste;
        public float tenacity;

        public static StatSet Default => new StatSet
        {
            maxHp = 200f,
            healthRegen = 0f,
            lifeOnKill = 0f,
            omnivamp = 0f,
            attackPower = 10f,
            attackSpeed = 1.0f,
            attackRange = 1f,
            critChance = 0f,    // �⺻ ũ���� 5%
            critDamage = 150f,  // �⺻ ũ�� ������ 150%
            damageIncrease = 0f,
            defense = 0f,
            magicResist = 0f,
            evasionChance = 0f,
            blockChance = 0f,
            blockPower = 30f,
            damageReduction = 0f,
            moveSpeed = 1f,
            abilityHaste = 0f,
            tenacity = 0f
        };

        /// <summary>
        /// StatMods�� ���� ���ȿ� �����Ͽ� ���� ������ ����մϴ�.
        /// </summary>
        public StatSet ApplyMods(StatMods mods)
        {
            StatSet final = this; // �⺻ �������� ����

            // ���� ����
            final.maxHp += mods.add_maxHp;
            final.healthRegen += mods.add_healthRegen;
            final.lifeOnKill += mods.add_lifeOnKill;
            final.attackPower += mods.add_attackPower;
            final.defense += mods.add_defense;
            final.magicResist += mods.add_magicResist;
            final.moveSpeed += mods.add_moveSpeed;
            final.abilityHaste += mods.add_abilityHaste;
            final.attackRange += mods.add_attackRange;
            final.critDamage += mods.add_critDamage; // ũ��Ƽ�� �������� ���� �տ���
            final.blockPower += mods.add_blockPower;

            // ����(%) ����
            final.attackSpeed *= (1 + mods.mult_attackSpeed / 100f);
            final.critChance *= (1 + mods.mult_critChance / 100f);
            final.omnivamp *= (1 + mods.mult_omnivamp / 100f);
            final.damageIncrease *= (1 + mods.mult_damageIncrease / 100f);
            final.evasionChance *= (1 + mods.mult_evasionChance / 100f);
            final.blockChance *= (1 + mods.mult_blockChance / 100f);
            final.damageReduction *= (1 + mods.mult_damageReduction / 100f);
            final.tenacity *= (1 + mods.mult_tenacity / 100f);

            return final;
        }
    }
}