using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameState_BootLoad : IState
{
    GameManager gameManager => GameManager.Instance;
    GameStateManager gameStateManager => GameManager.Instance.GameStateManager;
    PlayerController playerController => GameManager.Instance.PlayerController;
    UIManager uIManager => GameManager.Instance.UIManager;






    #region Singleton Instance
    // A single, readonly instance of the atate class is created.
    // The 'readonly' keyword ensures this instance cannot be modified after initialization.
    private static readonly GameState_BootLoad instance = new GameState_BootLoad();

    // Provides global access to the singleton instance of this state.
    // Uses an expression-bodied property to return the static _instance variable.
    public static GameState_BootLoad Instance = instance;
    #endregion



    public void EnterState()
    {
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
        Debug.Log($"GameState_BootLoad.EnterState() called! Full StackTrace:\n{stackTrace}");
        
        Cursor.visible = false;
        Time.timeScale = 0f;

        // Log the scene check
        int sceneCount = SceneManager.sceneCount;
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Scene check: sceneCount={sceneCount}, sceneName={sceneName}");

        // if BootLoader is the only active scene, redirect to MainMenu
        if (sceneCount == 1 && sceneName == "BootLoader")
        {
            Debug.Log("BootLoader is the only active scene. Loading MainMenu...");
            GameManager.Instance.LevelManager.LoadMainMenu();
            Debug.Log("LoadMainMenu() has been called, now returning from EnterState.");
            return;
        }

        // if the Bootloader is Initialized while in the MainMenu Scene
        else if (sceneCount > 1 && sceneName == "MainMenu")
        {
            Debug.Log("BootLoader initialized in MainMenu Scene, Switching to GameState_MainMenu");
            gameManager.GameStateManager.SwitchToState(GameState_MainMenu.Instance);
            return;
        }

        // if all the above fails the assumption is that we are in a Gameplay Scene
        else if (sceneCount > 1)
        {
            Debug.Log("BootLoader initialized in Gameplay Scene, Switching to GameState_Gameplay");
            gameManager.GameStateManager.SwitchToState(GameState_Gameplay.Instance);
            return;
        }
        else
        {
            Debug.LogError("BootLoader could not determine the current scene type. Please check scene setup.");
        }

        Debug.Log("GameState_BootLoad.EnterState() completed!");
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
