// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class AddPointsToDrawingAction :
        DrawingAction<AddPointsToDrawingActionType, IInteractableDrawing>,
        IAddPointsToDrawingAction<AddPointsToDrawingActionType, IInteractableDrawing>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AddPointsToDrawingAction);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private AddPointsToDrawingActionType serializedDrawingAction;

        #region Serialization
        /// <seealso cref="DrawingAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(AddPointsToDrawingActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedDrawingAction = serialized;

            // Process this object specific deserialization

            // Deserialize the new points (required)
            Vector3[] points = null;
            SchemaUtil.DeserializePoints(serializedDrawingAction.Points, ref points);

            // Check for a valid points array
            if (points == null)
            {
                // Error condition
                deserializationState.Error("No valid drawing points");
                return;
            }

            // Update the _points list
            _points = new List<Vector3>(points);

            // Add the points
            foreach (Vector3 point in points)
            {
                ActionObject.AddPoint(point);
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="DrawingAction{T}.Serialize(T, SerializationState)"/>
        protected override void Serialize(AddPointsToDrawingActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the drawing points (required)
            PointsType serializedPoints = new PointsType();
            SchemaUtil.SerializePoints(Points, serializedPoints);
            serialized.Points = serializedPoints;

            // Save the final serialized reference
            serializedDrawingAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
        }

        /// <summary>
        /// Constructor for the <code>AddPointsToDrawingAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="DrawingAction{T,C}.DrawingAction(T)"/>
        public AddPointsToDrawingAction(AddPointsToDrawingActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>DrawingAction</code>
        /// </summary>
        /// <param name="drawing">The <code>IInteractableDrawing</code> associated with this action</param>
        /// <param name="points">The points associated with this action</param>
        /// <seealso cref="DrawingAction{T,C}.DrawingAction(C, ISceneObject, Vector3[])(T)"/>
        public AddPointsToDrawingAction(IInteractableDrawing drawing, Vector3[] points) : base(drawing, null, points)
        {
        }
    }
}