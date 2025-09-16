using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq를 사용하기 위해 추가
using Game.Combat;
using Game.Items;

public class UnitStats : MonoBehaviour
{
    [Header("ID")]
    public Team team;

    [Header("Base Stats")]
    public float maxHp = 100f;
    public float healthRegen = 1f;
    public float lifeOnKill = 0f;
    public float omnivamp = 0f;
    public float attackPower = 10f;
    public float attackSpeed = 1f;
    public float attackRange = 1f;
    [Range(0, 100)] public float critChance = 0f;
    public float critDamage = 150f;
    [Range(0, 100)] public float damageIncrease = 0f;
    public float defense = 10f;
    public float magicResist = 10f;
    [Range(0, 100)] public float evasionChance = 0f;
    [Range(0, 100)] public float blockChance = 0f;
    [Range(0, 100)] public float blockPower = 30f;
    [Range(0, 100)] public float damageReduction = 0f;
    public float moveSpeed = 2f;
    public float abilityHaste = 0f;
    [Range(0, 100)] public float tenacity = 0f;

    [Header("Runtime Calculated Stats (Debug Mode)")]
    public float CurrentMaxHp { get; private set; }
    public float CurrentHealthRegen { get; private set; }
    public float CurrentLifeOnKill { get; private set; }
    public float CurrentOmnivamp { get; private set; }
    public float CurrentAttackPower { get; private set; }
    public float CurrentAttackSpeed { get; private set; }
    public float CurrentAttackRange { get; private set; }
    public float CurrentCritChance { get; private set; }
    public float CurrentCritDamage { get; private set; }
    public float CurrentDamageIncrease { get; private set; }
    public float CurrentDefense { get; private set; }
    public float CurrentMagicResist { get; private set; }
    public float CurrentEvasionChance { get; private set; }
    public float CurrentBlockChance { get; private set; }
    public float CurrentBlockPower { get; private set; }
    public float CurrentDamageReduction { get; private set; }
    public float CurrentMoveSpeed { get; private set; }
    public float CurrentAbilityHaste { get; private set; }
    public float CurrentTenacity { get; private set; }

    private readonly List<StatModifier> _statModifiers = new List<StatModifier>();

    void Awake()
    {
        RecalculateStats();
    }

    public void AddModifiers(StatModifier[] mods)
    {
        foreach (var mod in mods) _statModifiers.Add(mod);
    }

    public void ClearAllModifiers()
    {
        _statModifiers.Clear();
    }

    public void RecalculateStats()
    {
        // 1. 모든 'Current' 스탯을 'Base' 스탯으로 초기화
        CurrentMaxHp = maxHp;
        CurrentHealthRegen = healthRegen;
        CurrentLifeOnKill = lifeOnKill;
        CurrentOmnivamp = omnivamp;
        CurrentAttackPower = attackPower;
        CurrentAttackSpeed = attackSpeed;
        CurrentAttackRange = attackRange;
        CurrentCritChance = critChance;
        CurrentCritDamage = critDamage;
        CurrentDamageIncrease = damageIncrease;
        CurrentDefense = defense;
        CurrentMagicResist = magicResist;
        CurrentEvasionChance = evasionChance;
        CurrentBlockChance = blockChance;
        CurrentBlockPower = blockPower;
        CurrentDamageReduction = damageReduction;
        CurrentMoveSpeed = moveSpeed;
        CurrentAbilityHaste = abilityHaste;
        CurrentTenacity = tenacity;

        // 2. 곱셈(%) 모디파이어를 먼저 적용
        foreach (var mod in _statModifiers.Where(m => m.modifierType == ModifierType.Multiply)) ApplyModifier(mod);
        // 3. 덧셈(고정 수치) 모디파이어를 나중에 적용
        foreach (var mod in _statModifiers.Where(m => m.modifierType == ModifierType.Add)) ApplyModifier(mod);
    }

    // [수정] 모든 스탯에 대한 case를 추가하여 모든 스탯이 정상 적용되도록 수정
    private void ApplyModifier(StatModifier mod)
    {
        switch (mod.statType)
        {
            case StatType.MaxHp: CurrentMaxHp = CalculateModifiedValue(CurrentMaxHp, mod); break;
            case StatType.HealthRegen: CurrentHealthRegen = CalculateModifiedValue(CurrentHealthRegen, mod); break;
            case StatType.LifeOnKill: CurrentLifeOnKill = CalculateModifiedValue(CurrentLifeOnKill, mod); break;
            case StatType.Omnivamp: CurrentOmnivamp = CalculateModifiedValue(CurrentOmnivamp, mod); break;
            case StatType.AttackPower: CurrentAttackPower = CalculateModifiedValue(CurrentAttackPower, mod); break;
            case StatType.AttackSpeed: CurrentAttackSpeed = CalculateModifiedValue(CurrentAttackSpeed, mod); break;
            case StatType.AttackRange: CurrentAttackRange = CalculateModifiedValue(CurrentAttackRange, mod); break;
            case StatType.CritChance: CurrentCritChance = CalculateModifiedValue(CurrentCritChance, mod); break;
            case StatType.CritDamage: CurrentCritDamage = CalculateModifiedValue(CurrentCritDamage, mod); break;
            case StatType.DamageIncrease: CurrentDamageIncrease = CalculateModifiedValue(CurrentDamageIncrease, mod); break;
            case StatType.Defense: CurrentDefense = CalculateModifiedValue(CurrentDefense, mod); break;
            case StatType.MagicResist: CurrentMagicResist = CalculateModifiedValue(CurrentMagicResist, mod); break;

            case StatType.EvasionChance: CurrentEvasionChance = CalculateModifiedValue(CurrentEvasionChance, mod); break;
            case StatType.BlockChance: CurrentBlockChance = CalculateModifiedValue(CurrentBlockChance, mod); break;
            case StatType.BlockPower: CurrentBlockPower = CalculateModifiedValue(CurrentBlockPower, mod); break;
            case StatType.DamageReduction: CurrentDamageReduction = CalculateModifiedValue(CurrentDamageReduction, mod); break;
            case StatType.MoveSpeed: CurrentMoveSpeed = CalculateModifiedValue(CurrentMoveSpeed, mod); break;
            case StatType.AbilityHaste: CurrentAbilityHaste = CalculateModifiedValue(CurrentAbilityHaste, mod); break;
            case StatType.Tenacity: CurrentTenacity = CalculateModifiedValue(CurrentTenacity, mod); break;
        }
    }

    // [추가] 중복 코드를 줄이기 위한 헬퍼 함수
    private float CalculateModifiedValue(float baseValue, StatModifier mod)
    {
        if (mod.modifierType == ModifierType.Add) return baseValue + mod.value;
        return baseValue * (1 + mod.value / 100f);
    }
}