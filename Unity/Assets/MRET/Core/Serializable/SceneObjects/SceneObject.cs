// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 18 November 2020: Created
    /// 6 January 2022: Added touch hold (DZB)
    /// 26 January 2022: Made trigger methods overridable (DZB)
    /// 11 October 2022: Renamed to SceneObject in preparation for the new schema hierarchy
    /// </remarks>
    /// 
    /// <summary>
    /// Base scene object
    /// 
    /// Author: Dylan Z. Baker
    /// Author: Jeffrey Hosler (refactored)
    /// </summary>
	public abstract class SceneObject<T> : Identifiable<T>, ISceneObject<T>
        where T : SceneObjectType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObject<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private SceneObjectType serializedSceneObject;

        /// <summary>
        /// The group of child scene objects
        /// </summary>
        public IGroup ChildSceneObjectGroup { get => _childSceneObjectGroup; }
        private SceneObjectGroup _childSceneObjectGroup = null;

        protected bool useSizeForDefaultOnSerialize;

        #region ISceneObject
        /// <seealso cref="ISceneObject.CreateSerializedType"/>
        SceneObjectType ISceneObject.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="ISceneObject.parent"/>
        public ISceneObject parent
        {
            get => (ProjectManager.SceneObjectManager != null)
                ? ProjectManager.SceneObjectManager.GetParent<ISceneObject>(gameObject)
                : null;
        }

        /// <seealso cref="ISceneObject.Synchronize(SceneObjectType, Action{bool, string})"/>
        void ISceneObject.Synchronize(SceneObjectType serialized, Action<bool, string> onFinished)
        {
            Synchronize(serialized as T, onFinished);
        }

        /// <seealso cref="ISceneObject.Deserialize(SceneObjectType, Action{bool, string})"/>
        void ISceneObject.Deserialize(SceneObjectType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="ISceneObject.Serialize(SceneObjectType, Action{bool, string})"/>
        void ISceneObject.Serialize(SceneObjectType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion ISceneObject

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
            useSizeForDefaultOnSerialize = false;
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Destroy the group
            Destroy(_childSceneObjectGroup);
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <summary>
        /// Asynchronously Deserializes the supplied serialized transform and updates the supplied
        /// state with the result.
        /// </summary>
        /// <param name="serializedTransform">The serialized <code>TransformType</code> transform</param>
        /// <param name="transformDeserializationState">The <code>SerializationState</code> to populate
        ///     with the result.</param>
        /// 
        /// <see cref="TransformType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeTransform(TransformType serializedTransform, SerializationState transformDeserializationState)
        {
            // Transform is optional, but we still need to deserialize the default settings
            if (serializedTransform is null)
            {
                // Create the defaults
                serializedTransform = new TransformType();
            }

            // Deserialize the transform
            SchemaUtil.DeserializeTransform(serializedTransform, gameObject);

            // Mark as complete
            transformDeserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Identifiable{T}.Deserialize(T, SerializationState)"/>
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
            serializedSceneObject = serialized;

            // Transform deserialization (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                TransformType serializedTransform = serializedSceneObject.Transform ?? new TransformType();

                // Deserialize the Transform (required)
                SerializationState transformDeserializationState = new SerializationState();
                StartCoroutine(DeserializeTransform(serializedTransform, transformDeserializationState));

                // Wait for the coroutine to complete
                while (!transformDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(transformDeserializationState);

                // If the user deserialization failed, there's no point in continuing
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Clear existing child scene object group
            if (_childSceneObjectGroup != null)
            {
                Destroy(_childSceneObjectGroup);
                _childSceneObjectGroup = null;
            }

            // Deserialize the child scene objects
            if (serializedSceneObject.ChildSceneObjects != null)
            {
                // Perform the group deserialization
                VersionedSerializationState<SceneObjectGroup> groupDeserializationState = new VersionedSerializationState<SceneObjectGroup>();
                StartCoroutine(DeserializeVersioned(serializedSceneObject.ChildSceneObjects, gameObject, gameObject.transform, groupDeserializationState));

                // Wait for the coroutine to complete
                while (!groupDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(groupDeserializationState);

                // Make sure the resultant child scene object group type is not null
                if (groupDeserializationState.versioned is null)
                {
                    deserializationState.Error("Deserialized child scene objects group cannot be null, denoting a possible internal issue.");
                }

                // If the group deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Assign the group
                _childSceneObjectGroup = groupDeserializationState.versioned;

                // Clear the state
                deserializationState.Clear();
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously serializes the transform into the supplied serialized transform and
        /// updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedTransform">The serialized <code>TransformType</code> to populate with the transform</param>
        /// <param name="transformSerializationState">The <code>SerializationState</code> to populate with the serialization state.</param>
        /// 
        /// <see cref="TransformType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator SerializeTransform(TransformType serializedTransform, SerializationState transformSerializationState)
        {
            // Transform settings are optional
            if (serializedTransform != null)
            {
                // Serialize the transform
                SchemaUtil.SerializeTransform(serializedTransform, gameObject, useSizeForDefaultOnSerialize);
            }

            // Mark as complete
            transformSerializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Identifiable{T}.Serialize(T, SerializationState)"/>
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

            // Serialize the transform (optional)
            {
                // Start with our internal serialized transform to serialize out the transform
                // using the original deserialized structure (if was provided during deserialization)
                // or just use the default. We want to serialize and then check for all defaults.
                serialized.Transform = (serializedSceneObject != null) && (serializedSceneObject.Transform != null) ?
                    serializedSceneObject.Transform :
                    new TransformType();

                // Serialize the transform
                SerializationState transformSerializationState = new SerializationState();
                StartCoroutine(SerializeTransform(serialized.Transform, transformSerializationState));

                // Wait for the coroutine to complete
                while (!transformSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(transformSerializationState);

                // If the serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Don't serialize out anything if all defaults
                if ((serialized.Transform != null) &&
                    (serialized.Transform.Position == null) &&
                    (serialized.Transform.Item == null) &&
                    (serialized.Transform.Item1 == null))
                {
                    // Set to null for defaults
                    serialized.Transform = null;
                }

                // Clear the state
                serializationState.Clear();
            }

            // Serialize the child scene object group. The child can serialize itself.
            SceneObjectsType serializedSceneObjects = null;
            if (_childSceneObjectGroup != null)
            {
                serializedSceneObjects = _childSceneObjectGroup.CreateSerializedType();

                // Perform the group serialization
                SerializationState groupSerializationState = new SerializationState();
                StartCoroutine(_childSceneObjectGroup.SerializeWithLogging(serializedSceneObjects, groupSerializationState));

                // Wait for the coroutine to complete
                while (!groupSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(groupSerializationState);

                // If the child serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }
            serialized.ChildSceneObjects = serializedSceneObjects;

            // Save the final serialized reference
            serializedSceneObject = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization


        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedSceneObject>();
        }

    }

    /// <summary>
    /// Provides an implementation for the abstract SceneObject class
    /// </summary>
    public class SceneObject : SceneObject<SceneObjectType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObject);
    }

    /// <summary>
    /// Used to keep the default values from the schema in sync
    /// </summary>
    public class SceneObjectDefaults
    {
    }

}