// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// 03 February 2021: Added ArmSwing locomotion (J. Hosler)
    /// 25 february 2021: Added the Rigidbody physics field, added gravity accessors and
    ///     properties, and added accessor fields to indicate if locomotion modes are
    ///     enabled. (J. Hosler)
    /// 15 March 2021: Changed the locomotion controller references to interfaces in an attempt to
    ///     begin isolating the cross platform input system from the MRET application. May have to
    ///     revisit the use of interfaces since they require custom property handling in order to
    ///     make them work in the editor. (J. Hosler)
    /// 17 March 2021: Added the motion constraint properties and multipliers to support fast, normal
    ///     and slow motion to centralize the logic for locomotion controllers across input rig
    ///     implementations (J. Hosler)
    /// 24 July 2021: Added Climbing locomotion (C. Lian)
    /// 17 November 2021: Removed RequireInterface for FlyingController reference, does not seem to
    ///     be assignable from inspector (DZB)
    /// 08 September 2022: Added player height and event model to listen for changes
    /// </remarks>
    /// <summary>
    /// SDK wrapper for the input rig.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputRigSDK : MonoBehaviour
    {
        /// <summary>
        /// The default rig height in meters. 1.67m is about the average human height.
        /// </summary>
        public const float DEFAULT_PLAYER_HEIGHT = 1.67f;

        /// <summary>
        /// Reference to the input rig class.
        /// </summary>
        [Tooltip("Reference to the input rig class.")]
        public InputRig inputRig;

        /// <summary>
        /// The player height in meters.
        /// </summary>
        [SerializeField]
        [Tooltip("The player height in meters.")]
        float playerHeight = DEFAULT_PLAYER_HEIGHT;

        /// <summary>
        /// Physics for this input rig.
        /// </summary>
        [Tooltip("The physics for the input rig.")]
        public Rigidbody physics;

        /// <summary>
        /// The character controller for the input rig.
        /// </summary>
        [Tooltip("The character controller for the input rig.")]
        public CharacterController characterController;

        /// <summary>
        /// ArmSwing Controller for this input rig.
        /// </summary>
        [Tooltip("Armswing Controller for the input rig.")]
#if UNITY_EDITOR
        [RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public UnityEngine.Object armswingController;
        protected IInputRigLocomotionControl _armswingController => armswingController as IInputRigLocomotionControl;

        /// <summary>
        /// ArmSwing Controller for this input rig.
        /// </summary>
        [Tooltip("Flying Controller for the input rig.")]
#if UNITY_EDITOR
        [RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public UnityEngine.Object flyingController;
        protected IInputRigLocomotionControl _flyingController => flyingController as IInputRigLocomotionControl;

        /// <summary>
        /// Navigation Controller for this input rig.
        /// </summary>
        [Tooltip("Navigation Controller for the input rig.")]
#if UNITY_EDITOR
        [RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public UnityEngine.Object navigationController;
        protected IInputRigLocomotionControl _navigationController => navigationController as IInputRigLocomotionControl;

        /// <summary>
        /// Climbing Controller for this input rig.
        /// </summary>
        [Tooltip("Climbing Controller for the input rig.")]
#if UNITY_EDITOR
        [RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public UnityEngine.Object climbingController;
        protected IInputRigLocomotionControl _climbingController => climbingController as IInputRigLocomotionControl;

        /// <summary>
        /// The hand used for placing.
        /// </summary>
        public virtual InputHand placingHand
        {
            get
            {
                Debug.LogError("PlacingHand not set for InputRigSDK.");
                return null;
            }
        }

        private void Start()
        {
            if (inputRig == null)
            {
                inputRig = GetComponent<InputRig>();
            }
        }

        /// <summary>
        /// Initialize method for SDK input rig wrapper.
        /// </summary>
        public virtual void Initialize()
        {
            //Debug.LogWarning("Initialize() not implemented for InputRigSDK.");
        }

#region Player

        /// <summary>
        /// The player height in meters
        /// </summary>
        public float PlayerHeight
        {
            get => playerHeight;
            set
            {
                if (playerHeight != value)
                {
                    // Update the player height
                    float previousValue = playerHeight;
                    playerHeight = value;

                    // Notify listeners
                    OnPlayerHeightChange(previousValue, playerHeight);
                }
            }
        }

        /// <summary>
        /// <code>Action</code> to respond to player height changes
        /// </summary>
        public event Action<float,float> PlayerHeightChange;

        /// <summary>
        /// Invokes the <code>PlayerHeightChange</code> action on a player height change
        /// </summary>
        /// <param name="previousHeight">Previous player height</param>
        /// <param name="newHeight">New player height</param>
        public void OnPlayerHeightChange(float previousHeight, float newHeight)
        {
            PlayerHeightChange?.Invoke(previousHeight, newHeight);
        }

#endregion Player

#region Physics

        /// <summary>
        /// The value of gravity.
        /// </summary>
        public virtual Vector3 Gravity
        {
            set
            {
                Debug.LogWarning("Gravity not implemented for InputRigSDK.");
            }
            get
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Indicates if gravity is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not gravity is enabled</returns>
        public virtual bool GravityEnabled
        {
            get
            {
                Debug.LogWarning("GravityEnabled not implemented for InputRigSDK.");
                return false;
            }
        }

        /// <summary>
        /// Enables gravity for this rig.
        /// </summary>
        public virtual void EnableGravity()
        {
            Debug.LogWarning("EnableGravity() not implemented for InputRigSDK.");
        }

        /// <summary>
        /// Disables gravity for this rig.
        /// </summary>
        public virtual void DisableGravity()
        {
            Debug.LogWarning("EnableGravity() not implemented for InputRigSDK.");
        }

#endregion // Physics

#region Locomotion

        /// <summary>
        /// The motion constraint to be applied to locomotion.
        /// </summary>
        private MotionConstraint _motionConstraint = MotionConstraint.Normal;
        public MotionConstraint MotionConstraint
        {
            set
            {
                _motionConstraint = value;
            }
            get
            {
                return _motionConstraint;
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
                // Set the controller normal motion constraint multiplier if assigned
                if (armswingController != null)
                {
                    _armswingController.SetMotionConstraintMultiplier(MotionConstraint.Normal, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Armswing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (armswingController != null) ? _armswingController.GetMotionConstraintMultiplier(MotionConstraint.Normal) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint arm swing motion.
        /// </summary>
        public float ArmswingSlowMotionConstraintMultiplier
        {
            set
            {
                // Set the controller slow motion constraint multiplier if assigned
                if (armswingController != null)
                {
                    _armswingController.SetMotionConstraintMultiplier(MotionConstraint.Slow, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Armswing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (armswingController != null) ? _armswingController.GetMotionConstraintMultiplier(MotionConstraint.Slow) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint arm swing motion.
        /// </summary>
        public float ArmswingFastMotionConstraintMultiplier
        {
            set
            {
                // Set the controller fast motion constraint multiplier if assigned
                if (armswingController != null)
                {
                    _armswingController.SetMotionConstraintMultiplier(MotionConstraint.Fast, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Armswing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (armswingController != null) ? _armswingController.GetMotionConstraintMultiplier(MotionConstraint.Fast) : 0f;
            }
        }

        /// <summary>
        /// The gravity constraint to be applied to the arm swing motion.
        /// </summary>
        public GravityConstraint ArmswingGravityConstraint
        {
            set
            {
                // Set the controller gravity constraint if assigned
                if (armswingController != null)
                {
                    _armswingController.SetGravityConstraint(value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Armswing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (armswingController != null) ? _armswingController.GetGravityConstraint() : GravityConstraint.Allowed;
            }
        }

        /// <summary>
        /// Indicates if armswing is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not armswing is enabled</returns>
        public virtual bool ArmswingEnabled
        {
            get
            {
                Debug.LogWarning("ArmswingEnabled not implemented for InputRigSDK.");
                return false;
            }
        }

        /// <summary>
        /// Enables armswing for this rig.
        /// </summary>
        public virtual void EnableArmswing()
        {
            Debug.LogWarning("EnableArmswing() not implemented for InputRigSDK.");
        }

        /// <summary>
        /// Disables armswing for this rig.
        /// </summary>
        public virtual void DisableArmswing()
        {
            Debug.LogWarning("DisableArmswing() not implemented for InputRigSDK.");
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
                // Set the controller motion multiplier if assigned
                if (flyingController != null)
                {
                    _flyingController.SetMotionConstraintMultiplier(MotionConstraint.Normal, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Flying not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (flyingController != null) ? _flyingController.GetMotionConstraintMultiplier(MotionConstraint.Normal) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint flying motion.
        /// </summary>
        public float FlyingSlowMotionConstraintMultiplier
        {
            set
            {
                // Set the controller motion multiplier if assigned
                if (flyingController != null)
                {
                    _flyingController.SetMotionConstraintMultiplier(MotionConstraint.Slow, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Flying not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (flyingController != null) ? _flyingController.GetMotionConstraintMultiplier(MotionConstraint.Slow) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint flying motion.
        /// </summary>
        public float FlyingFastMotionConstraintMultiplier
        {
            set
            {
                // Set the controller motion multiplier if assigned
                if (flyingController != null)
                {
                    _flyingController.SetMotionConstraintMultiplier(MotionConstraint.Fast, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Flying not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (flyingController != null) ? _flyingController.GetMotionConstraintMultiplier(MotionConstraint.Fast) : 0f;
            }
        }

        /// <summary>
        /// The gravity constraint to be applied to the flying motion.
        /// </summary>
        public GravityConstraint FlyingGravityConstraint
        {
            set
            {
                // Set the controller gravity constraint if assigned
                if (flyingController != null)
                {
                    _flyingController.SetGravityConstraint(value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Flying not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (flyingController != null) ? _flyingController.GetGravityConstraint() : GravityConstraint.Allowed;
            }
        }

        /// <summary>
        /// Indicates if flying is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not flying is enabled</returns>
        public virtual bool FlyingEnabled
        {
            get
            {
                Debug.LogWarning("FlyingEnabled not implemented for InputRigSDK.");
                return false;
            }
        }

        /// <summary>
        /// Enables flying for this rig.
        /// </summary>
        public virtual void EnableFlying()
        {
            Debug.LogWarning("EnableFlying() not implemented for InputRigSDK.");
        }

        /// <summary>
        /// Disables flying for this rig.
        /// </summary>
        public virtual void DisableFlying()
        {
            Debug.LogWarning("DisableFlying() not implemented for InputRigSDK.");
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
                // Set the controller motion multiplier if assigned
                if (navigationController != null)
                {
                    _navigationController.SetMotionConstraintMultiplier(MotionConstraint.Normal, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Navigation not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (navigationController != null) ? _navigationController.GetMotionConstraintMultiplier(MotionConstraint.Normal) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint navigation motion.
        /// </summary>
        public float NavigationSlowMotionConstraintMultiplier
        {
            set
            {
                // Set the controller motion multiplier if assigned
                if (navigationController != null)
                {
                    _navigationController.SetMotionConstraintMultiplier(MotionConstraint.Slow, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Navigation not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (navigationController != null) ? _navigationController.GetMotionConstraintMultiplier(MotionConstraint.Slow) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint navigation motion.
        /// </summary>
        public float NavigationFastMotionConstraintMultiplier
        {
            set
            {
                // Set the controller motion multiplier if assigned
                if (navigationController != null)
                {
                    _navigationController.SetMotionConstraintMultiplier(MotionConstraint.Fast, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Navigation not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (navigationController != null) ? _navigationController.GetMotionConstraintMultiplier(MotionConstraint.Fast) : 0f;
            }
        }

        /// <summary>
        /// The gravity constraint to be applied to the navigation motion.
        /// </summary>
        public GravityConstraint NavigationGravityConstraint
        {
            set
            {
                // Set the controller gravity constraint if assigned
                if (navigationController != null)
                {
                    _navigationController.SetGravityConstraint(value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Navigation not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (navigationController != null) ? _navigationController.GetGravityConstraint() : GravityConstraint.Allowed;
            }
        }

        /// <summary>
        /// Indicates if navigation is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not navigation is enabled</returns>
        public virtual bool NavigationEnabled
        {
            get
            {
                Debug.LogWarning("NavigationEnabled not implemented for InputRigSDK.");
                return false;
            }
        }

        /// <summary>
        /// Enables navigation for this rig.
        /// </summary>
        public virtual void EnableNavigation()
        {
            Debug.LogWarning("EnableNavigation() not implemented for InputRigSDK.");
        }

        /// <summary>
        /// Disables navigation for this rig.
        /// </summary>
        public virtual void DisableNavigation()
        {
            Debug.LogWarning("DisableNavigation() not implemented for InputRigSDK.");
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
                // Set the controller motion multiplier if assigned
                if (climbingController != null)
                {
                    _climbingController.SetMotionConstraintMultiplier(MotionConstraint.Normal, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Climbing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (climbingController != null) ? _climbingController.GetMotionConstraintMultiplier(MotionConstraint.Normal) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the slow motion constraint climbing motion.
        /// </summary>
        public float ClimbingSlowMotionConstraintMultiplier
        {
            set
            {
                // Set the controller motion multiplier if assigned
                if (climbingController != null)
                {
                    _climbingController.SetMotionConstraintMultiplier(MotionConstraint.Slow, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Climbing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (climbingController != null) ? _climbingController.GetMotionConstraintMultiplier(MotionConstraint.Slow) : 0f;
            }
        }

        /// <summary>
        /// The multiplier to be applied to the fast motion constraint climbing motion.
        /// </summary>
        public float ClimbingFastMotionConstraintMultiplier
        {
            set
            {
                // Set the controller motion multiplier if assigned
                if (climbingController != null)
                {
                    _climbingController.SetMotionConstraintMultiplier(MotionConstraint.Fast, value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Climbing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (climbingController != null) ? _climbingController.GetMotionConstraintMultiplier(MotionConstraint.Fast) : 0f;
            }
        }

        /// <summary>
        /// The gravity constraint to be applied to the climbing motion.
        /// </summary>
        public GravityConstraint ClimbingGravityConstraint
        {
            set
            {
                // Set the controller gravity constraint if assigned
                if (climbingController != null)
                {
                    _climbingController.SetGravityConstraint(value);
                }
                else
                {
                    Debug.LogWarning(nameof(IInputRigLocomotionControl) + " for Climbing not defined for " + nameof(InputRigSDK));
                }
            }
            get
            {
                return (climbingController != null) ? _climbingController.GetGravityConstraint() : GravityConstraint.Allowed;
            }
        }

        /// <summary>
        /// Indicates if climbing is enabled for this rig.
        /// </summary>
        /// <returns>A boolean value indicating whether or not climbing is enabled</returns>
        public virtual bool ClimbingEnabled
        {
            get
            {
                Debug.LogWarning("ClimbingEnabled not implemented for InputRigSDK.");
                return false;
            }
        }

        /// <summary>
        /// Enables climbing for this rig.
        /// </summary>
        public virtual void EnableClimbing()
        {
            Debug.LogWarning("EnableClimbing() not implemented for InputRigSDK.");
        }

        /// <summary>
        /// Disables climbing for this rig.
        /// </summary>
        public virtual void DisableClimbing()
        {
            Debug.LogWarning("DisableClimbing() not implemented for InputRigSDK.");
        }

        #endregion // Locomotion [Climbing]

        #endregion // Locomotion

    }
}