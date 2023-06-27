// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISceneObjectAction
	///
	/// A scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObjectTransformAction : ISceneObjectAction
	{
        /// <seealso cref="ISceneObjectAction.CreateSerializedType"/>
        new public TransformSceneObjectActionType CreateSerializedType();

        /// <summary>
        /// The position associated with this transform action
        /// </summary>
        /// <see cref="Vector3"/>
        public Vector3 Position { get; }

        /// <summary>
        /// The rotation associated with this transform action
        /// </summary>
        /// <see cref="Quaternion"/>
        public Quaternion Rotation { get; }

        /// <summary>
        /// The scale associated with this transform action
        /// </summary>
        /// <see cref="Vector3"/>
        public Vector3 Scale { get; }

        /// <summary>
        /// The reference space for this transform action
        /// </summary>
        /// <see cref="ReferenceSpaceType"/>
        public ReferenceSpaceType Reference { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="ISceneObjectAction.SerializedAction"/>
        new public TransformSceneObjectActionType SerializedAction { get; }

        /// <seealso cref="ISceneObjectAction.Deserialize(SceneObjectActionType, System.Action{bool, string})"/>
        public void Deserialize(TransformSceneObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObjectAction.Serialize(SceneObjectActionType, System.Action{bool, string})"/>
        public void Serialize(TransformSceneObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISceneObjectAction
	///
	/// A scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObjectTransformAction<T,I> : ISceneObjectAction<T,I>, ISceneObjectTransformAction
        where T : TransformSceneObjectActionType
        where I : ISceneObject
    {
    }
}
