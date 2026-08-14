using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{



    [Header("UI")]
    public Image image;
    public TextMeshProUGUI countText;

    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;

    public UIManager uIManager;

    void Start()
    {
       uIManager = UIManager.Instance;
    }

    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.icon;
        RefreshCount();
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();

        // Hide the count text if the count is 1 or less
        bool isCountVisible = count > 1;
        countText.gameObject.SetActive(isCountVisible);

    }


    // Tooltip on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        uIManager.ShowItemTooltip(item, (RectTransform)transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uIManager.HideItemTooltip();
    }


    // Registers this item as an active Unity drag so InventorySlot.OnDrop fires on release.
    // Pickup itself already happened via InventorySlot.OnPointerDown; InventoryManager.Update()
    // already follows the cursor, so these don't need to do anything themselves.
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData) { }

}
