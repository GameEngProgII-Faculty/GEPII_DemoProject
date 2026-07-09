using UnityEngine;
using UnityEngine.UIElements;

public class PauseUIController : MonoBehaviour
{
    GameManager gameManager;
    UIManager uIManager;
    LevelManager levelManager;
    InputManager inputManager;
    GameStateManager gameStateManager;

    UIDocument pauseUIDoc;

    Button resumeButton;
    Button mainMenuButton;
    Button quitButton;

    private Button[] menuButtons;

    private void Awake()
    {

    }

    public void Initialize()
    {
        #region Set UI References

        // Set UI Document Reference ( "??=" if not already set)
        pauseUIDoc ??= GetComponent<UIDocument>();
        if (pauseUIDoc == null) Debug.LogError("No UIDocument component found on this gameobject!");

        // Set Button References ( "??=" if not already set)
        resumeButton ??= pauseUIDoc.rootVisualElement.Q<Button>("ResumeButton");
        mainMenuButton ??= pauseUIDoc.rootVisualElement.Q<Button>("MainMenuButton");
        quitButton ??= pauseUIDoc.rootVisualElement.Q<Button>("QuitButton");

        // Check to make sure buttons are found
        if (resumeButton == null) Debug.LogError("Resume Button not found in PauseMenuUI Doc");
        if (mainMenuButton == null) Debug.LogError("MainMenu Button not found in PauseMenuUI Doc");
        if (quitButton == null) Debug.LogError("Quit Button not found in PauseMenuUI Doc");

        #endregion

        #region Subscribe to Button Click Events

        resumeButton.clicked += OnResumeButtonClicked;
        mainMenuButton.clicked += OnMainMenuButtonClicked;
        quitButton.clicked += OnQuitButtonClicked;

        #endregion

        #region Set UI References

        // Set UI Document Reference ( "??=" if not already set)
        pauseUIDoc ??= GetComponent<UIDocument>();
        if (pauseUIDoc == null) Debug.LogError("No UIDocument component found on this gameobject!");

        // Set Button References ( "??=" if not already set)
        resumeButton ??= pauseUIDoc.rootVisualElement.Q<Button>("ResumeButton");
        mainMenuButton ??= pauseUIDoc.rootVisualElement.Q<Button>("MainMenuButton");
        quitButton ??= pauseUIDoc.rootVisualElement.Q<Button>("QuitButton");

        // Check to make sure buttons are found
        if (resumeButton == null) Debug.LogError("Resume Button not found in PauseMenuUI Doc");
        if (mainMenuButton == null) Debug.LogError("MainMenu Button not found in PauseMenuUI Doc");
        if (quitButton == null) Debug.LogError("Quit Button not found in PauseMenuUI Doc");

        #endregion

        #region Subscribe to Button Click Events

        resumeButton.clicked += OnResumeButtonClicked;
        mainMenuButton.clicked += OnMainMenuButtonClicked;
        quitButton.clicked += OnQuitButtonClicked;

        #endregion



    }



    #region Button Click Handlers

    private void OnResumeButtonClicked()
    {
        Debug.Log("Resume Clicked");

        gameStateManager.Resume();
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
        resumeButton.clicked -= OnResumeButtonClicked;
        mainMenuButton.clicked -= OnMainMenuButtonClicked;
        quitButton.clicked -= OnQuitButtonClicked;
    }
}
