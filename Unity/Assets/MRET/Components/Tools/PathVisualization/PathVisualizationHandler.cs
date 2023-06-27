// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.PathVisualization
{
    public class PathVisualizationHandler : MonoBehaviour
    {
        public PathVisualizationData data;
        public PathVisalizer pathVisalizer;
        public int resolution = 8;

        private void Start()
        {
            pathVisalizer.ReadPathVisData(data); // read data from PathVisalizationData scriptable object
            pathVisalizer.GenerateInterpolations(resolution); // generate interpolations (so it adapts to curvature of terrain) given a resolution
        }
    }
}