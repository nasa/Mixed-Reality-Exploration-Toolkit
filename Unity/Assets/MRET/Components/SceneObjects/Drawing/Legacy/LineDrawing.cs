// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing.Legacy;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy
{
    public class LineDrawing : Interactable3dDrawing
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(LineDrawing);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private Drawing3dType serializedDrawing;

        public enum unit { meters, centimeters, millimeters, yards, feet, inches };

        public bool initialized = false;
        public float lineWidth = 0.005f;
        public GameObject meshModel, previewModel;

        private GameObject cablePrefab, cornerPrefab;
        private GameObject parentContainer;
        private Text text;
        private Material lMat, highlightMat;
        private bool isRendered = false;

        // Renderer Types.
        private MeshLineRenderer meshLine, previewLine;

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Initialize defaults
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="InteractableDrawing{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(Drawing3dType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedDrawing = serialized;

            // Make sure to name the mesh model
            meshModel.name = serializedDrawing.Name;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableDrawing{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(Drawing3dType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedDrawing = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        #region DrawingObject
        /// <seealso cref="InteractableDrawing{T}.SetWidth"/>
        public override void SetWidth(float width)
        {
            lineWidth = width;
            if (previewLine != null)
            {
                previewLine.setWidth(lineWidth);
            }
            if (meshLine != null)
            {
                meshLine.setWidth(lineWidth);
            }
        }

        /// <seealso cref="InteractableDrawing{T}.GetWidth"/>
        public override float GetWidth()
        {
            return lineWidth;
        }

        /// <seealso cref="InteractableDrawing{T}.SetMaterial"/>
        public override void SetMaterial(Material material)
        {
            lMat = material;
        }

        /// <seealso cref="InteractableDrawing{T}.GetMaterial"/>
        public override Material GetMaterial()
        {
            return lMat;
        }

        /// <seealso cref="InteractableDrawing{T}.SetGradient"/>
        public override void SetGradient(Gradient gradient)
        {
            // TODO: Not implemented
        }

        /// <seealso cref="InteractableDrawing{T}.GetGradient"/>
        public override Gradient GetGradient()
        {
            // TODO: Not implemented
            return null;
        }

        #region Interactable3dDrawing
        /// <seealso cref="Interactable3dDrawing.RenderType"/>
        public override DrawingRender3dType RenderType => _renderType;
        private DrawingRender3dType _renderType = DrawingRender3dType.Basic;

        /// <seealso cref="Interactable3dDrawing.BeforeRendering"/>
        protected override void BeforeRendering()
        {
            // Take inherited behavior
            base.BeforeRendering();

            // Force a rerender
            Rerender();
        }
        #endregion Intereactable3dDrawing

        #endregion DrawingObject
        public void Initialize(DrawingRender3dType rType, Material mat, Material highlightMaterial, GameObject cable, GameObject corner, float width, GameObject container, bool preview)
        {
            _renderType = rType;
            lMat = mat;
            highlightMat = highlightMaterial;
            cablePrefab = cable;
            cornerPrefab = corner;
            lineWidth = width;
            parentContainer = container;
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

        public void Initialize(Drawing3dType serializedDrawing, Material mat, Material highlightMaterial, GameObject cable, GameObject corner, float width, GameObject container, bool preview)
        {
            Initialize(serializedDrawing.Type, mat, highlightMaterial, cable, corner, width, container, preview);

            // Deserialize the drawing
            Deserialize(serializedDrawing);

            // Filter out small drawings
            if (GetDistance(LengthUnitType.Meter) >= 0.001f)
            {
                MeshCollider coll = meshModel.AddComponent<MeshCollider>();
                coll.convex = true;
                coll.isTrigger = true;
                InteractableDrawingPanel idraw = meshModel.AddComponent<InteractableDrawingPanel>();
                idraw.Grabbable = false;
                idraw.Usable = true;
                idraw.HighlightMaterial = highlightMaterial;
                if (RenderType == DrawingRender3dType.Volumetric)
                {
                    meshModel.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        public void SetRenderType(DrawingRender3dType newRenderType)
        {
            _renderType = newRenderType;

            // Force a rerender
            Rerender();
        }

        public GameObject GetPreviewModel()
        {
            return previewModel;
        }

        public Vector3 GetLastPoint()
        {
            if (points.Length > 0)
            {
                return points[points.Length - 1];
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
            return points.Length;
        }

        public float GetDistance(LengthUnitType units, Vector3 lastPoint)
        {
            float measuredDistance = 0;

            // Get size (magnitude) in meters.
            for (int i = points.Length - 2; i >= 0; i--)
            {
                measuredDistance += Vector3.Distance(points[i + 1], points[i]);
            }
            if (points.Length != 0)
            {
                measuredDistance += Vector3.Distance(points[0], lastPoint);
            }

            // Convert size to desired units.
            return SchemaUtil.UnityUnitsToLength(measuredDistance, units);
        }

        public float GetDistance(LengthUnitType units)
        {
            float measuredDistance = 0;

            // Get size (magnitude) in meters.
            for (int i = points.Length - 2; i >= 0; i--)
            {
                measuredDistance += Vector3.Distance(points[i + 1], points[i]);
            }

            // Convert size to desired units.
            return SchemaUtil.UnityUnitsToLength(measuredDistance, units);
        }

        public Vector3 GetMidpoint()
        {
            int last = points.Length - 1;
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

        public void AddPointAt(int index, Vector3 pointToAdd)
        {
            if (isRendered)
            {
                InsertPoint(index, pointToAdd);
            }
            else
            {
                Debug.Log("Line is not being rendered.");
            }
        }

        public void RemovePointByValue(Vector3 value)
        {
            RemovePoint(value);
        }

        public void RemoveAllPointsByValue(Vector3 value)
        {
            while (RemovePoint(value)) { }
        }

        public void RemovePointByIndex(int pointIndex)
        {
            if (pointIndex > points.Length)
            {
                Debug.LogError("Point index " + pointIndex + " not found.");
            }
            else
            {
                RemovePoint(pointIndex);
            }
        }

        public void Render()
        {
            if (isRendered)
            {
                Debug.Log("Nothing to do... already rendered.");
            }
            else
            {
                InteractableDrawingPanel interactableDrawing;

                switch (RenderType)
                {
                    case DrawingRender3dType.Volumetric:
                        meshModel = new GameObject();
                        meshModel.transform.parent = parentContainer.transform;
                        meshLine = meshModel.AddComponent<MeshLineRenderer>();
                        meshLine.setWidth(lineWidth);
                        meshLine.lmat = lMat;
                        meshLine.hmat = highlightMat;
                        meshLine.drawingScript = this;
                        interactableDrawing = meshModel.AddComponent<InteractableDrawingPanel>();
                        interactableDrawing.enabled = true;
                        interactableDrawing.Usable = true;
                        interactableDrawing.headsetObject = MRET.InputRig.head.transform;
                        interactableDrawing.drawingPanelPrefab = ProjectManager.DrawingManager.drawingPanelPrefab;

                        isRendered = true;
                        initialized = true;
                        break;

                    case DrawingRender3dType.Basic:
                        meshModel = new GameObject();
                        meshModel.transform.parent = parentContainer.transform;
                        meshModel.AddComponent<MeshFilter>();
                        meshModel.AddComponent<MeshRenderer>();
                        meshLine = meshModel.AddComponent<MeshLineRenderer>();
                        meshLine.setWidth(lineWidth);
                        meshLine.lmat = lMat;
                        meshLine.hmat = highlightMat;
                        meshLine.drawingScript = this;
                        interactableDrawing = meshModel.AddComponent<InteractableDrawingPanel>();
                        interactableDrawing.enabled = true;
                        interactableDrawing.Usable = true;
                        interactableDrawing.headsetObject = MRET.InputRig.head.transform;
                        interactableDrawing.drawingPanelPrefab = ProjectManager.DrawingManager.drawingPanelPrefab;

                        isRendered = true;
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
            isRendered = false;
        }

        public void Rerender()
        {
            // Start a new model.
            Unrender();
            Render();

            // Add each point to the model.
            for (int i = 0; i < points.Length; i++)
            {
                if (RenderType == DrawingRender3dType.Volumetric)
                {
                    if (i > 0)
                    {
                        DrawCableBetween(points[i - 1], points[i]);
                    }
                }
                else
                {
                    meshLine.AddPoint(points[i]);
                }
            }
        }

        public Vector3 SetPreviewLine(Vector3 endPoint, bool snap)
        {
            if (points.Length > 0)
            {
                if (snap)
                {
                    endPoint = SnapLine(points[points.Length - 1], endPoint);
                }

                previewLine.MakeSingleLine(points[points.Length - 1], endPoint);
            }
            return endPoint;
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

        #region Helpers
        private float DegreesToRadians(float degreeValue)
        {
            return (float)Math.PI * degreeValue / 180;
        }

        private float RadiansToDegrees(float radValue)
        {
            return radValue * (180 / (float)Math.PI);
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
                angleInRads += 2 * (float)Math.PI;
            }

            while (angleInRads > 2 * (float)Math.PI)
            {
                angleInRads -= 2 * (float)Math.PI;
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

            double thetaXY = NormalizeAngle((float)Math.Atan(deltaYMag / deltaXMag));
            double thetaXZ = NormalizeAngle((float)Math.Atan(deltaZMag / deltaXMag));
            double thetaYZ = NormalizeAngle((float)Math.Atan(deltaZMag / deltaYMag));

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

        protected override void UpdateDrawingColliders()
        {
            // Add components to re-rendered drawing.
            MeshCollider coll = meshModel.AddComponent<MeshCollider>();
            coll.convex = true;
            coll.isTrigger = true;
        }
        #endregion
    }
}