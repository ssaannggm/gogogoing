// Assets/Game/Scripts/Runtime/UnitLoadout.cs (최종 수정본)
using System;
using UnityEngine;
using Game.Items;
using Game.Combat;
using Game.Data; // DataCatalog를 사용하기 위해 추가

namespace Game.Runtime
{
    [DisallowMultipleComponent]
    public sealed class UnitLoadout : MonoBehaviour
    {
        [Header("Equipped Items (Runtime)")]
        public ItemSO leftHand, rightHand, helmet, armor;

        [Header("VFX Emitter")]
        public AttackVfxEmitter emitter;

        public event Action OnLoadoutChanged;

        void Awake()
        {
            // 이 컴포넌트는 이제 스스로 아무것도 하지 않고, BattleManager가 초기화해주기를 기다립니다.
            if (!emitter) emitter = GetComponentInChildren<AttackVfxEmitter>(true);
        }

        /// <summary>
        /// [핵심] BattleManager가 호출할 새로운 초기화 함수.
        /// PartyMemberState를 기반으로 장비 정보를 설정하고 시각적 요소를 업데이트합니다.
        /// </summary>
        public void InitializeLoadout(PartyMemberState memberState, DataCatalog data)
        {
            // 1. 모든 장비 슬롯 초기화
            leftHand = null;
            rightHand = null;
            helmet = null;
            armor = null;

            // 2. PartyMemberState에 있는 아이템 ID를 기반으로 ItemSO를 찾아 할당
            foreach (var itemEntry in memberState.equippedItemIds)
            {
                var itemSO = data.GetItemById(itemEntry.Value);
                if (itemSO == null) continue;

                switch (itemEntry.Key) // itemEntry.Key는 EquipSlot 입니다.
                {
                    case EquipSlot.LeftHand: leftHand = itemSO; break;
                    case EquipSlot.RightHand: rightHand = itemSO; break;
                    case EquipSlot.Helmet: helmet = itemSO; break;
                    case EquipSlot.Armor: armor = itemSO; break;
                }
            }

            // 3. 시각/공격 효과 업데이트
            ApplyToEmitter();

            // 4. 장비 변경 이벤트를 다른 곳(예: SpumVisualApplier)에 알림
            OnLoadoutChanged?.Invoke();
        }

        public void ApplyToEmitter()
        {
            if (!emitter)
            {
                Debug.LogError("UnitLoadout에 AttackVfxEmitter가 연결되지 않았습니다!", gameObject);
                return;
            }

            emitter.leftDef = leftHand ? leftHand.attackEffect : ItemSO.AttackEffectDef.Default;
            emitter.rightDef = rightHand ? rightHand.attackEffect : ItemSO.AttackEffectDef.Default;
        }
    }
}