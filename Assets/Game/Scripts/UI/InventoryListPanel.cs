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
        [SerializeField] private GameObject _itemIconPrefab;

        // [추가] 생성된 아이콘들을 관리하기 위한 리스트
        private List<ItemIconUI> _spawnedIcons = new List<ItemIconUI>();
        private InventoryPartyMode _controller;

        public void Refresh(InventoryPartyMode controller)
        {
            _controller = controller;

            // 기존 아이콘들을 모두 삭제
            foreach (var icon in _spawnedIcons)
            {
                if (icon) Destroy(icon.gameObject);
            }
            _spawnedIcons.Clear();

            var run = GameManager.I?.CurrentRun;
            var dataCatalog = GameManager.I?.Data;
            if (run == null || dataCatalog == null || _itemIconPrefab == null) return;

            IReadOnlyList<string> itemIds = run.InventoryItemIds;
            foreach (var itemId in itemIds)
            {
                var itemSO = dataCatalog.GetItemById(itemId);
                if (itemSO != null)
                {
                    var iconGO = Instantiate(_itemIconPrefab, _contentParent);
                    var itemIcon = iconGO.GetComponent<ItemIconUI>();
                    itemIcon.Setup(itemSO, controller);
                    _spawnedIcons.Add(itemIcon);
                }
            }
        }

        /// <summary>
        /// [추가] 인벤토리의 모든 아이콘들의 드래그 가능 여부를 설정합니다.
        /// </summary>
        public void SetDraggable(bool isDraggable)
        {
            foreach (var iconUI in _spawnedIcons)
            {
                if (iconUI != null)
                {
                    // ItemIconUI 스크립트 자체를 켜거나 꺼서 드래그 기능을 제어합니다.
                    iconUI.enabled = isDraggable;
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var itemIcon = eventData.pointerDrag.GetComponent<ItemIconUI>();
            if (itemIcon != null)
            {
                _controller?.OnUnequipRequest(itemIcon.ItemData);
            }
        }
    }
}