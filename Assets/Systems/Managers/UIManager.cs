using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{




    // Static singleton instance
    public static UIManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    // Fired whenever the toolbar panel is shown (gameplay HUD or full inventory screen).
    // Other managers subscribe instead of UIManager calling them directly.
    public event Action OnToolbarPanelShown;

    public Canvas rootCanvas;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject toolbarPanel;
    [SerializeField] private GameObject backpackPanel;
    [SerializeField] private GameObject containerPanel;
    [SerializeField] private GameObject darkeningPanel;

    // Read-only access so drop-target resolution (GameState_Inventory) can tell which panel a
    // pointer is over without UIManager needing to know anything about inventory slot logic.
    public GameObject ToolbarPanel => toolbarPanel;
    public GameObject BackpackPanel => backpackPanel;



    [Header("Inventory")]
    [SerializeField] private Transform containerSlots;
    [SerializeField] private GameObject slotPrefab;


    [Header("InteractPrompt")]
    [SerializeField] private TextMeshProUGUI interactPromptText;

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
        CreateItemTooltipUI();

        HideAllUIMenus();

        Debug.Log($"{GetType().Name}: Initialized");
    }

    private void Update()
    {
        UpdateTooltipPosition();
    }


    #region ShowHide UI Panels
    public void ShowMainMenu()
    {
        HideAllUIMenus();
        mainMenuPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        HideAllUIMenus();
        pausePanel.SetActive(true);
        toolbarPanel.SetActive(true);
    }

    public void ShowGameplayUI()
    {
        Debug.Log("ShowGameplayUI");

        HideAllUIMenus();
        gameplayPanel.SetActive(true);
        toolbarPanel.SetActive(true);
        OnToolbarPanelShown?.Invoke();
    }

    public void ShowLoadingScreenUI()
    {
        HideAllUIMenus();
        loadingScreenPanel.SetActive(true);
    }

    public void ShowPlayerBackpack()
    {
        HideAllUIMenus();
        backpackPanel.SetActive(true);
        toolbarPanel.SetActive(true);
        darkeningPanel.SetActive(true);
        gameplayPanel.SetActive(true);
        OnToolbarPanelShown?.Invoke();
    }

    public void ShowInventoryContainer(int slotCount)
    {
        ShowPlayerBackpack();
        containerPanel.SetActive(true);
        BuildInventoryContainer(slotCount);
    }





    public void HideAllUIMenus()
    {
        Debug.Log("Hide all UI Panels");

        if (mainMenuPanel == null) Debug.LogError("mainMenuPanel is null, please check the UIManager setup.");
        if (pausePanel == null) Debug.LogError("pausePanel is null, please check the UIManager setup.");
        if (gameplayPanel == null) Debug.LogError("gameplayPanel is null, please check the UIManager setup.");
        if (loadingScreenPanel == null) Debug.LogError("loadingScreenPanel is null, please check the UIManager setup.");

        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        pausePanel.SetActive(false);
        loadingScreenPanel.SetActive(false);
        toolbarPanel.SetActive(false);
        backpackPanel.SetActive(false); 
        containerPanel.SetActive(false);
        darkeningPanel.SetActive(false);
    }

    #endregion


    #region Inventory Container UI

    public void BuildInventoryContainer(int slotCount)
    {
        ClearContainer();

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, containerSlots.transform);
        }

        // Force the ContentSizeFitters (on containerSlots' grid and on containerPanel itself) to
        // recompute immediately, so the panel resizes to fit this slot count on the same frame
        // instead of showing the previous size for a frame first.
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerSlots as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerPanel.GetComponent<RectTransform>());
    }

    void ClearContainer()
    {
        // Loop backwards through the children to safely delete them. Destroy() is deferred to the
        // end of the frame, so if we left these parented, the immediate layout rebuild right after
        // (in BuildInventoryContainer) would still count them alongside the newly instantiated
        // slots and compute an inflated size - unparent first so childCount reflects reality now.
        for (int i = containerSlots.childCount - 1; i >= 0; i--)
        {
            Transform child = containerSlots.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    #endregion



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


    #region Item Tooltip

    [Header("Item Tooltip")]
    [SerializeField] private Vector2 tooltipAnchorOffset = new Vector2(0f, 8f);
    [SerializeField] private Color tooltipBackgroundColor = new Color(0f, 0f, 0f, 0.85f);
    [SerializeField] private float tooltipShowDelay = 0.5f;

    private RectTransform tooltipRT;
    private CanvasGroup tooltipCanvasGroup;
    private TextMeshProUGUI tooltipNameText;
    private TextMeshProUGUI tooltipDescriptionText;
    private Coroutine tooltipShowRoutine;
    private RectTransform tooltipAnchor;
    private readonly Vector3[] tooltipAnchorCorners = new Vector3[4];

    private void CreateItemTooltipUI()
    {
        // Child of rootCanvas instead of its own Canvas.
        GameObject panel = new GameObject("TooltipPanel", typeof(RectTransform));
        panel.transform.SetParent(rootCanvas.transform, false);

        tooltipRT = panel.GetComponent<RectTransform>();
        tooltipRT.pivot = new Vector2(0.5f, 0f); // Bottom-center, so it sits just above the anchored slot

        tooltipCanvasGroup = panel.AddComponent<CanvasGroup>();
        tooltipCanvasGroup.blocksRaycasts = false;
        tooltipCanvasGroup.interactable = false;

        Image background = panel.AddComponent<Image>();
        background.color = tooltipBackgroundColor;
        background.raycastTarget = false;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.spacing = 4f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        tooltipNameText = CreateTooltipText(panel.transform, "NameText", 20, FontStyles.Bold);
        tooltipDescriptionText = CreateTooltipText(panel.transform, "DescriptionText", 16, FontStyles.Normal);

        HideItemTooltip();
    }

    private TextMeshProUGUI CreateTooltipText(Transform parent, string objectName, int fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.raycastTarget = false;

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 220f;

        return text;
    }

    public void ShowItemTooltip(Item item, RectTransform anchor)
    {
        if (item == null || anchor == null) return;

        tooltipAnchor = anchor;

        if (tooltipShowRoutine != null) StopCoroutine(tooltipShowRoutine);
        tooltipShowRoutine = StartCoroutine(ShowItemTooltipAfterDelay(item));
    }

    private IEnumerator ShowItemTooltipAfterDelay(Item item)
    {
        // Unscaled so the tooltip still delays correctly while the inventory pauses the game (Time.timeScale = 0).
        yield return new WaitForSecondsRealtime(tooltipShowDelay);

        tooltipNameText.text = item.name;

        bool hasDescription = !string.IsNullOrEmpty(item.description);
        tooltipDescriptionText.gameObject.SetActive(hasDescription);
        if (hasDescription) tooltipDescriptionText.text = item.description;

        tooltipCanvasGroup.alpha = 1f;
        tooltipRT.SetAsLastSibling(); // Render above other rootCanvas content
        tooltipShowRoutine = null;
    }

    public void HideItemTooltip()
    {
        if (tooltipShowRoutine != null)
        {
            StopCoroutine(tooltipShowRoutine);
            tooltipShowRoutine = null;
        }

        tooltipAnchor = null;
        tooltipCanvasGroup.alpha = 0f;
    }

    private void UpdateTooltipPosition()
    {
        if (tooltipCanvasGroup == null || tooltipCanvasGroup.alpha <= 0f || tooltipAnchor == null) return;

        tooltipAnchor.GetWorldCorners(tooltipAnchorCorners);
        Vector3 anchorTopCenter = (tooltipAnchorCorners[1] + tooltipAnchorCorners[2]) * 0.5f; // top-left + top-right

        tooltipRT.position = anchorTopCenter + (Vector3)tooltipAnchorOffset;
    }

    #endregion


    #region Interact Prompt

    public void ShowInteractPrompt(string promptText)
    {
        if (interactPromptText == null || string.IsNullOrEmpty(promptText))
        {
            HideInteractPrompt();
            return;
        }

        interactPromptText.text = promptText;
        interactPromptText.gameObject.SetActive(true);
    }

    public void HideInteractPrompt()
    {
        if (interactPromptText == null) return;

        interactPromptText.text = " ";
        interactPromptText.gameObject.SetActive(false);
    }

    #endregion
}
