// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractableDrawing
	///
	/// Represents an interactable drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractableDrawing : IInteractable
	{
        /// <seealso cref="IInteractable.CreateSerializedType"/>
        new public DrawingType CreateSerializedType();

        /// <summary>
        /// The width of the drawing
        /// </summary>
        public float width { get; set; }

        /// <summary>
        /// The length of the drawing
        /// </summary>
        public float length { get; }

        /// <summary>
        /// The center point of the drawing
        /// </summary>
        public Vector3 center { get; }

        /// <summary>
        /// Points in the drawing.
        /// </summary>
        public Vector3[] points { get; set; }

        /// <summary>
        /// The drawing <code>Material</code>
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// The drawing <code>Gradient</code>
        /// </summary>
        public Gradient Gradient { get; set; }

        /// <summary>
        /// The drawing <code>Color32</code>
        /// </summary>
        public Color32 Color { get; set; }

        /// <summary>
        /// Clears the drawing
        /// </summary>
        public void ClearDrawing();

        /// <summary>
        /// Adds a point to the end of the drawing
        /// </summary>
        /// <param name="point">The <code>Vector3</code> to add to the drawing</param>
        public void AddPoint(Vector3 point);

        /// <summary>
        /// Inserts a point into the drawing
        /// </summary>
        /// <param name="index">Index to insert the point</param>
        /// <param name="point">The <code>Vector3</code> to insert into to the drawing</param>
        public void InsertPoint(int index, Vector3 point);

        /// <summary>
        /// Inserts points into the drawing
        /// </summary>
        /// <param name="index">Starting index to insert the points</param>
        /// <param name="points">Array of <code>Vector3</code> points to insert</param>
        public void InsertPoints(int index, Vector3[] points);

        /// <summary>
        /// Removes a point from the drawing
        /// </summary>
        /// <param name="point">The <code>Vector3</code> to remove from the drawing</param>
        /// <returns>A boolean indicating successful removal</returns>
        public bool RemovePoint(Vector3 point);

        /// <summary>
        /// Removes a point at the supplied index from the drawing
        /// </summary>
        /// <param name="index">The point index to remove from the drawing</param>
        /// <returns>A boolean indicating successful removal</returns>
        public bool RemovePoint(int index);

        /// <summary>
        /// Replace a point in the drawing
        /// </summary>
        /// <param name="index">Index of the point to replace</param>
        /// <param name="value">The <code>Vector3</code> representing the new point</param>
        public void ReplacePoint(int index, Vector3 value);

        /// <summary>
        /// Append a drawing to this drawing
        /// </summary>
        /// <param name="drawing">The <code>IInteractableDrawing</code> to append.</param>
        /// <param name="appendBackwards">Whether or not to append points in reverse order.</param>
        /// <param name="appendToFront">Whether or not to append points to front</param>
        public void AppendDrawing(IInteractableDrawing drawing, bool appendBackwards, bool appendToFront);

        /// <seealso cref="IInteractable.Deserialize(InteractableSceneObjectType, Action{bool, string})"/>
        public void Deserialize(DrawingType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractable.Serialize(InteractableSceneObjectType, Action{bool, string})"/>
        public void Serialize(DrawingType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractableDrawing
	///
	/// Represents a <generic> interactable drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractableDrawing<T> : IInteractable<T>, IInteractableDrawing
        where T : DrawingType
    {
    }
}
