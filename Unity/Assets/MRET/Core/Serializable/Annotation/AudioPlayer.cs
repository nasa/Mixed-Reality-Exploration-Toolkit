// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    public class AudioPlayer : SourceAnnotation<AudioAnnotationType>, IAudioAnnotation<AudioAnnotationType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AudioPlayer);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private AudioAnnotationType serializedAudioAnnotation;

        private AudioSource audioSource;

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

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            if (audioSource)
            {
                if (IsPlaying && !audioSource.isPlaying)
                {
                    audioSource.time = StartTime;
                    audioSource.PlayDelayed(StartDelay);
                    audioSource.loop = Loop;
                }

                if (audioSource.isPlaying)
                {
                    if (audioSource.time < StartTime)
                    {
                        audioSource.time = StartTime;
                    }
                    else if (audioSource.time > StartTime + Duration)
                    {
                        if (Loop) audioSource.time = StartTime;
                        else audioSource.Stop();
                    }
                }
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="SourceAnnotation{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(AudioAnnotationType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedAudioAnnotation = serialized;

            // Process this object specific deserialization

            // Deserialize (nothing unique from the base class yet)

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="SourceAnnotation{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(AudioAnnotationType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize (nothing unique from the base class yet)

            // Save the final serialized reference
            serializedAudioAnnotation = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        #region Annotation
        /// <seealso cref="Annotation{T}.AttachDependentComponents(GameObject)"/>
        protected override void AttachDependentComponents(GameObject toGameObject)
        {
            audioSource = toGameObject.GetComponent<AudioSource>();
            if (!audioSource)
            {
                audioSource = toGameObject.AddComponent<AudioSource>();
            }
        }
        #endregion Annotation

        #region Annotation
        /// <seealso cref="Annotation{T}.CanPlay"/>
        protected override bool CanPlay()
        {
            return (audioSource != null);
        }

        /// <seealso cref="Annotation{T}.DoPlay"/>
        protected override void DoPlay()
        {
            // Do nothing
        }

        /// <seealso cref="Annotation{T}.DoPause"/>
        protected override void DoPause()
        {
            audioSource.Pause();
        }

        /// <seealso cref="Annotation{T}.DoResume"/>
        protected override void DoResume()
        {
            audioSource.UnPause();
        }

        /// <seealso cref="Annotation{T}.DoStop"/>
        protected override void DoStop()
        {
            audioSource.Stop();
        }

        /// <seealso cref="SourceAnnotation{T}.InitializeSource(ObjectSerializationState{object})"/>
        protected override IEnumerator InitializeSource(ObjectSerializationState<object> sourceDeserializationState)
        {
            // Attempt to process the source
            object source = sourceDeserializationState.obj;
            if ((source is Uri) || (source is string))
            {
                // Convert the source to a path
                string sourcePath = "";
                if (source is Uri)
                {
                    sourcePath = (source as Uri).AbsoluteUri;
                }
                else
                {
                    sourcePath = source as string;
                }

                // FIXME: Assign the correct audio type from the serialized input
                UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(sourcePath, AudioType.UNKNOWN);

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    sourceDeserializationState.Error(www.error);
                    yield break;
                }

                // Assign the clip
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
            }
            else if (source is GameObject)
            {
                sourceDeserializationState.Error("Audio source 'GameObject' not implemented.");
                yield break;
            }
            else
            {
                sourceDeserializationState.Error("Audio source type unknown.");
                yield break;
            }

            // Mark as complete
            sourceDeserializationState.complete = true;

            yield return null;
        }
        #endregion SourceAnnotation

    }
}