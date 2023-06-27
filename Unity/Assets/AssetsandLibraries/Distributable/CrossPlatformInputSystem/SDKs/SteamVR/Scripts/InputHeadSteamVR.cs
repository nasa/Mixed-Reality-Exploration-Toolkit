// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.SteamVR
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

        /// <seealso cref="InputHeadSDK.Initialize"/>
        public override void Initialize()
        {
        }
    }
}