// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Components.LineDrawing;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// LineDrawingManager is a class that manages line drawings.
    /// Author: Dylan Z. Baker
    /// </summary>
	public class LineDrawingManager : MRETBehaviour
	{
        /// <summary>
        /// Data manager key for drawing mode.
        /// </summary>
        public static readonly string ISDRAWINGFLAGKEY = "MRET.INTERNAL.DRAWING.ACTIVE";

        /// <summary>
        /// Type of drawing.
        /// </summary>
        public enum DrawingType { Basic, Volumetric };

        /// <summary>
        /// Measurement units.
        /// </summary>
        public enum Units
        {
            meters, centimeters, millimeters,
            micrometers, nanometers, kilometers,
            inches, feet, yards, miles
        }

        /// <summary>
        /// Prefab for the drawing edit menu.
        /// </summary>
        [Tooltip("Prefab for the drawing edit menu.")]
        public GameObject drawingEditPrefab;

        /// <summary>
        /// Threshold for when a drawing should show its edit menu.
        /// </summary>
        [Tooltip("Threshold for when a drawing should show its edit menu.")]
        public float drawingTouchThreshold = 1f; // TODO DZB: Should probably go somewhere else.

        /// <summary>
        /// The line drawing controllers.
        /// </summary>
        public List<LineDrawingController> lineDrawingControllers { get; private set; }

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
		{
			get
			{
				return nameof(LineDrawingManager);
			}
		}

        /// <summary>
        /// Prefab for the drawing end visual.
        /// </summary>
        [Tooltip("Prefab for the drawing end visual.")]
        public GameObject drawingEndVisualPrefab;

        /// <summary>
        /// Prefab for the drawing point visual.
        /// </summary>
        [Tooltip("Prefab for the drawing point visual.")]
        public GameObject drawingPointVisualPrefab;

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

        public void Initialize()
        {
            lineDrawingControllers = new List<LineDrawingController>();
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                LineDrawingController ldc = hand.GetComponent<LineDrawingController>();
                if (ldc != null)
                {
                    lineDrawingControllers.Add(ldc);
                }
            }

            Debug.Log("Line Drawing Manager Initialized.");
        }
	}
}