// Assets/Game/Scripts/UI/InventoryListPanel.cs
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
        [Header("UI 설정")]
        [SerializeField] private Transform _contentParent;

        private readonly List<GameObject> _spawnedIcons = new List<GameObject>();

        public void Refresh(InventoryPartyMode controller, IReadOnlyList<string> itemIds, bool isDraggable)
        {
            foreach (var icon in _spawnedIcons) if (icon) Destroy(icon);
            _spawnedIcons.Clear();

            var dataCatalog = GameManager.I?.Data;
            var itemIconPrefab = controller.GetItemIconPrefab();
            if (dataCatalog == null || itemIconPrefab == null) return;

            foreach (var itemId in itemIds)
            {
                var itemSO = dataCatalog.GetItemById(itemId);
                if (itemSO != null)
                {
                    var iconGO = Object.Instantiate(itemIconPrefab, _contentParent);
                    var itemIcon = iconGO.GetComponent<ItemIconUI>();
                    itemIcon.Setup(itemSO, controller, isDraggable);
                    _spawnedIcons.Add(iconGO);
                }
            }
        }

        // InventoryListPanel.OnDrop
        public void OnDrop(PointerEventData eventData)
        {
            var droppedIcon = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<ItemIconUI>() : null;
            Debug.Log($"[INV] OnDrop pointerDrag={eventData.pointerDrag?.name} icon={droppedIcon?.ItemData?.name}");
            if (droppedIcon == null) return;

            if (droppedIcon.IsGhost && droppedIcon.SourceSlot != null && droppedIcon.Controller != null)
            {
                var source = droppedIcon.SourceSlot;
                droppedIcon.Controller.HandleUnequipRequest(source.CharacterIndex, source.slotType);
                droppedIcon.Controller.NotifyDropSucceeded();
            }
        }

    }
}
