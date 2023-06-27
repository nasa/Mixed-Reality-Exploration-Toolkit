// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class NoteAction<T,C> : InteractableSceneObjectAction<T,C>, INoteAction<T,C>
        where T : NoteActionType, new()
        where C : InteractableNote
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(NoteAction<T, C>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedNoteAction;

        #region INoteAction
        /// <seealso cref="INoteAction.CreateSerializedType"/>
        NoteActionType INoteAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="INoteAction.ActionObject"/>
        InteractableNote INoteAction.ActionObject => ActionObject;

        /// <seealso cref="INoteAction.SerializedAction"/>
        NoteActionType INoteAction.SerializedAction => SerializedAction;

        /// <seealso cref="INoteAction.Deserialize(NoteActionType, Action{bool, string})"/>
        void INoteAction.Deserialize(NoteActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="INoteAction.Serialize(NoteActionType, Action{bool, string})"/>
        void INoteAction.Serialize(NoteActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion INoteAction

        #region Serializable
        /// <seealso cref="InteractableSceneObjectAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedNoteAction = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="InteractableSceneObjectAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedNoteAction = serialized;

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
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <seealso cref="InteractableSceneObjectAction{T,C}.InteractableSceneObjectAction()"/>
        public NoteAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,C}.InteractableSceneObjectAction(T)"/>
        public NoteAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <param name="note">The <code>InteractableNote</code> associated with this action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,C}.InteractableSceneObjectAction(C,ISceneObject)"/>
        public NoteAction(C note, ISceneObject noteParent = null) : base(note, noteParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <param name="noteId">The interactable note ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,C}.InteractableSceneObjectAction(string,string)"/>
        public NoteAction(string noteId, string parentId = null) : base(noteId, parentId)
        {
        }
    }

    /// <summary>
    /// Provides an implementation for the generic NoteAction class
    /// </summary>
    public class NoteAction : NoteAction<NoteActionType, InteractableNote>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(NoteAction);

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <seealso cref="NoteAction{T,C}.NoteAction()"/>
        public NoteAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(T)"/>
        public NoteAction(NoteActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <param name="note">The <code>InteractableNote</code> associated with this action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(C,ISceneObject)"/>
        public NoteAction(InteractableNote note, ISceneObject noteParent = null) : base(note, noteParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <param name="noteId">The interactable note ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(string,string)"/>
        public NoteAction(string noteId, string parentId = null) : base(noteId, parentId)
        {
        }
    }
}