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
	/// IUserHand
	///
	/// Defines a user hand in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserHand : IUserComponent
    {
        /// <summary>
        /// The handedness associated with this pointer
        /// </summary>
        public InputHand.Handedness Handedness { get; }

        /// <summary>
        /// The controller associated with this hand
        /// </summary>
        public IUserController Controller { get; }

        /// <summary>
        /// The pointer associated with this hand
        /// </summary>
        public IUserPointer Pointer { get; }

        /// <seealso cref="IUserComponent.InputComponent"/>
        new public InputHand InputComponent { get; }

        /// <seealso cref="IUserComponent.Initialize(User, Component)"/>
        public void Initialize(User user, InputHand inputComponent);

        /// <summary>
        /// Performs a synchronization of a user hand controller. This is only applicable for
        /// remote users.
        /// </summary>
        /// <param name="serializedController">The serialized <code>UserControllerType</code>
        ///     defining the current state of the hand controller</param>
        public void SynchronizeController(UserControllerType serializedController);

        /// <summary>
        /// Performs a synchronization of a user hand pointer. This is only applicable for
        /// remote users.
        /// </summary>
        /// <param name="serializedController">The serialized <code>UserPointerType</code>
        ///     defining the current state of the hand pointer</param>
        public void SynchronizePointer(UserPointerType serializedPointer);

        /// <seealso cref="IUserComponent.CreateSerializedType"/>
        new public UserHandType CreateSerializedType();

        /// <seealso cref="IUserComponent.Deserialize(UserComponentType, Action{bool, string})"/>
        public void Deserialize(UserHandType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IUserComponent.Serialize(UserComponentType, Action{bool, string})"/>
        public void Serialize(UserHandType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUser
	///
	/// Defines a <generic> user hand in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserHand<T> : IUserComponent<T, InputHand>, IUserHand
        where T : UserHandType
    {
    }
}
