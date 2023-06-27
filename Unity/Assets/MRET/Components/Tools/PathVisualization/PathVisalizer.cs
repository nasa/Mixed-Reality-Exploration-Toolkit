// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.PathVisualization
{
    public class PathVisalizer : MonoBehaviour
    {
        public LayerMask layerMask;
        public LineRenderer lineRenderer;
        List<Vector3> nodePositions = new List<Vector3>();
        Vector3[] castedNodePositions;
        [Tooltip("Should the points be casted to the ground?")]
        public bool drawCasted;
        [Tooltip("Length of ray that is casted to find the ground")]
        public float rayCastDistance = 200f;

        // clears points
        public void ClearPoints()
        {
            nodePositions.Clear();
            UpdateCastedNodePositions();
        }

        // reads data from scriptable object
        public void ReadPathVisData(PathVisualizationData data)
        {
            nodePositions.Clear();
            for (int i = 0; i < data.positions.Length; i++)
            {
                nodePositions.Add(data.positions[i]);
            }
            UpdateCastedNodePositions();
        }

        // adds node positions for interpolation
        public void AddNodePosition(Vector3 pos)
        {
            nodePositions.Add(pos);
            UpdateCastedNodePositions();
        }

        void UpdateCastedNodePositions()
        {
            castedNodePositions = new Vector3[nodePositions.Count];

            for (int i = 0; i < nodePositions.Count; i++)
            {
                Vector3 castSource = nodePositions[i];
                castSource.y = 100f;
                if (drawCasted && Physics.Raycast(nodePositions[i], Vector3.down, out RaycastHit hitInfo, rayCastDistance, layerMask.value))
                {
                    castedNodePositions[i] = hitInfo.point;
                }
                else
                {
                    castedNodePositions[i] = nodePositions[i];
                }
            }
            UpdateDrawLine();
        }

        // updates drawn line
        void UpdateDrawLine()
        {
            lineRenderer.positionCount = castedNodePositions.Length;
            lineRenderer.SetPositions(castedNodePositions);
        }

        // generates interpolation so the line stays on the terrain
        public void GenerateInterpolations(int count)
        {
            for (int i = nodePositions.Count - 2; i >= 0; i--)
            {
                Vector3 min = nodePositions[i];
                Vector3 max = nodePositions[i + 1];

                for (int j = count - 1; j >= 0; j--)
                {
                    float factor = (1.0f * j) / (1.0f * count);
                    Vector3 interp = Vector3.Lerp(min, max, factor);
                    nodePositions.Insert(i, interp);
                }
            }
            UpdateCastedNodePositions();
        }
    }
}