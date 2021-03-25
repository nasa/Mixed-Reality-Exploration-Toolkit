// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework.Interactable;

public class GrabTabScript : Interactable
{
    //private Transform originalParent = null;

    /*public override void OnInteractableObjectTouched(InteractableObjectEventArgs e)
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
    }*/
}