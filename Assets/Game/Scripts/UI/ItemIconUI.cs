// Assets/Game/Scripts/UI/ItemIconUI.cs (최종 수정본)
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;
using Game.Runtime;

[RequireComponent(typeof(CanvasGroup))]
public class ItemIconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemSO ItemData { get; private set; }
    public InventoryPartyMode Controller { get; private set; }
    public bool IsGhost { get; private set; } = false;
    public EquipmentSlotUI SourceSlot { get; private set; }

    private Transform _originalParent;
    private CanvasGroup _canvasGroup;
    private bool _isDraggable = true;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Setup(ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        ItemData = item;
        Controller = controller;
        _isDraggable = isDraggable;
        GetComponent<Image>().sprite = item.icon;
        IsGhost = false;
        SourceSlot = null;
    }

    public void SetupAsGhost(ItemSO item, InventoryPartyMode controller, EquipmentSlotUI sourceSlot)
    {
        Setup(item, controller, true);
        IsGhost = true;
        SourceSlot = sourceSlot;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isDraggable || Controller == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        // [핵심] 드래그되는 동안 '유령'은 이벤트를 완벽하게 통과시켜야 합니다.
        _canvasGroup.blocksRaycasts = false;

        if (!IsGhost)
        {
            // 실제 인벤토리 아이콘일 경우, 드래그를 위해 최상위로 잠시 이동
            _originalParent = transform.parent;
            transform.SetParent(Controller.dragParent);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 유령 아이콘은 드롭 성공/실패와 관계없이 역할을 다했으므로 무조건 파괴합니다.
        if (IsGhost)
        {
            Destroy(gameObject);
            return;
        }

        // 실제 인벤토리 아이콘의 드래그가 끝났을 때
        _canvasGroup.blocksRaycasts = true;
        // 허공에 드롭되었다면 원래 부모로 돌아갑니다.
        if (eventData.pointerEnter == null)
        {
            transform.SetParent(_originalParent);
        }
        // 드롭에 성공했다면, RunManager 이벤트에 의해 UI가 새로고침되면서
        // 이 아이콘은 어차피 파괴될 것이므로 여기서 특별히 처리할 필요가 없습니다.
    }
}