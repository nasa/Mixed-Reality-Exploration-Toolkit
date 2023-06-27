// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Part
{
    public class InteractableEnclosure : PhysicalSceneObject<EnclosureType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableEnclosure);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private EnclosureType serializedEnclosure;

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
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="PhysicalSceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(EnclosureType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process the object specific configuration

            // Save the serialized reference
            serializedEnclosure = serialized;

            // Deserialize the configuration

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="PhysicalSceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(EnclosureType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Serialize the object specific configuration

            // Serialize the enclosure

            // Save the final serialized reference
            serializedEnclosure = serialized;

            // Record the deserialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

    }
}