// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.SteamVR
{
    /// <remarks>
    /// History:
    /// 5 November 2020: Created
    /// 03 February 2021: Added ArmSwing locomotion (J. Hosler)
    /// 25 February 2021: Added a state machine implementation to address the issue where a
    ///     grip press by one controller was cancelling the grip press of the other controller
    ///     and then immediately cancelling the grip press of the first controller. This
    ///     affected the logic in the input system event model, so I disabled the event model
    ///     until we can confirm that the behavior works as expected. (J. Hosler)
    /// 17 March 2021: Removed the unused Steam VR head reference, wrapped debug log messages
    ///     in the CPIS_DEBUG compiler directive, and updated UpdateNavigateState() to support
    ///     the WindowsMixedReality which makes use of the secondary 2D axis for touchpad. (J. Hosler)
    /// 16 April 2021: Removing benign warning message if secondary 2D axis doesn't exist (as in
    ///     non-WMR headsets. Hiding warning message for missing menu button as there is
    ///     currently a SteamVR bug
    ///     https://www.reddit.com/r/Unity3D/comments/mnhtko/openxr_not_detecting_primary_button_of_htc_vive/ (DZB)
    /// 20 July 2021: SteamVR bug has been fixed, uncommenting code (DZB)
    /// 24 July 2021: Added Climbing locomotion (C. Lian)
    /// 28 July 2021: Adding support for velocity (DZB)
    /// 17 August 2021: Added pointer functions.
    /// 23 December 2021: Adding Drawing Laser (DZB)
    /// </remarks>
    /// <summary>
    /// SteamVR wrapper for the input hand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHandSteamVR : InputHandSDK
    {
        /// <summary>
        /// The name of this class
        ///
        public string ClassName => nameof(InputHandSteamVR);

        public const float NAVIGATION_TOLERANCE = 0.3f;

        /// <summary>
        /// Velocity of the hand.
        /// </summary>
        public override Vector3 velocity
        {
            get
            {
                Vector3 vel = Vector3.zero;
                if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out vel))
                {
                    Debug.LogWarning("[" + ClassName + "->" + nameof(velocity) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.deviceVelocity + "' does not exist.");
                    return Vector3.zero;
                }
                return vel;
            }
        }

        /// <summary>
        /// Used to poll the menu button in the pressing state.
        /// </summary>
        public override bool menuPressing
        {
            get
            {
                return _menuPressing;
            }
        }
        private bool _menuPressing = false;

        /// <summary>
        /// Used to poll menu press.
        /// </summary>
        public override bool menuPressed
        {
            get
            {
                return _menuPressed;
            }
        }
        private bool _menuPressed = false;

        /// <summary>
        /// Used to poll selecting.
        /// </summary>
        public override bool selecting
        {
            get
            {
                return _selecting;
            }
        }
        private bool _selecting = false;

        /// <summary>
        /// Used to poll select press.
        /// </summary>
        public override bool selectPressed
        {
            get
            {
                return _selectPressed;
            }
        }
        private bool _selectPressed = false;

        /// <summary>
        /// Used to poll select value.
        /// </summary>
        public override float selectValue
        {
            get
            {
                return _selectValue;
            }
        }
        private float _selectValue = 0;

        /// <summary>
        /// Used to poll grabbing.
        /// </summary>
        public override bool grabbing
        {
            get
            {
                return _grabbing;
            }
        }
        private bool _grabbing = false;

        /// <summary>
        /// Used to poll grab pressing.
        /// </summary>
        public override bool grabPressing
        {
            get
            {
                return _grabPressing;
            }
        }
        private bool _grabPressing = false;

        /// <summary>
        /// Used to poll the grab value.
        /// </summary>
        public override float grabValue
        {
            get
            {
                return _grabValue;
            }
        }
        private float _grabValue = 0f;

        /// <summary>
        /// Used to poll navigating.
        /// </summary>
        public override bool navigating
        {
            get
            {
                return _navigating;
            }
        }
        private bool _navigating = false;

        /// <summary>
        /// Used to poll navigate pressing.
        /// </summary>
        public override bool navigatePressing
        {
            get
            {
                return _navigatePressing;
            }
        }
        private bool _navigatePressing = false;

        /// <summary>
        /// Used to poll navigate value.
        /// </summary>
        public override Vector2 navigateValue
        {
            get
            {
                return _navigateValue;
            }
        }
        private Vector2 _navigateValue = Vector2.zero;

        /// <summary>
        /// Whether or not the pointer is currently on.
        /// </summary>
        public override bool pointerOn
        {
            get
            {
                if (uiPointerController.raycastLaser != null)
                {
                    if (uiPointerController.raycastLaser.active)
                    {
                        return true;
                    }
                }

                if (teleportController.raycastLaser != null)
                {
                    if (teleportController.raycastLaser.active)
                    {
                        return true;
                    }
                }

                if (drawingPointerController.raycastLaser != null)
                {
                    if (drawingPointerController.raycastLaser.active)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// The current endpoint of the pointer.
        /// </summary>
        public override Vector3 pointerEnd
        {
            get
            {
                // TODO: May need to support multiple pointers.
                if (drawingPointerController.raycastLaser != null)
                {
                    if (drawingPointerController.raycastLaser.active)
                    {
                        return drawingPointerController.raycastLaser.hitPos;
                    }
                }

                if (uiPointerController.raycastLaser != null)
                {
                    if (uiPointerController.raycastLaser.active)
                    {
                        return uiPointerController.raycastLaser.hitPos;
                    }
                }

                if (teleportController.raycastLaser != null)
                {
                    if (teleportController.raycastLaser.active)
                    {
                        return teleportController.raycastLaser.hitPos;
                    }
                }

                return Vector3.zero;
            }
        }

        public override bool teleportBlocked
        {
            get
            {
                return teleportController.TeleportBlocked;
            }
            set
            {
                if (value == true)
                {
                    BlockTeleport();
                }
                else
                {
                    UnblockTeleport();
                }
            }
        }

        // DZB: This showed up during a merge, results in an error.
        //private InputDevice currentDevice = null;

        // Performance Management
        private int updateCounter = 0;
        [Tooltip("Modulates the frequency of state machine updates. The value represents a counter modulo to determine how many calls to Update will be skipped before updating.")]
        public int updateRateModulo = 1;

        // Log tracking
        private bool warningReported = false;

#if XR_ISSUE_RESOLVED
        private UnityEngine.InputSystem.InputDevice currentDevice = null;
#endif
        private UnityEngine.XR.InputDevice inputDevice;

        #region XRStateMachine

        private bool initialized = false;

        /// <summary>
        /// Initializes the XR input device based upon the characteristics that define the input hand
        /// associated with this script instance.
        /// </summary>
        protected void InitializeInputDevice()
        {
            // Obtain the XR input device associated with this input hand
            InputDeviceCharacteristics inputDeviceCharacteristics = InputDeviceCharacteristics.None;
            if (inputHand.handedness == InputHand.Handedness.left)
            {
                inputDeviceCharacteristics = InputDeviceCharacteristics.Left;
            }
            else if (inputHand.handedness == InputHand.Handedness.right)
            {
                inputDeviceCharacteristics = InputDeviceCharacteristics.Right;
            }

            // Make sure we got a valid input device role
            if (inputDeviceCharacteristics != InputDeviceCharacteristics.None)
            {
                var inputDevices = new List<UnityEngine.XR.InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(inputDeviceCharacteristics, inputDevices);

                if (inputDevices.Count == 1)
                {
                    // Obtain the input device reference
                    inputDevice = inputDevices[0];
                    Debug.Log("[InputHandSteamVR] " + gameObject.name + "; " +
                        string.Format("XR input device name '{0}' with characteristics '{1}'",
                        inputDevice.name, inputDevice.characteristics.ToString()));

                    // Update the device state
                    UpdateState();

                    // This is the expected state so clear any warning tracking
                    warningReported = false;

                    // Mark as initialized
                    initialized = true;
                }
                else if (!warningReported)
                {
                    Debug.LogWarning("[InputHandSteamVR] " + gameObject.name + "; " +
                        string.Format("Unexpected number of XR input devices with characteristics '{0}' located. Count: {1}",
                        inputDevice.characteristics.ToString(), inputDevices.Count));

                    // Track that we reported the warning
                    warningReported = true;
                }
            }
            else if (!warningReported)
            {
                Debug.LogWarning("[InputHandSteamVR] " + gameObject.name + "; Input device characteristics could not be determined from the handedness of the InputHand.");

                // Track that we reported the warning
                warningReported = true;
            }
        }

        /// <summary>
        /// Updates the menu state of this InputHandSteamVR object
        /// </summary>
        protected virtual void UpdateMenuState()
        {
            // Check assertions
            if (!inputDevice.isValid) return;
            
            // Menu
            if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out _menuPressed))
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(UpdateMenuState) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.menuButton + "' does not exist.");
            }

            // Notify listeners if there was a change once all menu state values are obtained
            if (menuPressed && !menuPressing)
            {
#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateMenuState) + "; " + name + "] Menu pressed");
#endif

                // Begin the menu pressing state
                _menuPressing = menuPressed; // Before
                inputHand.MenuPressed(transform);
            }
            else if (!menuPressed && menuPressing)
            {
                // Complete the menu pressing state
                inputHand.MenuReleased(transform);
                _menuPressing = menuPressed; // After

#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateMenuState) + "; " + name + "] Menu released");
#endif
            }
        }

        /// <summary>
        /// Updates the select state of this InputHandSteamVR object
        /// </summary>
        protected virtual void UpdateSelectState()
        {
            // Check assertions
            if (!inputDevice.isValid) return;

            // Trigger/Select
            if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out _selectPressed))
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(UpdateSelectState) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.triggerButton + "' does not exist.");
            }

            // Trigger/Select Value
            if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out _selectValue))
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(UpdateSelectState) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.trigger + "' does not exist.");
            }

            // Notify listeners if there was a change once all select state values are obtained
            // MRET UMD change: Notify listeners if there was a change once all select state values are obtained
            if (selectPressed && !selecting && (_selectValue > 0.75))
            {
#if MRET_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateSelectState) + "; " + name + "] Selection beginning");
#endif

                // Begin the selection
                _selecting = true; // Before
                inputHand.SelectBegin();

            }
            else if (selecting && (_selectValue < 0.25))
            {
                // Complete the selection
                inputHand.SelectComplete();
                _selecting = false; // After

#if MRET_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateSelectState) + "; " + name + "] Selection complete");
#endif
            }
        }

        /// <summary>
        /// Updates the grab state of this InputHandSteamVR object
        /// </summary>
        protected virtual void UpdateGrabState()
        {
            // Check assertions
            if (!inputDevice.isValid) return;

            // Grip/Grab
            if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out _grabPressing))
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(UpdateGrabState) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.gripButton + "' does not exist.");
            }

            // Grip/Grab Value
            if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out _grabValue))
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(UpdateGrabState) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.grip + "' does not exist.");
            }

            // Notify listeners if there was a change once all grab state values are obtained
            if (grabPressing && !grabbing)
            {
#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateGrabState) + "; " + name + "] Grab beginning");
#endif

                // Begin the grab
                _grabbing = grabPressing; // Before
                inputHand.GrabBegin();
            }
            else if (!grabPressing && grabbing)
            {
                // Complete the grab
                inputHand.GrabComplete();
                _grabbing = grabPressing; // After

#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateGrabState) + "; " + name + "] Grab complete");
#endif
            }
        }

        /// <summary>
        /// Updates the navigate state of this InputHandSteamVR object
        /// </summary>
        protected virtual void UpdateNavigateState()
        {
            // Check assertions
            if (!inputDevice.isValid) return;

            // Reset the navigate value
            _navigateValue = Vector2.zero;

            // Joytick or Touchpad/Navigate
            bool primary2DAxisMoved = false;
            if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out _navigateValue))
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(UpdateNavigateState) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.primary2DAxis + "' does not exist.");
            }
            primary2DAxisMoved =
                (Mathf.Abs(_navigateValue.x) > NAVIGATION_TOLERANCE) ||
                (Mathf.Abs(_navigateValue.y) > NAVIGATION_TOLERANCE);

            // Joytick or Touchpad/Navigate Pressed
            if (!inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool primary2DAxisClicking))
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(UpdateNavigateState) + "; " + name + "] '" +
                    UnityEngine.XR.CommonUsages.primary2DAxisClick + "' does not exist.");
            }

            // Detect if we are toggling the navigation press
            if (primary2DAxisClicking && !navigatePressing)
            {
#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateNavigateState) + "; " + name + "] Navigate press beginning");
#endif

                // Begin the navigate press
                _navigatePressing = primary2DAxisClicking; // Before
                inputHand.NavigatePressBegin();
            }
            else if (!primary2DAxisClicking && navigatePressing)
            {
                // Complete the navigate press
                inputHand.NavigatePressComplete();
                _navigatePressing = primary2DAxisClicking; // After

#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateNavigateState) + "; " + name + "] Navigate press complete");
#endif
            }

            // Notify listeners if there was a change once all navigation state values are obtained
            if (primary2DAxisMoved && !navigating)
            {
#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateNavigateState) + "; " + name + "] Navigate beginning");
#endif

                // Begin the navigate
                _navigating = primary2DAxisMoved; // Before
                inputHand.NavigateBegin();
            }
            else if (!primary2DAxisMoved && navigating)
            {
                // Complete the navigate
                inputHand.NavigateComplete();
                _navigating = primary2DAxisMoved; // After

#if CPIS_DEBUG
                Debug.Log("[" + ClassName + "->" + nameof(UpdateNavigateState) + "; " + name + "] Navigate complete");
#endif
            }
        }

        /// <summary>
        /// Updates the state of this InputHandSteamVR object
        /// </summary>
        protected virtual void UpdateState()
        {
            // Check assertions
            if (!inputDevice.isValid) return;

            // Menu
            UpdateMenuState();

            // Trigger/Select
            UpdateSelectState();

            // Grip/Grab
            UpdateGrabState();

            // Touchpad/Navigate
            UpdateNavigateState();
        }

