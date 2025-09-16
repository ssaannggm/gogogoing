// InventoryPartyMode.cs (최종 수정본)
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

        public void Setup(GameFlowController flow) => _flow = flow;
        public GameObject GetItemIconPrefab() => _itemIconPrefab;

        void Awake()
        {
            // [수정] 모든 UI 참조가 제대로 연결되었는지 확인
            if (!_partySetupView) Debug.LogError("PartySetupView가 인스펙터에 연결되지 않았습니다!", this);
            if (!_statDisplayPanel) Debug.LogError("StatDisplayPanel이 인스펙터에 연결되지 않았습니다!", this);
            if (!_equipmentSlotPanel) Debug.LogError("EquipmentSlotPanel이 인스PECTOR에 연결되지 않았습니다!", this);
            if (!_inventoryListPanel) Debug.LogError("InventoryListPanel이 인스펙터에 연결되지 않았습니다!", this);
            if (_confirmButton) _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnEnable() => RunManager.OnRunDataChanged += RefreshAllUI;
        private void OnDisable() => RunManager.OnRunDataChanged -= RefreshAllUI;

        public void EnterMode() => Open(false);

        public void Open(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
            gameObject.SetActive(true);

            _currentRun = GameManager.I?.CurrentRun;
            if (_currentRun == null)
            {
                Debug.LogError("RunManager가 존재하지 않아 파티 UI를 열 수 없습니다.");
                gameObject.SetActive(false);
                return;
            }
            RefreshAllUI();
        }

        public void ExitMode()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            var battleManager = Battle.BattleManager.Instance; // 싱글톤 인스턴스 사용
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
                {
                    _selectedMemberIndex = (_currentRun.PartyState.Count > 0) ? 0 : -1;
                }
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
            if (_partySetupView) _partySetupView.SetSelection(index);
            DisplayMemberDetails();
        }

        private void DisplayMemberDetails()
        {
            if (_selectedMemberIndex < 0 || _selectedMemberIndex >= _currentRun.PartyState.Count)
            {
                if (_characterPortrait) _characterPortrait.enabled = false;
                // [수정] UpdateStats(null, null)로 호출
                if (_statDisplayPanel) _statDisplayPanel.UpdateStats(null, null);
                if (_equipmentSlotPanel) _equipmentSlotPanel.UpdateSlots(null, -1, this, !_isReadOnly);
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

            if (_statDisplayPanel != null)
            {
                _statDisplayPanel.UpdateStats(unitSO, memberState);
            }

            if (_equipmentSlotPanel != null)
            {
                _equipmentSlotPanel.UpdateSlots(memberState, _selectedMemberIndex, this, !_isReadOnly);
            }
        }

        public void HandleEquipRequest(ItemSO itemToEquip, int characterIndex, EquipSlot targetSlot)
        {
            if (_isReadOnly || itemToEquip == null || characterIndex < 0) return;
            var memberState = _currentRun.PartyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(targetSlot, out var existingItemId))
            {
                _currentRun.AddItemToInventory(existingItemId);
            }
            _currentRun.EquipItem(characterIndex, targetSlot, itemToEquip.itemId);
            _currentRun.RemoveItemFromInventory(itemToEquip.itemId);
        }

        public void HandleUnequipRequest(int characterIndex, EquipSlot sourceSlot)
        {
            if (_isReadOnly || characterIndex < 0) return;
            var memberState = _currentRun.PartyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(sourceSlot, out var itemIdToUnequip))
            {
                _currentRun.AddItemToInventory(itemIdToUnequip);
                _currentRun.UnequipItem(characterIndex, sourceSlot);
            }
        }
    }
}