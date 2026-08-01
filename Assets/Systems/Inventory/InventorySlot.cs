using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public Image image;
    public Color selectedColor, notSelectedColor;

    [HideInInspector] public Transform parentAfterDrag;

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


    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }


}
