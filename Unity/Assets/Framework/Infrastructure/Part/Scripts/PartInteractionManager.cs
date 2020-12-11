using UnityEngine;

public class PartInteractionManager : MonoBehaviour
{
    public VRTK.VRTK_InteractGrab leftGrabScript, rightGrabScript;

	void Start ()
    {
        DisableGrabbing();
	}

    public void EnableGrabbing()
    {
        leftGrabScript.enabled = rightGrabScript.enabled = true;
    }

    public void DisableGrabbing()
    {
        leftGrabScript.enabled = rightGrabScript.enabled = false;
    }
}