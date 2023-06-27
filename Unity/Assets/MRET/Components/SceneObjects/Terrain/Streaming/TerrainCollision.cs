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
    /// TerrainCollision class contains the event trigger for when the character controller enters a new terrain tile.
    /// It also determines the starting terrain tile by finding the closest tile.
    /// !!! This component must be added to the object in MRET with the character controller !!!
    /// Author: Molly T. Goldstein
    /// </summary>
    public class TerrainCollision : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TerrainCollision);

        ///<summary>
        /// Flag to indicate if the stream has started yet.
        ///</summary>
        private bool start = true;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            TerrainLoadingManager.character = this.gameObject;
        }

        ///<summary>
        /// If the start flag is true, this is the first update and the closest tile
        /// to the character controller.
        ///</summary>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (start)
            {
                string tMin = "";
                float minDist = Mathf.Infinity;
                Vector3 currentPos = this.transform.position;
                if(string.IsNullOrEmpty(TerrainStreamingManager.start))
                {
                    foreach(Terrain tile in TerrainLoadingManager.allTerrainTiles)
                    {
                        float dist = Vector3.Distance(tile.transform.position, currentPos);
                        if (dist < minDist)
                        {
                            tMin = tile.gameObject.name;
                            minDist = dist;
                        }
                    }
                    TerrainStreamingManager.start = tMin;
                    TerrainStreamingManager.activeTerrain = tMin;
                }
            start = false;
            }
        }
        ///<summary>
        /// This function triggers when the character controller enters a new terrain.
        ///</summary>
        ///<param name="hit">The hit event</param>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If the object hit is a terrain, not the active terrain, and this is a new hit
            // then set the latest hit to this hit, update the active terrain, and call
            // the EnteredNewTerrain() function to update the active tiles.
            if(!string.IsNullOrEmpty(TerrainStreamingManager.activeTerrain) &&
                !hit.gameObject.name.Equals(TerrainStreamingManager.activeTerrain) &&
                hit.gameObject.GetComponent<Terrain>() != null &&
                (TerrainStreamingManager.hit == null || hit.point != TerrainStreamingManager.hit.point))
            {
                TerrainStreamingManager.hit = hit;
                TerrainStreamingManager.activeTerrain = hit.gameObject.name;
                TerrainTile tile = hit.gameObject.GetComponent<TerrainTile>();
                tile.EnteredNewTerrain();
            }  
        }
    }
}