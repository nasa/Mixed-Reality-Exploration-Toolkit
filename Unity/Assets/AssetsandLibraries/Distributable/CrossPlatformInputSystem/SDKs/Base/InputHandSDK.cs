// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Locomotion;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.UI.Interactions;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// 18 November 2020: Adding interaction modes
    /// 03 February 2021: Added ArmSwing locomotion (J. Hosler)
    /// 25 February 2021: Added additional behavior accessor properties to allow for fine
    ///     tuned polling of hand behaviors, and changed visibility of the Start method
    ///     for subclass overriding (J. Hosler)
    /// 15 March 2021: Changed the locomotion controller references to interfaces in an attempt to
    ///     begin isolating the cross platform input system from the MRET application. May have to
    ///     revisit the use of interfaces since they require custom property handling in order to
    ///     make them work in the editor. (J. Hosler)
    /// 24 July 2021: Added Climbing locomotion (C. Lian)
    /// 28 July 2021: Adding support for velocity (DZB)
    /// 17 August 2021: Added pointer functions.
    /// 17 November 2021: Removed RequireInterface for FlyingController reference, does not seem to
    ///     be assignable from Inspector (DZB)
    /// 23 December 2021: Adding Drawing Laser (DZB)
    /// </remarks>
    /// <summary>
    /// SDK wrapper for the input hand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHandSDK : MonoBehaviour
    {
        /// <summary>
        /// Reference to the input hand class.
        /// </summary>
        [Tooltip("Reference to the input hand class.")]
        public InputHand inputHand;

        /// <summary>
        /// The hand target. This should not be the hand Transform itself, but a child
        /// <code>GameObject</code> parented by the hand so that the 'target' can be
        /// repositioned and oriented within the hand to align the transform to match the
        /// location of the user's hand.
        /// </summary>
        [Tooltip("The hand target. This should not be the hand Transform itself, but a child GameObject parented by the hand so that the 'target' can be repositioned and oriented within the hand to align the transform to match the location of the user's hand.")]
        public Transform target;

        /// <summary>
        /// The hand collider.
        /// </summary>
        [Tooltip("The hand collider.")]
        public new Collider collider;

        /// <summary>
        /// Teleport Controller for the hand.
        /// </summary>
        [Tooltip("Teleport Controller for the hand.")]
        public TeleportController teleportController; // TODO: Change this to an interface to keep the cross platform system as an independently distributable package

        /// <summary>
        /// Armswing Controller for this hand.
        /// </summary>
        [Tooltip("Armswing Controller for the hand.")]
#if UNITY_EDITOR
        [RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public Object armswingController;
        protected IInputRigLocomotionControl _armswingController => armswingController as IInputRigLocomotionControl;

        /// <summary>
        /// Flying Controller for this hand.
        /// </summary>
        [Tooltip("Flying Controller for the hand.")]
#if UNITY_EDITOR
        //[RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public Object flyingController;
        protected IInputRigLocomotionControl _flyingController => flyingController as IInputRigLocomotionControl;

        /// <summary>
        /// ArmSwing Controller for this hand.
        /// </summary>
        [Tooltip("Navigation Controller for the hand.")]
#if UNITY_EDITOR
        [RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public Object navigationController;
        protected IInputRigLocomotionControl _navigationController => navigationController as IInputRigLocomotionControl;

        /// <summary>
        /// Climbing Controller for this hand.
        /// </summary>
        [Tooltip("Climbing Controller for the hand.")]
#if UNITY_EDITOR
        [RequireInterface(typeof(IInputRigLocomotionControl))]
#endif
        public Object climbingController;
        protected IInputRigLocomotionControl _climbingController => climbingController as IInputRigLocomotionControl;

        /// <summary>
        /// UI Pointer Controller for the hand.
        /// </summary>
        [Tooltip("UI Pointer Controller for the hand.")]
        public UIPointerController uiPointerController;

        /// <summary>
        /// Drawing Pointer Controller for the hand.
        /// </summary>
        [Tooltip("Drawing Pointer Controller for the hand.")]
        public DrawingPointerController drawingPointerController;

        /// <summary>
        /// Whether or not the pointer is currently on.
        /// </summary>
        public virtual bool pointerOn
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The current endpoint of the pointer.
        /// </summary>
        public virtual Vector3 pointerEnd
        {
            get
            {
                return Vector3.zero;
            }
        }
        
        /// <summary>
        /// Velocity of the hand.
        /// </summary>
        public virtual Vector3 velocity
        {
            get
            {
                return Vector3.zero;
            }
        }

        public virtual bool teleportBlocked
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        /// <summary>
        /// Used to poll the menu pressing state.
        /// </summary>
        public virtual bool menuPressing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll menu press.
        /// </summary>
        public virtual bool menuPressed
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll selecting.
        /// </summary>
        public virtual bool selecting
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll select press.
        /// </summary>
        public virtual bool selectPressed
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll select value.
        /// </summary>
        public virtual float selectValue
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Used to poll grabbing.
        /// </summary>
        public virtual bool grabbing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll grab pressing.
        /// </summary>
        public virtual bool grabPressing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll the grab value.
        /// </summary>
        public virtual float grabValue
        {
            get
            {
                return 0f;
            }
        }

        /// <summary>
        /// Used to poll navigating.
        /// </summary>
        public virtual bool navigating
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll navigate pressing.
        /// </summary>
        public virtual bool navigatePressing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Used to poll navigate value.
        /// </summary>
        public virtual Vector2 navigateValue
        {
            get
            {
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Unity Start method called once during initialization prior to any calls to Update
        /// </summary>
        protected virtual void Start()
        {
            if (inputHand == null)
            {
                inputHand = GetComponent<InputHand>();
            }

            if (collider == null)
            {
                Debug.LogWarning("[InputHandSDK] " + nameof(collider) + " not set.");
            }

            if (target == null)
            {
                Debug.LogWarning("[InputHandSDK] " + nameof(target) + "not set.");
            }
        }

        /// <summary>
        /// Initialize method for SDK input hand wrapper.
        /// </summary>
        public virtual void Initialize()
        {
            //Debug.LogWarning("Initialize() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Moves the hand target to the supplied local position and local rotation
        /// </summary>
        /// <param name="localPosition">A <code>Vector3</code> representing the new local position of the target</param>
        /// <param name="localRotation">A <code>Quaternion</code> representing the new local rotation of the target</param>
        public virtual void MoveTarget(Vector3 localPosition, Quaternion localRotation)
        {
            // Adjust the target
            if (target != null)
            {
                target.localPosition = localPosition;
                target.localRotation = localRotation;

                // Invert the target y position adjustment to compensate for the move
                // so that the collider remains centered on the hand. x/z remains the same
                // because it is forward to compensate for the hand position.
                if (collider is CapsuleCollider)
                {
                    CapsuleCollider capsuleCollider = (collider as CapsuleCollider);
                    capsuleCollider.center = new Vector3(
                        capsuleCollider.center.x,
                        -(localPosition.y),
                        capsuleCollider.center.z);
                }
                else if (collider is BoxCollider)
                {
                    BoxCollider boxCollider = (collider as BoxCollider);
                    boxCollider.center = new Vector3(
                        boxCollider.center.x,
                        -(localPosition.y),
                        boxCollider.center.z);
                }
                else if (collider is SphereCollider)
                {
                    SphereCollider sphereCollider = (collider as SphereCollider);
                    sphereCollider.center = new Vector3(
                        sphereCollider.center.x,
                        -(localPosition.y),
                        sphereCollider.center.z);
                }
            }
        }

        /// <summary>
        /// Sets the hand object.
        /// </summary>
        /// <param name="handObject">Object to set to.</param>
        public virtual void SetHandObject(GameObject handObject)
        {
            Debug.LogWarning("SetHandObject() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Enters object interaction mode.
        /// </summary>
        public virtual void EnterObjectInteractionMode()
        {
            Debug.LogWarning("EnterObjectInteractionMode() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Exits object interaction mode.
        /// </summary>
        public virtual void ExitObjectInteractionMode()
        {
            Debug.LogWarning("ExitObjectInteractionMode() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Enters UI interaction mode.
        /// </summary>
        public virtual void EnterUIInteractionMode()
        {
            Debug.LogWarning("EnterUIInteractionMode() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Exits UI interaction mode.
        /// </summary>
        public virtual void ExitUIInteractionMode()
        {
            Debug.LogWarning("ExitUIInteractionMode() not implemented for InputHandSDK.");
        }

#region Locomotion

#region Locomotion [Teleport]

        /// <summary>
        /// The maximum distance for teleporting (meters).
        /// </summary>
        public float TeleportMaxDistance
        {
            set
            {
                // Set the controller distance if assigned
                if (teleportController)
                {
                    teleportController.maxDistance = value;
                }
                else
                {
                    Debug.LogWarning("TeleportController not defined for " + nameof(InputHandSDK));
                }
            }
            get
            {
                return (teleportController) ? teleportController.maxDistance : 0f;
            }
        }

        /// <summary>
        /// Enables teleportation interaction mode.
        /// </summary>
        public virtual void EnableTeleportation()
        {
            Debug.LogWarning("EnableTeleportation() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Disables teleportation interaction mode.
        /// </summary>
        public virtual void DisableTeleportation()
        {
            Debug.LogWarning("DisableTeleportation() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Turns the teleport laser on.
        /// </summary>
        public virtual void ToggleTeleportOn()
        {
            Debug.LogWarning("ToggleTeleportOn() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Turns the teleport laser off.
        /// </summary>
        public virtual void ToggleTeleportOff()
        {
            Debug.LogWarning("ToggleTeleportOff() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Performs the teleport.
        /// </summary>
        public virtual void CompleteTeleport()
        {
            Debug.LogWarning("CompleteTeleport() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Blocks teleporting.
        /// </summary>
        public virtual void BlockTeleport()
        {
            Debug.LogWarning("BlockTeleport() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Unblocks teleporting.
        /// </summary>
        public virtual void UnblockTeleport()
        {
            Debug.LogWarning("UnblockTeleport() not implemented for InputHandSDK.");
        }

#endregion // Locomotion [Teleport]

#region Locomotion [Armswing]

        /// <summary>
        /// Enables armswing for this hand.
        /// </summary>
        public virtual void EnableArmswing()
        {
            Debug.LogWarning("EnableArmswing() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Disables armswing for this hand.
        /// </summary>
        public virtual void DisableArmswing()
        {
            Debug.LogWarning("DisableArmswing() not implemented for InputHandSDK.");
        }

#endregion // Locomotion [Armswing]

#region Locomotion [Fly]

        /// <summary>
        /// Enables flying for this hand.
        /// </summary>
        public virtual void EnableFly()
        {
            Debug.LogWarning("EnableFly() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Disables flying for this hand.
        /// </summary>
        public virtual void DisableFly()
        {
            Debug.LogWarning("DisableFly() not implemented for InputHandSDK.");
        }

#endregion // Locomotion [Fly]

#region Locomotion [Navigate]

        /// <summary>
        /// Enables navigation for this hand.
        /// </summary>
        public virtual void EnableNavigate()
        {
            Debug.LogWarning("EnableNavigate() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Disables navigation for this hand.
        /// </summary>
        public virtual void DisableNavigate()
        {
            Debug.LogWarning("DisableNavigate() not implemented for InputHandSDK.");
        }

        #endregion // Locomotion [Navigate]

#region Locomotion [Climb]

        /// <summary>
        /// Enables climbing for this hand.
        /// </summary>
        public virtual void EnableClimb()
        {
            Debug.LogWarning("EnableClimb() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Disables climbing for this hand.
        /// </summary>
        public virtual void DisableClimb()
        {
            Debug.LogWarning("DisableClimb() not implemented for InputHandSDK.");
        }

        #endregion // Locomotion [Climb]

#endregion // Locomotion

        /// <summary>
        /// Turns the UI laser on.
        /// </summary>
        /// <param name="soft">Whether or not this is a soft disable. Soft enables can be disabled with soft
        /// or hard disables, while hard enables can only be disabled with hard disables.</param>
        /// <param name="showWhenInvalid">Whether or not to show the pointer in the invalid state.</param>
        public virtual void ToggleUIPointerOn(bool soft, bool showWhenInvalid)
        {
            Debug.LogWarning("ToggleUIPointerOn() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Turns the UI laser off.
        /// </summary>
        /// <param name="soft">Whether or not this is a soft disable. Soft enables can be disabled with soft
        /// or hard disables, while hard enables can only be disabled with hard disables.</param>
        public virtual void ToggleUIPointerOff(bool soft)
        {
            Debug.LogWarning("ToggleUIPointerOff() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Performs UI laser selection.
        /// </summary>
        public virtual void UIPointerSelect()
        {
            Debug.LogWarning("UIPointerSelect() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Turns the Drawing laser on.
        /// </summary>
        public virtual void ToggleDrawingPointerOn()
        {
            Debug.LogWarning("ToggleDrawingPointerOn() not implemented for InputHandSDK.");
        }

        /// <summary>
        /// Turns the Drawing laser off.
        /// </summary>
        public virtual void ToggleDrawingPointerOff()
        {
            Debug.LogWarning("ToggleDrawingPointerOff() not implemented for InputHandSDK.");
        }
    }
}