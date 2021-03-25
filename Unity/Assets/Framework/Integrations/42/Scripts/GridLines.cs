// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLines : MonoBehaviour
{
    public GameObject cube;

    public void Start()
    {
        LineRenderer lineRenderer = cube.AddComponent<LineRenderer>();
        for(int c = 0; c < 4; c++)
        {
            lineRenderer.SetPosition(c, new Vector3(1, 1, 1));
        }
    }
}
