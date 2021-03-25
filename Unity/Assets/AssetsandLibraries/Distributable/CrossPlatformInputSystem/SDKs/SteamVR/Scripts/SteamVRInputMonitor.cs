// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.SteamVR
{
    /// <summary>
    /// Unity Event with Float Parameter.
    /// </summary>
    [System.Serializable]
    public class FloatEvent : UnityEvent<float>
    {

    }

    /// <summary>
    /// Unity Event with Boolean Parameter.
    /// </summary>
    [System.Serializable]
    public class BooleanEvent : UnityEvent<bool>
    {

    }

    /// <summary>
    /// Unity Event with Vector2 Parameter.
    /// </summary>
    [System.Serializable]
    public class Vector2Event : UnityEvent<Vector2>
    {

    }

    /// <remarks>
    /// History:
    /// 24 November 2020: Created
    /// </remarks>
    /// <summary>
    /// Input monitor for SteamVR input events.
    /// This script exists solely because of incompatibilities
    /// between Unity and Valve. As a result, SteamVR and Unity's
    /// XR Input System are incompatible when it comes to getting
    /// device input. It is my sincere hope that this script
    /// becomes obsolete.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class SteamVRInputMonitor : MonoBehaviour
    {
        public string thing;
        public static SteamVRInputMonitor instance;

        private static readonly string leftTriggerID = "XRI_Left_Trigger",
            rightTriggerID = "XRI_Right_Trigger", leftGripID = "XRI_Left_Grip",
            rightGripID = "XRI_Right_Grip", /*leftMenuID = "XRI_Left_MenuButton",*/
            leftMenuID = "Fire3",
            rightMenuID = "XRI_Right_MenuButton",
            leftVerticalTouchID = "XRI_Left_Primary2DAxis_Vertical",
            leftHorizontalTouchID = "XRI_Left_Primary2DAxis_Horizontal",
            rightVerticalTouchID = "XRI_Right_Primary2DAxis_Vertical",
            rightHorizontalTouchID = "XRI_Right_Primary2DAxis_Horizontal",
            leftTouchpadPressID = "Left Touchpad Press", rightTouchpadPressID = "Right Touchpad Press";


        // Trigger.

        [Tooltip("Event to call when left trigger state is changed.")]
        public FloatEvent leftTriggerChangeEvent;

        [Tooltip("Event to call when left trigger is pressed.")]
        public UnityEvent leftTriggerPressEvent;

        [Tooltip("Event to call when left trigger is released.")]
        public UnityEvent leftTriggerReleaseEvent;

        [Tooltip("Event to call when right trigger state is changed.")]
        public FloatEvent rightTriggerChangeEvent;

        [Tooltip("Event to call when right trigger is pressed.")]
        public UnityEvent rightTriggerPressEvent;

        [Tooltip("Event to call when right trigger is released.")]
        public UnityEvent rightTriggerReleaseEvent;

        /// <summary>
        /// Previous state of the left trigger.
        /// </summary>
        private float previousLeftTriggerState = 0;

        /// <summary>
        /// Previous state of the right trigger.
        /// </summary>
        private float previousRightTriggerState = 0;

        /// <summary>
        /// State of the left trigger.
        /// </summary>
        public float leftTrigger
        {
            get
            {
                return previousLeftTriggerState;
            }
        }

        /// <summary>
        /// State of the right trigger.
        /// </summary>
        public float rightTrigger
        {
            get
            {
                return previousRightTriggerState;
            }
        }



        // Grip.

        [Tooltip("Event to call wen left grip state is changed.")]
        public BooleanEvent leftGripChangeEvent;

        [Tooltip("Event to call when left grip is pressed.")]
        public UnityEvent leftGripPressEvent;

        [Tooltip("Event to call when left grip is released.")]
        public UnityEvent leftGripReleaseEvent;

        [Tooltip("Event to call wen right grip state is changed.")]
        public BooleanEvent rightGripChangeEvent;

        [Tooltip("Event to call when right grip is pressed.")]
        public UnityEvent rightGripPressEvent;

        [Tooltip("Event to call when right grip is released.")]
        public UnityEvent rightGripReleaseEvent;

        /// <summary>
        /// Previous state of the left grip.
        /// </summary>
        private bool previousLeftGripState = false;

        /// <summary>
        /// Previous state of the right grip.
        /// </summary>
        private bool previousRightGripState = false;

        /// <summary>
        /// State of the left grip.
        /// </summary>
        public bool leftGrip
        {
            get
            {
                return previousLeftGripState;
            }
        }

        /// <summary>
        /// State of the right grip.
        /// </summary>
        public bool rightGrip
        {
            get
            {
                return previousRightGripState;
            }
        }



        // Menu.

        [Tooltip("Event to call when left menu state is changed.")]
        public BooleanEvent leftMenuChangeEvent;

        [Tooltip("Event to call when left menu is pressed.")]
        public UnityEvent leftMenuPressEvent;

        [Tooltip("Event to call when left menu is released.")]
        public UnityEvent leftMenuReleaseEvent;

        [Tooltip("Event to call when right menu state is changed.")]
        public BooleanEvent rightMenuChangeEvent;

        [Tooltip("Event to call when right menu is pressed.")]
        public UnityEvent rightMenuPressEvent;

        [Tooltip("Event to call when right menu is released.")]
        public UnityEvent rightMenuReleaseEvent;

        /// <summary>
        /// Previous state of the left menu.
        /// </summary>
        private bool previousLeftMenuState = false;

        /// <summary>
        /// Previous state of the right menu.
        /// </summary>
        private bool previousRightMenuState = false;

        /// <summary>
        /// State of the left menu.
        /// </summary>
        public bool leftMenu
        {
            get
            {
                return previousLeftMenuState;
            }
        }

        /// <summary>
        /// State of the right menu.
        /// </summary>
        public bool rightMenu
        {
            get
            {
                return previousRightMenuState;
            }
        }

        // Touchpad.

        [Tooltip("Event to call when left touchpad touching state is changed.")]
        public BooleanEvent leftTouchpadTouchingChangeEvent;

        [Tooltip("Event to call when left touchpad press state is changed.")]
        public BooleanEvent leftTouchpadPressChangeEvent;

        [Tooltip("Event to call when left touchpad axis value is changed.")]
        public Vector2Event leftTouchpadAxisChangeEvent;

        [Tooltip("Event to call when right touchpad touching state is changed.")]
        public BooleanEvent rightTouchpadTouchingChangeEvent;

        [Tooltip("Event to call when right touchpad press state is changed.")]
        public BooleanEvent rightTouchpadPressChangeEvent;

        [Tooltip("Event to call when right touchpad axis value is changed.")]
        public Vector2Event rightTouchpadAxisChangeEvent;

        /// <summary>
        /// Previous touch state of the left touchpad.
        /// </summary>
        private Vector2 previousLeftTouchpadTouchState = Vector2.zero;

        /// <summary>
        /// Previous press state of the left touchpad.
        /// </summary>
        private bool previousLeftTouchpadPressState = false;

        /// <summary>
        /// Previous touch state of the right touchpad.
        /// </summary>
        private Vector2 previousRightTouchpadTouchState = Vector2.zero;

        /// <summary>
        /// Previous press state of the right touchpad.
        /// </summary>
        private bool previousRightTouchpadPressState = false;

        /// <summary>
        /// Touching state of the left touchpad.
        /// </summary>
        public bool leftTouchpadTouching
        {
            get
            {
                return previousLeftTouchpadTouchState != Vector2.zero;
            }
        }

        /// <summary>
        /// State of the left touchpad.
        /// </summary>
        public Vector2 leftTouchpadTouch
        {
            get
            {
                return previousLeftTouchpadTouchState;
            }
        }

        /// <summary>
        /// Pressing state of the left touchpad.
        /// </summary>
        public bool leftTouchpadPressing
        {
            get
            {
                return previousLeftTouchpadPressState;
            }
        }

        /// <summary>
        /// Touching state of the right touchpad.
        /// </summary>
        public bool rightTouchpadTouching
        {
            get
            {
                return previousRightTouchpadTouchState != Vector2.zero;
            }
        }

        /// <summary>
        /// State of the right touchpad.
        /// </summary>
        public Vector2 rightTouchpadTouch
        {
            get
            {
                return previousRightTouchpadTouchState;
            }
        }

        /// <summary>
        /// Pressing state of the right touchpad.
        /// </summary>
        public bool rightTouchpadPressing
        {
            get
            {
                return previousRightTouchpadPressState;
            }
        }


        void PerformSuperInefficientCheckOnAllControllerInputs()
        {
            // Left trigger.
            float lt = Input.GetAxis(leftTriggerID);
            if (lt != previousLeftTriggerState)
            {
                previousLeftTriggerState = lt;
                leftTriggerChangeEvent.Invoke(lt);

                if (lt == 1)
                {
                    leftTriggerPressEvent.Invoke();
                }
                else if (lt == 0)
                {
                    leftTriggerReleaseEvent.Invoke();
                }
            }

            // Right trigger.
            float rt = Input.GetAxis(rightTriggerID);
            if (rt != previousRightTriggerState)
            {
                previousRightTriggerState = rt;
                rightTriggerChangeEvent.Invoke(rt);

                if (rt == 1)
                {
                    rightTriggerPressEvent.Invoke();
                }
                else if (rt == 0)
                {
                    rightTriggerReleaseEvent.Invoke();
                }
            }

            // Left grip.
            bool lg = Input.GetAxis(leftGripID) == 1;
            if (lg != previousLeftGripState)
            {
                previousLeftGripState = lg;
                leftGripChangeEvent.Invoke(lg);
                if (lg == true)
                {
                    leftGripPressEvent.Invoke();
                }
                else if (lg == false)
                {
                    leftGripReleaseEvent.Invoke();
                }
            }

            // Right grip.
            bool rg = Input.GetAxis(rightGripID) == 1;
            if (rg != previousRightGripState)
            {
                previousRightGripState = rg;
                rightGripChangeEvent.Invoke(rg);
                if (rg == true)
                {
                    rightGripPressEvent.Invoke();
                }
                else if (rg == false)
                {
                    rightGripReleaseEvent.Invoke();
                }
            }

            // Left menu.
            bool lm = Input.GetAxis(leftMenuID) == 1;
            if (lm != previousLeftMenuState)
            {
                previousLeftMenuState = lm;
                leftMenuChangeEvent.Invoke(lm);
                if (lm == true)
                {
                    leftMenuPressEvent.Invoke();
                }
                else if (lm == false)
                {
                    leftMenuReleaseEvent.Invoke();
                }
            }

            // Right menu.
            bool rm = Input.GetAxis(rightMenuID) == 1;
            if (rm != previousRightMenuState)
            {
                previousRightMenuState = rm;
                rightMenuChangeEvent.Invoke(rm);
                if (rm == true)
                {
                    rightMenuPressEvent.Invoke();
                }
                else if (rm == false)
                {
                    rightMenuReleaseEvent.Invoke();
                }
            }
            
            // Left touchpad.
            Vector2 ltt = new Vector2(Input.GetAxis(leftHorizontalTouchID),
                Input.GetAxis(leftVerticalTouchID));
            bool ltp = Input.GetAxis(leftTouchpadPressID) == 1;
            if (ltt != previousLeftTouchpadTouchState)
            {
                leftTouchpadAxisChangeEvent.Invoke(ltt);
                if (ltt == Vector2.zero)
                {
                    if (previousLeftTouchpadTouchState != Vector2.zero)
                    {
                        // Previously touching, not any more.
                        leftTouchpadTouchingChangeEvent.Invoke(false);
                    }
                }
                else
                {
                    if (previousLeftTouchpadTouchState == Vector2.zero)
                    {
                        // Previously not touching, now is.
                        leftTouchpadTouchingChangeEvent.Invoke(true);
                    }
                }
                previousLeftTouchpadTouchState = ltt;
            }
            if (ltp != previousLeftTouchpadPressState)
            {
                previousLeftTouchpadPressState = ltp;
                leftTouchpadPressChangeEvent.Invoke(ltp);
            }

            // Right touchpad.
            Vector2 rtt = new Vector2(Input.GetAxis(rightHorizontalTouchID),
                Input.GetAxis(rightVerticalTouchID));
            bool rtp = Input.GetAxis(rightTouchpadPressID) == 1;
            if (rtt != previousRightTouchpadTouchState)
            {
                rightTouchpadAxisChangeEvent.Invoke(rtt);
                if (rtt == Vector2.zero)
                {
                    if (previousRightTouchpadTouchState != Vector2.zero)
                    {
                        // Previously touching, not any more.
                        rightTouchpadTouchingChangeEvent.Invoke(false);
                    }
                }
                else
                {
                    if (previousRightTouchpadTouchState == Vector2.zero)
                    {
                        // Previously not touching, now is.
                        rightTouchpadTouchingChangeEvent.Invoke(true);
                    }
                }
                previousRightTouchpadTouchState = rtt;
            }
            if (rtp != previousRightTouchpadPressState)
            {
                previousRightTouchpadPressState = rtp;
                rightTouchpadPressChangeEvent.Invoke(rtp);
            }
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            PerformSuperInefficientCheckOnAllControllerInputs();
        }
    }
}