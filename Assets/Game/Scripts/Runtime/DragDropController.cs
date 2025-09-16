// Game/UI/DragDropController.cs
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Runtime;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// �巡��-����� ����/�Һ�/������ ���� å������ �����մϴ�.
    /// - ���� ������ ���� �� �׻� blocksRaycasts=false
    /// - ��� �Һ� ����(ConsumedByDrop) ��۸� �ϰ� �ı��� EndDrag������
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
                Destroy(eventData.pointerDrag); // �ı��� �׻� ���⼭��
        }
    }
}
