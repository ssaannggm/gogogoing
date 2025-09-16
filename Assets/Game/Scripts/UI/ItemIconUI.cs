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
    private Transform _dragParent;
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
        _dragParent = controller.dragParent;
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

        // 드래그되는 동안 '유령' 아이콘이 이벤트를 막지 않도록 설정합니다.
        _canvasGroup.blocksRaycasts = false;

        if (!IsGhost)
        {
            _originalParent = transform.parent;
            transform.SetParent(_dragParent);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggable) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝나면 다시 이벤트를 받을 수 있도록 복원합니다.
        _canvasGroup.blocksRaycasts = true;

        if (IsGhost)
        {
            Destroy(gameObject);
            return;
        }

        if (transform.parent == _dragParent)
        {
            transform.SetParent(_originalParent);
        }
    }
}