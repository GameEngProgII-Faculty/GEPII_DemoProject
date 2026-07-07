using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Handles loading scenes/levels and keeping GameStateManager in sync with whatever scene is active.
// Also drives the loading screen progress bar during async loads.
public class LevelManager : MonoBehaviour
{
    // Temp storage used by LoadNextLevel() to calculate which build index to load next.
    private int nextScene;

    // Cached shortcut references to other managers via the GameManager singleton.
    // Using expression-bodied properties (=>) means these are re-evaluated live each time
    // they're accessed, rather than cached once — so they'll always reflect the current
    // GameManager.Instance, even if it changes (e.g. across domain reloads in the editor).
    GameManager gameManager => GameManager.Instance;
    GameStateManager gameStateManager => GameManager.Instance.GameStateManager;
    PlayerController playerController => GameManager.Instance.PlayerController;
    UIManager uIManager => GameManager.Instance.UIManager;

    // Named constants for build indices, so scene numbers aren't scattered as "magic numbers"
    // throughout the code. Update these if scene order in Build Settings ever changes.
    public const int BOOTLOADER_SCENE = 0;
    public const int MAIN_MENU_SCENE = 1;


    // Loads a scene by build index SYNCHRONOUSLY (blocking), and switches GameStateManager
    // to the appropriate state to match. This is the main entry point most other methods
    // in this class funnel through, so state and scene stay in sync.
    public void LoadScene(int sceneId)
    {
        // Subscribe to the sceneLoaded event BEFORE triggering the load, so our callback
        // (OnSceneLoaded) is guaranteed to catch the moment this scene finishes loading.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Trigger the actual scene load. Note: LoadScene() with no LoadSceneMode argument
        // defaults to LoadSceneMode.Single, meaning it replaces all currently loaded scenes.
        SceneManager.LoadScene(sceneId);

        // Decide which game state to switch to based on which scene we just requested.
        // NOTE: this only correctly distinguishes "Main Menu" vs "everything else as Gameplay" —
        // if BootLoader (0) is ever passed in here, it will incorrectly fall into the Gameplay
        // branch below, since only MAIN_MENU_SCENE is explicitly checked.
        if (sceneId == MAIN_MENU_SCENE)
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_MainMenu);
        }
        else if (sceneId == BOOTLOADER_SCENE)
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_BootLoad);
        }
        else // it should be a Gameplay level
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_Gameplay);
        }
    }

    // Loads a scene ASYNCHRONOUSLY, showing the Loading state/UI and updating the progress bar
    // as the load proceeds. Currently unused elsewhere in this file (no public callers) —
    // likely intended to eventually replace LoadScene() for smoother transitions.
    IEnumerator LoadSceneAsync(int sceneId)
    {
        // Subscribe before starting the load, same reasoning as LoadScene() above.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Switch to the Loading state immediately so the loading screen UI shows up right away.
        gameStateManager.SwitchToState(gameStateManager.gameState_Loading);

        // Kick off the async load. Note: this only STARTS loading — activation of the scene
        // happens automatically once progress reaches ~90% unless allowSceneActivation is set to false.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);

        // Poll the load's progress every frame and update the on-screen progress bar.
        // asyncLoad.progress caps at 0.9 until the scene is allowed to activate, hence the
        // division by 0.9f below — this rescales 0�0.9 up to a full 0�1 range for display purposes.
        while (asyncLoad.isDone == false)
        {
            float progressValue = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            uIManager.LoadingUIController.UpdateProgressBar(progressValue);
            yield return null; // wait one frame before checking progress again
        }

    }


    // Loads the next scene in Build Settings order, relative to whatever scene is currently active.
    // Used for progressing from one gameplay level to the next.
    public void LoadNextLevel()
    {
        nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        // If there's still a valid next scene within Build Settings, load it.
        if (nextScene <= SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(nextScene);
        }

        // Otherwise, we've run out of levels — nothing left to load.
        else if (nextScene > SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("All levels complete!");
        }
    }

    // Convenience method to jump straight to the first gameplay level
    // (i.e. whatever comes immediately after Main Menu in build order).
    public void LoadFirstGameplayLevel()
    {
        LoadScene(MAIN_MENU_SCENE + 1);
    }

    // Loads the Main Menu scene. Routed through LoadScene() so GameStateManager
    // correctly switches to gameState_MainMenu at the same time.
    public void LoadMainMenu()
    {
        LoadScene(MAIN_MENU_SCENE);
    }

    // Reloads whatever scene is currently active (e.g. for a "Restart Level" button).
    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Callback registered to SceneManager.sceneLoaded — fires automatically once a scene
    // has finished loading. Used here to move the player to the correct spawnpoint,
    // but only for actual gameplay scenes (not BootLoader or Main Menu, which have no player/spawnpoint).
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // perform any functions that need to happen ONLY AFTER a scene is done loading

        // Only move the player if this is a genuine gameplay scene — i.e. its build index
        // is greater than MAIN_MENU_SCENE (1). This correctly skips both BootLoader (0)
        // and Main Menu (1), since neither scene contains a PlayerSpawnpoint.
        if (scene.buildIndex > MAIN_MENU_SCENE) // skip Bootloader (0) and Main Menu (1)
        {
            playerController.MovePlayerToSpawnpoint("StartPosition");
        }

        // Unsubscribe immediately after handling this load, so OnSceneLoaded doesn't
        // stack multiple subscriptions and fire more than once per scene load.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}