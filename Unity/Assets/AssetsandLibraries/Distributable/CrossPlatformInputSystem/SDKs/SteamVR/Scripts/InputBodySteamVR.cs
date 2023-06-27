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
    /// SteamVR wrapper for the body.
    /// Author: Jeffrey Hosler
    /// </summary>
	public class InputBodySteamVR : InputBodySDK
    {
        /// <summary>
        /// Reference to the Desktop input rig.
        /// </summary>
        [Tooltip("Reference to input rig.")]
        public InputRig inputRig;

        // Performance Management
        private int updateCounter = 0;
        [Tooltip("Modulates the frequency of state machine updates. The value represents a counter modulo to determine how many calls to Update will be skipped before updating.")]
        public int updateRateModulo = 1;

        /// <seealso cref="InputHeadSDK.Initialize"/>
        public override void Initialize()
        {
            if (inputRig == null)
            {
                inputRig = FindObjectOfType<InputRig>();
            }
            if (inputRig == null)
            {
                Debug.LogWarning("Initialize(); inputRig reference cannot be located " +
                    "so the player height cannot not be calculated.");
            }
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

                // Reposition the body collider if we have valid references
                if ((inputRig != null) && (inputRig.head != null) &&
                    (inputRig.head.Collider != null))
                {
                    // Offset the body collider to the x/z of the head collider. The collider could
                    // be offset from the head so we need to account for that.
                    transform.position = new Vector3(
                        inputRig.head.Collider.transform.position.x,
                        collider.transform.position.y,
                        inputRig.head.Collider.transform.position.z);

                    // Calculate the size of the body collider using the head collider and
                    // player height
                    float headTop = inputRig.head.Collider.bounds.max.y;
                    float headHeight = inputRig.head.Collider.bounds.size.y;
                    float bodyHeight = inputRig.PlayerHeight - headHeight;
                    float bodyBase = headTop - headHeight - bodyHeight;

                    // Get the adjusted body height in case the player is crouching.
                    // Assume a fixed head size, so just the body would be adjusted.
                    if (bodyBase < inputRig.transform.position.y)
                    {
                        bodyHeight -= inputRig.transform.position.y - bodyBase;
                        bodyBase = inputRig.transform.position.y;
                    }

                    // Update the collider dimensions based upon the type of the collider
                    if (collider is CapsuleCollider)
                    {
                        CapsuleCollider capsuleCollider = (collider as CapsuleCollider);

                        // Calculate the new collider dimensions, converting back to local space
                        Vector3 worldHeight = new Vector3(0, bodyHeight, 0);
                        capsuleCollider.height = gameObject.transform.InverseTransformVector(worldHeight).y;
                        capsuleCollider.center = gameObject.transform.InverseTransformPoint(
                            new Vector3(
                                transform.position.x,
                                bodyBase + (bodyHeight / 2f),
                                transform.position.z));
                    }
                    else if (collider is BoxCollider)
                    {
                        BoxCollider boxCollider = (collider as BoxCollider);

                        // Calculate the new collider dimensions, converting back to local space
                        Vector3 worldHeight = new Vector3(
                            boxCollider.bounds.extents.x,
                            (bodyHeight / 2f),
                            boxCollider.bounds.extents.z);
                        boxCollider.size = gameObject.transform.InverseTransformVector(worldHeight);
                        boxCollider.center = gameObject.transform.InverseTransformPoint(
                            new Vector3(
                                transform.position.x,
                                bodyBase + (bodyHeight / 2f),
                                transform.position.z));
                    }
                    else if (collider is SphereCollider)
                    {
                        SphereCollider sphereCollider = (collider as SphereCollider);

                        // Calculate the new collider dimensions, converting back to local space
                        Vector3 worldHeight = new Vector3(0, (bodyHeight / 2f), 0);
                        sphereCollider.radius = gameObject.transform.InverseTransformVector(worldHeight).y;
                        sphereCollider.center = gameObject.transform.InverseTransformPoint(
                            new Vector3(
                                transform.position.x,
                                bodyBase + (bodyHeight / 2f),
                                transform.position.z));
                    }
                }
            }
        }
    }
}