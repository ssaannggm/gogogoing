// Assets/Game/Scripts/UI/EquipmentSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Items;
using Game.Runtime;
using Game.UI;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("슬롯 정보")]
    public EquipSlot slotType;
    public int CharacterIndex;

    [Header("UI 참조")]
    [SerializeField] private Image iconImage;      // 드롭 수신 Graphic
    [SerializeField] private CanvasGroup iconCg;

    [Header("고스트 프리팹(선택)")]
    [SerializeField] private GameObject _ghostPrefab;

    private InventoryPartyMode _controller;
    private ItemSO _currentItem;                   // ★ SO로 보관
    private RectTransform _ghost;

    void OnDisable()
    {
        if (_ghost) { Destroy(_ghost.gameObject); _ghost = null; }
        if (iconImage) iconImage.raycastTarget = true; // 항상 드롭 받게
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

        bool has = item != null;
        if (iconImage)
        {
            iconImage.sprite = has ? item.icon : null;
            iconImage.raycastTarget = true; // 아이템 유무와 무관하게 항상 켬
        }
        if (iconCg) iconCg.alpha = has ? 1f : 0f;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (_currentItem == null || _controller == null) return;

        // ★ SO 기반 페이로드
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

        // 처리 안 됨 → 원복
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
        if (_controller == null || !DragContext.IsActive) return;

        // ★ 슬롯 검증: 다른 타입이면 장착 불가
        var payload = DragContext.Current;
        if (payload.item == null || payload.item.slot != slotType) return;

        _controller.ApplyDropToSlotSO(CharacterIndex, slotType);
    }
}
