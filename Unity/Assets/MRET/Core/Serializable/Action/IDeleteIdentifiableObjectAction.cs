// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IDeleteIdentifiableObjectAction
	///
	/// A delete identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDeleteIdentifiableObjectAction : IIdentifiableObjectAction
    {
        /// <seealso cref="IIdentifiableObjectAction.CreateSerializedType"/>
        new public DeleteIdentifiableObjectActionType CreateSerializedType();

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IIdentifiableObjectAction.SerializedAction"/>
        new public DeleteIdentifiableObjectActionType SerializedAction { get; }

        /// <seealso cref="IIdentifiableObjectAction.Deserialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Deserialize(DeleteIdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiableObjectAction.Serialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Serialize(DeleteIdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IDeleteIdentifiableObjectAction
	///
	/// A <generic> delete identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IDeleteIdentifiableObjectAction<T, I> : IIdentifiableObjectAction<T, I>, IDeleteIdentifiableObjectAction
        where T : DeleteIdentifiableObjectActionType
        where I : IIdentifiable
    {
    }
}
