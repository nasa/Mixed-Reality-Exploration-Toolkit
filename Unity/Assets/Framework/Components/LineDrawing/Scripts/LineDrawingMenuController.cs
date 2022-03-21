// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Components.LineDrawing;
using GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing;
using GSFC.ARVR.MRET.Infrastructure.Framework;
using GSFC.ARVR.MRET.Components.UI;

public class LineDrawingMenuController : MonoBehaviour
{
    public VR_InputField widthText, cutoffText;
    public Dropdown drawingModeDropdown, drawingTypeDropdown, drawingUnitsDropdown;

    private LineDrawingManager.Units units = LineDrawingManager.Units.meters;

    private float widthInMeters
    {
        get
        {
            if (widthText.text == "")
            {
                return 0;
            }
            float width = float.Parse(widthText.text);
            switch (units)
            {
                case LineDrawingManager.Units.meters:
                    return width;

                case LineDrawingManager.Units.centimeters:
                    return width / 100;

                case LineDrawingManager.Units.millimeters:
                    return width / 1000;

                case LineDrawingManager.Units.micrometers:
                    return width / 1000000;

                case LineDrawingManager.Units.nanometers:
                    return width / 1000000000;

                case LineDrawingManager.Units.kilometers:
                    return width * 1000;

                case LineDrawingManager.Units.inches:
                    return width / 39.3701f;

                case LineDrawingManager.Units.feet:
                    return width / 3.28084f;

                case LineDrawingManager.Units.yards:
                    return width / 1.09361f;

                case LineDrawingManager.Units.miles:
                    return width * 1609.34f;

                default:
                    Debug.LogError("[LineDrawingMenuController->widthInMeters] Invalid units.");
                    return -1;
            }
        }
    }

    private float cutoffInMeters
    {
        get
        {
            if (cutoffText.text == "")
            {
                return -1;
            }
            float cutoff = float.Parse(cutoffText.text);
            switch (units)
            {
                case LineDrawingManager.Units.meters:
                    return cutoff;

                case LineDrawingManager.Units.centimeters:
                    return cutoff / 100;

                case LineDrawingManager.Units.millimeters:
                    return cutoff / 1000;

                case LineDrawingManager.Units.micrometers:
                    return cutoff / 1000000;

                case LineDrawingManager.Units.nanometers:
                    return cutoff / 1000000000;

                case LineDrawingManager.Units.kilometers:
                    return cutoff * 1000;

                case LineDrawingManager.Units.inches:
                    return cutoff / 39.3701f;

                case LineDrawingManager.Units.feet:
                    return cutoff / 3.28084f;

                case LineDrawingManager.Units.yards:
                    return cutoff / 1.09361f;

                case LineDrawingManager.Units.miles:
                    return cutoff * 1609.34f;

                default:
                    Debug.LogError("[LineDrawingMenuController->cutoffInMeters] Invalid units.");
                    return -1;
            }
        }
    }

    //private DrawLineManager drawLineManager;
    //private DrawLineManager.CaptureTypes lineCaptureType = DrawLineManager.CaptureTypes.None;
    //private LineDrawing.RenderTypes lineRenderType = LineDrawing.RenderTypes.Drawing;

    public void Start()
    {
        //drawLineManager = FindObjectOfType<DrawLineManager>();
        //lineCaptureType = drawLineManager.captureType;
        //lineRenderType = drawLineManager.renderType;

        // Initialize drawing mode.
        //switch (drawLineManager.captureType)
        LineDrawingController.DrawingMode mode = LineDrawingController.DrawingMode.None;
        LineDrawingManager.DrawingType type = LineDrawingManager.DrawingType.Basic;
        float width = 1;
        foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
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
            case LineDrawingManager.DrawingType.Basic:
                drawingTypeDropdown.value = 0;
                break;

            //case LineDrawing.RenderTypes.Cable:
            case LineDrawingManager.DrawingType.Volumetric:
                drawingTypeDropdown.value = 1;
                break;

            //case LineDrawing.RenderTypes.Measurement:
            //    drawingTypeDropdown.value = 2;
            //    break;

            default:
                drawingTypeDropdown.value = 0;
                break;
        }

        // Initialize drawing units.
        /*switch (drawLineManager.desiredUnits)
        {
            case LineDrawing.unit.meters:
                drawingUnitsDropdown.value = 0;
                break;

            case LineDrawing.unit.centimeters:
                drawingUnitsDropdown.value = 1;
                break;

            case LineDrawing.unit.millimeters:
                drawingUnitsDropdown.value = 2;
                break;

            case LineDrawing.unit.yards:
                drawingUnitsDropdown.value = 3;
                break;

            case LineDrawing.unit.feet:
                drawingUnitsDropdown.value = 4;
                break;

            case LineDrawing.unit.inches:
                drawingUnitsDropdown.value = 5;
                break;

            default:
                drawingUnitsDropdown.value = 0;
                break;
        }*/
        drawingUnitsDropdown.value = 0;

        // Initialize drawing width.
        //widthText.text = drawLineManager.lineWidth.ToString();
        widthText.text = width.ToString();

