using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    // Singleton instance of GameManager for global access
    public static InteractionManager Instance { get; private set; }

    // Name property for IManager interface implementation
    public string Name => GetType().Name;

    // Cached References
    private InputManager inputManager => InputManager.Instance;

    [Header("Interaction Settings")]
    [SerializeField] private LayerMask interactableLayer = 1 << 6; // "Interactable" layer
    [SerializeField] private float interactionDistance = 3f;


    public string DebugCurrentInteractable;

    private bool initialized = false;

    [Header("Interaction Cooldown")]
    [Tooltip("Time in seconds before the player can interact again after a successful interaction. Prevents multiple nteractions on one press")]
    [SerializeField] private float interactionCooldown = 0.1f; // seconds
    private float lastInteractionTime = -Mathf.Infinity;


    // Interface reference used internally
    private IInteractable currentFocusedInteractable;

    // Read-only access for other systems (e.g. GameState_Gameplay's left-click tool use) to see what's currently focused
    public IInteractable CurrentFocusedInteractable => currentFocusedInteractable;

    private Transform cameraRoot; // Reference to the player's camera root transform

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

        //Debug.Log($"{GetType().Name}: Initialized");
    }

    private void Start()
    {
        cameraRoot = PlayerController.Instance.CameraRoot;
        initialized = cameraRoot != null;

        InventoryManager.Instance.OnSelectedSlotChanged += HandleSelectedSlotChanged;
    }

    // Re-evaluate the prompt when the equipped tool changes (e.g. a required-tool interactable's
    // prompt needs to flip from "Requires Axe" to "[LMB] Harvest Wood" without the focus changing).
    private void HandleSelectedSlotChanged(int slotIndex)
    {
        if (currentFocusedInteractable == null) return;

        UIManager.Instance.ShowInteractPrompt(currentFocusedInteractable.GetInteractionPrompt());
    }


    private void Update()
    {
        if (!initialized)
            return;

        // Don't track/focus world interactables while anything but Gameplay is active (inventory,
        // pause menu, etc.) - otherwise a focused interactable stays "live" behind the UI and
        // pressing Interact there (e.g. to close the inventory) can re-trigger it.
        if (!GameStateManager.Instance.IsGameplayActive)
        {
            if (currentFocusedInteractable != null)
            {
                currentFocusedInteractable.SetFocus(false);
                currentFocusedInteractable = null;
                DebugCurrentInteractable = null;
                UIManager.Instance.HideInteractPrompt();
            }

            return;
        }

        HandleInteractionDetection();
    }

    private void HandleInteractionDetection()
    {
        if (Physics.Raycast(cameraRoot.transform.position, cameraRoot.transform.forward, out RaycastHit hitInfo, interactionDistance, interactableLayer))
        {
            // Debug.Log($"Raycast hit object: " +hitInfo.collider.name);

            // Get the interactable component from the hit object
            IInteractable hitInteractable = hitInfo.collider.GetComponent<IInteractable>();

            if (hitInteractable != null)
            {
                // If it's different from our current focus
                if (hitInteractable != currentFocusedInteractable)
                {
                    // 1. Clear previous focus if we had one
                    if (currentFocusedInteractable != null)
                    {
                        currentFocusedInteractable.SetFocus(false);
                    }

                    // 2. Set new focus
                    currentFocusedInteractable = hitInteractable;
                    currentFocusedInteractable.SetFocus(true);

                    // 3. Get the prompt text from interactable and tell the UI to show it
                    UIManager.Instance.ShowInteractPrompt(currentFocusedInteractable.GetInteractionPrompt());
                }
            }
        }
        else if (currentFocusedInteractable != null)
        {
            currentFocusedInteractable.SetFocus(false);
            currentFocusedInteractable = null;

            DebugCurrentInteractable = null;

            UIManager.Instance.HideInteractPrompt();
        }

    }

    private void OnInteractInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!GameStateManager.Instance.IsGameplayActive) return;

        // Cooldown check to prevent spamming interactions
        if (Time.time - lastInteractionTime < interactionCooldown)
            return; // Still cooling down


        if (context.performed)
        {
            // ResourceNodes only respond to the left-click "use tool" action (see
            // GameState_Gameplay.HandleUseToolInput) - the standard Interact (E) button doesn't apply.
            if (currentFocusedInteractable != null && currentFocusedInteractable is not ResourceNode)
            {
                currentFocusedInteractable.OnInteract();
            }
        }


    }


    private void OnEnable()
    {
        inputManager.OnInteractInputEvent += OnInteractInput;
    }

    private void OnDestroy()
    {
        inputManager.OnInteractInputEvent -= OnInteractInput;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSelectedSlotChanged -= HandleSelectedSlotChanged;
        }
    }


}
