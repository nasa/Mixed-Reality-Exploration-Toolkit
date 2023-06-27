// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class AddDrawingToNoteAction :
        NoteDrawingAction<AddDrawingToNoteActionType, InteractableNote>,
        IAddDrawingToNoteAction<AddDrawingToNoteActionType, InteractableNote>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AddDrawingToNoteAction);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private AddDrawingToNoteActionType serializedAddDrawingToNoteAction;

        #region IAddDrawingToNoteAction
        /// <seealso cref="IAddDrawingToNoteAction.Parent"/>
        InteractableNote IAddDrawingToNoteAction.Parent => Parent as InteractableNote;

        /// <seealso cref="IAddDrawingToNoteAction.SerializedDrawing"/>
        public Drawing2dType SerializedDrawing { get; protected set; }
        #endregion IAddDrawingToNoteAction

        #region Serializable
        /// <seealso cref="NoteDrawingAction{T,C}.DeserializeDrawingID(string, VersionedSerializationState{IInteractable2dDrawing})"/>
        protected override void DeserializeDrawingID(string serializedDrawingID, VersionedSerializationState<IInteractable2dDrawing> actionObjectDeserializationState)
        {
            // Take the inherited behavior to start
            base.DeserializeDrawingID(serializedDrawingID, actionObjectDeserializationState);

            // If the parent failed, there's no point in continuing
            if (actionObjectDeserializationState.IsError) return;

            // Start with an incomplete indicator
            actionObjectDeserializationState.Clear();

            // Determine if the action object ID is defined
            if (!string.IsNullOrEmpty(serializedDrawingID))
            {
                // Make sure a drawing doesn't already exist with this ID.
                // We want a null action object
                if (actionObjectDeserializationState.versioned == null)
                {
                    // Create the new action object
                    if (SerializedDrawing != null)
                    {
                        bool drawingAdded = ActionObject.AddDrawing(SerializedDrawing);
                        if (!drawingAdded)
                        {
                            // Error condition
                            actionObjectDeserializationState.Error("Drawing could not be added");
                        }
                    }
                }
                else
                {
                    // Error condition
                    actionObjectDeserializationState.Error("An object already exists with " +
                        "the specified ID: " + serializedDrawingID);
                }
            }

            // If there's an error, abort
            if (actionObjectDeserializationState.IsError) return;

            // Mark as complete
            actionObjectDeserializationState.complete = true;
        }

        /// <seealso cref="NoteDrawingAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(AddDrawingToNoteActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedAddDrawingToNoteAction = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="NoteDrawingAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(AddDrawingToNoteActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Store the serialized drawing
            serialized.Drawing = SerializedDrawing;

            // Save the final serialized reference
            serializedAddDrawingToNoteAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <summary>
        /// Constructor for the <code>AddDrawingToNoteAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(T)"/>
        public AddDrawingToNoteAction(AddDrawingToNoteActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>AddDrawingToNoteAction</code>
        /// </summary>
        /// <param name="note">The <code>InteractableNote</code> the drawing is being added to</param>
        /// <param name="serializedDrawing">The serialized <code>Drawing2dType</code> that describes
        /// the drawing to add.
        /// NOTE: The UUID and ID for the drawing should be unique across MRET. If an identifiable object exists
        /// with the same UUID, the deserialzation will fail.
        /// </param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(T)"/>
        public AddDrawingToNoteAction(InteractableNote note, Drawing2dType serializedDrawing) : base(note)
        {
            // Assign the unique settings for this action
            SerializedDrawing = serializedDrawing;
        }

        /// <summary>
        /// Constructor for the <code>AddDrawingToNoteAction</code>
        /// </summary>
        /// <param name="noteId">The interactable note ID the drawing is being added to</param>
        /// <param name="serializedDrawing">The serialized <code>Drawing2dType</code> that describes
        /// the drawing to add.
        /// NOTE: The UUID and ID for the drawing should be unique across MRET. If an identifiable object exists
        /// with the same UUID, the deserialzation will fail.
        /// </param>
        /// <seealso cref="NoteDrawingAction{T,C}.NoteDrawingAction(T)"/>
        public AddDrawingToNoteAction(string noteId, Drawing2dType serializedDrawing) : base(noteId)
        {
            // Assign the unique settings for this action
            SerializedDrawing = serializedDrawing;
        }
    }
}