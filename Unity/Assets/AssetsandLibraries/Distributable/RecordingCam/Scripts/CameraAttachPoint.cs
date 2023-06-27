// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.RecordingCamera
{
    public class CameraAttachPoint : MonoBehaviour
    {
        public Transform AttachPoint;

        public Material OffMaterial;
        public Material OnMaterial;

        public MeshRenderer CameraVisual;
        public GameObject PreviewVisual;

        public void SetCameraAttached(bool attached)
        {
            if (CameraVisual != null)
            {
                CameraVisual.material = attached ? OnMaterial : OffMaterial;
            }

            if (PreviewVisual != null)
            {
                PreviewVisual.SetActive(attached);
            }
        }
    }
}