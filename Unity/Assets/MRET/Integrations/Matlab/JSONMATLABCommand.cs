// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using Newtonsoft.Json;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.Matlab
{
    public class JSONMATLABCommand
    {
        public int cmd_id;
        public object param;
        public int seq_num;

        public static JSONMATLABCommand RobotIKCommand(int seqNum, Vector3 pos, Quaternion orient, string robot, string tool, float[] guess = null, float? sew = null, Vector3? elbow = null)
        {
            if (seqNum < 0 || pos == null || orient == null || robot == null
                || robot == "")
            {
                return null;
            }

            RobotIKCommandParameters ikParams = new RobotIKCommandParameters()
            {
                guess = guess,
                orientation = new float[] { orient.x, orient.y, orient.z, orient.w },
                position = new float[] { pos.x, pos.y, pos.z },
                robot = robot,
                sew = sew,
                elbow = elbow.HasValue ? new float[] { elbow.Value.x, elbow.Value.y, elbow.Value.z } : null,
                tool = tool
            };

            return new JSONMATLABCommand()
            {
                cmd_id = 101,
                param = ikParams,
                seq_num = seqNum
            };
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static JSONMATLABCommand FromJSON(string json)
        {
            return JsonConvert.DeserializeObject<JSONMATLABCommand>(json);
        }

        public bool ShouldSerializeparam()
        {
            return param != null;
        }
    }

    public class RobotIKCommandParameters
    {
        public string robot;
        public string tool;
        public float[] position;
        public float[] orientation;
        public string orientation_type = "quaternion";
        public string orientation_handedness = "left";
        public float? sew;
        public float[] guess;
        public float[] elbow;
        public string position_units = "m";
        public string orientation_units = "rad";

        public bool ShouldSerializesew()
        {
            return sew != null;
        }

        public bool ShouldSerializeguess()
        {
            return guess != null;
        }

        public bool ShouldSerializeelbow()
        {
            return elbow != null;
        }
    }
}