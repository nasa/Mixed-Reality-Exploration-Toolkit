// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 4 Nov 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// INoteDrawingAction
	///
	/// A note drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface INoteDrawingAction : INoteAction
    {
        /// <seealso cref="INoteAction.CreateSerializedType"/>
        new public NoteDrawingActionType CreateSerializedType();

        /// <summary>
        /// The drawing identifier associated with this note action.<br>
        /// </summary>
        public string DrawingID { get; }

        /// <summary>
        /// The drawing associated with the note action.<br>
        /// </summary>
        public IInteractable2dDrawing Drawing { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="INoteAction.SerializedAction"/>
        new public NoteDrawingActionType SerializedAction { get; }

        /// <seealso cref="INoteAction.Deserialize(NoteActionType, Action{bool, string})"/>
        public void Deserialize(NoteDrawingActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="INoteAction.Serialize(NoteActionType, Action{bool, string})"/>
        public void Serialize(NoteDrawingActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 4 Nov 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// INoteDrawingAction
	///
	/// A note drawing action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface INoteDrawingAction<T,C> : INoteAction<T,C>, INoteDrawingAction
        where T : NoteDrawingActionType
        where C : InteractableNote
    {
    }
}
