// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

public class InteractableLabel : SceneObject
{
    public override void BeginGrab(InputHand hand)
    {
        base.BeginGrab(hand);
        if (gameObject.GetComponent<Rigidbody>())
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        }
    }

    public override void EndGrab(InputHand hand)
    {
        base.EndGrab(hand);
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
    }
}