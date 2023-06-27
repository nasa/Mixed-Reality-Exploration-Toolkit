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
	/// IAnnotationAction
	///
	/// An annotation action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAnnotationAction : IIdentifiableObjectAction
    {
        /// <seealso cref="IIdentifiableObjectAction.CreateSerializedType"/>
        new public AnnotationActionType CreateSerializedType();

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        /// <seealso cref="IIdentifiableObjectAction.ActionObject"/>
        new public IAnnotation ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IIdentifiableObjectAction.SerializedAction"/>
        new public AnnotationActionType SerializedAction { get; }

        /// <seealso cref="IIdentifiableObjectAction.Deserialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Deserialize(AnnotationActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiableObjectAction.Serialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Serialize(AnnotationActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IAnnotationAction
	///
	/// A <generic> annotation action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAnnotationAction<T,I> : IIdentifiableObjectAction<T,I>, IAnnotationAction
        where T : AnnotationActionType
        where I : IAnnotation
	{
    }
}
