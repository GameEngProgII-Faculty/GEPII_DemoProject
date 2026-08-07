using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameState_Gameplay : IState
{

    // Cached shortcut references
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;
    InputManager inputManager => InputManager.Instance;
    InventoryManager inventoryManager => InventoryManager.Instance;
    InteractionManager interactionManager => InteractionManager.Instance;






    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_Gameplay instance = new GameState_Gameplay();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_Gameplay Instance = instance;
    #endregion


   

    public void EnterState()
    {
        // Debug.Log("Entered Gameplay State");

        Time.timeScale = 1f; // Resume  
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        uIManager.ShowGameplayUI();


        // Subscribe to necessary input events
        inputManager.OnToolbarSlotInputEvent += HandleToolbarSlotInput;
        inputManager.OnToolbarScrollInputEvent += HandleToolbarScrollInput;
        inputManager.UseToolInputEvent += HandleUseToolInput;

    }


    public void FixedUpdateState()
    {

    }

    public void UpdateState()
    {
        playerController.HandlePlayerMovement();
    }

    public void LateUpdateState()
    {
        playerController.HandlePlayerLook();
    }

    public void ExitState()
    {
        Cursor.lockState = CursorLockMode.None;

        // Ususcribe from Input events
        inputManager.OnToolbarSlotInputEvent += HandleToolbarSlotInput;
        inputManager.OnToolbarScrollInputEvent -= HandleToolbarScrollInput;
        inputManager.UseToolInputEvent -= HandleUseToolInput;
    }




    private void HandleToolbarSlotInput(InputAction.CallbackContext context, int inputID)
    {
        inventoryManager.ChangeSelectedSlot(inputID - 1);
    }

    private void HandleToolbarScrollInput(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        float scrollValue = context.ReadValue<float>();
        if (scrollValue == 0f) return;

        // Scroll up selects the previous slot, scroll down selects the next slot
        int direction = scrollValue > 0f ? -1 : 1;
        inventoryManager.ScrollSelectedSlot(direction);
    }

    // Left click: only ResourceNodes respond to this (with their own required-tool check).
    // Other interactables (pickups, etc.) stay on the "Interact" (E) action via InteractionManager.
    private void HandleUseToolInput(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        if (interactionManager.CurrentFocusedInteractable is ResourceNode resourceNode)
        {
            resourceNode.OnInteract();
        }
    }


}
