using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    // Singleton instance of GameManager for global access
    public static InventoryManager Instance { get; private set; }

    public int itemStackLimit = 5; // Maximum number of items that can be stacked in one slot
    public InventorySlot[] toolbarSlots;
    public InventorySlot[] backpackSlots;
    public GameObject inventoryItemPrefab;

    // Fired whenever the selected toolbar slot changes (number key, scroll, or click), carrying the
    // new slot index. Anything that cares what tool/item is currently equipped can subscribe instead
    // of InventoryManager needing to know about it directly.
    public event Action<int> OnSelectedSlotChanged;

    [Header("Drop Offset")]
    [SerializeField] private float dropDistance = 0.5f;
    [SerializeField] private float dropHeight = 1.5f;

    int selectedSlot = -1; // Index of the currently selected inventory slot

    private InventoryItem heldItem; // The real item currently picked up and following the cursor
    private InventorySlot heldItemOriginSlot; // Slot the held item is currently "from" (shows its ghost)
    private GameObject heldItemGhost; // Faded placeholder left behind in heldItemOriginSlot


    private void Awake()
    {
        #region Singleton Pattern Setup

        // Enforce a unique instance: if one already exists, self-destruct.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Establish this instance as the global instance and persist across scene loads.
        Instance = this;

        #endregion
    }

    private void Start()
    {
        UIManager.Instance.OnToolbarPanelShown += OnToolbarShown;
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnToolbarPanelShown -= OnToolbarShown;
        }
    }

    private void Update()
    {
        // Held item follows the cursor until it's placed in a slot
        if (heldItem != null)
        {
            heldItem.transform.position = Input.mousePosition;
        }
    }


    // Called whenever the toolbar panel is shown (gameplay HUD or full inventory screen).
    // InventoryManager is the sole writer of slot color state, so every slot is explicitly
    // deselected here before selecting - no reliance on each slot's own Awake running before
    // or after this, which was the source of the color not sticking on the very first frame
    // the toolbar appeared.
    public void OnToolbarShown()
    {
        foreach (InventorySlot slot in toolbarSlots)
        {
            slot.DeSelect();
        }

        // Keep whatever was previously selected; only default to slot 0 the first time.
        int slotToSelect = selectedSlot >= 0 ? selectedSlot : 0;
        ChangeSelectedSlot(slotToSelect);
    }

    public void OpenContainer(InventoryContainer container)
    {
        GameState_Inventory.Instance.SetOpenContainer(container);
        GameStateManager.Instance.SwitchToState(GameState_Inventory.Instance);
    }

    public void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0)
        {
            toolbarSlots[selectedSlot].DeSelect(); // Deselect the previously selected slot
        }

        toolbarSlots[newValue].Select(); // Select the new slot
        selectedSlot = newValue; // Update the selected slot index

        OnSelectedSlotChanged?.Invoke(selectedSlot);
    }

    // Moves the selected slot by +1/-1 with wraparound, e.g. for mouse scroll wheel input
    public void ScrollSelectedSlot(int direction)
    {
        if (toolbarSlots == null || toolbarSlots.Length == 0) return;

        int newSlot = ((selectedSlot + direction) % toolbarSlots.Length + toolbarSlots.Length) % toolbarSlots.Length;
        ChangeSelectedSlot(newSlot);
    }


    public bool AddItemtoInventory(Item item)
    {
        // Prefer the toolbar; only spill into the backpack once it's full
        if (TryAddItemToSlots(item, toolbarSlots)) return true;
        if (TryAddItemToSlots(item, backpackSlots)) return true;

        // Inventory Full
        return false;
    }

    // Adds up to `count` units of an item, e.g. when picking a stack back up off the ground.
    // Returns how many were actually added, since the inventory may run out of room partway through.
    public int AddItemsToInventory(Item item, int count)
    {
        int added = 0;
        while (added < count && AddItemtoInventory(item))
        {
            added++;
        }

        return added;
    }

    private bool TryAddItemToSlots(Item item, InventorySlot[] slots)
    {
        // Check if any slot already contains the same item and if it is stackable
        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null &&
                itemInSlot.item == item &&
                itemInSlot.count < itemStackLimit &&
                itemInSlot.item.stackable)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }

        // Find first empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            InventoryItem itemSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }

        return false;
    }

    // Click-to-pick-up / click-to-place slot interaction (Valheim-style)
    public void SlotClicked(InventorySlot slot)
    {
        // Clicking a toolbar slot also selects it, same as scrolling or pressing its number key.
        int toolbarIndex = Array.IndexOf(toolbarSlots, slot);
        if (toolbarIndex >= 0) ChangeSelectedSlot(toolbarIndex);

        if (heldItem == null)
        {
            TryPickUpItem(slot);
        }
        else
        {
            TryPlaceHeldItem(slot);
        }
    }

    private void TryPickUpItem(InventorySlot slot)
    {
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot == null) return; // Nothing to pick up

        UIManager.Instance.HideItemTooltip();

        heldItem = itemInSlot;
        heldItemOriginSlot = slot;
        heldItemGhost = CreateGhost(slot, itemInSlot.item, itemInSlot.count);

        heldItem.image.raycastTarget = false;
        heldItem.transform.SetParent(UIManager.Instance.rootCanvas.transform, false);
        heldItem.transform.SetAsLastSibling();
    }

    private void TryPlaceHeldItem(InventorySlot targetSlot)
    {
        InventoryItem itemInTarget = targetSlot.GetComponentInChildren<InventoryItem>();

        // Empty target, or clicking back on where it came from: just place it, hold ends.
        if (itemInTarget == null || targetSlot == heldItemOriginSlot)
        {
            PlaceHeldItem(targetSlot);
            return;
        }

        // Occupied by a different item: swap. The held item lands here; the displaced item becomes newly held.
        Destroy(heldItemGhost);

        InventoryItem itemToPlace = heldItem;
        itemToPlace.transform.SetParent(targetSlot.transform, false);
        itemToPlace.image.raycastTarget = true;

        heldItem = itemInTarget;
        heldItemOriginSlot = targetSlot;
        heldItemGhost = CreateGhost(targetSlot, heldItem.item, heldItem.count);

        heldItem.image.raycastTarget = false;
        heldItem.transform.SetParent(UIManager.Instance.rootCanvas.transform, false);
        heldItem.transform.SetAsLastSibling();
    }

    private void PlaceHeldItem(InventorySlot targetSlot)
    {
        heldItem.transform.SetParent(targetSlot.transform, false);
        heldItem.image.raycastTarget = true;

        Destroy(heldItemGhost);

        heldItem = null;
        heldItemOriginSlot = null;
        heldItemGhost = null;
    }

    // Released over a specific panel but not over any specific slot within it: stack onto a
    // matching slot in that same panel with room if one exists, otherwise drop into that panel's
    // first empty slot. Scoped to whichever panel was actually dropped on, so e.g. dragging from
    // the toolbar onto empty backpack space lands in the backpack, not back in the toolbar.
    public void DropHeldItemIntoSlots(InventorySlot[] slots)
    {
        if (heldItem == null) return;

        if (TryStackHeldItem(slots)) return;

        TryPlaceHeldItemInFirstEmptySlot(slots);
    }

    // Fallback for when the drop can't be attributed to a specific panel (e.g. the panel doesn't
    // expose a slot array to target yet): toolbar preferred, then backpack, same order as
    // AddItemtoInventory.
    public void DropHeldItemIntoInventory()
    {
        if (heldItem == null) return;

        if (TryStackHeldItem(toolbarSlots) || TryStackHeldItem(backpackSlots)) return;

        if (!TryPlaceHeldItemInFirstEmptySlot(toolbarSlots))
        {
            TryPlaceHeldItemInFirstEmptySlot(backpackSlots);
        }
    }

    private bool TryStackHeldItem(InventorySlot[] slots)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot == null) continue; // Unassigned array element (e.g. missing Inspector wiring)

            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null ||
                itemInSlot.item != heldItem.item ||
                !itemInSlot.item.stackable ||
                itemInSlot.count + heldItem.count > itemStackLimit)
            {
                continue;
            }

            itemInSlot.count += heldItem.count;
            itemInSlot.RefreshCount();

            Destroy(heldItem.gameObject);
            Destroy(heldItemGhost);

            heldItem = null;
            heldItemOriginSlot = null;
            heldItemGhost = null;
            return true;
        }

        return false;
    }

    private bool TryPlaceHeldItemInFirstEmptySlot(InventorySlot[] slots)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot != null && slot.GetComponentInChildren<InventoryItem>() == null)
            {
                PlaceHeldItem(slot);
                return true;
            }
        }

        return false;
    }

    private GameObject CreateGhost(InventorySlot slot, Item item, int count)
    {
        GameObject ghost = Instantiate(inventoryItemPrefab, slot.transform);

        // Reuse the item visuals (icon + count), then strip interactivity so it's a pure placeholder.
        InventoryItem ghostItem = ghost.GetComponent<InventoryItem>();
        ghostItem.InitializeItem(item);
        ghostItem.count = count;
        ghostItem.RefreshCount();

        Color ghostColor = ghostItem.image.color;
        ghostColor.a = 0.4f;
        ghostItem.image.color = ghostColor;

        Destroy(ghostItem);

        return ghost;
    }

    // Drops the currently held item into the world as its physical mesh
    public void DropItem()
    {
        Debug.Log("Dropping item from inventory");

        if (heldItem == null) return;

        Item droppedItem = heldItem.item;
        int droppedCount = heldItem.count;

        if (droppedItem.mesh != null)
        {
            Debug.Log($"Spawning dropped item mesh for {droppedCount}x {droppedItem.name}");

            GameObject spawnedMesh = Instantiate(droppedItem.mesh, GetDropPosition(), Quaternion.identity);

            // Make sure the object knows what item (and stack size) it represents, regardless of how its prefab was wired up.
            PickupItem pickup = spawnedMesh.GetComponent<PickupItem>();
            if (pickup != null)
            {
                pickup.SetItem(droppedItem, droppedCount);
            }
        }

        Destroy(heldItem.gameObject);
        Destroy(heldItemGhost);

        heldItem = null;
        heldItemOriginSlot = null;
        heldItemGhost = null;

        Debug.Log($"Dropped {droppedCount}x {droppedItem.name} into the world");
    }

    private Vector3 GetDropPosition()
    {
        Transform player = PlayerController.Instance.transform;
        Vector3 direction = GetMouseDirectionFromPlayer(player);

        return player.position + direction * dropDistance + Vector3.up * dropHeight;
    }

    // Horizontal direction from the player towards wherever the mouse cursor is currently pointing
    private Vector3 GetMouseDirectionFromPlayer(Transform player)
    {
        Camera cam = Camera.main;
        if (cam == null) return player.forward;

        Vector3 direction = cam.ScreenPointToRay(Input.mousePosition).direction;
        direction.y = 0f; // Keep the drop on the horizontal plane regardless of where the camera is pitched

        return direction.sqrMagnitude > 0.0001f ? direction.normalized : player.forward;
    }

    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

    public Item GetSelectedItem()
    {
        InventorySlot slot = toolbarSlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
          
        if (itemInSlot != null)
        {
            return itemInSlot.item;
        }
        
        return null; // No item in the selected slot
        

    }
}
