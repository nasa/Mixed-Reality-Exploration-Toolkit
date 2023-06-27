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
	/// IAddSceneObjectAction
	///
	/// An add scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAddSceneObjectAction : ISceneObjectTransformAction
	{
        /// <seealso cref="ISceneObjectTransformAction.CreateSerializedType"/>
        new public AddSceneObjectActionType CreateSerializedType();

        /// <summary>
        /// The serialized scene object to instantiate
        /// </summary>
        public SceneObjectType SerializedSceneObject { get; }

        /// <seealso cref="ISceneObjectTransformAction.Deserialize(TransformSceneObjectActionType, Action{bool, string})"/>
        public void Deserialize(AddSceneObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObjectTransformAction.Serialize(TransformSceneObjectActionType, Action{bool, string})"/>
        public void Serialize(AddSceneObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IAddSceneObjectAction
	///
	/// A <generic> add scene object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAddSceneObjectAction<T, C, I> : ISceneObjectTransformAction<T, I>, IAddSceneObjectAction
        where T : AddSceneObjectActionType
        where C : SceneObjectType
        where I : ISceneObject
    {
        /// <seealso cref="IAddSceneObjectAction.SerializedSceneObject"/>
        new public C SerializedSceneObject { get; }
    }
}
