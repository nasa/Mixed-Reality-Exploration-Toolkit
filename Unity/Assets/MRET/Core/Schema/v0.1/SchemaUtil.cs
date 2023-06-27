// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_1
{
    /// <remarks>
    /// History:
    /// 09 Sep 2021: Created (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// SchemaUtil
    /// 
    /// A utility class containing a collection of functions for deserializing and
    /// serializing Unity classes into schema v0.1 types.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class SchemaUtil
    {
        public static string ClassName => nameof(SchemaUtil);

        public static Vector3 DeserializeVector3(Vector3Type input)
        {
            if (input == null) return new Vector3();
            return new Vector3(input.X, input.Y, input.Z);
        }

        public static Vector3[] DeserializeVector3Array(Vector3Type[] input)
        {
            List<Vector3> outVec = new List<Vector3>();
            if (input != null)
            {
                foreach (Vector3Type vec in input)
                {
                    if (vec != null)
                    {
                        outVec.Add(DeserializeVector3(vec));
                    }
                }
            }

            return outVec.ToArray();
        }

    }
}
