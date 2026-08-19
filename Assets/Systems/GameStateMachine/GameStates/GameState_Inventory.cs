using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameState_Inventory : IState
{
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;
    InputManager inputManager => InputManager.Instance;
    InventoryManager inventoryManager => InventoryManager.Instance;

    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_Inventory instance = new GameState_Inventory();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_Inventory Instance = instance;
    #endregion

    // if the player opens a container we will store a reference in here.
    private InventoryContainer openContainer = null;

    // Set by whoever is initiating the state switch (e.g. InventoryManager.OpenContainer)
    // before GameStateManager.SwitchToState runs EnterState, since IState.EnterState() takes no args.
    public void SetOpenContainer(InventoryContainer container)
    {
        openContainer = container;
    }

    public void EnterState()
    {
        //Debug.Log("Entered Main Menu State");

        Time.timeScale = 1f; // Pause the game

        Cursor.visible = true;

        if (openContainer != null)
        {
            uIManager.ShowInventoryContainer(openContainer.Slots);
        }
        else
        {
            uIManager.ShowPlayerBackpack();
        }

        // Subscribe to necessary input events
        inputManager.OnDragDropEvent += HandleDragDropInventory;
        inputManager.OnInteractInputEvent += HandleCloseInventoryInput;
    }

    // Pressing Interact (E) again while the inventory is open closes it. Subscribed only while this
    // state is active (added here in EnterState, not globally in GameStateManager) so it can never
    // fire on the same press that opened a container: that press is still being dispatched by
    // InteractionManager (subscribed first, in OnEnable, so it always runs before this state even
    // exists) when EnterState adds this handler, and C# event invocation uses a snapshot of the
    // subscriber list taken at the start of that dispatch - a handler added mid-dispatch only runs
    // on the next, separate raise of the event.
    private void HandleCloseInventoryInput(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        gameStateManager.ReturnToGameplay();
    }

    // Fires on both press (Performed) and release (Canceled) - DropItem()/DropHeldItemIntoInventory()
    // self-guard on heldItem == null, so this is safe: whichever phase happens to catch a valid drop
    // does it, the other is a no-op.
    // Uses its own InventoryInteract action (separate from the world "Interact" action) so left click
    // can later be repurposed (e.g. an attack) in GameState_Gameplay without affecting this.
    //
    // Three outcomes depending on what's under the cursor:
    //  - nothing (no UI hit): drop the held item into the world
    //  - a slot: do nothing here - the slot's own OnPointerDown/OnDrop already ran SlotClicked
    //  - UI, but not a slot (e.g. panel background): stack/place into the inventory automatically
    private void HandleDragDropInventory(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed && context.phase != InputActionPhase.Canceled) return;

        List<RaycastResult> uiHits = GetPointerUIHits();

        if (uiHits.Count == 0)
        {
            Debug.Log("Dropping item from inventory into the world");
            inventoryManager.DropItem();
            return;
        }

        bool overSlot = uiHits.Exists(hit => hit.gameObject.GetComponentInParent<InventorySlot>() != null);
        if (overSlot) return;

        InventorySlot[] targetSlots = ResolveDropTargetSlots(uiHits);
        if (targetSlots != null)
        {
            inventoryManager.DropHeldItemIntoSlots(targetSlots);
        }
        else
        {
            // Dropped on inventory UI that isn't a recognized panel (e.g. the container panel,
            // which doesn't expose its slots as an array yet) - fall back to the general search.
            inventoryManager.DropHeldItemIntoInventory();
        }
    }

    // Which slot array a non-slot drop should target, based on which panel was actually hit -
    // dragging from the toolbar onto empty backpack space should land in the backpack, not
    // silently fall back into whichever toolbar slot happens to look empty (e.g. the one the
    // item just came from).
    private InventorySlot[] ResolveDropTargetSlots(List<RaycastResult> uiHits)
    {
        foreach (RaycastResult hit in uiHits)
        {
            if (hit.gameObject.transform.IsChildOf(uIManager.BackpackPanel.transform)) return inventoryManager.backpackSlots;
            if (hit.gameObject.transform.IsChildOf(uIManager.ToolbarPanel.transform)) return inventoryManager.toolbarSlots;
        }

        return null;
    }

    // EventSystem.current.IsPointerOverGameObject() (no args) relies on a pointer-id convention that
    // isn't reliable with InputSystemUIInputModule, so raycast explicitly against the current mouse position instead.
    private List<RaycastResult> GetPointerUIHits()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results;
    }

    public void FixedUpdateState()
    {
        
    }

    public void UpdateState()
    {



    }

    public void LateUpdateState()
    {
        
    }

    public void ExitState()
    {
        //Debug.Log("Exiting Main Menu State");
        inputManager.OnDragDropEvent -= HandleDragDropInventory;
        inputManager.OnInteractInputEvent -= HandleCloseInventoryInput;

        if (openContainer != null)
        {
            openContainer.open = false;
        }

        openContainer = null;
    }

}
