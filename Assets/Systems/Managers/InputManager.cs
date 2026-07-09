using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, Inputs.IPlayerActions, IManager
{
    // Static singleton instance
    public static InputManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    private Inputs inputs;

    // Cached References
    private PlayerController playerController => PlayerController.Instance;


    void Awake()
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
        await Task.Yield();

        try
        {
            // If an old instance somehow exists, clean it up first
            if (inputs != null)
            {
                inputs.Player.Disable();
                inputs.Dispose();
            }

            inputs = new Inputs();
            inputs.Player.SetCallbacks(this);
            inputs.Player.Enable();

            playerController.InitializeInput();
        }
        catch (Exception ex)
        {
            Debug.LogError($"{Name}: Initialization failed — {ex.Message}");

            // CRITICAL: Clean up if it failed AFTER inputs was created and enabled
            if (inputs != null)
            {
                inputs.Player.Disable();
                inputs.Dispose();
                inputs = null;
            }
            return false;
        }

        return true;
    }


    #region Input Events

    // Events that are triggered when input activity is detected

    public event Action<Vector2> MoveInputEvent;
    public event Action<Vector2> LookInputEvent;

    public event Action<InputAction.CallbackContext> JumpInputEvent;
    public event Action<InputAction.CallbackContext> CrouchInputEvent;
    public event Action<InputAction.CallbackContext> SprintInputEvent;
    public event Action<InputAction.CallbackContext> InteractInputEvent;

    #endregion


    #region Input Callbacks

    // Handles input action callbacks and dispatches input data to listeners.

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInputEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInputEvent?.Invoke(context.ReadValue<Vector2>());
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        JumpInputEvent?.Invoke(context);

        /* old version without passing context
        if(context.started) {JumpStartedInputEvent?.Invoke();}
        if(context.performed) {JumpPerformedInputEvent?.Invoke(); }
        if(context.canceled) {JumpCanceledInputEvent?.Invoke(); }
        */
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        CrouchInputEvent?.Invoke(context);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        SprintInputEvent?.Invoke(context);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        InteractInputEvent?.Invoke(context);
    }


    #endregion

    void OnEnable()
    {
        if (inputs != null)
        {
            inputs.Player.Enable();
        }
    }

    void OnDestroy()
    {
        if (inputs != null)
        {
            inputs.Player.Disable();
            inputs.Dispose();
            inputs = null;
        }
    }
}

 

