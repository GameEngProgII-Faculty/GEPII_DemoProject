using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUIController : MonoBehaviour
{
    GameManager gameManager;
    UIManager uIManager;
    LevelManager levelManager;
    InputManager inputManager;
    GameStateManager gameStateManager;

    UIDocument mainMenuUIDoc;

    Button playButton;
    Button optionsButton;
    Button quitButton;

    Button[] menuButtons;

    private void Awake()
    {
        #region Set Manager References

        // Set Managers References ( "??=" if not already set)
        gameManager ??= GameManager.Instance;
        uIManager ??= GameManager.Instance.UIManager;
        levelManager ??= GameManager.Instance.LevelManager;
        inputManager ??= GameManager.Instance.InputManager;
        gameStateManager ??= GameManager.Instance.GameStateManager;

        //check manager references for null
        if (gameManager == null) Debug.LogError("GameManager reference is null!");
        if (uIManager == null) Debug.LogError("UIManager reference is null!");
        if (levelManager == null) Debug.LogError("LevelManager reference is null!");
        if (inputManager == null) Debug.LogError("InputManager reference is null!");
        if (gameStateManager == null) Debug.LogError("GameStateManager reference is null!");

        #endregion
    }

    // Start() call is the reccomended init method for setting UItoolkit references
    private void Start()
    {
        #region Set UI References

        // Set UI Document Reference ( "??=" if not already set)
        mainMenuUIDoc ??= GetComponent<UIDocument>();
        if(mainMenuUIDoc == null) Debug.LogError("No UIDocument component found on this gameobject!");

        // Set Button References ( "??=" if not already set)
        playButton = mainMenuUIDoc.rootVisualElement.Q<Button>("PlayButton");
        optionsButton = mainMenuUIDoc.rootVisualElement.Q<Button>("OptionsButton");
        quitButton = mainMenuUIDoc.rootVisualElement.Q<Button>("QuitButton");

        // Check to make sure buttons are found
        if(playButton == null) Debug.LogError("Play Button not found in MainMenu_UIDoc");
        if(optionsButton == null) Debug.LogError("Options Button not found in MainMenu_UIDoc");
        if(quitButton == null) Debug.LogError("Quit Button not found in MainMenu_UIDoc");

        #endregion

        #region Subscribe to Button Click Events

        if (playButton != null) playButton.clicked += OnPlayButtonClicked;
        if (optionsButton != null) optionsButton.clicked += OnOptionsClicked;
        if (quitButton != null) quitButton.clicked += OnQuitButtonClicked;

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

        if (playButton != null) playButton.clicked -= OnPlayButtonClicked;
        if (optionsButton != null) optionsButton.clicked -= OnOptionsClicked;
        if (quitButton != null) quitButton.clicked -= OnQuitButtonClicked;

        #endregion
    }


}
