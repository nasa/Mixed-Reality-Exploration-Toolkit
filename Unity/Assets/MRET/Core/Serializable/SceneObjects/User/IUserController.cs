// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUserController
	///
	/// Defines a user controller in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserController : IUserHandComponent
    {
        /// <seealso cref="IUserHandComponent.CreateSerializedType"/>
        new public UserControllerType CreateSerializedType();

        /// <seealso cref="IUserHandComponent.Deserialize(SceneObjectType, Action{bool, string})"/>
        public void Deserialize(UserControllerType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IUserHandComponent.Serialize(SceneObjectType, Action{bool, string})"/>
        public void Serialize(UserControllerType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUser
	///
	/// Defines a <generic> user controller in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserController<T> : IUserHandComponent<T>, IUserController
        where T : UserControllerType
    {
    }
}
