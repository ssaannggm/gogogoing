using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace Game.Items
{
    // 어떤 스탯을 수정할지 나타내는 enum (UnitStats의 변수명과 일치)
    public enum StatType
    {
        MaxHp, HealthRegen, LifeOnKill, Omnivamp, AttackPower, AttackSpeed,
        AttackRange, CritChance, CritDamage, DamageIncrease, Defense, MagicResist,
        EvasionChance, BlockChance, BlockPower, DamageReduction, MoveSpeed,
        AbilityHaste, Tenacity
    }

    // 스탯을 어떻게 수정할지 나타내는 enum
    public enum ModifierType
    {
        Add,        // 덧셈 (고정 수치 증가)
        Multiply    // 곱셈 (% 증가)
    }

    [Serializable]
    public struct StatModifier
    {
        public StatType statType;
        public ModifierType modifierType;
        public float value;
    }

    // SPUM_Prefabs와 통일된 PlayerState enum
    

    [Serializable]
    public struct AnimationOverride
    {
        public PlayerState state;
        public AnimationClip clip;
    }

    [CreateAssetMenu(menuName = "Game/Items/Item", fileName = "Item_")]
    public sealed class ItemSO : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        public ItemRarity rarity = ItemRarity.Common;
        public Sprite icon;
        [TextArea(1, 2)] public string tooltipHeader;
        [TextArea(2, 6)] public string tooltipBody;

        [Header("Slot")]
        public EquipSlot slot;

        [Header("Stats")]
        public StatModifier[] statMods;

        [Header("Visual Groups")]
        public VisualGroup[] visualGroups;
        public string[] groupsToHide;

        [Header("Animation Overrides")]
        [Tooltip("이 아이템 착용 시 특정 상태의 애니메이션 목록에 이 클립을 추가합니다.")]
        public AnimationOverride[] animationOverrides;

        [Header("Attack Effect")]
        public AttackEffectDef attackEffect = AttackEffectDef.Default;

        [Serializable]
        public struct VisualGroup
        {
            public string key;
            public Sprite[] sprites;
        }

        [Serializable]
        public struct AttackEffectDef
        {
            [Header("VFX")]
            public GameObject vfxPrefab;
            public float vfxLifetime;
            public bool attachToAnchor;
            public Vector3 localPosition;
            public Vector3 localEuler;
            public Vector3 localScale;

            [Header("정렬")]
            public bool matchSortingGroup;
            public int orderOffset;

            [Header("SFX")]
            public AudioClip sfx;
            [Range(0f, 1f)] public float volume;
            public float pitch;
            [Range(0f, 1f)] public float pitchJitter;
            public AudioMixerGroup mixerGroup;
            public int maxInstances;
            public float instanceCooldown;

            [Header("Alignment (Z축 회전)")]
            public bool alignToAim;
            public float angleOffset;

            public static AttackEffectDef Default => new AttackEffectDef
            {
                vfxPrefab = null,
                vfxLifetime = 0.6f,
                attachToAnchor = true,
                localPosition = Vector3.zero,
                localEuler = Vector3.zero,
                localScale = Vector3.one,
                matchSortingGroup = true,
                orderOffset = +10,
                sfx = null,
                volume = 1f,
                pitch = 1f,
                pitchJitter = 0.05f,
                mixerGroup = null,
                maxInstances = 0,
                instanceCooldown = 0.02f,
                alignToAim = true,
                angleOffset = 0f,
            };
        }
    }

    // ItemRarity, EquipSlot enum (별도 파일에 있다면 이 부분은 지워도 됩니다)
    //public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
    //public enum EquipSlot { RightHand, LeftHand, Helmet, Armor, Accessory }
}