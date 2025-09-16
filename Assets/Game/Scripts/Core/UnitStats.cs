// UnitStats.cs - 전체 코드
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Combat;
using Game.Data;      // StatSet, UnitSO 사용
using Game.Items;      // StatMods, ItemSO 사용
using Game.Services;   // GameManager 사용

public class UnitStats : MonoBehaviour
{
    [Header("ID")]
    public Team team;

    // [수정] 기본 스탯과 아이템 스탯을 합친 '장비 스탯'
    private StatSet _baseStatsFromData;

    // [수정] 버프/디버프 등 실시간으로 적용되는 StatMods 리스트
    private readonly List<StatMods> _runtimeModifiers = new List<StatMods>();

    [Header("Runtime Calculated Stats (Debug Mode)")]
    // [수정] 최종 계산된 스탯을 담는 StatSet
    public StatSet CurrentStats { get; private set; }


    /// <summary>
    /// [핵심 추가] 전투 시작 시 호출되는 초기화 함수
    /// </summary>
    public void Initialize(UnitSO unitSO, PartyMemberState memberState)
    {
        // 1. UnitSO에서 기본 스탯을 가져옴
        _baseStatsFromData = unitSO.baseStats;
        var dataCatalog = GameManager.I.Data;

        // 2. memberState에 있는 모든 장착 아이템의 스탯을 합산
        StatMods totalItemMods = new StatMods();
        foreach (var itemEntry in memberState.equippedItemIds)
        {
            var itemSO = dataCatalog.GetItemById(itemEntry.Value);
            if (itemSO != null)
            {
                totalItemMods = totalItemMods.Add(itemSO.statMods);
            }
        }

        // 3. 기본 스탯에 아이템 스탯을 적용하여 이 유닛의 '순수 스탯'으로 저장
        _baseStatsFromData = _baseStatsFromData.ApplyMods(totalItemMods);

        // 4. 모든 스탯을 최종 재계산
        RecalculateStats();
    }

    /// <summary>
    /// 버프 등 실시간 StatMods를 추가합니다.
    /// </summary>
    public void AddRuntimeModifier(StatMods mods)
    {
        _runtimeModifiers.Add(mods);
        RecalculateStats(); // 스탯 변경이 있으므로 재계산
    }

    /// <summary>
    /// 적용되었던 StatMods를 제거합니다. (버프 종료 시)
    /// </summary>
    public void RemoveRuntimeModifier(StatMods mods)
    {
        _runtimeModifiers.Remove(mods);
        RecalculateStats();
    }

    /// <summary>
    /// 모든 실시간 모디파이어를 제거합니다.
    /// </summary>
    public void ClearAllRuntimeModifiers()
    {
        _runtimeModifiers.Clear();
        RecalculateStats();
    }

    /// <summary>
    /// 최종 스탯을 다시 계산합니다.
    /// </summary>
    public void RecalculateStats()
    {
        // 1. 기본+아이템 스탯으로 시작
        StatSet statsWithModifiers = _baseStatsFromData;

        // 2. 모든 실시간 모디파이어(버프/디버프)를 합산
        StatMods totalRuntimeMods = new StatMods();
        foreach (var mod in _runtimeModifiers)
        {
            totalRuntimeMods = totalRuntimeMods.Add(mod);
        }

        // 3. 최종 스탯 계산
        CurrentStats = statsWithModifiers.ApplyMods(totalRuntimeMods);
    }
}