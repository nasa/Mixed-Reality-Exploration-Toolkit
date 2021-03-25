// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

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

    public InputHand hand;
	public GameObject leftController;
	public GameObject rightController;
	private static bool leftControllerPressed;
	private static bool rightControllerPressed;
	private static Vector3 referenceDirection;
	private static Vector3 rotationRefPoint;

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

	public void HandleControllerReleased()
	{
		if (hand.handedness == InputHand.Handedness.left)
		{
			leftControllerPressed = false;
		}
		else if (hand.handedness == InputHand.Handedness.right)
		{
			rightControllerPressed = false;
		}

		if (relativeRotationIndicator != null)
		{
			relativeRotationIndicator.SetActive (false);
		}
	}

	public void HandleControllerPressed()
	{
		if (hand.handedness == InputHand.Handedness.left)
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
		else if (hand.handedness == InputHand.Handedness.right)
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
