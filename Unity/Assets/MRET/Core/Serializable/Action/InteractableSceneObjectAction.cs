// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class InteractableSceneObjectAction<T,I> : SceneObjectAction<T,I>, IInteractableSceneObjectAction<T,I>
        where T : InteractableSceneObjectActionType, new()
        where I : IInteractable
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableSceneObjectAction<T, I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedInteractableSceneObjectAction;

        #region IInteractableSceneObjectAction
        /// <seealso cref="IInteractableSceneObjectAction.CreateSerializedType"/>
        InteractableSceneObjectActionType IInteractableSceneObjectAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IInteractableSceneObjectAction.ActionObject"/>
        IInteractable IInteractableSceneObjectAction.ActionObject => ActionObject;

        /// <seealso cref="IInteractableSceneObjectAction.SerializedAction"/>
        InteractableSceneObjectActionType IInteractableSceneObjectAction.SerializedAction => SerializedAction;

        /// <seealso cref="IInteractableSceneObjectAction.Deserialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        void IInteractableSceneObjectAction.Deserialize(InteractableSceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IInteractableSceneObjectAction.Serialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        void IInteractableSceneObjectAction.Serialize(InteractableSceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IInteractableSceneObjectAction

        #region Serialization
        /// <seealso cref="SceneObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedInteractableSceneObjectAction = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="SceneObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedInteractableSceneObjectAction = serialized;

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
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction()"/>
        public InteractableSceneObjectAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(T)"/>
        public InteractableSceneObjectAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="interactableSceneObject">The <code>IInteractable</code> associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public InteractableSceneObjectAction(I interactableSceneObject, ISceneObject sceneObjectParent = null) : base(interactableSceneObject, sceneObjectParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="interactableSceneObjectId">The interactable scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(string,string)"/>
        public InteractableSceneObjectAction(string interactableSceneObjectId, string parentId = null) : base(interactableSceneObjectId, parentId)
        {
        }
    }

    /// <summary>
    /// Provides an implementation for the generic InteractableSceneObjectAction class
    /// </summary>
    public class InteractableSceneObjectAction : InteractableSceneObjectAction<InteractableSceneObjectActionType, IInteractable>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableSceneObjectAction);

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction()"/>
        public InteractableSceneObjectAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="InteractableSceneObjectAction{T,I}.InteractableSceneObjectAction(T)"/>
        public InteractableSceneObjectAction(InteractableSceneObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="interactableSceneObject">The <code>IInteractable</code> associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public InteractableSceneObjectAction(IInteractable interactableSceneObject, ISceneObject sceneObjectParent = null) : base(interactableSceneObject, sceneObjectParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>InteractableSceneObjectAction</code>
        /// </summary>
        /// <param name="interactableSceneObjectId">The interactable scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(string,string)"/>
        public InteractableSceneObjectAction(string interactableSceneObjectId, string parentId = null) : base(interactableSceneObjectId, parentId)
        {
        }
    }
}