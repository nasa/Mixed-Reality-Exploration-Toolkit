// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Annotation;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class TextAnnotationAction : 
        AnnotationAction<TextAnnotationActionType,ITextAnnotation>,
        ITextAnnotationAction<TextAnnotationActionType,ITextAnnotation>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TextAnnotationAction);

        public readonly int DEFAULT_INDEX = 0;
        public readonly float DEFAULT_TIME_PER_TEXT = 1f;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private TextAnnotationActionType serializedTextAnnotationAction;

        #region ITextAnnotation
        /// <summary>
        /// The text index to goto in the text annotation for this action
        /// </summary>
        public int Index { get; protected set; }

        /// <summary>
        /// The amount of time to display each text in the annotation for this action
        /// </summary>
        public float TimePerText { get; protected set; }
        #endregion ITextAnnotation

        #region Serialization
        /// <seealso cref="AnnotationAction{T,C}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(TextAnnotationActionType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedTextAnnotationAction = serialized;

            // Process this object specific deserialization

            // Deserialize the index
            Index = serialized.TextIndex;
            ActionObject.Goto(Index);

            // Deserialize the time per text
            float timePerText = DEFAULT_TIME_PER_TEXT;
            if (serialized.TimePerText != null)
            {
                SchemaUtil.DeserializeDuration(serialized.TimePerText, ref timePerText);
            }
            TimePerText = timePerText;
            ActionObject.TimePerText = TimePerText;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="AnnotationAction{T,C}.Serialize(T, SerializationState)"/>
        protected override void Serialize(TextAnnotationActionType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the index (optional) only if it is different than the default
            if (Index != DEFAULT_INDEX)
            {
                serialized.TextIndex = Index;
            }

            // Serialize the time per text (optional) only if it was defined during deserialization
            DurationType serializedTimePerText = null;
            if (serializedTextAnnotationAction.TimePerText != null)
            {
                serializedTimePerText = serializedTextAnnotationAction.TimePerText;
                SchemaUtil.SerializeDuration(TimePerText, serializedTimePerText);
            }
            serialized.TimePerText = serializedTimePerText;

            // Save the final serialized reference
            serializedTextAnnotationAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            Index = DEFAULT_INDEX;
            TimePerText = DEFAULT_TIME_PER_TEXT;
        }

        /// <summary>
        /// Constructor for the <codeTextAnnotationAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="AnnotationAction{T}.AnnotationAction(T)"/>
        public TextAnnotationAction(TextAnnotationActionType serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>TextAnnotationAction</code>
        /// </summary>
        /// <param name="annotation">The <code>ITextAnnotation</code> associated with this action</param>
        /// <param name="index">The text index position for this action</param>
        /// <seealso cref="AnnotationAction{T}.AnnotationAction(T)"/>
        public TextAnnotationAction(ITextAnnotation annotation, int index) : base(annotation)
        {
            // Assign the unique settings for this action
            Index = index;
        }

        /// <summary>
        /// Constructor for the <code>TextAnnotationAction</code>
        /// </summary>
        /// <param name="annotation">The <code>ITextAnnotation</code> associated with this action</param>
        /// <param name="timePerText">The amount of time the text is displayed before moving to the next annotation text for this action</param>
        /// <seealso cref="AnnotationAction{T}.AnnotationAction(T)"/>
        public TextAnnotationAction(ITextAnnotation annotation, float timePerText) : base(annotation)
        {
            // Assign the unique settings for this action
            TimePerText = TimePerText;
        }

        /// <summary>
        /// Constructor for the <code>TextAnnotationAction</code>
        /// </summary>
        /// <param name="annotation">The <code>ITextAnnotation</code> associated with this action</param>
        /// <param name="index">The text index position for this action</param>
        /// <param name="timePerText">The amount of time the text is displayed before moving to the next annotation text for this action</param>
        /// <seealso cref="AnnotationAction{T}.AnnotationAction(T)"/>
        public TextAnnotationAction(ITextAnnotation annotation, int index, float timePerText) : base(annotation)
        {
            // Assign the unique settings for this action
            Index = index;
            TimePerText = TimePerText;
        }

    }
}