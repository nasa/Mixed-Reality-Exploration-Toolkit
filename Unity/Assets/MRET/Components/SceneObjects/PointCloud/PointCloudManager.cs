// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.PointCloud
{
    public class PointCloudManager : MRETSerializableManager<PointCloudManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PointCloudManager);

        /// <summary>
        /// Material used for point clouds.
        /// </summary>
        public static Material PointCloudMaterial => Instance.pointCloudMaterial;

        /// <summary>
        /// Material used for point clouds.
        /// </summary>
        [Tooltip("Material used for point clouds.")]
        public Material pointCloudMaterial;

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

        }

        #region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            return ProjectManager.PointCloudContainer.transform;
        }

        /// <summary>
        /// Instantiates the point cloud from the supplied serialized point cloud.
        /// </summary>
        /// <param name="serializedPointCloud">The <code>PointCloudType</code> class instance
        ///     containing the serialized representation of the point cloud to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     point cloud. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     point cloud. If null, the project point cloud container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishPointCloudInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     point cloud instantiation. Called before the onLoaded action is called. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishPointCloudInstantiation method to provide additional context</param>
        public void InstantiatePointCloud(PointCloudType serializedPointCloud, GameObject go = null,
            Transform container = null, Action<PointCloud> onLoaded = null,
            FinishSerializableInstantiationDelegate<PointCloudType, PointCloud> finishPointCloudInstantiation = null,
            params object[] context)
        {
            // Instantiate and load the new point cloud
            InstantiateSerializable(serializedPointCloud, go, container, onLoaded,
                finishPointCloudInstantiation, context);
        }

        /// <summary>
        /// Instantiates the point cloud group from the supplied serialized point cloud group.
        /// </summary>
        /// <param name="serializedPointClouds">The <code>PointCloudsType</code> class instance
        ///     containing the serialized representation of the point cloud group to instantiate.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the
        ///     instantiated point cloud group. If null, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     point cloud. If null, the default project point cloud container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion.</param>
        public void InstantiatePointClouds(PointCloudsType serializedPointClouds, GameObject go = null,
            Transform container = null, Action<PointCloudGroup> onLoaded = null,
            FinishSerializableInstantiationDelegate<PointCloudsType, PointCloudGroup> finishPointCloudsInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize
            InstantiateSerializable(serializedPointClouds, go, container, onLoaded,
                finishPointCloudsInstantiation, context);
        }

        /// <summary>
        /// Creates a point cloud.
        /// </summary>
        /// <param name="pointCloudName">Name of the point cloud</param>
        /// <param name="sourceFile">The point cloud file</param>
        /// <param name="parent">Parent for the point cloud</param>
        /// <param name="localPosition">Local position of the point cloud</param>
        /// <param name="localRotation">Local rotation of the point cloud</param>
        /// <param name="localScale">Local scale of the point cloud</param>
        /// <returns>A <code>PointCloud</code> instance</returns>
        /// <see cref="PointCloud"/>
        public PointCloud CreatePointCloud(string pointCloudName, string sourceFile,
            GameObject parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            PointCloud newPointCloud = PointCloud.Create(pointCloudName, sourceFile);

            // Additional settings if valid reference
            if (newPointCloud != null)
            {
                // Parent
                if (parent == null)
                {
                    newPointCloud.transform.SetParent(ProjectManager.PointCloudContainer.transform);
                }
                else
                {
                    newPointCloud.transform.SetParent(parent.transform);
                }

                // Transform
                newPointCloud.transform.localPosition = localPosition;
                newPointCloud.transform.localRotation = localRotation;
                newPointCloud.transform.localScale = localScale;

                // Record the action
                var serializedPointCloud = newPointCloud.CreateSerializedType();
                newPointCloud.Serialize(serializedPointCloud);
                ProjectManager.UndoManager.AddAction(
                    new AddSceneObjectAction(serializedPointCloud),
                    new DeleteIdentifiableObjectAction(newPointCloud.id));
            }

            return newPointCloud;
        }
        #endregion Serializable Instantiation

    }
}