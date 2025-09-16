// Game/UI/DragDropController.cs
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Runtime;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 드래그-드롭의 생성/소비/정리를 단일 책임으로 관리합니다.
    /// - 유령 아이콘 생성 시 항상 blocksRaycasts=false
    /// - 드롭 소비 여부(ConsumedByDrop) 토글만 하고 파괴는 EndDrag에서만
    /// </summary>
    public sealed class DragDropController : MonoBehaviour
    {
        [SerializeField] private InventoryPartyMode _controller;

        public ItemIconUI CreateGhost(ItemSO item, EquipmentSlotUI origin)
        {
            var go = Instantiate(_controller.GetItemIconPrefab(), _controller.dragParent);
            var icon = go.GetComponent<ItemIconUI>();
            icon.Setup(item, _controller);
            //icon.OriginSlot = origin;

            var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            return icon;
        }

        public void ConsumeDrop(ItemIconUI icon)
        {
            //if (icon != null) icon.ConsumedByDrop = true;
        }

        public void EndDragCleanup(PointerEventData eventData, System.Action restoreIfNotConsumed)
        {
            var icon = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<ItemIconUI>() : null;

            //if (icon != null && !icon.ConsumedByDrop)
            {
                restoreIfNotConsumed?.Invoke();
            }

            if (eventData.pointerDrag != null)
                Destroy(eventData.pointerDrag); // 파괴는 항상 여기서만
        }
    }
}
