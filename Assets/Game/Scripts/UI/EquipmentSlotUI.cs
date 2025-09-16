// Assets/Game/Scripts/UI/EquipmentSlotUI.cs (최종 수정본)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Items;
using Game.Runtime;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("슬롯 정보")]
    public EquipSlot slotType;
    public int CharacterIndex { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup _iconCanvasGroup;

    private InventoryPartyMode _controller;
    private ItemSO _currentItem;
    private bool _isDraggable = true;

    void Awake()
    {
        if (_iconCanvasGroup == null && iconImage != null)
        {
            _iconCanvasGroup = iconImage.GetComponent<CanvasGroup>();
            if (_iconCanvasGroup == null) _iconCanvasGroup = iconImage.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Setup(int characterIndex, ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        CharacterIndex = characterIndex;
        _controller = controller;
        _currentItem = item;
        _isDraggable = isDraggable;

        bool hasItem = item != null;
        if (iconImage)
        {
            iconImage.sprite = hasItem ? item.icon : null;
        }
        if (_iconCanvasGroup)
        {
            _iconCanvasGroup.alpha = hasItem ? 1f : 0f;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var itemIcon = eventData.pointerDrag.GetComponent<ItemIconUI>();
        if (itemIcon == null || itemIcon.ItemData == null) return;

        if (itemIcon.ItemData.slot == this.slotType)
        {
            _controller?.HandleEquipRequest(itemIcon.ItemData, this.CharacterIndex, this.slotType);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentItem == null || !_isDraggable) return;

        var iconGO = Instantiate(_controller.GetItemIconPrefab(), _controller.dragParent);
        var ghostIcon = iconGO.GetComponent<ItemIconUI>();
        ghostIcon.SetupAsGhost(_currentItem, _controller, this);
        eventData.pointerDrag = iconGO;

        // 드래그하는 동안 원래 아이콘은 반투명하게 만듭니다.
        if (_iconCanvasGroup) _iconCanvasGroup.alpha = 0.5f;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        // [✨✨✨ 핵심 수정 ✨✨✨]
        // 드래그가 끝났을 때, 일단 원래 아이콘의 투명도를 100%로 되돌립니다.
        // - 드롭에 성공했다면: 잠시 뒤 RunManager 이벤트가 이 슬롯을 비우고 투명도를 0으로 다시 설정할 것입니다.
        // - 드롭에 실패했다면: 이 코드가 실행되어 아이템이 원래 모습으로 돌아옵니다.
        if (_iconCanvasGroup) _iconCanvasGroup.alpha = 1f;

        // 이제 "빨리 드롭해야 하는 문제"가 해결되었는지 확인해보세요.
        // 만약 여전히 문제가 발생한다면, 이 아래 로그가 찍히는지 확인이 필요합니다.
        // Debug.Log($"OnEndDrag from {gameObject.name}. Drop Target: {(eventData.pointerEnter != null ? eventData.pointerEnter.name : "None")}");
    }
}