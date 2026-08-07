using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameState_PlayerInventory : IState
{
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;
    InputManager inputManager => InputManager.Instance;
    InventoryManager inventoryManager => InventoryManager.Instance;

    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_PlayerInventory instance = new GameState_PlayerInventory();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_PlayerInventory Instance = instance;
    #endregion



    public void EnterState()
    {
        //Debug.Log("Entered Main Menu State");

        Time.timeScale = 1f; // Pause the game

        Cursor.visible = true;

        uIManager.ShowPlayerInventory();

        // Subscribe to necessary input events
        inputManager.InventoryInteractInputEvent += HandleInventoryInteractInput;


    }

    // Clicking anywhere that isn't a slot/UI element while holding an item drops it into the world.
    // Fires on both press (Performed) and release (Canceled) - DropItem() self-guards on heldItem == null,
    // so this is safe: whichever phase happens to catch a valid drop does it, the other is a no-op.
    // Uses its own InventoryInteract action (separate from the world "Interact" action) so left click
    // can later be repurposed (e.g. an attack) in GameState_Gameplay without affecting this.
    private void HandleInventoryInteractInput(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed && context.phase != InputActionPhase.Canceled) return;
        if (IsPointerOverUI()) return;

        Debug.Log("Dropping item from inventory into the world");
        inventoryManager.DropItem();
    }

    // EventSystem.current.IsPointerOverGameObject() (no args) relies on a pointer-id convention that
    // isn't reliable with InputSystemUIInputModule, so raycast explicitly against the current mouse position instead.
    private bool IsPointerOverUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
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
        inputManager.InventoryInteractInputEvent -= HandleInventoryInteractInput;
    }

}
