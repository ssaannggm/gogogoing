// Assets/Game/Scripts/Runtime/InventoryPartyMode.DragApplySO.cs
using UnityEngine;
using Game.Items;
using Game.UI;
using Game.Services;

namespace Game.Runtime
{
    public sealed partial class InventoryPartyMode
    {
        // ★ SO 기반 드롭 → 슬롯 적용
        public void ApplyDropToSlotSO(int targetMemberIndex, EquipSlot targetSlot)
        {
            if (!DragContext.IsActive || GameManager.I?.CurrentRun == null) { DragContext.Clear(); return; }
            var run = GameManager.I.CurrentRun;
            var payload = DragContext.Current;
            var item = payload.item;
            if (item == null) { DragContext.Clear(); return; }

            // 슬롯 검증
            if (item.slot != targetSlot) { DragContext.Clear(); ForceRefreshUI(); return; }

            // 현재 슬롯의 기존 아이템(있으면)
            string existingId = null;
            if (targetMemberIndex >= 0 &&
                run.PartyState[targetMemberIndex].equippedItemIds.TryGetValue(targetSlot, out existingId))
            {
                // 동일 아이템이면 NO-OP
                if (!string.IsNullOrEmpty(existingId) && existingId == item.itemId)
                {
                    DragContext.Clear(); RefreshAllUI(); return;
                }
            }

            switch (payload.source)
            {
                case DragSourceType.Inventory:
                    {
                        // 교체 순서 보장: 1) 기존 있으면 인벤토리로 2) 새로 장착 3) 인벤토리에서 제거
                        if (!string.IsNullOrEmpty(existingId))
                            run.AddItemToInventory(existingId);

                        run.EquipItem(targetMemberIndex, targetSlot, item.itemId);
                        run.RemoveItemFromInventory(item.itemId);
                        break;
                    }

                case DragSourceType.Slot:
                    {
                        // 같은 슬롯/멤버면 이미 위에서 동일 아이템 NO-OP로 빠짐
                        if (payload.memberIndex == targetMemberIndex && payload.slot == targetSlot)
                        {
                            DragContext.Clear(); RefreshAllUI(); return;
                        }

                        // 출발 슬롯 해제
                        run.UnequipItem(payload.memberIndex, payload.slot);

                        // 타깃에 기존 있으면 인벤토리로
                        if (!string.IsNullOrEmpty(existingId))
                            run.AddItemToInventory(existingId);

                        // 타깃에 장착
                        run.EquipItem(targetMemberIndex, targetSlot, item.itemId);
                        break;
                    }
            }

            DragContext.Clear();
            RefreshAllUI();
        }

        // ★ SO 기반 드롭 → 인벤토리로 해제
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
