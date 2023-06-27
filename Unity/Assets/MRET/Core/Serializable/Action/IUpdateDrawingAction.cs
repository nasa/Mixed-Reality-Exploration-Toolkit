// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
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
	/// IUpdateDrawingAction
	///
	/// An update drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUpdateDrawingAction : IDrawingAction
	{
        /// <seealso cref="IDrawingAction.CreateSerializedType"/>
        new public UpdateDrawingActionType CreateSerializedType();

        /// <summary>
        /// The width of the drawing for this action
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IDrawingAction.SerializedAction"/>
        new public UpdateDrawingActionType SerializedAction { get; }

        /// <seealso cref="IDrawingAction.Deserialize(DrawingActionType, Action{bool, string})"/>
        public void Deserialize(UpdateDrawingActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IDrawingAction.Serialize(DrawingActionType, Action{bool, string})"/>
        public void Serialize(UpdateDrawingActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IUpdateDrawingAction
	///
	/// A <generic> update drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUpdateDrawingAction<T, I> : IDrawingAction<T, I>, IUpdateDrawingAction
        where T : UpdateDrawingActionType
        where I : IInteractableDrawing
    {
    }
}
