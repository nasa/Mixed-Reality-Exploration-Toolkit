// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.Utilities.Math
{
    /// <remarks>
    /// History:
    /// 12 Oct 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// MathUtil
    ///
    /// Math utility functions
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class MathUtil
	{
        /// <summary>
        /// Normalizes an angle to the range of [-180,180]
        /// </summary>
        /// <param name="angle">The angle in degrees</param>
        /// <returns>The normalized angle in the range [-180,180]</returns>
        public static double NormalizeAngle180(double angle)
        {
            return (angle + 540) % 360 - 180;
        }

        /// <summary>
        /// Normalizes an angle to the range of [0,360]
        /// </summary>
        /// <param name="angle">The angle in degrees</param>
        /// <returns>The normalized angle in the range [0,360]</returns>
        public static double NormalizeAngle360(double angle)
        {
            return (angle + 360) % 360;
        }

        /// <summary>
        /// Compares two floats to determine approximate equality
        /// </summary>
        /// <param name="a">The first float</param>
        /// <param name="b">The second float</param>
        /// <param name="epsilon">The acceptable error. Default is float.Epsilon</param>
        /// <returns>An indicator that the supplied floats are approximately equal</returns>
        public static bool ApproximatelyEquals(float a, float b, float epsilon = float.Epsilon)
        {
            const float floatNormal = (1 << 23) * float.Epsilon;
            float absA = System.Math.Abs(a);
            float absB = System.Math.Abs(b);
            float diff = System.Math.Abs(a - b);

            if (a == b)
            {
                // Shortcut, handles infinities
                return true;
            }

            if (a == 0.0f || b == 0.0f || diff < floatNormal)
            {
                // a or b is zero, or both are extremely close to it.
                // relative error is less meaningful here
                return diff < (epsilon * floatNormal);
            }

            // Use relative error
            return diff / System.Math.Min((absA + absB), float.MaxValue) < epsilon;
        }

        /// <summary>
        /// Compares two three dimentional vectors to determine approximate equality
        /// </summary>
        /// <param name="vector1">The first <code>Vector3</code></param>
        /// <param name="vector2">The second <code>Vector3</code></param>
        /// <param name="epsilon">The acceptable error. Default is float.Epsilon</param>
        /// <returns>An indicator that the supplied vectors are approximately equal</returns>
        public static bool ApproximatelyEquals(UnityEngine.Vector3 vector1, UnityEngine.Vector3 vector2, float epsilon = float.Epsilon)
        {
            return
                ApproximatelyEquals(vector1.x, vector2.x, epsilon) &&
                ApproximatelyEquals(vector1.y, vector2.y, epsilon) &&
                ApproximatelyEquals(vector1.z, vector2.z, epsilon);
        }

        /// <summary>
        /// Compares two quaternions to determine approximate equality
        /// </summary>
        /// <param name="q1">The first <code>Quaternion</code></param>
        /// <param name="q2">The second <code>Quaternion</code></param>
        /// <param name="epsilon">The acceptable error. Default is float.Epsilon</param>
        /// <returns>An indicator that the supplied quaternions are approximately equal</returns>
        public static bool ApproximatelyEquals(UnityEngine.Quaternion q1, UnityEngine.Quaternion q2, float epsilon = float.Epsilon)
        {
            return
                ApproximatelyEquals(q1.x, q2.x, epsilon) &&
                ApproximatelyEquals(q1.y, q2.y, epsilon) &&
                ApproximatelyEquals(q1.z, q2.z, epsilon) &&
                ApproximatelyEquals(q1.w, q2.w, epsilon);
        }

        /// <summary>
        /// Normalizes the supplied value to a normalized range of values.<br>
        /// </summary>
        /// <param name="value">The value to be normalized</param>
        /// <param name="valueRangeMin">The allowed minimum of the value</param>
        /// <param name="valueRangeMax">The allowed maximum of the value</param>
        /// <param name="normalRangeMin">The normalized minimum</param>
        /// <param name="normalRangeMax">The normalized maximum</param>
        /// <returns>The normalized value</returns>
        public static double Normalize(double value,
            double valueRangeMin, double valueRangeMax,
            double normalRangeMin, double normalRangeMax)
        {
            // Make sure the supplied value is in the supplied range
            if (value > valueRangeMax)
            {
                value = valueRangeMax;
            }
            else if (value < valueRangeMin)
            {
                value = valueRangeMin;
            }

            // Normalize
            double valueSize = valueRangeMax - valueRangeMin;
            double normalSize = normalRangeMax - normalRangeMin;
            return (normalRangeMin + (((value - valueRangeMin) * normalSize)) / valueSize);
        }
    }
}
