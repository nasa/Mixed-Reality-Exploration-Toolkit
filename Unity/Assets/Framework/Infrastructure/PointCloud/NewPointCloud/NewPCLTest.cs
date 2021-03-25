// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud
{
    public class NewPCLTest : MonoBehaviour
    {
        public Material cloudMaterial;
        int counter = 0;
        // Start is called before the first frame update
        void Start() { }

        // Update is called once per frame
        void Update()
        {
            counter++;
            if (counter == 500)
            {
                UnityEngine.Debug.Log("PointCloudLoader TEST SCRIPT started");
                GameObject PointCloudLoaderObject = GameObject.Find("NewPointCloudLoader");
                NewPointCloudLoader pointCloudloaderScript = PointCloudLoaderObject.GetComponent<NewPointCloudLoader>();
                string PCFilePath = "PointCloudResources/xyzrgb_dragon.bin";
                string pointCloudName = "PointCloudDX11";
                
                pointCloudloaderScript.InstantiatePointCloud(Vector3.zero, Quaternion.identity, Vector3.one,
                    PCFilePath, pointCloudName, cloudMaterial, null);
            }
        }
    }
}