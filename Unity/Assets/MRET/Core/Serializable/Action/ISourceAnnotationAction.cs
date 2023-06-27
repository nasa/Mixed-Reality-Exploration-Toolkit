// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Annotation;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISourceAnnotationAction
	///
	/// A source annotation action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISourceAnnotationAction : IAnnotationAction
    {
        /// <seealso cref="IAnnotationAction.CreateSerializedType"/>
        new public SourceAnnotationActionType CreateSerializedType();

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        new public ISourceAnnotation ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IAnnotationAction.SerializedAction"/>
        new public SourceAnnotationActionType SerializedAction { get; }

        /// <seealso cref="IAnnotationAction.Deserialize(AnnotationActionType, Action{bool, string})"/>
        public void Deserialize(SourceAnnotationActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAnnotationAction.Serialize(AnnotationActionType, Action{bool, string})"/>
        public void Serialize(SourceAnnotationActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISourceAnnotationAction
	///
	/// A source annotation action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISourceAnnotationAction<T,I> : IAnnotationAction<T,I>, ISourceAnnotationAction
        where T : SourceAnnotationActionType
        where I : ISourceAnnotation
    {
    }
}
