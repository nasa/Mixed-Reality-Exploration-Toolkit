// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// LineDrawing is a class that represents a connected series of lines as
    /// an interactable drawing.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class LineDrawing : Interactable3dDrawing
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(LineDrawing);

        /// <summary>
        /// The line renderer.
        /// </summary>
        private LineRenderer lineRenderer;

        /// <summary>
        /// The collider.
        /// </summary>
        protected Collider lineCollider;

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
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;

            // Initialize defaults
            TouchBehavior = IInteractable.TouchBehaviors.Hold;
        }
        #endregion MRETUpdateBehaviour

        /// <summary>
        /// Create a line drawing.
        /// </summary>
        /// <param name="name">Name of the line drawing.</param>
        /// <param name="width">Width of the line drawing.</param>
        /// <param name="color">Color of the line drawing.</param>
        /// <param name="positions">Positions in the line drawing.</param>
        /// <param name="cutoff">Cutoff to give the line drawing.</param>
        /// <param name="showMeasurement">Whether or not to show the measurement text.</param>
        /// <returns>The specified line drawing.</returns>
        public static IInteractable3dDrawing Create(string name, float width, Color32 color,
            Vector3[] positions, float cutoff = float.PositiveInfinity, bool showMeasurement = false)
        {
            GameObject ldGO = new GameObject(name);
            LineDrawing lineDrawing = ldGO.AddComponent<LineDrawing>();
            lineDrawing.LengthLimit = cutoff;
            lineDrawing.DisplayMeasurement = showMeasurement;
            lineDrawing.points = positions;
            lineDrawing.width = width;
            lineDrawing.Color = color;
            lineDrawing.RefreshState();
            return lineDrawing;
        }

        #region InteractableDrawing
        /// <seealso cref="InteractableDrawing{T}.SetWidth(float)"/>
        public override void SetWidth(float width)
        {
            if (lineRenderer == null)
            {
                LogError("Not initialized.", nameof(SetWidth));
                return;
            }

            lineRenderer.startWidth = lineRenderer.endWidth = width;

            // Update the internal state
            RefreshState();

            // Render the drawing
            RenderDrawing();
        }

        /// <seealso cref="InteractableDrawing{T}.GetWidth"/>
        public override float GetWidth()
        {
            if (lineRenderer == null)
            {
                LogError("Not initialized.", nameof(GetWidth));
                return 0;
            }

            return lineRenderer.startWidth;
        }

        /// <seealso cref="InteractableDrawing{T}.SetMaterial(Material)"/>
        public override void SetMaterial(Material material)
        {
            if (lineRenderer == null)
            {
                LogError("Not initialized.", nameof(SetMaterial));
                return;
            }

            lineRenderer.material = material;
        }

        /// <seealso cref="InteractableDrawing{T}.GetMaterial"/>
        public override Material GetMaterial()
        {
            if (lineRenderer == null)
            {
                LogError("Not initialized.", nameof(SetMaterial));
                return null;
            }

            return lineRenderer.material;
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
        #endregion InteractableDrawing

        #region Interactable3dDrawing
        /// <seealso cref="Interactable3dDrawing.RenderType"/>
        public override DrawingRender3dType RenderType => DrawingRender3dType.Basic;

        /// <seealso cref="Interactable3dDrawing.BeforeRendering"/>
        protected override void BeforeRendering()
        {
            // Take inherited behavior
            base.BeforeRendering();

            // Update the line renderer points
            Vector3[] pointsArray = points;
            lineRenderer.positionCount = pointsArray.Length;
            if (lineRenderer.positionCount > 0)
            {
                lineRenderer.SetPositions(pointsArray);
            }
        }
        #endregion Intereactable3dDrawing

        public virtual void ColorSegment(Vector3 pointA, Vector3 pointB)
        {
            Vector3[] pointValues = points;
            for (int i = 1; i < pointValues.Length; i++)
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
        /// Gets a mesh for the line.
        /// </summary>
        /// <returns>Mesh for the line.</returns>
        private Mesh GetMesh()
        {
            if (lineRenderer == null)
            {
                LogError("Not initialized.", nameof(GetMesh));
                return null;
            }

            Mesh mesh = null;
            if (points.Length > 1)
            {
                mesh = new Mesh();
                lineRenderer.BakeMesh(mesh);
            }
            return mesh;
        }

        /// <seealso cref="Interactable3dDrawing.UpdateDrawingColliders"/>
        protected override void UpdateDrawingColliders()
        {
            if (lineRenderer == null)
            {
                LogError("Not initialized.", nameof(UpdateDrawingColliders));
                return;
            }

            // Make sure we have points for a mesh
            if (points.Length > 1)
            {
                Mesh mesh = GetMesh();
                if (mesh == null)
                {
                    LogError("No mesh currently available to update the drawing colliders.", nameof(UpdateDrawingColliders));
                    return;
                }

                if (lineCollider != null)
                {
                    Destroy(lineCollider);
                    lineCollider = null;
                }

                lineCollider = gameObject.AddComponent<MeshCollider>();
                // Tyler Kitts: I've set collider to non-convex because otherwise it causes errors, this means
                // it cannot have collision detection. 
                // Notably the Volumetric drawings don't have colliders anyways so why does the 2d version have it?
                ((MeshCollider) lineCollider).convex = false;
                //lineCollider.isTrigger = true;
                if (mesh.vertexCount > 0)
                {
                    ((MeshCollider)lineCollider).sharedMesh = mesh;
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