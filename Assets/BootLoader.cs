using UnityEngine;
using UnityEngine.SceneManagement;

// This class runs automatically before any scene loads, thanks to [RuntimeInitializeOnLoadMethod].
// Its only job is to make sure the "BootLoader" scene is loaded additively, exactly once,
// no matter which scene you press Play on in the editor.
[DefaultExecutionOrder(-100)] // Ensures this runs before other scripts that might depend on BootLoader/GameManager existing
public static class PerformBootload
{
    // Name of the BootLoader scene as it appears in Build Settings.
    // Public so other scripts (like BootLoader.Start()) can reference it without duplicating the string.
    public const string sceneName = "BootLoader";

    // [RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] makes Unity call this method automatically
    // at runtime, before the first scene's Awake() calls run — even before you press Play in some cases.
    // This is what allows BootLoader to "inject" itself no matter which scene is active.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        // If the scene we're about to play is NOT already the BootLoader scene,
        // we need to check whether BootLoader is loaded somewhere else (additively) already.
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            // Loop through all currently loaded scenes (there can be more than one if using additive loading)
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var candidateScene = SceneManager.GetSceneAt(sceneIndex);

                // If we find BootLoader already loaded, we don't need to load it again — exit early.
                if (candidateScene.name == sceneName)
                {
                    return;
                }
            }

            // If we reach this point, BootLoader was not found in any loaded scene, so load it now.
            Debug.Log("Loading BootLoader scene" + sceneName);

            // Load BootLoader ADDITIVELY (alongside whatever scene is currently active),
            // rather than replacing the current scene. This lets you press Play on ANY scene
            // during development and still have BootLoader's managers/singletons available.
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
        // If the active scene IS already "BootLoader", we do nothing here —
        // it's already loaded and active, so there's nothing to inject.
    }
}


// MonoBehaviour version of BootLoader — this is the actual component that lives in the BootLoader scene.
// It acts as a singleton and decides what to do once the BootLoader scene itself has become active.
public class BootLoader : MonoBehaviour
{
    // Singleton instance, accessible globally as BootLoader.Instance
    public static BootLoader Instance { get; private set; } = null;

    private void Awake()
    {
        #region Singleton
        // If another BootLoader instance already exists (e.g. from a previous scene load,
        // or because BootLoader got loaded twice), destroy this duplicate and stop here.
        if (Instance != null)
        {
            Debug.LogWarning("Another instance of BootLoader already exists. Destroying this one.");
            Destroy(this.gameObject);
            return;
        }

        // Otherwise, this becomes the one true instance.
        Instance = this;

        // Keep this GameObject alive across scene loads, since BootLoader (and anything
        // parented under it, like persistent managers) needs to survive into Main Menu / Gameplay.
        DontDestroyOnLoad(this.gameObject);
        #endregion
    }

    private void Start()
    {
        // This check distinguishes between two situations:
        // 1) BootLoader was loaded ADDITIVELY alongside some other scene (e.g. you pressed Play
        //    directly on a gameplay/test scene) — in that case, GetActiveScene() will be that
        //    OTHER scene, not "BootLoader", so this condition is false and we do nothing.
        // 2) You pressed Play directly on the BootLoader scene itself — in that case,
        //    GetActiveScene() IS "BootLoader", so we know we need to move on to Main Menu.
        if (SceneManager.GetActiveScene().name == PerformBootload.sceneName)
        {
            // Route through LevelManager (not a direct SceneManager.LoadScene call) so that
            // GameStateManager's state gets switched correctly at the same time the scene loads.
            // This keeps state and scene in sync, avoiding bugs like being in the Main Menu scene
            // while GameStateManager still thinks it's in Gameplay state.
            //
            // NOTE: This assumes GameManager.Instance (and its LevelManager) already exists
            // and is initialized by the time this Start() runs. If GameManager isn't ready yet,
            // this line will throw a NullReferenceException — double check initialization/execution
            // order before relying on this.
            GameManager.Instance.LevelManager.LoadMainMenu();
        }
    }

    // Simple test method to confirm BootLoader is active and reachable — useful for debugging
    // via the Inspector's context menu or a temporary button, not called anywhere critical.
    public void Test()
    {
        Debug.Log("BootLoader Scene is ACTIVE.");
    }
}