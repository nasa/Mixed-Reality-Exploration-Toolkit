// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing
{
    public class LineDrawingMenuController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(LineDrawingMenuController);

        public VR_InputField widthText, cutoffText;
        public Dropdown drawingModeDropdown, drawingTypeDropdown, drawingUnitsDropdown;

        private LengthUnitType units = LengthUnitType.Meter;

        private float Width
        {
            get
            {
                float result = -1;

                if ((widthText != null) && !string.IsNullOrEmpty(widthText.text) && float.TryParse(widthText.text, out float width))
                {
                    result = SchemaUtil.LengthToUnityUnits(width, units);
                }
                else
                {
                    LogError("Failed to convert width.", nameof(Width));
                }

                return result;
            }
        }

        private float LengthLimit
        {
            get
            {
                float result = -1;

                if ((cutoffText != null) && !string.IsNullOrEmpty(cutoffText.text) && float.TryParse(cutoffText.text, out float cutoff))
                {
                    result = SchemaUtil.LengthToUnityUnits(cutoff, units);
                }
                else
                {
                    result = Interactable3dDrawingDefaults.LIMIT_LENGTH;
                }

                return result;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Initialize drawing mode.
            LineDrawingController.DrawingMode mode = LineDrawingController.DrawingMode.None;
            DrawingRender3dType type = DrawingRender3dType.Basic;
            float width = 1;
            foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
            {
                mode = lineDrawingController.drawingMode;
                type = lineDrawingController.drawingType;
                width = lineDrawingController.drawingWidth;
            }

            switch (mode)
            {
                //case DrawLineManager.CaptureTypes.None:
                case LineDrawingController.DrawingMode.None:
                    drawingModeDropdown.value = 0;
                    break;

                //case DrawLineManager.CaptureTypes.Free:
                case LineDrawingController.DrawingMode.Free:
                    drawingModeDropdown.value = 1;
                    break;

                //case DrawLineManager.CaptureTypes.Lines:
                case LineDrawingController.DrawingMode.Straight:
                    drawingModeDropdown.value = 2;
                    break;

                //case DrawLineManager.CaptureTypes.Laser:
                case LineDrawingController.DrawingMode.Laser:
                    drawingModeDropdown.value = 3;
                    break;

                default:
                    drawingModeDropdown.value = 0;
                    break;
            }

            // Initialize drawing style.
            //switch (drawLineManager.renderType)
            switch (type)
            {
                //case LineDrawing.RenderTypes.Drawing:
                case DrawingRender3dType.Basic:
                    drawingTypeDropdown.value = 0;
                    break;

                //case LineDrawing.RenderTypes.Cable:
                case DrawingRender3dType.Volumetric:
                    drawingTypeDropdown.value = 1;
                    break;

                default:
                    drawingTypeDropdown.value = 0;
                    break;
            }
            drawingUnitsDropdown.value = 0;

            // Initialize drawing width.
            widthText.text = width.ToString();

            // Initialize drawing length limit
            //cutoffText.text = drawLineManager.cableCutoff == 0 ? "" : drawLineManager.cableCutoff.ToString();

            // Handle Drawing Option Updates.
            drawingModeDropdown.onValueChanged.AddListener(delegate
            {
                HandleDrawingModeChange(drawingModeDropdown);
            });
            drawingTypeDropdown.onValueChanged.AddListener(delegate
            {
                HandleDrawingTypeChange(drawingTypeDropdown);
            });
            drawingUnitsDropdown.onValueChanged.AddListener(delegate
            {
                HandleDrawingUnitsChange(drawingUnitsDropdown);
            });
        }

        private string previousWidthValue = "", previousCutoffValue = "";
        void Update()
        {
            if (widthText.text != previousWidthValue)
            {
                previousWidthValue = widthText.text;
                SetLineWidth(widthText);
            }

            if (cutoffText.text != previousCutoffValue)
            {
                previousCutoffValue = cutoffText.text;
                SetLineCutoff(cutoffText);
            }
        }

        void Destroy()
        {
            drawingModeDropdown.onValueChanged.RemoveAllListeners();
            drawingTypeDropdown.onValueChanged.RemoveAllListeners();
        }

        public void HandleDrawingModeChange(Dropdown target)
        {
            switch (target.value)
            {
                // Off.
                case 0:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingMode = LineDrawingController.DrawingMode.None;
                    }
                    MRET.ControlMode.DisableAllControlTypes();
                    break;

                // Freeform.
                case 1:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingMode = LineDrawingController.DrawingMode.Free;
                    }
                    break;

                // Straight.
                case 2:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingMode = LineDrawingController.DrawingMode.Straight;
                    }
                    break;

                // Laser.
                case 3:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingMode = LineDrawingController.DrawingMode.Laser;
                    }
                    break;

                // Spline.
                case 4:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingMode = LineDrawingController.DrawingMode.None;
                    }
                    MRET.ControlMode.DisableAllControlTypes();
                    LogWarning("This option is currently unavailable", nameof(HandleDrawingModeChange));
                    break;

                // Unknown.
                default:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingMode = LineDrawingController.DrawingMode.None;
                    }
                    MRET.ControlMode.DisableAllControlTypes();
                    LogWarning("Unknown State", nameof(HandleDrawingModeChange));
                    break;
            }
            SetLineWidth(widthText);
            SetLineCutoff(cutoffText);
            RestartDrawingMode();
        }

        public void HandleDrawingTypeChange(Dropdown target)
        {
            switch (target.value)
            {
                // Basic
                case 0:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingType = DrawingRender3dType.Basic;
                    }
                    break;

                // Cable.
                case 1:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingType = DrawingRender3dType.Volumetric;
                    }
                    break;

                // Unknown.
                default:
                    foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
                    {
                        lineDrawingController.drawingType = DrawingRender3dType.Basic;
                    }
                    LogWarning("Unknown State", nameof(HandleDrawingTypeChange));
                    break;
            }
            SetLineWidth(widthText);
            SetLineCutoff(cutoffText);
            RestartDrawingMode();
        }

        public void HandleDrawingUnitsChange(Dropdown target)
        {
            switch (target.value)
            {
                // Meters.
                case 0:
                    units = LengthUnitType.Meter;
                    break;

                // Centimeters.
                case 1:
                    units = LengthUnitType.Centimeter;
                    break;

                // Millimeters.
                case 2:
                    units = LengthUnitType.Millimeter;
                    break;

                // Yards.
                case 3:
                    units = LengthUnitType.Yard;
                    break;

                // Feet.
                case 4:
                    units = LengthUnitType.Foot;
                    break;

                // Inches:
                case 5:
                    units = LengthUnitType.Inch;
                    break;

                // Unknown.
                default:
                    units = LengthUnitType.Meter;
                    LogWarning("Unknown State", nameof(HandleDrawingUnitsChange));
                    break;
            }
            SetLineWidth(widthText);
            SetLineCutoff(cutoffText);
        }

        public void SetLineWidth(VR_InputField widthToSet)
        {
            if (widthToSet.text != "")
            {
                SetLineWidth(float.Parse(widthToSet.text));
            }
        }

        public void SetLineWidth(float widthToSet)
        {
            // TODO: Will need to limit this to a certain range.
            float convertedWidth = widthToSet;
            foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
            {
                lineDrawingController.drawingWidth = Width;
            }
            RestartDrawingMode();
        }

        public void SetLineCutoff(VR_InputField cutoffToSet)
        {
            // TODO: Will need to limit this to a certain range.
            foreach (LineDrawingController lineDrawingController in ProjectManager.DrawingManager.lineDrawingControllers)
            {
                lineDrawingController.LengthLimit = LengthLimit;
            }
            if (cutoffToSet.text != "")
            {
                float convertedCutoff = float.Parse(cutoffToSet.text);
                RestartDrawingMode();
            }
        }

        #region Helpers
        private void RestartDrawingMode()
        {
            /*if (lineCaptureType == DrawLineManager.CaptureTypes.Free)
            {
                switch (lineRenderType)
                {
                    case LineDrawing.RenderTypes.Drawing:
                        drawLineManager.StartFreeformDrawings();
                        break;

                    case LineDrawing.RenderTypes.Cable:
                        drawLineManager.StartFreeformCables();
                        break;

                    case LineDrawing.RenderTypes.Measurement:
                        drawLineManager.StartFreeformMeasurements();
                        break;
                    default:
                        drawLineManager.ExitDrawings();
                        Debug.LogWarning("[ModeNavigator->RestartDrawingMode] Unknown State");
                        break;
                }
            }
            else if (lineCaptureType == DrawLineManager.CaptureTypes.Lines)
            {
                switch (lineRenderType)
                {
                    case LineDrawing.RenderTypes.Drawing:
                        drawLineManager.StartStraightDrawings();
                        break;

                    case LineDrawing.RenderTypes.Cable:
                        drawLineManager.StartStraightCables();
                        break;

                    case LineDrawing.RenderTypes.Measurement:
                        drawLineManager.StartStraightMeasurements();
                        break;

                    default:
                        drawLineManager.ExitDrawings();
                        Debug.LogWarning("[ModeNavigator->RestartDrawingMode] Unknown State");
                        break;
                }
            }
            else if (lineCaptureType == DrawLineManager.CaptureTypes.Laser)
            {
                switch (lineRenderType)
                {
                    case LineDrawing.RenderTypes.Drawing:
                        drawLineManager.StartLaserDrawings();
                        break;

                    case LineDrawing.RenderTypes.Cable:
                        drawLineManager.StartLaserCables();
                        break;

                    case LineDrawing.RenderTypes.Measurement:
                        drawLineManager.StartLaserMeasurements();
                        break;

                    default:
                        drawLineManager.ExitDrawings();
                        Debug.LogWarning("[ModeNavigator->RestartDrawingMode] Unknown State");
                        break;
                }
            }*/
        }

        #endregion
    }
}