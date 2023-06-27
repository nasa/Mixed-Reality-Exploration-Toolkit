// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms
{
    /// <remarks>
    /// History:
    /// 22 Sept 2022: Updated to work with the LocomotionManager, replaced use of enable
    ///               with the use of the scaleEnabled property, and now used a project
    ///               prefab for the indicator. (JCH)
    /// </remarks>
    ///
    /// <summary>
    /// Provides simple scaling of a target object based on controller motion when the
    /// specified buttons are pressed on both controllers simultaneously.
    /// </summary>
    public class ScaleObjectTransform : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ScaleObjectTransform);

        public Text scaleLabel;

        public InputHand leftController;
        public InputHand rightController;
        private bool leftControllerPressed;
        private bool rightControllerPressed;
        private float controllerDist = -1;

        [Tooltip("Enables two controller scaling of target object")]
        public bool scalingEnabled = true;
        [Tooltip("Game object to translate and scale")]
        public GameObject targetGameObject = null;
        [Tooltip("Game object to show as an indicator of the relative scaling position.")]
        public GameObject relativeScaleIndicator = null;
        [Tooltip("Scale step multiplier to exagerate (values > 1) or reduce scaling (values < 1)")]
        public float stepMultiplier = 1.0f;
        [Tooltip("The maximum resulting scale magnitude allowed")]
        public float maxScaleLimit = 1.0f;

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

            if ((leftController == null) && (MRET.InputRig.leftHand != null))
            {
                leftController = MRET.InputRig.leftHand;
            }
            if (leftController != null)
            {
                leftController.grabBeginEvent.AddListener(HandleControllerPressed);
                leftController.grabCompleteEvent.AddListener(HandleControllerReleased);
            }

            if ((rightController == null) && (MRET.InputRig.rightHand != null))
            {
                rightController = MRET.InputRig.rightHand;
            }
            if (rightController != null)
            {
                rightController.grabBeginEvent.AddListener(HandleControllerPressed);
                rightController.grabCompleteEvent.AddListener(HandleControllerReleased);
            }

            if (targetGameObject == null)
            {
                targetGameObject = gameObject;
            }
            if (relativeScaleIndicator != null)
            {
                // Hide
                relativeScaleIndicator.SetActive(false);
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            if (leftController != null)
            {
                leftController.grabBeginEvent.RemoveListener(HandleControllerPressed);
                leftController.grabCompleteEvent.RemoveListener(HandleControllerReleased);
            }
            if (rightController != null)
            {
                rightController.grabBeginEvent.RemoveListener(HandleControllerPressed);
                rightController.grabCompleteEvent.RemoveListener(HandleControllerReleased);
            }
        }

        /// <summary>
        /// When grips are released, turn booleans off and hide the scale indicator prefab
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

            if (relativeScaleIndicator != null)
            {
                relativeScaleIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// When one controller is pressed, determine if the other is pressed.  
        /// If both pressed, show the scale indicator prefab and enable booleans.
        /// </summary>
        public void HandleControllerPressed(InputHand hand)
        {
            if (!scalingEnabled)
            {
                return;
            }

            if (hand.handedness == InputHand.Handedness.left)
            {
                leftControllerPressed = true;
                if (rightControllerPressed)
                {
                    controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                    Vector3 scaleRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);

                    if (relativeScaleIndicator != null)
                    {
                        relativeScaleIndicator.transform.position = scaleRefPoint;
                        relativeScaleIndicator.SetActive(true);
                    }
                }
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                rightControllerPressed = true;
                if (leftControllerPressed)
                {
                    controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                    Vector3 scaleRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);

                    if (relativeScaleIndicator != null)
                    {
                        relativeScaleIndicator.transform.position = scaleRefPoint;
                        relativeScaleIndicator.SetActive(true);
                    }
                }
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (scalingEnabled && (leftControllerPressed == true) && (rightControllerPressed == true) && (targetGameObject != null))
            {
                IInteractable leftSceneObject = leftController.GetComponentInChildren<IInteractable>();
                IInteractable rightSceneObject = leftController.GetComponentInChildren<IInteractable>();

                // Don't scale if interacting with an object.
                if ((leftSceneObject == null) && (rightSceneObject == null))
                {
                    Vector3 scaleRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                    float newDistance = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                    float scaleFactor = (newDistance / controllerDist) - 1.0f;
                    scaleFactor = 1.0f + (scaleFactor * stepMultiplier);
                    float scale = targetGameObject.transform.localScale.x * scaleFactor;

                    // Update text and location of scale point indicator
                    if (relativeScaleIndicator != null)
                    {
                        relativeScaleIndicator.transform.position = scaleRefPoint;
                        relativeScaleIndicator.transform.LookAt(MRET.InputRig.head.transform); // Ensure indicator faces the head
                        if (scaleLabel != null)
                        {
                            if (scale > maxScaleLimit)
                            {
                                scaleLabel.text = Math.Round((decimal)(maxScaleLimit * 100f), 1).ToString() + "%";
                            }
                            else
                            {
                                scaleLabel.text = Math.Round((decimal)(scale * 100f), 1).ToString() + "%";
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
}