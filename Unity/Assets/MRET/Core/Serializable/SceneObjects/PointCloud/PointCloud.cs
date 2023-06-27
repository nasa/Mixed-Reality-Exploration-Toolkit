// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Extensions.PointCloud;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.PointCloud
{
    /// <remarks>
    /// History:
    /// 09 June 2023: Created
    /// </remarks>
    ///
    /// <summary>
    /// PointCloud
    ///
    /// A point cloud in MRET
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public class PointCloud : SceneObject<PointCloudType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PointCloud);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private PointCloudType serializedPointCloud;

        /// <summary>
        /// Indicates the type of 3D drawing being rendered
        /// </summary>
        public PointCloudCategoryType PointCloudType { get; private set; }

        /// <summary>
        /// The point cloud source<br>
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        /// The point cloud source<br>
        /// </summary>
        public int LODLevel { get; private set; }

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

        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <summary>
        /// Asynchronously Deserializes the supplied serialized point cloud source and updates the supplied state
        /// with the resulting point cloud.
        /// </summary>
        /// <param name="serializedSource">The serialized <code>PointCloudSourceType</code> model</param>
        /// <param name="sourceDeserializationState">The <code>ObjectSerializationState</code> to populate with the result.</param>
        /// 
        /// <see cref="ModelType"/>
        /// <see cref="ObjectSerializationState{O}"/>
        /// 
        protected virtual IEnumerator DeserializeSource(PointCloudSourceType serializedSource, ObjectSerializationState<object> sourceDeserializationState)
        {
            void DeserializeSourceAction(object source)
            {
                // Assign the source
                sourceDeserializationState.obj = source;

                // Update the state
                if (source == null)
                {
                    sourceDeserializationState.Error("The point cloud source failed to deserialize");
                }

                // Mark as complete
                sourceDeserializationState.complete = true;
            };

            // Load the source
            SchemaUtil.LoadPointCloudSource(serializedSource, DeserializeSourceAction);

            yield return null;
        }

        /// <seealso cref="SceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(PointCloudType serialized, SerializationState deserializationState)
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
            serializedPointCloud = serialized;

            // Extract the settings
            PointCloudType = serializedPointCloud.Type;
            LODLevel = serializedPointCloud.LODLevel;

            // Perform the source deserialization
            ObjectSerializationState<object> sourceDeserializationState = new ObjectSerializationState<object>();
            StartCoroutine(DeserializeSource(serializedPointCloud.Source, sourceDeserializationState));

            // Wait for the coroutine to complete
            while (!sourceDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // Record the deserialization state
            deserializationState.Update(sourceDeserializationState);

            // Make sure the resultant source object is not null
            if (sourceDeserializationState.obj is null)
            {
                deserializationState.Error("Deserialized point cloud source cannot be null, denoting a possible internal issue.");
            }

            // If the scene objects deserialization failed, there's no point in continuing. Something is wrong
            if (deserializationState.IsError) yield break;

            // Allow the subclasses to initialize the source
            StartCoroutine(InitializeSource(sourceDeserializationState));

            // Wait for the coroutine to complete
            while (!sourceDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // Record the deserialization state
            deserializationState.Update(sourceDeserializationState);

            // If the source loading failed, exit with an error
            if (deserializationState.IsError) yield break;

            // Assign the source
            Source = sourceDeserializationState.obj;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="SceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(PointCloudType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize out the settings
            serialized.Type = PointCloudType;
            serialized.LODLevel = LODLevel;

            // Start with our internal serialized source to serialize out the point cloud source
            // using the original deserialized structure (if was provided during deserialization)
            PointCloudSourceType serializedSource = null;
            if (serializedPointCloud != null)
            {
                // Use this source structure
                serializedSource = serializedPointCloud.Source;
            }

            // Make sure we have a valid serialized source reference
            if (serializedSource is null)
            {
                // Default to basic source structure
                serializedSource = new PointCloudSourceType();
            }

            // Serialize out the source
            serialized.Source = serializedSource;

            // Save the final serialized reference
            serializedPointCloud = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <summary>
        /// Creates an interactable part
        /// </summary>
        /// <param name="partName">The name of the part</param>
        /// <param name="partSpecifications">The optional <code>PartSpecifications</code></param>
        /// <returns>A <code>InteractablePart</code> instance</returns>
        public static PointCloud Create(string pointCloudName, string sourceFile)
        {
            // FIXME
            GameObject pointCloudGameObject = new GameObject(pointCloudName);
            PointCloud result = pointCloudGameObject.AddComponent<PointCloud>();
            PointCloudExt.CreateStaticPointCloud(pointCloudName, pointCloudGameObject, sourceFile);
 
            return result;
        }

        /// <summary>
        /// Called to initialize the point cloud from the source. Implementing routine should update
        /// the state when complete.
        /// </summary>
        /// <param name="sourceDeserializationState">The <code>ObjectSerializationState</code> to be updated</param>
        /// 
        /// <see cref="ObjectSerializationState{O}"/>
        protected virtual IEnumerator InitializeSource(ObjectSerializationState<object> sourceDeserializationState)
        {
            // FIXME
            if (sourceDeserializationState.obj is string)
            {
                GameObject pointCloudGameObject = new GameObject("PointCloudViewer");
                pointCloudGameObject.transform.parent = transform;
                pointCloudGameObject.transform.localPosition = Vector3.zero;
                pointCloudGameObject.transform.localRotation = Quaternion.identity;
                pointCloudGameObject.transform.localScale = Vector3.one;
                PointCloudExt.CreateStaticPointCloud(name, pointCloudGameObject, sourceDeserializationState.obj as string);
                sourceDeserializationState.complete = true; // <= You are done and it was successful
            }
            else
            {
                string message = "Currently unsupported type: " + sourceDeserializationState.obj;
                LogError(message);
                sourceDeserializationState.Error(message); // <= This will terminate the deserialization process
                yield break;
            }

            yield return null;
        }

    }

}