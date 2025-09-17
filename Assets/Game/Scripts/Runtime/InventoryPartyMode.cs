// Assets/Game/Scripts/Runtime/InventoryPartyMode.cs
using UnityEngine;
using UnityEngine.UI;
using Game.Services;
using Game.UI;
using Game.Data;
using Game.Items;
using System.Collections.Generic;
using System.Linq;

namespace Game.Runtime
{
    public sealed class InventoryPartyMode : MonoBehaviour, IGameMode
    {
        private GameFlowController _flow;

        [Header("UI 컨트롤러 참조")]
        [SerializeField] private PartySetupView _partySetupView;
        [SerializeField] private StatDisplayPanel _statDisplayPanel;
        [SerializeField] private EquipmentSlotPanel _equipmentSlotPanel;
        [SerializeField] private InventoryListPanel _inventoryListPanel;
        [SerializeField] private Image _characterPortrait;
        [SerializeField] private Button _confirmButton;

        [Header("프리팹")]
        [SerializeField] private GameObject _itemIconPrefab;

        [Header("드래그앤드롭 설정")]
        public Transform dragParent;

        private RunManager _currentRun;
        private int _selectedMemberIndex = -1;
        private bool _isReadOnly = false;

        private bool _lastDropSucceeded;

        public void Setup(GameFlowController flow) => _flow = flow;
        public GameObject GetItemIconPrefab() => _itemIconPrefab;

        public void NotifyDropSucceeded() => _lastDropSucceeded = true;
        public bool ConsumeDropSuccess()
        {
            var ok = _lastDropSucceeded;
            _lastDropSucceeded = false;
            return ok;
        }

        void Awake()
        {
            if (!_partySetupView) Debug.LogError("PartySetupView가 인스펙터에 연결되지 않았습니다!", this);
            if (!_statDisplayPanel) Debug.LogError("StatDisplayPanel이 인스펙터에 연결되지 않았습니다!", this);
            if (!_equipmentSlotPanel) Debug.LogError("EquipmentSlotPanel이 인스펙터에 연결되지 않았습니다!", this);
            if (!_inventoryListPanel) Debug.LogError("InventoryListPanel이 인스펙터에 연결되지 않았습니다!", this);
            if (_confirmButton) _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnEnable() => RunManager.OnRunDataChanged += RefreshAllUI;
        private void OnDisable() => RunManager.OnRunDataChanged -= RefreshAllUI;

        public void ForceRefreshUI() => RefreshAllUI();

        public void EnterMode() => Open(false);

        // InventoryPartyMode.Open
        public void Open(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
            Debug.Log($"[INV] Open(readOnly={_isReadOnly}) phase={_flow?.Phase}"); // ★ 확인 로그
            gameObject.SetActive(true);
            _currentRun = GameManager.I?.CurrentRun;
            if (_currentRun == null) { Debug.LogError("RunManager null"); gameObject.SetActive(false); return; }
            RefreshAllUI();
        }


        public void ExitMode() => gameObject.SetActive(false);

        private void OnConfirmClicked()
        {
            var battleManager = Battle.BattleManager.Instance;
            if (battleManager != null && battleManager.State == Battle.BattleState.Spawning)
            {
                battleManager.StartBattle();
                ExitMode();
            }
            else
            {
                _flow?.RequestMap();
            }
        }

        private void RefreshAllUI()
        {
            if (_currentRun == null || !gameObject.activeInHierarchy) return;

            if (_partySetupView)
            {
                _partySetupView.BuildSlots(4, GetSlotLabel, SelectPartyMember);
                if (_selectedMemberIndex < 0 || _selectedMemberIndex >= _currentRun.PartyState.Count)
                    _selectedMemberIndex = (_currentRun.PartyState.Count > 0) ? 0 : -1;
                _partySetupView.SetSelection(_selectedMemberIndex);
            }

            DisplayMemberDetails();

            if (_inventoryListPanel)
                _inventoryListPanel.Refresh(this, _currentRun.InventoryItemIds, !_isReadOnly);
        }

        private string GetSlotLabel(int index)
        {
            if (index < _currentRun.PartyState.Count)
            {
                var unitSO = GameManager.I.Data.GetUnitById(_currentRun.PartyState[index].unitId);
                return unitSO != null ? unitSO.displayName : "(알 수 없음)";
            }
            return "(비어있음)";
        }

        private void SelectPartyMember(int index)
        {
            if (index < 0 || index >= _currentRun.PartyState.Count) return;
            _selectedMemberIndex = index;
            _partySetupView?.SetSelection(index);
            DisplayMemberDetails();
        }

        private void DisplayMemberDetails()
        {
            if (_selectedMemberIndex < 0 || _selectedMemberIndex >= _currentRun.PartyState.Count)
            {
                if (_characterPortrait) _characterPortrait.enabled = false;
                _statDisplayPanel?.UpdateStats(null, null);
                _equipmentSlotPanel?.UpdateSlots(null, -1, this, !_isReadOnly);
                return;
            }

            var memberState = _currentRun.PartyState[_selectedMemberIndex];
            var unitSO = GameManager.I.Data.GetUnitById(memberState.unitId);
            if (unitSO == null) return;

            if (_characterPortrait != null)
            {
                _characterPortrait.sprite = unitSO.portrait;
                _characterPortrait.enabled = true;
            }

            _statDisplayPanel?.UpdateStats(unitSO, memberState);
            _equipmentSlotPanel?.UpdateSlots(memberState, _selectedMemberIndex, this, !_isReadOnly);
        }

        public void HandleEquipRequest(ItemSO itemToEquip, int characterIndex, EquipSlot targetSlot)
        {
            // 읽기전용이면 즉시 차단
            if (_isReadOnly || itemToEquip == null || characterIndex < 0) return;

            var memberState = _currentRun.PartyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(targetSlot, out var existingItemId))
                _currentRun.AddItemToInventory(existingItemId);

            _currentRun.EquipItem(characterIndex, targetSlot, itemToEquip.itemId);
            _currentRun.RemoveItemFromInventory(itemToEquip.itemId);
            // RunManager 내부에서 OnRunDataChanged가 발생하도록 되어 있어야 합니다.
        }

        public void HandleUnequipRequest(int characterIndex, EquipSlot sourceSlot)
        {
            if (_isReadOnly || characterIndex < 0) return;

            var memberState = _currentRun.PartyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(sourceSlot, out var itemIdToUnequip))
            {
                _currentRun.AddItemToInventory(itemIdToUnequip);
                _currentRun.UnequipItem(characterIndex, sourceSlot);
                // RunManager 내부에서 OnRunDataChanged가 발생하도록 되어 있어야 합니다.
            }
        }
    }
}
