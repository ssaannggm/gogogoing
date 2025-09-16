// ItemIconUI.cs - 전체 코드
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;
using Game.Runtime;

[RequireComponent(typeof(CanvasGroup))]
public class ItemIconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemSO ItemData { get; private set; }

    // [추가] 이 아이콘이 어디서 왔는지에 대한 정보
    public bool IsGhost { get; private set; } = false;
    public EquipmentSlotUI SourceSlot { get; private set; } // 장비 슬롯에서 왔다면 원본 슬롯 참조

    private Transform _originalParent;
    private Transform _dragParent;
    private CanvasGroup _canvasGroup;
    private InventoryPartyMode _controller;
    private bool _isDraggable = true;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    // 인벤토리 아이템을 위한 설정 함수
    public void Setup(ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        ItemData = item;
        _controller = controller;
        _dragParent = controller.dragParent;
        _isDraggable = isDraggable;
        GetComponent<Image>().sprite = item.icon;

        IsGhost = false;
        SourceSlot = null;
    }

    // 장비슬롯에서 드래그 시작 시 '유령'으로 설정하는 함수
    public void SetupAsGhost(ItemSO item, InventoryPartyMode controller, EquipmentSlotUI sourceSlot)
    {
        Setup(item, controller, true);
        IsGhost = true;
        SourceSlot = sourceSlot;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isDraggable || _controller == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        // 유령이 아닌 실제 인벤토리 아이콘일 때만
        if (!IsGhost)
        {
            _originalParent = transform.parent;
            transform.SetParent(_dragParent);
        }

        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭이 어디에 되었든, 유령 아이콘은 항상 파괴되어야 함
        if (IsGhost)
        {
            Destroy(gameObject);
            return;
        }

        // 유령이 아닌 실제 아이콘의 드래그가 끝났을 때
        _canvasGroup.blocksRaycasts = true;

        // 드롭이 성공적으로 어떤 슬롯 위에서 이루어졌다면, 아이콘은 파괴되거나 이동될 것임.
        // 하지만 허공에 드롭되었다면(eventData.pointerEnter == null), 원래 자리로 돌아가야 함.
        // 그러나 우리는 이벤트 기반으로 전체 UI를 새로고침하므로, 
        // 여기서 아이콘을 되돌리는 복잡한 로직 대신 그냥 파괴하고 새로 그려도 무방함.
        // 지금 구조에서는 데이터 변경이 없었으면 UI가 새로고침되지 않으므로, 원래 부모로 돌려놓는다.
        if (transform.parent == _dragParent)
        {
            transform.SetParent(_originalParent);
            // 위치도 원래대로 돌려놓는 로직이 필요할 수 있지만, Layout Group이 잡아줄 것임.
        }
    }
}