        // Initialize drawing cutoff.
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
                //lineCaptureType = DrawLineManager.CaptureTypes.None;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingMode = LineDrawingController.DrawingMode.None;
                }
                //drawLineManager.ExitDrawings();
                MRET.ControlMode.DisableAllControlTypes();
                break;

            // Freeform.
            case 1:
                //lineCaptureType = DrawLineManager.CaptureTypes.Free;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingMode = LineDrawingController.DrawingMode.Free;
                }
                //MRET.ControlMode.EnterDrawingMode();
                break;

            // Straight.
            case 2:
                //lineCaptureType = DrawLineManager.CaptureTypes.Lines;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingMode = LineDrawingController.DrawingMode.Straight;
                }
                //MRET.ControlMode.EnterDrawingMode();
                break;

            // Laser.
            case 3:
                //lineCaptureType = DrawLineManager.CaptureTypes.Laser;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingMode = LineDrawingController.DrawingMode.Laser;
                }
                //MRET.ControlMode.EnterDrawingMode();
                break;

            // Spline.
            case 4:
                //lineCaptureType = DrawLineManager.CaptureTypes.None;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingMode = LineDrawingController.DrawingMode.None;
                }
                //drawLineManager.ExitDrawings();
                MRET.ControlMode.DisableAllControlTypes();
                Debug.LogWarning("[ModeNavigator->HandleDrawingModeChange] This option is currently unavailable.");
                break;

            // Unknown.
            default:
                //lineCaptureType = DrawLineManager.CaptureTypes.None;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingMode = LineDrawingController.DrawingMode.None;
                }
                MRET.ControlMode.DisableAllControlTypes();
                Debug.LogWarning("[ModeNavigator->HandleDrawingModeChange] Unknown State");
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
            // Drawing.
            case 0:
                //lineRenderType = LineDrawing.RenderTypes.Drawing;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingType = GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Basic;
                }
                break;

            // Cable.
            case 1:
                //lineRenderType = LineDrawing.RenderTypes.Cable;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingType = GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Volumetric;
                }
                break;

            // Measurement.
            case 2:
                //lineRenderType = LineDrawing.RenderTypes.Measurement;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingType = GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Basic;
                }
                break;

            // Unknown.
            default:
                //lineRenderType = LineDrawing.RenderTypes.Drawing;
                foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
                {
                    lineDrawingController.drawingType = GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Basic;
                }
                Debug.LogWarning("[ModeNavigator->HandleDrawingTypeChange] Unknown State");
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
                units = LineDrawingManager.Units.meters;
                //drawLineManager.desiredUnits = LineDrawing.unit.meters;
                break;

            // Centimeters.
            case 1:
                units = LineDrawingManager.Units.centimeters;
                //drawLineManager.desiredUnits = LineDrawing.unit.centimeters;
                break;

            // Millimeters.
            case 2:
                units = LineDrawingManager.Units.millimeters;
                //drawLineManager.desiredUnits = LineDrawing.unit.millimeters;
                break;

            // Yards.
            case 3:
                units = LineDrawingManager.Units.yards;
                //drawLineManager.desiredUnits = LineDrawing.unit.yards;
                break;

            // Feet.
            case 4:
                units = LineDrawingManager.Units.feet;
                //drawLineManager.desiredUnits = LineDrawing.unit.feet;
                break;

            // Inches:
            case 5:
                units = LineDrawingManager.Units.inches;
                //drawLineManager.desiredUnits = LineDrawing.unit.inches;
                break;

            // Unknown.
            default:
                units = LineDrawingManager.Units.meters;
                //drawLineManager.desiredUnits = LineDrawing.unit.meters;
                Debug.LogWarning("[ModeNavigator->HandleDrawingUnitsChange] Unknown State");
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
        foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
        {
            lineDrawingController.drawingWidth = widthInMeters;
        }
        /*switch (drawLineManager.desiredUnits)
        {
            case LineDrawing.unit.meters:
                // Leave at current value.
                break;

            case LineDrawing.unit.centimeters:
                convertedWidth = convertedWidth * 0.01f;
                break;

            case LineDrawing.unit.millimeters:
                convertedWidth = convertedWidth * 0.001f;
                break;

            case LineDrawing.unit.yards:
                convertedWidth = convertedWidth * 0.9144f;
                break;

            case LineDrawing.unit.feet:
                convertedWidth = convertedWidth * 0.3048f;
                break;

            case LineDrawing.unit.inches:
                convertedWidth = convertedWidth * 0.0254f;
                break;

            default:
                break;
        }
        drawLineManager.lineWidth = convertedWidth;*/
        RestartDrawingMode();
    }

    public void SetLineCutoff(VR_InputField cutoffToSet)
    {
        // TODO: Will need to limit this to a certain range.
        foreach (LineDrawingController lineDrawingController in MRET.LineDrawingManager.lineDrawingControllers)
        {
            lineDrawingController.drawingCutoff = cutoffInMeters;
        }
        if (cutoffToSet.text != "")
        {
            float convertedCutoff = float.Parse(cutoffToSet.text);
            //drawLineManager.cableCutoff = convertedCutoff;
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

    private string AbbreviateUnit(LineDrawing.unit unit)
    {
        switch (unit)
        {
            case LineDrawing.unit.meters:
                return "m";

            case LineDrawing.unit.centimeters:
                return "cm";

            case LineDrawing.unit.millimeters:
                return "mm";

            case LineDrawing.unit.yards:
                return "yd";

            case LineDrawing.unit.feet:
                return "ft";

            case LineDrawing.unit.inches:
                return "in";

            default:
                return "unk";
        }
    }
#endregion
}