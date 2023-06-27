// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Extensions.Geospatial;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    ///<summary>
    /// DynamicTerrainStreamingManager manages the dynamic terrain streaming (not currently included in TerrainLoadingManager)
    /// It works similarly to TerrainStreamingManager, but the terrain tile DEM files and tile terrains are generated on an as-needed basis (hence dynamically)
    /// The data structures used in this class are different due to the nature of dynamic terrain loading, so there are several differences from TerrainStreamingManager.
    /// However, connecting this class with TerrainLoadingManager should be overall the same.
    /// Author: Molly T. Goldstein
    ///</summary>
    public class DynamicTerrainStreamingManager : MRETBehaviour
    {
        public override string ClassName  => nameof(DynamicTerrainStreamingManager);

        /// <summary>
        /// Absolute path to the DEM file (i.e., "C:\Users\mtgoldst\Documents\Terrain_Data\Apollo15\Test\NAC_DTM_APOLLO15_2_CROPPED_C.raw")
        ///</summary>
        public static string filepath = @"C:\Users\mtgoldst\Documents\Terrain_Data\Apollo15\Test\NAC_DTM_APOLLO15_2_CROPPED_C.raw";

        ///<summary>
        /// Width of tiles to stream
        ///</summary>
        public static int tileSizeX = 512;

        ///<summary>
        ///Length of tiles to stream
        ///</summary>
        public static int tileSizeY = 512;

        /// <summary>
        /// Range of the height of the DEM file
        ///</summary>
        public static int height;

        ///<summary>
        /// All of the terrain tile names to be used in the scene.
        ///</summary>
        public static Dictionary<string,List<string>> allTerrains = new Dictionary<string, List<string>>();

        ///<summary>
        /// The current terrain that the character controller object is standing on (the active terrain).
        ///</summary>
        public static string activeTerrain;

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
        /// A map of all generated terrain tiles associated with their object names.
        ///</summary>
        public static Dictionary<string, Terrain> generatedTerrains;

        ///<summary>
        /// This function goes through all of the terrains to be created in the scene and stores them in the
        /// allTerrains list
        ///</summary>
        override protected void MRETStart()
        {
            GeospatialExt.GeoInfo info = new GeospatialExt.GeoInfo(filepath);

            int fileSizeX = info.Width();
            int fileSizeY = info.Length();

            // Calculate the dimensions of the overall terrain (i.e., 4 tiles by 4 tiles)
            int tilesNumX = (int) Math.Floor((decimal)fileSizeX/tileSizeX);
            int tilesNumY = (int) Math.Floor((decimal)fileSizeY/tileSizeY);

            height = info.Range();

            // Populate the allTerrains list with each terrain tile in the scene and their corresponding list of neighboring terrains.
            for (int i = 0; i < tilesNumX; i++)
            {
                for(int j = 0; j < tilesNumY; j++)
                {
                    string current = "Terrain_" + i.ToString() + "_" + j.ToString();

                    List<string> neighbors = new List<string>();

                    if(j > 0)
                    {
                        neighbors.Add("Terrain_" + (i).ToString() + "_" + (j-1).ToString());
                    }

                    if(i > 0)
                    {   
                        neighbors.Add("Terrain_" + (i - 1).ToString() + "_" + (j).ToString());
                    }

                    if(i > 0 && j > 0)
                    {
                        neighbors.Add("Terrain_" + (i - 1).ToString() + "_" + (j - 1).ToString());                   
                    }

                    if (i > 0 && j < tilesNumX-1)
                    {
                        neighbors.Add("Terrain_" + (i - 1).ToString() + "_" + (j + 1).ToString());
                    }

                    allTerrains.Add(current, neighbors);

                    foreach(string neighbor in neighbors)
                    {
                        if(allTerrains[neighbor] == null)
                        {
                            allTerrains[neighbor] = new List<string>();
                        }
                        allTerrains[neighbor].Add(current);
                    }
                }
            }

            // Start the streaming in the middle of the overall terrain (middle tile)
            int x = (int) Math.Ceiling((decimal) tilesNumX/2);
            int y = (int) Math.Ceiling((decimal) tilesNumY/2);

            // Generate the starting terrain tile
            string startTerrain = "Terrain_" + x.ToString() + "_" + y.ToString();
            activeTerrain = startTerrain;
            generatedTerrains = new Dictionary<string, Terrain>();
            CreateTerrain(startTerrain).start = true;

            // Generate all neighbors of the starting terrain tile
            foreach (string neighbor in allTerrains[startTerrain])
            {
                CreateTerrain(neighbor);
            }
        }

        ///<summary>
        /// Function to create the terrain tile game object, generate the terrain, and set it in the correct position.
        ///</summary>
        ///<param name"terrainName">Name of the terrain to generate</param>
        ///<returns>The DynamicTerrainTile component added to the new game object of the terrain</returns>
        public static DynamicTerrainTile CreateTerrain(string terrainName)
        {
            string outputPath = Path.GetDirectoryName(DynamicTerrainStreamingManager.filepath);
            string output = outputPath + @"\" + terrainName + ".raw";
            // if(File.Exists(output))
            // {
            //     File.Delete(output);
            // }
            
            Match m1 = Regex.Match(terrainName, @"Terrain_(\d+)_(\d+)");
            int i = Convert.ToInt32(m1.Groups[1].Value);
            int j = Convert.ToInt32(m1.Groups[2].Value);
            
            GameObject obj = GameObject.Find("Terrain_" + i.ToString() + "_" + j.ToString());

            if(obj != null)
            {
                return obj.GetComponent<DynamicTerrainTile>();
            }

            GeospatialExt.CropDEM(Path.GetExtension(filepath).EndsWith("raw") ? filepath : GeospatialExt.DEMtoRAW(filepath), output, i*tileSizeX, j*tileSizeY, tileSizeX, tileSizeY);

            Terrain terrain = TerrainLoadingManager.GenerateTerrain(output, height);
            terrain.transform.position = new Vector3(i*tileSizeY*5, 0, j*tileSizeX*5);

            terrain.gameObject.name = terrainName;

            generatedTerrains.Add(terrainName, terrain);

            return terrain.gameObject.AddComponent<DynamicTerrainTile>();
        }
}
}

