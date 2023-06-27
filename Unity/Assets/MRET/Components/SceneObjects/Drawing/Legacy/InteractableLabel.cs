// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy
{
    public class InteractableLabel : InteractableSceneObject<InteractableSceneObjectType>
    {
        protected override void AfterBeginGrab(InputHand hand)
        {
            base.AfterBeginGrab(hand);
            if (gameObject.GetComponent<Rigidbody>())
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().useGravity = false;
            }
        }

        protected override void AfterEndGrab(InputHand hand)
        {
            base.AfterEndGrab(hand);
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        }
    }
}