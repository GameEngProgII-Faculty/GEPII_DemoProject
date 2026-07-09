using UnityEngine;
using UnityEngine.UIElements;

public class GameplayUIController : MonoBehaviour
{
    GameManager gameManager;
    UIManager uIManager;
    LevelManager levelManager;
    InputManager inputManager;
    GameStateManager gameStateManager;

    UIDocument gameplayUIDoc;


    private void Awake()
    {
        
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

        #region Set UI References

        // Set UI Document Reference ( "??=" if not already set)
        gameplayUIDoc ??= GetComponent<UIDocument>();
        if (gameplayUIDoc == null) Debug.LogError("No UIDocument component found on this gameobject!");

        #endregion
    }





}
