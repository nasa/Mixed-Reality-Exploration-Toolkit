// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    public class TextAnnotationPlayer : Annotation<TextAnnotationType>, ITextAnnotation<TextAnnotationType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TextAnnotationPlayer);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private TextAnnotationType serializedTextAnnotation;

        #region ITextAnnotation
        /// <seealso cref="ITextAnnotation.TimePerText"/>
        public float TimePerText { get; set; }

        /// <seealso cref="ITextAnnotation.TextIndex"/>
        public int TextIndex { get; private set; }

        /// <seealso cref="ITextAnnotation.Texts"/>
        public List<string> Texts { get; private set; }
        #endregion ITextAnnotation

        /// <summary>
        /// <code>Text</code> object used for the annotation
        /// </summary>
        public Text textObject;

        private bool started = false;
        private System.Diagnostics.Stopwatch ticker = new System.Diagnostics.Stopwatch();

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

            // Create the Text list
            Texts = new List<string>();

            // Set the defaults
            TimePerText = TextAnnotationDefaults.TIME_PER_TEXT;
            TextIndex = TextAnnotationDefaults.TEXT_INDEX;
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (IsPlaying && textObject)
            {
                if (!IsPaused)
                {
                    if (TimeToSwitchText())
                    {
                        textObject.text = Texts[TextIndex++];

                        if (TextIndex >= Texts.Count)
                        {
                            TextIndex = 0;

                            if (!Loop)
                            {
                                Stop();
                            }
                        }
                    }
                }
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="Annotation{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(TextAnnotationType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedTextAnnotation = serialized;

            // Process this object specific deserialization

            // Deserialize time per text (optional)
            if (serializedTextAnnotation.TimePerText != null)
            {
                float timePerText = TextAnnotationDefaults.TIME_PER_TEXT;
                SchemaUtil.DeserializeDuration(serializedTextAnnotation.TimePerText, ref timePerText);
                TimePerText = timePerText;
            }

            // Deserialize the text objects
            Texts.Clear();
            if ((serializedTextAnnotation.Texts != null) &&
                (serializedTextAnnotation.Texts.Items != null))
            {
                foreach (string text in serializedTextAnnotation.Texts.Items)
                {
                    Texts.Add(text);
                }
            }

            // Deserialize text index
            int serializedTextIndex = serializedTextAnnotation.TextIndex;
            TextIndex = (serializedTextIndex >= Texts.Count) ? Texts.Count - 1 : serializedTextIndex;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Annotation{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(TextAnnotationType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the time per text (optional) only if it was defined during deserialization
            DurationType serializedTimePerText = null;
            if (serializedTextAnnotation.TimePerText != null)
            {
                serializedTimePerText = serializedTextAnnotation.TimePerText;
                SchemaUtil.SerializeDuration(TimePerText, serializedTimePerText);
            }
            serialized.TimePerText = serializedTimePerText;

            // Serialize the text strings (optional)
            serialized.Texts = null;
            if (Texts.Count > 0)
            {
                // Generate a new serialized string array
                serialized.Texts = new TextsType
                {
                    Items = Texts.ToArray()
                };
            }

            // Serialize text index
            serializedTextAnnotation.TextIndex = TextIndex;

            // Save the final serialized reference
            serializedTextAnnotation = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        #region Annotation
        /// <seealso cref="Annotation{T}.AttachDependentComponents(GameObject)"/>
        protected override void AttachDependentComponents(GameObject toGameObject)
        {
            textObject = toGameObject.GetComponent<Text>();
            if (!textObject)
            {
                textObject = toGameObject.AddComponent<Text>();
            }
        }

        /// <seealso cref="Annotation{T}.CanPlay"/>
        protected override bool CanPlay()
        {
            return (textObject != null);
        }

        /// <seealso cref="Annotation{T}.DoPlay"/>
        protected override void DoPlay()
        {
            ticker.Start();
        }

        /// <seealso cref="Annotation{T}.DoPause"/>
        protected override void DoPause()
        {
            ticker.Stop();
        }

        /// <seealso cref="Annotation{T}.DoResume"/>
        protected override void DoResume()
        {
            ticker.Start();
        }

        /// <seealso cref="Annotation{T}.DoStop"/>
        protected override void DoStop()
        {
            started = false;
            TextIndex = 0;
            ticker.Reset();
        }
        #endregion Annotation

        #region ITextAnnotation
        public void Goto(int index)
        {
            if (IsPlaying && index < Texts.Count)
            {
                TextIndex = index;
            }
        }
        #endregion ITextAnnotation

        bool TimeToSwitchText()
        {
            if (!started)
            {
                started = true;
                return true;
            }

            if (ticker.ElapsedMilliseconds / 1000 >= TimePerText)
            {
                ticker.Reset();
                ticker.Start();
                return true;
            }
            return false;
        }
    }

    public class TextAnnotationDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly float TIME_PER_TEXT = 1f;
        public static readonly int TEXT_INDEX = new TextAnnotationType().TextIndex;
    }
}