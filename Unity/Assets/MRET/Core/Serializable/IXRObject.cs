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
	/// IXRObject
	///
	/// An XR object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IXRObject : IVersioned
    {
        /// <summary>
        /// Indicates if this <code>IXRObject</code> is enabled for Agumented-Reality (AR).<br>
        /// </summary>
        public bool AREnabled { get; }

        /// <summary>
        /// Indicates if this <code>IXRObject</code> is enabled for Agumented-Reality (AR).<br>
        /// </summary>
        public bool VREnabled { get; }

        /// <summary>
        /// The game object of this <code>IXRObject</code>.<br>
        /// </summary>
        public GameObject gameObject { get; }

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        new public XRType CreateSerializedType();

        /// <seealso cref="IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(XRType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(XRType serialized, Action<bool, string> onFinished = null);

    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IXRObject
	///
	/// A <generic> XR object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IXRObject<T> : IVersioned<T>, IXRObject
        where T : XRType
    {
    }
}
