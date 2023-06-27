// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// IIdentifiableObjectAction
    ///
    /// An identifiable object action in MRET
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public interface IIdentifiableObjectAction : IAction
    {
        /// <seealso cref="IAction.CreateSerializedType"/>
        new public IdentifiableObjectActionType CreateSerializedType();

        /// <summary>
        /// The object identifier associated with the action.<br>
        /// </summary>
        public string ActionObjectID { get; }

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        /// <seealso cref="IIdentifiable"/>
        public IIdentifiable ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IAction.SerializedAction"/>
        new public IdentifiableObjectActionType SerializedAction { get; }

        /// <seealso cref="IAction.Deserialize(ActionType, Action{bool, string})"/>
        public void Deserialize(IdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAction.Serialize(ActionType, Action{bool, string})"/>
        public void Serialize(IdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IIdentifiableObjectAction
	///
	/// A <generic> identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IIdentifiableObjectAction<T,I> : IAction<T>, IIdentifiableObjectAction
        where T : IdentifiableObjectActionType
        where I : IIdentifiable
    {
        /// <seealso cref="IIdentifiableObjectAction.ActionObject"/>
        new public I ActionObject { get; }

    }
}
