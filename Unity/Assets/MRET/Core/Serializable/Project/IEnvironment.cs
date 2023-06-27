// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 23 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IEnvironment
	///
	/// The projject environment in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IEnvironment : IXRObject
    {
        /// <seealso cref="IXRObject.CreateSerializedType"/>
        new public EnvironmentType CreateSerializedType();

        /// <seealso cref="IXRObject.Deserialize(XRType, Action{bool, string})"/>
        public void Deserialize(EnvironmentType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IXRObject.Serialize(XRType, Action{bool, string})"/>
        public void Serialize(EnvironmentType serialized, Action<bool, string> onFinished = null);

    }

    /// <remarks>
    /// History:
    /// 23 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IEnvironment
	///
	/// The <generic> project environment in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IEnvironment<T> : IXRObject<T>, IEnvironment
        where T : EnvironmentType
    {
    }

}
