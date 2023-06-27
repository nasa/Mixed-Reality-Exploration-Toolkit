// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.PointCloud
{
    /// <remarks>
    /// History:
    /// 09 June 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// PointCloudGroup
	///
	/// A point cloud group in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    public class PointCloudGroup : Group<PointCloudsType, PointCloudGroup, PointCloudGroup, PointCloudType, PointCloud, PointCloud>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PointCloudGroup);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private PointCloudsType serializedPointClouds;

        #region MRETUpdateBehaviour
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

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.Deserialize(GT, SerializationState)"/>
        protected override IEnumerator Deserialize(PointCloudsType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization
            serializedPointClouds = serialized;

            // Perform the group deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.Serialize(GT, SerializationState)"/>
        protected override IEnumerator Serialize(PointCloudsType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedPointClouds = serialized;

            // Record the deserialization state as complete
            serializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.InstantiateAndDeserialize(CT, GameObject, Transform, Action{CI})"/>
        protected override void InstantiateSerializable(PointCloudType serializedChild, GameObject go, Transform parent, Action<PointCloud> onFinished = null)
        {
            ProjectManager.PointCloudManager.InstantiatePointCloud(serializedChild, go, parent, onFinished);
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.InstantiateAndDeserialize(GT, GameObject, Transform, Action{GI})"/>
        protected override void InstantiateSerializable(PointCloudsType serializedChildGroup, GameObject go, Transform parent, Action<PointCloudGroup> onFinished = null)
        {
            ProjectManager.PointCloudManager.InstantiatePointClouds(serializedChildGroup, go, parent, onFinished);
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.ReadSerializedItems(GT)"/>
        protected override VersionedType[] ReadSerializedItems(PointCloudsType serializedGroup) => serializedGroup.Items;

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.WriteSerializedItems(GT, VersionedType[])"/>
        protected override void WriteSerializedItems(PointCloudsType serializedGroup, VersionedType[] serializedItems)
        {
            serializedGroup.Items = serializedItems;
        }
        #endregion Serialization

    }
}
