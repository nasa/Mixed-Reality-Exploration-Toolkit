// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class UpdateDrawingAction :
        DrawingAction<UpdateDrawingActionType, IInteractableDrawing>,
        IUpdateDrawingAction<UpdateDrawingActionType, IInteractableDrawing>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UpdateDrawingAction);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UpdateDrawingActionType serializedDrawingAction;

        #region IUpdateDrawingAction
        /// <seealso cref="IUpdateDrawingAction.Width"/>
        public float Width { get; protected set; }
        #endregion IUpdateDrawingAction

        #region Serialization
        /// <summary>
        /// Asynchronously Deserializes the drawing settings from the supplied serialized action and updates
        /// the supplied state with the deserialization state.
        /// </summary>
        /// <param name="serializedAction">The serialized <code>UpdateDrawingActionType</code> action</param>
        /// <param name="settingsDeserializationState">The <code>SerializationState</code> to populate with the deserialization state.</param>
        /// 
        /// <see cref="UpdateDrawingActionType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual void DeserializeDrawingSettings(UpdateDrawingActionType serializedAction, SerializationState settingsDeserializationState)
        {
            // Mark as incomplete
            settingsDeserializationState.complete = false;

            // Drawing action must be valid
            if (serializedAction is null)
            {
                settingsDeserializationState.Error("Supplied serialized drawing action is null");
                return;
            }

            // Deserialize the width (optional)
            if (serializedAction.Width != null)
            {
                float width = float.NaN;
                SchemaUtil.DeserializeLength(serializedAction.Width, ref width);
                if (float.IsNaN(width))
                {
                    // Error condition
                    settingsDeserializationState.Error("Width deserialization failed");
                    return;
                }
                ActionObject.width = Width;
            }

            // Mark as complete
            settingsDeserializationState.complete = true;
        }

        /// <seealso cref="DrawingAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(UpdateDrawingActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedDrawingAction = serialized;

            // Process this object specific deserialization

            // Deserialize the drawing settings
            SerializationState settingsDeserializationState = new SerializationState();
            DeserializeDrawingSettings(serialized, settingsDeserializationState);

            // Record the deserialization state
            deserializationState.Update(settingsDeserializationState);

            // If the drawing deserialization failed, exit with an error
            if (deserializationState.IsError) return;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <summary>
        /// Asynchronously serializes the drawing settings into the supplied serialized action
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedAction">The serialized <code>UpdateDrawingActionType</code> to populate with the drawing settings</param>
        /// <param name="settingsSerializationState">The <code>SerializationState</code> to populate with the serialization state.</param>
        /// 
        /// <see cref="UpdateDrawingActionType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual void SerializeDrawingSettings(UpdateDrawingActionType serializedAction, SerializationState settingsSerializationState)
        {
            // Drawing action must be valid
            if (serializedAction is null)
            {
                // Report the error
                settingsSerializationState.Error("Supplied serialized drawing action is null");
                return;
            }

            // Serialize the width (optional)
            if (serializedAction.Width != null)
            {
                SchemaUtil.SerializeLength(Width, serializedAction.Width);
            }

            // Mark as complete
            settingsSerializationState.complete = true;
        }

        /// <seealso cref="DrawingAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(UpdateDrawingActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the drawing settings
            SerializationState settingsSerializationState = new SerializationState();
            SerializeDrawingSettings(serialized, settingsSerializationState);

            // Record the serialization state
            serializationState.Update(settingsSerializationState);

            // If the drawing serialization failed, exit with an error
            if (serializationState.IsError) return;

            // Save the final serialized reference
            serializedDrawingAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            Width = InteractableDrawingDefaults.WIDTH;
        }

        /// <summary>
        /// Constructor for the <code>UpdateDrawingAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="DrawingAction{T,C}.DrawingAction(T)"/>
        public UpdateDrawingAction(UpdateDrawingActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>UpdateDrawingAction</code>
        /// </summary>
        /// <param name="drawing">The <code>IInteractableDrawing</code> associated with this action</param>
        /// <param name="width">The width of the drawing</param>
        /// <seealso cref="DrawingAction{T,C}.DrawingAction(T)"/>
        public UpdateDrawingAction(IInteractableDrawing drawing, float width) : base(drawing)
        {
            // Assign the unique settings for this action
            Width = width;
        }

    }
}