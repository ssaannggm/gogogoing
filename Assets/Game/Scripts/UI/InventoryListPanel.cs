// InventoryListPanel.cs - 전체 코드
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

            // [✨ 핵심 수정]
            // 드롭된 아이콘이 장비 슬롯에서 온 '유령'이고,
            // 그 유령 아이콘이 컨트롤러(controller)에 대한 참조를 가지고 있을 경우에만 장비 해제 처리
            if (droppedIcon.IsGhost && droppedIcon.SourceSlot != null && droppedIcon.Controller != null)
            {
                var sourceSlot = droppedIcon.SourceSlot;
                // GetComponentInParent 대신, 유령 아이콘이 직접 들고 있는 컨트롤러 참조를 사용합니다.
                // 이것이 훨씬 안정적입니다.
                droppedIcon.Controller.HandleUnequipRequest(sourceSlot.CharacterIndex, sourceSlot.slotType);
            }
        }
    }
}