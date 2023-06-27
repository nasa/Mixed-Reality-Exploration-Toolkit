// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MRET_EXTENSION_POINTCLOUDVIEWER
using unitycodercom_PointCloudBinaryViewer;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Extensions.PointCloud
{
    public class NewPointCloudLoader : MRETManager<NewPointCloudLoader>
    {
        public Material defaultPCMaterial;

#if MRET_EXTENSION_POINTCLOUDVIEWER
        public static PointCloudViewerDX11 InstantiatePointCloud_s(Vector3 pos, Quaternion rot, Vector3 scl,
            string PCFilePath, string pointCloudName = "PC",
            Material cloudMaterial = null, GameObject pcParent = null)
        {
            if (Instance)
            {
                return Instance.InstantiatePointCloud(pos, rot, scl, PCFilePath,
                    pointCloudName, cloudMaterial, pcParent);
            }

            return null;
        }

        public PointCloudViewerDX11 InstantiatePointCloud(Vector3 pos, Quaternion rot, Vector3 scl,
            string PCFilePath, string pointCloudName,
            Material cloudMaterial, GameObject pcParent)
        {
            UnityEngine.Debug.Log("PointCloudLoader script started");

            if (cloudMaterial == null)
            {
                cloudMaterial = defaultPCMaterial;
            }

            GameObject pointCloudGameObject = new GameObject();
            pointCloudGameObject.transform.parent = pcParent.transform;
            pointCloudGameObject.transform.position = pos;
            pointCloudGameObject.transform.rotation = rot;
            pointCloudGameObject.transform.localScale = scl;
            NewPointCloudRenderer pointCloudRenderer = pointCloudGameObject.AddComponent<NewPointCloudRenderer>();
            PointCloudViewerDX11 pcrScript = pointCloudGameObject.AddComponent<PointCloudViewerDX11>();
            //pointCloudGameObject.tag = "PointcloudStatic_II";
            pointCloudGameObject.name = pointCloudName;
            pcrScript.cloudMaterial = cloudMaterial;
            pcrScript.fileName = PCFilePath;

            //TODO: See if there's a way to modify starting position of point cloud

            return pcrScript;
        }
#endif
    }
}