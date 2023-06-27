// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using LegacyLineDrawing = GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy.LineDrawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MeshLineRenderer : InteractableSceneObject<InteractableSceneObjectType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MeshLineRenderer);

        public Material lmat;

        public Material hmat;

        public LegacyLineDrawing drawingScript;

        private Mesh ml;

        private Vector3 s;

        private float lineSize = 0.001f;

        private bool firstQuad = true;

        private Vector3 lastGoodOrientation;

        #region MRETBehaviour
        protected override void MRETAwake()
        {
            base.MRETAwake();

            ml = GetComponent<MeshFilter>().mesh;
            Material[] renderMat = new Material[1];
            renderMat[0] = lmat;
            GetComponent<MeshRenderer>().materials = renderMat;
        }
        #endregion MRETBehaviour

        public void setWidth(float width)
        {
            lineSize = width;
        }

        public void MakeSingleLine(Vector3 start, Vector3 end)
        {
            Vector3[] quad = MakeQuad(start, end, lineSize, true);

            Vector3[] vs = new Vector3[2 * quad.Length];

            for (int i = 0; i < 2 * quad.Length; i += 2)
            {
                vs[i] = quad[i / 2];
                vs[i + 1] = quad[i / 2];
            }

            Vector2[] uvs = new Vector2[2 * quad.Length];

            if (quad.Length == 4)
            {
                uvs[0] = Vector2.zero;
                uvs[1] = Vector2.zero;
                uvs[2] = Vector2.right;
                uvs[3] = Vector2.right;
                uvs[4] = Vector2.up;
                uvs[5] = Vector2.up;
                uvs[6] = Vector2.one;
                uvs[7] = Vector2.one;
            }
            else
            {
                uvs[0] = Vector2.zero;
                uvs[1] = Vector2.zero;
                uvs[2] = Vector2.right;
                uvs[3] = Vector2.right;
            }

            int tl = 0;

            int[] ts = null;
            ts = new int[12];

            // front-facing quad
            ts[tl] = 0;
            ts[tl + 1] = 2;
            ts[tl + 2] = 4;

            ts[tl + 3] = 2;
            ts[tl + 4] = 6;
            ts[tl + 5] = 4;

            // back-facing quad
            ts[tl + 6] = 5;
            ts[tl + 7] = 3;
            ts[tl + 8] = 1;

            ts[tl + 9] = 5;
            ts[tl + 10] = 7;
            ts[tl + 11] = 3;

            ml.vertices = vs;
            ml.uv = uvs;
            ml.triangles = ts;
            ml.RecalculateBounds();
            ml.RecalculateNormals();

            Material[] renderMat = new Material[1];
            renderMat[0] = lmat;
            GetComponent<MeshRenderer>().materials = renderMat;
        }

        public void StartNewLine(Vector3 point)
        {
            MakeSingleLine(point, point);
        }

        public void AddPoint(Vector3 point)
        {
            if (s != Vector3.zero)
            {
                AddLine(ml, MakeQuad(s, point, lineSize, firstQuad));
                firstQuad = false;
            }

            s = point;

            Material[] renderMat = new Material[1];
            renderMat[0] = lmat;
            GetComponent<MeshRenderer>().materials = renderMat;
        }

        Vector3[] MakeQuad(Vector3 s, Vector3 e, float w, bool all)
        {
            w = w / 2;

            Vector3[] q;
            if (all)
            {
                q = new Vector3[4];
            }
            else
            {
                q = new Vector3[2];
            }

            Vector3 n = Vector3.Cross(s, e);
            Vector3 l = Vector3.Cross(n, e - s);

            if (l != Vector3.zero)
            {
                lastGoodOrientation = l;
            }
            else
            {
                l = lastGoodOrientation;
            }

            l.Normalize();

            if (all)
            {
                q[0] = transform.InverseTransformPoint(s + l * w);
                q[1] = transform.InverseTransformPoint(s + l * -w);
                q[2] = transform.InverseTransformPoint(e + l * w);
                q[3] = transform.InverseTransformPoint(e + l * -w);
            }
            else
            {
                q[0] = transform.InverseTransformPoint(e + l * w);
                q[1] = transform.InverseTransformPoint(e + l * -w);
            }
            return q;
        }

        void AddLine(Mesh m, Vector3[] quad)
        {
            int vl = m.vertices.Length;

            Vector3[] vs = m.vertices;
            vs = resizeVertices(vs, 2 * quad.Length);

            for (int i = 0; i < 2 * quad.Length; i += 2)
            {
                vs[vl + i] = quad[i / 2];
                vs[vl + i + 1] = quad[i / 2];
            }

            Vector2[] uvs = m.uv;
            uvs = resizeUVs(uvs, 2 * quad.Length);

            if (quad.Length == 4)
            {
                uvs[vl] = Vector2.zero;
                uvs[vl + 1] = Vector2.zero;
                uvs[vl + 2] = Vector2.right;
                uvs[vl + 3] = Vector2.right;
                uvs[vl + 4] = Vector2.up;
                uvs[vl + 5] = Vector2.up;
                uvs[vl + 6] = Vector2.one;
                uvs[vl + 7] = Vector2.one;
            }
            else
            {
                if (vl % 8 == 0)
                {
                    uvs[vl] = Vector2.zero;
                    uvs[vl + 1] = Vector2.zero;
                    uvs[vl + 2] = Vector2.right;
                    uvs[vl + 3] = Vector2.right;

                }
                else
                {
                    uvs[vl] = Vector2.up;
                    uvs[vl + 1] = Vector2.up;
                    uvs[vl + 2] = Vector2.one;
                    uvs[vl + 3] = Vector2.one;
                }
            }

            int tl = m.triangles.Length;

            int[] ts = m.triangles;
            ts = resizeTriangles(ts, 12);

            if (quad.Length == 2)
            {
                vl -= 4;
            }

            // front-facing quad
            ts[tl] = vl;
            ts[tl + 1] = vl + 2;
            ts[tl + 2] = vl + 4;

            ts[tl + 3] = vl + 2;
            ts[tl + 4] = vl + 6;
            ts[tl + 5] = vl + 4;

            // back-facing quad
            ts[tl + 6] = vl + 5;
            ts[tl + 7] = vl + 3;
            ts[tl + 8] = vl + 1;

            ts[tl + 9] = vl + 5;
            ts[tl + 10] = vl + 7;
            ts[tl + 11] = vl + 3;

            m.vertices = vs;
            m.uv = uvs;
            m.triangles = ts;
            m.RecalculateBounds();
            m.RecalculateNormals();
        }

        Vector3[] resizeVertices(Vector3[] ovs, int ns)
        {
            Vector3[] nvs = new Vector3[ovs.Length + ns];
            for (int i = 0; i < ovs.Length; i++)
            {
                nvs[i] = ovs[i];
            }

            return nvs;
        }

        Vector2[] resizeUVs(Vector2[] uvs, int ns)
        {
            Vector2[] nvs = new Vector2[uvs.Length + ns];
            for (int i = 0; i < uvs.Length; i++)
            {
                nvs[i] = uvs[i];
            }

            return nvs;
        }

        int[] resizeTriangles(int[] ovs, int ns)
        {
            int[] nvs = new int[ovs.Length + ns];
            for (int i = 0; i < ovs.Length; i++)
            {
                nvs[i] = ovs[i];
            }

            return nvs;
        }


        #region Selection

        protected override void AfterSelect(bool hierarchical = true)
        {
            base.AfterSelect(hierarchical);

            // Highlight the drawing
            drawingScript.Select(hierarchical);
        }

        protected override void AfterDeselect(bool hierarchical = true)
        {
            base.AfterDeselect(hierarchical);

            // Unhighlight the drawing
            drawingScript.Deselect(hierarchical);
        }
        #endregion
    }
}