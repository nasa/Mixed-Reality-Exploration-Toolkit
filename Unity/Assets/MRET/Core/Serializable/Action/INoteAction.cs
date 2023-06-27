// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// INoteAction
	///
	/// A note action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface INoteAction : IInteractableSceneObjectAction
    {
        /// <seealso cref="IInteractableSceneObjectAction.CreateSerializedType"/>
        new public NoteActionType CreateSerializedType();

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        new public InteractableNote ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IInteractableSceneObjectAction.SerializedAction"/>
        new public NoteActionType SerializedAction { get; }

        /// <seealso cref="IInteractableSceneObjectAction.Deserialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        public void Deserialize(NoteActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractableSceneObjectAction.Serialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        public void Serialize(NoteActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// INoteAction
	///
	/// A note action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface INoteAction<T,C> : IInteractableSceneObjectAction<T,C>, INoteAction
        where T : NoteActionType
        where C : InteractableNote
    {
    }
}
