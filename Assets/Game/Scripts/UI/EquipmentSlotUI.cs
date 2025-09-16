using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Items;
using TMPro;
using Game.Runtime;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("슬롯 정보")]
    public EquipSlot slotType;
    public int CharacterIndex { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;

    private InventoryPartyMode _controller;
    private ItemSO _currentItem;

    void Awake()
    {
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
    }

    public void Setup(int characterIndex, ItemSO item, InventoryPartyMode controller)
    {
        CharacterIndex = characterIndex;
        _controller = controller;
        SetItem(item);
        if (backgroundImage != null) backgroundImage.raycastTarget = true;
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
        if (nameText) nameText.text = hasItem ? item.displayName : "";
    }

    // --- 드롭: 인벤토리의 아이템을 이 슬롯에 놓을 때 ---
    public void OnDrop(PointerEventData eventData)
    {
        var itemIcon = eventData.pointerDrag.GetComponent<ItemIconUI>();
        if (itemIcon != null)
        {
            _controller?.OnItemDroppedOnEquipmentSlot(itemIcon, this);
        }
    }

    // --- 드래그: 장착된 아이템을 끌어낼 때 ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentItem == null) return;

        // 임시 '유령' 아이콘을 만들어 마우스를 따라다니게 함
        var iconGO = Instantiate(_controller.GetItemIconPrefab(), _controller.dragParent);
        var itemIcon = iconGO.GetComponent<ItemIconUI>();
        itemIcon.Setup(_currentItem, _controller);

        eventData.pointerDrag = iconGO; // 이제부터 드래그되는 대상은 이 '유령' 아이콘임
    }

    // 이 두 함수는 인터페이스 때문에 있어야 하지만, 실제 로직은 ItemIconUI가 처리
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝나면 모든 것은 지휘자가 알아서 처리하므로 여기서는 아무것도 하지 않습니다.
    }
}