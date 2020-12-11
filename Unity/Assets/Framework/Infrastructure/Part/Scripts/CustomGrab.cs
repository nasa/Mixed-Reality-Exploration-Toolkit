using UnityEngine;
using VRTK;

public class CustomGrab : MonoBehaviour
{
    private VRTK_ControllerEvents touchingCEvents = null;
    private Transform originalParent = null;
    private Vector3 originalScale = Vector3.one;
    private bool grabbed = false;

    void Start()
    {
        foreach (ControllerMenuManager man in FindObjectsOfType<ControllerMenuManager>())
        {
            VRTK_ControllerEvents cEvents = man.GetComponentInParent<VRTK_ControllerEvents>();
            cEvents.GripPressed += new ControllerInteractionEventHandler(OnGripPressed);
            cEvents.GripReleased += new ControllerInteractionEventHandler(OnGripReleased);
        }
    }

    void Update()
    {
        if (grabbed)
        {
            transform.position = touchingCEvents.transform.position;
        }
    }

    private void OnGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (touchingCEvents != null)
        {
            VRTK_ControllerEvents pressedCEvents = e.controllerReference.scriptAlias.GetComponent<VRTK_ControllerEvents>();
            if (pressedCEvents)
            {
                if (pressedCEvents == touchingCEvents)
                {
                    grabbed = true;
                    originalParent = transform.parent;
                    originalScale = transform.localScale;
                    //transform.SetParent(touchingCEvents.transform);
                }
            }
        }
    }

    private void OnGripReleased(object sender, ControllerInteractionEventArgs e)
    {
        VRTK_ControllerEvents releasedCEvents = e.controllerReference.scriptAlias.GetComponent<VRTK_ControllerEvents>();
        if (releasedCEvents && originalParent)
        {
            if (releasedCEvents == touchingCEvents)
            {
                Debug.Log(originalParent);
                //transform.SetParent(originalParent);
                transform.localScale = originalScale;
                originalParent = null;
                grabbed = false;
            }
        }
    }

    private void OnTriggerEnter(Collider touchingObject)
    {
        VRTK_ControllerEvents cEvents = touchingObject.GetComponentInParent<VRTK_ControllerEvents>();
        if (cEvents && !grabbed)
        {
            touchingCEvents = cEvents;
        }
    }

    private void OnTriggerExit(Collider previousTouchingObject)
    {
        VRTK_ControllerEvents cEvents = previousTouchingObject.GetComponentInParent<VRTK_ControllerEvents>();
        if (cEvents)
        {
            if (cEvents == touchingCEvents && !grabbed)
            {
                touchingCEvents = null;
            }
        }
    }
}