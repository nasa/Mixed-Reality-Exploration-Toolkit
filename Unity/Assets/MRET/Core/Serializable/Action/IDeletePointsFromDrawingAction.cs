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
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IDeletePointsFromDrawingAction
	///
	/// A delete points from drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDeletePointsFromDrawingAction : IDrawingAction
    {
        /// <seealso cref="IDrawingAction.CreateSerializedType"/>
        new public DeletePointsFromDrawingActionType CreateSerializedType();

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IDrawingAction.SerializedAction"/>
        new public DeletePointsFromDrawingActionType SerializedAction { get; }

        /// <seealso cref="IDrawingAction.Deserialize(DrawingActionType, Action{bool, string})"/>
        public void Deserialize(DeletePointsFromDrawingActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IDrawingAction.Serialize(DrawingActionType, Action{bool, string})"/>
        public void Serialize(DeletePointsFromDrawingActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IDeletePointsFromDrawingAction
	///
	/// A <generic> delete points from drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDeletePointsFromDrawingAction<T, I> : IDrawingAction<T, I>, IDeletePointsFromDrawingAction
        where T : DeletePointsFromDrawingActionType
        where I : IInteractableDrawing
    {
    }
}
