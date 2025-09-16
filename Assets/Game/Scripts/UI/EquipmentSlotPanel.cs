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

                // [����] isDraggable ���� Setup �Լ��� �Ѱ��ݴϴ�.
                slotUI.Setup(characterIndex, currentItem, controller, isDraggable);
            }
        }

        /// <summary>
        /// [�߰�] ��� ��� ������ �巡�� ���� ���θ� �����մϴ�.
        /// </summary>
        /// <param name="isDraggable">true�̸� �巡�� ����, false�̸� �巡�� �Ұ���</param>
        public void SetDraggable(bool isDraggable)
        {
            if (_equipmentSlots == null) return;

            foreach (var slotUI in _equipmentSlots)
            {
                if (slotUI != null)
                {
                    // EquipmentSlotUI ��ũ��Ʈ ��ü�� �Ѱų� ���� �巡�� ����� �����մϴ�.
                    slotUI.enabled = isDraggable;
                }
            }
        }
    }
}