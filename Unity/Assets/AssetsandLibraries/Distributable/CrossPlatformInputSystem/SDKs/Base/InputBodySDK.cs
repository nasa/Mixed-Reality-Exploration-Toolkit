// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base
{
    /// <remarks>
    /// History:
    /// 05 September 2020: Created
    /// </remarks>
    /// <summary>
    /// SDK wrapper for the body.
    /// Author: Jeffrey Hosler
    /// </summary>
	public class InputBodySDK : MonoBehaviour
    {
        /// <summary>
        /// Reference to the input body class.
        /// </summary>
        [Tooltip("Reference to the input body class.")]
        public InputBody inputBody;

        /// <summary>
        /// The body collider.
        /// </summary>
        public new Collider collider;

        private void Start()
        {
            if (inputBody == null)
            {
                inputBody = GetComponent<InputBody>();
            }

            if (collider == null)
            {
                collider = GetComponent<Collider>();
            }
        }

        /// <summary>
        /// Initialize method for SDK input body wrapper.
        /// </summary>
        public virtual void Initialize()
        {
            Debug.LogWarning("Initialize() not implemented for InputBodySDK.");
        }
    }
}
