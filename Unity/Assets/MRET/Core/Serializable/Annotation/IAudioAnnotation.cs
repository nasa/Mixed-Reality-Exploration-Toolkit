// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    /// <remarks>
    /// History:
    /// 22 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IAudioAnnotation
	///
	/// A audio annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAudioAnnotation : ISourceAnnotation
    {
        /// <seealso cref="ISourceAnnotation.CreateSerializedType"/>
        new public AudioAnnotationType CreateSerializedType();

        /// <seealso cref="ISourceAnnotation.Synchronize(SourceAnnotationType, Action{bool, string})"/>
        public void Synchronize(AudioAnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISourceAnnotation.Deserialize(SourceAnnotationType, Action{bool, string})"/>
        public void Deserialize(AudioAnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISourceAnnotation.Serialize(SourceAnnotationType, Action{bool, string})"/>
        public void Serialize(AudioAnnotationType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 22 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IAudioAnnotation
	///
	/// A <generic> audio annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAudioAnnotation<T> : ISourceAnnotation<T>, IAudioAnnotation
        where T : AudioAnnotationType
    {
    }

}