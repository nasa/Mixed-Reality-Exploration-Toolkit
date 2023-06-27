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
	/// IUpdateInteractionsAction
	///
	/// An update interactions action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUpdateInteractionsAction : IInteractableSceneObjectAction
	{
        /// <seealso cref="IInteractableSceneObjectAction.CreateSerializedType"/>
        new public UpdateInteractionsActionType CreateSerializedType();

        /// <summary>
        /// Whether or not interaction is enabled for the interactable scene object for this action
        /// </summary>
        public bool EnableInteraction { get; }

        /// <summary>
        /// Whether or not usability is enabled for the interactable scene object for this action
        /// </summary>
        public bool EnableUsability { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IInteractableSceneObjectAction.SerializedAction"/>
        new public UpdateInteractionsActionType SerializedAction { get; }

        /// <seealso cref="IInteractableSceneObjectAction.Deserialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        public void Deserialize(UpdateInteractionsActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractableSceneObjectAction.Serialize(InteractableSceneObjectActionType, Action{bool, string})"/>
        public void Serialize(UpdateInteractionsActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IInteractableSceneObjectAction
	///
	/// A <generic> update interactions action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUpdateInteractionsAction<T, I> : IInteractableSceneObjectAction<T, I>, IUpdateInteractionsAction
        where T : UpdateInteractionsActionType
        where I : IInteractable
    {
    }
}
