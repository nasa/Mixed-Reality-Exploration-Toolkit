// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 1 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IPhysicalSceneObjectAction
	///
	/// A physical scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IPhysicalSceneObjectAction : IInteractableSceneObjectAction
    {
        /// <seealso cref="IInteractableSceneObjectAction.CreateSerializedType"/>
        new public PhysicalSceneObjectActionType CreateSerializedType();

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        new public IPhysicalSceneObject ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IInteractableSceneObjectAction.SerializedAction"/>
        new public PhysicalSceneObjectActionType SerializedAction { get; }

        /// <seealso cref="IInteractableSceneObjectAction.Deserialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        public void Deserialize(PhysicalSceneObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractableSceneObjectAction.Serialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        public void Serialize(PhysicalSceneObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IPhysicalSceneObjectAction
	///
	/// A <generic> physical scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IPhysicalSceneObjectAction<T,I> : IInteractableSceneObjectAction<T,I>, IPhysicalSceneObjectAction
        where T : PhysicalSceneObjectActionType
        where I : IPhysicalSceneObject
	{
    }
}
