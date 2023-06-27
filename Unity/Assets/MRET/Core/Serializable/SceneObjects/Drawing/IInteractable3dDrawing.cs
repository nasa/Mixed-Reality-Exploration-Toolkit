// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractable3dDrawing
	///
	/// Represents an interactable 3D drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractable3dDrawing : IInteractableDrawing
	{
        /// <seealso cref="IInteractableDrawing.CreateSerializedType"/>
        new public Drawing3dType CreateSerializedType();

        /// <summary>
        /// Indicates the type of 3D drawing being rendered
        /// </summary>
        public DrawingRender3dType RenderType { get; }

        /// <summary>
        /// Indicates whether or not to display the total measurement of the drawing
        /// </summary>
        public bool DisplayMeasurement { get; set; }

        /// <summary>
        /// Indicates whether or not to display the segment measurements of the drawing
        /// </summary>
        public bool DisplaySegmentMeasurements { get; set; }

        /// <summary>
        /// The units to display if measurement displaying is enabled
        /// </summary>
        public LengthUnitType DesiredUnits { get; set; }

        /// <summary>
        /// The length limit of the drawing
        /// </summary>
        public float LengthLimit { get; set; }

        /// <summary>
        /// Enables the endpoint visuals
        /// </summary>
        public void EnableEndpointVisuals();

        /// <summary>
        /// Disables the endpoint visuals
        /// </summary>
        public void DisableEndpointVisuals();

        /// <summary>
        /// Enables the midpoint visuals
        /// </summary>
        /// <param name="numberOfPoints">Number of points to visualize</param>
        public void EnableMidpointVisuals(int numberOfPoints);

        /// <summary>
        /// Whether or not the editing menu is active.
        /// </summary>
        public bool EditingActive { get; set; }

        /// <summary>
        /// Whether or not editing is allowed.
        /// </summary>
        public bool EditingAllowed { get; set; }

        /// <summary>
        /// Disables the midpoint visuals
        /// </summary>
        public void DisableMidpointVisuals();

        /// <seealso cref="IInteractableDrawing.Deserialize(DrawingType, Action{bool, string})"/>
        public void Deserialize(Drawing3dType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractableDrawing.Serialize(DrawingType, Action{bool, string})"/>
        public void Serialize(Drawing3dType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractable3dDrawing
	///
	/// Represents a <generic> interactable 3D drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractable3dDrawing<T> : IInteractableDrawing<T>, IInteractable3dDrawing
        where T : Drawing3dType
    {
    }
}
