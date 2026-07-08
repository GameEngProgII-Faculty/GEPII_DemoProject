using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BootLoader ensures all game managers initialize in the correct order before gameplay begins.
/// Uses [RuntimeInitializeOnLoadMethod] to run before any scene loads, allowing you to
/// press Play in any scene and have all managers properly initialized.
/// 
/// STARTUP BEHAVIOR:
/// - Detects which scene you pressed Play in
/// - Loads BootLoader scene if needed
/// - Initializes all managers
/// - Reloads the original scene (or Main Menu if started from BootLoader)
/// 
/// SETUP INSTRUCTIONS:
/// 1. This script doesn't need to be attached to any GameObject!
/// 2. Just keep it in your project and it will auto-run when Play is pressed
/// 3. Optional: Set Script Execution Order for managers:
///    - GameManager: -100
///    - GameStateManager: 100
/// </summary>

public class BootLoaderSystem
{
    // Store the scene that was active when Play was pressed
    private static string initialSceneName;
    private static int initialSceneBuildIndex;
    private static bool showDetailedLogs = true;

    // This method runs automatically before ANY scene loads when you press Play.
    // It records which scene you started in.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        // Record the initial scene before anything else happens
        Scene activeScene = SceneManager.GetActiveScene();
        initialSceneName = activeScene.name;
        initialSceneBuildIndex = activeScene.buildIndex;

        if (showDetailedLogs)
            Debug.Log($"BootLoader: Detected initial scene: {initialSceneName} (Index: {initialSceneBuildIndex})");
    }

    // This method runs after the first scene is loaded.
    // It ensures we're in the BootLoader scene and starts initialization.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoad()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // If we're not in the BootLoader scene, load it first
        if (currentScene.name != "BootLoader")
        {
            if (showDetailedLogs)
                Debug.Log($"BootLoader: Not in BootLoader scene. Loading BootLoader first...");

            // Load BootLoader scene, then we'll reload the original scene after initialization
            SceneManager.sceneLoaded += OnBootLoaderSceneLoaded;
            SceneManager.LoadScene("BootLoader");
        }
        else
        {
            // Already in BootLoader, start initialization
            StartInitialization();
        }
    }

    // Called when the BootLoader scene finishes loading.
    private static void OnBootLoaderSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BootLoader")
        {
            SceneManager.sceneLoaded -= OnBootLoaderSceneLoaded;
            StartInitialization();
        }
    }

    // Starts the manager initialization process.
    private static void StartInitialization()
    {
        // Find or create a temporary GameObject to run our coroutine
        GameObject bootLoaderRunner = new GameObject("BootLoaderRunner");
        Object.DontDestroyOnLoad(bootLoaderRunner);

        BootLoaderRunner runner = bootLoaderRunner.AddComponent<BootLoaderRunner>();
        runner.StartCoroutine(InitializeAndLoad());
    }

    // Main initialization sequence. 
    private static IEnumerator InitializeAndLoad()
    {
        if (showDetailedLogs)
            Debug.Log("=== BootLoader: Starting Manager Initialization ===");

        // Wait for all managers to be ready
        yield return WaitForAllManagers();

        if (showDetailedLogs)
            Debug.Log("=== BootLoader: All Managers Initialized Successfully! ===");

        // Small delay to let everything settle
        yield return new WaitForSecondsRealtime(0.5f);

        // Load the appropriate scene
        LoadTargetScene();
    }


    // Waits for all managers to be initialized.
    private static IEnumerator WaitForAllManagers()
    {
        float maxWaitTime = 0.1f;

        // Wait for GameManager
        yield return WaitForCondition(() => GameManager.Instance != null, "GameManager", maxWaitTime);

        if (GameManager.Instance == null)
        {
            Debug.LogError("BootLoader: GameManager not found! Cannot continue.");
            yield break;
        }

        // Wait for each manager
        yield return WaitForCondition(() => GameManager.Instance.UIManager != null, "UIManager", maxWaitTime);

        yield return WaitForCondition(() => GameManager.Instance.LevelManager != null, "LevelManager", maxWaitTime);

        yield return WaitForCondition(() => GameManager.Instance.InputManager != null, "InputManager", maxWaitTime);

        yield return WaitForCondition(() => GameManager.Instance.PlayerController != null, "PlayerController", maxWaitTime);

        yield return WaitForCondition(() => GameManager.Instance.InteractionManager != null, "InteractionManager", maxWaitTime);

        yield return WaitForCondition(() => GameManager.Instance.GameStateManager != null, "GameStateManager", maxWaitTime);

    }

 
    // Waits for a condition to be true or times out.
    private static IEnumerator WaitForCondition(System.Func<bool> condition, string managerName, float maxWaitTime)
    {
        float elapsed = 0f;

        while (!condition() && elapsed < maxWaitTime)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (condition())
        {
            if (showDetailedLogs)
                Debug.Log($"✓ BootLoader: {managerName} initialized");
        }
        else
        {
            Debug.LogError($"✗ BootLoader: {managerName} failed to initialize within {maxWaitTime}s!");
        }
    }

    // Loads the appropriate target scene.
    private static void LoadTargetScene()
    {
        // If we started in BootLoader, go to Main Menu
        if (initialSceneName == "BootLoader")
        {
            if (showDetailedLogs)
                Debug.Log("BootLoader: Started from BootLoader → Loading Main Menu");

            GameManager.Instance.LevelManager.LoadMainMenu();
        }
        // Otherwise, reload the scene we started in
        else
        {
            if (showDetailedLogs)
                Debug.Log($"BootLoader: Started from {initialSceneName} → Reloading that scene");

            GameManager.Instance.LevelManager.LoadScene(initialSceneBuildIndex);
        }
    }

    // Helper MonoBehaviour to run coroutines (since BootLoader is not a MonoBehaviour). 
    private class BootLoaderRunner : MonoBehaviour
    {
        // This class exists only to run coroutines
        // It will be destroyed after the coroutine completes
    }
}