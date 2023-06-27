// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.RecordingCamera
{
    public class LookAt : MonoBehaviour
    {
        public Transform Target;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(Target.position + PositionOffset);
            transform.rotation *= Quaternion.Euler(RotationOffset);
        }
    }
}