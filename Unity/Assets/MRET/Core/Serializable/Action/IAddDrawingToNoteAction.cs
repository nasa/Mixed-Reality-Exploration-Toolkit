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
	/// IAddDrawingToNoteAction
	///
	/// An add drawing to note action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAddDrawingToNoteAction : INoteDrawingAction
	{
        /// <seealso cref="INoteDrawingAction.CreateSerializedType"/>
        new public AddDrawingToNoteActionType CreateSerializedType();

        /// <summary>
        /// The note associated with the action.<br>
        /// </summary>
        new public InteractableNote Parent { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="INoteDrawingAction.SerializedAction"/>
        new public AddDrawingToNoteActionType SerializedAction { get; }

        /// <summary>
        /// The serialized drawing to instantiate
        /// </summary>
        public Drawing2dType SerializedDrawing { get; }

        /// <seealso cref="INoteDrawingAction.Deserialize(NoteDrawingActionType, Action{bool, string})"/>
        public void Deserialize(AddDrawingToNoteActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="INoteDrawingAction.Serialize(NoteDrawingActionType, Action{bool, string})"/>
        public void Serialize(AddDrawingToNoteActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IAddDrawingToNoteAction
	///
	/// A <generic> add drawing to note action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAddDrawingToNoteAction<T, I> : INoteDrawingAction<T, I>, IAddDrawingToNoteAction
        where T : AddDrawingToNoteActionType
        where I : InteractableNote
    {
    }
}
