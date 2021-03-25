// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Components.Locomotion
{
    /// <remarks>
    /// History:
    /// 03 December 2020: Created
    /// 03 February 2021: Updated to work with the new LTS SDK abstraction
    /// 25 February 2021: Wrapped the log message to indicate no controllers are active into
    ///     the MRET_DEBUG compiler directive (J. Hosler)
    /// 17 March 2021: Removed speed references to use the motion contraints in the base
    ///     class. (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// ArmswingLocomotionController
	///
	/// Locomotion control for arm wing movement
    /// 
	/// TODO: No protection against one or more hands being unavailable, i.e. NULL
    /// 
    /// Author: Jeffrey C. Hosler
	/// </summary>
	/// 
    public class ArmswingLocomotionController : LocomotionController
    {
        enum ArmSwingMovement
        {
            Left,
            Right,
            Both
        }

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ArmswingLocomotionController);
            }
        }

        protected InputHand leftHand;
        protected InputHand rightHand;

        private Vector3 previousLeftControllerPosition = Vector3.zero;
        private Vector3 previousRightControllerPosition = Vector3.zero;

        /// <summary>
        /// GetDistance
        /// 
        /// Calculates the distance between the previous active hand positions and the new active hand positions
        /// </summary>
        /// <param name="movement"></param>
        /// <returns>The distance difference to move since the last time this method was called</returns>
        private float GetDistance(ArmSwingMovement movement)
        {
            float result;
            Vector3 leftHandPosition;
            Vector3 rightHandPosition;
            Vector3 differenceLeftControllerPosition;
            Vector3 differenceRightControllerPosition;

            switch (movement)
            {
                case ArmSwingMovement.Left:
                    // Calculate the difference of the left controller local position from the previous left controller local position
                    leftHandPosition = leftHand.transform.position;
                    differenceLeftControllerPosition = leftHandPosition - previousLeftControllerPosition;

                    // Calculate the magnitude of the difference vector to get the result
                    result = differenceLeftControllerPosition.magnitude;
                    break;

                case ArmSwingMovement.Right:
                    // Calculate the difference of the left controller local position from the previous left controller local position
                    rightHandPosition = rightHand.transform.position;
                    differenceRightControllerPosition = rightHandPosition - previousRightControllerPosition;

                    // Calculate the magnitude of the difference vector to get the result
                    result = differenceRightControllerPosition.magnitude;
                    break;

                case ArmSwingMovement.Both:
                default:
                    // Calculate the difference of the controller local positions from the previous controller local positions
                    leftHandPosition = leftHand.transform.position;
                    rightHandPosition = rightHand.transform.position;
                    differenceLeftControllerPosition = leftHandPosition - previousLeftControllerPosition;
                    differenceRightControllerPosition = rightHandPosition - previousRightControllerPosition;

                    // Add the magnitudes of the difference vectors to get the result
                    result = differenceLeftControllerPosition.magnitude + differenceRightControllerPosition.magnitude;
                    break;
            }

            return result;
        }

        /// <summary>
        /// GetDirection
        /// 
        /// Calculates the normalized direction being moved in this instant, based upon the active controller vectors
        /// </summary>
        /// <param name="movement"></param>
        /// <returns>The normalized direction to move</returns>
        private Vector3 GetDirection(ArmSwingMovement movement)
        {
            Vector3 result = Vector3.zero;
            Vector3 leftHandPosition;
            Vector3 rightHandPosition;
            Vector3 flattenedLeftControllerPosition;
            Vector3 flattenedRightControllerPosition;

            switch (movement)
            {
                case ArmSwingMovement.Left:
                    // Flatten the left controller orientation
                    leftHandPosition = leftHand.transform.forward;
                    flattenedLeftControllerPosition = new Vector3(leftHandPosition.x, 0, leftHandPosition.z);

                    // Normalize to get the result
                    result = flattenedLeftControllerPosition.normalized;
                    break;

                case ArmSwingMovement.Right:
                    // Flatten the right controller orientation
                    rightHandPosition = rightHand.transform.forward;
                    flattenedRightControllerPosition = new Vector3(rightHandPosition.x, 0, rightHandPosition.z);

                    // Normalize to get the result
                    result = flattenedRightControllerPosition.normalized;
                    break;

                case ArmSwingMovement.Both:
                default:
                    // Flatten the controller orientations
                    leftHandPosition = leftHand.transform.forward;
                    rightHandPosition = rightHand.transform.forward;
                    flattenedLeftControllerPosition = new Vector3(leftHandPosition.x, 0, leftHandPosition.z);
                    flattenedRightControllerPosition = new Vector3(rightHandPosition.x, 0, rightHandPosition.z);

                    // Normalize and calculate the mean to get the result
                    result = flattenedLeftControllerPosition.normalized + flattenedRightControllerPosition.normalized;
                    result = result / 2f;
                    break;
            }

            return result;
        }

        /// <summary>
        /// SetRigTransform
        /// 
        /// Sets the new Rig transform based upon the movement that has occured since the last time this method was called
        /// </summary>
        /// <param name="movement"></param>
        private void SetRigTransform(ArmSwingMovement movement)
        {
            // Get the direction and distance of both controllers
            Vector3 direction = GetDirection(movement);
            float distance = GetDistance(movement);

            // Multiply the direction and distance and movement speed to get the delta we will move
            Vector3 deltaPosition = direction * distance * GetMotionMultiplier();

            // Move the rig to the new location
            rig.transform.position += deltaPosition;
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(SetRigTransform) + "] New rig position: " + rig.transform.position);
#endif
        }

        /// <seealso cref="LocomotionController.DoLocomotion"/>
        protected override void DoLocomotion()
        {
            // Obtain the active state of the hands
            bool leftHandActive = IsHandActive(leftHand);
            bool rightHandActive = IsHandActive(rightHand);

            // Perform the locomotion based upon which controllers are active
            if (leftHandActive && rightHandActive)
            {
                // Both controllers
                SetRigTransform(ArmSwingMovement.Both);
            }
            else if (leftHandActive)
            {
                // Left controller
                SetRigTransform(ArmSwingMovement.Left);
            }
            else if (rightHandActive)
            {
                // Right controller
                SetRigTransform(ArmSwingMovement.Right);
            }
            else
            {
#if MRET_DEBUG
                // Not active
                Debug.Log("[" + ClassName + "->" + nameof(DoLocomotion) + "] No controllers active");
#endif
            }

            // Set the controller positions for the next time this method is executed
            previousLeftControllerPosition = leftHand.transform.position;
            previousRightControllerPosition = rightHand.transform.position;
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (leftHand == null) ||
                (rightHand == null)
                    ? IntegrityState.Failure   // Fail is base class fails or we don't have the hand references
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Get the internal references needed to perform locomotion
            leftHand = rig.leftHand;
            rightHand = rig.rightHand;
        }

    }

}