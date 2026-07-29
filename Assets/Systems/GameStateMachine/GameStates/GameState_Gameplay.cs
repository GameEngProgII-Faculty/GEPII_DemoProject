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

        uIManager.ShowGameplayUI();


        // Subscribe to necessary input events
        inputManager.OnPauseInputEvent += HandlePauseInput;
        inputManager.OnInventoryInputEvent += HandleInventoryInput;
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
        // Ususcribe from Input events
        inputManager.OnPauseInputEvent -= HandlePauseInput;
        inputManager.OnInventoryInputEvent -= HandleInventoryInput;
    }


    private void HandleInventoryInput(InputAction.CallbackContext context)
    {
        gameStateManager.SwitchToState(GameState_PlayerInventory.Instance);
    }

    private void HandlePauseInput(InputAction.CallbackContext context)
    {
        gameStateManager.Pause();
    }

}
