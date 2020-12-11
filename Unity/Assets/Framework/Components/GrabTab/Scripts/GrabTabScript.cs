using UnityEngine;
using VRTK;

public class GrabTabScript : VRTK_InteractableObject
{
    private Transform originalParent = null;

    public override void OnInteractableObjectTouched(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectTouched(e);
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        originalParent = transform.parent.parent;
        transform.parent.SetParent(e.interactingObject.transform);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        transform.parent.SetParent(originalParent);
    }
}