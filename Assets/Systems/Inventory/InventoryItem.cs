using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IbeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item item;

    public Image icon;
    public GameObject mesh;

    [HideInInspector]public Transform parentAfterDrag;

    public void InitializeItem(Item newItem)
    { 
        icon.sprite = newItem.icon;
        mesh = newItem.mesh;
    }

    // Drag and Drop functionality
    public void OnBeginDrag(PointerEventData eventData)
    {
        icon.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        icon.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }



}
