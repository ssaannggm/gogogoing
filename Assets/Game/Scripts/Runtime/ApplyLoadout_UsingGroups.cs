using System.Collections.Generic;
using UnityEngine;
using Game.Items;
using Game.Runtime;
using Game.Visual;

[RequireComponent(typeof(UnitLoadout), typeof(EquipmentSpriteGroups))]
public sealed class ApplyLoadout_UsingGroups : MonoBehaviour
{
    private UnitLoadout _loadout;
    private EquipmentSpriteGroups _spriteGroups;
    private SPUM_Prefabs _spum;

    // SPUM의 원본 애니메이션 클립들을 안전하게 백업하기 위한 Dictionary
    private Dictionary<string, List<AnimationClip>> _spumOriginalAnimations;

    private readonly Dictionary<EquipSlot, HashSet<string>> _touchedKeysBySlot = new();

    void Awake()
    {
        _loadout = GetComponent<UnitLoadout>();
        _spriteGroups = GetComponent<EquipmentSpriteGroups>();
        _spum = GetComponentInChildren<SPUM_Prefabs>(true);

        foreach (EquipSlot slot in System.Enum.GetValues(typeof(EquipSlot)))
        {
            _touchedKeysBySlot[slot] = new HashSet<string>();
        }

        // SPUM 초기화 및 원본 애니메이션 백업
        InitializeAndBackupSpumAnimations();
    }

    private void InitializeAndBackupSpumAnimations()
    {
        if (_spum == null)
        {
            Debug.LogWarning("[ApplyLoadout] SPUM_Prefabs 컴포넌트를 찾을 수 없습니다. 애니메이션 기능이 비활성화됩니다.");
            return;
        }

        // SPUM의 내부 목록이 비어있거나 초기화되지 않았다면, SPUM의 초기화 함수들을 호출합니다.
        if (_spum.StateAnimationPairs == null || _spum.StateAnimationPairs.Count == 0)
        {
            // PopulateAnimationLists가 Resources.Load를 사용하므로, 에디터가 아닐 때도 작동하는지 확인이 필요할 수 있습니다.
            _spum.PopulateAnimationLists();
            _spum.OverrideControllerInit();
        }

        // 백업을 위해 새로운 Dictionary를 생성합니다.
        _spumOriginalAnimations = new Dictionary<string, List<AnimationClip>>();

        // StateAnimationPairs가 초기화된 후에도 null이 아닌지 다시 한번 확인합니다.
        if (_spum.StateAnimationPairs != null)
        {
            foreach (var pair in _spum.StateAnimationPairs)
            {
                // 백업할 리스트가 null이 아닌 경우에만 복사합니다.
                if (pair.Value != null)
                {
                    _spumOriginalAnimations[pair.Key] = new List<AnimationClip>(pair.Value);
                }
            }
        }
    }

    void OnEnable()
    {
        if (_loadout) _loadout.OnLoadoutChanged += ReapplyAll;
        ReapplyAll();
    }

    void OnDisable()
    {
        if (_loadout) _loadout.OnLoadoutChanged -= ReapplyAll;
    }

    public void ReapplyAll()
    {
        if (_loadout == null) return;
        ApplyAllVisuals();
        RebuildAnimationLists();
    }

    private void ApplyAllVisuals()
    {
        if (_spriteGroups == null) return;

        _spriteGroups.ClearAll();
        foreach (var keySet in _touchedKeysBySlot.Values) keySet.Clear();

        ApplyVisualsForSlot(EquipSlot.Armor, _loadout.armor);
        ApplyVisualsForSlot(EquipSlot.Helmet, _loadout.helmet);
        ApplyVisualsForSlot(EquipSlot.LeftHand, _loadout.leftHand);
        ApplyVisualsForSlot(EquipSlot.RightHand, _loadout.rightHand);
    }

    private void ApplyVisualsForSlot(EquipSlot slot, ItemSO item)
    {
        if (item == null || _spriteGroups == null) return;

        var touchedKeys = _touchedKeysBySlot[slot];

        if (item.visualGroups != null)
        {
            foreach (var g in item.visualGroups)
            {
                if (!string.IsNullOrEmpty(g.key))
                {
                    _spriteGroups.Apply(g.key, g.sprites);
                    touchedKeys.Add(g.key);
                }
            }
        }

        if (item.groupsToHide != null)
        {
            foreach (var key in item.groupsToHide)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _spriteGroups.Clear(key);
                    touchedKeys.Add(key);
                }
            }
        }
    }

    private void RebuildAnimationLists()
    {
        // SPUM 관련 객체들이 하나라도 없으면 실행하지 않습니다.
        if (_spum == null || _spum.StateAnimationPairs == null || _spumOriginalAnimations == null) return;

        var items = new List<ItemSO> { _loadout.rightHand, _loadout.leftHand, _loadout.armor, _loadout.helmet };

        foreach (PlayerState state in System.Enum.GetValues(typeof(PlayerState)))
        {
            var stateName = state.ToString();
            if (!_spum.StateAnimationPairs.TryGetValue(stateName, out var targetList)) continue;

            // targetList가 null일 경우를 대비한 안전장치
            if (targetList == null) continue;

            targetList.Clear();

            foreach (var item in items)
            {
                if (item == null || item.animationOverrides == null) continue;
                foreach (var ov in item.animationOverrides)
                {
                    if (ov.state == state && ov.clip != null)
                    {
                        targetList.Add(ov.clip);
                    }
                }
            }

            // 아이템에서 가져온 애니메이션이 하나도 없다면, 백업해둔 원본으로 복구합니다.
            if (targetList.Count == 0 && _spumOriginalAnimations.TryGetValue(stateName, out var originalClips))
            {
                if (originalClips != null)
                {
                    targetList.AddRange(originalClips);
                }
            }

            // 목록에 클립이 있고, OverrideController가 존재할 때만 업데이트합니다.
            if (targetList.Count > 0 && _spum.OverrideController != null)
            {
                _spum.OverrideController[stateName] = targetList[0];
            }
        }
    }
}