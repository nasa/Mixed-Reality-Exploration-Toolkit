// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.PathVisualization
{
    [CreateAssetMenu(fileName = "Data", menuName = "pathvisulization points", order = 1)]

    public class PathVisualizationData : ScriptableObject
    {
        // scriptable object that holds a Vector3 array of positions to be drawn
        public Vector3[] positions;
    }
}