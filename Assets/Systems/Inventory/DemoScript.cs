using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public Item[] itemsToPickup;

    public void PickupItem(int id)
    {
        bool result = InventoryManager.Instance.AddItemtoInventory(itemsToPickup[id]);

        if (result == true)
        {
            Debug.Log($"Picked up item: {itemsToPickup[id].name}");
        }
        else
        {
            Debug.Log($"INVENTORY FULL! Could not pick up item: {itemsToPickup[id].name}");
        }


    }

    public void GetSelectedItem()        
    {
        Item receivedItem = InventoryManager.Instance.GetSelectedItem();

        if (receivedItem != null)
        {
            Debug.Log($"Selected item: {receivedItem.name}");
        }
        else
        {
            Debug.Log("No item selected.");
        }
    }


}
