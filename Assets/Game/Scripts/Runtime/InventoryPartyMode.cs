// InventoryPartyMode.cs - 전체 코드
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
        // [삭제] 캐릭터 프리팹은 더 이상 여기서 필요 없음
        [SerializeField] private GameObject _itemIconPrefab;

        [Header("드래그앤드롭 설정")]
        public Transform dragParent;

        // [수정] 현재 런의 파티/인벤토리 데이터에 대한 참조만 유지
        private RunManager _currentRun;
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

        // [추가] RunManager의 데이터 변경 이벤트를 구독
        private void OnEnable()
        {
            RunManager.OnRunDataChanged += RefreshAllUI;
        }

        // [추가] 이벤트 구독 해제 (메모리 누수 방지)
        private void OnDisable()
        {
            RunManager.OnRunDataChanged -= RefreshAllUI;
        }

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

            RefreshAllUI(); // UI 최초 갱신
        }

        public void ExitMode()
        {
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

        // [수정] 이제 이 함수는 RunManager의 데이터를 읽어 UI에 뿌려주는 역할만 합니다.
        private void RefreshAllUI()
        {
            if (_currentRun == null || !gameObject.activeInHierarchy) return;

            // 1. 파티 슬롯 목록 갱신
            _partySetupView.BuildSlots(4, GetSlotLabel, SelectPartyMember);

            // 2. 선택된 멤버가 유효한지 확인하고, 아니면 0번으로 재설정
            if (_selectedMemberIndex < 0 || _selectedMemberIndex >= _currentRun.PartyState.Count)
            {
                _selectedMemberIndex = 0;
            }
            _partySetupView.SetSelection(_selectedMemberIndex);

            // 3. 선택된 멤버 정보 표시
            DisplayMemberDetails();

            // 4. 인벤토리 목록 갱신
            _inventoryListPanel?.Refresh(this, _currentRun.InventoryItemIds, !_isReadOnly);
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
            _partySetupView.SetSelection(index); // 시각적으로 선택되었음을 표시
            DisplayMemberDetails();
        }

        // [수정] 더 이상 임시 캐릭터 인스턴스를 사용하지 않음
        private void DisplayMemberDetails()
        {
            if (_selectedMemberIndex < 0 || _selectedMemberIndex >= _currentRun.PartyState.Count)
            {
                // 선택된 멤버가 없을 때 UI 클리어
                if (_characterPortrait) _characterPortrait.enabled = false;
                if (_statDisplayPanel) _statDisplayPanel.UpdateStats(null); // StatDisplayPanel은 null을 처리할 수 있어야 함
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

            // [핵심] 임시 캐릭터의 UnitStats 대신 PartyMemberState 자체를 넘겨 스탯을 표시
            // StatDisplayPanel이 이 데이터를 기반으로 스탯을 '계산'해서 보여주도록 수정해야 합니다.
            if (_statDisplayPanel != null)
            {
                // TODO: StatDisplayPanel이 UnitSO와 PartyMemberState를 받아 스탯을 계산하고 표시하도록 수정해야 합니다.
                // 지금은 임시로 null을 전달합니다.
                _statDisplayPanel.UpdateStats(null); // 이 부분은 StatDisplayPanel 수정 후 바꿔야 합니다.
            }

            if (_equipmentSlotPanel != null)
            {
                _equipmentSlotPanel.UpdateSlots(memberState, _selectedMemberIndex, this, !_isReadOnly);
            }
        }

        // --- 드래그 앤 드롭 로직 핸들러 ---
        // 모든 데이터 변경은 이 함수들을 통해 RunManager에 요청됩니다.

        public void HandleEquipRequest(ItemSO itemToEquip, int characterIndex, EquipSlot targetSlot)
        {
            if (_isReadOnly || itemToEquip == null || characterIndex < 0) return;

            // 1. 장착하려는 슬롯에 이미 다른 아이템이 있는지 확인
            var memberState = _currentRun.PartyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(targetSlot, out var existingItemId))
            {
                // 2. 만약 있다면, 그 아이템은 인벤토리로 보냄
                _currentRun.AddItemToInventory(existingItemId);
            }

            // 3. 새 아이템을 장착 (데이터 변경)
            _currentRun.EquipItem(characterIndex, targetSlot, itemToEquip.itemId);

            // 4. 인벤토리에서 새 아이템 제거 (데이터 변경)
            _currentRun.RemoveItemFromInventory(itemToEquip.itemId);

            // 5. RunManager가 이벤트를 호출하여 UI가 자동으로 새로고침될 것임
        }

        public void HandleUnequipRequest(int characterIndex, EquipSlot sourceSlot)
        {
            if (_isReadOnly || characterIndex < 0) return;

            var memberState = _currentRun.PartyState[characterIndex];
            if (memberState.equippedItemIds.TryGetValue(sourceSlot, out var itemIdToUnequip))
            {
                // 1. 벗은 아이템을 인벤토리에 추가 (데이터 변경)
                _currentRun.AddItemToInventory(itemIdToUnequip);

                // 2. 해당 슬롯을 비움 (데이터 변경)
                _currentRun.UnequipItem(characterIndex, sourceSlot);

                // 3. RunManager가 이벤트를 호출하여 UI가 자동으로 새로고침될 것임
            }
        }
    }
}