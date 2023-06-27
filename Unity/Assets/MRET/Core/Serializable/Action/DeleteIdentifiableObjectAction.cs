// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class DeleteIdentifiableObjectAction<I> :
        IdentifiableObjectAction<DeleteIdentifiableObjectActionType,I>,
        IDeleteIdentifiableObjectAction<DeleteIdentifiableObjectActionType, I>
        where I : IIdentifiable
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DeleteIdentifiableObjectAction<I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private DeleteIdentifiableObjectActionType serializedDeleteObjectAction;

        #region Serialization
        /// <seealso cref="IdentifiableObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(DeleteIdentifiableObjectActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedDeleteObjectAction = serialized;

            // Process this object specific deserialization
            UnityEngine.Object.Destroy(ActionObject as UnityEngine.Object);
            ActionObject = default;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(DeleteIdentifiableObjectActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedDeleteObjectAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
        }

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction(T)"/>
        public DeleteIdentifiableObjectAction(DeleteIdentifiableObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="identifiableObject">The <code>IIdentifiable</code> associated with this action</param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction(I)"/>
        public DeleteIdentifiableObjectAction(I identifiableObject) : base(identifiableObject)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="identifiableObjectId">The identifiable object ID associated with this action</param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction(string)"/>
        public DeleteIdentifiableObjectAction(string identifiableObjectId) : base(identifiableObjectId)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="serializedIdentifiableObject">The serialized <code>IdentifiableType</code> that describes
        /// the identifiable object to delete.
        /// </param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction()"/>
        public DeleteIdentifiableObjectAction(IdentifiableType serializedIdentifiableObject) : base()
        {
            // Assign the unique settings for this action
            ActionObjectID = serializedIdentifiableObject.ID;
        }

    }

    /// <summary>
    /// Provides an implementation for the generic SceneObjectTransformAction class
    /// </summary>
    public class DeleteIdentifiableObjectAction : DeleteIdentifiableObjectAction<IIdentifiable>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DeleteIdentifiableObjectAction);

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="DeleteIdentifiableObjectAction{I}.DeleteIdentifiableObjectAction(DeleteIdentifiableObjectActionType)"/>
        public DeleteIdentifiableObjectAction(DeleteIdentifiableObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="identifiableObject">The <code>IIdentifiable</code> associated with this action</param>
        /// <seealso cref="DeleteIdentifiableObjectAction{I}.DeleteIdentifiableObjectAction(I)"/>
        public DeleteIdentifiableObjectAction(IIdentifiable identifiableObject) : base(identifiableObject)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="identifiableObjectId">The identifiable object ID associated with this action</param>
        /// <seealso cref="DeleteIdentifiableObjectAction{T,I}.DeleteIdentifiableObjectAction(string)"/>
        public DeleteIdentifiableObjectAction(string identifiableObjectId) : base(identifiableObjectId)
        {
        }

        /// <summary>
        /// Constructor for the <code>DeleteIdentifiableObjectAction</code>
        /// </summary>
        /// <param name="serializedIdentifiableObject">The serialized <code>IdentifiableType</code> that describes
        /// the identifiable object to delete.
        /// </param>
        /// <seealso cref="DeleteIdentifiableObjectAction{I}.DeleteIdentifiableObjectAction(IdentifiableType)"/>
        public DeleteIdentifiableObjectAction(IdentifiableType serializedIdentifiableObject) : base(serializedIdentifiableObject)
        {
        }

    }
}