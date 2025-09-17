// Assets/Game/Scripts/Battle/BattleInventoryToggle.cs (교체)
using UnityEngine;
using Game.Runtime;

namespace Game.Battle
{
    public enum BattlePhase { Spawning, Setup, Running, Results }

    public sealed class BattleInventoryToggle : MonoBehaviour
    {
        [SerializeField] private int inventorySortingOrder = 500;
        [SerializeField] private InventoryPartyMode inventory; // Inspector에서 직접 연결 권장

        private PlayerControls _controls;
        private bool _visible = false;
        public BattlePhase Phase { get; private set; } = BattlePhase.Setup;

        void Awake()
        {
            if (!inventory) inventory = FindObjectOfType<InventoryPartyMode>(true);
            if (!inventory) { Debug.LogError("[BattleInventoryToggle] InventoryPartyMode를 찾을 수 없습니다."); return; }

            // 씬 진입 시 혹시 다른 곳에서 켜둔 상태라면 강제로 닫아서 기준 맞추기
            if (inventory.Visible)
                inventory.ExitMode();
        }

        void OnEnable()
        {
            if (!inventory) inventory = FindObjectOfType<InventoryPartyMode>(true);
            if (!inventory) { Debug.LogError("[BattleInventoryToggle] InventoryPartyMode를 찾을 수 없습니다."); return; }

            // 씬 진입 시 혹시 다른 곳에서 켜둔 상태라면 강제로 닫아서 기준 맞추기
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

            // 페이즈에 따른 읽기전용 락을 Inventory에 전달
            if (inventory)
            {
                bool lockRO = (Phase != BattlePhase.Setup);
                inventory.SetReadOnlyLock(lockRO);

                // 이미 열려 있으면 상태 갱신 강제
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

            _visible = !inventory.Visible; // 현재 실제 표시 상태 기준으로 토글
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

            // 드롭 수신/차단 설정 정리
            foreach (var slot in inventory.GetComponentsInChildren<EquipmentSlotUI>(true))
            {
                var img = slot.GetComponent<UnityEngine.UI.Image>();
                if (img) img.raycastTarget = true; // 빈 슬롯도 드롭 수신
            }
            if (inventory.dragParent &&
                inventory.dragParent.TryGetComponent(out UnityEngine.UI.Image dlImg))
                dlImg.raycastTarget = false;

            inventory.ForceRefreshUI();
        }
    }
}
