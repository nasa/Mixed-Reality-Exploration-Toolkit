// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// ActiveTerrain class is used as the class for the active terrains linked list
    /// maintained in TerrainStreamingManager.
    /// Author: Molly T. Goldstein
    /// </summary>
    public class ActiveTerrain
    {
        public Terrain terrain;
        public ActiveTerrain next; 
    }
}