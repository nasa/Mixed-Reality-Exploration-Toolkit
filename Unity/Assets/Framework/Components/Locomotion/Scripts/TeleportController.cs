// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Components.Locomotion
{
    /// <remarks>
    /// History:
    /// 8 December 2020: Created
    /// </remarks>
    /// <summary>
    /// TeleportController is a controller class that
    /// handles teleport for one hand. It should be included
    /// with the InputHand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class TeleportController : MonoBehaviour
    {
        public const float DEFAULT_MAX_DISTANCE = 100f;

        /// <summary>
        /// Hand to attach laser to.
        /// </summary>
        [Tooltip("Hand to attach laser to.")]
        public InputHand hand;

        /// <summary>
        /// Layer mask to use for the laser.
        /// </summary>
        [Tooltip("Layer mask to use for the laser.")]
        public LayerMask laserMask;

        /// <summary>
        /// The maximum distance that will be used for teleporting (in meters).
        /// </summary>
        [Tooltip("The maximum distance that will be used for teleporting (in meters).")]
        public float maxDistance = DEFAULT_MAX_DISTANCE;

        /// <summary>
        /// Whether or not to block teleportation.
        /// </summary>
        public bool TeleportBlocked
        {
            get
            {
                return teleportBlocked;
            }
            set
            {
                teleportBlocked = value;
                if (value == true && raycastLaser != null)
                {
                    raycastLaser.active = false;
                }
            }
        }
        private bool teleportBlocked;

        /// <summary>
        /// Raycast laser to use.
        /// </summary>
        public RaycastLaser raycastLaser { get; private set; }

        /// <summary>
        /// Toggles the teleport laser on.
        /// </summary>
        public void ToggleTeleportOn()
        {
            if (raycastLaser == null)
            {
                Debug.LogError("[TeleportController] Teleport state error.");
                return;
            }

            if (teleportBlocked == true)
            {
                return;
            }

            raycastLaser.active = true;
        }

        /// <summary>
        /// Toggles the teleport laser off.
        /// </summary>
        public void ToggleTeleportOff()
        {
            if (raycastLaser == null)
            {
                Debug.LogError("[TeleportController] Teleport state error.");
                return;
            }

            if (teleportBlocked == true)
            {
                return;
            }

            raycastLaser.active = false;
        }

        /// <summary>
        /// Completes the teleport operation.
        /// </summary>
        /// <param name="rig">Rig to teleport to selected location.</param>
        public void CompleteTeleport(GameObject rig)
        {
            if (raycastLaser == null)
            {
                return;
            }

            // Place the input rig at the hit point.
            if (raycastLaser.isHitting)
            {
                Vector3 headOffset = Framework.MRET.InputRig.head.transform.localPosition;

                rig.transform.position = raycastLaser.hitPos - new Vector3(headOffset.x, 0, headOffset.z);
            }
        }

        /// <summary>
        /// Enters teleport mode.
        /// </summary>
        public void EnterMode()
        {
            if (raycastLaser != null)
            {
                Debug.LogWarning("Attempting duplicate interactor creation.");
                return;
            }

            raycastLaser = gameObject.AddComponent<RaycastLaser>();
            raycastLaser.layerMask = laserMask;
            raycastLaser.pointerType = RaycastLaser.PointerType.straight;
            raycastLaser.width = 0.01f;
            raycastLaser.maxLength = maxDistance;
            raycastLaser.validMaterial = hand.teleportationLaserMaterial;
            raycastLaser.invalidMaterial = hand.invalidLaserMaterial;
            raycastLaser.cursorPrefab = hand.teleportationCursor;
            raycastLaser.cursorScale = hand.teleportationCursorScale;
            raycastLaser.showInvalid = true;

            raycastLaser.active = false;
        }

        /// <summary>
        /// Exits teleport mode.
        /// </summary>
        public void ExitMode()
        {
            Destroy(raycastLaser);
            raycastLaser = null;
        }
    }
}