using System;
using System.Threading.Tasks;
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
    private LayerMask interactableLayer;
    [SerializeField] private float interactionDistance = 3f;


    public string DebugCurrentInteractable;

    private bool initialized = false;

    [Header("Interaction Cooldown")]
    [Tooltip("Time in seconds before the player can interact again after a successful interaction. Prevents multiple nteractions on one press")]
    [SerializeField] private float interactionCooldown = 0.1f; // seconds
    private float lastInteractionTime = -Mathf.Infinity;


    // Interface reference used internally
    private IInteractable currentFocusedInteractable;

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

        Debug.Log($"{GetType().Name}: Initialized");
    }
   

    private void Update()
    {
        if (!initialized)
            return;

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

                    // use reference to UI text to pass through Interact Prompt


                }
            } 
        }
        else if (currentFocusedInteractable != null)
        {
            currentFocusedInteractable.SetFocus(false);
            currentFocusedInteractable = null;

            DebugCurrentInteractable = null;
        }

    }

    private void OnInteractInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // Cooldown check to prevent spamming interactions
        if (Time.time - lastInteractionTime < interactionCooldown)
            return; // Still cooling down


        if (context.performed)
        {
            if (currentFocusedInteractable != null)
            {
                currentFocusedInteractable.OnInteract();
            }
        }
       

    }


    private void OnEnable()
    {
        inputManager.InteractInputEvent += OnInteractInput;
    }

    private void OnDestroy()
    {
        inputManager.InteractInputEvent -= OnInteractInput;
    }


}
