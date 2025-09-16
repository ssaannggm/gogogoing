using UnityEngine;
using Game.Items;
using Game.Data;
using Game.Services;
using System.Collections.Generic;
using Game.Runtime;

namespace Game.UI
{
    public class EquipmentSlotPanel : MonoBehaviour
    {
        [SerializeField] private List<EquipmentSlotUI> _equipmentSlots;

        void Awake()
        {
            if (_equipmentSlots == null || _equipmentSlots.Count == 0)
            {
                _equipmentSlots = new List<EquipmentSlotUI>(GetComponentsInChildren<EquipmentSlotUI>(true));
            }
        }

        public void UpdateSlots(PartyMemberState memberState, int characterIndex, InventoryPartyMode controller, bool isDraggable)
        {
            var dataCatalog = GameManager.I?.Data;

            foreach (var slotUI in _equipmentSlots)
            {
                ItemSO currentItem = null;
                if (memberState != null && dataCatalog != null && memberState.equippedItemIds.TryGetValue(slotUI.slotType, out var itemId))
                {
                    currentItem = dataCatalog.GetItemById(itemId);
                }

                // [수정] isDraggable 값을 Setup 함수로 넘겨줍니다.
                slotUI.Setup(characterIndex, currentItem, controller, isDraggable);
            }
        }

        /// <summary>
        /// [추가] 모든 장비 슬롯의 드래그 가능 여부를 설정합니다.
        /// </summary>
        /// <param name="isDraggable">true이면 드래그 가능, false이면 드래그 불가능</param>
        public void SetDraggable(bool isDraggable)
        {
            if (_equipmentSlots == null) return;

            foreach (var slotUI in _equipmentSlots)
            {
                if (slotUI != null)
                {
                    // EquipmentSlotUI 스크립트 자체를 켜거나 꺼서 드래그 기능을 제어합니다.
                    slotUI.enabled = isDraggable;
                }
            }
        }
    }
}