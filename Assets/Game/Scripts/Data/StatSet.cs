// Assets/Game/Scripts/Data/StatSet.cs (새 파일)
using System;
using Game.Items; // StatMods를 사용하기 위해 추가

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
            critChance = 0f,    // 기본 크리율 5%
            critDamage = 150f,  // 기본 크리 데미지 150%
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
        /// StatMods를 현재 스탯에 적용하여 최종 스탯을 계산합니다.
        /// </summary>
        public StatSet ApplyMods(StatMods mods)
        {
            StatSet final = this; // 기본 스탯으로 시작

            // 덧셈 연산
            final.maxHp += mods.add_maxHp;
            final.healthRegen += mods.add_healthRegen;
            final.lifeOnKill += mods.add_lifeOnKill;
            final.attackPower += mods.add_attackPower;
            final.defense += mods.add_defense;
            final.magicResist += mods.add_magicResist;
            final.moveSpeed += mods.add_moveSpeed;
            final.abilityHaste += mods.add_abilityHaste;
            final.attackRange += mods.add_attackRange;
            final.critDamage += mods.add_critDamage; // 크리티컬 데미지는 보통 합연산
            final.blockPower += mods.add_blockPower;

            // 곱셈(%) 연산
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