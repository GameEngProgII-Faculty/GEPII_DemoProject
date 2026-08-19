using UnityEngine;
using UnityEngine.InputSystem;

public class GameStateManager : MonoBehaviour
{
    // Singleton instance of GameManager for global access
    public static GameStateManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    [Header("Debug (read only)")]
    [SerializeField] private string currentActiveState;
    [SerializeField] private string lastActiveState;

    // Cached shortcut references
    InputManager inputManager => InputManager.Instance;

    // Private variables to store state information
    private IState currentState;  // Current active state
    private IState lastState;     // Last active state (kept private for encapsulation)

    // Cached shortcut references to the state singletons.
    GameState_MainMenu gameState_MainMenu => GameState_MainMenu.Instance;
    GameState_Gameplay gameState_Gameplay => GameState_Gameplay.Instance;
    GameState_Paused gameState_Paused => GameState_Paused.Instance;
    GameState_BootLoad gameState_BootLoad => GameState_BootLoad.Instance;
    GameState_Loading gameState_Loading => GameState_Loading.Instance;
    GameState_Inventory gameState_Inventory => GameState_Inventory.Instance;

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

        Debug.Log($"{GetType().Name}: Initialized");

        currentState = gameState_BootLoad;
        currentActiveState = currentState.ToString();
        currentState.EnterState();
    }

    private void Start()
    {
        if (inputManager == null) Debug.Log("ERRRRRRRRR");

        // Subscribe to necessary input events
        inputManager.OnPauseInputEvent += HandlePauseInput;
        inputManager.OnInventoryInputEvent += HandleInventoryInput;
    }

    // True while Gameplay is the active state - lets systems that run independently of the
    // state machine (e.g. InteractionManager's world raycast) know when they should be live.
    public bool IsGameplayActive => currentState == gameState_Gameplay;

    private void OnDestroy()
    {
        // Unsubscribe from necessary input events
        inputManager.OnPauseInputEvent -= HandlePauseInput;
        inputManager.OnInventoryInputEvent -= HandleInventoryInput;
 
    }

    public void SwitchToState(IState newState)
    {
        Debug.Log($"Switching from {lastActiveState} to {currentActiveState}");


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


    #region Input Handling

    private void HandlePauseInput(InputAction.CallbackContext context)
    {
        // ignore input for anything except context.Performed
        if (context.phase != InputActionPhase.Performed) return;

        if (currentState == gameState_Gameplay)
        {
            SwitchToState(gameState_Paused);
        }
        else if (currentState == gameState_Paused || currentState == gameState_Inventory)
        {
            SwitchToState(gameState_Gameplay);
        }
    }

    private void HandleInventoryInput(InputAction.CallbackContext context)
    {
        // ignore input for anything except context.Performed
        if (context.phase != InputActionPhase.Performed) return;

        if (currentState == gameState_Gameplay)
        {
            SwitchToState(gameState_Inventory);
        }
        else if (currentState == gameState_Inventory)
        {
            SwitchToState(gameState_Gameplay);
        }
    }

    #endregion

    public void ReturnToGameplay()
    {
        if (currentState != gameState_Paused && currentState != gameState_Inventory)
            return;

        SwitchToState(gameState_Gameplay);
    }
    

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








}
