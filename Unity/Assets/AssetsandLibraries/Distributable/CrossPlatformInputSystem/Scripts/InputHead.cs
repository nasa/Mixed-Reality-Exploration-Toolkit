// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// </remarks>
    /// <summary>
    /// InputHead is a class that contains references
    /// and methods for a user's head.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHead : MonoBehaviour
    {
        /// <summary>
        /// Reference to the SDK wrapper for the input head.
        /// </summary>
        [Tooltip("Reference to the SDK wrapper for the input head.")]
        public InputHeadSDK inputHeadSDK;

        /// <summary>
        /// Initializes the input head.
        /// </summary>
        public void Initialize()
        {
            // Call sdk initialize.
            inputHeadSDK.Initialize();
        }
    }
}