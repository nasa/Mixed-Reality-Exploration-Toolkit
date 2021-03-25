// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class LineDrawing
{
    public enum RenderTypes { Cable, Drawing, Measurement };
    public enum unit { meters, centimeters, millimeters, yards, feet, inches };

    public bool initialized = false;
    public RenderTypes renderType;
    public unit desiredUnits = unit.meters;
    public float lineWidth = 0.005f;
    public string name;
    public GameObject measurementText, meshModel, previewModel;
    public Guid guid;

    private GameObject cablePrefab, cornerPrefab;
    private GameObject parentContainer;
    private List<Vector3> points = new List<Vector3>();
    private Text text;
    private Material lMat, highlightMat;
    private bool isRendered = false;

    // Renderer Types.
    private MeshLineRenderer meshLine, previewLine;

    public LineDrawing(RenderTypes rType, Material mat, Material highlightMaterial, GameObject cable, GameObject corner, float width, GameObject container, bool preview)
    {
        renderType = rType;
        lMat = mat;
        highlightMat = highlightMaterial;
        cablePrefab = cable;
        cornerPrefab = corner;
        lineWidth = width;
        parentContainer = container;
        guid = Guid.NewGuid();
        Render();

        if (preview)
        {
            previewModel = new GameObject();
            previewModel.AddComponent<MeshFilter>();
            previewModel.AddComponent<MeshRenderer>();
            previewLine = previewModel.AddComponent<MeshLineRenderer>();
            previewLine.setWidth(lineWidth);
            previewLine.lmat = lMat;
            previewLine.hmat = highlightMat;
            previewLine.drawingScript = this;
        }
    }

    public DrawingType Serialize()
    {
        DrawingType serializedDrawing = new DrawingType();
        serializedDrawing.Name = meshModel.name;
        serializedDrawing.RenderType = renderType.ToString();

        Vector3[] pts = GetPoints();
        int index = 0;
        serializedDrawing.Points = new Vector3Type[pts.Length];
        serializedDrawing.GUID = guid.ToString();
        foreach (Vector3 pt in pts)
        {
            serializedDrawing.Points[index] = new Vector3Type();
            serializedDrawing.Points[index].X = pt.x;
            serializedDrawing.Points[index].Y = pt.y;
            serializedDrawing.Points[index].Z = pt.z;
            index++;
        }
        index++;

        return serializedDrawing;
    }

    public GameObject GetPreviewModel()
    {
        return previewModel;
    }

    public GameObject GetMeasurementText()
    {
        return measurementText;
    }

    public Vector3[] GetPoints()
    {
        return points.ToArray();
    }

    public Vector3 GetLastPoint()
    {
        if (points.Count > 0)
        {
            return points[points.Count - 1];
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void DestroyPreviewLine()
    {
        GameObject.Destroy(previewModel);
    }

    public int GetNumPoints()
    {
        return points.Count;
    }

    public float GetDistance(unit units, Vector3 lastPoint)
    {
        float measuredDistance = 0;

        // Get size (magnitude) in meters.
        for (int i = points.Count - 2; i >= 0; i--)
        {
            measuredDistance += Vector3.Distance(points[i + 1], points[i]);
        }
        if (points.Count != 0)
        {
            measuredDistance += Vector3.Distance(points[0], lastPoint);
        }

        // Convert size to desired units.
        switch (units)
        {
            case unit.meters:
                return measuredDistance;

            case unit.centimeters:
                return measuredDistance * 100f;

            case unit.millimeters:
                return measuredDistance * 1000f;

            case unit.yards:
                return measuredDistance * 1.09361f;

            case unit.feet:
                return measuredDistance * 3.28084f;

            case unit.inches:
                return measuredDistance * 39.37008f;

            default:
                return measuredDistance;
        }
    }

    public float GetDistance(unit units)
    {
        float measuredDistance = 0;

        // Get size (magnitude) in meters.
        for (int i = points.Count - 2; i >= 0; i--)
        {
            measuredDistance += Vector3.Distance(points[i + 1], points[i]);
        }

        // Convert size to desired units.
        switch (units)
        {
            case unit.meters:
                return measuredDistance;

            case unit.centimeters:
                return measuredDistance * 100f;

            case unit.millimeters:
                return measuredDistance * 1000f;

            case unit.yards:
                return measuredDistance * 1.09361f;

            case unit.feet:
                return measuredDistance * 3.28084f;

            case unit.inches:
                return measuredDistance * 39.37008f;

            default:
                return measuredDistance;
        }
    }

    public Vector3 GetMidpoint()
    {
        int last = points.Count - 1;
        if (last < 1)
        {
            return new Vector3(0, 0, 0);
        }
        else
        {
            Vector3 midpoint = Vector3.Lerp(points[0], points[last], 0.5f);
            midpoint.x = midpoint.x + 0.025f;
            midpoint.y = midpoint.y + 0.025f;
            midpoint.z = midpoint.z + 0.025f;
            return midpoint;
        }
    }

    public void AddPoint(Vector3 pointToAdd)
    {
        points.Add(pointToAdd);
        if (isRendered)
        {
            if (renderType == RenderTypes.Cable)
            {
                if (points.Count > 1)
                {
                    DrawCableBetween(points[points.Count - 2], pointToAdd);
                }
            }
            else
            {
                meshLine.AddPoint(pointToAdd);

                if (renderType == RenderTypes.Measurement)
                {
                    UpdateMeasurementText();
                }
            }
        }
        else
        {
            Debug.Log("Line is not being rendered.");
        }
    }

    public void AddPointAt(int index, Vector3 pointToAdd)
    {
        if (isRendered)
        {
            points.Insert(index, pointToAdd);
            Rerender();
        }
        else
        {
            Debug.Log("Line is not being rendered.");
        }
    }

    public void RemovePointByValue(Vector3 value)
    {
        points.Remove(value);
        Rerender();
    }

    public void RemoveAllPointsByValue(Vector3 value)
    {
        while (points.Remove(value)) { }
        Rerender();
    }

    public void RemovePointByIndex(int pointIndex)
    {
        if (pointIndex > points.Count)
        {
            Debug.LogError("Point index " + pointIndex + " not found.");
        }
        else
        {
            points.RemoveAt(pointIndex);
            Rerender();
        }
    }

    public void GenerateMeasurementText()
    {
        measurementText = new GameObject("MeasurementText");
        measurementText.AddComponent<Canvas>();
        measurementText.AddComponent<CanvasScaler>();
        measurementText.AddComponent<GraphicRaycaster>();
        GameObject textSubObject = new GameObject();
        textSubObject.transform.SetParent(measurementText.transform);
        text = textSubObject.AddComponent<Text>();
        text.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        Font ArialFont = (Font) Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = ArialFont;
        text.fontSize = 20;
        text.material = ArialFont.material;
        measurementText.transform.SetParent(parentContainer.transform);

        textSubObject.AddComponent<BoxCollider>();
        textSubObject.GetComponent<BoxCollider>().isTrigger = true;
        textSubObject.AddComponent<InteractableLabel>();
        textSubObject.GetComponent<InteractableLabel>().enabled = true;
        textSubObject.GetComponent<InteractableLabel>().grabbable = true;
        textSubObject.GetComponent<InteractableLabel>().useable = true;
    }

    public void Render()
    {
        if (isRendered)
        {
            Debug.Log("Nothing to do... already rendered.");
        }
        else
        {
            InteractableDrawing interactableDrawing;

            switch (renderType)
            {
                case RenderTypes.Cable:
                    meshModel = new GameObject();
                    meshModel.transform.parent = parentContainer.transform;
                    meshLine = meshModel.AddComponent<MeshLineRenderer>();
                    meshLine.setWidth(lineWidth);
                    meshLine.lmat = lMat;
                    meshLine.hmat = highlightMat;
                    meshLine.drawingScript = this;
                    interactableDrawing = meshModel.AddComponent<InteractableDrawing>();
                    interactableDrawing.enabled = true;
                    interactableDrawing.useable = true;
                    interactableDrawing.headsetObject = MRET.InputRig.head.transform;
                    interactableDrawing.drawingPanelPrefab = MRET.DrawingPanelPrefab;

                    isRendered = true;
                    initialized = true;
                    break;

                case RenderTypes.Drawing:
                    meshModel = new GameObject();
                    meshModel.transform.parent = parentContainer.transform;
                    meshModel.AddComponent<MeshFilter>();
                    meshModel.AddComponent<MeshRenderer>();
                    meshLine = meshModel.AddComponent<MeshLineRenderer>();
                    meshLine.setWidth(lineWidth);
                    meshLine.lmat = lMat;
                    meshLine.hmat = highlightMat;
                    meshLine.drawingScript = this;
                    interactableDrawing = meshModel.AddComponent<InteractableDrawing>();
                    interactableDrawing.enabled = true;
                    interactableDrawing.useable = true;
                    interactableDrawing.headsetObject = MRET.InputRig.head.transform;
                    interactableDrawing.drawingPanelPrefab = MRET.DrawingPanelPrefab;

                    isRendered = true;
                    initialized = true;
                    break;

                case RenderTypes.Measurement:
                    meshModel = new GameObject();
                    meshModel.transform.parent = parentContainer.transform;
                    meshModel.AddComponent<MeshFilter>();
                    meshModel.AddComponent<MeshRenderer>();
                    meshLine = meshModel.AddComponent<MeshLineRenderer>();
                    meshLine.setWidth(lineWidth);
                    meshLine.lmat = lMat;
                    meshLine.hmat = highlightMat;
                    meshLine.drawingScript = this;
                    interactableDrawing = meshModel.AddComponent<InteractableDrawing>();
                    interactableDrawing.enabled = true;
                    interactableDrawing.useable = true;
                    interactableDrawing.headsetObject = MRET.InputRig.head.transform;
                    interactableDrawing.drawingPanelPrefab = MRET.DrawingPanelPrefab;

                    isRendered = true;

                    GenerateMeasurementText();
                    initialized = true;
                    break;
            }
        }
    }

    public void SetMat(Material mat)
    {
        lMat = mat;
    }

    public void Unrender()
    {
        // Clear old Drawing components.
        GameObject.Destroy(meshModel);
        GameObject.Destroy(measurementText);
        isRendered = false;
    }

    public void Rerender()
    {
        // Start a new model.
        Unrender();
        Render();

        // Add each point to the model.
        for (int i = 0; i < points.Count; i++)
        {
            if (renderType == RenderTypes.Cable)
            {
                if (i > 0)
                {
                    DrawCableBetween(points[i - 1], points[i]);
                }
            }
            else
            {
                meshLine.AddPoint(points[i]);

                if (renderType == RenderTypes.Measurement)
                {
                    UpdateMeasurementText();
                }
            }
        }

        // Add components to re-rendered drawing.
        MeshCollider coll = meshModel.AddComponent<MeshCollider>();
        coll.convex = true;
        coll.isTrigger = true;
    }

    public Vector3 SetPreviewLine(Vector3 endPoint, bool snap)
    {
        if (points.Count > 0)
        {
            if (snap)
            {
                endPoint = SnapLine(points[points.Count - 1], endPoint);
            }

            previewLine.MakeSingleLine(points[points.Count - 1], endPoint);
        }
        return endPoint;
    }

    public void UpdateMeasurementText()
    {
        if (points.Count == 0)
        {
            measurementText.SetActive(false);
        }
        else
        {
            measurementText.SetActive(true);
            text.text = Math.Round((decimal) GetDistance(desiredUnits), 3).ToString() + " " + GetUnitsLabel();

            // Orient text towards user's eyes.
            measurementText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            text.transform.position = points[points.Count - 1];
        }
    }

    private string GetUnitsLabel()
    {
        switch (desiredUnits)
        {
            case unit.centimeters: return "cm";
            case unit.feet: return "ft";
            case unit.inches: return "in";
            case unit.meters: return "m";
            case unit.millimeters: return "mm";
            case unit.yards: return "yd";
            default: return "";
        }
    }

#region CableDrawing
    private GameObject lastSegment = null;
    private void DrawCableBetween(Vector3 pointA, Vector3 pointB)
    {
        // Instantiate prefab and scale/rotate it.
        GameObject segment = GameObject.Instantiate(cablePrefab);
        segment.transform.SetParent(meshModel.transform);
        segment.transform.localPosition = Vector3.Lerp(pointA, pointB, 0.5f);
        segment.transform.localScale = new Vector3(lineWidth * 1.5f + 0.005f, Vector3.Distance(pointA, pointB) / 2, lineWidth * 1.5f + 0.005f);
        segment.transform.LookAt(pointB);
        Vector3 currentAngles = segment.transform.rotation.eulerAngles;
        segment.transform.localRotation = Quaternion.Euler(currentAngles.x + 90, currentAngles.y, currentAngles.z);

        // Smooth edges.
        if (lastSegment != null)
        {
            Vector3 relativeRotation = (Quaternion.Inverse(lastSegment.transform.localRotation) * segment.transform.localRotation).eulerAngles;
            if ((Math.Abs(NormalizeDegreesAroundZero(relativeRotation.x)) > 3) || (Math.Abs(NormalizeDegreesAroundZero(relativeRotation.y)) > 3) || (Math.Abs(NormalizeDegreesAroundZero(relativeRotation.z)) > 3))
            {
                GameObject corner = GameObject.Instantiate(cornerPrefab);
                corner.transform.SetParent(meshModel.transform);
                corner.transform.localScale = new Vector3(lineWidth * 1.5f + 0.005f, lineWidth * 1.5f + 0.005f, lineWidth * 1.5f + 0.005f);
                corner.transform.localPosition = pointA;
            }
        }
        lastSegment = segment;

        // For collider generation.
        meshLine.AddPoint(pointB);
    }
#endregion

#region Highlighting

    public void Highlight(bool hierarchical = true)
    {
        // Highlight the entire drawing.
        foreach (MeshRenderer rend in meshModel.GetComponentsInChildren<MeshRenderer>())
        {
            int rendMatCount = rend.materials.Length;
            Material[] rendMats = new Material[rendMatCount];
            for (int j = 0; j < rendMatCount; j++)
            {
                rendMats[j] = highlightMat;
            }
            rend.materials = rendMats;
        }
    }

    public void Unhighlight(bool hierarchical = true)
    {
        // Unhighlight the entire drawing.
        foreach (MeshRenderer rend in meshModel.GetComponentsInChildren<MeshRenderer>())
        {
            int rendMatCount = rend.materials.Length;
            Material[] rendMats = new Material[rendMatCount];
            for (int j = 0; j < rendMatCount; j++)
            {
                rendMats[j] = lMat;
            }
            rend.materials = rendMats;
        }

    }
#endregion

#region Helpers
    private float DegreesToRadians(float degreeValue)
    {
        return (float) Math.PI * degreeValue / 180;
    }

    private float RadiansToDegrees(float radValue)
    {
        return radValue * (180 / (float) Math.PI);
    }

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

    private float NormalizeAngle(float angleInRads)
    {
        while (angleInRads < 0)
        {
            angleInRads += 2 * (float) Math.PI;
        }

        while (angleInRads > 2 * (float) Math.PI)
        {
            angleInRads -= 2 * (float) Math.PI;
        }
        
        return angleInRads;
    }

    private Vector3 SnapLine(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 snappedEndPoint = endPoint;

        float deltaX = endPoint.x - startPoint.x;
        float deltaY = endPoint.y - startPoint.y;
        float deltaZ = endPoint.z - startPoint.z;
        float deltaXMag = Math.Abs(deltaX);
        float deltaYMag = Math.Abs(deltaY);
        float deltaZMag = Math.Abs(deltaZ);

        double thetaXY = NormalizeAngle((float) Math.Atan(deltaYMag / deltaXMag));
        double thetaXZ = NormalizeAngle((float) Math.Atan(deltaZMag / deltaXMag));
        double thetaYZ = NormalizeAngle((float) Math.Atan(deltaZMag / deltaYMag));
        
        if (thetaXY < DegreesToRadians(22.5f))
        {   // On X-axis in this direction. Y remains constant.
            snappedEndPoint.y = startPoint.y;

            if (thetaXZ < DegreesToRadians(22.5f))
            {   // On X-axis in this direction. Z remains constant.
                snappedEndPoint.z = startPoint.z;
            }
            else if (thetaXZ < DegreesToRadians(67.5f))
            {   // On 45-degree line betw X-Z axes in this direction.
                float greaterDelta = (deltaZMag > deltaXMag) ? deltaZMag : deltaXMag;

                snappedEndPoint.x = (deltaX > 0) ? startPoint.x + greaterDelta : startPoint.x - greaterDelta;
                snappedEndPoint.z = (deltaZ > 0) ? startPoint.z + greaterDelta : startPoint.z - greaterDelta;
            }
            else
            {   // On Z-axis in this direction. X remains constant.
                snappedEndPoint.x = startPoint.x;
            }
        }
        else if (thetaXY < DegreesToRadians(67.5f))
        {   // On 45-degree line betw X-Y axes in this direction.
            float greaterDelta = (deltaYMag > deltaXMag) ? deltaYMag : deltaXMag;
            
            snappedEndPoint.x = (deltaX > 0) ? startPoint.x + greaterDelta : startPoint.x - greaterDelta;
            snappedEndPoint.y = (deltaY > 0) ? startPoint.y + greaterDelta : startPoint.y - greaterDelta;

            if (thetaYZ < DegreesToRadians(22.5f))
            {   // On Y-axis in this direction. Z remains constant.
                snappedEndPoint.z = startPoint.z;
            }
            else if (thetaYZ < DegreesToRadians(67.5f))
            {   // On 45-degree line betw X-Z axes in this direction.
                greaterDelta = (deltaZMag > deltaXMag) ?
                    ((deltaZMag > deltaYMag) ? deltaZMag : deltaYMag) :
                    ((deltaXMag > deltaYMag) ? deltaXMag : deltaYMag);

                snappedEndPoint.x = (deltaX > 0) ? startPoint.x + greaterDelta : startPoint.x - greaterDelta;
                snappedEndPoint.y = (deltaY > 0) ? startPoint.y + greaterDelta : startPoint.y - greaterDelta;
                snappedEndPoint.z = (deltaZ > 0) ? startPoint.z + greaterDelta : startPoint.z - greaterDelta;
            }
            else
            {   // On Z-axis in this direction. Y remains constant.
                snappedEndPoint.y = startPoint.y;
            }
        }
        else
        {   // On Y-axis in this direction. X remains constant.
            snappedEndPoint.x = startPoint.x;

            if (thetaYZ < DegreesToRadians(22.5f))
            {   // On Y-axis in this direction. Z remains constant.
                snappedEndPoint.z = startPoint.z;
            }
            else if (thetaYZ < DegreesToRadians(67.5f))
            {   // On 45-degree line betw X-Z axes in this direction.
                float greaterDelta = (deltaZMag > deltaXMag) ? deltaZMag : deltaXMag;

                snappedEndPoint.y = (deltaY > 0) ? startPoint.y + greaterDelta : startPoint.y - greaterDelta;
                snappedEndPoint.z = (deltaZ > 0) ? startPoint.z + greaterDelta : startPoint.z - greaterDelta;
            }
            else
            {   // On Z-axis in this direction. Y remains constant.
                snappedEndPoint.y = startPoint.y;
            }
        }

        return snappedEndPoint;
    }
#endregion
}