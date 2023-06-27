// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class PhysicalSceneObjectAction<T,I> : InteractableSceneObjectAction<T,I>, IPhysicalSceneObjectAction<T,I>
        where T : PhysicalSceneObjectActionType, new()
        where I : IPhysicalSceneObject
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PhysicalSceneObjectAction<T, I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedPhysicalSceneObjectAction;

        #region IPhysicalSceneObjectAction
        /// <seealso cref="IPhysicalSceneObjectAction.CreateSerializedType"/>
        PhysicalSceneObjectActionType IPhysicalSceneObjectAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IPhysicalSceneObjectAction.ActionObject"/>
        IPhysicalSceneObject IPhysicalSceneObjectAction.ActionObject => ActionObject;

        /// <seealso cref="IPhysicalSceneObjectAction.SerializedAction"/>
        PhysicalSceneObjectActionType IPhysicalSceneObjectAction.SerializedAction => SerializedAction;

        /// <seealso cref="IPhysicalSceneObjectAction.Deserialize(PhysicalSceneObjectActionType, Action{bool, string})"/>
        void IPhysicalSceneObjectAction.Deserialize(PhysicalSceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IPhysicalSceneObjectAction.Serialize(PhysicalSceneObjectActionType, Action{bool, string})"/>
        void IPhysicalSceneObjectAction.Serialize(PhysicalSceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IInteractableSceneObjectAction

        #region Serialization
        /// <seealso cref="InteractableSceneObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedPhysicalSceneObjectAction = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="InteractableSceneObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedPhysicalSceneObjectAction = serialized;

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
        /// Constructor for the <code>SceneObjectTransformAction</code>
        /// </summary>
        /// <seealso cref="PhysicalSceneObjectAction{T,I}.PhysicalSceneObjectAction()"/>
        public PhysicalSceneObjectAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>PhysicalSceneObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction(T)"/>
        public PhysicalSceneObjectAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>PhysicalSceneObjectAction</code>
        /// </summary>
        /// <param name="physicalSceneObject">The <code>IPhysicalSceneObject</code> associated with this action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction(I,ISceneObject)"/>
        public PhysicalSceneObjectAction(I physicalSceneObject, ISceneObject sceneObjectParent = null) : base(physicalSceneObject, sceneObjectParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="physicalSceneObjectId">The physical scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction(string,string)"/>
        public PhysicalSceneObjectAction(string physicalSceneObjectId, string parentId = null) : base(physicalSceneObjectId, parentId)
        {
        }
    }

    /// <summary>
    /// Provides an implementation for the generic PhysicalSceneObjectAction class
    /// </summary>
    public class PhysicalSceneObjectAction : PhysicalSceneObjectAction<PhysicalSceneObjectActionType, IPhysicalSceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PhysicalSceneObjectAction);

        /// <summary>
        /// Constructor for the <code>PhysicalSceneObjectAction</code>
        /// </summary>
        /// <seealso cref="PhysicalSceneObjectAction{T,I}.PhysicalSceneObjectAction()"/>
        public PhysicalSceneObjectAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>PhysicalSceneObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction(T)"/>
        public PhysicalSceneObjectAction(PhysicalSceneObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>PhysicalSceneObjectAction</code>
        /// </summary>
        /// <param name="physicalSceneObject">The <code>IPhysicalSceneObject</code> associated with this action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction(I,ISceneObject)"/>
        public PhysicalSceneObjectAction(IPhysicalSceneObject physicalSceneObject, ISceneObject sceneObjectParent = null) : base(physicalSceneObject, sceneObjectParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="physicalSceneObjectId">The physical scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction(string,string)"/>
        public PhysicalSceneObjectAction(string physicalSceneObjectId, string parentId = null) : base(physicalSceneObjectId, parentId)
        {
        }
    }
}