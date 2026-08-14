using UnityEngine;

public class GameplayUIController : MonoBehaviour
{
    UIManager uIManager;
    LevelManager levelManager;
    InputManager inputManager;
    GameStateManager gameStateManager;

    void Start()
    {
        // We use Start() to Initialize UIControllers
        // This ensures to ensure all Managers are initialized in Awake()
        Initialize();
    }

    public void Initialize()
    {
        gameStateManager = GameStateManager.Instance;
        inputManager = InputManager.Instance;
        levelManager = LevelManager.Instance;
        uIManager = UIManager.Instance;

        if (gameStateManager == null) Debug.LogError("GameStateManager reference is null!");
        if (inputManager == null) Debug.LogError("InputManager reference is null!");
        if (levelManager == null) Debug.LogError("LevelManager reference is null!");
        if (uIManager == null) Debug.LogError("UIManager reference is null!");
    }

}
