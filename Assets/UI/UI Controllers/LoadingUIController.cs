using UnityEngine;

using UnityEngine.UIElements;

public class LoadingUIController : MonoBehaviour
{
    GameManager gameManager;
    UIManager uIManager;
    LevelManager levelManager;
    InputManager inputManager;
    GameStateManager gameStateManager;

    UIDocument loadingUIDoc;
    ProgressBar progressBar;
    Label assetLabel;
    Image loadingSpinner;


    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2.0f;

    private float spinnerRotation;

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

    // Start() call is reccomended for setting UItoolkit references
    private void Start()
    {
        #region Set UI References

        // Set UI Document Reference ( "??=" if not already set)
        loadingUIDoc ??= GetComponent<UIDocument>();
        if (loadingUIDoc == null) Debug.LogError("No UIDocument component found on this gameobject!");

        // Set progressBar ( "??=" if not already set)
        progressBar ??= loadingUIDoc.rootVisualElement.Q<ProgressBar>("ProgressBar");
        if (progressBar == null) Debug.LogError("ProgressBar not found in LoadingUI Doc");

        // Set assetLabel ( "??=" if not already set)
        assetLabel ??= loadingUIDoc.rootVisualElement.Q<Label>("AssetLabel");
        if (assetLabel == null) Debug.LogError("assetLabel not found in LoadingUI Doc");

        // Set loadingSpinner ( "??=" if not already set)
        loadingSpinner ??= loadingUIDoc.rootVisualElement.Q<Image>("LoadingSpinner");
        if (loadingSpinner == null) Debug.LogError("loadingSpinner not found in LoadingUI Doc");

        // Initialize the progress bar
        UpdateProgressBar(0f, "Loading...");

        #endregion
    }

    public void UpdateProgressBar(float progress, string assetName)
    {
        // progress bar is a value between 0 and 1
        progressBar.value = progress;

        // Shows 0-100% (just the integer part)
        progressBar.title = $"{(int)(progress * 100)}%";

        assetLabel.text = assetName + "...";
    }

    private void Update()
    {
        if (loadingSpinner == null)
            return;
        ;
        spinnerRotation -= 200f * Time.deltaTime;

        loadingSpinner.style.rotate = new Rotate(new Angle(spinnerRotation, AngleUnit.Degree));
    }


}

