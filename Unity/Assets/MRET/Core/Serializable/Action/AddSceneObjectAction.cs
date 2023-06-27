// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// Generic AddSceneObjectAction
	///
	/// An add scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public class AddSceneObjectAction<C, I> :
        SceneObjectTransformAction<AddSceneObjectActionType, I>,
        IAddSceneObjectAction<AddSceneObjectActionType, C, I>
        where C : SceneObjectType, new()
        where I : ISceneObject
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AddSceneObjectAction<C, I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private AddSceneObjectActionType serializedAddObjectAction;

        #region IAddSceneObjectAction
        /// <seealso cref="IAddSceneObjectAction.CreateSerializedType"/>
        AddSceneObjectActionType IAddSceneObjectAction.CreateSerializedType() => CreateSerializedType();

        /// <summary>
        /// The serialized scene object to instantiate
        /// </summary>
        public C SerializedSceneObject { get; protected set; }

        /// <seealso cref="IAddSceneObjectAction.SerializedSceneObject"/>
        SceneObjectType IAddSceneObjectAction.SerializedSceneObject => SerializedSceneObject;
        #endregion IAddSceneObjectAction

        #region Serialization
        /// <seealso cref="IdentifiableObjectAction{T, I}.DeserializeActionObjectID(string, VersionedSerializationState{I})"/>
        protected override void DeserializeActionObjectID(string serializedActionObjectID, VersionedSerializationState<I> actionObjectDeserializationState)
        {
            // Take the inherited behavior to start
            base.DeserializeActionObjectID(serializedActionObjectID, actionObjectDeserializationState);

            // ********* FIXME!!!! This seems ripe for issues ************
            // The issue here is that ActionObjectID is in the base class,
            // but do we want the ID set in the XML under ActionObjectID or
            // as part of the ActionObject definition...
            // 1) do they need to match?
            // 2) do we ignore the ActionObjectID?
            // 3) do we override ActionObjectID or ActionObject.id after instantiation?
            // 4) Does the whole action hierarchy need to change?

            // Make sure a scene object doesn't already exist with this ID.
            // We want a null scene object
            if (actionObjectDeserializationState.versioned == null)
            {
                // This is a valid condition
                actionObjectDeserializationState.Clear();

                Action<I> SceneObjectSerializedAction = (I loadedObj) =>
                {
                    if (loadedObj != null)
                    {
                        // FIXME: Override ActionObjectID or the other way around?
                        ActionObjectID = loadedObj.id;
                    }
                    else
                    {
                        // Error condition
                        actionObjectDeserializationState.Error("A problem occurred " +
                            "attempting to instantiate the scene object: " + serializedActionObjectID);
                    }
                };

                // Create the new scene object
                if (SerializedSceneObject != null)
                {
                    MRET.ProjectManager.InstantiateObject(
                        SerializedSceneObject, null, Parent?.transform, SceneObjectSerializedAction);
                }
            }

            // If there is an error, abort
            if (actionObjectDeserializationState.IsError) return;

            // Mark as complete
            actionObjectDeserializationState.complete = true;
        }

        /// <seealso cref="SceneObjectTransformAction{C}.Deserialize(C, SerializationState)"/>
        protected override void Deserialize(AddSceneObjectActionType serialized, SerializationState deserializationState)
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

        /// <seealso cref="SceneObjectTransformAction{C}.Serialize(C, SerializationState)"/>
        protected override void Serialize(AddSceneObjectActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Store the serialized scene object
            serialized.ActionObject = SerializedSceneObject;

            // Save the final serialized reference
            serializedAddObjectAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{C}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
        }

        /// <seealso cref="SceneObjectTransformAction{C,I}.SceneObjectTransformAction(C)"/>
        public AddSceneObjectAction(AddSceneObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>AddSceneObjectAction</code>
        /// </summary>
        /// <param name="serializedSceneObject">The serialized <code>SceneObjectType</code> that describes
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// the scene object to add.
        /// NOTE: The UUID and ID should be unique across MRET. If an identifiable object exists
        /// with the same UUID, the deserialzation will fail.
        /// </param>
        /// <seealso cref="SceneObjectTransformAction{C,I}.SceneObjectTransformAction()"/>
        public AddSceneObjectAction(C serializedSceneObject, string parentId = "") : base(serializedSceneObject.ID, parentId)
        {
            // Assign the unique settings for this action
            SerializedSceneObject = serializedSceneObject;
        }
    }
    
    /// <summary>
    /// Provides an non-generic implementation for the generic AddSceneObjectAction class
    /// </summary>
    public class AddSceneObjectAction : AddSceneObjectAction<SceneObjectType, ISceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AddSceneObjectAction);

        /// <seealso cref="AddSceneObjectAction{C,I}.AddSceneObjectAction(AddSceneObjectActionType)"/>
        public AddSceneObjectAction(AddSceneObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <seealso cref="AddSceneObjectAction{C,I}.AddSceneObjectAction(C, string)"/>
        public AddSceneObjectAction(SceneObjectType serializedSceneObject, string parentId = "") :
            base(serializedSceneObject, parentId)
        {
        }
    }

}