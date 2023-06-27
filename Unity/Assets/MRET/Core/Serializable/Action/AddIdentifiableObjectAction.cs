// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// Generic AddIdentifiableObjectAction
	///
	/// An add identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public class AddIdentifiableObjectAction<C, I> :
        IdentifiableObjectAction<AddIdentifiableObjectActionType,I>,
        IAddIdentifiableObjectAction<AddIdentifiableObjectActionType, C, I>
        where C : IdentifiableType, new()
        where I : IIdentifiable
    {
        /// <seealso cref="Versioned{T}.ClassName"/>
        public override string ClassName => nameof(AddIdentifiableObjectAction<C, I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private AddIdentifiableObjectActionType serializedAddObjectAction;

        #region IAddIdentifiableObjectAction
        /// <seealso cref="IAddIdentifiableObjectAction.CreateSerializedType"/>
        AddIdentifiableObjectActionType IAddIdentifiableObjectAction.CreateSerializedType() => CreateSerializedType();

        /// <summary>
        /// The serialized identifiable object to instantiate
        /// </summary>
        public C SerializedActionObject { get; protected set; }

        /// <seealso cref="IAddIdentifiableObjectAction.SerializedActionObject"/>
        IdentifiableType IAddIdentifiableObjectAction.SerializedActionObject => SerializedActionObject;
        #endregion IAddIdentifiableObjectAction

        #region Serialization
        /// <seealso cref="IdentifiableObjectAction{T,I}.DeserializeActionObjectID(string, VersionedSerializationState{I})"/>
        protected override void DeserializeActionObjectID(string serializedActionObjectID, VersionedSerializationState<I> actionObjectDeserializationState)
        {
            // Take the inherited behavior to start
            base.DeserializeActionObjectID(serializedActionObjectID, actionObjectDeserializationState);

            // If the parent failed, there's no point in continuing
            if (actionObjectDeserializationState.IsError) return;

            // Start with an incomplete indicator
            actionObjectDeserializationState.Clear();

            // Determine if the action object ID is defined
            if (!string.IsNullOrEmpty(serializedActionObjectID))
            {
                // Make sure a action object doesn't already exist with this ID.
                // We want a null action object
                if (actionObjectDeserializationState.versioned is null)
                {
                    // Valid condition so reset the deserialization state
                    actionObjectDeserializationState.Clear();

                    Action<I> ActionObjectSerializedAction = (I loadedObj) =>
                    {
                        ActionObject = loadedObj;
                        if (loadedObj == null)
                        {
                            // Error condition
                            actionObjectDeserializationState.Error("A problem occurred " +
                                "attempting to deserialize the action object: " + serializedActionObjectID);
                        }
                    };

                    // Create the new action object
                    if (SerializedActionObject != null)
                    {
                        MRET.ProjectManager.InstantiateObject(
                            SerializedActionObject, null, null, ActionObjectSerializedAction);
                    }
                }
                else
                {
                    // Error condition
                    actionObjectDeserializationState.Error("An object already exists with " +
                        "the specified ID: " + serializedActionObjectID);
                }
            }

            // If there's an error, abort
            if (actionObjectDeserializationState.IsError) return;

            // Mark as complete
            actionObjectDeserializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(AddIdentifiableObjectActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedAddObjectAction = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(AddIdentifiableObjectActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Store the serialized action object
            serialized.ActionObject = SerializedActionObject;

            // Save the final serialized reference
            serializedAddObjectAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction(T)"/>
        public AddIdentifiableObjectAction(AddIdentifiableObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>AddActionObjectAction</code>
        /// </summary>
        /// <param name="serializedIdentifiableObject">The serialized <code>IdentifiableType</code> that describes
        /// the identifiable object to add.
        /// NOTE: The UUID and ID should be unique across MRET. If an identifiable object exists
        /// with the same UUID, the deserialzation will fail.
        /// </param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction()"/>
        public AddIdentifiableObjectAction(C serializedIdentifiableObject) : base(serializedIdentifiableObject.ID)
        {
            // Assign the unique settings for this action
            SerializedActionObject = serializedIdentifiableObject;
        }
    }

    /// <summary>
    /// Provides a non-generic implementation for the generic AddIdentifiableObjectAction class
    /// </summary>
    public class AddIdentifiableObjectAction : AddIdentifiableObjectAction<IdentifiableType, IIdentifiable>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AddIdentifiableObjectAction);

        /// <seealso cref="AddIdentifiableObjectAction{C,I}.AddIdentifiableObjectAction(AddIdentifiableObjectActionType)"/>
        public AddIdentifiableObjectAction(AddIdentifiableObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <seealso cref="AddIdentifiableObjectAction{C,I}.AddIdentifiableObjectAction(C)"/>
        public AddIdentifiableObjectAction(IdentifiableType serializedSceneObject) : base(serializedSceneObject)
        {
        }
    }
}