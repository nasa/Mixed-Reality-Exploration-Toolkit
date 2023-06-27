// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    /// <remarks>
    /// History:
    /// 2 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IAnnotation
	///
	/// An annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAnnotation : IIdentifiable
	{
        /// <seealso cref="IXRObject.CreateSerializedType"/>
        new public AnnotationType CreateSerializedType();

        /// <summary>
        /// Indicates if the annotation is playing
        /// </summary>
        public bool IsPlaying { get; }

        /// <summary>
        /// Indicates if the annotation is paused
        /// </summary>
        public bool IsPaused { get; }

        /// <summary>
        /// The start delay for the annotation in seconds<br>
        /// </summary>
        public float StartDelay { get; }

        /// <summary>
        /// Indicates if the annotation is looping<br>
        /// </summary>
        public bool Loop { get; }

        /// <summary>
        /// The parent ID of the annotation <br>
        /// </summary>
        public string AttachTo { get; }

        /// <summary>
        /// Plays the annotation
        /// </summary>
        public void Play();

        /// <summary>
        /// Pauses the annotation
        /// </summary>
        public void Pause();

        /// <summary>
        /// Resumes the annotation
        /// </summary>
        public void Resume();

        /// <summary>
        /// Stops the annotation
        /// </summary>
        public void Stop();

        /// <seealso cref="IIdentifiable.Synchronize(IdentifiableType, Action{bool, string})"/>
        public void Synchronize(AnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiable.Deserialize(IdentifiableType, Action{bool, string})"/>
        public void Deserialize(AnnotationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiable.Serialize(IdentifiableType, Action{bool, string})"/>
        public void Serialize(AnnotationType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IAnnotation
	///
	/// A <generic> annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAnnotation<T> : IIdentifiable<T>, IAnnotation
        where T : AnnotationType
    {
    }

}
