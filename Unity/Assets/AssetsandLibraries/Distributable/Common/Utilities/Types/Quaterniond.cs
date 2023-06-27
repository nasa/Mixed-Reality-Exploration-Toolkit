﻿// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.Utilities
{
    public class Quaterniond
    {
        public double x, y, z, w;

        public Quaterniond(double X, double Y, double Z, double W)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion()
            {
                x = (float)x,
                y = (float)y,
                z = (float)z,
                w = (float)w
            };
        }

        public static Quaterniond operator +(Quaterniond a, Quaterniond b)
        {
            return new Quaterniond(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Quaterniond operator -(Quaterniond a, Quaterniond b)
        {
            return new Quaterniond(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Quaterniond operator *(Quaterniond a, double b)
        {
            return new Quaterniond(a.x * b, a.y * b, a.z * b, a.w * b);
        }
    }
}