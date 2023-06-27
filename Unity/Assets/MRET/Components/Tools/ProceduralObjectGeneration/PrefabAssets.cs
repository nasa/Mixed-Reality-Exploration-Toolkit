// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.ProceduralObjectGeneration
{
    /**
     * Container class for prefab asset references
     * 
     * @author Jeffrey Hosler
     * 
     * TODO: Candidate to move to a more general location
     */
    [System.Serializable]
    public class PrefabAssets
    {
        [Tooltip("The Mesh asset")]
        public GameObject mesh = null;
        [Tooltip("The Material asset to apply to the Mesh")]
        public Material material = null;
    }
}