// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject
{
    public class SceneObjectPrefab
    {
        /// <summary>
        /// Identifier (such as a file path) for the original resource.
        /// </summary>
        public string resourceID;

        public GameObject prefab;
    }
}