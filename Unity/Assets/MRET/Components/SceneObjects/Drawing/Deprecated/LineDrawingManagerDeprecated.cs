// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// LineDrawingManager is a class that manages line drawings.
    /// Author: Dylan Z. Baker
    /// </summary>
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.LineDrawingManager))]
    public class LineDrawingManagerDeprecated : MRETBehaviour
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
        public List<LineDrawingControllerDeprecated> lineDrawingControllers { get; private set; }

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(LineDrawingManagerDeprecated);

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
            lineDrawingControllers = new List<LineDrawingControllerDeprecated>();
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                LineDrawingControllerDeprecated ldc = hand.GetComponent<LineDrawingControllerDeprecated>();
                if (ldc != null)
                {
                    lineDrawingControllers.Add(ldc);
                }
            }

            Debug.Log("Line Drawing Manager Initialized.");
        }
	}
}