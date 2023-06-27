// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IDrawingAction
	///
	/// A drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDrawingAction : ISceneObjectAction
    {
        /// <seealso cref="ISceneObjectAction.CreateSerializedType"/>
        new public DrawingActionType CreateSerializedType();

        /// <summary>
        /// The points associated with this drawing action
        /// </summary>
        public Vector3[] Points { get; }

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        new public IInteractableDrawing ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="ISceneObjectAction.SerializedAction"/>
        new public DrawingActionType SerializedAction { get; }

        /// <seealso cref="ISceneObjectAction.Deserialize(SceneObjectActionType, Action{bool, string})"/>
        public void Deserialize(DrawingActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObjectAction.Serialize(SceneObjectActionType, Action{bool, string})"/>
        public void Serialize(DrawingActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IDrawingAction
	///
	/// A drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDrawingAction<T,I> : ISceneObjectAction<T,I>, IDrawingAction
        where T : DrawingActionType
        where I : IInteractableDrawing
	{
    }
}
