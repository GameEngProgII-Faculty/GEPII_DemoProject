using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Outline))]
public class ResourceNode : BaseInteractable
{
    [SerializeField] private Item item;
    [SerializeField] private Item requiredTool;
    [SerializeField] private int HitPoints = 3;

    AudioManager audioManager => AudioManager.Instance;

    [SerializeField] AudioClip[] audioClips;

    [SerializeField] GameObject harvestVFXPrefab;

    [SerializeField] int minAmount = 1;
    [SerializeField] int maxAmount = 3;

    // TODO:
    // - Play a different VFX when the resource node runs out of HP
    // - Play a different SFX when the resource node runs out of HP
    // - Match position of harvestVFXPrefab to cursor position on object (likely through a raycast)


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

        // TODO Play SWING AXE animation
        PlaySFX();


        // Play Particle system
        // TODO: Eventually change VFX spawnPosition to match the strike position (likely using a raycast)
        GameObject vfx = Instantiate(harvestVFXPrefab, transform);
        vfx.transform.localPosition = Vector3.zero;
        vfx.transform.localRotation = Quaternion.identity;   
     

        // Subtract hit points from the resource node (based on tool's damage value)
        HitPoints--;  // placeholder

        // check if hit points are zero or less, if so, add the item to the player's inventory and destroy the resource node
        if (HitPoints <= 0)
        {
            int amountToAdd = Random.Range(minAmount, maxAmount);


            int added = InventoryManager.Instance.AddItemsToInventory(item, amountToAdd);
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

    private void PlaySFX()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("No sound effects assigned to the array!");
            return;
        }

        // 1. Pick a random clip
        int randomIndex = Random.Range(0, audioClips.Length);
        AudioClip clipToPlay = audioClips[randomIndex];


        // 2. Play the sound effect
        audioManager.PlayTempSFX(clipToPlay);
    }

}
