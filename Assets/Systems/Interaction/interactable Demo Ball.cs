using UnityEngine;

public class interactableDemoBall : BaseInteractable
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void OnInteract()
    {
        Debug.Log("Using Logic from Interactable Demo Ball");
    }
}
