using System;
using System.Collections.Generic;
using System.Linq; // Linq 사용을 위해 추가
using UnityEngine;
using Game.Services;
using Game.Data;
using Game.Items;

namespace Game.Runtime
{
    public sealed class RunManager
    {
        public int Seed { get; private set; }
        public int Floor { get; private set; } = 0;
        public string CurrentNodeId { get; private set; }

        private readonly DataCatalog _data;
        private System.Random _rng;

        // [추가] 이번 런에서 획득한 아이템 ID 목록
        private readonly List<string> _inventoryItemIds = new List<string>();
        public IReadOnlyList<string> InventoryItemIds => _inventoryItemIds;

        // [수정] string 리스트 대신 PartyMemberState 리스트를 사용 , 참고: Snapshot 저장/로드 로직에도 나중에 이 _inventoryItemIds를 포함시켜야 합니다.
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
        }

        public void EquipItem(int partyMemberIndex, ItemSO item)
        {
            if (partyMemberIndex < 0 || partyMemberIndex >= _partyState.Count || item == null) return;
            _partyState[partyMemberIndex].equippedItemIds[item.slot] = item.itemId;
        }

        public void UnequipItem(int partyMemberIndex, EquipSlot slot)
        {
            if (partyMemberIndex < 0 || partyMemberIndex >= _partyState.Count) return;
            _partyState[partyMemberIndex].equippedItemIds.Remove(slot);
        }

        // [수정] _partyState를 기반으로 UnitSO 목록을 반환하도록 수정
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

        // --- DataCatalog 위임 ---
        public EncounterSO GetEncounterById(string id) => _data ? _data.GetEncounterById(id) : null;
        public MapNodeSO GetNodeById(string id) => _data ? _data.GetNodeById(id) : null;

        // --- 스냅샷 ---
        [Serializable]
        public struct Snapshot
        {
            public int seed;
            public int floor;
            public string currentNodeId;
            // [수정] string 목록 대신 PartyMemberState 목록을 저장
            public List<PartyMemberState> partyState;
        }

        public Snapshot ToSnapshot() => new Snapshot
        {
            seed = Seed,
            floor = Floor,
            currentNodeId = CurrentNodeId,
            // [수정] _partyState를 그대로 저장
            partyState = new List<PartyMemberState>(_partyState)
        };

        public static RunManager FromSnapshot(Snapshot s, DataCatalog data)
        {
            var rm = new RunManager(s.seed, data)
            {
                Floor = Math.Max(0, s.floor),
                CurrentNodeId = s.currentNodeId
            };
            // [수정] Snapshot의 partyState를 복원
            if (s.partyState != null)
            {
                rm._partyState.AddRange(s.partyState);
            }
            return rm;
        }
        // [추가] 인벤토리에 아이템을 추가/제거하는 함수
        public void AddItemToInventory(ItemSO item)
        {
            if (item == null) return;
            _inventoryItemIds.Add(item.itemId);
            Debug.Log($"{item.displayName}을(를) 인벤토리에 추가했습니다.");
        }

        public void RemoveItemFromInventory(ItemSO item)
        {
            if (item == null) return;
            _inventoryItemIds.Remove(item.itemId);
        }
    }
}