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
	/// ISourceAnnotation
	///
	/// An source annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISourceAnnotation : IAnnotation
	{
        /// <seealso cref="IAnnotation.CreateSerializedType"/>
        new public SourceAnnotationType CreateSerializedType();

        /// <summary>
        /// The starting time for the source.<br>
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// The duration of the source. A value less than or equal to 0 or NaN
        /// represents the full source duration.<br>
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// The speed of the source, i.e. 0.5 is half speed<br>
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// The source<br>
        /// </summary>
        public object Source { get; }

        /// <seealso cref="IAnnotation.Synchronize(AnnotationType, Action{bool, string})"/>
        public void Synchronize(SourceAnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAnnotation.Deserialize(AnnotationType, Action{bool, string})"/>
        public void Deserialize(SourceAnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAnnotation.Serialize(AnnotationType, Action{bool, string})"/>
        public void Serialize(SourceAnnotationType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISourceAnnotation
	///
	/// A <generic> source annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISourceAnnotation<T> : IAnnotation<T>, ISourceAnnotation
        where T : SourceAnnotationType
    {
    }

}
