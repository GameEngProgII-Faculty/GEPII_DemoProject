using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Static singleton instance
    public static UIManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject loadingScreenPanel;

    public LoadingUIController loadingUIController;

    // Global fade UI (created at runtime)
    private CanvasGroup globalFadeGroup;

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
        #endregion

        // Create global fade UI at runtime
        CreateGlobalFadeUI();

        HideAllUIMenus();

        Debug.Log($"{GetType().Name}: Initialized");
    }

    public void ShowMainMenu()
    {
        HideAllUIMenus();
        mainMenuPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        HideAllUIMenus();
        pausePanel.SetActive(true);
    }

    public void ShowGameplayUI()
    {
        HideAllUIMenus();
        gameplayPanel.SetActive(true);
    }

    public void ShowLoadingScreenUI()
    {
        HideAllUIMenus();
        loadingScreenPanel.SetActive(true);
    }

    public void HideAllUIMenus()
    {
        if (mainMenuPanel == null) Debug.LogError("mainMenuPanel is null, please check the UIManager setup.");
        if (pausePanel == null) Debug.LogError("pausePanel is null, please check the UIManager setup.");
        if (gameplayPanel == null) Debug.LogError("gameplayPanel is null, please check the UIManager setup.");
        if (loadingScreenPanel == null) Debug.LogError("loadingScreenPanel is null, please check the UIManager setup.");

        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        pausePanel.SetActive(false);
        loadingScreenPanel.SetActive(false);
    }

    #region Global Fade Panel

    private void CreateGlobalFadeUI()
    {
        // Create a Canvas as a child of UIManager, rendering above every other UI canvas.
        GameObject fadeObject = new GameObject("GlobalFadeCanvas");
        fadeObject.transform.SetParent(transform, false);

        Canvas fadeCanvas = fadeObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;

        fadeObject.AddComponent<GraphicRaycaster>();

        globalFadeGroup = fadeObject.AddComponent<CanvasGroup>();
        globalFadeGroup.alpha = 0f; // Start transparent
        globalFadeGroup.interactable = false;
        globalFadeGroup.blocksRaycasts = false; // Disabled so it doesn't block input when invisible

        // Full-screen black image
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(fadeObject.transform, false);

        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;
    }

    // Gradual Fade TO black (hides all UI content)
    public IEnumerator FadeToBlack(float duration)
    {
        if (globalFadeGroup == null)
        {
            Debug.LogError("Global fade group is null!");
            yield break;
        }

        // Block input during fade
        globalFadeGroup.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            globalFadeGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        globalFadeGroup.alpha = 1f;
    }

    // Gradual Fade FROM black to reveal content
    public IEnumerator FadeFromBlack(float duration)
    {
        if (globalFadeGroup == null)
        {
            Debug.LogError("Global fade group is null!");
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            globalFadeGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        globalFadeGroup.alpha = 0f;

        // Disable raycast blocking so it doesn't block input when invisible
        globalFadeGroup.blocksRaycasts = false;
    }

    #endregion
}
