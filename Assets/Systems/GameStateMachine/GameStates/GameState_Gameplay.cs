using UnityEngine;
using UnityEngine.InputSystem;

public class GameState_Gameplay : IState
{
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;






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



    }

    public void FixedUpdateState()
    {

    }

    public void UpdateState()
    {
        playerController.HandlePlayerMovement();

        // Debug.Log("Running Gameplay Update State");


        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            gameStateManager.Pause();
        }






    }

    public void LateUpdateState()
    {
        playerController.HandlePlayerLook();
    }

    public void ExitState()
    {
        //Debug.Log("Exiting gameplay State");
    }

}
