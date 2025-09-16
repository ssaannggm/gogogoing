// Assets/Game/Scripts/Runtime/UnitLoadout.cs (수정 완료된 최종 코드)
using System;
using UnityEngine;
using Game.Items;
using Game.Combat;

namespace Game.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnitStats))] // UnitStats가 항상 있도록 보장
    public sealed class UnitLoadout : MonoBehaviour
    {
        [Header("Equipped Items")]
        public ItemSO leftHand, rightHand, helmet, armor;

        [Header("VFX Emitter")]
        public AttackVfxEmitter emitter;

        public event Action OnLoadoutChanged;

        private UnitStats _stats;

        void Awake()
        {
            _stats = GetComponent<UnitStats>();
            if (!emitter) emitter = GetComponentInChildren<AttackVfxEmitter>(true);

            // 시작 시 기본 장착된 아이템들의 스탯을 한번 계산해줍니다.
            UpdateAllStatsAndEffects();
        }

        public void Equip(ItemSO item)
        {
            // --- ✨ 디버그 로그 1 ✨ ---
            // 이 로그가 안 찍히면, UI에서 이 함수를 호출하는 것부터 실패한 것입니다.
            Debug.Log($"<color=yellow>1. [UnitLoadout] Equip 함수 호출됨: {item?.displayName ?? "null"}</color>");

            if (!item) return;
            Unequip(item.slot);

            switch (item.slot)
            {
                case EquipSlot.LeftHand: leftHand = item; break;
                case EquipSlot.RightHand: rightHand = item; break;
                case EquipSlot.Helmet: helmet = item; break;
                case EquipSlot.Armor: armor = item; break;
            }
            UpdateAllStatsAndEffects();
        }

        public void Unequip(EquipSlot slot)
        {
            // 해당 슬롯의 아이템을 null로 만듭니다.
            switch (slot)
            {
                case EquipSlot.LeftHand: leftHand = null; break;
                case EquipSlot.RightHand: rightHand = null; break;
                case EquipSlot.Helmet: helmet = null; break;
                case EquipSlot.Armor: armor = null; break;
            }

            UpdateAllStatsAndEffects();
        }

        // ✨ 모든 스탯과 효과를 처음부터 다시 계산하고 적용하는 통합 함수
        private void UpdateAllStatsAndEffects()
        {
            if (_stats == null) _stats = GetComponent<UnitStats>();

            // 1. UnitStats의 모든 추가 능력치를 초기화합니다.
            _stats.ClearAllModifiers();

            // 2. 현재 장착된 모든 아이템의 능력치를 다시 추가합니다.
            if (leftHand) _stats.AddModifiers(leftHand.statMods);
            if (rightHand) _stats.AddModifiers(rightHand.statMods);
            if (helmet) _stats.AddModifiers(helmet.statMods);
            if (armor) _stats.AddModifiers(armor.statMods);

            // 3. UnitStats에게 최종 스탯을 다시 계산하라고 명령합니다.
            _stats.RecalculateStats();

            // 4. 시각/공격 효과를 업데이트합니다.
            ApplyToEmitter();

            // 5. 장비 변경 이벤트를 다른 곳에 알립니다 (스프라이트 변경용).
            OnLoadoutChanged?.Invoke();
        }

        public void ApplyToEmitter()
        {
            // --- ✨ 디버그 로그 2 ✨ ---
            // emitter가 null이면 연결이 끊어진 것입니다.
            if (!emitter)
            {
                Debug.LogError("-> 문제 발견: UnitLoadout에 AttackVfxEmitter가 연결되지 않았습니다!");
                return;
            }

            // rightHand 아이템과 그 VFX 프리팹의 이름을 로그로 찍어봅니다.
            string vfxName = (rightHand && rightHand.attackEffect.vfxPrefab != null) ? rightHand.attackEffect.vfxPrefab.name : "비어있음";
            Debug.Log($"<color=lime>2. [UnitLoadout] ApplyToEmitter 실행. 오른손 무기({rightHand?.displayName ?? "없음"})의 VFX는? -> {vfxName}</color>");

            emitter.leftDef = leftHand ? leftHand.attackEffect : ItemSO.AttackEffectDef.Default;
            emitter.rightDef = rightHand ? rightHand.attackEffect : ItemSO.AttackEffectDef.Default;
        }
    }
}