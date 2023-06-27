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
	/// IAction
	///
	/// An action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAction : IVersioned
	{
        /// <seealso cref="IVersioned.CreateSerializedType"/>
        new public ActionType CreateSerializedType();

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="PerformAction"/>
        public ActionType SerializedAction { get; }

        /// <summary>
        /// Called to perform the action
        /// </summary>
        public void PerformAction();

        /// <seealso cref="IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(ActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(ActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IAction
	///
	/// An action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAction<T> : IVersioned<T>, IAction
        where T : ActionType
    {
        /// <seealso cref="IAction.SerializedAction"/>
        new public T SerializedAction { get; }
    }
}
