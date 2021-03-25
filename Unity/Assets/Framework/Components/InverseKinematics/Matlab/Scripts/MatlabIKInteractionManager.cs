// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

public class MatlabIKInteractionManager : MonoBehaviour
{
    /// <summary>
    /// The hand to aim to follow.
    /// </summary>
    [Tooltip("The hand to aim to follow.")]
    public Transform handRoot;

    public int ikDivisor = 3;

    private MatlabIKScript matlabIKScript, touchingScript;
    private MatlabElbowScript matlabElbowScript, touchingElbowScript;
    private bool controllingArm = false, controllingElbow = false;
    private int sequenceNumber = 0;
    private int ticksSinceLastIK = 0;
	
	void Update()
    {
        if (ticksSinceLastIK++ < ikDivisor)
        {
            return;
        }

        ikDivisor = 0;
        if (controllingArm && matlabIKScript != null)
        {
            matlabIKScript.SetIKPosition(handRoot.position, handRoot.rotation, sequenceNumber++);
        }
        else if (controllingElbow && matlabElbowScript != null)
        {
            matlabElbowScript.SetElbowPosition(handRoot.position, sequenceNumber++);
        }
    }

    void OnEnable()
    {
        controllingArm = controllingElbow = false;
    }

    public void ProcessTouchpadPress()
    {
        if (touchingScript != null)
        {
            if (touchingScript != matlabIKScript)
            {
                touchingScript.enabled = true;
                matlabIKScript = touchingScript;
                touchingScript = null;
                controllingArm = true;
            }
        }
        else if (touchingElbowScript != null)
        {
            if (touchingElbowScript != matlabElbowScript)
            {
                touchingElbowScript.enabled = true;
                matlabElbowScript = touchingElbowScript;
                touchingElbowScript = null;
                controllingElbow = true;
            }
        }
    }

    public void ProcessTouchpadRelease()
    {
        if (matlabIKScript)
        {
            matlabIKScript.enabled = false;
            matlabIKScript = null;
            controllingArm = false;
        }

        if (matlabElbowScript)
        {
            matlabElbowScript.enabled = false;
            matlabElbowScript = null;
            controllingElbow = false;
        }
    }

    public void OnTriggerEnter(Collider touchingObject)
    {
        MatlabIKScript foundScript;
        MatlabElbowScript foundElbowScript;

        if ((foundElbowScript = touchingObject.GetComponentInParent<MatlabElbowScript>()) != null)
        {
            if (foundElbowScript != touchingElbowScript)
            {
                touchingElbowScript = foundElbowScript;
            }
        }
        else if ((foundScript = touchingObject.GetComponentInParent<MatlabIKScript>()) != null)
        {
            if (foundScript != touchingScript)
            {
                touchingScript = foundScript;
            }
        }
    }

    public void OnTriggerExit(Collider previousTouchingObject)
    {
        MatlabIKScript foundScript;
        MatlabElbowScript foundElbowScript;

        if ((foundScript = previousTouchingObject.GetComponentInParent<MatlabIKScript>()) != null)
        {
            if (foundScript == touchingScript)
            {
                touchingScript = null;
            }
        }

        if ((foundElbowScript = previousTouchingObject.GetComponentInParent<MatlabElbowScript>()) != null)
        {
            if (foundElbowScript == touchingElbowScript)
            {
                touchingElbowScript = null;
            }
        }
    }
}