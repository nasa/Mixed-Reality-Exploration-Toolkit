// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    /// <remarks>
    /// History:
    /// 2 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ITextAnnotation
	///
	/// A text annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ITextAnnotation : IAnnotation
	{
        /// <seealso cref="IAnnotation.CreateSerializedType"/>
        new public TextAnnotationType CreateSerializedType();

        /// <summary>
        /// The number of seconds for each annotation text before advancing to the next text
        /// in the sequence.
        /// </summary>
        public float TimePerText { get; set; }

        /// <summary>
        /// The current text index of the annotation
        /// </summary>
        public int TextIndex{ get; }

        /// <summary>
        /// The ordered list of <code>Text</code> objects in the annotation sequence
        /// </summary>
        public List<string> Texts { get; }

        /// <summary>
        /// The text index to goto
        /// </summary>
        public void Goto(int index);

        /// <seealso cref="IAnnotation.Synchronize(AnnotationType, Action{bool, string})"/>
        public void Synchronize(TextAnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAnnotation.Deserialize(AnnotationType, Action{bool, string})"/>
        public void Deserialize(TextAnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAnnotation.Serialize(AnnotationType, Action{bool, string})"/>
        public void Serialize(TextAnnotationType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ITextAnnotation
	///
	/// A <generic> text annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ITextAnnotation<T> : IAnnotation<T>, ITextAnnotation
        where T : TextAnnotationType
    {
    }

}
