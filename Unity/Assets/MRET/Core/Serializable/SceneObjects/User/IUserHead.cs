// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUserHead
	///
	/// Defines a user head in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserHead : IUserComponent
    {
        /// <seealso cref="IUserComponent.InputComponent"/>
        new public InputHead InputComponent { get; }

        /// <seealso cref="IUserComponent.Initialize(User, Component)"/>
        public void Initialize(User user, InputHead inputComponent);

        /// <seealso cref="IUserComponent.CreateSerializedType"/>
        new public UserHeadType CreateSerializedType();

        /// <seealso cref="IUserComponent.Deserialize(UserComponentType, Action{bool, string})"/>
        public void Deserialize(UserHeadType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IUserComponent.Serialize(UserComponentType, Action{bool, string})"/>
        public void Serialize(UserHeadType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUser
	///
	/// Defines a <generic> user head in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserHead<T> : IUserComponent<T, InputHead>, IUserHead
        where T : UserHeadType
    {
    }
}
