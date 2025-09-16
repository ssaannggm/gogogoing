using System.Collections.Generic;
using UnityEngine;
using Game.Data;
using Game.Items; // ItemSO를 사용하기 위해 추가
using System.Linq;

public class DataCatalog : MonoBehaviour
{
    [Header("Game Data Catalogs")]
    [SerializeField] private List<UnitSO> _unitCatalog = new();
    [SerializeField] private List<ItemSO> _itemCatalog = new(); // [추가] 아이템 마스터 목록
    [SerializeField] private List<EncounterSO> _encounterCatalog = new();
    [SerializeField] private List<MapNodeSO> _mapNodeCatalog = new();
    [SerializeField] private List<RecruitmentInfoSO> _recruitmentInfoCatalog = new();

    // ID를 통해 데이터에 빠르게 접근하기 위한 Dictionary들
    private Dictionary<string, UnitSO> _unitLookup;
    private Dictionary<string, ItemSO> _itemLookup; // [추가] 아이템 조회용 딕셔너리
    private Dictionary<string, EncounterSO> _encounterLookup;
    private Dictionary<string, MapNodeSO> _mapNodeLookup;

    void Awake()
    {
        // Unit 카탈로그 초기화
        _unitLookup = new Dictionary<string, UnitSO>();
        foreach (var unitSO in _unitCatalog)
        {
            if (unitSO != null && !string.IsNullOrEmpty(unitSO.unitId) && !_unitLookup.ContainsKey(unitSO.unitId))
            {
                _unitLookup[unitSO.unitId] = unitSO;
            }
        }

        // [추가] Item 카탈로그 초기화
        _itemLookup = new Dictionary<string, ItemSO>();
        foreach (var itemSO in _itemCatalog)
        {
            if (itemSO != null && !string.IsNullOrEmpty(itemSO.itemId) && !_itemLookup.ContainsKey(itemSO.itemId))
            {
                _itemLookup[itemSO.itemId] = itemSO;
            }
        }

        // Encounter 카탈로그 초기화
        _encounterLookup = new Dictionary<string, EncounterSO>();
        foreach (var e in _encounterCatalog)
        {
            if (e && !string.IsNullOrEmpty(e.encounterId))
                _encounterLookup[e.encounterId] = e;
        }

        // Map Node 카탈로그 초기화
        _mapNodeLookup = new Dictionary<string, MapNodeSO>();
        foreach (var n in _mapNodeCatalog)
        {
            if (n && !string.IsNullOrEmpty(n.nodeId))
                _mapNodeLookup[n.nodeId] = n;
        }

        Debug.Log("[DataCatalog] Initialized.");
    }

    // --- 데이터 조회 함수들 ---

    public UnitSO GetUnitById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _unitLookup.TryGetValue(id, out var unitSO);
        return unitSO;
    }

    // [추가] ID로 ItemSO를 찾아 반환하는 함수
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

    // --- 전체 목록을 반환하는 함수들 ---

    public IReadOnlyList<UnitSO> GetAllUnits() => _unitCatalog;

    // [추가] 인벤토리 UI 등에서 전체 아이템 목록을 보여줄 때 사용
    public IReadOnlyList<ItemSO> GetAllItems() => _itemCatalog;

    public IReadOnlyList<RecruitmentInfoSO> GetAllRecruitmentInfos() => _recruitmentInfoCatalog;
}