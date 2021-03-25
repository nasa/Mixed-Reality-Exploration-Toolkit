// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Infrastructure.Framework;
using GSFC.ARVR.MRET.Components.UI;

public class LineDrawingMenuController : MonoBehaviour
{
    public VR_InputField widthText, cutoffText;
    public Dropdown drawingModeDropdown, drawingTypeDropdown, drawingUnitsDropdown;

    private DrawLineManager drawLineManager;
    private DrawLineManager.CaptureTypes lineCaptureType = DrawLineManager.CaptureTypes.None;
    private LineDrawing.RenderTypes lineRenderType = LineDrawing.RenderTypes.Drawing;

    public void Start()
    {
        drawLineManager = FindObjectOfType<DrawLineManager>();
        lineCaptureType = drawLineManager.captureType;
        lineRenderType = drawLineManager.renderType;

        // Initialize drawing mode.
        switch (drawLineManager.captureType)
        {
            case DrawLineManager.CaptureTypes.None:
                drawingModeDropdown.value = 0;
                break;

            case DrawLineManager.CaptureTypes.Free:
                drawingModeDropdown.value = 1;
                break;

            case DrawLineManager.CaptureTypes.Lines:
                drawingModeDropdown.value = 2;
                break;

            case DrawLineManager.CaptureTypes.Laser:
                drawingModeDropdown.value = 3;
                break;

            default:
                drawingModeDropdown.value = 0;
                break;
        }

        // Initialize drawing style.
        switch (drawLineManager.renderType)
        {
            case LineDrawing.RenderTypes.Drawing:
                drawingTypeDropdown.value = 0;
                break;

            case LineDrawing.RenderTypes.Cable:
                drawingTypeDropdown.value = 1;
                break;

            case LineDrawing.RenderTypes.Measurement:
                drawingTypeDropdown.value = 2;
                break;

            default:
                drawingTypeDropdown.value = 0;
                break;
        }

        // Initialize drawing units.
        switch (drawLineManager.desiredUnits)
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
        }

        // Initialize drawing width.
        widthText.text = drawLineManager.lineWidth.ToString();

        // Initialize drawing cutoff.
        cutoffText.text = drawLineManager.cableCutoff == 0 ? "" : drawLineManager.cableCutoff.ToString();

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
                lineCaptureType = DrawLineManager.CaptureTypes.None;
                drawLineManager.ExitDrawings();
                MRET.ControlMode.DisableAllControlTypes();
                break;

            // Freeform.
            case 1:
                lineCaptureType = DrawLineManager.CaptureTypes.Free;
                MRET.ControlMode.EnterDrawingMode();
                break;

            // Straight.
            case 2:
                lineCaptureType = DrawLineManager.CaptureTypes.Lines;
                MRET.ControlMode.EnterDrawingMode();
                break;

            // Laser.
            case 3:
                lineCaptureType = DrawLineManager.CaptureTypes.Laser;
                MRET.ControlMode.EnterDrawingMode();
                break;

            // Spline.
            case 4:
                lineCaptureType = DrawLineManager.CaptureTypes.None;
                drawLineManager.ExitDrawings();
                MRET.ControlMode.DisableAllControlTypes();
                Debug.LogWarning("[ModeNavigator->HandleDrawingModeChange] This option is currently unavailable.");
                break;

            // Unknown.
            default:
                lineCaptureType = DrawLineManager.CaptureTypes.None;
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
                lineRenderType = LineDrawing.RenderTypes.Drawing;
                break;

            // Cable.
            case 1:
                lineRenderType = LineDrawing.RenderTypes.Cable;
                break;

            // Measurement.
            case 2:
                lineRenderType = LineDrawing.RenderTypes.Measurement;
                break;

            // Unknown.
            default:
                lineRenderType = LineDrawing.RenderTypes.Drawing;
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
                drawLineManager.desiredUnits = LineDrawing.unit.meters;
                break;

            // Centimeters.
            case 1:
                drawLineManager.desiredUnits = LineDrawing.unit.centimeters;
                break;

            // Millimeters.
            case 2:
                drawLineManager.desiredUnits = LineDrawing.unit.millimeters;
                break;

            // Yards.
            case 3:
                drawLineManager.desiredUnits = LineDrawing.unit.yards;
                break;

            // Feet.
            case 4:
                drawLineManager.desiredUnits = LineDrawing.unit.feet;
                break;

            // Inches:
            case 5:
                drawLineManager.desiredUnits = LineDrawing.unit.inches;
                break;

            // Unknown.
            default:
                drawLineManager.desiredUnits = LineDrawing.unit.meters;
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
        switch (drawLineManager.desiredUnits)
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
        drawLineManager.lineWidth = convertedWidth;
        RestartDrawingMode();
    }

    public void SetLineCutoff(VR_InputField cutoffToSet)
    {
        // TODO: Will need to limit this to a certain range.
        if (cutoffToSet.text != "")
        {
            float convertedCutoff = float.Parse(cutoffToSet.text);
            drawLineManager.cableCutoff = convertedCutoff;
            RestartDrawingMode();
        }
    }

#region Helpers
    private void RestartDrawingMode()
    {
        if (lineCaptureType == DrawLineManager.CaptureTypes.Free)
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
        }
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