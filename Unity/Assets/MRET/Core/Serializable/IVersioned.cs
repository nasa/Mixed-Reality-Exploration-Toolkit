// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// IVersioned
    ///
    /// Describes a serializable versioned object in MRET
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public interface IVersioned
    {
        /// <summary>
        /// Creates an empty serialized type instance associated with this class instance
        /// </summary>
        /// <returns>An empty serialized type instance</returns>
        public VersionedType CreateSerializedType();

        /// <summary>
        /// The version of this object.<br>
        /// </summary>
        public string version { get; }

        /// <summary>
        /// Deserializes the supplied serialized object into this implementing object instance. The caller
        /// should supply an action if it needs to take some action when the deserialization has completed.<br>
        /// </summary>
        /// <param name="serialized">The serialized versioned object to deserialize into this object instance</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the deserialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        /// 
        /// <see cref="System.Action"/>
        /// <see cref="VersionedType"/>
        public void Deserialize(VersionedType serialized, Action<bool, string> onFinished = null);

        /// <summary>
        /// Serializes this implementing object instance into the supplied serialized object. The caller
        /// should supply an action if it needs to take some action when the serialization has completed.<br>
        /// </summary>
        /// <param name="serialized">The serialized versioned object to write the serialization of this object instance</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the serialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        /// 
        /// <see cref="System.Action"/>
        /// <see cref="VersionedType"/>
        public void Serialize(VersionedType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// IVersioned
    ///
    /// Describes a <generic> serializable versioned object in MRET
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public interface IVersioned<T> : IVersioned
        where T : VersionedType
    {
        /// <seealso cref="IVersioned.CreateSerializedType"/>
        new public T CreateSerializedType();

        /// <seealso cref="IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(T serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(T serialized, Action<bool, string> onFinished = null);
    }
}
