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
	/// IIdentifiableObjectUpdateAction
	///
	/// An update identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IIdentifiableObjectUpdateAction : IIdentifiableObjectAction
    {
        /// <seealso cref="IIdentifiableObjectAction.CreateSerializedType"/>
        new public UpdateIdentifiableObjectActionType CreateSerializedType();

        /// <summary>
        /// The list of identifiable object attribute names being updated
        /// </summary>
        public string[] Attributes { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IIdentifiableObjectAction.SerializedAction"/>
        new public UpdateIdentifiableObjectActionType SerializedAction { get; }

        /// <seealso cref="IIdentifiableObjectAction.Deserialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Deserialize(UpdateIdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiableObjectAction.Serialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Serialize(UpdateIdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IIdentifiableObjectUpdateAction
	///
	/// A <generic> update identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IIdentifiableObjectUpdateAction<T, I> : IIdentifiableObjectAction<T, I>, IIdentifiableObjectUpdateAction
        where T : UpdateIdentifiableObjectActionType
        where I : IIdentifiable
    {
    }
}
