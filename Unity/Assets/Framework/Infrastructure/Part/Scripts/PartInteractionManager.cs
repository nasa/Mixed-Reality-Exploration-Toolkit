// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

public class PartInteractionManager : MonoBehaviour
{
    //public VRTK.VRTK_InteractGrab leftGrabScript, rightGrabScript;

	void Start ()
    {
        DisableGrabbing();
	}

    public void EnableGrabbing()
    {
        //leftGrabScript.enabled = rightGrabScript.enabled = true;
    }

    public void DisableGrabbing()
    {
        //leftGrabScript.enabled = rightGrabScript.enabled = false;
    }
}