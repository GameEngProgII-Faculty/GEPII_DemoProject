using UnityEngine;

[RequireComponent(typeof(Outline))]
public class ResourceNode : BaseInteractable
{
    [SerializeField] private Item item;
    [SerializeField] private Item requiredTool;
    [SerializeField] private int HitPoints = 3;

    public override string GetInteractionPrompt()
    {
        if (!HasRequiredTool())
        {
            return $"Requires {requiredTool.name}";
        }

        return $"[LMB] Harvest {item.name}";
    }

    private bool HasRequiredTool()
    {
        return requiredTool == null || InventoryManager.Instance.GetSelectedItem() == requiredTool;
    }

    public override void OnInteract()
    {
        // Check if the active toolbar slot has the correct tool
        if (!HasRequiredTool())
        {
            Debug.Log($"You need a {requiredTool.name} to harvest this.");
            return;
        }

        // Play Attack animation
        // Play Attack sound
        // Subtract hit points from the resource node (based on tool's damage value)
        HitPoints--;  // placeholder

        // check if hit points are zero or less, if so, add the item to the player's inventory and destroy the resource node
        if (HitPoints <= 0)
        {
            int added = InventoryManager.Instance.AddItemsToInventory(item, 1);
            if (added > 0)
            {
                Debug.Log($"Harvested {item.name} from resource node");
                // Play Destruction VFX
                // Play Destruction SFX
                Destroy(gameObject);
            }
            else // If inventory is full, Item(s) will spawn on ground.
            {
                Debug.Log($"Inventory full, can't pick up {item.name}");
            }
        }
    }
}
