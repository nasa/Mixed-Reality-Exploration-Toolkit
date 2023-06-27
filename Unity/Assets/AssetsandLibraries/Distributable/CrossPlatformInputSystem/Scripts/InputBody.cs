// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
{
    /// <remarks>
    /// History:
    /// 05 September 2022: Created
    /// </remarks>
    /// <summary>
    /// InputBody is a class that is an abstract representation of a user's body.
    /// Author: Jeffrey Hosler
    /// </summary>
	public class InputBody : MonoBehaviour
	{
        /// <summary>
        /// Reference to the SDK wrapper for the input body.
        /// </summary>
        [Tooltip("Reference to the SDK wrapper for the input body.")]
        public InputBodySDK inputBodySDK;

        /// <summary>
        /// A reference to the body collider. Null if none.
        /// </summary>
        public new Collider collider
        {
            get
            {
                return inputBodySDK.collider;
            }
        }

        /// <summary>
        /// Initializes the input body.
        /// </summary>
        public void Initialize()
        {
            // Call sdk initialize.
            inputBodySDK.Initialize();
        }
    }
}
