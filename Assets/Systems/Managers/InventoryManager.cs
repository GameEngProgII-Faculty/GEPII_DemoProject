using JetBrains.Annotations;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Singleton instance of GameManager for global access
    public static InventoryManager Instance { get; private set; }

    public int itemStackLimit = 5; // Maximum number of items that can be stacked in one slot
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

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


    public bool AddItemtoInventory(Item item)
    {
        // Check if any slot already contains the same item and if it is stackable
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
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
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        
        // Inventory Full
        return false;
    }




    public void DropItem()
    { 
               Debug.Log("Item dropped from inventory");
    }

    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }


}
