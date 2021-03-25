// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base
{
    /// <remarks>
    /// History:
    /// 27 October 2020: Created
    /// </remarks>
    /// <summary>
    /// SDK wrapper for the head.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHeadSDK : MonoBehaviour
    {
        /// <summary>
        /// Reference to the input head class.
        /// </summary>
        [Tooltip("Reference to the input head class.")]
        public InputHead inputHead;

        private void Start()
        {
            if (inputHead == null)
            {
                inputHead = GetComponent<InputHead>();
            }
        }

        /// <summary>
        /// Initialize method for SDK input head wrapper.
        /// </summary>
        public void Initialize()
        {
            Debug.LogWarning("Initialize() not implemented for InputHeadSDK.");
        }
    }
}