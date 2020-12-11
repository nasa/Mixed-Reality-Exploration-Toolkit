using System;
using UnityEngine;
using VRTK;

/// <summary>
/// Provides simple scaling of a target object based on controller motion when the specified buttons are pressed on both 
/// controllers simultaneously. Script should be placed as a child of both left and right controllers.
/// </summary>
public class ScaleObjectTransform : MonoBehaviour
{
	public enum ActivationButton
	{
		TriggerPress,
		GripPress,
	}

	GameObject leftController;
    GameObject rightController;
    private static bool leftControllerPressed;
    private static bool rightControllerPressed;
    private static float controllerDist = -1;
	private VRTK_ObjectTooltip scaleLabel;
	private ControllerInteractionEventHandler controllerPressedEventHandler;
	private ControllerInteractionEventHandler controllerReleasedEventHandler;

	[Tooltip("Enables two controller scaling of target object")]
	public bool scalingEnabled = true;
	[Tooltip("Button that will activate the scaling of an object.")]
	public ActivationButton activationButton = ActivationButton.GripPress;
	[Tooltip("Game object to translate and scale")]
	public GameObject targetGameObject = null;
	[Tooltip("Game object to show as an indicator of the relative scaling position.")]
	public GameObject relativePositionIndicator = null;
	[Tooltip("Scale step multiplier to exagerate (values > 1) or reduce scaling (values < 1)")]
	public float stepMultiplier = 1.0f;
	[Tooltip("The maximum resulting scale magnitude allowed")]
	public float maxScaleLimit = 1.0f;

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
		// TODO do we have to reregister for these events?
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

		if (controllerReleasedEventHandler  == null)
		{
			controllerReleasedEventHandler = new VRTK.ControllerInteractionEventHandler(handleControllerReleased);
			if (activationButton == ActivationButton.TriggerPress) 
			{
				GetComponent<VRTK.VRTK_ControllerEvents>().TriggerReleased += controllerReleasedEventHandler;
			} 
			if (activationButton == ActivationButton.GripPress) 
			{
				GetComponent<VRTK.VRTK_ControllerEvents> ().GripReleased += controllerReleasedEventHandler;
			}
		}

		leftController = VRTK_DeviceFinder.GetControllerLeftHand();
		rightController = VRTK_DeviceFinder.GetControllerRightHand();
	}

    // Use this for initialization
    void Start()
    {
		if (relativePositionIndicator != null)
		{
			// TODO use a prefab instead and instatiate it?
			//relativePositionIndicator = Instantiate (relativePositionIndicator);
			relativePositionIndicator.SetActive (false);
			scaleLabel = relativePositionIndicator.GetComponentInChildren<VRTK_ObjectTooltip> ();
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
			relativePositionIndicator.SetActive (false);
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
				Vector3 scaleRefPoint = Vector3.Lerp (leftController.transform.position, rightController.transform.position, 0.5f);

				if (relativePositionIndicator != null)
				{
					relativePositionIndicator.transform.position = scaleRefPoint;
					relativePositionIndicator.SetActive (true);
				}
			}
        }
		else if (VRTK_DeviceFinder.GetControllerIndex(rightController) == e.controllerReference.index)
        {
            rightControllerPressed = true;
			if (leftControllerPressed) 
			{
				controllerDist = Vector3.Distance (leftController.transform.position, rightController.transform.position);
				Vector3 scaleRefPoint = Vector3.Lerp (leftController.transform.position, rightController.transform.position, 0.5f);

				if (relativePositionIndicator != null)
				{
					relativePositionIndicator.transform.position = scaleRefPoint;
					relativePositionIndicator.SetActive (true);
				}
			}
        }
    }

    // Update is called once per frame
    void Update()
    {
		if (scalingEnabled && (leftControllerPressed == true) && (rightControllerPressed == true) && (targetGameObject != null))
        {
            // Don't scale if an object is being held.
            if (leftController.GetComponentInChildren<InteractablePart>() == null
                && rightController.GetComponentInChildren<InteractablePart>() == null
                && leftController.GetComponentInChildren<InteractableTool>() == null
                && rightController.GetComponentInChildren<InteractableTool>() == null)
            {

			Vector3 scaleRefPoint = Vector3.Lerp (leftController.transform.position, rightController.transform.position, 0.5f);
			float newDistance = Vector3.Distance(leftController.transform.position, rightController.transform.position);
			float scaleFactor = (newDistance / controllerDist) - 1.0f;
			scaleFactor = 1.0f + (scaleFactor * stepMultiplier);
			float scale = targetGameObject.transform.localScale.x * scaleFactor;

			// Update text and location of scale point indicator
			if (relativePositionIndicator != null)
			{
				relativePositionIndicator.transform.position = scaleRefPoint;
				if (scaleLabel != null)
				{
					if (scale > maxScaleLimit)
					{
						scaleLabel.UpdateText(Math.Round ((decimal)maxScaleLimit, 3).ToString ());
					} 
					else
					{
						scaleLabel.UpdateText(Math.Round ((decimal)scale, 3).ToString ());
					}
				}
			}

                if (scale > 0 && scale <= maxScaleLimit)
                {
                    controllerDist = newDistance;
                    Vector3 offsetPosition = targetGameObject.transform.position - scaleRefPoint;
                    Vector3 newPosition = (offsetPosition * scaleFactor) + scaleRefPoint;

                    // Update transform
                    targetGameObject.transform.localScale = targetGameObject.transform.localScale * scaleFactor;
                    targetGameObject.transform.position = newPosition;
                }
            }
        }
    }
}
