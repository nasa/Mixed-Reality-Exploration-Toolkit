// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.PathVisualization
{
    public class Breadcrumbing : MonoBehaviour
    {
        [Tooltip("Objet to be breadcrumbed")]
        public GameObject breadcrumbingObject;
        public ArrayList objPosition = new ArrayList();

        // add positions to an array list
        void Update()
        {
            objPosition.Add(breadcrumbingObject.transform.position);
        }
    }
}