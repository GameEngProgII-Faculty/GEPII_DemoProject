using UnityEngine;

public class InventoryManager : MonoBehaviour
{


    public void AddItemtoIventory()
    {
        Debug.Log("Item added to inventory");
    }

    public void DropItem()
    { 
               Debug.Log("Item dropped from inventory");
    }

    public void spawnItem()
    {
        Debug.Log("Item spawned from inventory");
    }


}
