// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Desktop
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// 22 July 2021: Inverting rotation lock behavior (DZB)
    /// </remarks>
    /// <summary>
    /// Desktop wrapper for the head.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHeadDesktop : InputHeadSDK
    {
        /// <summary>
        /// Reference to the desktop input hand.
        /// </summary>
        [Tooltip("Reference to desktop input hand.")]
        public InputHandDesktop inputHandDesktop;

        /// <summary>
        /// Transform of the controlled head.
        /// </summary>
        [Tooltip("Transform of the controlled head.")]
        public Transform controlled;

        /// <summary>
        /// Whether rotation is locked.
        /// </summary>
        [Tooltip("Whether rotation is locked.")]
        public bool rotationLocked = true;

        /// <summary>
        /// Speed of rotation.
        /// </summary>
        [Tooltip("Speed of rotation.")]
        public float rotationSpeed = 0.1f;

        /// <summary>
        /// Min and max vertical rotation angles.
        /// </summary>
        [Tooltip("Min and max vetical rotation angles.")]
        public Vector2 pitchRange = new Vector2(-90, 90);

        /// <summary>
        /// Current pitch to set for the head.
        /// </summary>
        private float playerPitch = 0f;
        /// <summary>
        /// Current yaw to set for the head.
        /// </summary>
        private float playerYaw = 0f;

        /// <summary>
        /// Handler for look event.
        /// </summary>
        /// <param name="callbackContext">InputSystem callback context.</param>
        public void LookEvent(InputAction.CallbackContext callbackContext)
        {
            if (!rotationLocked)
            {
                return;
            }

            // If rotation isn't locked, calculate pitch and yaw and set controlled to that.
            Vector2 look = callbackContext.ReadValue<Vector2>();
            if (look != null)
            {
                playerPitch = BetweenPlusAndMinusand180(playerPitch - look.y * rotationSpeed);
                if (playerPitch > pitchRange.y)
                {
                    playerPitch = pitchRange.y;
                }

                if (playerPitch < pitchRange.x)
                {
                    playerPitch = pitchRange.x;
                }

                playerYaw = BetweenPlusAndMinusand180(playerYaw + look.x * rotationSpeed);
            }

            controlled.eulerAngles = new Vector3(playerPitch, playerYaw, 0);
        }

        /// <summary>
        /// Normalize an angle between -/+180.
        /// </summary>
        /// <param name="input">Raw value.</param>
        /// <returns>Raw value normalized.</returns>
        private float BetweenPlusAndMinusand180(float input)
        {
            float rtn = input;

            // Subtract 360 until less than or equal to 180.
            while (rtn >= 180)
            {
                rtn -= 360;
            }

            // Add 360 until greater than or equal to -180.
            while (rtn <= -180)
            {
                rtn += 360;
            }

            return rtn;
        }
    }
}