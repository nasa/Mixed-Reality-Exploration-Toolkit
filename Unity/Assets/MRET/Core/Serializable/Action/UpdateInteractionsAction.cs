// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class UpdateInteractionsAction :
        InteractableSceneObjectAction<UpdateInteractionsActionType, IInteractable>,
        IUpdateInteractionsAction<UpdateInteractionsActionType, IInteractable>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UpdateInteractionsAction);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UpdateInteractionsActionType serializedInteractionsAction;

        #region IUpdateInteractionsAction
        /// <seealso cref="IUpdateInteractionsAction.EnableInteraction"/>
        public bool EnableInteraction { get; protected set; }

        /// <seealso cref="IUpdateInteractionsAction.EnableUsability"/>
        public bool EnableUsability { get; protected set; }
        #endregion IUpdateInteractionsAction

        #region Serialization
        /// <summary>
        /// Deserializes the supplied serialized interaction settings and updates the supplied state
        /// with the deserialization state.
        /// </summary>
        /// <param name="serializedInteractions">The serialized <code>InteractionsSettingsType</code> interaction settings</param>
        /// <param name="interactionsDeserializationState">The <code>SerializationState</code> to populate with the deserialization state.</param>
        /// 
        /// <see cref="InteractionSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual void DeserializeInteractions(InteractionSettingsType serializedInteractions, SerializationState interactionsDeserializationState)
        {
            // Mark as incomplete
            interactionsDeserializationState.complete = false;

            // Interactions settings are optional
            if (serializedInteractions is null)
            {
                interactionsDeserializationState.complete = true;
                return;
            }

            // Deserialize interaction
            ActionObject.Grabbable = serializedInteractions.EnableInteraction;

            // Deserialize usability
            ActionObject.Usable = serializedInteractions.EnableUsability;

            // Mark as complete
            interactionsDeserializationState.complete = true;
        }

        /// <seealso cref="InteractableSceneObjectAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(UpdateInteractionsActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedInteractionsAction = serialized;

            // Process this object specific deserialization

            // Deserialize the interactions settings
            SerializationState interactionsDeserializationState = new SerializationState();
            DeserializeInteractions(serialized.Interactions, interactionsDeserializationState);

            // Record the deserialization state
            deserializationState.Update(interactionsDeserializationState);

            // If the physics deserialization failed, exit with an error
            if (deserializationState.IsError) return;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <summary>
        /// Asynchronously serializes the interaction settings into the supplied serialized interaction settings
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedInteractions">The serialized <code>InteractionsSettingsType</code> to populate with the interaction settings</param>
        /// <param name="interactionsSerializationState">The <code>SerializationState</code> to populate with the serialization state.</param>
        /// 
        /// <see cref="InteractionSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual void SerializeInteractions(InteractionSettingsType serializedInteractions, SerializationState interactionsSerializationState)
        {
            // Mark as incomplete
            interactionsSerializationState.complete = false;

            // Interactions settings are optional
            if (serializedInteractions is null)
            {
                interactionsSerializationState.complete = true;
                return;
            }

            // Serialize interactable
            serializedInteractions.EnableInteraction = EnableInteraction;

            // Serialize usability
            serializedInteractions.EnableUsability = EnableUsability;

            // Mark as complete
            interactionsSerializationState.complete = true;
        }

        /// <seealso cref="InteractableSceneObjectAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(UpdateInteractionsActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the interaction settings
            InteractionSettingsType serializedInteractions = serialized.Interactions;
            if ((serializedInteractions == null) &&
                (serializedInteractionsAction != null))
            {
                // Use this interactions structure
                serializedInteractions = serializedInteractionsAction.Interactions;
            }
            SerializationState interactionsSerializationState = new SerializationState();
            SerializeInteractions(serializedInteractions, interactionsSerializationState);

            // Record the serialization state
            serializationState.Update(interactionsSerializationState);

            // If the interations serialization failed, exit with an error
            if (serializationState.IsError) return;

            // Serialize out the interaction settings
            serialized.Interactions = serializedInteractions;

            // Save the final serialized reference
            serializedInteractionsAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            EnableInteraction = InteractableDefaults.INTERACTABLE;
            EnableUsability = InteractableDefaults.USABLE;
        }

        /// <summary>
        /// Constructor for the <code>InteractionsAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="PhysicalSceneObjectAction{T,C}.PhysicalSceneObjectAction(T)"/>
        public UpdateInteractionsAction(UpdateInteractionsActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractionsAction</code>
        /// </summary>
        /// <param name="interactableSceneObject">The <code>IInteractable</code> associated with this action</param>
        /// <param name="enableInteraction">The interaction enabled associated with this action</param>
        /// <param name="enableUsability">The usability enabled associated with this action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,C}.InteractableSceneObjectAction(C)"/>
        public UpdateInteractionsAction(IInteractable interactableSceneObject,
            bool enableInteraction, bool enableUsability) : base(interactableSceneObject)
        {
            // Assign the unique settings for this action
            EnableInteraction = enableInteraction;
            EnableUsability = enableUsability;
        }
    }
}