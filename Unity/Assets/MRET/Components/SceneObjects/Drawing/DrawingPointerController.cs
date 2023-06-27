// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
#if HOLOLENS_BUILD
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Hololens;
#endif

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 23 December 2021: Created
    /// </remarks>
    /// <summary>
    /// DrawingPointerController is a controller class that
    /// handles raycast drawing for one hand.
    /// Author: Dylan Z. Baker
    /// </summary>
	public class DrawingPointerController : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(DrawingPointerController);

        /// <summary>
        /// The maximum distance that will be used for drawing (in meters).
        /// </summary>
        [Tooltip("The maximum distance that will be used for drawing (in meters).")]
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
        /// The maximum distance that will be used for drawing (in meters).
        /// </summary>
        [Tooltip("The maximum distance that will be used for drawing (in meters).")]
        public float maxDistance = DEFAULT_MAX_DISTANCE;

        /// <summary>
        /// Raycast laser to use.
        /// </summary>
        public RaycastLaser raycastLaser { get; private set; }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
				
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

        /// <summary>
        /// Toggles the drawing laser on.
        /// </summary>
        public void TogglePointingOn()
        {
            if (raycastLaser == null)
            {
                LogError("Unexpected state: Raycast laser is null.", nameof(TogglePointingOn));
                return;
            }

            raycastLaser.active = true;
        }

        /// <summary>
        /// Toggles the drawing laser off.
        /// </summary>
        public void TogglePointingOff()
        {
            if (raycastLaser == null)
            {
                LogError("Unexpected state: Raycast laser is null.", nameof(TogglePointingOff));
                return;
            }

            raycastLaser.active = false;
        }

        /// <summary>
        /// Enters drawing mode.
        /// </summary>
        public void EnterMode()
        {
            if (raycastLaser != null)
            {
                LogWarning("Attempting duplicate interactor creation.", nameof(EnterMode));
                return;
            }
#if HOLOLENS_BUILD
            FingerTracker finger = hand.GetComponentInChildren<FingerTracker>();
            raycastLaser = finger.gameObject.AddComponent<RaycastLaser>();
#else
            raycastLaser = gameObject.AddComponent<RaycastLaser>();
#endif
            raycastLaser.layerMask = laserMask;
            raycastLaser.pointerType = RaycastLaser.PointerType.straight;
            raycastLaser.width = 0.005f;
            raycastLaser.maxLength = maxDistance;
            raycastLaser.validMaterial = hand.drawingLaserMaterial;
            raycastLaser.invalidMaterial = hand.invalidLaserMaterial;
            raycastLaser.cursorPrefab = hand.drawingCursor;
            raycastLaser.cursorScale = hand.drawingCursorScale;
            raycastLaser.showInvalid = true;

            raycastLaser.active = false;
        }

        /// <summary>
        /// Exits drawing mode.
        /// </summary>
        public void ExitMode()
        {
            Destroy(raycastLaser);
            raycastLaser = null;
        }
    }
}