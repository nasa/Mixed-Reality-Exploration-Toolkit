// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
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
        /// The head target. This should not be the camera Transform itself, but a child
        /// <code>GameObject</code> parented by the head so that the 'target' can be
        /// repositioned and oriented within the head to align the transform to match the
        /// location of the camera/eyes.
        /// </summary>
        /// <returns>The <code>Transform</code> of the head target</returns>
        public Transform Target
        {
            get
            {
                return inputHeadSDK.target;
            }
        }

        /// <summary>
        /// The head collider.
        /// </summary>
        public Collider Collider
        {
            get
            {
                return inputHeadSDK.collider;
            }
        }

        /// <summary>
        /// Initializes the input head.
        /// </summary>
        public void Initialize()
        {
            // Call sdk initialize.
            inputHeadSDK.Initialize();
        }

        /// <summary>
        /// Moves the head target to the supplied local position and local rotation
        /// </summary>
        /// <param name="localPosition">A <code>Vector3</code> representing the new local position of the target</param>
        /// <param name="localRotation">A <code>Quaternion</code> representing the new local rotation of the target</param>
        public void MoveTarget(Vector3 localPosition, Quaternion localRotation)
        {
            inputHeadSDK.MoveTarget(localPosition, localRotation);
        }

    }
}