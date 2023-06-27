// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.Utilities.Math;

namespace GOV.NASA.GSFC.XR.Utilities.Color
{
    /// <remarks>
    /// History:
    /// 30 Jan 2023: Created
    /// </remarks>
    ///
    /// <summary>
    /// ColorUtil
    ///
    /// Color utility functions
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class ColorUtil
    {
        /// <summary>
        /// Compares the RGBA of two colors to determine if they are considered equal
        /// </summary>
        /// <param name="a">The first <code>Color32</code></param>
        /// <param name="b">The second <code>Color32</code></param>
        /// <returns>An indicator that the supplied colors are equal</returns>
        public static bool ColorRGBAEquals(UnityEngine.Color32 a, UnityEngine.Color b)
        {
            UnityEngine.Color32 c = b;
            return (
                (a.r == c.r) &&
                (a.g == c.g) &&
                (a.b == c.b) &&
                (a.a == c.a));
        }

        /// <summary>
        /// Compares the RGBA of two colors to determine if they are considered equal
        /// </summary>
        /// <param name="a">The first <code>Color</code></param>
        /// <param name="b">The second <code>Color</code></param>
        /// <returns>An indicator that the supplied colors are equal</returns>
        public static bool ColorRGBAEquals(UnityEngine.Color a, UnityEngine.Color b)
        {
            // Unity colors are stored as floats, so normal comparison doesn't work
            return (
                MathUtil.ApproximatelyEquals(a.r, b.r) &&
                MathUtil.ApproximatelyEquals(a.g, b.g) &&
                MathUtil.ApproximatelyEquals(a.b, b.b) &&
                MathUtil.ApproximatelyEquals(a.a, b.a));
        }
    }
}
