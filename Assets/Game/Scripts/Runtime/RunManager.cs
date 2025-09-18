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
        public static event Action OnRunDataChanged;

        public int Seed { get; private set; }
        public int Floor { get; private set; } = 0;
        public string CurrentNodeId { get; private set; }

        private int _gold;
        public int Gold => _gold;

        private int _fame;
        public int Fame => _fame;

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
            _gold = 0;
            _fame = 0;
        }

        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        // ★★★★★ 여기가 핵심: 보상 꾸러미 적용 함수 ★★★★★
        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

        /// <summary>
        /// Reward 구조체에 담긴 보상 꾸러미를 한 번에 적용합니다.
        /// </summary>
        public void ApplyReward(Reward reward)
        {
            // 기존에 만들어 둔 안전한 함수들을 그대로 재사용합니다.
            AddGold(reward.gold);
            AddFame(reward.fame);

            if (reward.items != null)
            {
                foreach (var item in reward.items)
                {
                    if (item != null)
                        AddItemToInventory(item.itemId);
                }
            }
        }

        // --- 개별 재화 관리 함수들 (기존과 동일) ---
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            _gold += amount;
            Debug.Log($"골드 +{amount} / 현재 골드: {_gold}");
            OnRunDataChanged?.Invoke();
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || _gold < amount)
            {
                Debug.LogWarning($"골드 부족! 필요: {amount}, 보유: {_gold}");
                return false;
            }
            _gold -= amount;
            Debug.Log($"골드 -{amount} / 현재 골드: {_gold}");
            OnRunDataChanged?.Invoke();
            return true;
        }

        public void AddFame(int amount)
        {
            if (amount <= 0) return;
            _fame += amount;
            Debug.Log($"명성 +{amount} / 현재 명성: {_fame}");
            OnRunDataChanged?.Invoke();
        }

        public void AddItemToInventory(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            _inventoryItemIds.Add(itemId);
            Debug.Log($"{itemId} 을(를) 인벤토리에 추가했습니다.");
            OnRunDataChanged?.Invoke();
        }

        public void RemoveItemFromInventory(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            _inventoryItemIds.Remove(itemId);
            OnRunDataChanged?.Invoke();
        }

        // ... (이하 나머지 코드는 이전과 동일합니다) ...
        #region Unchanged Methods
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
            OnRunDataChanged?.Invoke();
        }

        public void EquipItem(int partyMemberIndex, EquipSlot slot, string itemId)
        {
            if (partyMemberIndex < 0 || partyMemberIndex >= _partyState.Count || string.IsNullOrEmpty(itemId)) return;
            _partyState[partyMemberIndex].equippedItemIds[slot] = itemId;
            OnRunDataChanged?.Invoke();
        }

        public void UnequipItem(int partyMemberIndex, EquipSlot slot)
        {
            if (partyMemberIndex < 0 || partyMemberIndex >= _partyState.Count) return;
            _partyState[partyMemberIndex].equippedItemIds.Remove(slot);
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
        #endregion

        #region Snapshot
        [Serializable]
        public struct Snapshot
        {
            public int seed;
            public int floor;
            public string currentNodeId;
            public List<PartyMemberState> partyState;
            public List<string> inventoryItemIds;
            public int gold;
            public int fame;
        }

        public Snapshot ToSnapshot() => new Snapshot
        {
            seed = Seed,
            floor = Floor,
            currentNodeId = CurrentNodeId,
            partyState = new List<PartyMemberState>(_partyState),
            inventoryItemIds = new List<string>(_inventoryItemIds),
            gold = _gold,
            fame = _fame
        };

        public static RunManager FromSnapshot(Snapshot s, DataCatalog data)
        {
            var rm = new RunManager(s.seed, data)
            {
                Floor = Math.Max(0, s.floor),
                CurrentNodeId = s.currentNodeId,
                _gold = s.gold,
                _fame = s.fame
            };
            if (s.partyState != null)
            {
                rm._partyState.AddRange(s.partyState);
            }
            if (s.inventoryItemIds != null)
            {
                rm._inventoryItemIds.AddRange(s.inventoryItemIds);
            }
            return rm;
        }
        #endregion
    }
}