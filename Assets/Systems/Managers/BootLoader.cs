using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BootLoaderSystem
{
    private static string initialSceneName;
    private static int initialSceneBuildIndex;

    private static bool showDetailedLogs = true;

    // How long BootLoader will wait for ALL managers (parallel)
    private static float globalInitializationTimeoutSeconds = 2f;

    private static BootLoaderRunner runnerInstance;

    private static bool managersInitialized = false;

    // ------------------------------------------------------------
    //  BEFORE ANY SCENE LOADS — detect where Play was pressed
    // ------------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        initialSceneName = activeScene.name;
        initialSceneBuildIndex = activeScene.buildIndex;

        if (showDetailedLogs)
            Debug.Log($"BootLoader: Play pressed in scene '{initialSceneName}' (Index {initialSceneBuildIndex})");
    }

    // ------------------------------------------------------------
    //  AFTER FIRST SCENE LOADS — redirect to BootLoader if needed
    // ------------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoad()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name != "BootLoader")
        {
            if (showDetailedLogs)
                Debug.Log("BootLoader: Redirecting to BootLoader scene...");

            SceneManager.sceneLoaded += OnBootLoaderSceneLoaded;
            SceneManager.LoadScene("BootLoader");
        }
        else
        {
            StartInitialization();
        }
    }

    private static void OnBootLoaderSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BootLoader")
        {
            SceneManager.sceneLoaded -= OnBootLoaderSceneLoaded;
            StartInitialization();
        }
    }

    // ------------------------------------------------------------
    //  Start initialization using a runner MonoBehaviour
    // ------------------------------------------------------------
    private static void StartInitialization()
    {
        if (managersInitialized)
            return;

        managersInitialized = true;

        GameObject runnerObj = new GameObject("BootLoaderRunner");
        UnityEngine.Object.DontDestroyOnLoad(runnerObj);

        runnerInstance = runnerObj.AddComponent<BootLoaderRunner>();
        runnerInstance.StartCoroutine(InitializeAndLoad());


    }

    // ------------------------------------------------------------
    //  MAIN INITIALIZATION SEQUENCE
    // ------------------------------------------------------------
    private static IEnumerator InitializeAndLoad()
    {
        Debug.Log("=== BootLoader: Starting Manager Initialization ===");

        // Run parallel async initialization
        Task<bool[]> initTask = InitializeManagersParallelAsync();

        float timeout = globalInitializationTimeoutSeconds;
        float elapsed = 0f;

        while (!initTask.IsCompleted && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!initTask.IsCompleted)
        {
            Debug.LogError($"BootLoader: Initialization timed out after {timeout} seconds.");
            yield break;
        }

        bool[] results = initTask.Result;

        var managers = Managers.Instance.GetRegisteredManagers();

        for (int i = 0; i < managers.Count; i++)
        {
            if (!results[i])
            {
                Debug.LogError($"BootLoader: Manager '{managers[i].Name}' FAILED to initialize.");
                yield break;
            }

            Debug.Log($"BootLoader: ✓ {managers[i].Name} ready");
        }

        Debug.Log("=== BootLoader: All Managers Initialized Successfully! ===");

        yield return new WaitForSecondsRealtime(0.2f);

        LoadTargetScene();
    }

    // ------------------------------------------------------------
    //  PARALLEL INITIALIZATION LOGIC
    // ------------------------------------------------------------
    private static async Task<bool[]> InitializeManagersParallelAsync()
    {
        if (Managers.Instance == null)
            throw new Exception("Managers.Instance is NULL — Managers root must exist before BootLoader.");

        var managers = Managers.Instance.GetRegisteredManagers();

        if (managers == null || managers.Count == 0)
            throw new Exception("No managers registered in Managers.Instance.");

        var tasks = managers.Select(m => m.InitializeAsync()).ToArray();

        return await Task.WhenAll(tasks);
    }

    // ------------------------------------------------------------
    //  LOAD TARGET SCENE (MainMenu or original scene)
    // ------------------------------------------------------------
    private static void LoadTargetScene()
    {
        Debug.Log("BootLoader: Attempting to load Scene.");

        SceneManager.sceneLoaded += OnTargetSceneLoaded;

        if (initialSceneName == "BootLoader")
        {
            Debug.Log("BootLoader: Started from BootLoader → Loading Main Menu");
            LevelManager.Instance.LoadMainMenu();
        }
        else
        {
            Debug.Log($"BootLoader: Returning to original scene '{initialSceneName}'");
            LevelManager.Instance.LoadScene(initialSceneBuildIndex);
        }
    }

    private static void OnTargetSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnTargetSceneLoaded;

        if (runnerInstance != null)
        {
            Debug.Log("BootLoader: Cleaning up BootLoaderRunner.");
            UnityEngine.Object.Destroy(runnerInstance.gameObject);
            runnerInstance = null;
        }
    }


    // ------------------------------------------------------------
    //  Runner MonoBehaviour for coroutines
    // ------------------------------------------------------------
    private class BootLoaderRunner : MonoBehaviour { }
}
