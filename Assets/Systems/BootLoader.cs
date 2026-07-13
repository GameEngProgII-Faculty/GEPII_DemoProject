using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// Bootloader: guarantees all Singleton Managers are initialized before any gameplay scene runs.
///
/// PROCESS:
/// 1. StoreInitialScene() runs via [RuntimeInitializeOnLoadMethod(BeforeSceneLoad)], firing before
///    ANY scene's Awake() methods — even before the scene you pressed Play in has finished loading.
/// 2. It records which scene you pressed Play from (initialSceneName / initialSceneBuildIndex).
/// 3. If that scene isn't already the Bootloader scene, it force-loads the Bootloader scene instead.
/// 4. The Bootloader scene loads. Unity calls Awake() (then OnEnable()) on every object in the
///    scene — Managers and Bootloader included — before Start() runs on ANY of them. This is a
///    hard engine guarantee, so Hierarchy order between Managers and Bootloader no longer matters.
/// 5. BootLoader.Start() runs only once every Manager's Awake()/OnEnable() has completed, and
///    loads either the original scene (build index > 1) or Main Menu.
/// 6. Managers persist into that scene via DontDestroyOnLoad, fully initialized before gameplay.
///
///     
/// </summary>

public class BootLoader : MonoBehaviour
{
    private static bool showLogs = true;

    // Scene build indices for Bootloader and Main Menu scenes
    // These must match the order in Build Settings exactly, or the Bootloader will fail to load the correct scene.
    private const int BOOTLOADER_SCENE = 0;
    private const int MAINMENU_SCENE = 1;

    // Stores the scene the developer was looking at when they pressed Play
    public static string initialSceneName { get; private set; }
    public static int initialSceneBuildIndex { get; private set; }

    // -------------------------------------------------------------------------------
    //  RUNS BEFORE ANY SCENE LOADS — detect where Play was pressed
    // -------------------------------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StoreInitialScene()
    {
        initialSceneName = SceneManager.GetActiveScene().name;
        initialSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;

        if (initialSceneBuildIndex == BOOTLOADER_SCENE) return;

        if (showLogs) Debug.Log($"[Bootstrapper] Intercepted start from '{initialSceneName}'. Loading Bootloader...");
        SceneManager.LoadScene(BOOTLOADER_SCENE);
    }

    // -------------------------------------------------------------------------------
    //  BootLoader scene loads → Start() runs → load original scene
    //  We use Start() (not Awake()) because Unity guarantees every object's Awake()
    //  has already completed by the time any Start() runs — so all Managers are
    //  fully initialized here, regardless of Hierarchy order.
    // -------------------------------------------------------------------------------
    private void Start()
    {
        if (showLogs) Debug.Log("BootLoader: Start in scene " + SceneManager.GetActiveScene().name);

        if (initialSceneBuildIndex > 1)
        {
            LevelManager.Instance.LoadScene(initialSceneBuildIndex);
            //SceneManager.LoadScene(initialSceneBuildIndex);
        }
        else
        {
            LevelManager.Instance.LoadScene(MAINMENU_SCENE);
            //SceneManager.LoadScene(MAINMENU_SCENE);
        }
    }
}
