using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerDownHandler, IDropHandler
{
    [Header("UI")]
    public Image image;
    public Color selectedColor, notSelectedColor;

    public void Awake()
    {
        DeSelect();
    }

    public void Select()
    {
        image.color = selectedColor;
    }

    public void DeSelect()
    {
        image.color = notSelectedColor;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        InventoryManager.Instance.SlotClicked(this);
    }

    // Fires on release when a drag (press + move past the threshold) ends over this slot -
    // same action as clicking it, so press-drag-release and click-then-click both place/swap the same way.
    public void OnDrop(PointerEventData eventData)
    {
        InventoryManager.Instance.SlotClicked(this);
    }


}