#endregion // XRStateMachine

        /// <summary>
        /// Overridden to obtain the correct input device associated with this hand
        /// </summary>
        /// <seealso cref="InputHandSDK.Start"/>
        protected override void Start()
        {
            // Take the inherited behavior
            base.Start();

            // Initialize the input device
//            InitializeInputDevice();
        }

        /// <summary>
        /// Unity Update method called once per frame
        /// </summary>
        private void Update()
        {
            // Performance management
            updateCounter++;
            if (updateCounter >= updateRateModulo)
            {
                // Reset the update counter
                updateCounter = 0;

                // Check that we have a valid input device and only try to initialize it if
                // we don't. This can occur if the user didn't power on the controller until
                // after Start is called
                if (!initialized || !inputDevice.isValid)
                {
                    // Try to initialize the input hand
                    InitializeInputDevice();
                }

                // Update the input hand state
                UpdateState();
            }
        }

        /// <summary>
        /// Handler for menu press event.
        /// </summary>
        /// <param name="callbackContext">InputSystem callback context.</param>
        public void MenuPressEvent(InputAction.CallbackContext callbackContext)
        {
#if XR_ISSUE_RESOLVED
            if (callbackContext.phase == InputActionPhase.Started)
            {
                inputHand.MenuPressed(transform);
                _menuPressed = true;
            }
            else if (callbackContext.phase == InputActionPhase.Canceled)
            {
                inputHand.MenuReleased(transform);
                _menuPressed = false;
            }
#endif
        }

        /// <summary>
        /// Handler for primary press event.
        /// </summary>
        /// <param name="callbackContext">InputSystem callback context.</param>
        public void PrimaryPressEvent(InputAction.CallbackContext callbackContext)
        {
#if XR_ISSUE_RESOLVED
            if (callbackContext.phase == InputActionPhase.Started)
            {
                inputHand.SelectBegin();
                _selectPressed = true;
            }
            else if (callbackContext.phase == InputActionPhase.Canceled)
            {
                inputHand.SelectComplete();
                _selectPressed = false;
            }
#endif
        }

        /// <summary>
        /// Handler for secondary press event.
        /// </summary>
        /// <param name="callbackContext">InputSystem callback context.</param>
        public void SecondaryPressEvent(InputAction.CallbackContext callbackContext)
        {
#if XR_ISSUE_RESOLVED
            if (callbackContext.started)
            {
                Debug.Log("[InputHandSteamVR] " + gameObject.name + "; " + inputHand.handedness + " Started");
                inputHand.GrabBegin();
                _grabbing = true;
            }
            else if (callbackContext.canceled)
            {
                Debug.Log("[InputHandSteamVR] " + gameObject.name + "; " + inputHand.handedness + " Canceled");
                inputHand.GrabComplete();
                _grabbing = false;
            }
#endif
        }

        /// <summary>
        /// Handler for touchpad press event.
        /// </summary>
        /// <param name="callbackContext">InputSystem callback context.</param>
        public void TouchpadPressEvent(InputAction.CallbackContext callbackContext)
        {
#if XR_ISSUE_RESOLVED
            if (callbackContext.phase == InputActionPhase.Started)
            {
                inputHand.NavigatePressBegin();
                _navigatePressing = true;
            }
            else if (callbackContext.phase == InputActionPhase.Canceled)
            {
                inputHand.NavigatePressComplete();
                _navigatePressing = false;
            }
#endif
        }

        /// <summary>
        /// Handler for touchpad press event.
        /// </summary>
        /// <param name="callbackContext">InputSystem callback context.</param>
        public void TouchpadTouchEvent(InputAction.CallbackContext callbackContext)
        {
#if XR_ISSUE_RESOLVED
            if (callbackContext.phase == InputActionPhase.Started)
            {
                inputHand.NavigateBegin();
                _navigating = true;
            }
            else if (callbackContext.phase == InputActionPhase.Canceled)
            {
                inputHand.NavigateComplete();
                _navigating = false;
            }

            if (callbackContext.action.actionMap.devices.HasValue)
            {
                currentDevice = callbackContext.action.actionMap.devices.Value[0];
            }
#endif
        }

        public override void Initialize()
        {
            ToggleUIPointerOn(true, false);
        }

        public override void EnterObjectInteractionMode()
        {
            base.EnterObjectInteractionMode();
        }

        public override void ExitObjectInteractionMode()
        {
            base.ExitObjectInteractionMode();
        }

        public override void EnterUIInteractionMode()
        {
            base.EnterUIInteractionMode();
        }

        public override void ExitUIInteractionMode()
        {
            base.ExitUIInteractionMode();
        }

