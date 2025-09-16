// Assets/Game/Scripts/Items/StatMods.cs (새 파일)
using System;

namespace Game.Items
{
    [Serializable]
    public struct StatMods
    {
        // 합연산 (고정 수치 증가)
        public float add_maxHp;
        public float add_healthRegen;
        public float add_lifeOnKill;
        public float add_attackPower;
        public float add_defense;
        public float add_magicResist;
        public float add_moveSpeed;
        public float add_abilityHaste;
        public float add_attackRange;
        public float add_critDamage;
        public float add_blockPower;

        // 곱연산 (퍼센트 증가)
        public float mult_attackSpeed;
        public float mult_critChance;
        public float mult_omnivamp;
        public float mult_damageIncrease;
        public float mult_evasionChance;
        public float mult_blockChance;
        public float mult_damageReduction;
        public float mult_tenacity;

        /// <summary>
        /// 두 StatMods를 합칩니다.
        /// </summary>
        public StatMods Add(StatMods other)
        {
            StatMods result = this;
            result.add_maxHp += other.add_maxHp;
            result.add_healthRegen += other.add_healthRegen;
            result.add_lifeOnKill += other.add_lifeOnKill;
            result.add_attackPower += other.add_attackPower;
            result.add_defense += other.add_defense;
            result.add_magicResist += other.add_magicResist;
            result.add_moveSpeed += other.add_moveSpeed;
            result.add_abilityHaste += other.add_abilityHaste;
            result.add_attackRange += other.add_attackRange;
            result.add_critDamage += other.add_critDamage;
            result.add_blockPower += other.add_blockPower;

            result.mult_attackSpeed += other.mult_attackSpeed;
            result.mult_critChance += other.mult_critChance;
            result.mult_omnivamp += other.mult_omnivamp;
            result.mult_damageIncrease += other.mult_damageIncrease;
            result.mult_evasionChance += other.mult_evasionChance;
            result.mult_blockChance += other.mult_blockChance;
            result.mult_damageReduction += other.mult_damageReduction;
            result.mult_tenacity += other.mult_tenacity;

            return result;
        }
    }
}