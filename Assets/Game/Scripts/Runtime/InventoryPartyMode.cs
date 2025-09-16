using UnityEngine;
using UnityEngine.UI;
using Game.Services;
using Game.UI;
using Game.Data;
using Game.Visual;
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
        [SerializeField] private GameObject _characterPrefab;
        [SerializeField] private GameObject _itemIconPrefab;

        [Header("드래그앤드롭 설정")]
        public Transform dragParent;

        private IReadOnlyList<PartyMemberState> _partyState;
        private List<UnitSO> _partyUnitSOs = new List<UnitSO>();
        private List<UnitStats> _partyMemberInstances = new List<UnitStats>();
        private int _selectedMemberIndex = -1;
        private bool _isReadOnly = false;

        public void Setup(GameFlowController flow) => _flow = flow;
        public GameObject GetItemIconPrefab() => _itemIconPrefab;

        void Awake()
        {
            if (!_partySetupView) _partySetupView = GetComponentInChildren<PartySetupView>(true);
            if (!_statDisplayPanel) _statDisplayPanel = GetComponentInChildren<StatDisplayPanel>(true);
            if (!_equipmentSlotPanel) _equipmentSlotPanel = GetComponentInChildren<EquipmentSlotPanel>(true);
            if (!_inventoryListPanel) _inventoryListPanel = GetComponentInChildren<InventoryListPanel>(true);
            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        public void EnterMode() => Open(false);

        public void Open(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
            gameObject.SetActive(true);

            if (_inventoryListPanel != null)
                _inventoryListPanel.SetDraggable(!_isReadOnly);

            if (_equipmentSlotPanel != null)
                _equipmentSlotPanel.SetDraggable(!_isReadOnly);

            RefreshAllUI();
        }

        public void ExitMode()
        {
            ClearInstances();
            gameObject.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            var battleManager = FindObjectOfType<Game.Battle.BattleManager>();
            if (battleManager != null && battleManager.State == Game.Battle.BattleState.Spawning)
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
            LoadDataAndCreateInstances();
            _inventoryListPanel?.Refresh(this);
        }

        private void LoadDataAndCreateInstances()
        {
            var run = GameManager.I?.CurrentRun;
            var dataCatalog = GameManager.I?.Data;
            if (run == null || dataCatalog == null) return;

            _partyState = run.PartyState;
            _partyUnitSOs = run.GetPartyAsUnitSOs().ToList();

            ClearInstances();

            foreach (var memberState in _partyState)
            {
                var unitSO = dataCatalog.GetUnitById(memberState.unitId);
                if (unitSO == null || _characterPrefab == null) continue;

                var unitGO = Instantiate(_characterPrefab, transform);
                foreach (var renderer in unitGO.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }

                var loadout = unitGO.GetComponent<UnitLoadout>();
                if (loadout)
                {
                    foreach (var itemEntry in memberState.equippedItemIds)
                    {
                        var itemSO = dataCatalog.GetItemById(itemEntry.Value);
                        if (itemSO != null) loadout.Equip(itemSO);
                    }
                }
                _partyMemberInstances.Add(unitGO.GetComponent<UnitStats>());
            }

            _partySetupView.BuildSlots(4, GetSlotLabel, SelectPartyMember);
            SelectPartyMember(_selectedMemberIndex >= 0 && _selectedMemberIndex < _partyMemberInstances.Count ? _selectedMemberIndex : 0);
        }

        private string GetSlotLabel(int index)
        {
            if (index < _partyUnitSOs.Count) return _partyUnitSOs[index].displayName;
            return "(비어있음)";
        }

        private void SelectPartyMember(int index)
        {
            if (index < 0 || index >= _partyMemberInstances.Count)
            {
                _selectedMemberIndex = -1;
                DisplayMemberDetails(null, null, -1);
                return;
            }
            _selectedMemberIndex = index;
            DisplayMemberDetails(_partyMemberInstances[index], _partyState[index], index);
        }

        private void DisplayMemberDetails(UnitStats memberStats, PartyMemberState memberState, int characterIndex)
        {
            if (memberState == null || memberStats == null)
            {
                if (_characterPortrait) _characterPortrait.enabled = false;
                if (_statDisplayPanel) _statDisplayPanel.UpdateStats(null);
                if (_equipmentSlotPanel) _equipmentSlotPanel.UpdateSlots(null, -1, this);
                return;
            }

            var unitSO = GameManager.I.Data.GetUnitById(memberState.unitId);
            if (unitSO == null) return;

            if (_characterPortrait != null)
            {
                _characterPortrait.sprite = unitSO.portrait;
                _characterPortrait.enabled = true;
            }

            if (_statDisplayPanel != null)
            {
                _statDisplayPanel.UpdateStats(memberStats);
            }

            if (_equipmentSlotPanel != null)
            {
                _equipmentSlotPanel.UpdateSlots(memberState, characterIndex, this);
            }
        }
        // [추가] 인벤토리 패널로부터 아이템 해제 '요청'을 받는 함수
        public void OnUnequipRequest(ItemSO itemToUnequip)
        {
            if (_isReadOnly) return; // 읽기 전용 모드에서는 해제 불가

            // 이 아이템을 장착하고 있는 캐릭터를 파티 상태에서 찾습니다.
            for (int i = 0; i < _partyState.Count; i++)
            {
                // 각 슬롯을 확인
                foreach (var equippedItem in _partyState[i].equippedItemIds)
                {
                    // 아이템 ID가 일치하는지 확인
                    if (equippedItem.Value == itemToUnequip.itemId)
                    {
                        // 일치하면 해당 캐릭터의 해당 슬롯 아이템을 해제
                        UnequipItem(i, equippedItem.Key);
                        return; // 찾았으므로 함수 종료
                    }
                }
            }
        }
        public void OnItemDroppedOnEquipmentSlot(ItemIconUI droppedItem, EquipmentSlotUI targetSlot)
        {
            if (_isReadOnly) return;
            if (droppedItem.ItemData.slot == targetSlot.slotType)
            {
                EquipItem(targetSlot.CharacterIndex, droppedItem.ItemData);
            }
        }

        public void OnItemDroppedOnInventory(EquipmentSlotUI sourceSlot)
        {
            if (_isReadOnly) return;
            UnequipItem(sourceSlot.CharacterIndex, sourceSlot.slotType);
        }

        public void EquipItem(int characterIndex, ItemSO itemToEquip)
        {
            if (characterIndex < 0) return;
            var run = GameManager.I?.CurrentRun;
            if (run == null) return;

            var memberState = _partyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(itemToEquip.slot, out var existingItemId))
            {
                var itemToUnequip = GameManager.I.Data.GetItemById(existingItemId);
                if (itemToUnequip != null) run.AddItemToInventory(itemToUnequip);
            }
            run.EquipItem(characterIndex, itemToEquip);
            run.RemoveItemFromInventory(itemToEquip);
            RefreshAllUI();
        }

        public void UnequipItem(int characterIndex, EquipSlot slot)
        {
            if (characterIndex < 0) return;
            var run = GameManager.I?.CurrentRun;
            if (run == null) return;

            var memberState = _partyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(slot, out var itemIdToUnequip))
            {
                var itemSO = GameManager.I.Data.GetItemById(itemIdToUnequip);
                if (itemSO != null) run.AddItemToInventory(itemSO);

                run.UnequipItem(characterIndex, slot);
                RefreshAllUI();
            }
        }

        private void ClearInstances()
        {
            foreach (var instance in _partyMemberInstances)
            {
                if (instance != null) Destroy(instance.gameObject);
            }
            _partyMemberInstances.Clear();
        }
    }
}