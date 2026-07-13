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


    private float spinnerRotation;

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

        spinnerRotation -= 200f * Time.unscaledDeltaTime;

        loadingSpinner.style.rotate = new Rotate(new Angle(spinnerRotation, AngleUnit.Degree));
    }


}

