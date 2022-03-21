using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Components.Locomotion
{
    /// <remarks>
    /// History:
    /// 14 July 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// ClimbingLocomotionController
    ///
    /// Locomotion control for climbing movement
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 
    public class ClimbingLocomotionController : LocomotionController
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ClimbingLocomotionController);
            }
        }

        //Assigned in ClimbingHandController
        public InputHand currentHand = null; 

        /// <summary>
        /// CalculateClimb
        /// 
        /// Moves the rig by the Vector3 negative velocity of the chosen InputHand
        /// </summary>
        /// <param name="hand"the hand whose movement will dictate the transform of the rig></param>
        void CalculateClimb(InputHand hand)
        {
            float sensitivity = GetMotionMultiplier();

            //Find how much we need to change the position of the rig by (negative of the velocity of the hand)
            Vector3 deltaPosition = -hand.velocity * sensitivity * UnityEngine.Time.deltaTime;

            //Move the rig to new location
            rig.transform.position += deltaPosition;
        }

        /// <seealso cref="LocomotionController.DoLocomotion"/>
        protected override void DoLocomotion()
        {
            //currentHand != null when there is an active hand grabbing a rung
            if (currentHand)
            {
                CalculateClimb(currentHand);
            }
        }

    }
}
