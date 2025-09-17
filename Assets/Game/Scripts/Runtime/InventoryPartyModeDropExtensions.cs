// Assets/Game/Scripts/Runtime/InventoryPartyMode.DragApplySO.cs
using UnityEngine;
using Game.Items;
using Game.UI;
using Game.Services;

namespace Game.Runtime
{
    public sealed partial class InventoryPartyMode
    {
        // �� SO ��� ��� �� ���� ����
        public void ApplyDropToSlotSO(int targetMemberIndex, EquipSlot targetSlot)
        {
            if (!DragContext.IsActive || GameManager.I?.CurrentRun == null) { DragContext.Clear(); return; }
            var run = GameManager.I.CurrentRun;
            var payload = DragContext.Current;
            var item = payload.item;
            if (item == null) { DragContext.Clear(); return; }

            // ���� ����
            if (item.slot != targetSlot) { DragContext.Clear(); ForceRefreshUI(); return; }

            // ���� ������ ���� ������(������)
            string existingId = null;
            if (targetMemberIndex >= 0 &&
                run.PartyState[targetMemberIndex].equippedItemIds.TryGetValue(targetSlot, out existingId))
            {
                // ���� �������̸� NO-OP
                if (!string.IsNullOrEmpty(existingId) && existingId == item.itemId)
                {
                    DragContext.Clear(); RefreshAllUI(); return;
                }
            }

            switch (payload.source)
            {
                case DragSourceType.Inventory:
                    {
                        // ��ü ���� ����: 1) ���� ������ �κ��丮�� 2) ���� ���� 3) �κ��丮���� ����
                        if (!string.IsNullOrEmpty(existingId))
                            run.AddItemToInventory(existingId);

                        run.EquipItem(targetMemberIndex, targetSlot, item.itemId);
                        run.RemoveItemFromInventory(item.itemId);
                        break;
                    }

                case DragSourceType.Slot:
                    {
                        // ���� ����/����� �̹� ������ ���� ������ NO-OP�� ����
                        if (payload.memberIndex == targetMemberIndex && payload.slot == targetSlot)
                        {
                            DragContext.Clear(); RefreshAllUI(); return;
                        }

                        // ��� ���� ����
                        run.UnequipItem(payload.memberIndex, payload.slot);

                        // Ÿ�꿡 ���� ������ �κ��丮��
                        if (!string.IsNullOrEmpty(existingId))
                            run.AddItemToInventory(existingId);

                        // Ÿ�꿡 ����
                        run.EquipItem(targetMemberIndex, targetSlot, item.itemId);
                        break;
                    }
            }

            DragContext.Clear();
            RefreshAllUI();
        }

        // �� SO ��� ��� �� �κ��丮�� ����
        public void ApplyDropToInventorySO()
        {
            if (!DragContext.IsActive || GameManager.I?.CurrentRun == null) { DragContext.Clear(); return; }
            var run = GameManager.I.CurrentRun;
            var payload = DragContext.Current;

            if (payload.source == DragSourceType.Slot && payload.item != null)
            {
                run.UnequipItem(payload.memberIndex, payload.slot);
                run.AddItemToInventory(payload.item.itemId);
            }

            DragContext.Clear();
            RefreshAllUI();
        }
    }
}
