// Assets/Game/Scripts/UI/EquipmentSlotUI.cs (Always-Ghost)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Items;
using Game.Runtime;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("슬롯 정보")]
    public EquipSlot slotType;
    public int CharacterIndex { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup _iconCanvasGroup; // 보조용

    private InventoryPartyMode _controller;
    private ItemSO _currentItem;
    private bool _isDraggable = true;

    void Awake()
    {
        if (_iconCanvasGroup == null && iconImage != null)
            _iconCanvasGroup = iconImage.GetComponent<CanvasGroup>() ?? iconImage.gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(int characterIndex, ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        CharacterIndex = characterIndex;
        _controller = controller;
        _currentItem = item;
        _isDraggable = isDraggable;

        var has = item != null;
        if (iconImage) iconImage.sprite = has ? item.icon : null;
        UiIconUtil.Show(iconImage, has);
        if (_iconCanvasGroup) _iconCanvasGroup.alpha = has ? 1f : 0f;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_controller == null || eventData == null || eventData.pointerDrag == null) return;

        var itemIcon = eventData.pointerDrag.GetComponent<ItemIconUI>();
        if (itemIcon == null || itemIcon.ItemData == null) return;

        if (itemIcon.ItemData.slot == this.slotType)
        {
            _controller.HandleEquipRequest(itemIcon.ItemData, this.CharacterIndex, this.slotType);
            _controller.NotifyDropSucceeded(); // 성공 통지
        }
    }

    // EquipmentSlotUI.OnBeginDrag
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_controller == null || _currentItem == null || !_isDraggable) return;
        var prefab = _controller.GetItemIconPrefab();
        if (prefab == null) return;

        var iconGO = Instantiate(prefab, _controller.dragParent);
        var ghostIcon = iconGO.GetComponent<ItemIconUI>();
        ghostIcon.SetupAsGhost(_currentItem, _controller, this);

        eventData.pointerDrag = iconGO; // ★ 반드시 고스트를 드래그 주체로
        UiIconUtil.Show(iconImage, false);
        if (_iconCanvasGroup) _iconCanvasGroup.alpha = 0f;

        Debug.Log($"[SLOT] BeginDrag set pointerDrag={iconGO.name} item={_currentItem.name}");
    }


    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 허공 드롭 등으로 데이터 변경이 없으면 UI 리프레시가 없으므로 직접 복구
        if (eventData == null || eventData.pointerEnter == null)
        {
            _controller?.ForceRefreshUI();
        }
    }
}

static class UiIconUtil
{
    public static void Show(Image img, bool on)
    {
        if (!img) return;
        var hasSprite = img.sprite != null;
        var visible = on && hasSprite;
        img.enabled = visible;
        img.raycastTarget = visible;
    }
}
