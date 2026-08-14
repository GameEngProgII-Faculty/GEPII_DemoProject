using UnityEngine;
using static UnityEditor.Progress;

public class InventoryContainer : BaseInteractable
{
    [SerializeField] private string containerID;
    [SerializeField] private int containerSlots = 6;
    [SerializeField] private ContainerType containerType;

    public override string GetInteractionPrompt()
    {
        return $"[E] Open Container";
    }

    public override void OnInteract()
    {
        InventoryManager.Instance.OpenContainer(containerSlots);
    }
}

public enum ContainerType
{
    StorageBox,
    Corpse
}


