// Assets/Game/Scripts/Battle/BattleInventoryToggle.cs (��ü)
using UnityEngine;
using Game.Runtime;

namespace Game.Battle
{
    public enum BattlePhase { Spawning, Setup, Running, Results }

    public sealed class BattleInventoryToggle : MonoBehaviour
    {
        [SerializeField] private int inventorySortingOrder = 500;
        [SerializeField] private InventoryPartyMode inventory; // Inspector���� ���� ���� ����

        private PlayerControls _controls;
        private bool _visible = false;
        public BattlePhase Phase { get; private set; } = BattlePhase.Setup;

        void Awake()
        {
            if (!inventory) inventory = FindObjectOfType<InventoryPartyMode>(true);
            if (!inventory) { Debug.LogError("[BattleInventoryToggle] InventoryPartyMode�� ã�� �� �����ϴ�."); return; }

            // �� ���� �� Ȥ�� �ٸ� ������ �ѵ� ���¶�� ������ �ݾƼ� ���� ���߱�
            if (inventory.Visible)
                inventory.ExitMode();
        }

        void OnEnable()
        {
            if (!inventory) inventory = FindObjectOfType<InventoryPartyMode>(true);
            if (!inventory) { Debug.LogError("[BattleInventoryToggle] InventoryPartyMode�� ã�� �� �����ϴ�."); return; }

            // �� ���� �� Ȥ�� �ٸ� ������ �ѵ� ���¶�� ������ �ݾƼ� ���� ���߱�
            if (inventory.Visible)
                inventory.ExitMode();
        }

        void OnDisable()
        {
            if (_controls != null)
            {
                _controls.UI.ToggleInventory.performed -= OnTogglePerformed;
                _controls.Disable();
            }
        }

        private void OnTogglePerformed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            Toggle();
        }

        public void SetPhase(BattlePhase phase)
        {
            Phase = phase;

            // ����� ���� �б����� ���� Inventory�� ����
            if (inventory)
            {
                bool lockRO = (Phase != BattlePhase.Setup);
                inventory.SetReadOnlyLock(lockRO);

                // �̹� ���� ������ ���� ���� ����
                if (inventory.Visible)
                {
                    inventory.Open(inventory.IsReadOnly);
                    EnsureTop();
                }
            }
        }

        public void Toggle()
        {
            if (!inventory) return;

            _visible = !inventory.Visible; // ���� ���� ǥ�� ���� �������� ���
            if (_visible)
            {
                bool readOnly = (Phase != BattlePhase.Setup);
                inventory.Open(readOnly);
                EnsureTop();
                Debug.Log($"[BattleInventoryToggle] Open (readOnly={readOnly}, phase={Phase})");
            }
            else
            {
                inventory.ExitMode();
                Debug.Log("[BattleInventoryToggle] Close");
            }
        }

        private void EnsureTop()
        {
            var invCanvas = inventory.GetComponentInChildren<Canvas>(true);
            if (invCanvas)
            {
                invCanvas.overrideSorting = true;
                invCanvas.sortingOrder = inventorySortingOrder;
            }

            // ��� ����/���� ���� ����
            foreach (var slot in inventory.GetComponentsInChildren<EquipmentSlotUI>(true))
            {
                var img = slot.GetComponent<UnityEngine.UI.Image>();
                if (img) img.raycastTarget = true; // �� ���Ե� ��� ����
            }
            if (inventory.dragParent &&
                inventory.dragParent.TryGetComponent(out UnityEngine.UI.Image dlImg))
                dlImg.raycastTarget = false;

            inventory.ForceRefreshUI();
        }
    }
}
