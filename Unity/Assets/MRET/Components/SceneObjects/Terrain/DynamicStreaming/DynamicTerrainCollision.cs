// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// DynamicTerrainCollision class contains the event trigger for when the character controller enters a new terrain tile.
    /// !!! This component must be added to the object in MRET with the character controller !!!
    /// Author: Molly T. Goldstein
    /// </summary>
    public class DynamicTerrainCollision : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DynamicTerrainCollision);

        /// <summary>
        /// This function triggers when the character controller enters a new terrain.
        /// </summary>
        /// <param name="hit">The hit event</param>
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If the object hit is a terrain, not the active terrain, and this is a new hit
            // then set the latest hit to this hit, update the active terrain, and call
            // the EnteredNewTerrain() function to update the active tiles.
            if (!string.IsNullOrEmpty(DynamicTerrainStreamingManager.activeTerrain) &&
                !hit.gameObject.name.Equals(DynamicTerrainStreamingManager.activeTerrain) &&
                hit.gameObject.GetComponent<Terrain>() != null &&
                (DynamicTerrainStreamingManager.hit == null || hit.point != DynamicTerrainStreamingManager.hit.point))
            {
                DynamicTerrainStreamingManager.hit = hit;
                DynamicTerrainStreamingManager.activeTerrain = hit.gameObject.name;
                DynamicTerrainTile tile = hit.gameObject.GetComponent<DynamicTerrainTile>();
                tile.EnteredNewTerrain();
            }
        }
    }
}