// Assets/Game/Scripts/UI/Drag/DragContext.cs
using Game.Items;

namespace Game.UI
{
    public enum DragSourceType { None, Inventory, Slot }

    public struct DragPayload
    {
        public DragSourceType source;
        public ItemSO item;         // �� ItemSO�� ���� ����
        public int memberIndex;     // ���� ��� �� �� ���
        public EquipSlot slot;      // ���� ��� �� �� ����
    }

    public static class DragContext
    {
        public static DragPayload Current;
        public static bool IsActive => Current.source != DragSourceType.None;

        public static void StartFromInventory(ItemSO item)
        {
            Current = new DragPayload { source = DragSourceType.Inventory, item = item };
        }

        public static void StartFromSlot(int memberIndex, EquipSlot slot, ItemSO item)
        {
            Current = new DragPayload { source = DragSourceType.Slot, memberIndex = memberIndex, slot = slot, item = item };
        }

        public static void Clear() => Current = default;
    }
}
