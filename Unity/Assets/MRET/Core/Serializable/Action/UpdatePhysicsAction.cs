// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class UpdatePhysicsAction :
        PhysicalSceneObjectAction<UpdatePhysicsActionType,IPhysicalSceneObject>,
        IUpdatePhysicsAction<UpdatePhysicsActionType, IPhysicalSceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UpdatePhysicsAction);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UpdatePhysicsActionType serializedUpdatePhysicsAction;

        #region IUpdatePhysicsAction
        /// <seealso cref="IUpdatePhysicsAction.mass"/>
        public float mass { get; protected set; }

        /// <seealso cref="IUpdatePhysicsAction.EnableCollisions"/>
        public bool EnableCollisions { get; protected set; }

        /// <seealso cref="IUpdatePhysicsAction.EnableGravity"/>
        public bool EnableGravity { get; protected set; }
        #endregion IUpdatePhysicsAction

        #region Serialization
        /// <summary>
        /// Deserializes the supplied serialized physics settings and updates the supplied state
        /// with the deserialization state.
        /// </summary>
        /// <param name="serializedPhysics">The serialized <code>PhysicsSettingsType</code> physics settings</param>
        /// <param name="physicsDeserializationState">The <code>SerializationState</code> to populate with the deserialization state.</param>
        /// 
        /// <see cref="PhysicsSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual void DeserializePhysics(PhysicsSettingsType serializedPhysics, SerializationState physicsDeserializationState)
        {
            // Mark as incomplete
            physicsDeserializationState.complete = false;

            // Physics settings are optional
            if (serializedPhysics is null)
            {
                physicsDeserializationState.complete = true;
                return;
            }

            // Deserialize collisions
            ActionObject.EnableCollisions = serializedPhysics.EnableCollisions;

            // Deserialize gravity
            ActionObject.EnableGravity = serializedPhysics.EnableGravity;

            // Mark as complete
            physicsDeserializationState.complete = true;
        }

        /// <seealso cref="PhysicalSceneObjectAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(UpdatePhysicsActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedUpdatePhysicsAction = serialized;

            // Process this object specific deserialization

            // Deserialize the physics settings
            SerializationState physicsDeserializationState = new SerializationState();
            DeserializePhysics(serialized.Physics, physicsDeserializationState);

            // Record the deserialization state
            deserializationState.Update(physicsDeserializationState);

            // If the physics deserialization failed, exit with an error
            if (deserializationState.IsError) return;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <summary>
        /// Asynchronously serializes the physics settings into the supplied serialized physics settings
        /// and updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedPhysics">The serialized <code>PhysicsSettingsType</code> to populate with the physics settings</param>
        /// <param name="physicsSerializationState">The <code>SerializationState</code> to populate with the serialization state.</param>
        /// 
        /// <see cref="PhysicsSettingsType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual void SerializePhysics(PhysicsSettingsType serializedPhysics, SerializationState physicsSerializationState)
        {
            // Mark as incomplete
            physicsSerializationState.complete = false;

            // Physics settings are optional
            if (serializedPhysics is null)
            {
                physicsSerializationState.complete = true;
                return;
            }

            // Serialize collisions
            serializedPhysics.EnableCollisions = EnableCollisions;

            // Serialize gravity
            serializedPhysics.EnableGravity = EnableGravity;

            // Mark as complete
            physicsSerializationState.complete = true;
        }

        /// <seealso cref="PhysicalSceneObjectAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(UpdatePhysicsActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the physics settings
            PhysicsSettingsType serializedPhysics = serialized.Physics;
            if ((serializedPhysics == null) &&
                (serializedUpdatePhysicsAction != null))
            {
                // Use this physics structure
                serializedPhysics = serializedUpdatePhysicsAction.Physics;
            }
            SerializationState physicsSerializationState = new SerializationState();
            SerializePhysics(serializedPhysics, physicsSerializationState);

            // Record the serialization state
            serializationState.Update(physicsSerializationState);

            // If the physics serialization failed, exit with an error
            if (serializationState.IsError) return;

            // Serialize out the physics settings
            serialized.Physics = serializedPhysics;

            // Save the final serialized reference
            serializedUpdatePhysicsAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            EnableGravity = PhysicalSceneObjectDefaults.GRAVITY_ENABLED;
            EnableCollisions = PhysicalSceneObjectDefaults.COLLISIONS_ENABLED;
        }

        /// <summary>
        /// Constructor for the <code>UpdatePhysicsAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="PhysicalSceneObjectAction{T,C}.PhysicalSceneObjectAction(T)"/>
        public UpdatePhysicsAction(UpdatePhysicsActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>UpdatePhysicsAction</code>
        /// </summary>
        /// <param name="physicalSceneObject">The <code>IPhysicalSceneObject</code> associated with this action</param>
        /// <param name="enableCollisions">The collisions enabled associated with this action</param>
        /// <param name="enableGravity">The gravlty enabled associated with this action</param>
        /// <param name="mass">The mass associated with this action</param>
        /// <seealso cref="PhysicalSceneObjectAction{T,C}.PhysicalSceneObjectAction(T)"/>
        public UpdatePhysicsAction(IPhysicalSceneObject physicalSceneObject,
            bool enableCollisions, bool enableGravity, float mass) : base(physicalSceneObject)
        {
            // Assign the unique settings for this action
            EnableCollisions = enableCollisions;
            EnableGravity = enableGravity;
            this.mass = mass;
        }
    }
}