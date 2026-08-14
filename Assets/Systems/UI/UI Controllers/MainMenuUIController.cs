using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    UIManager uIManager;
    LevelManager levelManager;
    InputManager inputManager;
    GameStateManager gameStateManager;

    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        // We use Start() to Initialize UIControllers
        // This ensures to ensure all Managers are initialized in Awake()
        Initialize();
    }

    public void Initialize()
    {
        #region Set Manager References

        // Set Managers References ( "??=" if not already set)
        uIManager ??= UIManager.Instance;
        levelManager ??= LevelManager.Instance;
        inputManager ??= InputManager.Instance;
        gameStateManager ??= GameStateManager.Instance;

        //check manager references for null
        if (uIManager == null) Debug.LogError("UIManager reference is null!");
        if (levelManager == null) Debug.LogError("LevelManager reference is null!");
        if (inputManager == null) Debug.LogError("InputManager reference is null!");
        if (gameStateManager == null) Debug.LogError("GameStateManager reference is null!");

        #endregion

        #region Check UI References

        // Check Button References (assigned via Inspector)
        if (playButton == null) Debug.LogError("Play Button not assigned on MainMenuUIController");
        if (optionsButton == null) Debug.LogError("Options Button not assigned on MainMenuUIController");
        if (quitButton == null) Debug.LogError("Quit Button not assigned on MainMenuUIController");

        #endregion

        #region Subscribe to Button Click Events

        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);

        #endregion
    }




    #region Button Click Handlers
    private void OnQuitButtonClicked()
    {
        Application.Quit();
        Debug.Log("Quit Clicked - Application.Quit() called");
    }

    private void OnOptionsClicked()
    {
        Debug.Log("Options Clicked - Not Implemented Yet");
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log("Play Clicked - Loading Level 1");

        levelManager.LoadFirstGameplayLevel();
    }

    #endregion


    private void OnDestroy()
    {
        #region UnSubscribe to Button Click Events

        if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
        if (optionsButton != null) optionsButton.onClick.RemoveListener(OnOptionsClicked);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitButtonClicked);

        #endregion
    }


}
