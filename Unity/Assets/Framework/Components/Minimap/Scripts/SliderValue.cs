// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
