// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class SceneObjectAction<T,I> : IdentifiableObjectAction<T,I>, ISceneObjectAction<T,I>
        where T : SceneObjectActionType, new()
        where I : ISceneObject
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectAction<T, I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedSceneObjectAction;

        #region ISceneObjectAction
        /// <seealso cref="ISceneObjectAction.CreateSerializedType"/>
        SceneObjectActionType ISceneObjectAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="ISceneObjectAction.ParentID"/>
        public virtual string ParentID
        {
            get => (Parent == null) ? _parentId : Parent.id;
            protected set
            {
                // If the Parent is not defined, allow the change
                _parentId = (Parent == null) ? value : Parent.id;
            }
        }
        private string _parentId;

        /// <seealso cref="ISceneObjectAction.SceneObjectParent"/>
        public ISceneObject Parent { get; protected set; }

        /// <seealso cref="ISceneObjectAction.ActionObject"/>
        ISceneObject ISceneObjectAction.ActionObject => ActionObject;

        /// <seealso cref="ISceneObjectAction.SerializedAction"/>
        SceneObjectActionType ISceneObjectAction.SerializedAction => SerializedAction;

        /// <seealso cref="ISceneObjectAction.Deserialize(SceneObjectActionType, Action{bool, string})"/>
        void ISceneObjectAction.Deserialize(SceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="ISceneObjectAction.Serialize(SceneObjectActionType, Action{bool, string})"/>
        void ISceneObjectAction.Serialize(SceneObjectActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion ISceneObjectAction

        #region Serialization
        /// <summary>
        /// Deserializes the supplied serialized parent ID and updates the supplied state with the
        /// resulting action object reference. This method is available for subclasses that require
        /// an object associated with the action.<br>
        /// </summary>
        /// <param name="serializedParentObjectID">The serialized parent object ID</param>
        /// <param name="actionObjectDeserializationState">The <code>VersionedSerializationState</code>
        ///     to populate with the <code>IIdentifiable</code> result.</param>
        /// 
        /// <see cref="VersionedSerializationState{V}"/>
        protected virtual void DeserializeParentID(string serializedParentObjectID, VersionedSerializationState<ISceneObject> actionObjectDeserializationState)
        {
            // Determine if the action object parent ID is specified
            if (!string.IsNullOrEmpty(serializedParentObjectID))
            {
                // Set the scene object parent ID
                ParentID = serializedParentObjectID;

                // Look up the scene object parent by ID
                IIdentifiable parentObject = MRET.UuidRegistry.GetByID(serializedParentObjectID);
                if (parentObject is ISceneObject)
                {
                    // Set the scene object parent
                    actionObjectDeserializationState.versioned = parentObject as ISceneObject;
                }
                else
                {
                    // Error condition
                    actionObjectDeserializationState.Error("Specified scene object parent ID is invalid. " +
                        "Must be of type: " + nameof(ISceneObject));

                    // Clear the scene object reference
                    actionObjectDeserializationState.versioned = null;
                }
            }

            // If there's an error, abort
            if (actionObjectDeserializationState.IsError) return;

            // Mark as complete
            actionObjectDeserializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedSceneObjectAction = serialized;

            // Process this object specific deserialization

            // Deserialize the scene object parent ID
            VersionedSerializationState<ISceneObject> actionObjectDeserializationState = new VersionedSerializationState<ISceneObject>();
            DeserializeParentID(serialized.ParentID, actionObjectDeserializationState);

            // Record the deserialization state
            deserializationState.Update(actionObjectDeserializationState);

            // If the action object parent ID loading failed, exit with an error
            if (deserializationState.IsError) return;

            // Assign the scene object parent reference
            Parent = actionObjectDeserializationState.versioned;
            ActionObject.transform.parent = Parent.transform;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the action object parent ID
            serialized.ParentID = ParentID;

            // Save the final serialized reference
            serializedSceneObjectAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            ParentID = "";
            Parent = default(I);
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction()"/>
        public SceneObjectAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction(T)"/>
        public SceneObjectAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="sceneObjectParent">The optional <code>ISceneObject</code> parent associated with this action</param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction(I)"/>
        public SceneObjectAction(I sceneObject, ISceneObject sceneObjectParent = null) : base(sceneObject)
        {
            // Assign the unique settings for this action
            Parent = sceneObjectParent;
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="IdentifiableObjectAction{T,I}.IdentifiableObjectAction(string)"/>
        public SceneObjectAction(string sceneObjectId, string parentId = null) : base(sceneObjectId)
        {
            // Assign the unique settings for this action
            ParentID = parentId;
        }
    }

    /// <summary>
    /// Provides an implementation for the generic SceneObjectAction class
    /// </summary>
    public class SceneObjectAction : SceneObjectAction<SceneObjectActionType, ISceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectAction);

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction()"/>
        public SceneObjectAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(T)"/>
        public SceneObjectAction(SceneObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <param name="sceneObject">The <code>ISceneObject</code> associated with this action</param>
        /// <param name="sceneObjectParent">The optional <code>ISceneObject</code> parent associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(I,ISceneObject)"/>
        public SceneObjectAction(ISceneObject sceneObject, ISceneObject sceneObjectParent = null) : base(sceneObject, sceneObjectParent)
        {
        }

        /// <summary>
        /// Constructor for the <code>SceneObjectAction</code>
        /// </summary>
        /// <param name="sceneObjectId">The scene object ID associated with this action</param>
        /// <param name="parentId">The optional scene object parent ID associated with this action</param>
        /// <seealso cref="SceneObjectAction{T,I}.SceneObjectAction(string)"/>
        public SceneObjectAction(string sceneObjectId, string parentId = null) : base(sceneObjectId, parentId)
        {
        }
    }
}