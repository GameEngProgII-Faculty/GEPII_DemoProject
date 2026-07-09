using UnityEngine;


public class GameState_Loading : IState
{
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;






    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_Loading instance = new GameState_Loading();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_Loading Instance = instance;
    #endregion



    public void EnterState()
    {
        // Hide cursor and lock it to the center of the screen
        Cursor.visible = false;

        // Set timescale to 0f;
        Time.timeScale = 0f;

        uIManager.ShowLoadingScreenUI();

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
    }

}
