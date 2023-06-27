// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.RecordingCamera
{
    public class Screen_correction : MonoBehaviour
    {
        // Start is called before the first frame update

        void Start()
        {
            Screen.SetResolution((int)Screen.width, (int)Screen.height, true);
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}