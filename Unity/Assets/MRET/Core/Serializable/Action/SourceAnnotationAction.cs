// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Annotation;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class SourceAnnotationAction : AnnotationAction<SourceAnnotationActionType,ISourceAnnotation>,
        ISourceAnnotationAction<SourceAnnotationActionType,ISourceAnnotation>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SourceAnnotationAction);

        public readonly float DEFAULT_SPEED = 1f;
        public readonly float DEFAULT_START_TIME = 0f;
        public readonly float DEFAULT_DURATION = 0f;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private SourceAnnotationActionType serializedSourceAnnotationAction;

        #region ISourceAnnotationAction
        /// <summary>
        /// The start time for this source annotation action
        /// </summary>
        public float StartTime { get; protected set; }

        /// <summary>
        /// The duration for this source annotation action
        /// </summary>
        public float Duration { get; protected set; }

        /// <summary>
        /// The speed for this source annotation action
        /// </summary>
        public float Speed { get; protected set; }
        #endregion ISourceAnnotationAction

        #region Serialization
        /// <seealso cref="AnnotationAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(SourceAnnotationActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedSourceAnnotationAction = serialized;

            // Process this object specific deserialization

            // Deserialize the start time
            float startTime = DEFAULT_START_TIME;
            if (serialized.StartTime != null)
            {
                SchemaUtil.DeserializeDuration(serialized.StartTime, ref startTime);
            }
            StartTime = startTime;
            ActionObject.StartTime = StartTime;

            // Deserialize the duration
            float duration = DEFAULT_DURATION;
            if (serialized.Duration != null)
            {
                SchemaUtil.DeserializeDuration(serialized.Duration, ref duration);
            }
            Duration = duration;
            ActionObject.Duration = Duration;

            // Deserialize the speed
            Speed = serialized.Speed;
            ActionObject.Speed = Speed;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="AnnotationAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(SourceAnnotationActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the start time (optional) only if it was defined during deserialization
            DurationType serializedStartTime = null;
            if (serializedSourceAnnotationAction.StartTime != null)
            {
                serializedStartTime = serializedSourceAnnotationAction.StartTime;
                SchemaUtil.SerializeDuration(StartTime, serializedStartTime);
            }
            serialized.StartTime = serializedStartTime;

            // Serialize the duration (optional) only if it was defined during deserialization
            DurationType serializedDuration = null;
            if (serializedSourceAnnotationAction.Duration != null)
            {
                serializedDuration = serializedSourceAnnotationAction.Duration;
                SchemaUtil.SerializeDuration(Duration, serializedDuration);
            }
            serialized.Duration = serializedDuration;

            // Serialize the speed (optional)
            if (Speed != DEFAULT_SPEED)
            {
                serialized.Speed = Speed;
            }

            // Save the final serialized reference
            serializedSourceAnnotationAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            StartTime = DEFAULT_START_TIME;
            Duration = DEFAULT_DURATION;
            Speed = DEFAULT_SPEED;
        }

        /// <summary>
        /// Constructor for the <codeSourceAnnotationAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="AnnotationAction{T}.AnnotationAction(T)"/>
        public SourceAnnotationAction(SourceAnnotationActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>SourceAnnotationAction</code>
        /// </summary>
        /// <param name="annotation">The <code>ISourceAnnotation</code> associated with this action</param>
        /// <param name="startTime">The start time of the source</param>
        /// <param name="duration">The duration of the source</param>
        /// <param name="speed">The speed of the source</param>
        /// <seealso cref="AnnotationAction{T,C}.AnnotationAction(C,AnnotationControlType)"/>
        public SourceAnnotationAction(ISourceAnnotation annotation,
            float startTime, float duration, float speed) : base(annotation)
        {
            // Assign the unique settings for this action
            StartTime = startTime;
            Duration = duration;
            Speed = speed;
        }

    }
}