// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// VolumetricDrawingDeprecated is a class that represents a connected series of lines as
    /// a SceneObject in a volumetric way.
    /// Author: Dylan Z. Baker
    /// </summary>
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.VolumetricDrawingDeprecated))]
    public class VolumetricDrawingDeprecated : LineDrawingDeprecated
    {
        /// <summary>
        /// Length of the line drawing.
        /// </summary>
        public override float length
        {
            get
            {
                if (_points == null)
                {
                    Debug.LogError("[VolumetricDrawingDeprecated->length] Not initialized.");
                    return 0;
                }

                float length = 0;
                for (int i = 1; i < points.Length; i++)
                {
                    length += Math.Abs(Vector3.Distance(points[i], points[i - 1]));
                }
                return length;
            }
        }

        /// <summary>
        /// Segment length calculator. Currently using PinA and PinB <from cref="PathDeprecated"/>
        /// </summary>
        public override float segmentLength
        {
            get
            {
                if (_points == null)
                {
                    Debug.LogError("[LineDrawing->length] Not initialized.");
                    return 0;
                }
                
                float length = 0;

                if (ProjectManager.PinManagerDeprecated.paths[ProjectManager.PinManagerDeprecated.pathNumber].PinB != new Vector3(0, 0, 0))
                {
                    length += Math.Abs(Vector3.Distance(ProjectManager.PinManagerDeprecated.paths[ProjectManager.PinManagerDeprecated.pathNumber].PinA, ProjectManager.PinManagerDeprecated.paths[ProjectManager.PinManagerDeprecated.pathNumber].PinB));
                }

                else
                {
                    length = 0;
                }

                return length;
            }
        }

        /// <summary>
        /// Points in the line drawing.
        /// </summary>
        public override Vector3[] points
        {
            get
            {
                return _points.ToArray();
            }
            set
            {
                _points = new List<Vector3>(value);
                DrawLine();
            }
        }

        /// <summary>
        /// Number of points in the line drawing.
        /// </summary>
        public override int numPoints
        {
            get
            {
                return _points.Count;
            }
        }

        /// <summary>
        /// Threshold angle for adding a corner.
        /// </summary>
        private static readonly float CORNERTHRESHOLD = 3;

        /// <summary>
        /// Points in the line.
        /// </summary>
        private List<Vector3> _points;

        /// <summary>
        /// Width of the line.
        /// </summary>
        private float width;

        /// <summary>
        /// Color of the line.
        /// </summary>
        private Color32 color;

        /// <summary>
        /// Segment prefab for the line.
        /// </summary>
        private GameObject segmentPrefab;

        /// <summary>
        /// Corner prefab for the line.
        /// </summary>
        private GameObject cornerPrefab;

        /// <summary>
        /// Container for meshes in the line.
        /// </summary>
        private GameObject meshContainer;

        /// <summary>
        /// Segments and their start and end points in the line.
        /// </summary>
        private List<Tuple<Tuple<Vector3, Vector3>, GameObject[]>> segments;

        /// <summary>
        /// Create a line drawing.
        /// </summary>
        /// <param name="name">Name of the line drawing.</param>
        /// <param name="diameter">Diameter of the line drawing.</param>
        /// <param name="color">Color of the line drawing.</param>
        /// <param name="positions">Positions in the line drawing.</param>
        /// <param name="uuid">UUID of the line drawing.</param>
        /// <param name="showMeasurement">Whether or not to show the measurement text.</param>
        /// <returns>The specified line drawing.</returns>
        public static new VolumetricDrawingDeprecated Create(string name, float diameter, Color32 color, Vector3[] positions, Guid? uuid = null, float cutoff = -1, bool showMeasurement = false)
        {
            GameObject vdGO = new GameObject(name);
            VolumetricDrawingDeprecated volumetricDrawing = vdGO.AddComponent<VolumetricDrawingDeprecated>();
            volumetricDrawing.Initialize(uuid.HasValue ? uuid.Value : Guid.NewGuid());
            volumetricDrawing.SetWidth(diameter);
            volumetricDrawing.drawingCutoff = cutoff;
            volumetricDrawing.SetColor(color);
            volumetricDrawing.points = positions;
            if (showMeasurement)
            {
                volumetricDrawing.EnableMeasurement();
            }
            else
            {
                volumetricDrawing.DisableMeasurement();
            }
            volumetricDrawing.PositionMeasurementText();
            return volumetricDrawing;
        }

        /// <summary>
        /// Initialize the line drawing.
        /// </summary>
        /// <param name="uuid">UUID to apply to the line drawing.</param>
        public override void Initialize(Guid uuid)
        {
            _points = new List<Vector3>();
            segments = new List<Tuple<Tuple<Vector3, Vector3>, GameObject[]>>();
            this.uuid = uuid;
            meshContainer = new GameObject("Meshes");
            meshContainer.transform.SetParent(transform);
            segmentPrefab = ProjectManager.SceneObjectManagerDeprecated.volumetricDrawingSegmentPrefab;
            cornerPrefab = ProjectManager.SceneObjectManagerDeprecated.volumetricDrawingCornerPrefab;
            SetUpMeasurementText();
            measurementText.SetValue(length, remainingLength);
            SetUpEditController();
            pointVisualContainer = new GameObject("PointVisuals");
            pointVisualContainer.transform.SetParent(transform);
            pointVisualContainer.transform.localPosition = Vector3.zero;
            pointVisualContainer.transform.localRotation = Quaternion.identity;
            pointVisualContainer.transform.localScale = Vector3.one;
            useable = true;
            touchBehavior = TouchBehavior.Hold;
        }

        /// <summary>
        /// Set width of the line drawing.
        /// </summary>
        /// <param name="width">Width in meters.</param>
        public override void SetWidth(float width)
        {
            this.width = width;
            DrawLine();
        }

        /// <summary>
        /// Get width of the line drawing.
        /// </summary>
        /// <returns>Width in meters.</returns>
        public override float GetWidth()
        {
            return width;
        }

        /// <summary>
        /// Set color of the line drawing.
        /// </summary>
        /// <param name="color">Color of the drawing.</param>
        public override void SetColor(Color32 color)
        {
            this.color = color;
            RecolorLine();
        }

        /// <summary>
        /// Get color of the line drawing.
        /// </summary>
        /// <returns>Color of the drawing.</returns>
        public override Color32 GetColor()
        {
            return color;
        }

        /// <summary>
        /// Add a point to the drawing.
        /// </summary>
        /// <param name="point">Point to add.</param>
        public override void AddPoint(Vector3 point)
        {
            AddPoint(point, numPoints);
        }

        /// <summary>
        /// Add a point to the drawing.
        /// </summary>
        /// <param name="point">Point to add.</param>
        /// <param name="index">Index to add it at.</param>
        public override void AddPoint(Vector3 point, int index)
        {
            _points.Insert(index, point);
            if (index > 0)
            {
                DrawBetween(points[index - 1], points[index]);
            }

            if (index < points.Length - 1)
            {
                DrawBetween(points[index], points[index + 1]);
            }
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Add points to the line drawing.
        /// </summary>
        /// <param name="points">Points to add.</param>
        /// <param name="startIndex">Starting index to add them at.</param>
        public override void AddPoints(Vector3[] points, int startIndex)
        {
            _points.InsertRange(startIndex, points);
            DrawLine();
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Remove point from the line drawing.
        /// </summary>
        /// <param name="index">Index of the point to remove.</param>
        public override void RemovePoint(int index)
        {
            if (index >= _points.Count || index < 0)
            {
                Debug.LogError("[VolumetricDrawingDeprecated->RemovePoint] Invalid index.");
                return;
            }
            
            if (index < _points.Count - 1)
            {
                // Point exists after this one.
                if (index > 0)
                {
                    // Point exists before this one.
                    RemoveSegment(_points[index - 1], _points[index]);
                    RemoveSegment(_points[index], _points[index + 1]);
                    DrawBetween(_points[index - 1], _points[index]);
                }
                else
                {
                    // First point.
                    RemoveSegment(_points[index], _points[index + 1]);
                }
            }
            else
            {
                // Last point.
                if (index > 0)
                {
                    // Point exists before this one.
                    RemoveSegment(_points[index - 1], _points[index]);
                }
                else
                {
                    // Only point.
                    // No segment to remove.
                }
            }

            _points.RemoveAt(index);
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Replace a point in the line drawing.
        /// </summary>
        /// <param name="index">Index of the point to replace.</param>
        /// <param name="value">Value to replace it with.</param>
        public override void ReplacePoint(int index, Vector3 value)
        {
            if (index >= _points.Count || index < 0)
            {
                Debug.LogError("[VolumetricDrawingDeprecated->ReplacePoint] Invalid index.");
                return;
            }

            if (index < _points.Count - 1)
            {
                // Point exists after this one.
                if (index > 0)
                {
                    // Point exists before this one.
                    RemoveSegment(_points[index - 1], _points[index]);
                    RemoveSegment(_points[index], _points[index + 1]);
                    DrawBetween(_points[index - 1], value);
                    DrawBetween(value, points[index + 1]);
                }
                else
                {
                    // First point.
                    RemoveSegment(_points[index], _points[index + 1]);
                    DrawBetween(value, _points[index + 1]);
                }
            }
            else
            {
                // Last point.
                if (index > 0)
                {
                    // Point exists before this one.
                    RemoveSegment(_points[index - 1], _points[index]);
                    DrawBetween(_points[index - 1], value);
                }
                else
                {
                    // Only point.
                    // No segment to replace.
                }
            }
            _points[index] = value;
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Draws the current line from scratch.
        /// </summary>
        private void DrawLine()
        {
            ClearLine();

            for (int i = 1; i < points.Length; i++)
            {
                DrawBetween(points[i - 1], points[i]);
            }
            SetUpMeshColliders();
        }

        /// <summary>
        /// Last segment in the line.
        /// </summary>
        private GameObject lastSegment = null;

        /// <summary>
        /// Draw a line between point A and B.
        /// </summary>
        /// <param name="pointA">Starting point.</param>
        /// <param name="pointB">Ending point.</param>
        private void DrawBetween(Vector3 pointA, Vector3 pointB)
        {
            List<GameObject> segmentObjects = new List<GameObject>();

            // Instantiate prefab and scale/rotate it.
            GameObject segment = Instantiate(segmentPrefab);
            RecolorSegment(segment);
            segment.transform.SetParent(meshContainer.transform);
            segment.transform.localPosition = Vector3.Lerp(pointA, pointB, 0.5f);
            segment.transform.localScale = new Vector3(width,
                Math.Abs(Vector3.Distance(pointA, pointB)) / 2, width);
            segment.transform.LookAt(transform.TransformPoint(pointB));
            Vector3 currentAngles = segment.transform.rotation.eulerAngles;
            segment.transform.rotation = Quaternion.Euler(currentAngles.x + 90, currentAngles.y, currentAngles.z);
            segmentObjects.Add(segment);

            // Smooth edges.
            if (lastSegment != null)
            {
                Vector3 relativeRotation =
                    (Quaternion.Inverse(lastSegment.transform.rotation) * segment.transform.rotation).eulerAngles;
                if (Math.Abs(NormalizeDegreesAroundZero(relativeRotation.x)) > CORNERTHRESHOLD ||
                    Math.Abs(NormalizeDegreesAroundZero(relativeRotation.y)) > CORNERTHRESHOLD ||
                    Math.Abs(NormalizeDegreesAroundZero(relativeRotation.z)) > CORNERTHRESHOLD)
                {
                    GameObject corner = Instantiate(cornerPrefab);
                    RecolorSegment(corner);
                    corner.transform.SetParent(meshContainer.transform);
                    corner.transform.localScale = new Vector3(width, width, width);
                    corner.transform.localPosition = pointA;
                    segmentObjects.Add(corner);
                }
            }
            lastSegment = segment;
            segments.Add(new Tuple<Tuple<Vector3, Vector3>, GameObject[]>(
                new Tuple<Vector3, Vector3>(pointA, pointB), segmentObjects.ToArray()));
        }

        /// <summary>
        /// Remove a line segment between point A and B.
        /// </summary>
        /// <param name="pointA">Starting point.</param>
        /// <param name="pointB">Ending point.</param>
        private void RemoveSegment(Vector3 pointA, Vector3 pointB)
        {
            List<GameObject> segs = new List<GameObject>();
            foreach (Tuple<Tuple<Vector3, Vector3>, GameObject[]> segment in segments)
            {
                if (segment.Item1.Item1 == pointA && segment.Item1.Item2 == pointB)
                {
                    foreach (GameObject seg in segment.Item2)
                    {
                        segs.Add(seg);
                    }
                }
            }
            if (segs.Count < 1)
            {
                Debug.LogError("[VolumetricDrawingDeprecated->RemoveSegment] Unable to find segment.");
                return;
            }

            bool found = false;
            List<Tuple<Tuple<Vector3, Vector3>, GameObject[]>> itemsToRemove = new List<Tuple<Tuple<Vector3, Vector3>, GameObject[]>>();
            foreach (Tuple<Tuple<Vector3, Vector3>, GameObject[]> item in segments)
            {
                if (found)
                {
                    break;
                }
                foreach (GameObject go in item.Item2)
                {
                    if (found)
                    {
                        break;
                    }
                    foreach (GameObject otherGO in segs)
                    {
                        if (go == otherGO)
                        {
                            itemsToRemove.Add(item);
                            found = true;
                            break;
                        }
                    }
                }
            }

            foreach (Tuple<Tuple<Vector3, Vector3>, GameObject[]> item in itemsToRemove)
            {
                segments.Remove(item);
            }

            foreach (GameObject seg in segs)
            {
                if (seg != null)
                {
                    Destroy(seg);
                }
            }
            SetUpMeshColliders();
        }

        /// <summary>
        /// Clear out all line segments.
        /// </summary>
        private void ClearLine()
        {
            foreach (Transform mesh in meshContainer.transform)
            {
                Destroy(mesh.gameObject);
            }
            lastSegment = null;
            segments = new List<Tuple<Tuple<Vector3, Vector3>, GameObject[]>>();
        }

        /// <summary>
        /// Recolors the line.
        /// </summary>
        private void RecolorLine()
        {
            foreach (MeshRenderer rend in meshContainer.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material mat in rend.materials)
                {
                    if (mat.shader.name != "Universal Render Pipeline/Lit")
                    {
                        Debug.LogError("[VolumetricDrawing->RecolorLine] Incorrect shader. Must be Universal Render Pipeline/Lit");
                        continue;
                    }

                    mat.SetColor("_BaseColor", color);
                }
            }
        }

        /// <summary>
        /// Recolors the provided segment.
        /// </summary>
        /// <param name="segment">Segment to recolor.</param>
        private void RecolorSegment(GameObject segment)
        {
            foreach (MeshRenderer rend in segment.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material mat in rend.materials)
                {
                    if (mat.shader.name != "Universal Render Pipeline/Lit")
                    {
                        Debug.LogError("[VolumetricDrawing->RecolorSegment] Incorrect shader. Must be Universal Render Pipeline/Lit.");
                        continue;
                    }

                    mat.SetColor("_BaseColor", color);
                }
            }
        }

        // TODO: Need library.
        /// <summary>
        /// Normalize the angle and keep between -180 and 180.
        /// </summary>
        /// <param name="rawAngle">Raw angle.</param>
        /// <returns>Normalized angle.</returns>
        private float NormalizeDegreesAroundZero(float rawAngle)
        {
            if (rawAngle < 180 && rawAngle > -180)
            {
                return rawAngle;
            }
            else if (rawAngle > 180)
            {
                while (rawAngle > 180)
                {
                    rawAngle -= 360;
                }
                return rawAngle;
            }
            else
            {
                while (rawAngle < -180)
                {
                    rawAngle += 360;
                }
                return rawAngle;
            }
        }

        private void SetUpMeshColliders()
        {
            if (_points == null || meshContainer == null)
            {
                Debug.LogError("[VolumetricDrawingDeprecated->SetUpMeshColliders] Not initialized.");
                return;
            }

            foreach(Transform meshObject in meshContainer.transform)
            {
                MeshFilter mf = meshObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    foreach (Collider collider in meshObject.GetComponents<Collider>())
                    {
                        Destroy(collider);
                    }
                    lineCollider = meshObject.gameObject.AddComponent<MeshCollider>();
                    ((MeshCollider) lineCollider).sharedMesh = mf.mesh;
                    ((MeshCollider)lineCollider).convex = true;
                    lineCollider.isTrigger = true;
                }
            }
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
        }
    }
}