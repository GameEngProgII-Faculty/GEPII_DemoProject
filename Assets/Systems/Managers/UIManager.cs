using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    // Static singleton instance
    public static UIManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    [Header("UI Menu Objects")]
    [SerializeField] private UIDocument mainMenuUI;
    [SerializeField] private UIDocument gameplayUI;
    [SerializeField] private UIDocument pauseUI;
    [SerializeField] private UIDocument loadingScreenUI;

  
    public LoadingUIController loadingUIController;
    private MainMenuUIController mainMenuUIController;
    private GameplayUIController gameplayUIController;
    private PauseUIController pauseUIController;

    // Global fade UI (created at runtime)
    private UIDocument globalFadeUI;
    private VisualElement globalFadePanel;

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

        Initialize();

        Debug.Log($"{GetType().Name}: Initialized");

    }

    private void Initialize()
    {
        mainMenuUI = FindUIDocument("MainMenuUI");
        gameplayUI = FindUIDocument("GameplayUI");
        pauseUI = FindUIDocument("PauseUI");
        loadingScreenUI = FindUIDocument("LoadingScreenUI");

        //LoadingUIController = loadingScreenUI.GetComponent<LoadingUIController>();

        // Activate Parent GameObject of all UI Screens
        if (mainMenuUI != null) mainMenuUI.gameObject.SetActive(true);
        if (gameplayUI != null) gameplayUI.gameObject.SetActive(true);
        if (pauseUI != null) pauseUI.gameObject.SetActive(true);
        if (loadingScreenUI != null) loadingScreenUI.gameObject.SetActive(true);

        // Create global fade UI at runtime
        CreateGlobalFadeUI();

        HideAllUIMenus();
    }




    public void ShowMainMenu()
    {
        // Debug.Log("UIManager: ShowMainMenu called.");

        HideAllUIMenus();

        mainMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void ShowPauseMenu()
    {
        HideAllUIMenus();
        pauseUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void ShowGameplayUI()
    {
        HideAllUIMenus();
        gameplayUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void ShowLoadingScreenUI()
    {
        HideAllUIMenus();
        loadingScreenUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void HideAllUIMenus()
    {
        if (mainMenuUI == null) Debug.LogError("mainMenuUI is null, please check the UIManager setup.");
        if (pauseUI == null) Debug.LogError("pausedUI is null, please check the UIManager setup.");
        if (gameplayUI == null) Debug.LogError("gameplayUI is null, please check the UIManager setup.");

        mainMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        gameplayUI.rootVisualElement.style.display = DisplayStyle.None;
        pauseUI.rootVisualElement.style.display = DisplayStyle.None;
        loadingScreenUI.rootVisualElement.style.display = DisplayStyle.None;
    }

    private UIDocument FindUIDocument(string name)
    {
        var documents = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var doc in documents)
        {
            if (doc.name == name)
            {
                return doc;
            }
        }
        Debug.LogWarning($"UIDocument '{name}' not found in scene.");
        return null;
    }

    #region Global Fade Panel

    private void CreateGlobalFadeUI()
    {
        // Create a new GameObject as a child of UIManager
        GameObject fadeUIObject = new GameObject("GlobalFadeUI");
        fadeUIObject.transform.SetParent(transform);

        // Add UIDocument component
        globalFadeUI = fadeUIObject.AddComponent<UIDocument>();

        // Get the PanelSettings from one of the existing UI documents
        if (mainMenuUI != null && mainMenuUI.panelSettings != null)
        {
            globalFadeUI.panelSettings = mainMenuUI.panelSettings;
        }
        else
        {
            Debug.LogError("Cannot set PanelSettings for GlobalFadeUI - mainMenuUI or its panelSettings is null!");
            return;
        }

        // Set sort order to render on top of everything
        globalFadeUI.sortingOrder = 9999;

        // Create the fade panel visual element
        globalFadePanel = new VisualElement();
        globalFadePanel.name = "GlobalFadePanel";

        // Style it to cover the entire screen
        globalFadePanel.style.position = Position.Absolute;
        globalFadePanel.style.left = 0;
        globalFadePanel.style.top = 0;
        globalFadePanel.style.right = 0;
        globalFadePanel.style.bottom = 0;
        globalFadePanel.style.width = Length.Percent(100);
        globalFadePanel.style.height = Length.Percent(100);
        globalFadePanel.style.backgroundColor = new Color(0, 0, 0, 1); // Black
        globalFadePanel.style.opacity = 0f; // Start transparent
        
        // IMPORTANT: Disable picking (pointer events) so it doesn't block input when transparent
        globalFadePanel.pickingMode = PickingMode.Ignore;

        // Add the fade panel to the UIDocument's root
        globalFadeUI.rootVisualElement.Add(globalFadePanel);

        //Debug.Log("Global fade UI and panel created successfully!");
    }



    // Gradual Fade TO black (hides all UI content)
    public IEnumerator FadeToBlack(float duration)
    {
        if (globalFadePanel == null)
        {
            Debug.LogError("Global fade panel is null!");
            yield break;
        }

        // Enable picking so it blocks input during fade
        globalFadePanel.pickingMode = PickingMode.Position;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float opacity = Mathf.Lerp(0f, 1f, t);
            
            globalFadePanel.style.opacity = opacity;
            
            yield return null;
        }

        globalFadePanel.style.opacity = 1f;
    }
     
    // Gradual Fade FROM black to reveal content
    public IEnumerator FadeFromBlack(float duration)
    {
        if (globalFadePanel == null)
        {
            Debug.LogError("Global fade panel is null!");
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float opacity = Mathf.Lerp(1f, 0f, t);
            
            globalFadePanel.style.opacity = opacity;
            
            yield return null;
        }

        globalFadePanel.style.opacity = 0f;
        
        // Disable picking so it doesn't block input when invisible
        globalFadePanel.pickingMode = PickingMode.Ignore;
        
    }

    public IEnumerator InstantBlackout()
    {
        Debug.Log("Instant blackout called");

        yield return globalFadePanel.style.opacity = 1f;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        yield return null;
        Debug.Log("opacity: " + globalFadePanel.style.opacity);
    }

    #endregion


}
