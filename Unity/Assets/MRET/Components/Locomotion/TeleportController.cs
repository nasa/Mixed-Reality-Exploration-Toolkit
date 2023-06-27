// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
#if HOLOLENS_BUILD
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Hololens;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Locomotion
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
            get => teleportBlocked;
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
        /// The type of pointer that is rendered.
        /// </summary>
        [Tooltip("The type of pointer that is rendered.")]
        public RaycastLaser.PointerType pointerType = RaycastLaser.PointerType.straight;

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
            if (teleportBlocked == true)
            {
                return;
            }

            if (raycastLaser == null)
            {
                return;
            }

            // Place the input rig at the hit point.
            if (raycastLaser.isHitting)
            {
                Vector3 headOffset = MRET.InputRig.head.transform.localPosition;

                //and move all objects marked as locked if we are in AR
                //TODO: reenable anchor
#if HOLOLENS_BUILD
                if (MRET.ProjectManager.anchorManager.anchoredObjects.Count >= 0)
                {
                    foreach (GameObject lockedSceneObject in MRET.ProjectManager.anchorManager.anchoredObjects)
                    {
                        if (lockedSceneObject != null)
                        {
                            Transform AnchorTransform = lockedSceneObject.transform;
                            //Change in anchored object position is equal to new position-old position
                            AnchorTransform.position += (raycastLaser.hitPos - new Vector3(headOffset.x, 0, headOffset.z)) - rig.transform.position;
                        }
                    }
                }
#endif
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
            //if hololens build then have the raycast come out of the index finger
#if HOLOLENS_BUILD
            FingerTracker finger = hand.GetComponentInChildren<FingerTracker>();
            raycastLaser = finger.gameObject.AddComponent<RaycastLaser>();
#else
            raycastLaser = gameObject.AddComponent<RaycastLaser>();
#endif
            raycastLaser.layerMask = laserMask;
            raycastLaser.pointerType = pointerType;
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