// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.SteamVR
{
    /// <remarks>
    /// History:
    /// 5 November 2020: Created
    /// 03 February 2021: Added Armswing and Navigation locomotion (J. Hosler)
    /// 10 February 2021: Added Flying locomotion (J. Hosler)
    /// 25 February 2021: Added the gravity accessor and property implementations, and added
    ///     implementations for the accessor fields to indicate if locomotion modes are
    ///     enabled. (J. Hosler)
    /// 17 March 2021: Fixed gravity by making sure kinematic is turned off when gravity is enabled,
    ///     and updated to reflect the use of the controller interface. (J. Hosler)
    /// 24 July 2021: Added Climbing locomotion (C. Lian)
    /// </remarks>
    /// <summary>
    /// SteamVR wrapper for the input rig.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputRigSteamVR : InputRigSDK
    {
        public override InputHand placingHand
        {
            get
            {
                foreach (InputHand hand in inputRig.hands)
                {
                    if (hand.handedness == InputHand.Handedness.right)
                    {
                        return hand;
                    }
                }
                Debug.LogError("[InputHandSteamVR] Unable to find placing hand.");
                return null;
            }
        }

        public override void Initialize()
        {

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
            physics.isKinematic = !physics.useGravity;
        }

        /// <seealso cref="InputRigSDK.DisableGravity"/>
        public override void DisableGravity()
        {
            physics.useGravity = false;
            physics.isKinematic = !physics.useGravity;
        }

#endregion // Physics

#region Locomotion

#region Locomotion [Armswing]

        /// <seealso cref="InputRigSDK.ArmswingEnabled"/>
        public override bool ArmswingEnabled
        {
            get
            {
                return _armswingController.GetControlActive();
            }
        }

        /// <seealso cref="InputRigSDK.EnableArmswing"/>
        public override void EnableArmswing()
        {
            _armswingController.SetControlActive(true);
        }

        /// <seealso cref="InputRigSDK.DisableArmswing"/>
        public override void DisableArmswing()
        {
            _armswingController.SetControlActive(false);
        }

#endregion // Locomotion [Armswing]

#region Locomotion [Flying]

        /// <seealso cref="InputRigSDK.FlyingEnabled"/>
        public override bool FlyingEnabled
        {
            get
            {
                return _flyingController.GetControlActive();
            }
        }

        /// <seealso cref="InputRigSDK.EnableFlying"/>
        public override void EnableFlying()
        {
            _flyingController.SetControlActive(true);
        }

        /// <seealso cref="InputRigSDK.DisableFlying"/>
        public override void DisableFlying()
        {
            _flyingController.SetControlActive(false);
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

#region Locomotion [Climbing]

        /// <seealso cref="InputRigSDK.ClimbingEnabled"/>
        public override bool ClimbingEnabled
        {
            get
            {
                return _climbingController.GetControlActive();
            }
        }

        /// <seealso cref="InputRigSDK.EnableClimbing"/>
        public override void EnableClimbing()
        {
            _climbingController.SetControlActive(true);
        }

        /// <seealso cref="InputRigSDK.DisableClimbing"/>
        public override void DisableClimbing()
        {
            _climbingController.SetControlActive(false);
        }

#endregion // Locomotion [Climbing]

#endregion // Locomotion

    }
}