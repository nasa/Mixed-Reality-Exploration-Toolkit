// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// 25 february 2021: Added gravity accessors and properties, and added accessor
    ///     fields to indicate if locomotion modes are enabled. (J. Hosler)
    /// 17 March 2021: Added the motion constraint properties and multipliers to support fast, normal
    ///     and slow motion to centralize the logic for locomotion controllers across input rig
    ///     implementations
    /// 26 April 2021: Added mode field to indicate which mode the SDK is in
    ///     (desktop, VR, AR). (D. Baker)
    /// </remarks>
    /// <summary>
    /// InputRig is a class that contains top-level references
    /// and methods for a user's input.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputRig : MonoBehaviour
    {
        /// <summary>
        /// Mode for the rig.
        /// </summary>
        public enum Mode { AR, VR, Desktop }

        /// <summary>
        /// Mode for the rig.
        /// </summary>
        [Tooltip("Mode for the rig.")]
        public Mode mode = Mode.Desktop;

        /// <summary>
        /// The head object.
        /// </summary>
        [Tooltip("The head object.")]
        public InputHead head;

        /// <summary>
        /// The hands object.
        /// </summary>
        [Tooltip("The hands objects.")]
        public List<InputHand> hands = new List<InputHand>();

        /// <summary>
        /// The body object.
        /// </summary>
        [Tooltip("The body object.")]
        public InputBody body;

        /// <summary>
        /// The user avatar.
        /// </summary>
        [Tooltip("The user avatar.")]
        public GameObject avatar;

        /// <summary>
        /// Reference to the SDK wrapper for the input rig.
        /// </summary>
        [Tooltip("Reference to the SDK wrapper for the input rig.")]
        public InputRigSDK inputRigSDK;

        /// <summary>
        /// A reference to the active head camera. Null if none.
        /// </summary>
        public Camera activeCamera
        {
            get
            {
                if (head != null)
                {
                    // FIXME: We should not search every time. We should defer to the
                    // InputRigSDK or perhaps the InputHeadSDK?
                    return head.GetComponentInChildren<Camera>();
                }

                return null;
            }
        }

        /// <summary>
        /// The left hand if it exists.
        /// </summary>
        public InputHand leftHand
        {
            get
            {
                foreach (InputHand hand in hands)
                {
                    if (hand.handedness == InputHand.Handedness.left)
                    {
                        return hand;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The right hand if it exists.
        /// </summary>
        public InputHand rightHand
        {
            get
            {
                foreach (InputHand hand in hands)
                {
                    if (hand.handedness == InputHand.Handedness.right)
                    {
                        return hand;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The hand used for placing.
        /// </summary>
        public InputHand placingHand
        {
            get
            {
                return inputRigSDK.placingHand;
            }
        }

        /// <summary>
        /// Initializes the input rig.
        /// </summary>
        /// <param name="controllerMode">The mode to set all controllers to.</param>
        public void Initialize(InputHand.ControllerMode controllerMode = InputHand.ControllerMode.Controller)
        {
            // Call sdk initialize.
            inputRigSDK.Initialize();

            // Initialize the head.
            if (head != null)
            {
                head.Initialize();
            }

            // Initialize the body.
            if (body != null)
            {
                body.Initialize();
            }

            // Initialize all input hands.
            foreach (InputHand hand in hands)
            {
                hand.Initialize(controllerMode);
            }
        }

        /// <summary>
        /// Method to remove this rig.
        /// </summary>
        public void Remove()
        {
            Destroy(gameObject);
        }

#region Player

        /// <summary>
        /// The value of player height in meters.
        /// </summary>
        public virtual float PlayerHeight
        {
            set
            {
                inputRigSDK.PlayerHeight = value;
            }
            get
            {
                return inputRigSDK.PlayerHeight;
            }
        }

#endregion Player

#region Physics

        /// <summary>
        /// The value of gravity.
        /// </summary>
        public Vector3 Gravity
        {
            set
            {
                inputRigSDK.Gravity = value;
            }
            get
            {
                return inputRigSDK.Gravity;
            }
        }

        /// <summary>
        /// Indicates if gravity is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not gravity is enabled</returns>
        public bool GravityEnabled
        {
            get
            {
                return inputRigSDK.GravityEnabled;
            }
        }

        /// <summary>
        /// Enables gravity for this rig.
        /// </summary>
        public void EnableGravity()
        {
            inputRigSDK.EnableGravity();
        }

        /// <summary>
        /// Disables gravity for this rig.
        /// </summary>
        public void DisableGravity()
        {
            inputRigSDK.DisableGravity();
        }

#endregion // Physics

#region Locomotion

        /// <summary>
        /// The motion constraint being applied to the rig.
        /// </summary>
        public MotionConstraint MotionConstraint
        {
            set
            {
                inputRigSDK.MotionConstraint = value;
            }
            get
            {
                return inputRigSDK.MotionConstraint;
            }
        }

#region Locomotion [Armswing]

        /// <summary>
        /// The multiplier to be applied to the normal motion constraint arm swing motion.
        /// </summary>
        public float ArmswingNormalMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.ArmswingNormalMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.ArmswingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint arm swing motion.
        /// </summary>
        public float ArmswingSlowMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.ArmswingSlowMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.ArmswingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint arm swing motion.
        /// </summary>
        public float ArmswingFastMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.ArmswingFastMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.ArmswingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The gravity constraint for armswing locomotion.
        /// </summary>
        public GravityConstraint ArmswingGravityConstraint
        {
            set
            {
                inputRigSDK.ArmswingGravityConstraint = value;
            }
            get
            {
                return inputRigSDK.ArmswingGravityConstraint;
            }
        }

        /// <summary>
        /// Indicates if armswing is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not armswing is enabled</returns>
        public bool ArmswingEnabled
        {
            get
            {
                return inputRigSDK.ArmswingEnabled;
            }
        }

        /// <summary>
        /// Enables armswing for this rig.
        /// </summary>
        public void EnableArmswing()
        {
            inputRigSDK.EnableArmswing();
        }

        /// <summary>
        /// Disables armswing for this rig.
        /// </summary>
        public void DisableArmswing()
        {
            inputRigSDK.DisableArmswing();
        }

#endregion // Locomotion [Armswing]

#region Locomotion [Flying]

        /// <summary>
        /// The multiplier to be applied to the normal motion constraint flying motion.
        /// </summary>
        public float FlyingNormalMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.FlyingNormalMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.FlyingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint flying motion.
        /// </summary>
        public float FlyingSlowMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.FlyingSlowMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.FlyingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint flying motion.
        /// </summary>
        public float FlyingFastMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.FlyingFastMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.FlyingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The gravity constraint for flying locomotion.
        /// </summary>
        public GravityConstraint FlyingGravityConstraint
        {
            set
            {
                inputRigSDK.FlyingGravityConstraint = value;
            }
            get
            {
                return inputRigSDK.FlyingGravityConstraint;
            }
        }

        /// <summary>
        /// Indicates if flying is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not flying is enabled</returns>
        public bool FlyingEnabled
        {
            get
            {
                return inputRigSDK.FlyingEnabled;
            }
        }

        /// <summary>
        /// Enables flying for this rig.
        /// </summary>
        public void EnableFlying()
        {
            inputRigSDK.EnableFlying();
        }

        /// <summary>
        /// Disables flying for this rig.
        /// </summary>
        public void DisableFlying()
        {
            inputRigSDK.DisableFlying();
        }

#endregion // Locomotion [Flying]

#region Locomotion [Navigation]

        /// <summary>
        /// The multiplier to be applied to the normal motion constraint navigation motion.
        /// </summary>
        public float NavigationNormalMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.NavigationNormalMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.NavigationNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint navigation motion.
        /// </summary>
        public float NavigationSlowMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.NavigationSlowMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.NavigationSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint navigation motion.
        /// </summary>
        public float NavigationFastMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.NavigationFastMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.NavigationFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The gravity constraint for navigation locomotion.
        /// </summary>
        public GravityConstraint NavigationGravityConstraint
        {
            set
            {
                inputRigSDK.NavigationGravityConstraint = value;
            }
            get
            {
                return inputRigSDK.NavigationGravityConstraint;
            }
        }

        /// <summary>
        /// Indicates if navigation is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not navigation is enabled</returns>
        public bool NavigationEnabled
        {
            get
            {
                return inputRigSDK.NavigationEnabled;
            }
        }

        /// <summary>
        /// Enables navigation for this rig.
        /// </summary>
        public void EnableNavigation()
        {
            inputRigSDK.EnableNavigation();
        }

        /// <summary>
        /// Disables navigation for this rig.
        /// </summary>
        public void DisableNavigation()
        {
            inputRigSDK.DisableNavigation();
        }

        #endregion // Locomotion [Navigation]

#region Locomotion [Climbing]

        /// <summary>
        /// The multiplier to be applied to the normal motion constraint climbing motion.
        /// </summary>
        public float ClimbingNormalMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.ClimbingNormalMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.ClimbingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint climbing motion.
        /// </summary>
        public float ClimbingSlowMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.ClimbingSlowMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.ClimbingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint climbing motion.
        /// </summary>
        public float ClimbingFastMotionConstraintMultiplier
        {
            set
            {
                inputRigSDK.ClimbingFastMotionConstraintMultiplier = value;
            }
            get
            {
                return inputRigSDK.ClimbingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// The gravity constraint for climbing locomotion.
        /// </summary>
        public GravityConstraint ClimbingGravityConstraint
        {
            set
            {
                inputRigSDK.ClimbingGravityConstraint = value;
            }
            get
            {
                return inputRigSDK.ClimbingGravityConstraint;
            }
        }

        /// <summary>
        /// Indicates if climbing is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not climbing is enabled</returns>
        public bool ClimbingEnabled
        {
            get
            {
                return inputRigSDK.ClimbingEnabled;
            }
        }

        /// <summary>
        /// Enables climbing for this rig.
        /// </summary>
        public void EnableClimbing()
        {
            inputRigSDK.EnableClimbing();
        }

        /// <summary>
        /// Disables climbing for this rig.
        /// </summary>
        public void DisableClimbing()
        {
            inputRigSDK.DisableClimbing();
        }

        #endregion // Locomotion [Climbing]

#endregion // Locomotion

    }
}