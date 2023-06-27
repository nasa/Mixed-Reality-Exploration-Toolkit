// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
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
	/// IInteractableSceneObjectAction
	///
	/// An interactable scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractableSceneObjectAction : ISceneObjectAction
    {
        /// <seealso cref="ISceneObjectAction.CreateSerializedType"/>
        new public InteractableSceneObjectActionType CreateSerializedType();

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        new public IInteractable ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="ISceneObjectAction.SerializedAction"/>
        new public InteractableSceneObjectActionType SerializedAction { get; }

        /// <seealso cref="ISceneObjectAction.Deserialize(SceneObjectActionType, Action{bool, string})"/>
        public void Deserialize(InteractableSceneObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObjectAction.Serialize(SceneObjectActionType, Action{bool, string})"/>
        public void Serialize(InteractableSceneObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IInteractableSceneObjectAction
	///
	/// A <generic> interactable scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractableSceneObjectAction<T,I> : ISceneObjectAction<T,I>, IInteractableSceneObjectAction
        where T : InteractableSceneObjectActionType
        where I : IInteractable
    {
    }
}
