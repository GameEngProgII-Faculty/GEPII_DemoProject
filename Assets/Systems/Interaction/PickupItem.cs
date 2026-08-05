using UnityEngine;

// Lets the player interact with a world-spawned item (e.g. one dropped from the inventory)
// to pick it back up. Attach to an Item's "mesh" prefab alongside a Collider + Outline.
[RequireComponent(typeof(Outline))]
public class PickupItem : BaseInteractable
{
    [SerializeField] private Item item;
    [SerializeField] private int count = 1;

    public void SetItem(Item newItem, int newCount = 1)
    {
        item = newItem;
        count = Mathf.Max(1, newCount);
    }

    public override string GetInteractionPrompt()
    {
        if (item == null) return base.GetInteractionPrompt();

        return count > 1 ? $"[E] {item.name} x{count}" : $"[E] {item.name}";
    }

    public override void OnInteract()
    {
        if (item == null) return;

        SetFocus(false); // Remove focus to prevent multiple pickups in one frame


        int added = InventoryManager.Instance.AddItemsToInventory(item, count);
        count -= added;

        if (count <= 0)
        {
            Destroy(gameObject);
        }
        else if (added > 0)
        {
            Debug.Log($"Inventory full: picked up {added}, {count} of {item.name} left on the ground");
        }
        else
        {
            Debug.Log($"Inventory full, can't pick up {item.name}");
        }
    }
}
