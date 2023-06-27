// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 19 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISceneObjectGroup
	///
	/// A scene object group object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObjectGroup : IGroup
	{
        /// <seealso cref="IGroup.CreateSerializedType"/>
        new public SceneObjectsType CreateSerializedType();

        /// <seealso cref="IGroup.Deserialize(GroupType, Action{bool, string})"/>
        public void Deserialize(SceneObjectsType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(GroupType, Action{bool, string})"/>
        public void Serialize(SceneObjectsType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 19 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISceneObjectGroup
	///
	/// A <generic> scene object group object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISceneObjectGroup<GT, GI, CT, CI> : IGroup<GT, GI, CT, CI>, ISceneObjectGroup
        where GT : SceneObjectsType
        where GI : ISceneObjectGroup
        where CT : SceneObjectType
        where CI : ISceneObject
    {
    }
}
