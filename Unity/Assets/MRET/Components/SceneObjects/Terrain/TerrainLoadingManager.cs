// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
#if MRET_EXTENSION_TERRAINTOOLS
using UnityEngine.Experimental.TerrainAPI;
#endif
using GOV.NASA.GSFC.XR.MRET.Extensions.Geospatial;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// TerrainLoadingManager is a class that provides
    /// top-level control of terrain creation and streaming from DEM files in MRET.
    /// Author: Molly T. Goldstein
    /// </summary>
    public class TerrainLoadingManager : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TerrainLoadingManager);

        /// <summary>
        /// Enum that defines the different options for generating terrain
        ///</summary>
        public enum State
        {
            /// <summary>
            ///Generate one terrain tile from a DEM file
            ///</summary>
            GenerateOneTile = 0,

            /// <summary>
            ///Crop the DEM file and generate one terrain tile from the cropped file (use pixel coordinates for center point)
            ///</summary>
            CropAndGenerateOneTilePixelCenter = 1,

            /// <summary>
            ///Crop the DEM file and generate one terrain tile from the cropped file (use lat/lon coordinates for center point)
            ///</summary>
            CropAndGenerateOneTileLatLonCenter = 2,

            /// <summary>
            ///Stream the terrain from one DEM file
            ///</summary>
            StreamFile = 3
        }

        /// <summary>
        /// Current state/mode
        ///</summary>
        public State state = State.GenerateOneTile;

        /// <summary>
        /// Overwrite any existing copies of files for cropping and for streaming
        ///</summary>
        public bool Overwrite = true;

        /// <summary>
        /// Absolute path to the DEM file (i.e., "C:\Users\mtgoldst\Documents\Terrain_Data\Apollo15\Test\NAC_DTM_APOLLO15_2_CROPPED_C.raw")
        ///</summary>
        public string filepath;
        // public string filepath = @"C:\Users\mtgoldst\Documents\Terrain_Data\Apollo15\Test\NAC_DTM_APOLLO15_2_CROPPED_C.raw"; 
        /// public string filepath = @"C:\Users\mtgoldst\Documents\Terrain_Data\LDEM\Test\LDEM_875S_5M.TIF";

        /// <summary>
        /// Width to crop the DEM file to
        ///</summary>
        public int cropSizeX;

        /// <summary>
        /// Width to crop the DEM file to
        ///</summary>
        public int cropSizeY;

        /// <summary>
        /// Center x coordinate (pixel or lat) to crop from
        ///</summary>
        public double centerX;

        /// <summary>
        /// Center y coordinate (pixel or lon) to crop from
        ///</summary>
        public double centerY;

		/// <summary>
		/// Path to a material file for terrain generation
		///</summary>
		public string materialPath;

        /// (start) For cropping with latitude/longitude coordinates

        /// <summary>
        /// Since the center point is known in lat/lon, these four points should also be known by the user
        ///</summary>
        public double westernmostLongitude;
        public double easternmostLongitude;
        
        public double minimumLatitude;
        public double maximumLatitude;
        /// (end) For cropping with latitude/longitude coordinates

        ///<summary>
        /// Width of tiles to stream
        ///</summary>
        public int tileSizeX;
        
        ///<summary>
        ///Length of tiles to stream
        ///</summary>
        public int tileSizeY;

        ///<summary>
        /// The list of all terrain tiles to stream in the scene
        ///</summary>
        public static List<Terrain> allTerrainTiles = new List<Terrain>();

        ///<summary>
        /// The character controller object in the scene
        ///</summary>
        public static GameObject character;
        
        ///<summary>
        /// For states that are not streaming, save the generated terrain to this variable for repositioning in MRETUpdate()
        ///</summary>
        private Terrain terrain;

        ///<summary>
        /// Flag to indicate if the stream has started yet.
        ///</summary>
        private bool start = true;

        ///<summary>
        /// Entry point for TerrainLoadingManager. Checks the state/mode of set by the user and generates (and crops if applicable) accordingly.
        ///</summary>
        protected override void MRETStart()
        {
            base.MRETStart();

            GeospatialExt.GeoInfo info = new GeospatialExt.GeoInfo(filepath); // Retrieve information about the DEM file from `GeoInfo -hist <filepath>`
            string outputPath = Path.GetDirectoryName(filepath); // output path of any output will be in the same directory as the input path

            if (!File.Exists(filepath))
            {
                throw new Exception("File does not exist. Please make sure the filepath is correct.");
            }

            switch (state)
            {
                case State.GenerateOneTile:
                    // All DEM files need to be in the "raw" format before Unity can generate the terrain
                    if (!Path.GetExtension(filepath).EndsWith("raw"))
                    {
                        terrain = GenerateTerrain(GeospatialExt.DEMtoRAW(filepath));
                        terrain.transform.SetParent(this.transform);
                    }
                    else
                    {
                        terrain = GenerateTerrain(filepath);
                        terrain.transform.SetParent(this.transform);
                    }
                    break;

                case State.CropAndGenerateOneTilePixelCenter:
                    string output1 = outputPath + @"\" + Path.GetFileNameWithoutExtension(filepath) + "-" + cropSizeX + "x" + cropSizeY + ".raw";
                    Debug.Log(output1);
                    // If the output file does not exist or does exist and the overwrite option has been selected, (re)generate the cropped DEM file
                    if (!File.Exists(output1) || (File.Exists(output1) && Overwrite))
                    {
                        if (File.Exists(output1))
                        {
                            File.Delete(output1);
                        }
                        GeospatialExt.CropDEM(filepath, output1, (int)centerX - (cropSizeX / 2), (int)centerY - (cropSizeY / 2), cropSizeX, cropSizeY);
                    }
                    terrain = GenerateTerrain(output1, info.Range());
                    terrain.transform.SetParent(this.transform);
                    break;

                case State.CropAndGenerateOneTileLatLonCenter:
                    if (centerX < westernmostLongitude || centerX > easternmostLongitude || centerY < minimumLatitude || centerY > maximumLatitude)
                    {
                        throw new Exception("Lat/Lon center coordinate exceeds provided lat/lon bounds. Please enter a point within the given bounds.");
                    }
                    string output2 = outputPath + @"\" + Path.GetFileNameWithoutExtension(filepath) + "-" + cropSizeX + "x" + cropSizeY + ".raw";

                    if (!File.Exists(output2) || (File.Exists(output2) && Overwrite))
                    {
                        if (File.Exists(output2))
                        {
                            File.Delete(output2);
                        }

                        double[] lon = Conversions.Linspace(westernmostLongitude, easternmostLongitude, info.Width());
                        double[] lat = Conversions.Linspace(minimumLatitude, maximumLatitude, info.Length());
                        GeospatialExt.CropDEM(filepath, output2, Conversions.FindPixel(lon, centerX) - (cropSizeX / 2), Conversions.FindPixel(lat, centerY) - (cropSizeY / 2), cropSizeX, cropSizeY);
                    }
                    terrain = GenerateTerrain(output2, info.Range());
                    terrain.transform.SetParent(this.transform);
                    break;

                case State.StreamFile:

                    Vector2 dims = CropAndGenerateTilesForStream(info, outputPath);
                    TerrainStreamingManager.SetNeighbors((int)dims.x, (int)dims.y);
                    break;
            }
        }

        /// <summary>
        /// If the state is not streaming, then move the terrain to be centered under the character controller object in the scene.
        /// </summary>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (start && character != null)
            {
                switch (state)
                {
                    case State.GenerateOneTile:
                    case State.CropAndGenerateOneTilePixelCenter:
                    case State.CropAndGenerateOneTileLatLonCenter:
                        float x = character.transform.position.x;
                        float z = character.transform.position.z;
                        terrain.transform.position = new Vector3(x - terrain.terrainData.size.x / 2, terrain.transform.position.y, z - terrain.terrainData.size.z / 2);
                        break;
                }
                start = false;
            }
        }

        /// <summary>
        /// CropAndGenerateTilesForStream generates the tiles to stream a DEM file by cropping the
        /// file into smaller raw files for tiling.
        /// </summary>
        /// <param name="info">Type GDALManager.GeoInfo. Contains size information of DEM file.</param>
        /// <param name="outputPath">The output directory (absolute path) of the generated tiles</param>
        /// <returns>The dimensions of the terrain (ie, 4 tiles by 4 tiles)</returns>
        private Vector2 CropAndGenerateTilesForStream(GeospatialExt.GeoInfo info, string outputPath)
        {
            int fileSizeX = info.Width();
            int fileSizeY = info.Length();
            // For testing:
            // tileSizeX = 512;
            // tileSizeY = 512;

            if (tileSizeX > 4096 || tileSizeY > 4096 || !IsPowerOfTwo(tileSizeX) || !IsPowerOfTwo(tileSizeY)
                || tileSizeX > info.Width() || tileSizeY > info.Length() || tileSizeX <= 0 || tileSizeY <= 0)
            {
                throw new Exception("Tile parameters (TerrainLoadingManager.tileSizeX and/or TerrainLoadingManager.tileSizeY) incorrect dimensions. Please refer to the README for list of correct dimensions to use.");
            }

            // Calculate the dimensions of the overall terrain (i.e., 4 tiles by 4 tiles)
            int tilesNumX = (int)Math.Floor((decimal)fileSizeX / tileSizeX);
            int tilesNumY = (int)Math.Floor((decimal)fileSizeY / tileSizeY);

            int height = info.Range();

            // Crop (if needed) and generate the terrain tiles in the Unity scene
            for (int i = 0; i < tilesNumX; i++)
            {
                for (int j = 0; j < tilesNumY; j++)
                {
                    string output = outputPath + @"\" + i.ToString() + "-" + j.ToString() + ".raw";
                    if (!File.Exists(output) || (File.Exists(output) && Overwrite))
                    {
                        if (File.Exists(output))
                        {
                            File.Delete(output);
                        }
                        GeospatialExt.CropDEM(Path.GetExtension(filepath).EndsWith("raw") ? filepath : GeospatialExt.DEMtoRAW(filepath), output, i * tileSizeX, j * tileSizeY, tileSizeX, tileSizeY);
                    }

                    Terrain terrain = GenerateTerrain(output, height);
                    terrain.transform.position = new Vector3(i * tileSizeY * 5, 0, j * tileSizeX * 5);

                    terrain.gameObject.name = "Terrain_" + i.ToString() + "_" + j.ToString();
                    terrain.transform.SetParent(this.transform);
                    allTerrainTiles.Add(terrain);
                }
            }
            return new Vector2(tilesNumX, tilesNumY);
        }

        private static bool IsPowerOfTwo(int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        /// <summary>
    	/// Generates the terrain in Unity using the TerrainTools package (dependency) Heightmap class
		/// </summary>
        /// <param name="path">The filepath of the terrain to generate</param>
        /// <param name="remap">The flag to remap the terrain (default: true)</param>
        /// <param name="height">The height range of the heightmap (default: 0)</param>
        /// <param name="materialPath">The material path for the terrain data (default: "")</param>
        /// <returns></returns>
        public static Terrain GenerateTerrain(string path, int _height=0, string materialPath="", bool remap=true)
		{
            GeospatialExt.GeoInfo info = new GeospatialExt.GeoInfo(path);
            if (info.Width() > 4096 || info.Length() > 4096 || !IsPowerOfTwo(info.Width()) || !IsPowerOfTwo(info.Length()))
            {
                throw new Exception("File does not have the correct dimensions. Please refer to the README and ensure either the file dimensions, the crop dimensions, or the tile dimensions are correct. If using the first mode of operation, consider using one of the cropping modes.");
            }
            int height = _height > 0 ? _height : info.Range(); //set the remap height

            // Read the raw bytes from the file
            byte[] rawData = File.ReadAllBytes(path);

            // Create the terrain object and its terrain data
            TerrainData terrainData = new TerrainData();
            Terrain terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();

#if MRET_EXTENSION_TERRAINTOOLS
            // Create the Heightmap object
            Heightmap map = new Heightmap(rawData, Heightmap.Flip.None);

            // Remap the Heightmap and apply it to the terrain
            if (remap)
            {
                // From TerrainTools:
                // var remap = (m_Settings.HeightmapRemapMax - m_Settings.HeightmapRemapMin) / m_Settings.TerrainHeight;
                // var baseLevel = m_Settings.HeightmapRemapMin / m_Settings.TerrainHeight;

                // var _remap = 65536 / height; //STATISTICS_MAXIMUM - STATISTICS_MINIMUM for height
                var baseLevel = 0; // STATISTICS_MINIMUM / height;

                // regex for the size scale (*5)
                Heightmap tileMap = new Heightmap(map, Vector2Int.zero, new Vector2Int(info.Width(), info.Length()), 65536 / height, baseLevel); //Dependency here on TerrainToolbox
                                                                                                                                                 // Heightmap tileMap = new Heightmap(map, Vector2Int.zero, new Vector2Int(512, 512), (65536/2427), baseLevel); //Dependency here on TerrainToolbox
                tileMap.ApplyTo(terrain);
            }
            else
            {
                map.ApplyTo(terrain);
            }

            // Set the correct size of the terrain
            terrainData.size = new Vector3(info.Width() * info.PixelSize(), height, info.Length() * info.PixelSize());
#endif

            return terrain;
        }

        // This function was the original attempt to generate the terrain. It is no longer in use as it created a
        // remap issue and did not display an accurate height for the terrain.
        public static void GenerateTerrain1(string path, string materialPath)
		{
            string outputStr;

            GeospatialExt.GeoInfo info = new GeospatialExt.GeoInfo(path);

            string width = info.Width().ToString();
            string length = info.Length().ToString();

            var fi = new FileInfo(path);

            int hfSamples = (int)fi.Length / 2;
            int hfWidth = 0;
            int hfHeight = 0;

            if (hfHeight <= 0)
            {
                if (hfWidth > 0)
                    hfHeight = hfSamples / hfWidth;
                else
                    hfHeight = hfWidth = Mathf.CeilToInt(Mathf.Sqrt(hfSamples));
            }
            else
            {
                if (hfWidth <= 0)
                    hfWidth = hfSamples / hfHeight;
            }

            int size = hfWidth;

            int tOffX = 0;
            int tOffY = 0;

            if (tOffX < 0 || tOffY < 0 || (size - 1) * tOffX > hfWidth || (size - 1) * tOffY > hfHeight)
            {
                Debug.LogError("terrainTile (" + tOffX + "," + tOffY + ") of size " + size + "x" + size + " "
                        + "is outside heightFile size " + hfWidth + "x" + hfHeight);
                return; // We don't want to Seek/Read outside file bounds.
            }

            // // Stitching reuses right/bottom edges.
            tOffX = (size - 1) * tOffX;
            tOffY = (size - 1) * tOffY;

            var bpp = 2; // only word formats are currently supported

            var fs = fi.OpenRead();
            var b = new byte[size * size * bpp];
            fs.Seek((tOffX + tOffY * hfWidth) * bpp, SeekOrigin.Current);

            if (size == hfWidth)
            {
                fs.Read(b, 0, size * size * bpp);
            }
            else
            {
                for (int y = 0; y < size; ++y)
                {
                    fs.Read(b, y * size * bpp, size * bpp);
                    if (y + 1 < size)
                        fs.Seek((hfWidth - size) * bpp, SeekOrigin.Current);
                }
            }
            fs.Close();

            float[,] h = new float[size, size];
            int i = 0;

            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    h[size - 1 - x, y] = (b[i++] + b[i++] * 256.0F * 2F) / 65535F;
                }
            }

            // float[,] h2 = h;
            // Array.Sort(h2);

            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    Debug.Log(h[size - 1 - x, y]);
                }
            }

            TerrainData terrainData = new TerrainData();
            //TODO: Scale down size from one larger than the resolution
            terrainData.heightmapResolution = size - 1;

            //TODO: Scale down size from one larger than the resolution
            terrainData.size = new Vector3(Convert.ToInt32(width) * 5, 2470, Convert.ToInt32(length) * 5);
            terrainData.SetHeights(0, 0, h);
            Terrain terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();

            if (!string.IsNullOrEmpty(materialPath))
            {
                Texture2D texture2D = new Texture2D(513, 513);
                // texture2D.name = Path.GetFileNameWithoutExtension(materialPath);

                // Load file meta data with FileInfo
                FileInfo fileInfo = new FileInfo(materialPath);

                // The byte[] to save the data in
                byte[] data = new byte[fileInfo.Length];

                // Load a filestream and put its content into the byte[]
                using (FileStream f = fileInfo.OpenRead())
                {
                    f.Read(data, 0, data.Length);
                }

                texture2D.LoadImage(data);

                Material material = new Material(Shader.Find("HDRP/TerrainLit"));
                terrain.materialTemplate = material;

                TerrainLayer[] terrainLayer;
                terrainLayer = new TerrainLayer[1];
                int terrainIndex = 0;
                terrainLayer[terrainIndex] = new TerrainLayer();
                terrainLayer[terrainIndex].diffuseTexture = texture2D;
                terrainLayer[terrainIndex].diffuseTexture.Apply(true);
                // apply textures back into the terrain data
                terrainData.terrainLayers = terrainLayer;
            }
        }
    }
}