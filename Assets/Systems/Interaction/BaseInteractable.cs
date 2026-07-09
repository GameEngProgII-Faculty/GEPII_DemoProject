using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Highlight Effect Settings")]
    protected Outline outline;

    protected bool isFocused = false;

    public string interactionPromptText;



    protected virtual void Awake()
    {
        if(gameObject.layer != LayerMask.NameToLayer("Interactable"))
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        #region Initalize Hightlight Effect
        outline = GetComponent<Outline>();
        outline.OutlineColor = Color.green;
        outline.OutlineWidth = 10f;
        outline.enabled = false;
        #endregion
    }

    public string GetInteractionPrompt()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnInteract()
    {
        Debug.Log("BaseInteracatable default logic called");
        Debug.Log("interacted with " + gameObject.name);
    }

    public void SetFocus(bool focused)
    {
        if (isFocused == focused) return;

        isFocused = focused;    

        if (focused)
        {
           // perform the focus logic
           outline.enabled = true;
        }
        else
        {
            // Lose focus logic
            outline.enabled = false;
        }
    }


}
