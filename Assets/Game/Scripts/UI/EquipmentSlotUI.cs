// Assets/Game/Scripts/UI/EquipmentSlotUI.cs (최종 수정본)
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
    [SerializeField] private CanvasGroup _iconCanvasGroup;

    private InventoryPartyMode _controller;
    private ItemSO _currentItem;
    private bool _isDraggable = true;

    void Awake()
    {
        if (_iconCanvasGroup == null && iconImage != null)
        {
            _iconCanvasGroup = iconImage.GetComponent<CanvasGroup>() ?? iconImage.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Setup(int characterIndex, ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        CharacterIndex = characterIndex;
        _controller = controller;
        _currentItem = item;
        _isDraggable = isDraggable;

        bool hasItem = item != null;
        if (iconImage) iconImage.sprite = hasItem ? item.icon : null;
        if (_iconCanvasGroup) _iconCanvasGroup.alpha = hasItem ? 1f : 0f;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var itemIcon = eventData.pointerDrag.GetComponent<ItemIconUI>();
        if (itemIcon == null || itemIcon.ItemData == null) return;

        // 슬롯 타입이 맞는지 확인하고 컨트롤러에 장착 요청
        if (itemIcon.ItemData.slot == this.slotType)
        {
            _controller?.HandleEquipRequest(itemIcon.ItemData, this.CharacterIndex, this.slotType);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentItem == null || !_isDraggable) return;

        // 1. 유령 아이콘 생성
        var iconGO = Instantiate(_controller.GetItemIconPrefab(), _controller.dragParent);
        var ghostIcon = iconGO.GetComponent<ItemIconUI>();
        ghostIcon.SetupAsGhost(_currentItem, _controller, this);
        eventData.pointerDrag = iconGO;

        // 2. 원래 아이콘은 "집었다"는 표시로 완전히 투명하게 만듭니다.
        if (_iconCanvasGroup) _iconCanvasGroup.alpha = 0f;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝났을 때 이 슬롯이 직접 자신의 모습을 바꾸지 않습니다.
        // 모든 시각적 복원은 RunManager의 데이터 변경 이벤트가 발생시킨
        // RefreshAllUI -> Setup() 호출을 통해 이루어집니다.
        // 만약 드롭이 실패하여 데이터 변경이 없다면, 이벤트도 없으므로
        // 이 슬롯의 모습은 바뀌지 않고 계속 투명한 상태로 남아있게 됩니다.
        // 이를 해결하기 위해, 드롭이 실패했을 가능성을 대비하여 컨트롤러에 "새로고침"을 요청합니다.

        // 유령 아이콘은 스스로 파괴됩니다.
        // 만약 드롭이 유효한 대상 위에서 끝나지 않았다면(허공), UI를 강제로 한번 새로고침합니다.
        if (eventData.pointerEnter == null)
        {
            _controller.ForceRefreshUI();
        }
    }
}