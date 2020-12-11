using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


// NOTE: Belongs in interface/scripts once completed

public class GripMeasure : MonoBehaviour {

    public enum ActivationButton
    {
        TriggerPress,
        GripPress,
    }

    // if ray casts are active when gripping (trigger + grip) then set z positions to the same plane and measure from rays if no object is hit

    GameObject leftController;
    GameObject rightController;
    private static bool leftControllerPressed;
    private static bool rightControllerPressed;
    private static float controllerDist = -1;

    private static Vector3 InitialDist;



    private VRTK_ObjectTooltip distanceLabel;
    private ControllerInteractionEventHandler controllerPressedEventHandler;
    private ControllerInteractionEventHandler controllerReleasedEventHandler;

    [Tooltip("Enables measurement between two controllers or their rays")]
    public bool measureEnabled = true;
    [Tooltip("Button that will activate the measuring tool.")]
    public ActivationButton activationButton = ActivationButton.GripPress;
    [Tooltip("Game object to resize, if empty the tool will just measure")]
    public GameObject targetGameObject = null;
    [Tooltip("Game object to show as an indicator of the distance.")]
    public GameObject relativePositionIndicator = null;
    
    protected virtual void Awake()
    {
        VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
    }

    protected virtual void OnEnable()
    {
        ResetConfiguration();
    }

    protected virtual void OnDestroy()
    {
        VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
    }

    public virtual void ResetConfiguration()
    {
        if (controllerPressedEventHandler == null)
        {
            controllerPressedEventHandler = new VRTK.ControllerInteractionEventHandler(handleControllerPressed);
            if (activationButton == ActivationButton.TriggerPress)
            {
                GetComponent<VRTK.VRTK_ControllerEvents>().TriggerPressed += controllerPressedEventHandler;
            }
            if (activationButton == ActivationButton.GripPress)
            {
                GetComponent<VRTK.VRTK_ControllerEvents>().GripPressed += controllerPressedEventHandler;
            }
        }

        if (controllerReleasedEventHandler == null)
        {
            controllerReleasedEventHandler = new VRTK.ControllerInteractionEventHandler(handleControllerReleased);
            if (activationButton == ActivationButton.TriggerPress)
            {
                GetComponent<VRTK.VRTK_ControllerEvents>().TriggerReleased += controllerReleasedEventHandler;
            }
            if (activationButton == ActivationButton.GripPress)
            {
                GetComponent<VRTK.VRTK_ControllerEvents>().GripReleased += controllerReleasedEventHandler;
            }
        }

        leftController = VRTK_DeviceFinder.GetControllerLeftHand();
        rightController = VRTK_DeviceFinder.GetControllerRightHand();
    }

    void Start()
    {
        if (relativePositionIndicator != null)
        {
            // TODO use a prefab instead and instatiate it?
            //relativePositionIndicator = Instantiate (relativePositionIndicator);
            relativePositionIndicator.SetActive(false);
            distanceLabel = relativePositionIndicator.GetComponentInChildren<VRTK_ObjectTooltip>();
        }
    }

    private void handleControllerReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (VRTK_DeviceFinder.GetControllerIndex(leftController) == e.controllerReference.index)
        {
            leftControllerPressed = false;
        }
        else if (VRTK_DeviceFinder.GetControllerIndex(rightController) == e.controllerReference.index)
        {
            rightControllerPressed = false;
        }

