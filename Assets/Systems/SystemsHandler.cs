using UnityEngine;

public class SystemsHandler : MonoBehaviour
{
    /// This script is responsible for handling the systems in the game.
    /// It will be attached to a Root GameObject in the Bootloader scene and will persist across scene loads.
    /// Manager will exist under this will be static instances, but the don't destroy on load will be handled by this script.
    ///
    /// SystemsHandler
    ///     > GameStateManager
    ///     > LevelManager
    ///     > InputManager
    ///     > PlayerController
    ///     > UIManager
    /// 

    public static SystemsHandler Instance { get; private set; }

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



}
