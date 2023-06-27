// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.LineOfSight
{
    public class LineOfSightSource : MonoBehaviour
    {
        public float losLineWidth = 0.05f;
        public Material losLineMaterial;
        public bool directional = true;
        public List<LineOfSightTarget> nondirectionalTargets;

        private LineRenderer losLine;

        void Start()
        {
            losLine = gameObject.AddComponent<LineRenderer>();
            losLine.widthMultiplier = losLineWidth;
            losLine.material = losLineMaterial;
            losLine.useWorldSpace = true;
            losLine.positionCount = 0;
        }

        void Update()
        {
            if (directional)
            {
                LineOfSightTarget hitTarget = TestLineOfSightDirectional();
                if (hitTarget == null)
                {
                    DestroyLine();
                }
                else
                {
                    SetLine(transform.position, hitTarget.transform.position);
                }
            }
            else
            {
                DestroyLine();
                if (nondirectionalTargets != null)
                {
                    foreach (LineOfSightTarget target in nondirectionalTargets)
                    {
                        if (TestLineOfSightNondirectional(target))
                        {
                            AddLine(transform.position, target.transform.position);
                        }
                    }
                }
            }
        }

        private LineOfSightTarget TestLineOfSightDirectional()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                return hit.collider.gameObject.GetComponent<LineOfSightTarget>();
            }
            return null;
        }

        private bool TestLineOfSightNondirectional(LineOfSightTarget target)
        {
            RaycastHit hit;
            if (Physics.Linecast(transform.position, target.transform.position, out hit))
            {
                if (hit.collider.gameObject.GetComponent<LineOfSightTarget>() == target)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void SetLine(Vector3 start, Vector3 end)
        {
            losLine.positionCount = 2;
            losLine.SetPosition(0, start);
            losLine.SetPosition(1, end);
        }

        private void AddLine(Vector3 start, Vector3 end)
        {
            losLine.positionCount += 2;
            losLine.SetPosition(losLine.positionCount - 2, start);
            losLine.SetPosition(losLine.positionCount - 1, end);
        }

        private void DestroyLine()
        {
            losLine.positionCount = 0;
        }
    }
}