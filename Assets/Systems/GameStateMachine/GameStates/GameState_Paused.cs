using UnityEngine;
using UnityEngine.InputSystem;

public class GameState_Paused : IState
{
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;

    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_Paused instance = new GameState_Paused();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_Paused Instance = instance;
    #endregion



    public void EnterState()        
    {
        //Debug.Log("Entered Paused State");

        Time.timeScale = 0f; // Pause the game

        Cursor.visible = true;

        uIManager.ShowPauseMenu();




    }

    public void FixedUpdateState()
    {
        
    }

    public void UpdateState()
    {
        //Debug.Log("Running MainMenu Update State");

        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            gameStateManager.Resume();  
        }
    }

    public void LateUpdateState()
    {
        
    }

    public void ExitState()
    {
        //Debug.Log("Exiting Main Menu State");
    }

}
