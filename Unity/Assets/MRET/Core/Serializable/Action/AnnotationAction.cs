// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Annotation;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public abstract class AnnotationAction<T,C> : IdentifiableObjectAction<T,C>, IAnnotationAction<T,C>
        where T : AnnotationActionType, new()
        where C : IAnnotation
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AnnotationAction<T,C>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedAnnotationAction;

        #region IAnnotationAction
        /// <seealso cref="IAnnotationAction.CreateSerializedType"/>
        AnnotationActionType IAnnotationAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IAnnotationAction.ActionObject"/>
        IAnnotation IAnnotationAction.ActionObject => ActionObject;

        /// <seealso cref="IAnnotationAction.SerializedAction"/>
        AnnotationActionType IAnnotationAction.SerializedAction => SerializedAction;

        /// <seealso cref="IAnnotationAction.Deserialize(AnnotationActionType, Action{bool, string})"/>
        void IAnnotationAction.Deserialize(AnnotationActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IAnnotationAction.Serialize(AnnotationActionType, Action{bool, string})"/>
        void IAnnotationAction.Serialize(AnnotationActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IAnnotationAction

        /// <summary>
        /// The annotation control for this action
        /// </summary>
        public AnnotationControlType Control { get; protected set; }

        #region Serializable
        /// <seealso cref="IdentifiableObjectAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedAnnotationAction = serialized;

            // Process this object specific deserialization

            // Deserialize the control
            Control = serialized.Control;
            switch (Control)
            {
                case AnnotationControlType.Play:
                    ActionObject.Play();
                    break;
                case AnnotationControlType.Stop:
                    ActionObject.Stop();
                    break;

                case AnnotationControlType.Pause:
                    ActionObject.Pause();
                    break;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the control
            serialized.Control = Control;

            // Save the final serialized reference
            serializedAnnotationAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            Control = AnnotationControlType.Pause;
        }

        /// <summary>
        /// Constructor for the <code>AnnotationAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="IdentifiableObjectAction{T,C}.IdentifiableObjectAction(T)"/>
        public AnnotationAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>AnnotationAction</code>
        /// </summary>
        /// <param name="annotation">The annotation associated with this action</param>
        /// <param name="control">The annotation control for this action</param>
        /// <seealso cref="IdentifiableObjectAction{T,C}.IdentifiableObjectAction(C)"/>
        public AnnotationAction(C annotation,
            AnnotationControlType control = AnnotationControlType.Pause) : base(annotation)
        {
            // Assign the unique settings for this action
            Control = control;
        }

    }
}