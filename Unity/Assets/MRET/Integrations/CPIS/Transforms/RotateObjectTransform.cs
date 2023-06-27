// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms
{
    /// <remarks>
    /// History:
    /// 22 Sept 2022: Updated to work with the LocomotionManager, replaced use of enable
    ///               with the use of the rotationEnabled property, and now used a project
    ///               prefab for the indicator. (JCH)
    /// </remarks>
    ///
    /// <summary>
    /// Provides simple rotation around a point of a target object based on controller
    /// motion when the specified buttons are pressed on both controllers simultaneously.
    /// </summary>
    public class RotateObjectTransform : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(RotateObjectTransform);
            }
        }

        public GameObject leftController;
        public GameObject rightController;
        private static bool leftControllerPressed;
        private static bool rightControllerPressed;
        private static Vector3 referenceDirection;
        private static Vector3 rotationRefPoint;

        [Tooltip("Enables two controller rotation of target object")]
        public bool rotationEnabled = true;
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

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            if (relativeRotationIndicator != null)
            {
                // Hide
                relativeRotationIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// When grips are released, turn booleans off and hide the rotation indicator prefab
        /// </summary>
        public void HandleControllerReleased(InputHand hand)
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
                relativeRotationIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// When one controller is pressed, determine if the other is pressed.  
        /// If both pressed, show the rotation indicator prefab and enable booleans.
        /// </summary>
        public void HandleControllerPressed(InputHand hand)
        {
            if (!rotationEnabled)
            {
                return;
            }

            if (hand.handedness == InputHand.Handedness.left)
            {
                leftControllerPressed = true;
                if (rightControllerPressed)
                {
                    rotationRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                    referenceDirection = leftController.transform.position - rightController.transform.position;

                    if (relativeRotationIndicator != null)
                    {
                        relativeRotationIndicator.transform.position = rotationRefPoint;
                        relativeRotationIndicator.SetActive(true);
                    }
                }
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                rightControllerPressed = true;
                if (leftControllerPressed)
                {
                    rotationRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                    referenceDirection = leftController.transform.position - rightController.transform.position;

                    if (relativeRotationIndicator != null)
                    {
                        relativeRotationIndicator.transform.position = rotationRefPoint;
                        relativeRotationIndicator.SetActive(true);
                    }
                }
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (leftControllerPressed && rightControllerPressed && rotationEnabled && (targetGameObject != null))
            {
                rotationRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                Vector3 controllerDirection = leftController.transform.position - rightController.transform.position;
                Quaternion deltaRotation = Quaternion.FromToRotation(referenceDirection, controllerDirection);
                Vector3 deltaAngles = deltaRotation.eulerAngles * stepMultiplier;

                if (xEnabled)
                {
                    targetGameObject.transform.RotateAround(rotationRefPoint, Vector3.right, deltaAngles.x);
                }
                if (yEnabled)
                {
                    targetGameObject.transform.RotateAround(rotationRefPoint, Vector3.up, deltaAngles.y);
                }
                if (zEnabled)
                {
                    targetGameObject.transform.RotateAround(rotationRefPoint, Vector3.forward, deltaAngles.z);
                }

                // Update rotation point indicator
                if (relativeRotationIndicator != null)
                {
                    relativeRotationIndicator.transform.position = rotationRefPoint;
                    relativeRotationIndicator.transform.LookAt(MRET.InputRig.head.transform); // Ensure indicator faces the head

                    if (xEnabled)
                    {
                        relativeRotationIndicator.transform.RotateAround(rotationRefPoint, Vector3.right, deltaAngles.x);
                    }
                    if (yEnabled)
                    {
                        relativeRotationIndicator.transform.RotateAround(rotationRefPoint, Vector3.up, deltaAngles.y);
                    }
                    if (zEnabled)
                    {
                        relativeRotationIndicator.transform.RotateAround(rotationRefPoint, Vector3.forward, deltaAngles.z);
                    }
                }

                referenceDirection = controllerDirection;
            }
        }
    }
}