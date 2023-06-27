// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.Locomotion
{
    /// <remarks>
    /// History:
    /// 30 May 2019: Created
    /// 10 Feb 2021: Renamed from SimpleMotionController to FlyingLocomotionController and restructured
    ///     to work with the LTS build (J. Hosler)
    /// 25 February 2021: Wrapped the log message to indicate no controllers are active into
    ///     the MRET_DEBUG compiler directive (J. Hosler)
    /// 17 March 2021: Removed speed references to use the motion contraints in the base
    ///     class. (J. Hosler)
    /// 8 November 2021: Separated flying into its own method. Script will need to be refactored to
    ///     separate hand based flying from generic flying (DZB)
    /// </remarks>
    ///
    /// <summary>
    /// FlyingLocomotionController
    ///
    /// Provides simple flying motion based on controller motion when the grip button is pressed.
    ///
    /// Author: Troy Ames
    /// </summary>
    /// 
    public class FlyingLocomotionController : LocomotionController
    {
        /// <summary>
        /// Flying mechanism type.
        /// </summary>
        public enum FlyingMechanism { Positional, DirectionalInput }

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(FlyingLocomotionController);
            }
        }

        /// <summary>
        /// Flying mechanism type to use.
        /// </summary>
        public FlyingMechanism flyingMechanism = FlyingMechanism.Positional;

        Vector3? lastHandPosition = null;

        [Tooltip("Reverses the camera motion relative to controller motion")]
        public bool reverseMotion = false;

        [Tooltip("Multiplier for directional motion.")]
        public float directionalMultiplier = 10;

        /// <seealso cref="LocomotionController.HandActiveStateChanged"/>
        protected override void HandActiveStateChanged(InputHand hand)
        {
            base.HandActiveStateChanged(hand);
            ResetLastPosition();
        }

        /// <summary>
        /// Resets the last registered hand position
        /// </summary>
        protected virtual void ResetLastPosition()
        {
            // Obtain the list of active hands
            InputHand[] activeHands = GetActiveHands();

            // Only save the position if one hand is active
            if (activeHands.Length == 1)
            {
                // Save the active hand postion
                lastHandPosition = activeHands[0].transform.position;
#if MRET_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(ResetLastPosition) + "] " + activeHands[0].name + " position: " + lastHandPosition);
#endif
            }
            else
            {
                // Reset the position
                lastHandPosition = null;
#if MRET_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(ResetLastPosition) + "] RESET");
#endif
            }
        }

        /// <summary>
        /// Perform one frame of flying motion.
        /// </summary>
        /// <param name="amount">Amount to fly.</param>
        /// <param name="reversed">Whether or not the motion is reversed.</param>
        public void Fly(Vector3 amount, bool reversed)
        {
            if (reversed)
            {
                rig.transform.position += amount;
            }
            else
            {
                rig.transform.position -= amount;
            }
        }

        /// <summary>
        /// SetRigTransform
        /// 
        /// Sets the new Rig transform based upon the movement that has occured since the last time this method was called
        /// </summary>
        /// <param name="hand">The hand used to perform the calculation</param>
        private void SetRigTransform(InputHand hand)
        {
            switch (flyingMechanism)
            {
                case FlyingMechanism.Positional:
                    if (lastHandPosition.HasValue)
                    {
                        // Obtain the new hand position
                        Vector3 newPosition = hand.transform.position;

                        float sensitivity = 0.0f;
                        Vector3 positionDelta = newPosition - (Vector3)lastHandPosition;
                        //print ("positionDelta.magnitude:" + positionDelta.magnitude);

                        // Guard against tracking errors or teleporting while in motion errors
                        if (positionDelta.magnitude < 1.5f)
                        {
                            // Sensitivity is variable based on magnitude of controller motion.
                            sensitivity = positionDelta.magnitude;
                        }

                        // Apply property setting
                        sensitivity *= GetMotionMultiplier();

                        // Update camera rig position
                        if (reverseMotion)
                        {
                            // Offset the rig position and the las hand position by equivalent amounts
                            Fly(positionDelta * sensitivity, true);
                            lastHandPosition += positionDelta * sensitivity;
                        }
                        else
                        {
                            // Offset the rig position and the las hand position by equivalent amounts
                            Fly(positionDelta * sensitivity, false);
                            lastHandPosition -= positionDelta * sensitivity;
                        }

#if MRET_DEBUG
                        Debug.Log("[" + ClassName + "->" + nameof(SetRigTransform) + "] New rig position: " + rig.transform.position);
                        Debug.Log("[" + ClassName + "->" + nameof(SetRigTransform) + "] New hand position: " + lastHandPosition);
#endif
                    }
                    else
                    {
                        // We are in an odd state so reset the last position
                        ResetLastPosition();
                    }
                    break;

                case FlyingMechanism.DirectionalInput:
                    Vector2 navigateValue = hand.navigateValue;
                    Vector3 movementAmount = ((MRET.InputRig.head.transform.right * navigateValue.x)
                        + (MRET.InputRig.head.transform.forward * navigateValue.y))
                        * UnityEngine.Time.deltaTime * GetMotionMultiplier() * directionalMultiplier;
                    Fly(movementAmount, reverseMotion);
                    break;

                default:
                    Debug.LogError("[" + ClassName + "->" + nameof(SetRigTransform) + "] flying mechanism not set.");
                    break;
            }
        }

        /// <seealso cref="LocomotionController.DoLocomotion"/>
        protected override void DoLocomotion()
        {
            // Obtain the list of active hands
            InputHand[] activeHands = GetActiveHands();

            // Only perform the transform if one hand is active
            if (activeHands.Length == 1)
            {
                SetRigTransform(activeHands[0]);
            }
            else if (activeHands.Length > 1)
            {
#if MRET_DEBUG
                // Too many controllers
                Debug.Log("[" + ClassName + "->" + nameof(DoLocomotion) + "] Too many controllers active");
#endif
            }
            else
            {
#if MRET_DEBUG
                // No controllers active
                Debug.Log("[" + ClassName + "->" + nameof(DoLocomotion) + "] No controllers active");
#endif
            }
        }

        private void DebugLogger(uint index, string button, string action, float pressure, Vector2 axis, float angle)
        {
            Debug.Log("[" + ClassName + "] Controller on index '" + index + "' " + button + " has been " + action
                + " with a pressure of " + pressure + " / trackpad axis at: " + axis + " (" + angle + " degrees)");
        }
    }
}