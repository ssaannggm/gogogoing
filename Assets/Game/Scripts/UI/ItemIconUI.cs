using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;
using Game.Runtime;

[RequireComponent(typeof(CanvasGroup))]
public class ItemIconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemSO ItemData { get; private set; }
    private Transform _originalParent;
    private Transform _dragParent;
    private CanvasGroup _canvasGroup;
    private Vector3 _startPosition;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Setup(ItemSO item, InventoryPartyMode controller)
    {
        ItemData = item;
        _dragParent = controller.dragParent;
        GetComponent<Image>().sprite = item.icon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        _startPosition = transform.position;
        transform.SetParent(_dragParent);
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드롭이 성공했든 실패했든, 이 '유령' 아이콘은 항상 파괴됩니다.
        // 실제 UI 갱신은 InventoryPartyMode가 책임집니다.
        if (eventData.pointerEnter == null)
        {
            // 허공에 드롭했을 경우, 원래 위치로 되돌아가는 것처럼 보이게 한 후 파괴
            transform.SetParent(_originalParent);
            transform.position = _startPosition;
        }
        _canvasGroup.blocksRaycasts = true;

        // 드롭 성공 여부와 관계없이 유령은 역할을 다했으므로 파괴
        Destroy(gameObject);
    }
}