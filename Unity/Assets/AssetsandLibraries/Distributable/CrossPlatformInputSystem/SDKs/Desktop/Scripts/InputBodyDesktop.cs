using UnityEngine;
using UnityEngine.InputSystem;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Desktop
{
    /// <remarks>
    /// History:
    /// 06 September 2022: Created
    /// </remarks>
    /// <summary>
    /// Desktop wrapper for the rig body.
    /// Author: Jeffrey Hosler
    /// </summary>
	public class InputBodyDesktop : InputBodySDK
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
                    if (collider is CharacterController)
                    {
                        CharacterController characterCollider = (collider as CharacterController);

                        // Calculate the new collider dimensions, converting back to local space
                        Vector3 worldHeight = new Vector3(0, bodyHeight, 0);
                        characterCollider.height = gameObject.transform.InverseTransformVector(worldHeight).y;
                        characterCollider.center = gameObject.transform.InverseTransformPoint(
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
