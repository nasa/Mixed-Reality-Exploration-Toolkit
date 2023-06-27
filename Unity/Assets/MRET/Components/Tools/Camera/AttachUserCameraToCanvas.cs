// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Camera
{
    /// <summary>
    /// Attaches the user camera to a canvas. If a canvas is not specified, the parent will
    /// be queried for a canvas to assign the camera.</br>
    /// </summary>
    public class AttachUserCameraToCanvas : MonoBehaviour
    {
        [Tooltip("The canvas to assign the user camera. If not specified, the parent will be queried for a canvas.")]
        Canvas canvas;

        [Tooltip("The canvas plane distance.")]
        public int planeDistance = 1;

        // Start is called before the first frame update
        void Start()
        {
            // Check if a canvas was assigned
            if (canvas == null)
            {
                // Try to locate a canvas in the parent
                canvas = GetComponentInParent<Canvas>();
            }

            // Make sure we have a valid canvas
            if (canvas != null)
            {
                // Assign the camera
                canvas.worldCamera = MRET.InputRig.activeCamera;
                canvas.planeDistance = planeDistance;
            }
        }

    }
}