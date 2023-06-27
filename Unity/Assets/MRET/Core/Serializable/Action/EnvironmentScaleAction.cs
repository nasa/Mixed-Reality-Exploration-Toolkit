// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class EnvironmentScaleAction :
        BaseAction<EnvironmentScaleActionType>,
        IEnvironmentScaleAction<EnvironmentScaleActionType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(EnvironmentScaleAction);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private EnvironmentScaleActionType serializedEnvironmentScaleAction;

        #region IEnvironmentScaleAction
        /// <seealso cref="IEnvironmentScaleAction.Scale"/>
        public Vector3 Scale { get; protected set; }
        #endregion IEnvironmentScaleAction

        #region Serialization
        /// <seealso cref="Versioned{T}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(EnvironmentScaleActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedEnvironmentScaleAction = serialized;

            // Process this object specific deserialization
            MRET.ProjectManager.projectContainer.transform.localScale = Scale;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="Versioned{T}.Serialize(T, SerializationState)"/>
        protected override void Serialize(EnvironmentScaleActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the reference point
            TransformScaleType serializedScale = serialized.Scale;
            if (serializedScale is null)
            {
                // Default to basic transform structure
                serializedScale = new TransformScaleType();
                serialized.Scale = serializedScale;
            }
            SchemaUtil.SerializeTransformScale(serializedScale, Scale);

            // Save the final serialized reference
            serializedEnvironmentScaleAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            Scale = Vector3.one;
        }

        /// <summary>
        /// Constructor for the <code>EnvironmentScaleAction</code>
        /// </summary>
        /// <seealso cref="BaseAction{T}.BaseAction()"/>
        public EnvironmentScaleAction() : base()
        {
        }

        /// <summary>
        /// Constructor for the <code>EnvironmentScaleAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="BaseAction{T}.BaseAction(T)"/>
        public EnvironmentScaleAction(EnvironmentScaleActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>EnvironmentScaleAction</code>
        /// </summary>
        /// <param name="scale">The scale for the action</param>
        /// <seealso cref="BaseAction{T}.BaseAction(T)"/>
        public EnvironmentScaleAction(Vector3 scale) : base()
        {
            // Assign the unique settings for this action
            Scale = scale;
        }
    }
}