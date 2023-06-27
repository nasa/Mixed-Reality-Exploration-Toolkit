// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.IO;
using UnityEngine;
#if MRET_EXTENSION_POINTCLOUDVIEWER
using unitycodercom_PointCloudBinaryViewer;
#endif
using GOV.NASA.GSFC.XR.MRET.SceneObjects.PointCloud;

namespace GOV.NASA.GSFC.XR.MRET.Extensions.PointCloud
{
    /// <summary>
    /// Abstract container class for point clouds.
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    public abstract class PointCloudExt
    {
        /// <summary>
        /// Creates a static point cloud from a file.
        /// </summary>
        /// <param name="pointCloudName">The name of the point cloud</param>
        /// <param name="go">The <code>GameObject</code> to contain the point cloud</param>
        /// <param name="sourceFile">The source file for the point cloud</param>
        public static void CreateStaticPointCloud(string pointCloudName, GameObject go, string sourceFile)
        {
            // This viewer expects a file, so make sure we received a file that exists
            string sourceFilePath = sourceFile;
            if (!File.Exists(sourceFilePath))
            {
                sourceFilePath = Path.Combine(MRET.ConfigurationManager.defaultPointCloudDirectory, sourceFile);
                if (!File.Exists(sourceFilePath))
                {
                    Debug.LogError("Source file cannot be located: " + sourceFile);
                    return;
                }
            }

#if MRET_EXTENSION_POINTCLOUDVIEWER
            PointCloudViewerDX11 pcrScript = go.AddComponent<PointCloudViewerDX11>();
            //pointCloudGameObject.tag = "PointcloudStatic_II";
            pcrScript.loadAtStart = true;
            pcrScript.useThreading = true;
            pcrScript.cloudMaterial = PointCloudManager.PointCloudMaterial;
            pcrScript.fileName = sourceFilePath;
#else
            Debug.LogWarning("PointCloudViewerDX11 not available");
#endif

        }
    }

}