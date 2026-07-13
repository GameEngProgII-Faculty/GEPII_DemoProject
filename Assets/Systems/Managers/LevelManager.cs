using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Handles loading scenes/levels and keeping GameStateManager in sync with whatever scene is active.
// Also drives the loading screen progress bar during async loads.
public class LevelManager : MonoBehaviour
{
    // Static singleton instance
    public static LevelManager Instance { get; private set; }

    // Temp storage used by LoadNextLevel() to calculate which build index to load next.
    private int nextScene;

    // Cached shortcut references to other managers.
    GameStateManager gameStateManager => GameStateManager.Instance;
    PlayerController playerController => PlayerController.Instance;
    UIManager uIManager => UIManager.Instance;

    LoadingUIController loadingUIController => UIManager.Instance.loadingUIController;

    // Named constants for build indices
    public const int BOOTLOADER_SCENE = 0;
    public const int MAIN_MENU_SCENE = 1;

    [Header("Loading Simulation Settings")]
    [SerializeField] private bool simulateLoading = true;
    private float currentProgress = 0f;
    private float holdAt100PercentDelay = 0.25f;



    private float quickFadeDuration = 0.3f;
    private float standardFadeDuration = 1.0f;

    public GameObject testReference;

    private void Awake()
    {
        #region Singleton Pattern Setup

        // Enforce a unique instance: if one already exists, self-destruct.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Establish this instance as the global instance and persist across scene loads.
        Instance = this;
        DontDestroyOnLoad(gameObject);

        #endregion

        Debug.Log($"{GetType().Name}: Initialized");
    }
    
    // Loads a scene - routes to either sync or async loading based on settings
    public void LoadScene(int sceneId)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        bool isBootloader = currentSceneIndex == BOOTLOADER_SCENE;
        bool shouldSkipSimulation = isBootloader || !simulateLoading;

        if (shouldSkipSimulation)
        {
            string reason = isBootloader ? "in bootloader scene" : "simulateLoading is disabled";
            Debug.Log($"Using quick load ({reason}). StackTrace: {System.Environment.StackTrace}");
            StartCoroutine(QuickLoadScene(sceneId));
        }
        else
        {
            Debug.Log("Using async load with simulation");
            StartCoroutine(LoadSceneAsync(sceneId));
        }
    }

    // Quick scene load with fades but no loading screen
    IEnumerator QuickLoadScene(int sceneId)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Fade to black
        yield return StartCoroutine(uIManager.FadeToBlack(quickFadeDuration));

        // Bypass the loading screen and load the scene immediately
        SceneManager.LoadScene(sceneId);


        // TODO: This is repeated in LoadSceneAsync, Refactor into a shared method to avoid duplication.
        // Switch to appropriate state
        if (sceneId == MAIN_MENU_SCENE)
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_MainMenu);
        }
        else if (sceneId == BOOTLOADER_SCENE)
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_BootLoad);
        }
        else
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_Gameplay);
        }

        // Fade from black to reveal new scene
        yield return StartCoroutine(uIManager.FadeFromBlack(quickFadeDuration));
    }

    // Loads the next scene in Build Settings order
    public void LoadNextLevel()
    {
        nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene <= SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(nextScene);
        }
        else
        {
            Debug.Log("All levels complete!");
        }
    }

    public void LoadFirstGameplayLevel()
    {
        LoadScene(MAIN_MENU_SCENE + 1);
    }

    public void LoadMainMenu()
    {
        // Get the calling method
        System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
        var method = frame.GetMethod();
        var callingClass = method.DeclaringType;
        
        Debug.Log($"LoadMainMenu() called from: {callingClass}.{method.Name}");
        
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
        Debug.Log($"LoadMainMenu() called! Full StackTrace:\n{stackTrace}");
        LoadScene(MAIN_MENU_SCENE);
        Debug.Log("LoadMainMenu() completed");
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #region Simulated Async Loading with Progress Bar

    // Loads a scene ASYNCHRONOUSLY with loading screen and progress bar
    IEnumerator LoadSceneAsync(int sceneId)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // STEP 1: Fade to black (hide current UI)
        yield return StartCoroutine(uIManager.FadeToBlack(standardFadeDuration));

        // STEP 2: Switch to the Loading state (UI changes while screen is black)
        gameStateManager.SwitchToState(gameStateManager.gameState_Loading);

        // STEP 3: Fade from black (reveal loading UI)
        yield return StartCoroutine(uIManager.FadeFromBlack(standardFadeDuration));

        // Kick off the async load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
        asyncLoad.allowSceneActivation = false;
        
        // Simulated Loading progess with jitter and stalls to make it feel more realistic 
        if (simulateLoading)
        {
            currentProgress = 0f;
            yield return StartCoroutine(SimulateProgress(2.0f)); 
        }
       
        // Update the message to show completion
        loadingUIController.UpdateProgressBar(1.0f, "Loading Complete");



        // STEP 4: Fade to black before scene transition
        yield return StartCoroutine(uIManager.FadeToBlack(standardFadeDuration));

        // Now activate the scene
        asyncLoad.allowSceneActivation = true;

        // Wait for scene to fully activate
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // STEP 5: Switch to the new game state (while still black)
        if (sceneId == MAIN_MENU_SCENE)
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_MainMenu);
        }
        else if (sceneId == BOOTLOADER_SCENE)
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_BootLoad);
        }
        else
        {
            gameStateManager.SwitchToState(gameStateManager.gameState_Gameplay);
        }

        // STEP 6: Fade from black to reveal the new scene
        yield return StartCoroutine(uIManager.FadeFromBlack(standardFadeDuration));
    }

    // Helper coroutine to simulate loading progress
    private IEnumerator SimulateProgress(float duration)
    {
        const float jitterStrength = 2.5f;
        const float stallChancePerSecond = 0.5f; // avg number of stalls per second of real playback
        const float stallDuration = 0.1f;

        float start = currentProgress;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float durationTime = Time.unscaledDeltaTime;
            elapsed += durationTime;

            // Scale the roll by dt so frame rate can't change how often stalls occur
            if (UnityEngine.Random.value < stallChancePerSecond * durationTime)
            {
                yield return new WaitForSecondsRealtime(stallDuration);
                elapsed += stallDuration; // count the stall against the target duration
            }

            float t = Mathf.Clamp01(elapsed / duration);
            float noise = Mathf.PerlinNoise(Time.time * jitterStrength, 0f);
            float noisyT = Mathf.Clamp01(t + (noise - 0.5f) * 0.3f);

            currentProgress = Mathf.Lerp(start, 1f, noisyT);
            loadingUIController.UpdateProgressBar(currentProgress, SelectRandomAssetName());

            yield return null;
        }

        currentProgress = 1f;
        loadingUIController.UpdateProgressBar(1f, "Loading Complete");

        yield return new WaitForSecondsRealtime(holdAt100PercentDelay);
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
        return assetNames[UnityEngine.Random.Range(0, assetNames.Length)];
    }

    #endregion

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only move the player if this is a gameplay scene
        if (scene.buildIndex > MAIN_MENU_SCENE)
        {
            playerController.MovePlayerToSpawnpoint("StartPosition");
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}