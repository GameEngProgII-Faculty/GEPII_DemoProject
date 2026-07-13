using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, Inputs.IPlayerActions
{
    // Static singleton instance
    public static InputManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    private Inputs inputs;

    // Cached References
    private PlayerController playerController => PlayerController.Instance;




    #region Input Events

    // ====== INPUT EVENTS ======

    // Value-based AXIS INPUTS (continuous detection)
    // Continuous events that forward a Vector2 representing input direction or magnitude.
    public event Action<Vector2> MoveInputEvent;
    public event Action<Vector2> LookInputEvent;


    // State-based BUTTON INPUTS (Triggers & Phases)
    // Contextual events that forward the state context (started, performed, canceled).
    public event Action<InputAction.CallbackContext> JumpInputEvent;
    public event Action<InputAction.CallbackContext> CrouchInputEvent;
    public event Action<InputAction.CallbackContext> InteractInputEvent;
    public event Action<InputAction.CallbackContext> SprintInputEvent;

    #endregion

    void Awake()
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

        inputs = new Inputs();
        inputs.Player.SetCallbacks(this);

        #endregion

        Debug.Log($"{GetType().Name}: Initialized");
    }


    void OnEnable()
    {
        inputs?.Player.Enable();
    }

    void OnDisable()
    {
        inputs?.Player.Disable();
    }

    #region Input System Callbacks

    // === Read Value Inputs ===

    // Called by the Input System when movement input is performed or changed.
    // Triggers MoveInputEvent and forwards the context(Vector2 value) to all subscribers.
    public void OnMove(InputAction.CallbackContext context) => MoveInputEvent?.Invoke(context.ReadValue<Vector2>());

    // Called by the Input System when look input is performed or changed.
    // Triggers LookInputEvent and forwards the context(Vector2 value) to all subscribers.
    public void OnLook(InputAction.CallbackContext context) => LookInputEvent?.Invoke(context.ReadValue<Vector2>());


    // === Action State Inputs ===

    // Called by the Input System when the jump action state changes.
    // Triggers JumpInputEvent and forwards the action state context to all subscribers.
    public void OnJump(InputAction.CallbackContext context) => JumpInputEvent?.Invoke(context);

    // Called by the Input System when the crouch action state changes.
    // Triggers CrouchInputEvent and forwards the action state context to all subscribers.
    public void OnCrouch(InputAction.CallbackContext context) => CrouchInputEvent?.Invoke(context);

    // Called by the Input System when the interact action state changes.
    // Triggers InteractInputEvent and forwards the action state context to all subscribers.
    public void OnInteract(InputAction.CallbackContext context) => InteractInputEvent?.Invoke(context);

    // Called by the Input System when the sprint action state changes.
    // Triggers SprintInputEvent and forwards the action state context to all subscribers.
    public void OnSprint(InputAction.CallbackContext context) => SprintInputEvent?.Invoke(context);

    #endregion


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

 

