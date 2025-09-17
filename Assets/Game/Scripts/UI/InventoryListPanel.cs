// Assets/Game/Scripts/UI/InventoryListPanel.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Game.Items;
using Game.Runtime;
using Game.UI;
using Game.Services;

namespace Game.UI
{
    public class InventoryListPanel : MonoBehaviour, IDropHandler
    {
        [Header("UI 설정")]
        [SerializeField] private Transform _contentParent;

        private readonly List<GameObject> _spawned = new();

        public void Refresh(InventoryPartyMode controller, IReadOnlyList<string> itemIds, bool isDraggable)
        {
            for (int i = 0; i < _spawned.Count; i++) if (_spawned[i]) Destroy(_spawned[i]);
            _spawned.Clear();

            var data = GameManager.I?.Data;
            var prefab = controller.GetItemIconPrefab();
            if (data == null || prefab == null) return;

            foreach (var id in itemIds)
            {
                var so = data.GetItemById(id);
                if (!so) continue;

                var go = Object.Instantiate(prefab, _contentParent);
                var icon = go.GetComponent<ItemIconUI>();
                icon.Setup(so, controller, isDraggable);
                _spawned.Add(go);
            }
        }

        // 드롭 수신: “현재 들고 있는 것”을 인벤토리로 보냄(= 해제)
        public void OnDrop(PointerEventData e)
        {
            if (!DragContext.IsActive) return;
            // InventoryPartyMode가 실제 데이터(RunManager)를 수정
            GameManager.I?.GetComponentInChildren<InventoryPartyMode>()?.ApplyDropToInventory();
            // 성공 시 내부에서 DragContext.Clear() + UI 리프레시 수행
        }
    }
}
