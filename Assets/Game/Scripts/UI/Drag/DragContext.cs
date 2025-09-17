// Assets/Game/Scripts/UI/Drag/DragContext.cs
using Game.Items;

namespace Game.UI
{
    public enum DragSourceType { None, Inventory, Slot }

    public struct DragPayload
    {
        public DragSourceType source;
        public ItemSO item;         // ¡Ú ItemSO·Î Á÷Á¢ Àü´Ş
        public int memberIndex;     // ½½·Ô Ãâ¹ß ½Ã ¿ø ¸â¹ö
        public EquipSlot slot;      // ½½·Ô Ãâ¹ß ½Ã ¿ø ½½·Ô
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
