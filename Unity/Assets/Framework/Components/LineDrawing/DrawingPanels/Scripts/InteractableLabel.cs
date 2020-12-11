using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class InteractableLabel : VRTK_InteractableObject
{
    private UndoManager undoManager;
    private Vector3 lastSavedPosition;
    private Quaternion lastSavedRotation;
    private Note note;

    void Start()
    {
     
    }

    protected override void Update()
    {
        base.Update();

       
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectGrabbed(e);
        if(gameObject.GetComponent<Rigidbody>())
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        }
        

    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUngrabbed(e);

        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().useGravity = false;

    }
}