using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUIController : MonoBehaviour
{

    UIManager uIManager;
    LevelManager levelManager;
    InputManager inputManager;
    GameStateManager gameStateManager;

    // Progress bar is a UGUI Slider (non-interactive), driven via value (0-1).
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressPercentText;
    [SerializeField] private TMP_Text assetLabel;
    [SerializeField] private Image loadingSpinner;


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


        #region Check UI References

        // Check UI References (assigned via Inspector)
        if (progressSlider == null) Debug.LogError("progressSlider not assigned on LoadingUIController");
        if (progressPercentText == null) Debug.LogError("progressPercentText not assigned on LoadingUIController");
        if (assetLabel == null) Debug.LogError("assetLabel not assigned on LoadingUIController");
        if (loadingSpinner == null) Debug.LogError("loadingSpinner not assigned on LoadingUIController");

        // Initialize the progress bar
        UpdateProgressBar(0f, "Loading...");

        #endregion
    }



    public void UpdateProgressBar(float progress, string assetName)
    {
        // progress is a value between 0 and 1
        progressSlider.value = progress;

        // Shows 0-100% (just the integer part)
        progressPercentText.text = $"{(int)(progress * 100)}%";

        assetLabel.text = assetName + "...";
    }

    private void Update()
    {
        if (loadingSpinner == null)
            return;

        spinnerRotation -= 200f * Time.unscaledDeltaTime;

        loadingSpinner.rectTransform.localEulerAngles = new Vector3(0f, 0f, spinnerRotation);
    }


}

