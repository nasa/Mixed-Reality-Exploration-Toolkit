// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    // FIXME: This script is too closely tied to the controllers, so should
    // be reevaluated for relocation/reimplementation. There could be issues
    // with locomotion, and some of the functionality is very similar to the
    // rotation and scaling transforms in locomotion.
    public class GripMeasure : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(GripMeasure);

        public Text distanceLabel;
        public InputHand leftController;
        public InputHand rightController;

        public enum ActivationButton
        {
            TriggerPress,
            GripPress,
        }

        // if ray casts are active when gripping (trigger + grip) then set z positions to the same plane and measure from rays if no object is hit

        private bool leftControllerPressed;
        private bool rightControllerPressed;
        private float controllerDist = -1;

        private Vector3 initialDist;

        [Tooltip("Enables measurement between two controllers or their rays")]
        public bool measureEnabled = true;
        [Tooltip("Button that will activate the measuring tool.")]
        public ActivationButton activationButton = ActivationButton.GripPress;
        [Tooltip("Game object to resize, if empty the tool will just measure")]
        public GameObject targetGameObject = null;
        [Tooltip("Game object to show as an indicator of the distance.")]
        public GameObject relativePositionIndicator = null;

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
                leftController.grabBeginEvent.AddListener(OnControllerPressed);
                leftController.grabCompleteEvent.AddListener(handleControllerReleased);
            }

            if ((rightController == null) && (MRET.InputRig.rightHand != null))
            {
                rightController = MRET.InputRig.rightHand;
            }
            if (rightController != null)
            {
                rightController.grabBeginEvent.AddListener(OnControllerPressed);
                rightController.grabCompleteEvent.AddListener(handleControllerReleased);
            }

            if (targetGameObject == null)
            {
                targetGameObject = gameObject;
            }
            if (relativePositionIndicator != null)
            {
                // Hide
                relativePositionIndicator.SetActive(false);
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            if (leftController != null)
            {
                leftController.grabBeginEvent.RemoveListener(OnControllerPressed);
                leftController.grabCompleteEvent.RemoveListener(handleControllerReleased);
            }
            if (rightController != null)
            {
                rightController.grabBeginEvent.RemoveListener(OnControllerPressed);
                rightController.grabCompleteEvent.RemoveListener(handleControllerReleased);
            }
        }

        public void handleControllerReleased(InputHand hand)
        {
            if (hand.handedness == InputHand.Handedness.left)
            {
                leftControllerPressed = false;
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                rightControllerPressed = false;
            }

            if (relativePositionIndicator != null)
            {
                relativePositionIndicator.SetActive(false);
            }
        }

        public void OnControllerPressed(InputHand hand)
        {
            if (hand.handedness == InputHand.Handedness.left)
            {
                leftControllerPressed = true;
                if (rightControllerPressed)
                {
                    controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                    initialDist = leftController.transform.position - rightController.transform.position;
                    Vector3 distRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                    if (relativePositionIndicator != null)
                    {
                        relativePositionIndicator.transform.position = distRefPoint;
                        relativePositionIndicator.SetActive(true);
                    }
                }
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                rightControllerPressed = true;
                if (leftControllerPressed)
                {
                    controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                    initialDist = leftController.transform.position - rightController.transform.position;
                    Vector3 distRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                    if (relativePositionIndicator != null)
                    {
                        relativePositionIndicator.transform.position = distRefPoint;
                        relativePositionIndicator.SetActive(true);
                    }
                }
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (measureEnabled && (leftControllerPressed == true) && (rightController == true))
            {
                Vector3 distanceRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                float newDistance = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                float deltaDistance = newDistance - controllerDist;
                Vector3 newDistanceVector = leftController.transform.position - rightController.transform.position;

                if (relativePositionIndicator != null)
                {
                    relativePositionIndicator.transform.position = distanceRefPoint;

                    if (distanceLabel != null)
                    {
                        distanceLabel.text = Math.Round((decimal)deltaDistance, 3).ToString();
                    }
                }

                if (newDistance > 0)
                {
                    if (targetGameObject != null)
                    {
                        RectTransform Size = targetGameObject.GetComponentInChildren<RectTransform>();
                        BoxCollider Box = targetGameObject.GetComponentInChildren<BoxCollider>();

                        // maybe display a axis to help the user
                        string axis = GetAxis(newDistanceVector, initialDist);
                        if (axis == "x")
                        {
                            if (Size.sizeDelta.x >= 0.1f && Size.sizeDelta.x <= 5)
                            {
                                Size.sizeDelta = new Vector2(Size.sizeDelta.x + deltaDistance / 50, Size.sizeDelta.y);
                                Box.size = new Vector3(Box.size.x + deltaDistance / 50, Box.size.y, Box.size.z);
                                //htmlBrowser.Resize(htmlBrowser.GetWidth()+(int)deltaDistance*1024,htmlBrowser.GetHeight());
                            }
                            else if (Size.sizeDelta.x > 5)
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
                        else if (axis == "y")
                        {
                            if (Size.sizeDelta.y >= 0.9f && Size.sizeDelta.y <= 5)
                            {
                                Size.sizeDelta = new Vector2(Size.sizeDelta.x, Size.sizeDelta.y + deltaDistance / 50);
                                Box.size = new Vector3(Box.size.x, Box.size.y + deltaDistance / 50, Box.size.z);
                                //htmlBrowser.Resize(htmlBrowser.GetWidth(), htmlBrowser.GetHeight()+(int)deltaDistance * 1024);
                            }
                            else if (Size.sizeDelta.y > 5)
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

            if (Math.Abs(Delta.x) > Math.Abs(Delta.y))
            {
                return "x";
            }
            else
            {
                return "y";
            }
        }
    }
}