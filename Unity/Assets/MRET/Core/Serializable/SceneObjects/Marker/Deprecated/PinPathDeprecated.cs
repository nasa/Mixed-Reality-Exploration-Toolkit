// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin
{
    public class PinPathDeprecated : MRETUpdateBehaviour
    {
        int pathCount;

        public LineDrawing lineRenderer;

        int i = 0;
        int j = 1;

        bool finished = false;

        public override string ClassName => nameof(PinPathDeprecated);

    }
}