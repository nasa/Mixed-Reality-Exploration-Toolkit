// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    /// <remarks>
    /// History:
    /// 3 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// Annotation source object.<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SourceAnnotation<T> : Annotation<T>, ISourceAnnotation<T>
        where T : SourceAnnotationType, new()
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(SourceAnnotation<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedAnnotationSource;

        #region ISourceAnnotation
        /// <seealso cref="ISourceAnnotation.CreateSerializedType"/>
        SourceAnnotationType ISourceAnnotation.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="ISourceAnnotation.StartTime"/>
        public float StartTime { get; set; }

        /// <seealso cref="ISourceAnnotation.Duration"/>
        public float Duration { get; set; }

        /// <seealso cref="ISourceAnnotation.Speed"/>
        public float Speed { get; set; }

        /// <seealso cref="ISourceAnnotation.Source"/>
        public object Source { get; private set; }

        /// <seealso cref="ISourceAnnotation.Synchronize(SourceAnnotationType, Action{bool, string})"/>
        void ISourceAnnotation.Synchronize(SourceAnnotationType serialized, Action<bool, string> onFinished)
        {
            Synchronize(serialized as T, onFinished);
        }

        /// <seealso cref="ISourceAnnotation.Deserialize(SourceAnnotationType, Action{bool, string})"/>
        void ISourceAnnotation.Deserialize(SourceAnnotationType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="ISourceAnnotation.Serialize(SourceAnnotationType, Action{bool, string})"/>
        void ISourceAnnotation.Serialize(SourceAnnotationType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion ISourceAnnotation

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Set the defaults
            StartTime = SourceAnnotationDefaults.START_TIME;
            Duration = SourceAnnotationDefaults.DURATION;
            Speed = SourceAnnotationDefaults.SPEED;
            Source = null;
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <summary>
        /// Asynchronously Deserializes the supplied serialized annotation source and updates the supplied state
        /// with the resulting annotation source.
        /// </summary>
        /// <param name="serializedSource">The serialized <code>AnnotationSourceType</code> model</param>
        /// <param name="sourceDeserializationState">The <code>ObjectSerializationState</code> to populate with the result.</param>
        /// 
        /// <see cref="ModelType"/>
        /// <see cref="ObjectSerializationState{O}"/>
        /// 
        protected virtual IEnumerator DeserializeSource(AnnotationSourceType serializedSource, ObjectSerializationState<object> sourceDeserializationState)
        {
            void DeserializeSourceAction(object source)
            {
                // Assign the source
                sourceDeserializationState.obj = source;

                // Update the model state
                if (source == null)
                {
                    sourceDeserializationState.Error("The source annotation failed to deserialize");
                }

                // Mark as complete
                sourceDeserializationState.complete = true;
            };

            // Load the source
            SchemaUtil.LoadAnnotationSource(serializedSource, DeserializeSourceAction);

            yield return null;
        }

        /// <seealso cref="Annotation{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(T serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedAnnotationSource = serialized;

            // Process this object specific deserialization

            // Deserialize start time
            if (serializedAnnotationSource.StartTime != null)
            {
                float startTime = 0f;
                SchemaUtil.DeserializeDuration(serializedAnnotationSource.StartTime, ref startTime);
                StartTime = startTime;
            }

            // Deserialize the source duration
            if (serializedAnnotationSource.Duration != null)
            {
                float duration = 0f;
                SchemaUtil.DeserializeDuration(serializedAnnotationSource.Duration, ref duration);
                Duration = duration;
            }

            // Deserialize the speed
            Speed = serializedAnnotationSource.Speed;

            // Perform the source deserialization
            ObjectSerializationState<object> sourceDeserializationState = new ObjectSerializationState<object>();
            StartCoroutine(DeserializeSource(serializedAnnotationSource.Source, sourceDeserializationState));

            // Wait for the coroutine to complete
            while (!sourceDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // Record the deserialization state
            deserializationState.Update(sourceDeserializationState);

            // If the source loading failed, exit with an error
            if (deserializationState.IsError) yield break;

            // Clear the source deserialization state so we can reuse it
            sourceDeserializationState.Clear();

            // Allow the subclasses to initialize the source
            StartCoroutine(InitializeSource(sourceDeserializationState));

            // Wait for the coroutine to complete
            while (!sourceDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // Record the deserialization state
            deserializationState.Update(sourceDeserializationState);

            // If the source loading failed, exit with an error
            if (deserializationState.IsError) yield break;

            // Assign the source
            Source = sourceDeserializationState.obj;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Annotation{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the start time (optional) only if it was defined during deserialization
            DurationType serializedStartTime = null;
            if (serializedAnnotationSource.StartTime != null)
            {
                serializedStartTime = serializedAnnotationSource.StartTime;
                SchemaUtil.SerializeDuration(StartTime, serializedStartTime);
            }
            serialized.StartTime = serializedStartTime;

            // Serialize the duration (optional) only if it was defined during deserialization
            DurationType serializedDuration = null;
            if (serializedAnnotationSource.Duration != null)
            {
                serializedDuration = serializedAnnotationSource.Duration;
                SchemaUtil.SerializeDuration(Duration, serializedStartTime);
            }
            serialized.Duration = serializedDuration;

            // Serialize the speed (optional) only if it is not the default
            if (Speed != SourceAnnotationDefaults.SPEED)
            {
                serialized.Speed = Speed;
            }

            // Start with our internal serialized source to serialize out the annotation source
            // using the original deserialized structure (if was provided during deserialization)
            AnnotationSourceType serializedSource = null;
            if (serializedAnnotationSource != null)
            {
                // Use this source structure
                serializedSource = serializedAnnotationSource.Source;
            }

            // Make sure we have a valid serialized source reference
            if (serializedSource is null)
            {
                // Default to basic source structure
                serializedSource = new AnnotationSourceType();
            }

            // Serialize out the source
            serialized.Source = serializedSource;

            // Save the final serialized reference
            serializedAnnotationSource = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <summary>
        /// Called to initialize the source. Implementing routine should update
        /// the state instead of calling yield break.
        /// </summary>
        /// <param name="sourceDeserializationState">The <code>ObjectSerializationState</code> to be updated</param>
        /// 
        /// <see cref="ObjectSerializationState{O}"/>
        protected abstract IEnumerator InitializeSource(ObjectSerializationState<object> sourceDeserializationState);
    }

    public class SourceAnnotationDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly float START_TIME = 0f;
        public static readonly float DURATION = 0f;
        public static readonly float SPEED = new SourceAnnotationType().Speed;
    }
}
