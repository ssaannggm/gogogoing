using System.Collections.Generic;
using UnityEngine;
using Game.Data;
using Game.Items; // ItemSO�� ����ϱ� ���� �߰�
using System.Linq;

public class DataCatalog : MonoBehaviour
{
    [Header("Game Data Catalogs")]
    [SerializeField] private List<UnitSO> _unitCatalog = new();
    [SerializeField] private List<ItemSO> _itemCatalog = new(); // [�߰�] ������ ������ ���
    [SerializeField] private List<EncounterSO> _encounterCatalog = new();
    [SerializeField] private List<MapNodeSO> _mapNodeCatalog = new();
    [SerializeField] private List<RecruitmentInfoSO> _recruitmentInfoCatalog = new();

    // ID�� ���� �����Ϳ� ������ �����ϱ� ���� Dictionary��
    private Dictionary<string, UnitSO> _unitLookup;
    private Dictionary<string, ItemSO> _itemLookup; // [�߰�] ������ ��ȸ�� ��ųʸ�
    private Dictionary<string, EncounterSO> _encounterLookup;
    private Dictionary<string, MapNodeSO> _mapNodeLookup;

    void Awake()
    {
        // Unit īŻ�α� �ʱ�ȭ
        _unitLookup = new Dictionary<string, UnitSO>();
        foreach (var unitSO in _unitCatalog)
        {
            if (unitSO != null && !string.IsNullOrEmpty(unitSO.unitId) && !_unitLookup.ContainsKey(unitSO.unitId))
            {
                _unitLookup[unitSO.unitId] = unitSO;
            }
        }

        // [�߰�] Item īŻ�α� �ʱ�ȭ
        _itemLookup = new Dictionary<string, ItemSO>();
        foreach (var itemSO in _itemCatalog)
        {
            if (itemSO != null && !string.IsNullOrEmpty(itemSO.itemId) && !_itemLookup.ContainsKey(itemSO.itemId))
            {
                _itemLookup[itemSO.itemId] = itemSO;
            }
        }

        // Encounter īŻ�α� �ʱ�ȭ
        _encounterLookup = new Dictionary<string, EncounterSO>();
        foreach (var e in _encounterCatalog)
        {
            if (e && !string.IsNullOrEmpty(e.encounterId))
                _encounterLookup[e.encounterId] = e;
        }

        // Map Node īŻ�α� �ʱ�ȭ
        _mapNodeLookup = new Dictionary<string, MapNodeSO>();
        foreach (var n in _mapNodeCatalog)
        {
            if (n && !string.IsNullOrEmpty(n.nodeId))
                _mapNodeLookup[n.nodeId] = n;
        }

        Debug.Log("[DataCatalog] Initialized.");
    }

    // --- ������ ��ȸ �Լ��� ---

    public UnitSO GetUnitById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _unitLookup.TryGetValue(id, out var unitSO);
        return unitSO;
    }

    // [�߰�] ID�� ItemSO�� ã�� ��ȯ�ϴ� �Լ�
    public ItemSO GetItemById(string id)
    {
        if (string.IsNullOrEmpty(id) || _itemLookup == null) return null;
        _itemLookup.TryGetValue(id, out var itemSO);
        return itemSO;
    }

    public EncounterSO GetEncounterById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _encounterLookup.TryGetValue(id, out var so);
        return so;
    }

    public MapNodeSO GetNodeById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _mapNodeLookup.TryGetValue(id, out var so);
        return so;
    }

    // --- ��ü ����� ��ȯ�ϴ� �Լ��� ---

    public IReadOnlyList<UnitSO> GetAllUnits() => _unitCatalog;

    // [�߰�] �κ��丮 UI ��� ��ü ������ ����� ������ �� ���
    public IReadOnlyList<ItemSO> GetAllItems() => _itemCatalog;

    public IReadOnlyList<RecruitmentInfoSO> GetAllRecruitmentInfos() => _recruitmentInfoCatalog;
}