// Assets/Game/Scripts/UI/InventoryDropCatcher.cs
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Runtime;
using Game.UI;

[RequireComponent(typeof(UnityEngine.UI.Image))] // ���� Image + raycastTarget=On
public class InventoryDropCatcher : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!DragContext.IsActive) return;
        // ���� ����� ���� ���� ó��
        if (DragContext.Current.source == DragSourceType.Slot)
        {
            var inv = FindObjectOfType<InventoryPartyMode>(true);
            if (inv) inv.ApplyDropToInventorySO();
        }
    }
}
