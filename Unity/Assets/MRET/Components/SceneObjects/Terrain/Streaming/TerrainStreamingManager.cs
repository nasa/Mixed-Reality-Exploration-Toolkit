// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    ///<summary>
    /// TerrainStreamingManager manages the terrain streaming.
    /// It sets up all of the neighbor list of the terrain tiles.
    /// It also tracks the active terrain (the terrain the character controller object is standing on).
    /// Author: Molly T. Goldstein
    ///</summary>
    public class TerrainStreamingManager
    {
        ///<summary>
        /// The string for the starting tile to stream.
        ///</summary>
        public static string start;

        ///<summary>
        /// The current terrain that the character controller object is standing on (the active terrain).
        ///</summary>
        public static string activeTerrain = start;

        /// For streaming: Head and Tail of active terrain tiles linked list
        /// <summary>
        /// Head of linked list
        ///</summary>
        public static ActiveTerrain activeTerrainHead;

        /// <summary>
        /// Tail of linked list
        ///</summary>
        public static ActiveTerrain activeTerrainTail;

        ///<summary>
        /// The most recent hit object set from the TerrainCollision class
        ///</summary>
        public static ControllerColliderHit hit = null;

        ///<summary>
        /// This function goes through all of the terrains created in TerrainLoadingManager and sets all of the neighbors in
        /// the terrain tile component.
        ///</summary>
        ///<param name="dimX">The number of tiles across</param>
        ///<param name="dimY">The number of tiles down</param>
        public static void SetNeighbors(int dimX, int dimY)
        {
            for (int i = 0; i < dimY; i++)
            {
                for (int j = 0; j < dimX; j++)
                {
                    Terrain terrain = GameObject.Find("Terrain_" + i.ToString() + "_" + (j).ToString()).GetComponent<Terrain>();
                    terrain.gameObject.AddComponent<TerrainTile>();
                    if (j > 0)
                    {
                        AddNeighbor(terrain, i, j - 1);
                    }
                    if (i > 0)
                    {
                        AddNeighbor(terrain, i - 1, j);
                    }

                    if (i > 0 && j > 0)
                    {
                        AddNeighbor(terrain, i - 1, j - 1);
                    }

                    if (i > 0 && j < dimX - 1)
                    {
                        AddNeighbor(terrain, i - 1, j + 1);
                    }
                }
            }
        }

        ///<summary>
        /// This function adds the terrain tile to the neighbor list of the terrain tile at Terrain_x_y.
        /// Terrain_x_y is also added to the terrain neighbor list of the passed in terrain tile.
        ///</summary>
        ///<param name="terrain">The terrain that is neighbors with Terrain_x_y</param>
        ///<param name="x">The x location of the neighbor</param>
        ///<param name="y">The y location of the neighbor</param>
        private static void AddNeighbor(Terrain terrain, int x, int y)
        {
            GameObject neighbor = GameObject.Find("Terrain_" + x.ToString() + "_" + y.ToString());
            neighbor.GetComponent<TerrainTile>().neighbors.Add(terrain);
            terrain.gameObject.GetComponent<TerrainTile>().neighbors.Add(neighbor.GetComponent<Terrain>());
        }
    }
}