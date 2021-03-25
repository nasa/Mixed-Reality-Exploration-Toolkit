// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Components.Locomotion
{
    /// <remarks>
    /// History:
    /// 03 February 2021: Created
    /// 25 February 2021: Wrapped the log message to indicate no controllers are active into
    ///     the MRET_DEBUG compiler directive (J. Hosler)
    /// 17 March 2021: Removed speed references to use the motion contraints in the base
    ///     class, and fixed logic in SetRigTransform to properly make use of the head
    ///     orientation to control "forward". (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// NavigationLocomotionController
	///
	/// Locomotion control for navigation movement
	///
    /// Author: Jeffrey C. Hosler
	/// </summary>
	/// 
    public class NavigationLocomotionController : LocomotionController
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(NavigationLocomotionController);
            }
        }

        /// <summary>
        /// GetDirection
        /// 
        /// Calculates the normalized direction being moved in this instant, based upon the head vector
        /// </summary>
        /// <returns>The normalized direction to move</returns>
        private Vector3 GetDirection()
        {
            Vector3 headOrientation;
            Vector3 flattenedHeadOrientation;

            // Flatten the head orientation
            headOrientation = head.transform.forward;
            flattenedHeadOrientation = new Vector3(headOrientation.x, 0, headOrientation.z);

            // Normalize to get the result
            return flattenedHeadOrientation.normalized;
        }

        /// <summary>
        /// SetRigTransform
        /// 
        /// Sets the new Rig transform based upon the movement that has occured since the last time this method was called
        /// </summary>
        /// <param name="hand">The hand used to perform the calculation</param>
        private void SetRigTransform(InputHand hand)
        {
            Vector3 direction = GetDirection();
            Vector2 axis = hand.navigateValue;

            // Calculate the new transform position, leaving Y where it is.
            // Example: https://www.youtube.com/watch?v=EUnQ4whsQcU
            rig.transform.position +=
                ((head.transform.right * axis.x) +
                 (direction * axis.y)) * UnityEngine.Time.deltaTime * GetMotionMultiplier();
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(SetRigTransform) + "] New rig position: " + rig.transform.position);
#endif
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

    }
}