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
	/// ISceneObjectAction
	///
	/// A scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObjectAction : IIdentifiableObjectAction
    {
        /// <seealso cref="IIdentifiableObjectAction.CreateSerializedType"/>
        new public SceneObjectActionType CreateSerializedType();

        /// <summary>
        /// The scene object parent identifier associated with the action.<br>
        /// </summary>
        public string ParentID { get; }

        /// <summary>
        /// The scene object parent associated with the action.<br>
        /// </summary>
        public ISceneObject Parent { get; }

        /// <summary>
        /// The object associated with the action.<br>
        /// </summary>
        /// <seealso cref="IIdentifiableObjectAction.ActionObject"/>
        new public ISceneObject ActionObject { get; }

        /// <summary>
        /// The serialized action that will be deserialized by the
        /// <code>SerializableAction</code> class when the action is performed
        /// </summary>
        /// <seealso cref="IIdentifiableObjectAction.SerializedAction"/>
        new public SceneObjectActionType SerializedAction { get; }

        /// <seealso cref="IIdentifiableObjectAction.Deserialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Deserialize(SceneObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiableObjectAction.Serialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Serialize(SceneObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISceneObjectAction
	///
	/// A <generic> scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObjectAction<T,I> : IIdentifiableObjectAction<T,I>, ISceneObjectAction
        where T : SceneObjectActionType
        where I : ISceneObject
    {
    }
}
