// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms
{
    /// <remarks>
    /// History:
    /// 29 June 2022: Created
    /// 22 Sept 2022: Updated to work with the LocomotionManager, replaced use of enable
    ///               with the use of the scaleEnabled property, and now used a project
    ///               prefab for the indicator. (JCH)
    /// </remarks>
    ///
    /// <summary>
    /// ExplodeObjectTransform
    /// 
    /// Provides user control over object explosions. While both grip buttons are pressed, the user can expand or
    /// contract their wingspan to control explosion distance.
    ///
    /// Author: Sean Letavish
    /// </summary>
    /// 
    public class ExplodeObjectTransform : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ExplodeObjectTransform);
            }
        }

        public Text explodeLabel;

        public GameObject leftController;
        public GameObject rightController;
        private static bool leftControllerPressed;
        private static bool rightControllerPressed;
        private static float controllerDist = -1;
        private static float controllerDistPrev = -1;

        [Tooltip("Enables two controller exploding of target object")]
        public bool explodingEnabled = true;
        [Tooltip("Object to explode")]
        public IPhysicalSceneObject explodingObject = null;
        [Tooltip("Game object to show as an indicator for exploding.")]
        public GameObject explodeIndicator;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            if (explodeIndicator != null)
            {
                // Hide
                explodeIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// When grips are released, turn booleans off and hide the explode indicator prefab
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

            if (explodeIndicator != null)
            {
                explodeIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// When one controller is pressed, determine if the other is pressed.  
        /// If both pressed, show the exploding indicatopr prefab and enable booleans.
        /// </summary>
        public void HandleControllerPressed(InputHand hand)
        {
            if (!explodingEnabled)
            {
                return;
            }

            // This method was being called even when the script was disabled, so check if script is enabled.
            if (hand.handedness == InputHand.Handedness.left)
            {
                leftControllerPressed = true;
                if (rightControllerPressed)
                {
                    controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                    Vector3 scaleRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);

                    if (explodeIndicator != null)
                    {
                        explodeIndicator.transform.position = scaleRefPoint;
                        explodeIndicator.SetActive(true);
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

                    if (explodeIndicator != null)
                    {
                        explodeIndicator.transform.position = scaleRefPoint;
                        explodeIndicator.SetActive(true);
                    }
                }
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (explodingEnabled && (leftControllerPressed == true) && (rightControllerPressed == true) && (explodingObject != null))
            {
                ///summary
                ///if wingspan is expanding, expand. If wingspan is decreasing, contract 
                ///summary
                if (leftControllerPressed && rightControllerPressed)
                {
                    // Expand or collapse based upon the controller positions compared to last time
                    controllerDist = Vector3.Distance(leftController.transform.position, rightController.transform.position);

                    if (controllerDist > controllerDistPrev)
                    {
                        explodingObject.Explode();
                    }
                    else if (controllerDist < controllerDistPrev)
                    {
                        explodingObject.Unexplode();
                    }

                    // Remember for next time
                    controllerDistPrev = controllerDist;

                    // Update text and location of exploding indicator
                    if (explodeIndicator != null)
                    {
                        Vector3 indicatorRefPoint = Vector3.Lerp(leftController.transform.position, rightController.transform.position, 0.5f);
                        explodeIndicator.transform.position = indicatorRefPoint;
                        explodeIndicator.transform.LookAt(MRET.InputRig.head.transform); // Ensure indicator faces the head
                        if (explodeLabel != null)
                        {
                            // Show magnitude of explosion
                            explodeLabel.text = "x" + Math.Round((decimal)(explodingObject.ExplodeFactor), 2).ToString();
                        }
                    }
                }
            }
        }
    }
}


    
