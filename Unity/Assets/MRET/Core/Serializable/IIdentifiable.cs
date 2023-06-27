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
	/// IIdentifiable
	///
	/// An identifiable object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IIdentifiable : IXRObject
	{
        /// <seealso cref="IXRObject.CreateSerializedType"/>
        new public IdentifiableType CreateSerializedType();

        /// <summary>
        /// The UUID for this <code>IIdentifiable</code>.<br>
        /// </summary>
        /// <see cref="Guid"/>
        public Guid uuid { get; set; }

        /// <summary>
        /// The "short" human readable unique identifier for this <code>IIdentifiable</code>.<br>
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The "long" human readable name of this <code>IIdentifiable</code>.<br>
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The description of this <code>IIdentifiable</code>.<br>
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Indicates if synchronization is enabled
        /// </summary>
        public bool SynchronizationEnabled { get; set; }

        /// <summary>
        /// Performs collaboration synchronization of this identifiable with the supplied serialized object.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to synchronize with this object instance</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the deserialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        /// 
        /// <see cref="System.Action"/>
        /// <see cref="IdentifiableType"/>
        public void Synchronize(IdentifiableType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IXRObject.Deserialize(XRType, Action{bool, string})"/>
        public void Deserialize(IdentifiableType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IXRObject.Serialize(XRType, Action{bool, string})"/>
        public void Serialize(IdentifiableType serialized, Action<bool, string> onFinished = null);

    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IIdentifiable
	///
	/// A <generic> identifiable object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IIdentifiable<T> : IXRObject<T>, IIdentifiable
        where T : IdentifiableType
    {
        /// <seealso cref="IIdentifiable.Synchronize(IdentifiableType, Action{bool, string})"/>
        public void Synchronize(T serialized, Action<bool, string> onFinished = null);
    }

}