        if (relativePositionIndicator != null)
        {
            relativePositionIndicator.SetActive(false);
        }
    }

    private void handleControllerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (VRTK_DeviceFinder.GetControllerIndex(leftController) == e.controllerReference.index)
        {
            leftControllerPressed = true;
            if (rightControllerPressed)
            {
                controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                InitialDist = leftController.transform.position - rightController.transform.position;
                Vector3 distRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                if (relativePositionIndicator != null)
                {
                    relativePositionIndicator.transform.position = distRefPoint;
                    relativePositionIndicator.SetActive(true);
                }
            }
        }
        else if (VRTK_DeviceFinder.GetControllerIndex(rightController) == e.controllerReference.index)
        {
            rightControllerPressed = true;
            if (leftControllerPressed)
            {
                controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                InitialDist = leftController.transform.position - rightController.transform.position;
                Vector3 distRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                if (relativePositionIndicator != null)
                {
                    relativePositionIndicator.transform.position = distRefPoint;
                    relativePositionIndicator.SetActive(true);
                }
            }
        }
    }

	// Update is called once per frame
	void Update ()
    {
        if(measureEnabled && (leftControllerPressed==true) && (rightController == true))
        {
            Vector3 distanceRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
            float newDistance = Vector3.Distance(leftController.transform.position, rightController.transform.position);
            float deltaDistance = newDistance - controllerDist;
            Vector3 newDistanceVector = leftController.transform.position - rightController.transform.position;

            if (relativePositionIndicator != null)
            {
                relativePositionIndicator.transform.position = distanceRefPoint;

                if(distanceLabel != null)
                {
                    distanceLabel.UpdateText(Math.Round((decimal)deltaDistance, 3).ToString());
                }
            }

            if(newDistance > 0)
            {
            
            if (targetGameObject != null)
                {
                    RectTransform Size = targetGameObject.GetComponentInChildren<RectTransform>();
                    BoxCollider Box = targetGameObject.GetComponentInChildren<BoxCollider>();
                    ZenFulcrum.EmbeddedBrowser.Browser htmlBrowser = targetGameObject.GetComponentInChildren<ZenFulcrum.EmbeddedBrowser.Browser>();

                    // maybe display a axis to help the user

                    string axis = GetAxis(newDistanceVector, InitialDist);
                    if(axis == "x")
                    {
                        if(Size.sizeDelta.x >= 0.1f && Size.sizeDelta.x <= 5)
                        {
                            Size.sizeDelta = new Vector2(Size.sizeDelta.x + deltaDistance / 50, Size.sizeDelta.y);
                            Box.size = new Vector3(Box.size.x + deltaDistance / 50, Box.size.y, Box.size.z);
                            //htmlBrowser.Resize(htmlBrowser.GetWidth()+(int)deltaDistance*1024,htmlBrowser.GetHeight());
                        }
                        else if(Size.sizeDelta.x > 5)
                        {
                            Size.sizeDelta = new Vector2(5, Size.sizeDelta.y);
                            Box.size = new Vector3(5, Box.size.y, Box.size.z);
                           // htmlBrowser.Resize(5* 1024, htmlBrowser.GetHeight());
                        }
                        else
                        {
                            Size.sizeDelta = new Vector2(0.1f, Size.sizeDelta.y);
                            Box.size = new Vector3(0.1f, Box.size.y, Box.size.z);
                           // htmlBrowser.Resize(103, htmlBrowser.GetHeight());
                        }

                    }
                    else if(axis == "y")
                    {
                        if(Size.sizeDelta.y >= 0.9f && Size.sizeDelta.y <= 5)
                        {
                            Size.sizeDelta = new Vector2(Size.sizeDelta.x, Size.sizeDelta.y + deltaDistance / 50);
                            Box.size = new Vector3(Box.size.x, Box.size.y + deltaDistance / 50, Box.size.z);
                            //htmlBrowser.Resize(htmlBrowser.GetWidth(), htmlBrowser.GetHeight()+(int)deltaDistance * 1024);
                        }
                        else if(Size.sizeDelta.y>5)
                        {
                            Size.sizeDelta = new Vector2(Size.sizeDelta.x, 5);
                            Box.size = new Vector3(Box.size.x, 5, Box.size.z);
                            //htmlBrowser.Resize(htmlBrowser.GetWidth(), 5 * 1024);
                        }
                        else
                        {
                            Size.sizeDelta = new Vector2(Size.sizeDelta.x, 0.9f);
                            Box.size = new Vector3(Box.size.x, 0.9f, Box.size.z);
                            //htmlBrowser.Resize(htmlBrowser.GetWidth(), 103);
                        }
                    }
                }
            else
                {
                    //Treat it like a measuring tape
                    
                    //Debug.DrawLine()
                }
            }
        }
	}

    public string GetAxis(Vector3 newDistance, Vector3 OldDistance) // can be used to get the axis the had the most change?
    {
        Vector3 Delta = newDistance - OldDistance;
        
        if(Math.Abs(Delta.x) > Math.Abs(Delta.y))
        {
            return "x";
        }
        else 
        {
            return "y";
        }
    }
}
