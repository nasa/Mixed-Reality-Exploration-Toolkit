// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IDeleteDrawingFromNoteAction
	///
	/// A delete drawing from note action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDeleteDrawingFromNoteAction : INoteDrawingAction
    {
        /// <seealso cref="INoteDrawingAction.CreateSerializedType"/>
        new public DeleteDrawingFromNoteActionType CreateSerializedType();

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="INoteDrawingAction.SerializedAction"/>
        new public DeleteDrawingFromNoteActionType SerializedAction { get; }

        /// <seealso cref="INoteDrawingAction.Deserialize(NoteDrawingActionType, Action{bool, string})"/>
        public void Deserialize(DeleteDrawingFromNoteActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="INoteDrawingAction.Serialize(NoteDrawingActionType, Action{bool, string})"/>
        public void Serialize(DeleteDrawingFromNoteActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IDeleteDrawingFromNoteAction
	///
	/// A <generic> delete drawing from note action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDeleteDrawingFromNoteAction<T, I> : INoteDrawingAction<T, I>, IDeleteDrawingFromNoteAction
        where T : DeleteDrawingFromNoteActionType
        where I : InteractableNote
    {
    }
}
