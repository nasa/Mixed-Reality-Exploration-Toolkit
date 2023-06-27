// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    /// <remarks>
    /// History:
    /// 3 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// Annotation object.<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class Annotation<T> : Identifiable<T>, IAnnotation<T>
        where T : AnnotationType, new()
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(Annotation<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedAnnotation;

        #region IAnnotation
        /// <seealso cref="IAnnotation.CreateSerializedType"/>
        AnnotationType IAnnotation.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IAnnotation.IsPlaying"/>
        public bool IsPlaying { get; private set; }

        /// <seealso cref="IAnnotation.IsPaused"/>
        public bool IsPaused { get; private set; }

        /// <seealso cref="IAnnotation.StartDelay"/>
        public float StartDelay { get; set; }

        /// <seealso cref="IAnnotation.Loop"/>
        public bool Loop { get; set; }

        /// <seealso cref="IAnnotation.AttachTo"/>
        private string _attachTo = AnnotationDefaults.ATTACH_TO;
        public string AttachTo
        {
            get => _attachTo;
            set
            {
                _attachTo = AnnotationDefaults.ATTACH_TO;
                if (!string.IsNullOrEmpty(value))
                {
                    IIdentifiable attachTo = MRET.UuidRegistry.GetByID(value);
                    if (attachTo != null)
                    {
                        _attachTo = value;
                        AttachDependentComponents(attachTo.gameObject);
                    }
                }
            }
        }

        /// <seealso cref="IAnnotation.Play"/>
        public void Play()
        {
            if (CanPlay() && !IsPlaying)
            {
                // Notify the subclass to perform the play
                DoPlay();

                IsPlaying = true;
            }
        }

        /// <seealso cref="IAnnotation.Pause"/>
        public void Pause()
        {
            if (IsPlaying && !IsPaused)
            {
                // Notify the subclass to perform the pause
                DoPause();

                IsPaused = true;
            }
        }

        /// <seealso cref="IAnnotation.Resume"/>
        public void Resume()
        {
            if (IsPlaying && IsPaused)
            {
                // Notify the subclass to perform the resume
                DoResume();

                IsPaused = false;
            }
        }

        /// <seealso cref="IAnnotation.Stop"/>
        public void Stop()
        {
            if (IsPlaying)
            {
                // Notify the subclass to perform the stop
                DoStop();

                IsPlaying = false;
            }
        }

        /// <seealso cref="IAnnotation.Synchronize(AnnotationType, Action{bool, string})"/>
        void IAnnotation.Synchronize(AnnotationType serialized, Action<bool, string> onFinished)
        {
            Synchronize(serialized as T, onFinished);
        }

        /// <seealso cref="IAnnotation.Deserialize(AnnotationType, Action{bool, string})"/>
        void IAnnotation.Deserialize(AnnotationType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IAnnotation.Serialize(AnnotationType, Action{bool, string})"/>
        void IAnnotation.Serialize(AnnotationType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IAnnotation

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
            AttachTo = AnnotationDefaults.ATTACH_TO;
            StartDelay = AnnotationDefaults.START_DELAY;
            Loop = AnnotationDefaults.LOOP;
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="Identifiable{T}.Deserialize(T, SerializationState)"/>
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
            serializedAnnotation = serialized;

            // Process this object specific deserialization

            // Deserialize start delay
            if (serializedAnnotation.StartDelay != null)
            {
                float startDelay = 0f;
                SchemaUtil.DeserializeDuration(serializedAnnotation.StartDelay, ref startDelay);
                StartDelay = startDelay;
            }

            // Deserialize loop
            Loop = serialized.Loop;

            // Deserialize the "AttachTo"
            AttachTo = serialized.AttachTo;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Identifiable{T}.Serialize(T, SerializationState)"/>
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

            // Serialize the start delay (optional) only if it was defined during deserialization
            DurationType serializedDelay = null;
            if (serializedAnnotation.StartDelay != null)
            {
                serializedDelay = serializedAnnotation.StartDelay;
                SchemaUtil.SerializeDuration(StartDelay, serializedDelay);
            }
            serialized.StartDelay = serializedDelay;

            // Serialize the loop (optional)
            serialized.Loop = Loop;

            // Serialize the attach to
            serialized.AttachTo = _attachTo;

            // Save the final serialized reference
            serializedAnnotation = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedAnnotation>();
        }

        /// <summary>
        /// Called to attach any dependent components for this annotation to the
        /// supplied game object.
        /// </summary>
        /// <param name="toGameObject">The <code>GameObject</code> to attach components</param>
        protected abstract void AttachDependentComponents(GameObject toGameObject);

        /// <summary>
        /// Called to confirm that the annotation can be played. Subclasses should override to
        /// check for assertions.
        /// </summary>
        /// <returns>A boolean indicating the annotation is allowed to play</returns>
        protected abstract bool CanPlay();

        /// <summary>
        /// Performs the playof the annotation
        /// </summary>
        protected abstract void DoPlay();

        /// <summary>
        /// Performs the pausing of the annotation
        /// </summary>
        protected abstract void DoPause();

        /// <summary>
        /// Performs the resuming of the annotation
        /// </summary>
        protected abstract void DoResume();

        /// <summary>
        /// Performs the stop of the annotation
        /// </summary>
        protected abstract void DoStop();
    }

    public class AnnotationDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly string ATTACH_TO = "";
        public static readonly bool LOOP = new AnnotationType().Loop;
        public static readonly float START_DELAY = 0f;
    }
}
