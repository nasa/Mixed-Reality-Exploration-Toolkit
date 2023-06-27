// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
#if MRET_EXTENSION_GDAL
using OSGeo.GDAL;
#endif
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace GOV.NASA.GSFC.XR.MRET.Extensions.Geospatial
{    
	/// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
	/// <summary>
	///  This class contains all functions using the GDAL C# interface (the machine running Unity must have Gdal installed as a dependency)
    ///  More information on GDAL can be found here https://gdal.org/index.html
    ///  The interface with GDAL is based on the C# bindings (https://gdal.org/api/csharp/index.html#:~:text=The%20GDAL%20project%20%28primarily%20Tamas%20Szekeres%29%20maintains%20SWIG,also%20usable%20from%20other.NET%20languages%2C%20such%20as%20VB.Net.)
    ///  
    ///  Author: Molly T. Goldstein
	///</summary>
    public static class GeospatialExt
    {
		/// <summary>
		/// This struct uses the `gdalinfo` command (https://gdal.org/programs/gdalinfo.html) to collect information on the DEM file using reg. expressions
		///</summary>
        public struct GeoInfo
        {
            /// <summary>
            ///width of the DEM file
            ///</summary>
            int width;

            /// <summary>
            ///length of the DEM file
            ///</summary>
            int length;

            /// <summary>
            ///maximum height of the file
            ///</summary>
            int max;

            /// <summary>
            ///minimum height of the file
            ///</summary>
            int min;

            /// <summary>
            /// the meter-to-pixel ratio (ie, pixel size is 5, then each pixel is ~ 5 meters)
            ///</summary>
            int pixelSize;
            
			/// <summary>
			/// GdalInfo constructor that takes in the DEM filepath and sets the struct fields from the gdalinfo command
			///</summary>
			///<param name="path">Path of the file to read information from<param>
			public GeoInfo(string path)
            {
#if MRET_EXTENSION_GDAL
                string outputStr;

                Gdal.AllRegister();

                using(Dataset output = Gdal.Open(path, Access.GA_ReadOnly))
                {
                    outputStr = Gdal.GDALInfo(output,  new GDALInfoOptions(new[]{"-hist"}));
                    output.FlushCache();
                    output.Dispose(); 
                }
				// Regex on the output of the gdalinfo command
                Match m1 = Regex.Match(outputStr, @"Size is (\d+), (\d+)");
                int width = Convert.ToInt32(m1.Groups[1].Value);
                int length = Convert.ToInt32(m1.Groups[2].Value);
                Match m2 = Regex.Match(outputStr, @"STATISTICS_MAXIMUM=(-?\d+.?\d*)");
                int max =  Convert.ToInt32(m2.Groups[1].Value);
                Match m3 = Regex.Match(outputStr, @"STATISTICS_MINIMUM=(-?\d+.?\d*)");
                int min = Convert.ToInt32(m3.Groups[1].Value);
                Match m4 = Regex.Match(outputStr, @"Pixel Size = \((-?\d+.?\d*),-?\d+.?\d*\)");
                double val = Convert.ToDouble(m4.Groups[1].Value);
                int pixelSize = (int) Math.Ceiling(val);

                this.width = width;
                this.length = length;
                this.max = max;
                this.min = min;
                this.pixelSize = pixelSize;
#else
                this.width = 0;
                this.length = 0;
                this.max = 0;
                this.min = 0;
                this.pixelSize = 0;
#endif
            }

            /// <summary>
            ///width of the DEM file
            ///</summary>
            public int Width()
            {
                return this.width;
            }

            /// <summary>
            ///length of the DEM file
            ///</summary>
			public int Length()
            {
                return this.length;
            }

			/// <summary>
            /// Range of the height of the DEM file
            ///</summary>
			/// <returns>
			/// this.max - this.min;
			///</returns>
            public int Range()
            {
                return this.max - this.min;
            }

            /// <summary>
            /// the meter-to-pixel ratio (ie, pixel size is 5, then each pixel is ~ 5 meters)
            ///</summary>
            public int PixelSize()
            {
                return this.pixelSize;
            }

        };

        /// <summary>
        /// Crop a DEM file into a smaller DEM file
        ///</summary>
        ///<param name="inputFile">Input file path</param>
        ///<param name="outputFilename">Output file path</param>
        ///<param name="xoff">Upper left corner x coordinate of cropped DEM on original DEM</param>
        ///<param name="yoff">Upper left corner y coordinate of cropped DEM on original DEM</param>
        ///<param name="xsize">Width of cropped DEM</param>
        ///<param name="ysize">Length of cropped DEM</param>
        ///<returns>Output file name of the cropped DEM</returns>
        public static string CropDEM(string inputFile, string outputFilename, int xoff, int yoff, int xsize, int ysize)
        {
#if MRET_EXTENSION_GDAL
            Gdal.AllRegister();

            if (File.Exists(outputFilename))
            {
                return outputFilename;
            }

            // Open the file to be cropped
            using (Dataset input = Gdal.Open(inputFile, Access.GA_ReadOnly))
            {
                GDALTranslateOptions translateOptions = new GDALTranslateOptions(new[] { "-of", "ENVI", "-srcwin", xoff.ToString(), yoff.ToString(), xsize.ToString(), ysize.ToString() });

                Dataset output = Gdal.wrapper_GDALTranslate(outputFilename, input, translateOptions, null, null);

                input.FlushCache();
                input.Dispose();
                output.FlushCache();
                output.Dispose();
            }
#else
            Debug.LogWarning("GDAL is unavailable");
#endif

            return outputFilename;
        }

        /// <summary>
        /// Convert the DEM file (.dem, .tif, .TIF, etc.) to a raw DEM file
        /// </summary>
        /// <param name="inputFile">Path of DEM input file</param>
        /// <returns>The output file name (including full path) of converted RAW DEM</returns>
        public static string DEMtoRAW(string inputFile)
		{
            //TODO: Handle multiple different file types

            string outputFilename = Path.ChangeExtension(inputFile, ".raw");

            if (File.Exists(outputFilename))
            {
                return outputFilename;
            }

#if MRET_EXTENSION_GDAL
            Gdal.AllRegister();

            // Open the file to be converted
            using (Dataset input = Gdal.Open(inputFile, Access.GA_ReadOnly))
            {
                string inputStr = Gdal.GDALInfo(input, new GDALInfoOptions(new[] { "-hist" }));

                Match m1 = Regex.Match(inputStr, @"Size is (\d+), (\d+)");
                Debug.Log(inputStr);
                string width = m1.Groups[1].Value;
                string length = m1.Groups[2].Value;
                Match m2 = Regex.Match(inputStr, @"STATISTICS_MAXIMUM=(-?\d+.?\d*)");
                double max = Convert.ToDouble(m2.Groups[1].Value);
                Match m3 = Regex.Match(inputStr, @"STATISTICS_MINIMUM=(-?\d+.?\d*)");
                double min = Convert.ToDouble(m3.Groups[1].Value);
                double range = max - min;

                GDALTranslateOptions translateOptions = new GDALTranslateOptions(new[] { "-ot", "Uint16", "-scale", min.ToString(), max.ToString(), "0", range.ToString(), "-of", "ENVI", "-outsize", width.ToString(), length.ToString() });

                // Create the new output file from the input data and translate options defined above
                Dataset output = Gdal.wrapper_GDALTranslate(outputFilename, input, translateOptions, null, null);

                input.FlushCache();
                input.Dispose();
                output.FlushCache();
                output.Dispose();
            }
#else
            Debug.LogWarning("GDAL is unavailable");
#endif

            return outputFilename;
        }
    }
}
