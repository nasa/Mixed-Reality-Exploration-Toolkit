// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class DeleteDrawingFromNoteAction :
        NoteDrawingAction<DeleteDrawingFromNoteActionType,InteractableNote>,
        IDeleteDrawingFromNoteAction<DeleteDrawingFromNoteActionType, InteractableNote>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DeleteDrawingFromNoteAction);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private DeleteDrawingFromNoteActionType serializedDeleteDrawingFromNoteAction;

        #region Serializable
        /// <seealso cref="NoteDrawingAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(DeleteDrawingFromNoteActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedDeleteDrawingFromNoteAction = serialized;

            // Process this object specific deserialization
            bool drawingDeleted = ActionObject.DeleteDrawing(serializedDeleteDrawingFromNoteAction.DrawingID);
            if (!drawingDeleted)
            {
                // Error condition
                deserializationState.Error("Drawing could not be deleted");
                return;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="Versioned{T}.Serialize(T, SerializationState)"/>
        protected override void Serialize(DeleteDrawingFromNoteActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedDeleteDrawingFromNoteAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <summary>
        /// Constructor for the <code>DeleteDrawingFromNoteAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(T)"/>
        public DeleteDrawingFromNoteAction(DeleteDrawingFromNoteActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteDrawingFromNoteAction</code>
        /// </summary>
        /// <param name="note">The <code>InteractableNote</code> associated with this action</param>
        /// <param name="drawing">The <code>IInteractable2dDrawing</code> being deleted</param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(C)"/>
        public DeleteDrawingFromNoteAction(InteractableNote note, IInteractable2dDrawing drawing) : base(note, drawing)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteDrawingFromNoteAction</code>
        /// </summary>
        /// <param name="noteId">The interactable note ID associated with this action</param>
        /// <param name="drawingId">The interactable drawing ID being deleted</param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(C)"/>
        public DeleteDrawingFromNoteAction(string noteId, string drawingId) : base(noteId, drawingId)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteDrawingFromNoteAction</code>
        /// </summary>
        /// <param name="note">The <code>InteractableNote</code> containing the drawing to delete.</param>
        /// <param name="serializedDrawing">The serialized <code>Drawing2dType</code> that describes
        /// the drawing to delete.
        /// </param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(C)"/>
        public DeleteDrawingFromNoteAction(InteractableNote note, Drawing2dType serializedDrawing) : base(note)
        {
            // Assign the unique settings for this action
            DrawingID = serializedDrawing.ID;
        }

        /// <summary>
        /// Constructor for the <code>DeleteDrawingFromNoteAction</code>
        /// </summary>
        /// <param name="note">The interactable note ID containing the drawing to delete.</param>
        /// <param name="serializedDrawing">The serialized <code>Drawing2dType</code> that describes
        /// the drawing to delete.
        /// </param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(string)"/>
        public DeleteDrawingFromNoteAction(string noteId, Drawing2dType serializedDrawing) : base(noteId)
        {
            // Assign the unique settings for this action
            DrawingID = serializedDrawing.ID;
        }
    }
}