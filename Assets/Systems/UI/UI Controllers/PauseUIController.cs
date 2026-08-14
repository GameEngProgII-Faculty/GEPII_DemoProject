using UnityEngine;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour
{
    public UIManager uIManager;
    public LevelManager levelManager;
    public InputManager inputManager;
    public GameStateManager gameStateManager;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
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
        if (resumeButton == null) Debug.LogError("Resume Button not assigned on PauseUIController");
        if (optionsButton == null) Debug.LogError("Options Button not assigned on PauseUIController");
        if (mainMenuButton == null) Debug.LogError("MainMenu Button not assigned on PauseUIController");
        if (quitButton == null) Debug.LogError("Quit Button not assigned on PauseUIController");

        #endregion

        #region Subscribe to Button Click Events

        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);

        #endregion
    }



    #region Button Click Handlers

    private void OnResumeButtonClicked()
    {
        Debug.Log("Resume Clicked");

        gameStateManager.Resume();
    }

    private void OnOptionsClicked()
    {
        Debug.Log("Options Clicked - Not Implemented Yet");
    }

    private void OnMainMenuButtonClicked()
    {
        Debug.Log("MainMenu Clicked");

        levelManager.LoadMainMenu();
    }



    private void OnQuitButtonClicked()
    {
        Application.Quit();
        Debug.Log("Quit Clicked - Application.Quit() called");
    }





    #endregion

    private void OnDestroy()
    {
        resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
        optionsButton.onClick.RemoveListener(OnOptionsClicked);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }
}
