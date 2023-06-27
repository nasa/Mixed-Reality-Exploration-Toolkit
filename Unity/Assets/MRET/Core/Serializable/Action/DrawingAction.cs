// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class DrawingAction<T,I> : SceneObjectAction<T,I>, IDrawingAction<T,I>
        where T : DrawingActionType, new()
        where I : IInteractableDrawing
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DrawingAction<T,I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedDrawingAction;

        protected List<Vector3> _points = new List<Vector3>();

        #region IDrawingAction
        /// <seealso cref="IDrawingAction.CreateSerializedType"/>
        DrawingActionType IDrawingAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IDrawingAction.Points"/>
        public Vector3[] Points => _points.ToArray();

        /// <seealso cref="IDrawingAction.ActionObject"/>
        IInteractableDrawing IDrawingAction.ActionObject => ActionObject;

        /// <seealso cref="IDrawingAction.SerializedAction"/>
        DrawingActionType IDrawingAction.SerializedAction => SerializedAction;

        /// <seealso cref="IDrawingAction.Deserialize(DrawingActionType, Action{bool, string})"/>
        void IDrawingAction.Deserialize(DrawingActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IDrawingAction.Serialize(DrawingActionType, Action{bool, string})"/>
        void IDrawingAction.Serialize(DrawingActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IDrawingAction

        #region Serializable
        /// <seealso cref="SceneObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedDrawingAction = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="SceneObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedDrawingAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
        }

        /// <summary>
        /// Constructor for the <code>DrawingAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(T)"/>
        public DrawingAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>DrawingAction</code>
        /// </summary>
        /// <param name="drawing">The <code>IInteractableDrawing</code> associated with this action</param>
        /// <param name="drawingParent">The optional <code>ISceneObject</code> parent associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public DrawingAction(I drawing, ISceneObject drawingParent = null) : base(drawing, drawingParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>DrawingAction</code>
        /// </summary>
        /// <param name="drawingId">The drawing ID associated with this action</param>
        /// <param name="drawingParentId">The optional parent ID of the drawing</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(string,string)"/>
        public DrawingAction(string drawingId, string drawingParentId = null) : base(drawingId, drawingParentId)
        {
        }

        /// <summary>
        /// Constructor for the <code>DrawingAction</code>
        /// </summary>
        /// <param name="drawing">The <code>IInteractableDrawing</code> associated with this action</param>
        /// <param name="drawingParent">The <code>ISceneObject</code> parent of the drawing</param>
        /// <param name="points">The points associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public DrawingAction(I drawing, ISceneObject drawingParent, Vector3[] points) : base(drawing, drawingParent)
        {
            // Assign the unique settings for this action
            _points = new List<Vector3>(points);
        }

        /// <summary>
        /// Constructor for the <code>DrawingAction</code>
        /// </summary>
        /// <param name="drawingId">The <code>IInteractableDrawing</code> associated with this action</param>
        /// <param name="drawingParentId">The <code>ISceneObject</code> parent associated with this action</param>
        /// <param name="points">The points associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public DrawingAction(string drawingId, string drawingParentId, Vector3[] points) : base(drawingId, drawingParentId)
        {
            // Assign the unique settings for this action
            _points = new List<Vector3>(points);
        }
    }
}