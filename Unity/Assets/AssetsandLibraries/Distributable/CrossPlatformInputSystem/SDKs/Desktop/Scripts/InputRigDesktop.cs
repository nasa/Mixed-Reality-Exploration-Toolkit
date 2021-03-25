// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Desktop
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// 17 March 2021: Removed the running/walking references to make use of the generalized
    ///     motion constraints in the input rig SDK. Removed the armswing implementations, and
    ///     updated to reflect use of the controller interface. (J. Hosler)
    /// </remarks>
    /// <summary>
    /// Desktop wrapper for the input rig.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputRigDesktop : InputRigSDK
    {
        /// <summary>
        /// The character controller for the input rig.
        /// </summary>
        [Tooltip("The character controller for the input rig.")]
        public CharacterController characterController;

        /// <summary>
        /// The head's transform for the input rig.
        /// </summary>
        [Tooltip("The head's transform for the input rig.")]
        public Transform head;

        public override InputHand placingHand
        {
            get
            {
                foreach (InputHand hand in inputRig.hands)
                {
                    InputHandDesktop ihd = hand.GetComponent<InputHandDesktop>();
                    if (ihd != null)
                    {
                        if (ihd.dynamic)
                        {
                            return hand;
                        }
                    }
                }
                Debug.LogError("[InputHandDesktop] Unable to find placing hand.");
                return null;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Cursor.lockState = CursorLockMode.Locked;
        }

#region Physics

        /// <seealso cref="InputRigSDK.GravityEnabled"/>
        public override bool GravityEnabled
        {
            get
            {
                return physics.useGravity;
            }
        }

        /// <seealso cref="InputRigSDK.EnableGravity"/>
        public override void EnableGravity()
        {
            physics.useGravity = true;
        }

        /// <seealso cref="InputRigSDK.DisableGravity"/>
        public override void DisableGravity()
        {
            physics.useGravity = false;
        }

#endregion // Physics

#region Locomotion

#region Locomotion [Armswing]

        /// <seealso cref="InputRigSDK.ArmswingEnabled"/>
        public override bool ArmswingEnabled
        {
            get
            {
                // TODO: implement armswing?
                return false;
            }
        }

        /// <seealso cref="InputRigSDK.EnableArmswing"/>
        public override void EnableArmswing()
        {
            Debug.Log("[InputRigDesktop->EnableArmswing] Armswing not implemented for desktop.");
        }

        /// <seealso cref="InputRigSDK.DisableArmswing"/>
        public override void DisableArmswing()
        {
            Debug.Log("[InputRigDesktop->DisableArmswing] Armswing not implemented for desktop.");
        }

#endregion // Locomotion [Armswing]

#region Locomotion [Flying]

        /// <seealso cref="InputRigSDK.FlyingEnabled"/>
        public override bool FlyingEnabled
        {
            get
            {
                // TODO: implement flying?
                return false;
            }
        }

        /// <seealso cref="InputRigSDK.EnableFlying"/>
        public override void EnableFlying()
        {
            Debug.Log("[InputRigDesktop->EnableFlying] Flying not implemented for desktop.");
        }

        /// <seealso cref="InputRigSDK.DisableFlying"/>
        public override void DisableFlying()
        {
            Debug.Log("[InputRigDesktop->EnableFlying] Flying not implemented for desktop.");
        }

#endregion // Locomotion [Flying]

#region Locomotion [Navigation]

        /// <seealso cref="InputRigSDK.NavigationEnabled"/>
        public override bool NavigationEnabled
        {
            get
            {
                return _navigationController.GetControlActive();
            }
        }

        /// <seealso cref="InputRigSDK.EnableNavigation"/>
        public override void EnableNavigation()
        {
            _navigationController.SetControlActive(true);
        }

        /// <seealso cref="InputRigSDK.DisableNavigation"/>
        public override void DisableNavigation()
        {
            _navigationController.SetControlActive(false);
        }

#endregion // Locomotion [Navigation]

#endregion // Locomotion
    }
}