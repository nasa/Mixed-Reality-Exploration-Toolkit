// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IEnvironmentScaleAction
	///
	/// An environment scale action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IEnvironmentScaleAction : IAction
    {
        /// <seealso cref="IAction.CreateSerializedType"/>
        new public EnvironmentScaleActionType CreateSerializedType();

        /// <summary>
        /// The scale in world space
        /// </summary>
        public Vector3 Scale { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IAction.SerializedAction"/>
        new public EnvironmentScaleActionType SerializedAction { get; }

        /// <seealso cref="IAction.Deserialize(ActionType, Action{bool, string})"/>
        public void Deserialize(EnvironmentScaleActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAction.Serialize(ActionType, Action{bool, string})"/>
        public void Serialize(EnvironmentScaleActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IEnvironmentScaleAction
	///
	/// A <generic> environment scale action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IEnvironmentScaleAction<T> : IAction<T>, IEnvironmentScaleAction
        where T : EnvironmentScaleActionType
    {
    }
}
