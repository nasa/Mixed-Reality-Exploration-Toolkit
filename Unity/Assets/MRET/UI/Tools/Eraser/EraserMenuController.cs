// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Tools.Eraser;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRUI;

namespace GOV.NASA.GSFC.XR.MRET.UI.Tools.Eraser
{
    public class EraserMenuController : MenuController
    {
        public static readonly string eraserKey = "MRET.INTERNAL.TOOLS.ERASER";

        public EraserManager eraserManager;

        public override void Initialize()
        {
            MRET.DataManager.SaveValue(eraserKey, false);
        }

        public void SwitchEraser()
        {
            SwitchEraser(!((bool)MRET.DataManager.FindPoint(eraserKey)));
        }

        public void SwitchEraser(bool on)
        {
            if (on)
            {
                eraserManager.Enable();
                MRET.ControlMode.EnterEraserMode();
            }
            else
            {
                eraserManager.Disable();
                if (MRET.ControlMode.activeControlType == ControlMode.ControlType.Eraser)
                {
                    MRET.ControlMode.DisableAllControlTypes();
                }
            }

            // Save to DataManager.
            MRET.DataManager.SaveValue(new DataManager.DataValue(eraserKey, on));
        }

        public void ExitMode()
        {
            eraserManager.Disable();

            // Save to DataManager.
            MRET.DataManager.SaveValue(new DataManager.DataValue(eraserKey, false));
        }
    }
}