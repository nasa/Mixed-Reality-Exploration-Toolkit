// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class IdentifiableObjectAction<T,I> : BaseAction<T>, IIdentifiableObjectAction<T,I>
        where T : IdentifiableObjectActionType, new()
        where I : IIdentifiable
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IdentifiableObjectAction<T,I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedIdentifiableObjectAction;

        #region IIdentifiableObjectAction
        /// <seealso cref="IIdentifiableObjectAction.CreateSerializedType"/>
        IdentifiableObjectActionType IIdentifiableObjectAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IIdentifiableObjectAction.ActionObjectID"/>
        public virtual string ActionObjectID
        {
            get
            {
                return (ActionObject == null) ? _actionObjectId : ActionObject.id;
            }
            protected set
            {
                // If the ActionObject is not defined, allow the change
                _actionObjectId = (ActionObject == null) ? value : ActionObject.id;
            }
        }
        private string _actionObjectId;

        /// <seealso cref="IIdentifiableObjectAction.ActionObject"/>
        public I ActionObject { get; protected set; }

        /// <seealso cref="IIdentifiableObjectAction.ActionObject"/>
        IIdentifiable IIdentifiableObjectAction.ActionObject => ActionObject;

        /// <seealso cref="IIdentifiableObjectAction.SerializedAction"/>
        IdentifiableObjectActionType IIdentifiableObjectAction.SerializedAction => SerializedAction;

        /// <seealso cref="IIdentifiableObjectAction.Deserialize(IdentifiableObjectActionType, Action{bool, string})"/>
        void IIdentifiableObjectAction.Deserialize(IdentifiableObjectActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IIdentifiableObjectAction.Serialize(IdentifiableObjectActionType, Action{bool, string})"/>
        void IIdentifiableObjectAction.Serialize(IdentifiableObjectActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IIdentifiableObjectAction

        #region Serializable
        /// <summary>
        /// Coroutine to deserialize the supplied serialized action object ID and updates the
        /// supplied state with the resulting action object reference. This method is available
        /// for subclasses thaat require an object associated with the action.<br>
        /// </summary>
        /// <param name="serializedActionObjectID">The serialized action object ID</param>
        /// <param name="actionObjectDeserializationState">The <code>VersionedSerializationState</code>
        ///     to populate with the result.</param>
        /// 
        /// <see cref="VersionedSerializationState{V}"/>
        /// 
        protected virtual void DeserializeActionObjectID(string serializedActionObjectID, VersionedSerializationState<I> actionObjectDeserializationState)
        {
            // Determine if the action object ID is valid
            if (!string.IsNullOrEmpty(serializedActionObjectID))
            {
                // Set the action object ID
                ActionObjectID = serializedActionObjectID;

                // Look up the action object by ID
                IIdentifiable actionObject = MRET.UuidRegistry.GetByID(serializedActionObjectID);
                if (actionObject is I)
                {
                    // Set the action object
                    actionObjectDeserializationState.versioned = (I)actionObject;
                }
                else
                {
                    // Clear the action object reference
                    actionObjectDeserializationState.versioned = default;

                    // Error condition
                    actionObjectDeserializationState.Error("Specified action object ID is invalid. " +
                        "Must be of type: " + nameof(I));
                }
            }
            else
            {
                // Clear the action object reference
                actionObjectDeserializationState.versioned = default;

                // Error condition
                actionObjectDeserializationState.Error("Specified action object ID is not defined");
            }

            // If there's an error, abort
            if (actionObjectDeserializationState.IsError) return;

            // Mark as complete
            actionObjectDeserializationState.complete = true;
        }

        /// <seealso cref="BaseAction{T}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedIdentifiableObjectAction = serialized;

            // Process this object specific deserialization

            // Deserialize the action object
            VersionedSerializationState<I> actionObjectDeserializationState = new VersionedSerializationState<I>();
            DeserializeActionObjectID(serialized.ActionObjectID, actionObjectDeserializationState);

            // Record the deserialization state
            deserializationState.Update(actionObjectDeserializationState);

            // If the action object ID loading failed, exit with an error
            if (deserializationState.IsError) return;

            // Assign the action object reference
            if (actionObjectDeserializationState.versioned is null)
            {
                // Error condition
                actionObjectDeserializationState.Error("Specified action object " +
                    "is invalid. Must be of type: " + nameof(I));
                return;
            }
            ActionObject = actionObjectDeserializationState.versioned;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="BaseAction{T}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the action object ID
            if (string.IsNullOrEmpty(ActionObjectID))
            {
                // Error condition
                serializationState.Error("Action object ID is not defined");
                return;
            }
            serialized.ActionObjectID = ActionObjectID;

            // Save the final serialized reference
            serializedIdentifiableObjectAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            ActionObjectID = "";
            ActionObject = default;
        }

        /// <summary>
        /// Constructor for the <code>IdentifiableObjectAction</code>
        /// </summary>
        /// <seealso cref="BaseAction{T}.BaseAction()"/>
        public IdentifiableObjectAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>IdentifiableObjectAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="BaseAction{T}.BaseAction(T)"/>
        public IdentifiableObjectAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>IdentifiableObjectAction</code>
        /// </summary>
        /// <param name="actionObject">The <code>IIdentifiable</code> object associated with this action</param>
        /// <seealso cref="BaseAction{T}.BaseAction()"/>
        public IdentifiableObjectAction(I actionObject) : base()
        {
            // Assign the unique settings for this action
            ActionObject = actionObject;
        }

        /// <summary>
        /// Constructor for the <code>IdentifiableObjectAction</code>
        /// </summary>
        /// <param name="actionObjectId">The action object ID associated with this action</param>
        /// <seealso cref="BaseAction{T}.BaseAction()"/>
        public IdentifiableObjectAction(string actionObjectId) : base()
        {
            // Assign the unique settings for this action
            ActionObjectID = actionObjectId;
        }
    }
}