#region Locomotion

#region Locomotion [Teleport]

        public override void EnableTeleportation()
        {
            teleportController.EnterMode();
        }

        public override void DisableTeleportation()
        {
            teleportController.ExitMode();
        }

        public override void ToggleTeleportOn()
        {
            teleportController.ToggleTeleportOn();
        }

        public override void ToggleTeleportOff()
        {
            teleportController.ToggleTeleportOff();
        }

        public override void CompleteTeleport()
        {
            teleportController.CompleteTeleport(inputHand.inputRig.gameObject);
        }

        public override void BlockTeleport()
        {
            teleportController.TeleportBlocked = true;
        }

        public override void UnblockTeleport()
        {
            teleportController.TeleportBlocked = false;
        }

#endregion // Locomotion [Teleport]

#region Locomotion [Armswing]

        /// <seealso cref="InputHandSDK.EnableArmswing"/>
        public override void EnableArmswing()
        {
            // Notify the locomotion controller that we are enabled
            if (_armswingController != null) _armswingController.SetHandActiveState(this.inputHand, true);
        }

        /// <seealso cref="InputHandSDK.DisableArmswing"/>
        public override void DisableArmswing()
        {
            // Notify the locomotion controller that we are disabled
            if (_armswingController != null) _armswingController.SetHandActiveState(this.inputHand, false);
        }

