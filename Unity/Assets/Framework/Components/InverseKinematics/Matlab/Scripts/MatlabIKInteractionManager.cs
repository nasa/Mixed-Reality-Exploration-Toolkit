using UnityEngine;

public class MatlabIKInteractionManager : MonoBehaviour
{
    public VRTK.VRTK_ControllerEvents controllerEvents;
    public int ikDivisor = 3;

    private MatlabIKScript matlabIKScript, touchingScript;
    private MatlabElbowScript matlabElbowScript, touchingElbowScript;
    private bool controllingArm = false, controllingElbow = false;
    private int sequenceNumber = 0;
    private int ticksSinceLastIK = 0;

    void Start()
    {
        controllerEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadPress);
        controllerEvents.TouchpadReleased += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadRelease);
    }
	
	void Update()
    {
        if (ticksSinceLastIK++ < ikDivisor)
        {
            return;
        }

        ikDivisor = 0;
        if (controllingArm && matlabIKScript != null)
        {
            matlabIKScript.SetIKPosition(transform.parent.position, transform.parent.rotation, sequenceNumber++);
        }
        else if (controllingElbow && matlabElbowScript != null)
        {
            matlabElbowScript.SetElbowPosition(transform.parent.position, sequenceNumber++);
        }
    }

    void OnEnable()
    {
        controllingArm = controllingElbow = false;
    }

    public void ProcessTouchpadPress(object sender, VRTK.ControllerInteractionEventArgs e)
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

    public void ProcessTouchpadRelease(object sender, VRTK.ControllerInteractionEventArgs e)
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