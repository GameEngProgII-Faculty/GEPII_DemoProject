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

    [Header("Loading Simulation Settings")]
    [SerializeField] private bool simulateLoading = true;
    [SerializeField] private float minimumLoadTime = 0.01f; // Minimum time to show loading screen
    [SerializeField] private float fakeLoadStepDelay = 0.02f; // Delay between progress updates
    [SerializeField] private float holdAt100PercentDelay = 2.0f; // How long to show 100% before transitioning

    [SerializeField] private float minSimulatedDuration = 0.2f;
    [SerializeField] private float maxSimulatedDuration = 0.6f;

    [SerializeField] private float burstStrength = 2.5f;   // Higher = more aggressive bursts
    [SerializeField] private float stallChance = 0.15f;    // Chance per step to stall
    [SerializeField] private float stallDuration = 0.25f;  // How long a stall lasts




    // Loads a scene by build index SYNCHRONOUSLY (blocking), and switches GameStateManager
    // to the appropriate state to match. This is the main entry point most other methods
    // in this class funnel through, so state and scene stay in sync.
    public void LoadScene(int sceneId)
    {
        // Check if we should skip simulation:
        // - If simulateLoading is disabled globally, OR
        // - If we're currently in the bootloader scene and skipSimulationForBootloader is true
        bool shouldSkipSimulation = !simulateLoading || SceneManager.GetActiveScene().buildIndex == BOOTLOADER_SCENE;

        // If simulation should be used, use async loading with delays
        if (!shouldSkipSimulation)
        {
            StartCoroutine(LoadSceneAsync(sceneId));
            return;
        }


        // Otherwise, use original synchronous loading
        // Subscribe to the sceneLoaded event BEFORE triggering the load, so our callback
        // (OnSceneLoaded) is guaranteed to catch the moment this scene finishes loading.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Switch to the Loading state immediately so the loading screen UI shows up right away.
        gameStateManager.SwitchToState(gameStateManager.gameState_Loading);

        // Trigger the actual scene load. Note: LoadScene() with no LoadSceneMode argument
        // defaults to LoadSceneMode.Single, meaning it replaces all currently loaded scenes.
        SceneManager.LoadScene(sceneId);

        // Decide which game state to switch to based on which scene we just requested.
        // NOTE: this only correctly distinguishes "Main Menu" vs "everything else as Gameplay" —
        // if BootLoader (0) is ever passed in here, it will incorrectly fall into theGameplay
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


    #region Simulated Async Loading with Progress Bar

    // Loads a scene ASYNCHRONOUSLY, showing the Loading state/UI and updating the progress bar
    // as the load proceeds. Now includes simulated delays to make loading feel more substantial.
    IEnumerator LoadSceneAsync(int sceneId)
    {
        // Subscribe before starting the load, same reasoning as LoadScene() above.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // STEP 1: Fade to black (hide current UI)
        yield return StartCoroutine(uIManager.LoadingUIController.FadeToBlack());

        // STEP 2: Switch to the Loading state (UI changes while screen is black)
        gameStateManager.SwitchToState(gameStateManager.gameState_Loading);

        // STEP 3: Fade from black (reveal loading UI)
        yield return StartCoroutine(uIManager.LoadingUIController.FadeFromBlack());

        float startTime = Time.realtimeSinceStartup;

        // Simulate "preparing to load" phase (0% - 20%)
        yield return StartCoroutine(SimulateProgress(0f, 0.2f, 0.1f));

        // Kick off the async load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
        asyncLoad.allowSceneActivation = false;

        // Simulate "loading assets" phase (20% - 70%)
        yield return StartCoroutine(SimulateProgress(0.2f, 0.7f, 0.1f));

        // Poll the actual async load progress (70% - 90%)
        while (asyncLoad.progress < 0.9f)
        {
            float progressValue = Mathf.Lerp(0.7f, 0.9f, asyncLoad.progress / 0.9f);
            uIManager.LoadingUIController.UpdateProgressBar(progressValue, SelectRandomAssetName());
            yield return null;
        }

        // Simulate "finalizing" phase (90% - 100%)
        yield return StartCoroutine(SimulateProgress(0.9f, 1.0f, 0.3f));

        // Update the message to show completion
        uIManager.LoadingUIController.UpdateProgressBar(1.0f, "Loading Complete");

        // Hold at 100% for a brief moment so users can see completion
        Debug.Log($"Holding at 100% for {holdAt100PercentDelay} seconds...");
        yield return new WaitForSecondsRealtime(holdAt100PercentDelay);
        Debug.Log("Hold complete! Transitioning to new scene...");

        // STEP 4: Fade to black before scene transition
        yield return StartCoroutine(uIManager.LoadingUIController.FadeToBlack());

        // Ensure minimum load time has elapsed
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (elapsedTime < minimumLoadTime)
        {
            yield return new WaitForSecondsRealtime(minimumLoadTime - elapsedTime);
        }

        // Now activate the scene (happens while screen is black)
        asyncLoad.allowSceneActivation = true;

        // Wait for scene to fully activate
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("Scene activated, switching state...");

        // STEP 5: Switch to the new game state (while still black)
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

        // STEP 6: Fade from black to reveal the new scene
        yield return StartCoroutine(uIManager.LoadingUIController.FadeFromBlack());
    }

    // Helper coroutine to simulate loading progress with smooth updates
    private IEnumerator SimulateProgress(float startProgress, float endProgress, float baseDuration)
    {
        // Use baseDuration instead of random range for more predictable, phase-specific timing
        float duration = baseDuration;

        float elapsed = 0f;
        float current = startProgress;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            // Normalized time (0–1)
            float t = Mathf.Clamp01(elapsed / duration);

            // Add chaotic bursts using Perlin noise
            float noise = Mathf.PerlinNoise(Time.time * burstStrength, 0f);

            // Random stall chance
            if (Random.value < stallChance)
            {
                yield return new WaitForSecondsRealtime(stallDuration);
            }

            // Blend linear progress with noisy bursts
            float noisyT = Mathf.Clamp01(t + (noise - 0.5f) * 0.3f);

            current = Mathf.Lerp(startProgress, endProgress, noisyT);

            uIManager.LoadingUIController.UpdateProgressBar(current, SelectRandomAssetName());

            yield return new WaitForSecondsRealtime(fakeLoadStepDelay);
        }

        // Ensure exact final value
        uIManager.LoadingUIController.UpdateProgressBar(endProgress, SelectRandomAssetName());
    }

    private string SelectRandomAssetName()
    {
        string[] assetNames = new string[]
        {
            "Player Model",
            "Enemy AI",
            "Environment Textures",
            "Sound Effects",
            "Music Track",
            "Level Geometry",
            "Particle Effects",
            "UI Elements",
            "Animation Clips",
            "Shaders"
        };
        int randomIndex = Random.Range(0, assetNames.Length);
        return assetNames[randomIndex];
    }



    #endregion


}