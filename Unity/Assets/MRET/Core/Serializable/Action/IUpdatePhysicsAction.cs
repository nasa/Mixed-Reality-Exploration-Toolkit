// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IUpdatePhysicsAction
	///
	/// An update physics action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface IUpdatePhysicsAction : IPhysicalSceneObjectAction
	{
        /// <seealso cref="IPhysicalSceneObjectAction.CreateSerializedType"/>
        new public UpdatePhysicsActionType CreateSerializedType();

        /// <summary>
        /// The mass of the physical scene object for this action
        /// </summary>
        public float mass { get; }

        /// <summary>
        /// Whether or not collisions are enabled for the physical scene object for this action
        /// </summary>
        public bool EnableCollisions { get; }

        /// <summary>
        /// Whether or not gravity is enabled for the physical scene object for this action
        /// </summary>
        public bool EnableGravity { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IPhysicalSceneObjectAction.SerializedAction"/>
        new public UpdatePhysicsActionType SerializedAction { get; }

        /// <seealso cref="IPhysicalSceneObjectAction.Deserialize(PhysicalSceneObjectActionType, Action{bool, string})"/>
        public void Deserialize(UpdatePhysicsActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IPhysicalSceneObjectAction.Serialize(PhysicalSceneObjectActionType, Action{bool, string})"/>
        public void Serialize(UpdatePhysicsActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IUpdatePhysicsAction
	///
	/// A <generic> update physics action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUpdatePhysicsAction<T, I> : IPhysicalSceneObjectAction<T, I>, IUpdatePhysicsAction
        where T : UpdatePhysicsActionType
        where I : IPhysicalSceneObject
    {
    }
}
