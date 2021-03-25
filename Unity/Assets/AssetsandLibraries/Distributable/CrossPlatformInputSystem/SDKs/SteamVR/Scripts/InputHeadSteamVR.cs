// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.SteamVR
{
    /// <remarks>
    /// History:
    /// 5 November 2020: Created
    /// </remarks>
    /// <summary>
    /// SteamVR wrapper for the head.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHeadSteamVR : InputHeadSDK
    {
        /// <summary>
        /// Reference to the SteamVR input hands.
        /// </summary>
        [Tooltip("Reference to SteamVR input hand.")]
        public InputHandSteamVR[] inputHandSteamVR = new InputHandSteamVR[2];
    }
}