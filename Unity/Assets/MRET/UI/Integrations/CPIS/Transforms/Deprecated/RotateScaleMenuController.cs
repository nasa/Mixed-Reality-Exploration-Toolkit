// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms;

namespace GOV.NASA.GSFC.XR.MRET.UI.Integrations.CPIS.Transforms
{
    public class RotateScaleMenuController : MonoBehaviour
    {
        public GameObject rotationCheck, scalingCheck, rotationX, scalingX;
        public RotateObjectTransform leftROT, rightROT;
        public ScaleObjectTransform leftSOT, rightSOT;

        public void OnEnable()
        {
            SwitchRotation(rightROT.enabled);
            SwitchScaling(leftSOT.enabled);
        }

        public void ToggleRotation()
        {
            SwitchRotation(!leftROT.enabled);
        }

        public void ToggleScaling()
        {
            SwitchScaling(!leftSOT.enabled);
        }

        public void SwitchRotation(bool on)
        {
            if (on)
            {
                rotationCheck.SetActive(true);
                rotationX.SetActive(false);
                leftROT.enabled = rightROT.enabled = true;
            }
            else
            {
                rotationCheck.SetActive(false);
                rotationX.SetActive(true);
                leftROT.enabled = rightROT.enabled = false;
            }
        }

        public void SwitchScaling(bool on)
        {
            if (on)
            {
                scalingCheck.SetActive(true);
                scalingX.SetActive(false);
                leftSOT.enabled = rightSOT.enabled = true;
            }
            else
            {
                scalingCheck.SetActive(false);
                scalingX.SetActive(true);
                leftSOT.enabled = rightSOT.enabled = false;
            }
        }
    }
}