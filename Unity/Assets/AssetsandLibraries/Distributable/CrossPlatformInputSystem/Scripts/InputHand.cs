// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// 03 February 2021: Added ArmSwing locomotion (J. Hosler)
    /// 17 August 2021: Added pointer functions.
    //  29 September 2021: Checking for teleport blocking when performing
    //  a teleport complete (D Baker)
    /// </remarks>
    /// <summary>
    /// InputHand is a class that contains references
    /// and methods for a user's hand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHand : MonoBehaviour
    {
        /// <summary>
        /// The input rig.
        /// </summary>
        [Tooltip("The input rig.")]
        public InputRig inputRig;

        /// <summary>
        /// Mode of what is being displayed for the controller.
        /// </summary>
        public enum ControllerMode { Initial, Controller, Hand }

        /// <summary>
        /// Mode the controller is in.
        /// </summary>
        public ControllerMode controllerMode { get; private set; } = ControllerMode.Initial;

        /// <summary>
        /// The maximum distance that will be used for the UI pointer (in meters).
        /// </summary>
        [Tooltip("The maximum distance that will be used for the UI pointer (in meters).")]
        public float maxUIPointerDistance = 100;

        /// <summary>
        /// Material to use for teleportation laser.
        /// </summary>
        [Tooltip("Material to use for teleportation laser.")]
        public Material teleportationLaserMaterial;

        /// <summary>
        /// Masterial to use for UI laser.
        /// </summary>
        [Tooltip("Material to use for UI laser.")]
        public Material uiLaserMaterial;

        /// <summary>
        /// Material to use for invalid laser.
        /// </summary>
        [Tooltip("Material to use for invalid laser.")]
        public Material invalidLaserMaterial;

        /// <summary>
        /// The object to use for the teleportation cursor.
        /// </summary>
        [Tooltip("The object to use for the teleportation cursor.")]
        public GameObject teleportationCursor;

        /// <summary>
        /// The object to use for the UI cursor.
        /// </summary>
        [Tooltip("The object to use for the UI cursor.")]
        public GameObject uiCursor;

        /// <summary>
        /// Scale to apply to the teleportation cursor.
        /// </summary>
        [Tooltip("Scale to apply to the teleportation cursor.")]
        public Vector3 teleportationCursorScale;

        /// <summary>
        /// Scale to apply to the UI cursor.
        /// </summary>
        [Tooltip("Scale to apply to the UI cursor.")]
        public Vector3 uiCursorScale;

        /// <summary>
        /// Whether or not the pointer is currently on.
        /// </summary>
        public bool pointerOn
        {
            get
            {
                return inputHandSDK.pointerOn;
            }
        }

        /// <summary>
        /// The current endpoint of the pointer.
        /// </summary>
        public Vector3 pointerEnd
        {
            get
            {
                return inputHandSDK.pointerEnd;
            }
        }

        /// <summary>
        /// Handedness of the controller.
        /// </summary>
        public enum Handedness { left, right, neutral }

        /// <summary>
        /// Handedness to set the controller to.
        /// </summary>
        [Tooltip("Handedness to set the controller to.")]
        public Handedness handedness = Handedness.neutral;

        /// <summary>
        /// Reference to the input hand SDK.
        /// </summary>
        [Tooltip("Reference to the input hand SDK.")]
        public InputHandSDK inputHandSDK;

        /// <summary>
        /// The prefab for the controller model to display.
        /// </summary>
        [Tooltip("The prefab for the controller model to display.")]
        public GameObject controllerPrefab;

        /// <summary>
        /// The prefab for the hand model to display.
        /// </summary>
        [Tooltip("The prefab for the hand model to display.")]
        public GameObject handPrefab;

        /// <summary>
        /// The event to call on menu press.
        /// </summary>
        [Tooltip("The event to call on menu press.")]
        public MenuPressEvent menuPressEvent;

        /// <summary>
        /// The event to call on menu release.
        /// </summary>
        [Tooltip("The event to call on menu release.")]
        public MenuPressEvent menuReleaseEvent;

        /// <summary>
        /// Used to poll menu press.
        /// </summary>
        public bool menuPressed
        {
            get
            {
                return inputHandSDK.menuPressed;
            }
        }

        /// <summary>
        /// The event to call when the hand initiates a selection.
        /// </summary>
        [Tooltip("The event to call when the hand initiates a selection.")]
        public UnityEvent selectBeginEvent;

        /// <summary>
        /// The event to call when the hand completes a selection.
        /// </summary>
        [Tooltip("The event to call when the hand completes a selection.")]
        public UnityEvent selectCompleteEvent;

        /// <summary>
        /// Used to poll select press.
        /// 
        /// Oculus: Trigger
        /// Vive: Trigger
        /// Knuckles: Trigger
        /// 
        /// Other Models: Most likely Trigger
        /// </summary>
        public bool selectPressed
        {
            get
            {
                return inputHandSDK.selectPressed;
            }
        }

        /// <summary>
        /// Used to poll select value.
        /// 
        /// 
        /// Oculus: Trigger
        /// Vive: Trigger
        /// Knuckles: Trigger
        /// 
        /// Other Models: Most likely Trigger
        /// </summary>
        public float selectValue
        {
            get
            {
                return inputHandSDK.selectValue;
            }
        }

        /// <summary>
        /// The event to call when the hand initiates a grab action.
        /// </summary>
        [Tooltip("The event to call when the hand initiates a grab.")]
        public InputHandEvent grabBeginEvent;

        /// <summary>
        /// The event to call when the hand completes a grab action.
        /// </summary>
        [Tooltip("The event to call when the hand completes a grab.")]
        public InputHandEvent grabCompleteEvent;

        /// <summary>
        /// Used to poll grabbing.
        /// </summary>
        public bool grabbing
        {
            get
            {
                return inputHandSDK.grabbing;
            }
        }

        /// <summary>
        /// The event to call when the hand initiates a navigate action.
        /// </summary>
        [Tooltip("The event to call when the hand initiates a navigate action.")]
        public InputHandEvent navigateBeginEvent;

        /// <summary>
        /// The event to call when the hand completes a navigate action.
        /// </summary>
        [Tooltip("The event to call when the hand completes a navigate action.")]
        public InputHandEvent navigateCompleteEvent;

        /// <summary>
        /// Used to poll navigating.
        /// </summary>
        public bool navigating
        {
            get
            {
                return inputHandSDK.navigating;
            }
        }

        /// <summary>
        /// The event to call when the hand initiates a navigate press action.
        /// </summary>
        [Tooltip("The event to call when the hand initiates a navigate press action.")]
        public InputHandEvent navigatePressBeginEvent;

        /// <summary>
        /// The event to call when the hand completes a navigate press action.
        /// </summary>
        [Tooltip("The event to call when the hand completes a navigate press action.")]
        public InputHandEvent navigatePressCompleteEvent;

        /// <summary>
        /// Used to poll navigate pressing.
        /// 
        /// Oculus: Joystick
        /// Vive: Touchpad
        /// Knuckles: Joystick
        /// 
        /// Other Models: Most likely Joystick
        /// </summary>
        public bool navigatePressing
        {
            get
            {
                return inputHandSDK.navigatePressing;
            }
        }

        /// <summary>
        /// Used to poll navigate value.
        /// 
        /// Oculus: Joystick
        /// Vive: Touchpad
        /// Knuckles: Joystick
        /// 
        /// Other Models: Most likely Joystick
        /// </summary>
        public Vector2 navigateValue
        {
            get
            {
                return inputHandSDK.navigateValue;
            }
        }

        /// <summary>
        /// The active hand model.
        /// </summary>
        public GameObject activeHandModel { get; private set; }

        /// <summary>
        /// Initializes the input hand.
        /// </summary>
        /// <param name="controllerMode">Mode to set the controller to.</param>
        public void Initialize(ControllerMode controllerMode = ControllerMode.Controller)
        {
            SwitchMode(controllerMode);
            inputHandSDK.Initialize();
        }

        /// <summary>
        /// Switch to the given mode.
        /// </summary>
        /// <param name="modeToSet">Mode to set the controller to.</param>
        public void SwitchMode(ControllerMode modeToSet)
        {
            if (modeToSet == controllerMode)
            {
                Debug.LogWarning("[InputHand] Already in mode.");
                return;
            }

            // If not already in mode, instantiate appropriate prefab and position it.
            if (controllerPrefab != null)
            {
                activeHandModel = Instantiate(controllerPrefab);
                if (activeHandModel == null)
                {
                    Debug.LogError("[InputHand] Error instantiating model.");
                    return;
                }
                activeHandModel.transform.SetParent(transform);
                activeHandModel.transform.localPosition = Vector3.zero;
                activeHandModel.transform.localRotation = Quaternion.identity;
                inputHandSDK.SetHandObject(activeHandModel);
            }
        }

        /// <summary>
        /// Toggles the hand model on or off.
        /// </summary>
        /// <param name="on">Whether to toggle the hand on or off.</param>
        public void ToggleHandModel(bool on)
        {
            if (activeHandModel != null)
            {
                activeHandModel.SetActive(on);
            }
        }

        /// <summary>
        /// Invokes menu press event.
        /// </summary>
        /// <param name="hand">Transform of hand.</param>
        public void MenuPressed(Transform hand)
        {
            menuPressEvent.Invoke(hand);
        }

        /// <summary>
        /// Invokes menu release event.
        /// </summary>
        /// <param name="hand">Transform of hand.</param>
        public void MenuReleased(Transform hand)
        {
            menuReleaseEvent.Invoke(hand);
        }

        /// <summary>
        /// Invokes select action initiated event.
        /// </summary>
        public void SelectBegin()
        {
            selectBeginEvent.Invoke();
        }

        /// <summary>
        /// Invokes select action completed event.
        /// </summary>
        public void SelectComplete()
        {
            selectCompleteEvent.Invoke();
        }

        /// <summary>
        /// Invokes grab action initiated event.
        /// </summary>
        public void GrabBegin()
        {
            grabBeginEvent.Invoke(this);
        }

        /// <summary>
        /// Invokes grab action completed event.
        /// </summary>
        public void GrabComplete()
        {
            grabCompleteEvent.Invoke(this);
        }

        /// <summary>
        /// Invokes navigate press action initiated event.
        /// </summary>
        public void NavigatePressBegin()
        {
            navigatePressBeginEvent.Invoke(this);
        }

        /// <summary>
        /// Invokes a navigate press action completed event.
        /// </summary>
        public void NavigatePressComplete()
        {
            navigatePressCompleteEvent.Invoke(this);
        }

        /// <summary>
        /// Invokes navigate action initiated event.
        /// </summary>
        public void NavigateBegin()
        {
            navigateBeginEvent.Invoke(this);
        }

        /// <summary>
        /// Invokes a navigate action completed event.
        /// </summary>
        public void NavigateComplete()
        {
            navigateCompleteEvent.Invoke(this);
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
                inputHandSDK.TeleportMaxDistance = value;
            }
            get
            {
                return inputHandSDK.TeleportMaxDistance;
            }
        }

        /// <summary>
        /// Enables Teleporting.
        /// </summary>
        public void EnableTeleport()
        {
            inputHandSDK.EnableTeleportation();
        }

        /// <summary>
        /// Disables teleporting.
        /// </summary>
        public void DisableTeleport()
        {
            inputHandSDK.DisableTeleportation();
        }

        /// <summary>
        /// Turns the teleport laser on.
        /// </summary>
        public void ToggleTeleportOn()
        {
            if (inputHandSDK.teleportBlocked == false)
            {
                ToggleUIPointerOff(true);
                inputHandSDK.ToggleTeleportOn();
            }
        }

        /// <summary>
        /// Turns the teleport laser off.
        /// </summary>
        public void ToggleTeleportOff()
        {
            if (inputHandSDK.teleportBlocked == false)
            {
                inputHandSDK.ToggleTeleportOff();
                ToggleUIPointerOn(true, false);
            }
        }

        /// <summary>
        /// Performs the teleport.
        /// </summary>
        public void CompleteTeleport()
        {
            if (inputHandSDK.teleportBlocked == false)
            {
                inputHandSDK.CompleteTeleport();
            }
        }

        /// <summary>
        /// Blocks teleporting.
        /// </summary>
        public void BlockTeleport()
        {
            inputHandSDK.BlockTeleport();
        }

        /// <summary>
        /// Unblocks teleporting.
        /// </summary>
        public void UnblockTeleport()
        {
            inputHandSDK.UnblockTeleport();
        }

        #endregion // Locomotion [Teleport]

        #region Locomotion [Armswing]

        /// <summary>
        /// Enables armswing for this hand.
        /// </summary>
        public void EnableArmswing()
        {
            inputHandSDK.EnableArmswing();
        }

        /// <summary>
        /// Disables armswing for this hand.
        /// </summary>
        public void DisableArmswing()
        {
            inputHandSDK.DisableArmswing();
        }

        #endregion // Locomotion [Armswing]

        #region Locomotion [Fly]

        /// <summary>
        /// Enables flying for this hand.
        /// </summary>
        public void EnableFly()
        {
            inputHandSDK.EnableFly();
        }

        /// <summary>
        /// Disables flying for this hand.
        /// </summary>
        public void DisableFly()
        {
            inputHandSDK.DisableFly();
        }

        #endregion // Locomotion [Fly]

        #region Locomotion [Navigate]

        /// <summary>
        /// Enables navigation for this hand.
        /// </summary>
        public void EnableNavigate()
        {
            inputHandSDK.EnableNavigate();
        }

        /// <summary>
        /// Disables navigation for this hand.
        /// </summary>
        public void DisableNavigate()
        {
            inputHandSDK.DisableNavigate();
        }

        #endregion // Locomotion [Navigate]

        #endregion // Locomotion

        #region UI Handling
        /// <summary>
        /// Turns the UI laser on.
        /// </summary>
        /// <param name="soft">Whether or not this is a soft enable. Soft enables can be disabled with soft
        /// or hard disables, while hard enables can only be disabled with hard disables.</param>
        /// <param name="showWhenInvalid">Whether or not to show the pointer in the invalid state.</param>
        public void ToggleUIPointerOn(bool soft, bool showWhenInvalid)
        {
            inputHandSDK.ToggleUIPointerOn(soft, showWhenInvalid);
        }

        /// <summary>
        /// Turns the UI laser off.
        /// </summary>
        /// /// <param name="soft">Whether or not this is a soft disable. Soft enables can be disabled with soft
        /// or hard disables, while hard enables can only be disabled with hard disables.</param>
        public void ToggleUIPointerOff(bool soft)
        {
            inputHandSDK.ToggleUIPointerOff(soft);
        }

        /// <summary>
        /// Performs UI laser selection.
        /// </summary>
        public void UIPointerSelect()
        {
            inputHandSDK.UIPointerSelect();
        }
        #endregion
    }

    /// <summary>
    /// Unity Event that takes relevant parameters to menu press event.
    /// </summary>
    [System.Serializable]
    public class MenuPressEvent : UnityEvent<Transform>
    {

    }

    /// <summary>
    /// Unity Event that takes an InputHand paramerter.
    /// </summary>
    [System.Serializable]
    public class InputHandEvent : UnityEvent<InputHand>
    {

    }

}