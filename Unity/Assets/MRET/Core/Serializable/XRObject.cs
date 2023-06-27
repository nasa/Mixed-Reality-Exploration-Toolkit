// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 3 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// Base scene object.<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class XRObject<T> : VersionedMRETBehaviour<T>, IXRObject<T>
        where T : XRType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(XRObject<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedXRSceneObject;

        #region IXRObject
        /// <seealso cref="IXRObject.AREnabled"/>
        public bool AREnabled { get; set; }

        /// <seealso cref="IXRObject.VREnabled"/>
        public bool VREnabled { get; set; }

        /// <seealso cref="IXRObject.CreateSerializedType"/>
        XRType IXRObject.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IXRObject.Deserialize(XRSceneObjectType, Action{bool, string})"/>
        void IXRObject.Deserialize(XRType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IXRObject.Serialize(XRSceneObjectType, Action{bool, string})"/>
        void IXRObject.Serialize(XRType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IXRObject

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Set the defaults
            AREnabled = XRObjectDefaults.AR_ENABLED;
            VREnabled = XRObjectDefaults.VR_ENABLED;
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="VersionedMRETBehaviour{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(T serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedXRSceneObject = serialized;

            // Load the configuration
            AREnabled = serializedXRSceneObject.AREnabled;
            VREnabled = serializedXRSceneObject.VREnabled;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="VersionedMRETBehaviour{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the XR flags
            serialized.AREnabled = AREnabled;
            serialized.VREnabled = VREnabled;

            // Save the final serialized reference
            serializedXRSceneObject = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

    }

    /// <summary>
    /// Provides an implementation for the abstract XRObject class
    /// </summary>
    public class XRObject : XRObject<XRType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(XRObject);
    }

    /// <summary>
    /// Used to keep the default values from the schema in sync
    /// </summary>
    public class XRObjectDefaults : VersionedDefaults
    {
        public static readonly bool AR_ENABLED = new XRType().AREnabled;
        public static readonly bool VR_ENABLED = new XRType().VREnabled;
    }

}
