// EquipmentSlotUI.cs - 전체 코드
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
    [SerializeField] private Image backgroundImage;

    private CanvasGroup _canvasGroup; // 드래그 시 아이콘 숨기기용
    private InventoryPartyMode _controller;
    private ItemSO _currentItem;
    private bool _isDraggable = true;

    void Awake()
    {
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        _canvasGroup = GetComponentInChildren<CanvasGroup>(); // 아이콘을 감싸는 CanvasGroup 필요
        if (_canvasGroup == null && iconImage != null)
        {
            _canvasGroup = iconImage.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Setup(int characterIndex, ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        CharacterIndex = characterIndex;
        _controller = controller;
        _isDraggable = isDraggable;
        SetItem(item);
    }

    public void SetItem(ItemSO item)
    {
        _currentItem = item;
        bool hasItem = item != null;
        if (iconImage)
        {
            iconImage.sprite = hasItem ? item.icon : null;
            iconImage.color = hasItem ? Color.white : Color.clear;
        }
    }

    // --- 드롭: 다른 아이템을 이 슬롯에 놓을 때 ---
    public void OnDrop(PointerEventData eventData)
    {
        var itemIcon = eventData.pointerDrag.GetComponent<ItemIconUI>();
        if (itemIcon == null || itemIcon.ItemData == null) return;

        // 슬롯 타입이 맞는 아이템만 장착 가능
        if (itemIcon.ItemData.slot == this.slotType)
        {
            _controller?.HandleEquipRequest(itemIcon.ItemData, this.CharacterIndex, this.slotType);
        }
    }

    // --- 드래그: 장착된 아이템을 끌기 시작할 때 ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentItem == null || !_isDraggable) return;

        // 1. 임시 '유령' 아이콘 생성
        var iconGO = Instantiate(_controller.GetItemIconPrefab(), _controller.dragParent);
        var ghostIcon = iconGO.GetComponent<ItemIconUI>();

        // 2. [핵심] 유령 아이콘에 원본 출처 정보를 저장
        ghostIcon.SetupAsGhost(_currentItem, _controller, this);

        eventData.pointerDrag = iconGO; // 드래그 대상을 유령 아이콘으로 지정

        if (_canvasGroup) _canvasGroup.alpha = 0.5f; // 원래 슬롯 아이콘은 반투명하게
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝나면 원래 슬롯 아이콘을 다시 보이게 함
        // 실제 데이터 처리는 OnDrop에서 이루어지고, UI 갱신은 RunManager의 이벤트가 처리하므로
        // 여기서는 시각적인 부분만 원래대로 돌려놓으면 됩니다.
        if (_canvasGroup) _canvasGroup.alpha = 1f;

        // 유령 아이콘은 ItemIconUI의 OnEndDrag에서 스스로 파괴됨
    }
}