// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.Utilities
{
    public class TypeConversion
    {
        public static string Vector3ToCSV(Vector3[] input)
        {
            string output = "";
            foreach (Vector3 v3 in input)
            {
                output = output + v3.x + "," + v3.y + "," + v3.z + ";";
            }

            return output;
        }

        public static Vector3[] CSVToVector3(string input)
        {
            List<Vector3> output = new List<Vector3>();
            foreach (string coordPoint in input.Split(new char[] { ';' }))
            {
                string[] coordVals = coordPoint.Split(new char[] { ',' });
                if (coordVals.Length != 3)
                {
                    continue;
                }
                else
                {
                    output.Add(new Vector3()
                    {
                        x = float.Parse(coordVals[0]),
                        y = float.Parse(coordVals[1]),
                        z = float.Parse(coordVals[2])
                    });
                }
            }

            return output.ToArray();
        }

    }
}