#endregion // Locomotion [Armswing]

#region Locomotion [Fly]

        /// <seealso cref="InputHandSDK.EnableFly"/>
        public override void EnableFly()
        {
            // Notify the locomotion controller that we are enabled
            if (_flyingController != null) _flyingController.SetHandActiveState(this.inputHand, true);
        }

        /// <seealso cref="InputHandSDK.DisableFly"/>
        public override void DisableFly()
        {
            // Notify the locomotion controller that we are disabled
            if (_flyingController != null) _flyingController.SetHandActiveState(this.inputHand, false);
        }

#endregion // Locomotion [Fly]

#region Locomotion [Navigate]

        /// <seealso cref="InputHandSDK.EnableNavigate"/>
        public override void EnableNavigate()
        {
            // Notify the locomotion controller that we are enabled
            if (_navigationController != null) _navigationController.SetHandActiveState(this.inputHand, true);
        }

        /// <seealso cref="InputHandSDK.DisableNavigate"/>
        public override void DisableNavigate()
        {
            // Notify the locomotion controller that we are disabled
            if (_navigationController != null) _navigationController.SetHandActiveState(this.inputHand, false);
        }

#endregion // Locomotion [Navigate]

#region Locomotion [Climb]

        /// <seealso cref="InputHandSDK.EnableClimb"/>
        public override void EnableClimb()
        {
            // Notify the locomotion controller that we are enabled
            if (_climbingController != null) _climbingController.SetHandActiveState(this.inputHand, true);
        }

        /// <seealso cref="InputHandSDK.DisableClimb"/>
        public override void DisableClimb()
        {
            // Notify the locomotion controller that we are disabled
            if (_climbingController != null) _climbingController.SetHandActiveState(this.inputHand, false);
        }

        #endregion // Locomotion [Climb]

#endregion // Locomotion

#region UI Handling
        public override void ToggleUIPointerOn(bool soft, bool showWhenInvalid)
        {
            //if (inputHand.interactionMode == InputHand.InteractionMode.UI)
            {
                uiPointerController.EnterMode();
                uiPointerController.ToggleUIPointingOn(soft);
                uiPointerController.showInvalid = showWhenInvalid;
            }
        }

        public override void ToggleUIPointerOff(bool soft)
        {
            //if (inputHand.interactionMode == InputHand.InteractionMode.UI)
            {
                uiPointerController.ToggleUIPointingOff(soft);
                uiPointerController.ExitMode();
            }
        }

        public override void UIPointerSelect()
        {
            //if (inputHand.interactionMode == InputHand.InteractionMode.UI)
            {
                uiPointerController.Select();
            }
        }
#endregion

#region Drawing
        public override void ToggleDrawingPointerOn()
        {
            drawingPointerController.EnterMode();
            drawingPointerController.TogglePointingOn();
        }

        public override void ToggleDrawingPointerOff()
        {
            drawingPointerController.TogglePointingOff();
            drawingPointerController.ExitMode();
        }
#endregion
    }
}