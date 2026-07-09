using UnityEngine;

/// <summary>
/// BootLoad state handles bootloader-specific behavior (cursor, timescale).
/// Does NOT handle scene loading - that's handled by BootLoader.cs after initialization.
/// 
/// This state is entered when:
/// 1. GameStateManager.Start() runs (initial bootup)
/// 2. Any time the bootloader scene is loaded
/// 
/// Scene loading is handled by BootLoader.cs MonoBehaviour, not this state.
/// </summary>

public class GameState_BootLoad : IState
{
    GameStateManager gameStateManager => GameStateManager.Instance;

    #region Singleton Instance
    private static readonly GameState_BootLoad instance = new GameState_BootLoad();
    public static GameState_BootLoad Instance = instance;
    #endregion

    public void EnterState()
    {
        // Hide cursor
        Cursor.visible = false;

        // Pause game time (bootloader shouldn't have gameplay)
        Time.timeScale = 0f;

        // NOTE: Scene loading is handled by BootLoader.cs, not here
        // BootLoader initializes all managers, then calls LevelManager.LoadMainMenu()
    }

    public void FixedUpdateState()
    {
        // No physics needed in bootloader
    }

    public void UpdateState()
    {
        // No updates needed in bootloader
    }

    public void LateUpdateState()
    {
        // No late updates needed in bootloader
    }

    public void ExitState()
    {
        // Cleanup when leaving bootloader state
    }
}