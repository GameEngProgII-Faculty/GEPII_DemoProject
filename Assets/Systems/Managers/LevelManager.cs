using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Handles loading scenes/levels and keeping GameStateManager in sync with whatever scene is active.
// Also drives the loading screen progress bar during async loads.
public class LevelManager : MonoBehaviour, IManager
{
    // Static singleton instance
    public static LevelManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

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
    [SerializeField] private bool simulateLoading = false;
    [SerializeField] private float minimumLoadTime = 0.01f;
    [SerializeField] private float fakeLoadStepDelay = 0.02f;
    private float holdAt100PercentDelay = 0.25f;

    [SerializeField] private float burstStrength = 2.5f;
    [SerializeField] private float stallChance = 0.15f;
    [SerializeField] private float stallDuration = 0.1f;

    private float quickFadeDuration = 1.0f;
    private float standardFadeDuration = 1.0f;

    public GameObject testReference;

    private void Awake()
    {
        #region Singleton
        // Singleton pattern to ensure only one instance of LevelManager exists

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        #endregion

        // Register with Managers root
        Managers.Instance.RegisterManager(this);
    }

    public async Task<bool> InitializeAsync()
    {
        /// What BELONGS in InitializeAsync():
        /// + Reference assignment
        /// + Validation of references
        /// + Anything that used to be in Awake() but must run after BootLoader loads
        /// 
        /// What does NOT BELONG in InitializeAsync():
        /// - Entering gameplay states
        /// - Running state machine transitions
        /// - Calling EnterState()
        /// - Anything that depends on the target scene being loaded

        await Task.Yield();

        try
        {
            // Assign references
            // Validate dependencies
            // Initialize subsystems
            // Enable input maps
            // Load config
            // Warm up resources
        }
        catch (Exception ex)
        {
            Debug.LogError($"{Name}: Initialization failed — {ex.Message}");
            return false;
        }

        // everything checks out, return true to indicate successful initialization
        return true;
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

        // Load scene ASYNC but don't show progress
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);

        // Wait for it to complete
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

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

        float startTime = Time.realtimeSinceStartup;

        // Simulate "preparing to load" phase (0% - 20%)
        yield return StartCoroutine(SimulateProgress(0f, 0.2f, 0.1f));

        // Simulate "loading assets" phase (20% - 70%)
        yield return StartCoroutine(SimulateProgress(0.2f, 0.7f, 0.1f));

        // Poll the actual async load progress (70% - 90%)
        while (asyncLoad.progress < 0.9f)
        {
            float progressValue = Mathf.Lerp(0.7f, 0.9f, asyncLoad.progress / 0.9f);
            loadingUIController.UpdateProgressBar(progressValue, SelectRandomAssetName());
            yield return null;
        }

        // Simulate "finalizing" phase (90% - 100%)
        yield return StartCoroutine(SimulateProgress(0.9f, 1.0f, 0.3f));

        // Update the message to show completion
        loadingUIController.UpdateProgressBar(1.0f, "Loading Complete");

        // Hold at 100% for a brief moment
        yield return new WaitForSecondsRealtime(holdAt100PercentDelay);

        // STEP 4: Fade to black before scene transition
        yield return StartCoroutine(uIManager.FadeToBlack(standardFadeDuration));

        // Ensure minimum load time has elapsed
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (elapsedTime < minimumLoadTime)
        {
            yield return new WaitForSecondsRealtime(minimumLoadTime - elapsedTime);
        }

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
    private IEnumerator SimulateProgress(float startProgress, float endProgress, float baseDuration)
    {
        float duration = baseDuration;
        float elapsed = 0f;
        float current = startProgress;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float noise = Mathf.PerlinNoise(Time.time * burstStrength, 0f);

            if (UnityEngine.Random.value < stallChance)
            {
                yield return new WaitForSecondsRealtime(stallDuration);
            }

            float noisyT = Mathf.Clamp01(t + (noise - 0.5f) * 0.3f);
            current = Mathf.Lerp(startProgress, endProgress, noisyT);

            loadingUIController.UpdateProgressBar(current, SelectRandomAssetName());

            yield return new WaitForSecondsRealtime(fakeLoadStepDelay);
        }

        loadingUIController.UpdateProgressBar(endProgress, SelectRandomAssetName());
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