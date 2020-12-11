using UnityEngine;
using Newtonsoft.Json;

public class JSONMATLABResponse
{
    public int cmd_id;
    public int seq_num;
    public int status;
    public object value;

    public static JSONMATLABResponse RobotIKResponse(int seqNum, int status, float[] joints, Vector3 pos, Quaternion orient, float? sew = null)
    {
        if (seqNum < 0 || joints == null || pos == null || orient == null)
        {
            return null;
        }

        RobotIKResponseParameters ikParams = new RobotIKResponseParameters()
        {
            joints = joints,
            position = new float[] { pos.x, pos.y, pos.z },
            orientation = new float[] { orient.x, orient.y, orient.z, orient.w },
            sew = sew
        };

        return new JSONMATLABResponse()
        {
            cmd_id = 101,
            seq_num = seqNum,
            status = status,
            value = ikParams
        };
    }

    public string ToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static JSONMATLABResponse FromJSON(string json)
    {
        return JsonConvert.DeserializeObject<JSONMATLABResponse>(json);
    }

    public bool ShouldSerializevalue()
    {
        return value != null;
    }
}

public class RobotIKResponseParameters
{
    public float[] joints;
    public float[] position;
    public float[] orientation;
    public float? sew;

    public bool ShouldSerializesew()
    {
        return sew != null;
    }

    public static RobotIKResponseParameters FromJSON(string json)
    {
        return JsonConvert.DeserializeObject<RobotIKResponseParameters>(json);
    }
}