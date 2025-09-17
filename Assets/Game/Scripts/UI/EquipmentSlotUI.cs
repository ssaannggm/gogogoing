// Assets/Game/Scripts/UI/EquipmentSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Items;
using Game.Runtime;
using Game.UI;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("슬롯 정보")] public EquipSlot slotType;
    public int CharacterIndex;

    [Header("UI 참조")]
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup iconCg;

    [Header("고스트 프리팹(선택)")]
    [SerializeField] private GameObject _ghostPrefab;

    private InventoryPartyMode _controller;
    private ItemSO _currentItem;
    private RectTransform _ghost;
    private bool _isDraggable; // ★ 실제 사용

    void OnDisable()
    {
        if (_ghost) { Destroy(_ghost.gameObject); _ghost = null; }
        if (iconImage) iconImage.raycastTarget = true;
        if (DragContext.IsActive &&
            DragContext.Current.source == DragSourceType.Slot &&
            DragContext.Current.memberIndex == CharacterIndex &&
            DragContext.Current.slot == slotType)
            DragContext.Clear();
    }

    public void Setup(int characterIndex, ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        CharacterIndex = characterIndex;
        _controller = controller;
        _currentItem = item;
        _isDraggable = isDraggable; // ★ 저장

        bool has = item != null;
        if (iconImage)
        {
            iconImage.sprite = has ? item.icon : null;
            iconImage.raycastTarget = true; // 빈 슬롯이어도 드롭 수신 가능
        }
        if (iconCg) iconCg.alpha = has ? 1f : 0f;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (_controller == null || _controller.IsReadOnly) return; // ★ 읽기전용 차단
        if (!_isDraggable || _currentItem == null) return;         // ★ 드래그 게이트

        DragContext.StartFromSlot(CharacterIndex, slotType, _currentItem);

        var prefab = _ghostPrefab ? _ghostPrefab : _controller.GetItemIconPrefab();
        if (prefab)
        {
            var go = Instantiate(prefab, _controller.dragParent);
            _ghost = go.transform as RectTransform;

            var gCg = go.GetComponentInChildren<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            gCg.blocksRaycasts = false;

            var ghostImg = go.GetComponentInChildren<Image>();
            if (ghostImg && iconImage) ghostImg.sprite = iconImage.sprite;
            _ghost.position = e.position;
        }

        if (iconCg) iconCg.alpha = 0f;
        if (iconImage) iconImage.raycastTarget = false; // 자기 자신으로 드롭 방지
    }

    public void OnDrag(PointerEventData e)
    {
        if (_ghost) _ghost.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (_ghost) { Destroy(_ghost.gameObject); _ghost = null; }

        // 실패 시 복구
        if (DragContext.IsActive)
        {
            if (iconCg) iconCg.alpha = (_currentItem != null) ? 1f : 0f;
            if (iconImage) iconImage.raycastTarget = true;
            DragContext.Clear();
        }
        else
        {
            if (iconImage) iconImage.raycastTarget = true;
        }
    }

    public void OnDrop(PointerEventData e)
    {
        if (_controller == null || _controller.IsReadOnly) return; // ★ 읽기전용 차단
        if (!DragContext.IsActive) return;

        var payload = DragContext.Current;
        if (payload.item == null || payload.item.slot != slotType) return; // 슬롯 타입 검증

        _controller.ApplyDropToSlotSO(CharacterIndex, slotType);
    }
}
