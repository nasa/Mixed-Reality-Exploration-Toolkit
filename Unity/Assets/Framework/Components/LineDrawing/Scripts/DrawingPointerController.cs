// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing
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
		public override string ClassName
		{
			get
			{
				return nameof(DrawingPointerController);
			}
		}

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
                Debug.LogError("[TeleportController] Teleport state error.");
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
                Debug.LogError("[TeleportController] Teleport state error.");
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
                Debug.LogWarning("Attempting duplicate interactor creation.");
                return;
            }

            raycastLaser = gameObject.AddComponent<RaycastLaser>();
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