// Assets/Game/Scripts/UI/ItemIconUI.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;
using Game.Runtime;

[RequireComponent(typeof(CanvasGroup), typeof(Image))]
public class ItemIconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemSO ItemData { get; private set; }
    public InventoryPartyMode Controller { get; private set; }
    public bool IsGhost { get; private set; } = false;
    public EquipmentSlotUI SourceSlot { get; private set; }

    private Transform _originalParent;
    private CanvasGroup _canvasGroup;
    private bool _isDraggable = true;

    private Image _image;
    private ItemIconUI _activeGhost; // 현재 드래그용 고스트

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _image = GetComponent<Image>();
    }

    public void Setup(ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        ItemData = item;
        Controller = controller;
        _isDraggable = isDraggable;

        _image.sprite = item ? item.icon : null;
        SetVisible(_image.sprite != null);

        IsGhost = false;
        SourceSlot = null;
        _canvasGroup.blocksRaycasts = true; // 원본은 차단하지 않음
        _activeGhost = null;
    }

    public void SetupAsGhost(ItemSO item, InventoryPartyMode controller, EquipmentSlotUI sourceSlot)
    {
        Setup(item, controller, true);
        IsGhost = true;
        SourceSlot = sourceSlot;
        _canvasGroup.blocksRaycasts = false; // 고스트는 통과
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isDraggable || Controller == null || ItemData == null || _image.sprite == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        // 항상 고스트 생성하되, pointerDrag는 ‘원본(this)’로 유지 → 원본이 EndDrag를 반드시 받음
        var prefab = Controller.GetItemIconPrefab();
        if (!IsGhost)
        {
            if (prefab == null) { eventData.pointerDrag = null; return; }

            var ghostGO = Object.Instantiate(prefab, Controller.dragParent);
            _activeGhost = ghostGO.GetComponent<ItemIconUI>();
            _activeGhost.SetupAsGhost(ItemData, Controller, SourceSlot);
            _activeGhost.transform.position = eventData.position;

            _originalParent = transform.parent;
            SetVisible(false); // 원본 숨김 + raycastTarget 꺼짐
            // eventData.pointerDrag 는 그대로 this 유지
        }
        else
        {
            // 고스트는 직접 집지 않도록 막음
            eventData.pointerDrag = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_activeGhost != null && _activeGhost.IsGhost)
            _activeGhost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 고스트 정리
        if (_activeGhost != null)
        {
            Destroy(_activeGhost.gameObject);
            _activeGhost = null;
        }

        // ItemIconUI.OnEndDrag 마지막
        var success = Controller != null && Controller.ConsumeDropSuccess();
        if (!success)
        {
            Debug.Log("[INV] Drop failed or rejected → restoring original icon");
            transform.SetParent(_originalParent);
            SetVisible(true);
            Controller?.ForceRefreshUI();
        }


        _canvasGroup.blocksRaycasts = true;
    }

    private void SetVisible(bool on)
    {
        if (_image == null) return;
        var has = _image.sprite != null;
        var vis = on && has;
        _image.enabled = vis;
        _image.raycastTarget = vis;
    }
}
