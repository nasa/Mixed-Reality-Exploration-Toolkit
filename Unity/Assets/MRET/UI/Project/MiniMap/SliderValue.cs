// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.UI.MiniMap
{
    public class SliderValue : MonoBehaviour
    {
        public Slider valueSlider;
        public TextMesh text;
        public Camera camera;
        public float zoomValue = 15;
        public float zoomValueF;

        private void FixedUpdate()
        {
            text.text = valueSlider.value.ToString();
            zoomValueF = valueSlider.value;
            camera.orthographicSize = zoomValueF;
        }
    }
}