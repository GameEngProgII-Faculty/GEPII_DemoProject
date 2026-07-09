using System;
using System.Threading.Tasks;
using UnityEngine;


public class GameStateManager : MonoBehaviour, IManager
{
    // Singleton instance of GameManager for global access
    public static GameStateManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    [Header("Debug (read only)")]
    [SerializeField] private string currentActiveState;
    [SerializeField] private string lastActiveState;

    // Private variables to store state information
    private IState currentState;  // Current active state
    private IState lastState;     // Last active state (kept private for encapsulation)

    // Instantiate GameStates
    public GameState_MainMenu gameState_MainMenu = GameState_MainMenu.Instance;
    public GameState_Gameplay gameState_Gameplay = GameState_Gameplay.Instance;
    public GameState_Paused gameState_Paused = GameState_Paused.Instance;
    public GameState_BootLoad gameState_BootLoad = GameState_BootLoad.Instance;
    public GameState_Loading gameState_Loading = GameState_Loading.Instance;

    public void Awake()
    {
        #region Singleton
        // Singleton pattern to ensure only one instance of GameManager exists

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        #endregion

        // Register with Managers root
        Managers.Instance.RegisterManager(this);
    }

    public async Task<bool> InitializeAsync()
    {
        /// What BELONGS in InitializeAsync():
        /// + Reference assignment
        /// + Validation of references
        /// + Anything that used to be in Awake() but must run after BootLoader loads
        /// 
        /// What does NOT BELONG in InitializeAsync():
        /// - Entering gameplay states
        /// - Running state machine transitions
        /// - Calling EnterState()
        /// - Anything that depends on the target scene being loaded

        await Task.Yield();

        try
        {
            // Assign references
            // Validate dependencies
            // Initialize subsystems
            // Enable input maps
            // Load config
            // Warm up resources
        }
        catch (Exception ex)
        {
            Debug.LogError($"{Name}: Initialization failed — {ex.Message}");
            return false;
        }

        // everything checks out, return true to indicate successful initialization
        return true;
    }



    private void Start()
    {
        currentState = gameState_BootLoad;
        currentActiveState = currentState.ToString();
        currentState.EnterState();
    }

    public void SwitchToState(IState newState)
    {
        lastState = currentState; // Store the current state as the last state
        lastActiveState = lastState.ToString(); // Update debug info in inspector
        currentState?.ExitState(); // Exit the current state

        currentState = newState; // Switch to the new state
        currentActiveState = currentState.ToString(); // Update debug info in inspector
        currentState.EnterState(); // Enter the new state

        Debug.Log($"Switched from {lastActiveState} to {currentActiveState}");
    }






    #region State Machine Update Calls

    private void FixedUpdate()
    {
        // Handle physics updates in the current active state
        currentState.FixedUpdateState();

    }


    private void Update()
    {
        // Handle regular frame updates in the current active state
        currentState.UpdateState();
    }


    private void LateUpdate()
    {
        // Handle late frame updates in the current active state
        currentState.LateUpdateState();
    }
    #endregion

    #region Button Call Methods

    public void Pause()
    {
        if (currentState != gameState_Gameplay)
            return;

        if(currentState == gameState_Gameplay)
        {
            SwitchToState(gameState_Paused);
            return;
        }
    }

    public void Resume()
    {
        if (currentState != gameState_Paused)
            return;

        if (currentState == gameState_Paused)
        {
            SwitchToState(gameState_Gameplay);
            return;
        }
    }

    public void Play()
    {
        SwitchToState(gameState_Gameplay);
    }

    public void MainMenu()
    {
        SwitchToState(gameState_MainMenu);
    }


    public void Quit()
    {
        Application.Quit();

    }






    #endregion


}
