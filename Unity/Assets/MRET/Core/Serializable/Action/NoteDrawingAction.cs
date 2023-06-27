// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class NoteDrawingAction<T,C> : NoteAction<T,C>, INoteDrawingAction<T,C>
        where T : NoteDrawingActionType, new()
        where C : InteractableNote
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(NoteDrawingAction<T,C>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedNoteDrawingAction;

        #region INoteDrawingAction
        /// <seealso cref="INoteDrawingAction.CreateSerializedType"/>
        NoteDrawingActionType INoteDrawingAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="INoteDrawingAction.DrawingID"/>
        public virtual string DrawingID
        {
            get
            {
                return (Drawing == null) ? _drawingId : Drawing.id;
            }
            protected set
            {
                // If the Drawing is not defined, allow the change
                _drawingId = (Drawing == null) ? value : Drawing.id;
            }
        }
        private string _drawingId;

        /// <seealso cref="INoteDrawingAction.Drawing"/>
        public IInteractable2dDrawing Drawing { get; protected set; }

        /// <seealso cref="INoteDrawingAction.SerializedAction"/>
        NoteDrawingActionType INoteDrawingAction.SerializedAction => SerializedAction;

        /// <seealso cref="INoteDrawingAction.Deserialize(NoteDrawingActionType, Action{bool, string})"/>
        void INoteDrawingAction.Deserialize(NoteDrawingActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="INoteDrawingAction.Serialize(NoteDrawingActionType, System.Action{bool, string})"/>
        void INoteDrawingAction.Serialize(NoteDrawingActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion INoteDrawingAction

        /// <summary>
        /// The interactable drawing associated with the action.<br>
        /// </summary>

        #region Serializable
        /// <summary>
        /// Coroutine to deserialize the supplied serialized action object ID and updates the
        /// supplied state with the resulting action object reference. This method is available
        /// for subclasses thaat require an object associated with the action.<br>
        /// </summary>
        /// <param name="serializedDrawingID">The serialized action object ID</param>
        /// <param name="actionObjectDeserializationState">The <code>VersionedSerializationState</code> to populate
        ///     with the resultant <code>IInteractable2dDrawing</code> or null if the reference doesn't exist.</param>
        /// 
        /// <see cref="VersionedSerializationState{V}"/>
        /// 
        protected virtual void DeserializeDrawingID(string serializedDrawingID, VersionedSerializationState<IInteractable2dDrawing> actionObjectDeserializationState)
        {
            // Determine if the drawing ID is valid
            if (!string.IsNullOrEmpty(serializedDrawingID))
            {
                // Set the drawing ID
                DrawingID = serializedDrawingID;

                // Look up the drawing by ID
                IIdentifiable drawing = MRET.UuidRegistry.GetByID(serializedDrawingID);
                if (drawing is IInteractable2dDrawing)
                {
                    // Set the drawing
                    actionObjectDeserializationState.versioned = drawing as IInteractable2dDrawing;
                }
                else
                {
                    // Error condition
                    actionObjectDeserializationState.Error("Specified drawing ID reference is invalid. " +
                        "Must be of type: " + nameof(IInteractable2dDrawing));

                    // Clear the action object reference
                    actionObjectDeserializationState.versioned = null;
                }
            }
            else
            {
                // Error condition
                actionObjectDeserializationState.Error("Specified drawing ID is not defined");

                // Clear the action object reference
                actionObjectDeserializationState.versioned = null;
            }

            // If there's an error, abort
            if (actionObjectDeserializationState.IsError) return;

            // Mark as complete
            actionObjectDeserializationState.complete = true;
        }

        /// <seealso cref="NoteAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedNoteDrawingAction = serialized;

            // Process this object specific deserialization

            // Deserialize the action object
            VersionedSerializationState<IInteractable2dDrawing> actionObjectDeserializationState = new VersionedSerializationState<IInteractable2dDrawing>();
            DeserializeDrawingID(serialized.DrawingID, actionObjectDeserializationState);

            // Record the deserialization state
            deserializationState.Update(actionObjectDeserializationState);

            // If the action object ID loading failed, exit with an error
            if (deserializationState.IsError) return;

            // Assign the action object reference
            if (actionObjectDeserializationState.versioned is null)
            {
                // Error condition
                actionObjectDeserializationState.Error("Specified drawing is invalid. " +
                    "Must be of type: " + nameof(IInteractable2dDrawing));
                return;
            }
            Drawing = actionObjectDeserializationState.versioned;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="NoteAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the drawing ID
            if (string.IsNullOrEmpty(DrawingID))
            {
                // Error condition
                serializationState.Error("Drawing ID is not defined");
                return;
            }
            serialized.DrawingID = DrawingID;

            // Save the final serialized reference
            serializedNoteDrawingAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            DrawingID = "";
            Drawing = null;
        }

        /// <summary>
        /// Constructor for the <code>NoteDrawingAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(T)"/>
        public NoteDrawingAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteDrawingAction</code>
        /// </summary>
        /// <param name="note">The <code>InteractableNote</code> associated with this action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(C,ISceneObject)"/>
        public NoteDrawingAction(C note) : base(note)
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteDrawingAction</code>
        /// </summary>
        /// <param name="noteId">The interactable note ID associated with this action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(string,string)"/>
        public NoteDrawingAction(string noteId) : base(noteId)
        {
        }

        /// <summary>
        /// Constructor for the <code>NoteDrawingAction</code>
        /// </summary>
        /// <param name="note">The <code>InteractableNote</code> associated with this action</param>
        /// <param name="drawing">The <code>IInteractable2dDrawing</code> associated with this action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(C)"/>
        public NoteDrawingAction(C note, IInteractable2dDrawing drawing) : base(note)
        {
            // Assign the unique settings for this action
            Drawing = drawing;
        }

        /// <summary>
        /// Constructor for the <code>NoteAction</code>
        /// </summary>
        /// <param name="noteId">The interactable note ID associated with this action</param>
        /// <param name="drawingId">The drawing ID associated with this action</param>
        /// <seealso cref="NoteAction{T,C}.NoteAction(string,string)"/>
        public NoteDrawingAction(string noteId, string drawingId) : base(noteId)
        {
            // Assign the unique settings for this action
            DrawingID = drawingId;
        }

    }
}