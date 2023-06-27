// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IUserPointer
	///
	/// Defines a user hand component pointer in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUserPointer : IUserHandComponent
    {
        /// <summary>
        /// The line renderer for the pointer
        /// </summary>
        public LineRenderer LineRenderer { get; }

        /// <summary>
        /// The end point of the pointer
        /// </summary>
        public Vector3 EndPoint { get; set; }

        /// <seealso cref="IUserHandComponent.CreateSerializedType"/>
        new public UserPointerType CreateSerializedType();

        /// <seealso cref="IUserHandComponent.Deserialize(UserHandComponentType, Action{bool, string})"/>
        public void Deserialize(UserPointerType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IUserHandComponent.Serialize(UserHandComponentType, Action{bool, string})"/>
        public void Serialize(UserPointerType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
    /// 
    /// <summary>
    /// IUser
    ///
    /// Defines a <generic> user hand component pointer in MRET
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public interface IUserPointer<T> : IUserHandComponent<T>, IUserPointer
        where T : UserPointerType
    {
    }
}
