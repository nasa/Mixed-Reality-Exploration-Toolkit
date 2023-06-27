﻿// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.XRUI
{
    public class ControllerUIRaycastDetectionManager : MonoBehaviour
    {
        [Tooltip("The layer number that the UI is set to.")]
        public int uiLayer = 5;
        public bool intersectionStatus
        {
            get
            {
                return isIntersecting;
            }
        }

        public Vector3 raycastPoint;
        public GameObject intersectingObject;

        private int layerMask;
        private bool isIntersecting = false;

        void Start()
        {
            layerMask = 1 << uiLayer;
        }

        void Update()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                isIntersecting = true;
                raycastPoint = hit.point;
                intersectingObject = hit.collider.gameObject;
            }
            else
            {
                isIntersecting = false;
            }
        }
    }
}