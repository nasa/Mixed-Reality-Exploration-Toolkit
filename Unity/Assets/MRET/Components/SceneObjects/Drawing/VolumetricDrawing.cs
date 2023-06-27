// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Renderer;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// VolumetricDrawing is a class that represents a connected series of lines as
    /// a InteractableSceneObject in a volumetric way.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class VolumetricDrawing : Interactable3dDrawing
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(VolumetricDrawing);

        /// <summary>
        /// Threshold angle for adding a corner.
        /// </summary>
        private static readonly float CORNERTHRESHOLD = 3;

        /// <summary>
        /// Width of the line.
        /// </summary>
        private float _width;

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
        /// Container for meshes in the line.
        /// </summary>
        private Material _material;

        /// <summary>
        /// Segments and their start and end points in the line.
        /// </summary>
        private List<Tuple<Tuple<Vector3, Vector3>, GameObject[]>> segments;

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

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Initialize the internal components
            segments = new List<Tuple<Tuple<Vector3, Vector3>, GameObject[]>>();
            meshContainer = new GameObject("Meshes");
            meshContainer.transform.SetParent(transform);
            segmentPrefab = ProjectManager.DrawingManager.volumetricDrawingSegmentPrefab;
            cornerPrefab = ProjectManager.DrawingManager.volumetricDrawingCornerPrefab;
            _material = new Material(Interactable3dDrawingDefaults.DRAWING_MATERIAL);
        }
        #endregion MRETUpdateBehaviour

        /// <summary>
        /// Create a volumetric drawing.
        /// </summary>
        /// <param name="name">Name of the drawing.</param>
        /// <param name="width">Width of the drawing.</param>
        /// <param name="color">Color of the drawing.</param>
        /// <param name="positions">Positions in the drawing.</param>
        /// <param name="showMeasurement">Whether or not to show the measurement text.</param>
        /// <returns>A <code>IInteractable3dDrawing</code> instance.</returns>
        public static IInteractable3dDrawing Create(string name, float width, Color32 color,
            Vector3[] positions, float cutoff = float.PositiveInfinity, bool showMeasurement = false)
        {
            GameObject vdGO = new GameObject(name);
            VolumetricDrawing volumetricDrawing = vdGO.AddComponent<VolumetricDrawing>();
            volumetricDrawing.DisplayMeasurement = showMeasurement;
            volumetricDrawing.points = positions;
            volumetricDrawing.width = width;
            volumetricDrawing.LengthLimit = cutoff;
            volumetricDrawing.Color = color;
            volumetricDrawing.RefreshState();
            return volumetricDrawing;
        }

        #region InteractableDrawing
        /// <seealso cref="InteractableDrawing{T}.SetWidth(float)"/>
        public override void SetWidth(float width)
        {
            _width = width;

            // Update the internal state
            RefreshState();

            // Render the drawing
            RenderDrawing();
        }

        /// <seealso cref="InteractableDrawing{T}.GetWidth"/>
        public override float GetWidth()
        {
            return _width;
        }

        /// <seealso cref="InteractableDrawing{T}.SetMaterial(Material)"/>
        public override void SetMaterial(Material material)
        {
            _material = new Material(material);
            RendererUtil.ApplyMaterial(meshContainer, _material);
        }

        /// <seealso cref="InteractableDrawing{T}.GetMaterial"/>
        public override Material GetMaterial()
        {
            return _material;
        }

        /// <seealso cref="InteractableDrawing{T}.SetGradient(Gradient)"/>
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

        /// <summary>
        /// Indicates whether or not the subclass is able to process segments of a drawing
        /// </summary>
        protected override bool CanProcessSegments => true;

        /// <summary>
        /// Last segment in the line.
        /// </summary>
        private GameObject lastSegment = null;

        /// <seealso cref="InteractableDrawing.RenderSegment(Vector3, Vector3)"/>
        protected override void RenderSegment(Vector3 pointA, Vector3 pointB)
        {
            List<GameObject> segmentObjects = new List<GameObject>();

            // Instantiate prefab and scale/rotate it.
            GameObject segment = Instantiate(segmentPrefab);
            RendererUtil.ApplyMaterial(segment, Material);
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
                    RendererUtil.ApplyMaterial(corner, Material);
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

        /// <seealso cref="InteractableDrawing.RemoveSegment(Vector3, Vector3)"/>
        protected override void RemoveSegment(Vector3 pointA, Vector3 pointB)
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
                Debug.LogError("[VolumetricDrawing->RemoveSegment] Unable to find segment.");
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
        }
        #endregion InteractableDrawing

        #region Interactable3dDrawing
        /// <seealso cref="Interactable3dDrawing.RenderType"/>
        public override DrawingRender3dType RenderType => DrawingRender3dType.Volumetric;

        /// <seealso cref="Interactable3dDrawing.BeforeRendering"/>
        protected override void BeforeRendering()
        {
            // Take inherited behavior
            base.BeforeRendering();

            // Clear the drawing
            ClearLine();

            // Redraw the drawing
            for (int i = 1; i < points.Length; i++)
            {
                RenderSegment(points[i - 1], points[i]);
            }
        }
        #endregion Intereactable3dDrawing

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

        /// <seealso cref="Interactable3dDrawing.UpdateDrawingColliders"/>
        protected override void UpdateDrawingColliders()
        {
            if (meshContainer == null)
            {
                Debug.LogError("[VolumetricDrawing->SetUpMeshColliders] Not initialized.");
                return;
            }

            // Create the mesh colliders
            foreach(Transform meshObject in meshContainer.transform)
            {
                MeshFilter mf = meshObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    foreach (Collider collider in meshObject.GetComponents<Collider>())
                    {
                        Destroy(collider);
                    }
                    Collider drawingCollider = meshObject.gameObject.AddComponent<MeshCollider>();
                    ((MeshCollider)drawingCollider).sharedMesh = mf.mesh;
                    ((MeshCollider)drawingCollider).convex = true;
                    drawingCollider.isTrigger = true;
                }
            }

            // Make sure we have a rigid body
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
        }
    }
}