// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUser
	///
	/// Defines a user in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUser : ISceneObject
    {
        /// <summary>
        /// Indicated is this user is local or remote
        /// </summary>
        public bool IsLocal { get; }

        /// <summary>
        /// The user alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The user color
        /// </summary>
        public Color32 Color { get; set; }

        /// <summary>
        /// The label for the alias
        /// </summary>
        public Text AliasLabel { get; }

        /// <summary>
        /// The user's serializable head
        /// </summary>
        public IUserHead Head { get; }

        /// <summary>
        /// The user's serializable torso
        /// </summary>
        public IUserTorso Torso { get; }

        /// <summary>
        /// The user's serializable hands
        /// </summary>
        public IUserHand[] Hands { get; }

        /// <summary>
        /// Called to initialize this user
        /// </summary>
        /// <param name="isLocal">Indicates whether or not this user is local (vs remote)</param>
        /// <param name="inputRig">The <code>InputRig</code> to use for the user</param>
        public void Initialize(bool isLocal, InputRig inputRig);

        /// <summary>
        /// Performs a synchronization of the user head. This is only applicable for remote users.
        /// </summary>
        /// <param name="serializedHead">The serialized <code>UserHeadType</code> defining the
        ///     current state of the head</param>
        public void SynchronizeHead(UserHeadType serializedHead);

        /// <summary>
        /// Performs a synchronization of the user torso. This is only applicable for remote users.
        /// </summary>
        /// <param name="serializedTorso">The serialized <code>UserTorsoType</code> defining the
        ///     current state of the torso</param>
        public void SynchronizeTorso(UserTorsoType serializedTorso);

        /// <summary>
        /// Performs a synchronization of a user hand. This is only applicable for remote users.
        /// </summary>
        /// <param name="serializedHand">The serialized <code>UserHandType</code> defining the
        ///     current state of the hand</param>
        public void SynchronizeHand(UserHandType serializedHand);

        /// <seealso cref="ISceneObject.CreateSerializedType"/>
        new public UserType CreateSerializedType();

        /// <seealso cref="ISceneObject.Deserialize(SceneObjectType, Action{bool, string})"/>
        public void Deserialize(UserType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObject.Serialize(SceneObjectType, Action{bool, string})"/>
        public void Serialize(UserType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUser
	///
	/// Defines a <generic> user in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUser<T> : ISceneObject<T>, IUser
        where T : UserType
    {
    }
}
