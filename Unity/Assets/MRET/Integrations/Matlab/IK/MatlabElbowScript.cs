﻿// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.Matlab.IK
{
    public class MatlabElbowScript : MonoBehaviour
    {
        public GameObject origin;
        public MatlabIKScript ikScript;

        public void SetElbowPosition(Vector3 elbowPos, int seqNum)
        {
            GameObject positioningObject = new GameObject();
            positioningObject.transform.position = elbowPos;
            positioningObject.transform.rotation = Quaternion.identity;
            positioningObject.transform.SetParent(origin.transform);
            Vector3 pos = new Vector3(positioningObject.transform.localPosition.x,
                positioningObject.transform.localPosition.y, positioningObject.transform.localPosition.z);
            Destroy(positioningObject);

            ikScript.SetElbowPosition(pos, seqNum);
        }
    }
}