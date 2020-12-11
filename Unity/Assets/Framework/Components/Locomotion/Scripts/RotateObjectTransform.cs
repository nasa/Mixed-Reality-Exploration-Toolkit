using UnityEngine;
using VRTK;

/// <summary>
/// Provides simple rotation around a point of a target object based on controller motion when the specified buttons are pressed on both 
/// controllers simultaneously. Script should be placed as a child of both left and right controllers.
/// </summary>
public class RotateObjectTransform : MonoBehaviour 
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
	private static Vector3 referenceDirection;
	private static Vector3 rotationRefPoint;
	private ControllerInteractionEventHandler controllerPressedEventHandler;
	private ControllerInteractionEventHandler controllerReleasedEventHandler;

	[Tooltip("Enables two controller rotation of target object")]
	public bool rotationEnabled = true;
	[Tooltip("Button that will activate the rotation of an object.")]
	public ActivationButton activationButton = ActivationButton.GripPress;
	[Tooltip("Game object to rotate")]
	public GameObject targetGameObject = null;
	[Tooltip("Game object to show as an indicator of the relative rotation position.")]
	public GameObject relativeRotationIndicator = null;
	[Tooltip("Enables rotation around the X axis")]
	public bool xEnabled = false;
	[Tooltip("Enables rotation around the Y axis")]
	public bool yEnabled = false;
	[Tooltip("Enables rotation around the Z axis")]
	public bool zEnabled = false;
	[Tooltip("Translation step multiplier to exagerate or limit rotation")]
	public float stepMultiplier = 1.0f;

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

		if (controllerReleasedEventHandler == null)
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

		if (relativeRotationIndicator != null)
		{
			relativeRotationIndicator.SetActive (false);
		}
	}

	private void handleControllerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
	{
		if (VRTK_DeviceFinder.GetControllerIndex(leftController) == e.controllerReference.index)
		{
			leftControllerPressed = true;
			if (rightControllerPressed) 
			{
				rotationRefPoint = Vector3.Lerp (leftController.transform.position, rightController.transform.position, 0.5f);
				referenceDirection = leftController.transform.position - rightController.transform.position;

				if (relativeRotationIndicator != null)
				{
					relativeRotationIndicator.transform.position = rotationRefPoint;
					relativeRotationIndicator.SetActive (true);
				}
			}
		}
		else if (VRTK_DeviceFinder.GetControllerIndex(rightController) == e.controllerReference.index)
		{
			rightControllerPressed = true;
			if (leftControllerPressed) 
			{
				rotationRefPoint = Vector3.Lerp (leftController.transform.position, rightController.transform.position, 0.5f);
				referenceDirection = leftController.transform.position - rightController.transform.position;

				if (relativeRotationIndicator != null)
				{
					relativeRotationIndicator.transform.position = rotationRefPoint;
					relativeRotationIndicator.SetActive (true);
				}
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (leftControllerPressed && rightControllerPressed && rotationEnabled && (targetGameObject != null))
		{
			rotationRefPoint = Vector3.Lerp (leftController.transform.position, rightController.transform.position, 0.5f);
			Vector3 controllerDirection = leftController.transform.position - rightController.transform.position;
			Quaternion deltaRotation = Quaternion.FromToRotation(referenceDirection, controllerDirection);
			Vector3 deltaAngles = deltaRotation.eulerAngles * stepMultiplier;
		
			if (xEnabled)
			{
				targetGameObject.transform.RotateAround (rotationRefPoint, Vector3.right, deltaAngles.x);
			}
			if (yEnabled)
			{
				targetGameObject.transform.RotateAround (rotationRefPoint, Vector3.up, deltaAngles.y);
			}
			if (zEnabled)
			{
				targetGameObject.transform.RotateAround (rotationRefPoint, Vector3.forward, deltaAngles.z );
			}

			// Update rotation point indicator
			if (relativeRotationIndicator != null)
			{
				relativeRotationIndicator.transform.position = rotationRefPoint;

				if (xEnabled)
				{
					relativeRotationIndicator.transform.RotateAround (rotationRefPoint, Vector3.right, deltaAngles.x);
				}
				if (yEnabled)
				{
					relativeRotationIndicator.transform.RotateAround (rotationRefPoint, Vector3.up, deltaAngles.y);
				}
				if (zEnabled)
				{
					relativeRotationIndicator.transform.RotateAround (rotationRefPoint, Vector3.forward, deltaAngles.z );
				}
			}
            
			referenceDirection = controllerDirection;
		}
	}
}
