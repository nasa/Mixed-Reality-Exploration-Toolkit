// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;

public class MatlabIKScript : MonoBehaviour
{
    public enum MatlabCommandType { Legacy, JSON };

    public string robotName;
    public string toolName;
    public MatlabCommandType commandType = MatlabCommandType.JSON;
    public GameObject origin;
    public List<MatlabIKJoint> joints = new List<MatlabIKJoint>();

    //public GameObject frame;

    private MATLABClient matlabClient;
    private List<float> jointAnglesToApply = null;
    private bool currentlyProcessing = false;
    private Vector3 lastIKPos = Vector3.zero;
    private Quaternion lastIKRot = Quaternion.identity;

	void Start()
    {
        matlabClient = FindObjectOfType<MATLABClient>();
    }

    void Update()
    {
        if (jointAnglesToApply != null)
        {
            ApplyJointAngles(jointAnglesToApply);

            jointAnglesToApply = null;
        }
    }

    public void SetElbowPosition(Vector3 elbowPos, int seqNum)
    {
        if (commandType == MatlabCommandType.JSON)
        {
            List<float> guess = new List<float>();
            foreach (MatlabIKJoint joint in joints)
            {
                guess.Add(DegreesToRads(Normalize(joint.GetJointAngle())));
            }

            matlabClient.Send(JSONMATLABCommand.RobotIKCommand(seqNum, lastIKPos, lastIKRot, robotName, toolName,
                    guess.Count > 0 ? guess.ToArray() : null, null, elbowPos).ToJSON());
        }
    }

    public void SetIKPosition(Vector3 endEffectorPos, Quaternion endEffectorRot, int seqNum)
    {
        if (!currentlyProcessing)
        {
            GameObject positioningObject = new GameObject();
            positioningObject.transform.position = endEffectorPos;
            positioningObject.transform.rotation = endEffectorRot;
            positioningObject.transform.SetParent(origin.transform);
            Vector3 pos = new Vector3(positioningObject.transform.localPosition.x,
                positioningObject.transform.localPosition.y, positioningObject.transform.localPosition.z);
            Quaternion rot = positioningObject.transform.localRotation;// * Quaternion.Euler(Vector3.forward * -90);
            Destroy(positioningObject);

            if (commandType == MatlabCommandType.JSON)
            {
                List<float> guess = new List<float>();
                foreach (MatlabIKJoint joint in joints)
                {
                    guess.Add(DegreesToRads(Normalize(joint.GetJointAngle())));
                }

                lastIKPos = pos;
                lastIKRot = rot;

                matlabClient.Send(JSONMATLABCommand.RobotIKCommand(seqNum, pos, rot, robotName, toolName,
                    guess.Count > 0 ? guess.ToArray() : null, null, null).ToJSON());
            }
            else if (commandType == MatlabCommandType.Legacy)
            {
                matlabClient.Send(":1:;restoreik:;[" + pos.x + ";" + -1 * pos.y + ";" + pos.z
                    + "]:;[" + rot.x + " " + rot.y + " " + rot.z + " " + rot.w + "]");
            }

            List<float> jointAngles = ExtractJointAngles(matlabClient.Receive());
            if (jointAngles != null)
            {
                jointAnglesToApply = jointAngles;
            }
        }
    }

    private List<float> ExtractJointAngles(string jointList)
    {
        List<float> jointAngles = new List<float>();
        if (commandType == MatlabCommandType.JSON)
        {
            JSONMATLABResponse resp = JSONMATLABResponse.FromJSON(jointList);
            if (resp != null && resp.cmd_id == 101 && resp.status == 0)
            {
                Debug.Log(resp.value);
                RobotIKResponseParameters parameters = RobotIKResponseParameters.FromJSON(resp.value.ToString());
                if (parameters.joints.Length == joints.Count)
                {
                    foreach (float joint in parameters.joints)
                    {
                        jointAngles.Add(joint);
                    }
                }

                //frame.transform.position = new Vector3(parameters.position[0], parameters.position[1], parameters.position[2]);
                //frame.transform.rotation = new Quaternion(parameters.orientation[0], parameters.orientation[1], parameters.orientation[2], parameters.orientation[3]);
            }
        }
        else if (commandType == MatlabCommandType.Legacy)
        {
            string formattedList = jointList.Substring(jointList.IndexOf('[') + 1);
            formattedList = formattedList.Remove(formattedList.LastIndexOf(']'), 1);

            foreach (string item in formattedList.Split(' '))
            {
                if (item == "NaN")
                {
                    return null;
                }

                jointAngles.Add(float.Parse(item));
            }
        }

        return jointAngles;
    }

    private void ApplyJointAngles(List<float> jointAngles)
    {
        if (jointAngles.Count == joints.Count)
        {
            for (int i = 0; i < jointAngles.Count; i++)
            {
                joints[i].ApplyJointAngle(Normalize(RadsToDegrees(jointAngles[i])));
            }
        }
    }

#region Helpers
    private float RadsToDegrees(float radAngle)
    {
        return radAngle * 180 / (float) System.Math.PI;
    }

    private float DegreesToRads(float degreeAngle)
    {
        return degreeAngle * (float) System.Math.PI / 180;
    }

    private float Normalize(float rawAngle)
    {
        if (rawAngle < 360 && rawAngle > 0)
        {
            return rawAngle;
        }
        else if (rawAngle > 360)
        {
            while (rawAngle > 360)
            {
                rawAngle -= 360;
            }
            return rawAngle;
        }
        else
        {
            while (rawAngle < 0)
            {
                rawAngle += 360;
            }
            return rawAngle;
        }
    }
#endregion
}