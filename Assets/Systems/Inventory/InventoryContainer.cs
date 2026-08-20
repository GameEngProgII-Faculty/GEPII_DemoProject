using UnityEngine;
using static UnityEditor.Progress;

public class InventoryContainer : BaseInteractable
{

    public bool open = false;

    [SerializeField] private string containerID;
    [SerializeField] private int containerSlots = 6;
    [SerializeField] private ContainerType containerType;

    public int Slots => containerSlots;

    // Persisted contents, index-aligned with the slots built in the container UI. Lives on this
    // instance so each InventoryContainer in the world keeps its own contents independently, and
    // survives close/reopen since UIManager rebuilds the slot GameObjects from scratch every time.
    public ContainerItemStack[] storedItems;

    protected override void Awake()
    {
        base.Awake();

        storedItems = new ContainerItemStack[containerSlots];
    }

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

[System.Serializable]
public struct ContainerItemStack
{
    public Item item;
    public int count;

    public bool IsEmpty => item == null;
}


