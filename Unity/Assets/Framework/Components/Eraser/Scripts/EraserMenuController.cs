// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Integrations.XRUI;

namespace GSFC.ARVR.MRET.Components.Eraser
{
    public class EraserMenuController : MenuController
    {
        public static readonly string eraserKey = "MRET.INTERNAL.TOOLS.ERASER";

        public EraserManager eraserManager;

        public override void Initialize()
        {
            Infrastructure.Framework.MRET.DataManager.SaveValue(eraserKey, false);
        }

        public void SwitchEraser()
        {
            SwitchEraser(!((bool) DataManager.instance.FindPoint(eraserKey)));
        }

        public void SwitchEraser(bool on)
        {
            if (on)
            {
                eraserManager.Enable();
                Infrastructure.Framework.MRET.ControlMode.EnterEraserMode();
            }
            else
            {
                eraserManager.Disable();
                if (Infrastructure.Framework.MRET.ControlMode.activeControlType == ControlMode.ControlType.Eraser)
                {
                    Infrastructure.Framework.MRET.ControlMode.DisableAllControlTypes();
                }
            }

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(eraserKey, on));
        }

        public void ExitMode()
        {
            eraserManager.Disable();

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(eraserKey, false));
        }
    }
}