// Assets/Game/Scripts/UI/ItemIconUI.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Items;
using Game.Runtime;
using Game.UI;

[RequireComponent(typeof(CanvasGroup), typeof(Image))]
public class ItemIconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemSO ItemData { get; private set; }
    public InventoryPartyMode Controller { get; private set; }

    [SerializeField] private GameObject _ghostPrefab;   // None이면 Controller.GetItemIconPrefab 사용
    private RectTransform _ghost;
    private CanvasGroup _cg;
    private Image _img;
    private bool _isDraggable;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _img = GetComponent<Image>();
    }

    void OnDisable()
    {
        if (_ghost) { Destroy(_ghost.gameObject); _ghost = null; }
        if (_img)
        {
            bool on = (_img.sprite != null);
            _img.enabled = on;
            _img.raycastTarget = on;
        }
        if (_cg) _cg.blocksRaycasts = true;

        if (DragContext.IsActive && DragContext.Current.source == DragSourceType.Inventory && ItemData != null)
            DragContext.Clear();
    }

    public void Setup(ItemSO item, InventoryPartyMode controller, bool isDraggable)
    {
        ItemData = item;
        Controller = controller;
        _isDraggable = isDraggable;

        _img.sprite = item ? item.icon : null;
        bool on = _img.sprite != null;
        _img.enabled = on;
        _img.raycastTarget = on;
        _cg.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!_isDraggable || ItemData == null || Controller == null || _img.sprite == null) return;
        if (Controller.IsReadOnly) return; // ★ 읽기전용 차단

        // ★ SO 기반 페이로드
        DragContext.StartFromInventory(ItemData);

        // 고스트 생성 (필요 시)
        var prefab = _ghostPrefab ? _ghostPrefab : Controller.GetItemIconPrefab();
        if (prefab)
        {
            var go = Instantiate(prefab, Controller.dragParent);
            _ghost = go.transform as RectTransform;

            var gCg = go.GetComponentInChildren<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            gCg.blocksRaycasts = false;
            gCg.alpha = 1f;

            var ghostImg = go.GetComponentInChildren<Image>(true);
            if (ghostImg)
            {
                ghostImg.sprite = _img.sprite;
                ghostImg.enabled = true;
                var c = ghostImg.color; c.a = 1f; ghostImg.color = c;
                ghostImg.raycastTarget = false;
                ghostImg.maskable = false;
            }

            _ghost.SetAsLastSibling();
            _ghost.localScale = Vector3.one;
            _ghost.position = e.position;

            var ghostCanvas = go.GetComponent<Canvas>();
            if (ghostCanvas) { ghostCanvas.overrideSorting = true; ghostCanvas.sortingOrder = 999; }
        }

        // 원본 비활성화(시각/입력)
        _img.enabled = false;
        _img.raycastTarget = false;
        _cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (_ghost) _ghost.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (_ghost) { Destroy(_ghost.gameObject); _ghost = null; }

        if (DragContext.IsActive)
        {
            // 실패 → 원복
            if (_img.sprite != null)
            {
                _img.enabled = true;
                _img.raycastTarget = true;
            }
            _cg.blocksRaycasts = true;
            DragContext.Clear();
        }
        else
        {
            // 성공 → 리빌드에 맡김
            _cg.blocksRaycasts = true;
        }
    }
}
