using UnityEngine;

namespace GSFC.ARVR.UTILITIES
{
    public class Vector3d
    {
        public double x, y, z;

        public Vector3d(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3()
            {
                x = (float)x,
                y = (float)y,
                z = (float)z
            };
        }

        public static Vector3d operator+ (Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3d operator- (Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3d operator* (Vector3d a, double b)
        {
            return new Vector3d(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3d operator /(Vector3d a, double b)
        {
            return new Vector3d(a.x / b, a.y / b, a.z / b);
        }
    }
}