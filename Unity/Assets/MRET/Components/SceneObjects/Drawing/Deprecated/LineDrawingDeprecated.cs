// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// LineDrawing is a class that represents a connected series of lines as
    /// a SceneObject.
    /// Author: Dylan Z. Baker
    /// </summary>
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.LineDrawing))]
    public class LineDrawingDeprecated : CustomShape
    {
        /// <summary>
        /// Scale multiplier for endpoint visuals.
        /// </summary>
        private static readonly float ENDPOINTVISUALSCALEMULTIPLIER = 1.5f;

        private static readonly float MIDDLEPOINTVISUALSCALEMULTIPLIER = 1.75f;

        /// <summary>
        /// Length of the line drawing.
        /// </summary>
        public virtual float length
        {
            get
            {
                if (lineRenderer == null)
                {
                    Debug.LogError("[LineDrawingDeprecated->length] Not initialized.");
                    return 0;
                }

                float length = 0;
                for (int i = 1; i < lineRenderer.positionCount; i++)
                {
                    length += Math.Abs(Vector3.Distance(lineRenderer.GetPosition(i),
                        lineRenderer.GetPosition(i - 1)));
                }
                return length;
            }
        }

        public virtual float segmentLength
        {
            get
            {
                if (lineRenderer == null)
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
        /// Center of the points.
        /// </summary>
        public Vector3 center
        {
            get
            {
                Vector3[] pts = points;
                if (pts.Length < 1)
                {
                    return transform.position;
                }

                float minX = pts[0].x, minY = pts[0].y, minZ = pts[0].z, maxX = pts[0].x, maxY = pts[0].y, maxZ = pts[0].z;
                foreach(Vector3 point in pts)
                {
                    if (point.x < minX)
                    {
                        minX = point.x;
                    }
                    else if (point.x > maxX)
                    {
                        maxX = point.x;
                    }

                    if (point.y < minY)
                    {
                        minY = point.y;
                    }
                    else if (point.y > maxY)
                    {
                        maxY = point.y;
                    }

                    if (point.z < minZ)
                    {
                        minZ = point.z;
                    }
                    else if (point.z > maxZ)
                    {
                        maxZ = point.z;
                    }
                }
                return Vector3.Lerp(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ), 0.5f);
            }
        }

        /// <summary>
        /// Points in the line drawing.
        /// </summary>
        public virtual Vector3[] points
        {
            get
            {
                if (lineRenderer == null)
                {
                    Debug.LogError("[LineDrawingDeprecated->points] Not initialized.");
                    return null;
                }

                List<Vector3> points = new List<Vector3>();
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    points.Add(lineRenderer.GetPosition(i));
                }
                return points.ToArray();
            }
            set
            {
                if (lineRenderer == null)
                {
                    Debug.LogError("[LineDrawingDeprecated->points] Not initialized.");
                    return;
                }

                lineRenderer.positionCount = value.Length;
                lineRenderer.SetPositions(value);
                SetUpMeshCollider();
            }
        }

        /// <summary>
        /// Number of points in the line drawing.
        /// </summary>
        public virtual int numPoints
        {
            get
            {
                return lineRenderer.positionCount;
            }
        }

        /// <summary>
        /// The drawing edit controller.
        /// </summary>
        public DrawingEditControllerDeprecated drawingEditController { get; private set; }

        /// <summary>
        /// Cutoff of the drawing, -1 for unlimited.
        /// </summary>
        public virtual float drawingCutoff { get; set; } = -1;

        /// <summary>
        /// Remaining length for drawing, -1 if no cutoff.
        /// </summary>
        public virtual float remainingLength
        {
            get
            {
                if (drawingCutoff < 0)
                {
                    return -1;
                }

                return Math.Max(drawingCutoff - length, 0);
            }
        }

        /// <summary>
        /// The line renderer.
        /// </summary>
        private LineRenderer lineRenderer;

        /// <summary>
        /// The measurement text.
        /// </summary>
        protected MeasurementTextDeprecated measurementText;

        protected MeasurementTextDeprecated[] segmentMeasurementText;

        /// <summary>
        /// The collider.
        /// </summary>
        protected Collider lineCollider;

        /// <summary>
        /// Container for point visuals.
        /// </summary>
        protected GameObject pointVisualContainer;

        /// <summary>
        /// Visualizations for the endpoints.
        /// </summary>
        private Dictionary<Vector3, GameObject> endpointVisuals;

        /// <summary>
        /// Visualizations for the points in between the endpoints.
        /// </summary>
        private Dictionary<Vector3, GameObject> midpointVisuals;

        /// <summary>
        /// Create a line drawing.
        /// </summary>
        /// <param name="name">Name of the line drawing.</param>
        /// <param name="width">Width of the line drawing.</param>
        /// <param name="color">Color of the line drawing.</param>
        /// <param name="positions">Positions in the line drawing.</param>
        /// <param name="uuid">UUID of the line drawing.</param>
        /// <param name="cutoff">Cutoff to give the line drawing.</param>
        /// <param name="showMeasurement">Whether or not to show the measurement text.</param>
        /// <returns>The specified line drawing.</returns>
        public static LineDrawingDeprecated Create(string name, float width, Color32 color, Vector3[] positions, Guid? uuid = null, float cutoff = -1, bool showMeasurement = false)
        {
            GameObject ldGO = new GameObject(name);
            LineDrawingDeprecated lineDrawing = ldGO.AddComponent<LineDrawingDeprecated>();
            lineDrawing.Initialize(uuid.HasValue ? uuid.Value : Guid.NewGuid());
            lineDrawing.SetWidth(width);
            lineDrawing.drawingCutoff = cutoff;
            lineDrawing.drawingEditController.OnColorChange(color);
            lineDrawing.drawingEditController.SetUnits(LineDrawingManagerDeprecated.Units.meters);
            lineDrawing.drawingEditController.SetWidth(width);
            lineDrawing.points = positions;
            if (showMeasurement)
            {
                lineDrawing.EnableMeasurement();
            }
            else
            {
                lineDrawing.DisableMeasurement();
            }
            lineDrawing.PositionMeasurementText();
            return lineDrawing;
        }

        /// <summary>
        /// Initialize the line drawing.
        /// </summary>
        /// <param name="uuid">UUID to apply to the line drawing.</param>
        public virtual void Initialize(Guid uuid)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            this.uuid = uuid;
            lineRenderer.material = MRET.LineDrawingMaterial;
            lineRenderer.useWorldSpace = false;
            SetUpMeasurementText();
            measurementText.SetValue(length, -1);
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
        public virtual void SetWidth(float width)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetWidth] Not initialized.");
                return;
            }

            lineRenderer.startWidth = lineRenderer.endWidth = width;
            SetUpMeshCollider();
        }

        /// <summary>
        /// Get width of the line drawing.
        /// </summary>
        /// <returns>Width in meters.</returns>
        public virtual float GetWidth()
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->GetWidth] Not initialized.");
                return 0;
            }

            return lineRenderer.startWidth;
        }

        /// <summary>
        /// Set color of the line drawing.
        /// </summary>
        /// <param name="color">Color of the drawing.</param>
        public virtual void SetColor(Color32 color)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetColor] Not initialized.");
                return;
            }

            if (lineRenderer.material == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetColor] No material.");
                return;
            }

            if (lineRenderer.material.shader.name != "Universal Render Pipeline/Lit")
            {
                Debug.LogError("[LineDrawing->SetColor] Incorrect shader. Must be Universal Render Pipeline/Lit.");
                return;
            }

            lineRenderer.material.SetColor("_BaseColor", color);
        }

        /// <summary>
        /// Get color of the line drawing.
        /// </summary>
        /// <returns>Color of the drawing.</returns>
        public virtual Color32 GetColor()
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->GetColor] Not initialized.");
                return Color.gray;
            }

            if (lineRenderer.material == null)
            {
                Debug.LogError("[LineDrawingDeprecated->GetColor] No material.");
                return Color.gray;
            }

            if (lineRenderer.material.shader.name != "Universal Render Pipeline/Lit")
            {
                Debug.LogError("[LineDrawing->GetColor] Incorrect shader. Must be Universal Render Pipeline/Lit.");
                return Color.gray;
            }

            return lineRenderer.material.GetColor("_BaseColor");
        }

        /// <summary>
        /// Add a point to the drawing.
        /// </summary>
        /// <param name="point">Point to add.</param>
        public virtual void AddPoint(Vector3 point)
        {
            AddPoint(point, numPoints);
            if (ProjectManager.PinManagerDeprecated.segmentedMeasurementEnabled)
            {
                SetUpSegmentMeasurementText();
            }
        }

        /// <summary>
        /// Add a point to the drawing.
        /// </summary>
        /// <param name="point">Point to add.</param>
        /// <param name="index">Index to add it at.</param>
        public virtual void AddPoint(Vector3 point, int index)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->AddPoint] Not initialized.");
                return;
            }

            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                positions.Add(lineRenderer.GetPosition(i));
            }

            if (index > lineRenderer.positionCount || index < 0)
            {
                Debug.LogError("[LineDrawingDeprecated->AddPoint] Invalid index.");
                return;
            }

            positions.Insert(index, point);
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
            SetUpMeshCollider();
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Add points to the line drawing.
        /// </summary>
        /// <param name="points">Points to add.</param>
        /// <param name="startIndex">Starting index to add them at.</param>
        public virtual void AddPoints(Vector3[] points, int startIndex)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->AddPoints] Not initialized.");
                return;
            }

            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                positions.Add(lineRenderer.GetPosition(i));
            }

            if (startIndex + points.Length > lineRenderer.positionCount + points.Length || startIndex < 0)
            {
                Debug.LogError("[LineDrawingDeprecated->AddPoints] Invalid index.");
                return;
            }

            positions.InsertRange(startIndex, points);
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
            SetUpMeshCollider();
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Remove point from the line drawing.
        /// </summary>
        /// <param name="index">Index of the point to remove.</param>
        public virtual void RemovePoint(int index)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->RemovePoint] Not initialized.");
                return;
            }

            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                positions.Add(lineRenderer.GetPosition(i)); ;
            }

            if (index >= lineRenderer.positionCount || index < 0)
            {
                Debug.LogError("[LineDrawingDeprecated->RemovePoint] Invalid index.");
                return;
            }

            positions.RemoveAt(index);
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
            SetUpMeshCollider();
            if (ProjectManager.PinManagerDeprecated.segmentedMeasurementEnabled)
            {
                Destroy(segmentMeasurementText[ProjectManager.PinManagerDeprecated.PathSegments].gameObject);
            }
            else
            {
                measurementText.SetValue(length, remainingLength);
            }
        }

        /// <summary>
        /// Replace a point in the line drawing.
        /// </summary>
        /// <param name="index">Index of the point to replace.</param>
        /// <param name="value">Value to replace it with.</param>
        public virtual void ReplacePoint(int index, Vector3 value)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->ReplacePoint] Not initialized.");
                return;
            }

            if (index >= lineRenderer.positionCount || index < 0)
            {
                Debug.LogError("[LineDrawingDeprecated->ReplacePoint] Invalid index.");
                return;
            }

            lineRenderer.SetPosition(index, value);
            SetUpMeshCollider();
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Append a line to this one.
        /// </summary>
        /// <param name="lineToAppend">Line to append.</param>
        /// <param name="appendBackwards">Whether or not to append points in reverse order.</param>
        /// <param name="appendToFront">Whether or not to append points to front</param>
        public void AppendLine(LineDrawingDeprecated lineToAppend, bool appendBackwards, bool appendToFront)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->AppendLine] Not initialized.");
                return;
            }
            Vector3[] appendPoints = lineToAppend.points;
            if (appendToFront)
            {
                Vector3[] originalLinePoints = points;
                for (int i = 0; i < originalLinePoints.Length; i++)
                {
                    RemovePoint(0);
                }

                if (appendBackwards)
                {
                    Vector3 appendLineStart = transform.InverseTransformPoint(
                        lineToAppend.transform.TransformPoint(appendPoints[0]));
                    // Add last point, don't add first point.
                    for (int i = appendPoints.Length - 1; i > 0; i--)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            lineToAppend.transform.TransformPoint(appendPoints[i]));
                        //AddPoint((newPoint - appendLineStart) + originalLinePoints[0]);
                        AddPoint(newPoint);
                    }
                }
                else
                {
                    Vector3 appendLineEnd = transform.InverseTransformPoint(
                        lineToAppend.transform.TransformPoint(appendPoints[appendPoints.Length - 1]));
                    // Add first point, don't add last point.
                    for (int i = 0; i < appendPoints.Length - 1; i++)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            lineToAppend.transform.TransformPoint(appendPoints[i]));
                        //AddPoint((newPoint - appendLineEnd) + originalLinePoints[0]);
                        AddPoint(newPoint);
                    }
                }

                foreach (Vector3 point in originalLinePoints)
                {
                    AddPoint(point);
                }
            }
            else
            {
                Vector3 originalEndPoint = points[points.Length - 1];
                if (appendBackwards)
                {
                    Vector3 appendLineEnd = transform.InverseTransformPoint(
                        lineToAppend.transform.TransformPoint(appendPoints[appendPoints.Length - 1]));
                    // Add first point, don't add last point.
                    for (int i = appendPoints.Length - 2; i > -1; i--)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            lineToAppend.transform.TransformPoint(appendPoints[i]));
                        //AddPoint((newPoint - appendLineEnd) + originalEndPoint);
                        AddPoint(newPoint);
                    }
                }
                else
                {
                    Vector3 appendLineStart = transform.InverseTransformPoint(
                        lineToAppend.transform.TransformPoint(appendPoints[0]));
                    // Add last point, don't add first point.
                    for (int i = 1; i < appendPoints.Length; i++)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            lineToAppend.transform.TransformPoint(appendPoints[i]));
                        //AddPoint((newPoint - appendLineStart) + originalEndPoint);
                        AddPoint(newPoint);
                    }
                }
            }
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Enable measurement display.
        /// </summary>
        public void EnableMeasurement()
        {
            measurementText.gameObject.SetActive(true);
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Disable measurement display.
        /// </summary>
        public void DisableMeasurement()
        {
            measurementText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Set the units for the measurement display.
        /// </summary>
        /// <param name="units">Units to set.</param>
        public void SetMeasurementUnits(LineDrawingManagerDeprecated.Units units)
        {
            measurementText.units = units;
            measurementText.SetValue(length, remainingLength);
        }

        /// <summary>
        /// Enables the endpoint visuals.
        /// </summary>
        public void EnableEndpointVisuals()
        {
            if (endpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> endpointVisual in endpointVisuals)
                {
                    Destroy(endpointVisual.Value);
                }
            }
            endpointVisuals = new Dictionary<Vector3, GameObject>();
            Vector3[] linePoints = points.Distinct().ToArray();
            if (linePoints == null || linePoints.Length < 1)
            {
                Debug.LogError("[LineDrawingDeprecated->EnableEndpointVisuals] No points.");
                return;
            }
            if (linePoints.Length > 0)
            {
                float pointDiameter = GetWidth() * ENDPOINTVISUALSCALEMULTIPLIER;
                // Create first point.
                endpointVisuals.Add(linePoints[0], SetUpEndPoint(0, linePoints[0], pointDiameter));
                if (linePoints.Length > 1)
                {
                    // Create last point.
                    endpointVisuals.Add(linePoints[linePoints.Length - 1], SetUpEndPoint(
                        linePoints.Length - 1, linePoints[linePoints.Length - 1], pointDiameter));
                }
            }
        }

        /// <summary>
        /// Disables the endpoint visuals.
        /// </summary>
        public void DisableEndpointVisuals()
        {
            if (endpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> endpointVisual in endpointVisuals)
                {
                    Destroy(endpointVisual.Value);
                }
            }
            endpointVisuals = null;
        }

        /// <summary>
        /// Enables the middle point visuals.
        /// </summary>
        /// <param name="numberOfPoints">Number of points to visualize.</param>
        public void EnableMiddlePointVisuals(int numberOfPoints)
        {
            Vector3[] linePoints = points.Distinct().ToArray();
            if (linePoints.Length < 3)
            {
                Debug.LogWarning("[LineDrawingDeprecated->EnableMiddlePointVisuals] No middle points.");
                return;
            }
            if (numberOfPoints > linePoints.Length - 2)
            {
                Debug.LogWarning("[LineDrawingDeprecated->EnableMiddlePointVisuals] Desired number of points exceeds actual.");
                numberOfPoints = linePoints.Length - 2;
            }
            if (midpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> midpointVisual in midpointVisuals)
                {
                    Destroy(midpointVisual.Value);
                }
            }
            midpointVisuals = new Dictionary<Vector3, GameObject>();
            float pointDiameter = GetWidth() * MIDDLEPOINTVISUALSCALEMULTIPLIER;
            float pointsPerVisual = linePoints.Length / numberOfPoints;
            for (float i = 1; i < linePoints.Length - 2; i+= pointsPerVisual)
            {
                int roundI = Mathf.RoundToInt(i);
                if (roundI <= 0 || roundI >= linePoints.Length - 1)
                {
                    continue;
                }
                midpointVisuals.Add(points[roundI], SetUpMiddlePoint(roundI, linePoints[roundI], pointDiameter));
            }
        }

        /// <summary>
        /// Disables the middle point visuals.
        /// </summary>
        public void DisableMiddlePointVisuals()
        {
            if (midpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> midpointVisual in midpointVisuals)
                {
                    Destroy(midpointVisual.Value);
                }
            }
            midpointVisuals = null;
        }

        public virtual void ColorSegment(Vector3 pointA, Vector3 pointB)
        {
            Vector3[] pointValues = points;
            for (int i = 1; i < numPoints; i++)
            {
                if ((pointValues[i - 1] == pointA && pointValues[i] == pointB)
                    || (pointValues[i] == pointA && pointValues[i - 1] == pointB))
                {
                    // TODO.
                    break;
                }
            }
        }

        public virtual void Segment(Vector3 pointA, Vector3 pointB)
        {
            Vector3[] pointValues = points;
            for (int i = 1; i < numPoints; i++)
            {
                if ((pointValues[i - 1] == pointA && pointValues[i] == pointB)
                    || (pointValues[i] == pointA && pointValues[i - 1] == pointB))
                {
                    // TODO.
                    break;
                }
            }
        }

        /// <summary>
        /// Set up the measurement text.
        /// </summary>
        protected void SetUpMeasurementText()
        {
            GameObject measurementObj = new GameObject("Measurement");
            measurementObj.transform.SetParent(transform);
            measurementObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            measurementObj.AddComponent<Canvas>();
            measurementText = measurementObj.AddComponent<MeasurementTextDeprecated>();
            measurementText.text = measurementObj.AddComponent<TMPro.TextMeshProUGUI>();
            measurementText.text.enableAutoSizing = true;
        }

        public virtual void SetUpSegmentMeasurementText()
        {
            GameObject measurementObj = Instantiate(ProjectManager.PinManagerDeprecated.segmentPathText);
            MeasurementTextDeprecated measurementText = measurementObj.GetComponentInChildren<MeasurementTextDeprecated>();
            ProjectManager.PinManagerDeprecated.paths[ProjectManager.PinManagerDeprecated.pathNumber].segmentMeasurements.Add(measurementText);
            measurementObj.transform.SetParent(transform);
            measurementObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }

        /// <summary>
        /// Begin to touch hold the provided hand.
        /// </summary>
        protected override void BeginTouchHold(InputHand hand)
        {
            base.BeginTouchHold(hand);
            drawingEditController.ShowButton();
        }

        /// <summary>
        /// End touching the touching hand.
        /// </summary>
        protected override void EndTouch()
        {
            base.EndTouch();
        }

        public override void Unuse(InputHand hand)
        {
            drawingEditController.HideButton();
            drawingEditController.DisableMenu();
        }

        /// <summary>
        /// Sets up an endpoint.
        /// </summary>
        /// <param name="index">Index of the endpoint.</param>
        /// <param name="position">Position to give endpoint.</param>
        /// <param name="diameter">Diameter to give endpoint.</param>
        /// <returns>Endpoint game object.</returns>
        private GameObject SetUpEndPoint(int index, Vector3 position, float diameter)
        {
            GameObject visual = Instantiate(ProjectManager.DrawingManager.drawingEndVisualPrefab);
            visual.name = "endpoint";
            visual.transform.SetParent(pointVisualContainer.transform);
            visual.transform.localPosition = position;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(diameter, diameter, diameter);
            DrawingPointVisualDeprecated drawingPointVisual = visual.GetComponent<DrawingPointVisualDeprecated>();
            if (drawingPointVisual == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetUpEndPoint] Error loading point visual.");
                return null;
            }
            Action<Vector3, InputHand> pointChangeAction = (Vector3 newPoint, InputHand hand) => {
                ReplacePoint(index, transform.InverseTransformPoint(newPoint));
                visual.transform.position = newPoint;
            };
            Action<Vector3, InputHand> pointChangeEndAction = (Vector3 newPoint, InputHand hand) => {
                
            };
            Action<Vector3, InputHand> pointAppendAction = (Vector3 newPoint, InputHand hand) => {
                LineDrawingControllerDeprecated ldc = hand.GetComponent<LineDrawingControllerDeprecated>();
                if (ldc == null)
                {
                    Debug.LogError("[LineDrawingDeprecated->EndPointChangeAction] No drawing edit controller.");
                }
                else if (ldc.touchingVisual.drawing == this)
                {
                    if (index == 0)
                    {
                        ldc.AddToLine(drawingPointVisual.drawing, true);
                    }
                    else
                    {
                        ldc.AddToLine(drawingPointVisual.drawing, false);
                    }
                    visual.transform.position = newPoint;
                }
            };
            Action<Vector3, InputHand, DrawingPointVisualDeprecated> pointAppendEndAction =
                (Vector3 newPoint, InputHand hand, DrawingPointVisualDeprecated touchingVisual) => {
                LineDrawingController ldc = hand.GetComponent<LineDrawingController>();
                if (ldc == null)
                {
                    Debug.LogError("[LineDrawingDeprecated->EndPointChangeEndAction] No drawing edit controller.");
                }
                else
                {
                    ldc.FinishLine();
                    if (touchingVisual != null && touchingVisual.drawing != this)
                    {
                        AppendLine(touchingVisual.drawing, !touchingVisual.isFirstPoint, index == 0);
                        ProjectManager.SceneObjectManagerDeprecated.DestroySceneObject(touchingVisual.drawing.uuid);
                    }
                    DisableEndpointVisuals();
                    EnableEndpointVisuals();
                }
            };
            drawingPointVisual.Initialize(pointChangeAction, pointChangeEndAction,
                pointAppendAction, pointAppendEndAction, true, index == 0, this);
            return visual;
        }

        /// <summary>
        /// Sets up a midpoint.
        /// </summary>
        /// <param name="index">Index of the midpoint.</param>
        /// <param name="position">Position to give midpoint.</param>
        /// <param name="diameter">Diameter to give midpoint.</param>
        /// <returns>Midpoint game object.</returns>
        private GameObject SetUpMiddlePoint(int index, Vector3 position, float diameter)
        {
            GameObject visual = Instantiate(ProjectManager.DrawingManager.drawingPointVisualPrefab);
            visual.name = "middlepoint";
            visual.transform.SetParent(pointVisualContainer.transform);
            visual.transform.localPosition = position;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(diameter, diameter, diameter);
            DrawingPointVisualDeprecated drawingPointVisual = visual.GetComponent<DrawingPointVisualDeprecated>();
            if (drawingPointVisual == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetUpMiddlePoint] Error loading point visual.");
                return null;
            }
            Action<Vector3, InputHand> pointChangeAction = (Vector3 newPoint, InputHand hand) => {
                /*int i = index - 1;
                bool restructuredLine = false;
                while (i >= 0 && points[i] != null)
                {
                    if (midpointVisuals.ContainsKey(points[i]))
                    {
                        // Key exists, finish finding previous point.
                        break;
                    }
                    else
                    {
                        midpointVisuals.Remove(points[i]);
                        RemovePoint(i);
                        restructuredLine = true;
                    }
                    i--;
                }
                i = index + 1;
                while (i < points.Length && points[i] != null)
                {
                    if (midpointVisuals.ContainsKey(points[i]))
                    {
                        // Key exists, finish finding next point.
                        break;
                    }
                    else
                    {
                        midpointVisuals.Remove(points[i]);
                        RemovePoint(i);
                        restructuredLine = true;
                    }
                    i++;
                }
                if (restructuredLine)
                {
                    int numPointVisuals = midpointVisuals.Count;
                    DisableMiddlePointVisuals();
                    EnableMiddlePointVisuals(numPointVisuals);
                }
                else
                {*/
                    ReplacePoint(index, transform.InverseTransformPoint(newPoint));
                    drawingPointVisual.transform.position = newPoint;
                //}
            };
            Action<Vector3, InputHand> pointChangeEndAction = (Vector3 newPoint, InputHand hand) => {

            };
            Action<Vector3, InputHand> pointAppendAction = (Vector3 newPoint, InputHand hand) => {
                
            };
            Action<Vector3, InputHand, DrawingPointVisualDeprecated> pointAppendEndAction =
                (Vector3 newPoint, InputHand hand, DrawingPointVisualDeprecated touchingVisual) => {
                
            };
            drawingPointVisual.Initialize(pointChangeAction, pointChangeEndAction,
                pointAppendAction, pointAppendEndAction, false, false, this);
            return visual;
        }

        protected void SetUpEditController()
        {
            GameObject drawingEditObject = Instantiate(ProjectManager.DrawingManager.drawingEditPrefab);
            drawingEditObject.transform.SetParent(transform);
            drawingEditObject.transform.localPosition = Vector3.zero;
            drawingEditObject.transform.localRotation = Quaternion.identity;
            drawingEditController = drawingEditObject.GetComponent<DrawingEditControllerDeprecated>();
            if (drawingEditController == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetUpEditController] Unable to set up.");
                return;
            }
            drawingEditController.currentDrawing = this;
            drawingEditController.Initialize();
            drawingEditController.DisableMenu();
        }

        /// <summary>
        /// Gets a mesh for the line.
        /// </summary>
        /// <returns>Mesh for the line.</returns>
        private Mesh GetMesh()
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->GetMesh] Not initialized.");
                return null;
            }

            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh);
            if (numPoints == 0)
            {
                mesh.vertices = new Vector3[0];
            }
            return mesh;
        }

        /// <summary>
        /// Sets up a mesh collider.
        /// </summary>
        private void SetUpMeshCollider()
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetUpMeshCollider] Not initialized.");
                return;
            }

            Mesh mesh = GetMesh();
            if (mesh == null)
            {
                Debug.LogError("[LineDrawingDeprecated->SetUp MeshCollider] Failed to get mesh.");
                return;
            }

            if (lineCollider != null)
            {
                Destroy(lineCollider);
                lineCollider = null;
            }

            lineCollider = gameObject.AddComponent<MeshCollider>();
            ((MeshCollider) lineCollider).convex = true;
            lineCollider.isTrigger = true;
            if (mesh.vertexCount > 0)
            {
                ((MeshCollider)lineCollider).sharedMesh = mesh;
            }
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
        }

        /// <summary>
        /// Positions the measurement text.
        /// </summary>
        protected virtual void PositionMeasurementText()
        {
            measurementText.transform.position = center + new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}