using System;
using UnityEngine;

public class GameState_PlayerInventory : IState
{
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;
    InputManager inputManager => InputManager.Instance;

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

        Time.timeScale = 0f; // Pause the game

        Cursor.visible = true;

        uIManager.ShowPlayerInventory();

        // Subscribe to necessary input events
        inputManager.OnInventoryInputEvent += HandleInventoryInput;


    }

    private void HandleInventoryInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        gameStateManager.SwitchToState(GameState_Gameplay.Instance);
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
    }

}
