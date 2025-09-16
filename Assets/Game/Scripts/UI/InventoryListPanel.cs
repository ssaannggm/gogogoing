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
        [SerializeField] private GameObject _itemIconPrefab;

        // [�߰�] ������ �����ܵ��� �����ϱ� ���� ����Ʈ
        private List<ItemIconUI> _spawnedIcons = new List<ItemIconUI>();
        private InventoryPartyMode _controller;

        public void Refresh(InventoryPartyMode controller)
        {
            _controller = controller;

            // ���� �����ܵ��� ��� ����
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
        /// [�߰�] �κ��丮�� ��� �����ܵ��� �巡�� ���� ���θ� �����մϴ�.
        /// </summary>
        public void SetDraggable(bool isDraggable)
        {
            foreach (var iconUI in _spawnedIcons)
            {
                if (iconUI != null)
                {
                    // ItemIconUI ��ũ��Ʈ ��ü�� �Ѱų� ���� �巡�� ����� �����մϴ�.
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