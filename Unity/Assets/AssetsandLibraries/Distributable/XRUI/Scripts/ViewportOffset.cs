// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.XRUI.Viewport
{
    public class ViewportOffset : MonoBehaviour
    {
        public GameObject viewportToOffset;

        public void OffsetViewportHorizontal(int amount)
        {
            if (viewportToOffset)
            {
                Vector3 vPos = viewportToOffset.transform.localPosition;
                viewportToOffset.transform.localPosition = new Vector3(vPos.x + amount, vPos.y, vPos.z);
            }
        }

        public void OffsetViewportVertical(int amount)
        {
            if (viewportToOffset)
            {
                Vector3 vPos = viewportToOffset.transform.localPosition;
                viewportToOffset.transform.localPosition = new Vector3(vPos.x, vPos.y + amount, vPos.z);
            }
        }
    }
}