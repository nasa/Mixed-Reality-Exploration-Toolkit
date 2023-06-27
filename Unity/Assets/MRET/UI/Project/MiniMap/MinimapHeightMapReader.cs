// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.MiniMap
{
    public class MinimapHeightMapReader : MonoBehaviour
    {
        [Tooltip("Minimap camera being referenced")]
        [SerializeField] Camera minimapCamera;
        [Tooltip("Terrain layer for raycast to hit")]
        [SerializeField] LayerMask terrainLayer;
        private Terrain _terrain;

        // cast a ray down to find the terrain being reference (optimized
        private void FindTerrain()
        {
            if (Physics.Raycast(minimapCamera.transform.position, Vector3.down, out RaycastHit hit, 300f, terrainLayer.value, QueryTriggerInteraction.Ignore))
            {
                Terrain terrain = hit.collider.GetComponent<Terrain>();

                if (terrain != null)
                {
                    _terrain = terrain;
                }
            }
        }

        private void Awake()
        {
            if (_terrain == null)
            {
                FindTerrain();
                return;
            }
        }

        // samples the height on the minimap
        public bool TrySampleHeight(float x, float y, out float height)
        {
            if (_terrain == null)
            {
                height = -1;
                return false;
            }

            Vector3 screenPoint = new Vector3(x * minimapCamera.pixelWidth, y * minimapCamera.pixelHeight, 0);
            Vector3 worldPoint = minimapCamera.ScreenToWorldPoint(screenPoint);
            height = _terrain.SampleHeight(worldPoint);

            return true;
        }

    }
}