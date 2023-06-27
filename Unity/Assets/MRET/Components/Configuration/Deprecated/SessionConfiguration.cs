// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.MRET.Configuration
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.MRET))]
    public class SessionConfiguration : MonoBehaviour
    {
        public static readonly int raycastLayer = 15;
        public static readonly int previewLayer = 20;
        public static readonly int defaultLayer = 0;

        public List<Material> defaultPartMaterials;
        public Material collisionMaterial;
        public Material highlightMaterial;
        public Material selectMaterial;
        public AudioClip collisionSound;
        public GameObject drawingPanelPrefab;
        public GameObject objectPlacementContainer;
        public bool partPanelEnabled = true;

        public static SessionConfiguration instance = null;

        private void Awake()
        {
            instance = this;
        }
    }
}