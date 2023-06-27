// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using System;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    ///<summary>
    ///Conversons class supports the conversions of lat/lon coordinates to pixel coordinates
    /// Author: Molly T. Goldstein
    ///</summary>
    public class Conversions
    {
        /// <summary>
        /// Linspace generates a vector of values linearly spaced between two endpoints
        /// </summary>
        /// <param name="start">Starting value in vector</param>
        /// <param name="end">Ending value in vector</param>
        /// <param name="size">Size of vector</param>
        /// <returns>Vector length "size" linearly spaced between "start" and "end"</returns>
        public static double[] Linspace(double start, double end, int size)
        {
            double x = (end - start)/ (double) size;
            
            double[] ret = new double[size];

            for(int i = 0; i < size; i++)
            {
                ret[i] = Math.Round(start, 4);
                start += x;
            }
            ret[size - 1] = end;

            return ret;
        }

        /// <summary>
        /// Find the pixel coordinate equivalent of latitude or longitude point
        /// </summary>
        /// <param name="vector">Linspace vector</param>
        /// <param name="val">Value to find index of</param>
        /// <returns>pixel coordinate equivalent of latitude or longitude point</returns>
        public static int FindPixel(double[] vector, double val)
        {
            List<double> list = new List<double>(vector);

            if (list.IndexOf(Math.Round(val, 4)) >= 0)
            {
                return list.IndexOf(Math.Round(val, 4));
            }

            // Find the value closest to val if val is not in the list

            double temp = Math.Round(val, 4);

            int lowc = 0;
            int highc = 0;

            int lower = 0;
            int higher = 0;

            while (list.IndexOf(Math.Round(temp, 4)) == -1)
            {
                highc++;
                temp += 0.0001;

            }

            higher = list.IndexOf(Math.Round(temp, 4));

            temp = Math.Round(val, 4);

            while (list.IndexOf(Math.Round(temp, 4)) == -1)
            {
                lowc++;
                temp -= 0.0001;
            }

            lower = list.IndexOf(Math.Round(temp, 4));

            if (highc < lowc)
            {
                return higher;
            }
            else if (lowc < highc)
            {
                return lower;
            }

            return -1;
        }
    }
}
