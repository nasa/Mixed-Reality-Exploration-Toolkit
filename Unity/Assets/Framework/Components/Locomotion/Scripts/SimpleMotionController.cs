using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/// <summary>
/// Provides simple flying motion based on controller motion when the grip button is pressed. 
/// Script should be placed as a child of the controller to use for grip events. Place a copy under each controller
/// for both left or right controller use.
/// </summary>
public class SimpleMotionController : MonoBehaviour
{
	GameObject leftController;
	GameObject rightController;
	Vector3? initialControllerPosition = null;
	private Transform playArea;
	private ControllerInteractionEventHandler gripPressedEventHandler;
	private ControllerInteractionEventHandler gripReleasedEventHandler;

	// Static class variables shared between controller scripts.
	private static bool leftControllerPressed;
	private static bool rightControllerPressed;

	[Tooltip("Enables one controller moving of camera location")]
	public bool motionEnabled = true;
	[Tooltip("Reverses the camera motion relative to controller motion")]
	public bool joystickMotionMode = false;
	[Tooltip("Motion step multiplier to exagerate (values > 1) or limit movement (values < 1)")]
    public float stepMultiplier;

	protected virtual void Awake()
	{
		VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
	}

	protected virtual void OnEnable()
	{
		print ("SimpleMotionController OnEnable() called");
		ResetConfiguration();
	}

	protected virtual void OnDestroy()
	{
		VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
	}

	public virtual void ResetConfiguration()
	{
		playArea = VRTK_DeviceFinder.PlayAreaTransform();
		if (!playArea)
		{
			VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.SDK_OBJECT_NOT_FOUND, "PlayArea", "Boundaries SDK"));
		}

		// TODO do we have to reregister for these events?
		if (gripPressedEventHandler == null)
		{
			print ("SimpleMotionController adding gripPressedEventHandler subscription");
			gripPressedEventHandler = new VRTK.ControllerInteractionEventHandler(DoGripPressed);
			GetComponent<VRTK.VRTK_ControllerEvents>().GripPressed += gripPressedEventHandler;
		}

		if (gripReleasedEventHandler  == null)
		{
			print ("SimpleMotionController adding gripReleasedEventHandler subscription");
			gripReleasedEventHandler = new VRTK.ControllerInteractionEventHandler(DoGripReleased);
			GetComponent<VRTK.VRTK_ControllerEvents> ().GripReleased += gripReleasedEventHandler;
		}

		leftController = VRTK_DeviceFinder.GetControllerLeftHand();
		rightController = VRTK_DeviceFinder.GetControllerRightHand();
	}

	// Update is called once per frame
	void Update () 
	{
		if ((leftControllerPressed == true) && (rightControllerPressed == true))
		{
			initialControllerPosition = null;
		}
		else if ((leftControllerPressed == true) || (rightControllerPressed == true))
		{
			if (initialControllerPosition.HasValue && motionEnabled)
			{
				Vector3 newPosition = (Vector3)initialControllerPosition;

				if (leftControllerPressed == true)
					newPosition = leftController.transform.position;
				else if (rightControllerPressed == true)
					newPosition = rightController.transform.position;

				float sensitivity = 0.0f;
				Vector3 positionDelta = newPosition - (Vector3)initialControllerPosition;
				//print ("positionDelta.magnitude:" + positionDelta.magnitude);

				// Guard against tracking errors or teleporting while in motion errors
				if (positionDelta.magnitude < 1.5f)
				{
					// Sensitivity is variable based on magnitude of controller motion.
					sensitivity = positionDelta.magnitude;
				}

				// Apply property setting
				sensitivity *= stepMultiplier;

				// Update camera rig position
				if (joystickMotionMode) 
				{
					playArea.position += positionDelta * sensitivity;
					initialControllerPosition += positionDelta * sensitivity;
				} 
				else 
				{
					playArea.position -= positionDelta * sensitivity;
					initialControllerPosition -= positionDelta * sensitivity;
				}
			}                
		}
	}

	private void DoGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
	{
		if (VRTK_DeviceFinder.GetControllerIndex(leftController) == e.controllerReference.index)
		{
			leftControllerPressed = true;
			if (!initialControllerPosition.HasValue)
				initialControllerPosition = leftController.transform.position;
		}
		else if (VRTK_DeviceFinder.GetControllerIndex(rightController) == e.controllerReference.index)
		{
			rightControllerPressed = true;
			if (!initialControllerPosition.HasValue)
				initialControllerPosition = rightController.transform.position;
		}
	}

	private void DoGripReleased(object sender, VRTK.ControllerInteractionEventArgs e)
	{
		if (VRTK_DeviceFinder.GetControllerIndex(leftController) == e.controllerReference.index)
		{
			leftControllerPressed = false;
		}
		else if (VRTK_DeviceFinder.GetControllerIndex(rightController) == e.controllerReference.index)
		{
			rightControllerPressed = false;
		}

		initialControllerPosition = null;
	}

	private void DebugLogger(uint index, string button, string action, VRTK.ControllerInteractionEventArgs e)
	{
		Debug.Log("Controller on index '" + index + "' " + button + " has been " + action
			+ " with a pressure of " + e.buttonPressure + " / trackpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)");
	}
}
