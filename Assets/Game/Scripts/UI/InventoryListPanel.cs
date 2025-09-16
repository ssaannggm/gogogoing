// InventoryListPanel.cs - ��ü �ڵ�
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Services;
using Game.Items;
using System.Collections.Generic;
using Game.Runtime;

namespace Game.UI
{
    public class InventoryListPanel : MonoBehaviour, IDropHandler
    {
        [Header("UI ����")]
        [SerializeField] private Transform _contentParent;

        private List<GameObject> _spawnedIcons = new List<GameObject>();

        public void Refresh(InventoryPartyMode controller, IReadOnlyList<string> itemIds, bool isDraggable)
        {
            foreach (var icon in _spawnedIcons)
            {
                if (icon) Destroy(icon);
            }
            _spawnedIcons.Clear();

            var dataCatalog = GameManager.I?.Data;
            var itemIconPrefab = controller.GetItemIconPrefab();
            if (dataCatalog == null || itemIconPrefab == null) return;

            foreach (var itemId in itemIds)
            {
                var itemSO = dataCatalog.GetItemById(itemId);
                if (itemSO != null)
                {
                    var iconGO = Instantiate(itemIconPrefab, _contentParent);
                    var itemIcon = iconGO.GetComponent<ItemIconUI>();
                    itemIcon.Setup(itemSO, controller, isDraggable);
                    _spawnedIcons.Add(iconGO);
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var droppedIcon = eventData.pointerDrag.GetComponent<ItemIconUI>();
            if (droppedIcon == null) return;

            // [�ٽ�] ��ӵ� �������� ��� ���Կ��� �� '����'�� ��쿡�� ��� ���� ó��
            if (droppedIcon.IsGhost && droppedIcon.SourceSlot != null)
            {
                var sourceSlot = droppedIcon.SourceSlot;
                var controller = sourceSlot.GetComponentInParent<InventoryPartyMode>(); // ��Ʈ�ѷ� ã��

                controller?.HandleUnequipRequest(sourceSlot.CharacterIndex, sourceSlot.slotType);
            }
        }
    }
}