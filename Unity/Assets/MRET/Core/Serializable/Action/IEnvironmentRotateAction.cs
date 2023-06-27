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
	/// IEnvironmentRotateAction
	///
	/// An environment rotate action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IEnvironmentRotateAction : IAction
	{
        /// <seealso cref="IAction.CreateSerializedType"/>
        new public EnvironmentRotateActionType CreateSerializedType();

        /// <summary>
        /// The rotation axis
        /// </summary>
        public AxisType Axis { get; }

        /// <summary>
        /// The rotation reference point in world space
        /// </summary>
        public Vector3 ReferencePoint { get; }

        /// <summary>
        /// The degree value to rotate
        /// </summary>
        public float Degrees { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IAction.SerializedAction"/>
        new public EnvironmentRotateActionType SerializedAction { get; }

        /// <seealso cref="IAction.Deserialize(ActionType, Action{bool, string})"/>
        public void Deserialize(EnvironmentRotateActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IAction.Serialize(ActionType, Action{bool, string})"/>
        public void Serialize(EnvironmentRotateActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IEnvironmentRotateAction
	///
	/// A <generic> environment rotate action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IEnvironmentRotateAction<T> : IAction<T>, IEnvironmentRotateAction
        where T : EnvironmentRotateActionType
    {
    }
}
