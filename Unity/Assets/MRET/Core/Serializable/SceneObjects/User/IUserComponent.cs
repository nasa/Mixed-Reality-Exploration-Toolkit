// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUserComponent
	///
	/// Defines a user component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserComponent : ISceneObject
    {
        /// <summary>
        /// Indicated is this user is local or remote
        /// </summary>
        public bool IsLocal { get; }

        /// <summary>
        /// The user associated with this component
        /// </summary>
        public IUser User { get; }

        /// <summary>
        /// The input component game object associated with this component, i.e. InputHead,
        /// Inputhand, etc.
        /// </summary>
        public Component InputComponent { get; }

        /// <summary>
        /// Called to initialize this component with the user and input component
        /// </summary>
        /// <param name="user">The <code>User</code> associated with this user component</param>
        /// <param name="inputComponent">The <code>Component</code> associated with this user component</param>
        public void Initialize(User user, Component inputComponent);

        /// <seealso cref="ISceneObject.CreateSerializedType"/>
        new public UserComponentType CreateSerializedType();

        /// <seealso cref="ISceneObject.Deserialize(SceneObjectType, Action{bool, string})"/>
        public void Deserialize(UserComponentType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObject.Serialize(SceneObjectType, Action{bool, string})"/>
        public void Serialize(UserComponentType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUser
	///
	/// Defines a <generic> user component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserComponent<T, C> : ISceneObject<T>, IUserComponent
        where T : UserComponentType
        where C : Component
    {
        /// <seealso cref="IUserComponent.InputComponent"/>
        new public C InputComponent { get; }

        /// <seealso cref="IUserComponent.Initialize(User, Component)"/>
        public void Initialize(User user, C inputComponent);
    }
}
