// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IGroup
	///
	/// A group object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IGroup : IVersioned
	{
        /// <summary>
        /// The group identifier.<br>
        /// </summary>
        public int group { get; }

        /// <summary>
        /// The game object of this <code>IXRObject</code>.<br>
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        /// The children in this group.
        /// </summary>
        public IIdentifiable[] Children { get; }

        /// <summary>
        /// The child groups of this group.
        /// </summary>
        public IGroup[] ChildGroups { get; }

        /// <summary>
        /// Destroys the group and all the children
        /// </summary>
        /// <param name="destroyGameObject">Indicates whether or not the underlying
        ///     <code>GameObject</code> should be destroyed</param>
        public void DestroyGroup(bool destroyGameObject = false);

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        new public GroupType CreateSerializedType();

        /// <seealso cref="IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(GroupType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(GroupType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IGroup
	///
	/// A <generic> group object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IGroup<GT, GI, CT, CI> : IVersioned<GT>, IGroup
        where GT : GroupType
        where GI : IGroup
        where CT : IdentifiableType
        where CI : IIdentifiable
    {
        /// <seealso cref="IGroup.Children"/>
        new public CI[] Children { get; }

        /// <seealso cref="IGroup.ChildGroups"/>
        new public GI[] ChildGroups { get; }
    }
}
