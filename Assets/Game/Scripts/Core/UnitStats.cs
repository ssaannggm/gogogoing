// UnitStats.cs - ��ü �ڵ�
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Combat;
using Game.Data;      // StatSet, UnitSO ���
using Game.Items;      // StatMods, ItemSO ���
using Game.Services;   // GameManager ���

public class UnitStats : MonoBehaviour
{
    [Header("ID")]
    public Team team;

    // [����] �⺻ ���Ȱ� ������ ������ ��ģ '��� ����'
    private StatSet _baseStatsFromData;

    // [����] ����/����� �� �ǽð����� ����Ǵ� StatMods ����Ʈ
    private readonly List<StatMods> _runtimeModifiers = new List<StatMods>();

    [Header("Runtime Calculated Stats (Debug Mode)")]
    // [����] ���� ���� ������ ��� StatSet
    public StatSet CurrentStats { get; private set; }


    /// <summary>
    /// [�ٽ� �߰�] ���� ���� �� ȣ��Ǵ� �ʱ�ȭ �Լ�
    /// </summary>
    public void Initialize(UnitSO unitSO, PartyMemberState memberState)
    {
        // 1. UnitSO���� �⺻ ������ ������
        _baseStatsFromData = unitSO.baseStats;
        var dataCatalog = GameManager.I.Data;

        // 2. memberState�� �ִ� ��� ���� �������� ������ �ջ�
        StatMods totalItemMods = new StatMods();
        foreach (var itemEntry in memberState.equippedItemIds)
        {
            var itemSO = dataCatalog.GetItemById(itemEntry.Value);
            if (itemSO != null)
            {
                totalItemMods = totalItemMods.Add(itemSO.statMods);
            }
        }

        // 3. �⺻ ���ȿ� ������ ������ �����Ͽ� �� ������ '���� ����'���� ����
        _baseStatsFromData = _baseStatsFromData.ApplyMods(totalItemMods);

        // 4. ��� ������ ���� ����
        RecalculateStats();
    }

    /// <summary>
    /// ���� �� �ǽð� StatMods�� �߰��մϴ�.
    /// </summary>
    public void AddRuntimeModifier(StatMods mods)
    {
        _runtimeModifiers.Add(mods);
        RecalculateStats(); // ���� ������ �����Ƿ� ����
    }

    /// <summary>
    /// ����Ǿ��� StatMods�� �����մϴ�. (���� ���� ��)
    /// </summary>
    public void RemoveRuntimeModifier(StatMods mods)
    {
        _runtimeModifiers.Remove(mods);
        RecalculateStats();
    }

    /// <summary>
    /// ��� �ǽð� ������̾ �����մϴ�.
    /// </summary>
    public void ClearAllRuntimeModifiers()
    {
        _runtimeModifiers.Clear();
        RecalculateStats();
    }

    /// <summary>
    /// ���� ������ �ٽ� ����մϴ�.
    /// </summary>
    public void RecalculateStats()
    {
        // 1. �⺻+������ �������� ����
        StatSet statsWithModifiers = _baseStatsFromData;

        // 2. ��� �ǽð� ������̾�(����/�����)�� �ջ�
        StatMods totalRuntimeMods = new StatMods();
        foreach (var mod in _runtimeModifiers)
        {
            totalRuntimeMods = totalRuntimeMods.Add(mod);
        }

        // 3. ���� ���� ���
        CurrentStats = statsWithModifiers.ApplyMods(totalRuntimeMods);
    }
}