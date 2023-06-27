// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.Utilities.Statistics
{
    /**
     * Abstract container class containing statistics functions.
     * 
     * @author Jeffrey Hosler
     */
    public abstract class StatisticsUtil
    {
        /**
         * Box Muller method to generate random normal values
         * 
         * @param mean The mean μ of the normal distribution
         * @param stdDeviation The standard deviation σ of the normal distributed
         * 
         * @return The normal distribution with mean μ and standard deviation σ
         */
        public static float BoxMuller(float mean, float stdDeviation)
        {
            // Must be two different random values
            float u1 = Random.value;
            float u2 = Random.value;

            // Return the random normal value
            return (float) (System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2) * stdDeviation + mean);
        }
    }
}
