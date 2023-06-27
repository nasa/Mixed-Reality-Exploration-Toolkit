// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISceneObject
	///
	/// An scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObject : IIdentifiable
    {
        /// <summary>
        /// The parent <code>ISceneObject</code> of this <code>ISceneObject</code>.<br>
        /// </summary>
        public ISceneObject parent { get; }

        /// <summary>
        /// The transform of this <code>ISceneObject</code>.<br>
        /// </summary>
        public Transform transform { get; }

        /// <seealso cref="IIdentifiable.CreateSerializedType"/>
        new public SceneObjectType CreateSerializedType();

        /// <seealso cref="IIdentifiable.Synchronize(IdentifiableType, Action{bool, string})"/>
        public void Synchronize(SceneObjectType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiable.Deserialize(IdentifiableType, Action{bool, string})"/>
        public void Deserialize(SceneObjectType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiable.Serialize(IdentifiableType, Action{bool, string})"/>
        public void Serialize(SceneObjectType serialized, Action<bool, string> onFinished = null);

    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISceneObject
	///
	/// A <generic> scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObject<T> : IIdentifiable<T>, ISceneObject
        where T : SceneObjectType
    {
    }
}
