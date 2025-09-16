// RunManager.cs - 전체 코드
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Services;
using Game.Data;
using Game.Items;

namespace Game.Runtime
{
    public sealed class RunManager
    {
        // [추가] 데이터 변경을 UI에 알리기 위한 C# 이벤트
        // static으로 선언하여 어디서든 구독할 수 있게 합니다.
        public static event Action OnRunDataChanged;

        public int Seed { get; private set; }
        public int Floor { get; private set; } = 0;
        public string CurrentNodeId { get; private set; }

        private readonly DataCatalog _data;
        private System.Random _rng;

        private readonly List<string> _inventoryItemIds = new List<string>();
        public IReadOnlyList<string> InventoryItemIds => _inventoryItemIds;

        private readonly List<PartyMemberState> _partyState = new List<PartyMemberState>();
        public IReadOnlyList<PartyMemberState> PartyState => _partyState;

        public RunManager(int seed, DataCatalog data)
        {
            Seed = seed;
            _data = data;
            _rng = new System.Random(seed);
        }

        public int NextBattleSeed() => _rng.Next(int.MinValue, int.MaxValue);

        public BattleRequest BuildBattleRequest(EncounterSO enc, int difficulty = 0)
        {
            if (enc == null) throw new ArgumentNullException(nameof(enc));
            var seed = NextBattleSeed();
            var req = new BattleRequest(enc.encounterId, seed, difficulty);
            Debug.Log($"[RunManager] BuildBattleRequest enc={enc.encounterId} seed={seed} diff={difficulty}");
            return req;
        }

        public void Reseed(int newSeed)
        {
            Seed = newSeed;
            _rng = new System.Random(newSeed);
        }

        public void SetCurrentNode(MapNodeSO node) => CurrentNodeId = node ? node.nodeId : null;

        public MapNodeSO GetCurrentNode() =>
            string.IsNullOrEmpty(CurrentNodeId) ? null : GetNodeById(CurrentNodeId);

        public void AdvanceFloor(int delta = 1) => Floor = Math.Max(0, Floor + delta);

        public void SetPartyFromUnitSOs(IEnumerable<UnitSO> partyMembers)
        {
            _partyState.Clear();
            if (partyMembers == null) return;
            foreach (var memberSO in partyMembers)
            {
                if (memberSO != null)
                {
                    var newMember = new PartyMemberState(memberSO.unitId);
                    var equip = memberSO.defaultEquipment;
                    if (equip.rightHand) newMember.equippedItemIds[EquipSlot.RightHand] = equip.rightHand.itemId;
                    if (equip.leftHand) newMember.equippedItemIds[EquipSlot.LeftHand] = equip.leftHand.itemId;
                    if (equip.helmet) newMember.equippedItemIds[EquipSlot.Helmet] = equip.helmet.itemId;
                    if (equip.armor) newMember.equippedItemIds[EquipSlot.Armor] = equip.armor.itemId;
                    _partyState.Add(newMember);
                }
            }
            // [추가] 파티 데이터가 변경되었으므로 이벤트 호출
            OnRunDataChanged?.Invoke();
        }

        // [수정] ItemSO 대신 itemId를 받도록 수정
        public void EquipItem(int partyMemberIndex, EquipSlot slot, string itemId)
        {
            if (partyMemberIndex < 0 || partyMemberIndex >= _partyState.Count || string.IsNullOrEmpty(itemId)) return;
            _partyState[partyMemberIndex].equippedItemIds[slot] = itemId;

            // [추가] 데이터 변경 이벤트 호출
            OnRunDataChanged?.Invoke();
        }

        public void UnequipItem(int partyMemberIndex, EquipSlot slot)
        {
            if (partyMemberIndex < 0 || partyMemberIndex >= _partyState.Count) return;
            _partyState[partyMemberIndex].equippedItemIds.Remove(slot);

            // [추가] 데이터 변경 이벤트 호출
            OnRunDataChanged?.Invoke();
        }

        public void AddItemToInventory(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            _inventoryItemIds.Add(itemId);
            Debug.Log($"{itemId} 을(를) 인벤토리에 추가했습니다.");

            // [추가] 데이터 변경 이벤트 호출
            OnRunDataChanged?.Invoke();
        }

        public void RemoveItemFromInventory(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            _inventoryItemIds.Remove(itemId);

            // [추가] 데이터 변경 이벤트 호출
            OnRunDataChanged?.Invoke();
        }

        public IReadOnlyList<UnitSO> GetPartyAsUnitSOs()
        {
            var unitSOs = new List<UnitSO>();
            if (_data == null) return unitSOs;

            foreach (var memberState in _partyState)
            {
                var unitSO = _data.GetUnitById(memberState.unitId);
                if (unitSO != null)
                {
                    unitSOs.Add(unitSO);
                }
            }
            return unitSOs;
        }

        public EncounterSO GetEncounterById(string id) => _data ? _data.GetEncounterById(id) : null;
        public MapNodeSO GetNodeById(string id) => _data ? _data.GetNodeById(id) : null;

        [Serializable]
        public struct Snapshot
        {
            public int seed;
            public int floor;
            public string currentNodeId;
            public List<PartyMemberState> partyState;
            public List<string> inventoryItemIds; // [추가] 인벤토리도 저장
        }

        public Snapshot ToSnapshot() => new Snapshot
        {
            seed = Seed,
            floor = Floor,
            currentNodeId = CurrentNodeId,
            partyState = new List<PartyMemberState>(_partyState),
            inventoryItemIds = new List<string>(_inventoryItemIds) // [추가]
        };

        public static RunManager FromSnapshot(Snapshot s, DataCatalog data)
        {
            var rm = new RunManager(s.seed, data)
            {
                Floor = Math.Max(0, s.floor),
                CurrentNodeId = s.currentNodeId
            };
            if (s.partyState != null)
            {
                rm._partyState.AddRange(s.partyState);
            }
            if (s.inventoryItemIds != null)
            {
                rm._inventoryItemIds.AddRange(s.inventoryItemIds); // [추가]
            }
            return rm;
        }
    }
}