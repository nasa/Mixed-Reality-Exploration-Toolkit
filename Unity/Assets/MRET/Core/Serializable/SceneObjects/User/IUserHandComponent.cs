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
	/// IUserHandComponent
	///
	/// Defines a user hand component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserHandComponent : IUserComponent
    {
        /// <seealso cref="IUserComponent.InputComponent"/>
        new public InputHand InputComponent { get; }

        /// <summary>
        /// The user hand associated with this hand component
        /// </summary>
        public IUserHand Hand { get; }

        /// <summary>
        /// Called to initialize this component with the user, hand and input component
        /// </summary>
        /// <param name="user">The <code>User</code> associated with this user hand component</param>
        /// <param name="hand">The <code>UserHand</code> associated with this user hand component</param>
        public void Initialize(User user, UserHand hand);

        /// <seealso cref="ISceneObject.CreateSerializedType"/>
        new public UserHandComponentType CreateSerializedType();

        /// <seealso cref="IUserComponent.Deserialize(UserComponentType, Action{bool, string})"/>
        public void Deserialize(UserHandComponentType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IUserComponent.Serialize(UserComponentType, Action{bool, string})"/>
        public void Serialize(UserHandComponentType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUserHandComponentType
	///
	/// Defines a <generic> user hand component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserHandComponent<T> : IUserComponent<T, InputHand>, IUserHandComponent
        where T : UserHandComponentType
    {
    }
}
