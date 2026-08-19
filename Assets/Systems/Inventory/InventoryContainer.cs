using UnityEngine;
using static UnityEditor.Progress;

public class InventoryContainer : BaseInteractable
{

    public bool open = false;

    [SerializeField] private string containerID;
    [SerializeField] private int containerSlots = 6;
    [SerializeField] private ContainerType containerType;

    public int Slots => containerSlots;

    public override string GetInteractionPrompt()
    {
        return $"[E] Open Container";
    }

    public override void OnInteract()
    {
        // Ignore calls to open if it's already opened
        if (open == true) { return; }

        InventoryManager.Instance.OpenContainer(this);
        open = true;
    }
}

public enum ContainerType
{
    StorageBox,
    Corpse
}


