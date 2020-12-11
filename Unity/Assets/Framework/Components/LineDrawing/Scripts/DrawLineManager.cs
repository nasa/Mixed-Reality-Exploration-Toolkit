using System;
using UnityEngine;
using System.Collections.Generic;

public class DrawLineManager : MonoBehaviour
{
    // Public Enumerations.
    public enum CaptureTypes { None, Free, Lines, Laser };

    // Public Declarations.
    public GameObject cablePrefab, cornerPrefab, panelPrefab;
    public GameObject drawingContainer;
    public GameObject leftController, rightController;
    public List<LaserDrawingRaycastManager> controllerRaycast;
    public Material cableMat, drawingMat, measurementMat, highlightMat;
    public CaptureTypes captureType = CaptureTypes.None;
    public LineDrawing.RenderTypes renderType = LineDrawing.RenderTypes.Drawing;
    public LineDrawing.unit desiredUnits = LineDrawing.unit.meters;
    public float cableCutoff = 0, lineWidth = 0.005f;
    public Material laserMaterial;
    public Color highlightColor;

    // Private Classes.
    private class DrawingController
    {
        public GameObject controller;
        public LineDrawing currentDrawing;
        public int previewLineTimer = 0, touchpadHoldTimer = 0, waitAfterSnap = 0;
        public Vector3 snappedPos;
        public bool touchpadHeld = false, initializedLine = false, newLine = true, snappingLine = false, justSnappedLine = false;
        public VRTK.VRTK_ControllerEvents cEvents;
    }

    // Private Declarations.
    private Material matToUse;
    private DrawingController[] drawingControllers = new DrawingController[2];
    private long lineID = 0;
    private GameObject leftLaserObject, rightLaserObject;
    private LineRenderer leftLaser, rightLaser;
    private UndoManager undoManager;
    private Vector3 mousePosition;

    // Exit line drawing without setting the global control mode.
    public void ExitMode()
    {
        TurnOffLasers();
        captureType = CaptureTypes.None;
        ExitDrawings();
    }

    public void Start()
    {
        if (!VRDesktopSwitcher.isDesktopEnabled())
        {
            // Set up left drawing controller.
            drawingControllers[0] = new DrawingController();
            drawingControllers[0].controller = leftController;
            drawingControllers[0].cEvents = drawingControllers[0].controller.GetComponent<VRTK.VRTK_ControllerEvents>();
            drawingControllers[0].cEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(LTouchpadPressed);
            drawingControllers[0].cEvents.TouchpadReleased += new VRTK.ControllerInteractionEventHandler(LTouchpadReleased);
            drawingControllers[0].cEvents.GripPressed += new VRTK.ControllerInteractionEventHandler(LGripPressed);

            // Set up right drawing controller;
            drawingControllers[1] = new DrawingController();
            drawingControllers[1].controller = rightController;
            drawingControllers[1].cEvents = drawingControllers[1].controller.GetComponent<VRTK.VRTK_ControllerEvents>();
            drawingControllers[1].cEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(RTouchpadPressed);
            drawingControllers[1].cEvents.TouchpadReleased += new VRTK.ControllerInteractionEventHandler(RTouchpadReleased);
            drawingControllers[1].cEvents.GripPressed += new VRTK.ControllerInteractionEventHandler(RGripPressed);
        }
        else
        {
            drawingControllers[0] = new DrawingController();
            drawingControllers[0].controller = leftController;
            EventManager.OnLeftClick += LTouchpadPressed;
            EventManager.OnLeftClickUp += LTouchpadReleased;
        }

        undoManager = FindObjectOfType<UndoManager>();
    }

    // This method adds a new line drawing with all of its points in one step.
    public void AddPredefinedDrawing(List<Vector3> points, LineDrawing.RenderTypes rType, LineDrawing.unit desUnits, string name, Guid guidToSet)
    {
        // Get the material and instantiate the line.
        Material lineMat = drawingMat;
        switch (rType)
        {
            case LineDrawing.RenderTypes.Cable:
                lineMat = cableMat;
                break;

            case LineDrawing.RenderTypes.Drawing:
                lineMat = drawingMat;
                break;

            case LineDrawing.RenderTypes.Measurement:
                lineMat = measurementMat;
                break;

            default:
                lineMat = drawingMat;
                break;
        }
        LineDrawing drw = new LineDrawing(rType, lineMat, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, false);

        // Set the name.
        drw.meshModel.name = name;

        // Set the display units (for measurements).
        drw.desiredUnits = desUnits;

        // Add all points to the line.
        foreach (Vector3 point in points)
        {
            drw.AddPoint(point);
        }
        
        // Filter out small drawings.
        if (drw.GetDistance(LineDrawing.unit.meters) < 0.001f)
        {
            Destroy(drw.measurementText);
            Destroy(drw.meshModel);
        }
        else
        {
            drw.guid = guidToSet;
            drw.meshModel.name = name;
            MeshCollider coll = drw.meshModel.AddComponent<MeshCollider>();
            coll.convex = true;
            coll.isTrigger = true;
            //VRTK.VRTK_InteractableObject iObj = drw.meshModel.AddComponent<VRTK.VRTK_InteractableObject>();
            //iObj.disableWhenIdle = false;
            //iObj.isGrabbable = false;
            //iObj.isUsable = true;
            //iObj.touchHighlightColor = highlightColor;
            //iObj.enabled = true;
            if (rType == LineDrawing.RenderTypes.Cable)
            {
                drw.meshModel.GetComponent<MeshRenderer>().enabled = false;
            }

        }
    }

    public void StartFreeformDrawings()
    {
        TurnOffLasers();
        matToUse = drawingMat;
        renderType = LineDrawing.RenderTypes.Drawing;
        captureType = CaptureTypes.Free;
    }

    public void StartStraightDrawings()
    {
        TurnOffLasers();
        matToUse = drawingMat;
        renderType = LineDrawing.RenderTypes.Drawing;
        captureType = CaptureTypes.Lines;
        
        for (int i = 0; i < (VRDesktopSwitcher.isDesktopEnabled() ? 1 : 2); i++)
        {
            drawingControllers[i].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
            drawingControllers[i].currentDrawing.desiredUnits = desiredUnits;
        }
    }

    public void StartLaserDrawings()
    {
        TurnOnLasers();
        matToUse = drawingMat;
        renderType = LineDrawing.RenderTypes.Drawing;
        captureType = CaptureTypes.Laser;

        for (int i = 0; i < (VRDesktopSwitcher.isDesktopEnabled() ? 1 : 2); i++)
        {
            drawingControllers[i].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
            drawingControllers[i].currentDrawing.desiredUnits = desiredUnits;
        }
    }

    public void StartFreeformCables()
    {
        TurnOffLasers();
        matToUse = cableMat;
        renderType = LineDrawing.RenderTypes.Cable;
        captureType = CaptureTypes.Free;
    }

    public void StartStraightCables()
    {
        TurnOffLasers();
        matToUse = cableMat;
        renderType = LineDrawing.RenderTypes.Cable;
        captureType = CaptureTypes.Lines;

        for (int i = 0; i < (VRDesktopSwitcher.isDesktopEnabled() ? 1 : 2); i++)
        {
            drawingControllers[i].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
            drawingControllers[i].currentDrawing.desiredUnits = desiredUnits;
        }
    }

    public void StartLaserCables()
    {
        TurnOnLasers();
        matToUse = cableMat;
        renderType = LineDrawing.RenderTypes.Cable;
        captureType = CaptureTypes.Laser;

        for (int i = 0; i < (VRDesktopSwitcher.isDesktopEnabled() ? 1 : 2); i++)
        {
            drawingControllers[i].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
            drawingControllers[i].currentDrawing.desiredUnits = desiredUnits;
        }
    }

    public void StartFreeformMeasurements()
    {
        TurnOffLasers();
        matToUse = measurementMat;
        renderType = LineDrawing.RenderTypes.Measurement;
        captureType = CaptureTypes.Free;
    }

    public void StartStraightMeasurements()
    {
        TurnOffLasers();
        matToUse = measurementMat;
        renderType = LineDrawing.RenderTypes.Measurement;
        captureType = CaptureTypes.Lines;

        for (int i = 0; i < (VRDesktopSwitcher.isDesktopEnabled() ? 1 : 2); i++)
        {
            drawingControllers[i].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
            drawingControllers[i].currentDrawing.desiredUnits = desiredUnits;
        }
    }

    public void StartLaserMeasurements()
    {
        TurnOnLasers();
        matToUse = measurementMat;
        renderType = LineDrawing.RenderTypes.Measurement;
        captureType = CaptureTypes.Laser;

        for (int i = 0; i < (VRDesktopSwitcher.isDesktopEnabled() ? 1 : 2); i++)
        {
            drawingControllers[i].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
            drawingControllers[i].currentDrawing.desiredUnits = desiredUnits;
        }
    }

    public void ExitDrawings()
    {
        TurnOffLasers();
        captureType = CaptureTypes.None;
    }

    public void LTouchpadPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        CaptureTouchpadPressEvent(0);
    }

    public void LTouchpadPressed()
    {
        CaptureTouchpadPressEvent(0);
    }

    public void RTouchpadPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        CaptureTouchpadPressEvent(1);
    }

    public void LTouchpadReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        CaptureTouchpadReleaseEvent(0);
    }

    public void LTouchpadReleased()
    {
        CaptureTouchpadReleaseEvent(0);
    }

    public void RTouchpadReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        CaptureTouchpadReleaseEvent(1);
    }

    public void LGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        CaptureGripPressEvent(0);
    }

    public void RGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        CaptureGripPressEvent(1);
    }

    private void SetLaserPosition(int index, Vector3 posToSet)
    {
        if (index == 0 && leftLaser)
        {
            leftLaser.SetPosition(0, leftController.transform.position);
            leftLaser.SetPosition(1, posToSet);
        }
        else if (index == 1 && rightLaser)
        {
            rightLaser.SetPosition(0, rightController.transform.position);
            rightLaser.SetPosition(1, posToSet);
        }
    }

    private void TurnOnLasers()
    {
        TurnOffLasers();

        leftLaserObject = new GameObject("DrawingLaser");
        leftLaserObject.transform.SetParent(leftController.transform);
        leftLaser = leftLaserObject.AddComponent<LineRenderer>();
        leftLaser.widthMultiplier = 0.0025f;
        leftLaser.material = laserMaterial;
        leftLaser.useWorldSpace = true;
        leftLaser.positionCount = 2;
        leftLaser.SetPosition(0, leftController.transform.position);
        leftLaser.SetPosition(1, leftController.transform.position);

        rightLaserObject = new GameObject("DrawingLaser");
        rightLaserObject.transform.SetParent(rightController.transform);
        rightLaser = rightLaserObject.AddComponent<LineRenderer>();
        rightLaser.widthMultiplier = 0.0025f;
        rightLaser.material = laserMaterial;
        rightLaser.useWorldSpace = true;
        rightLaser.positionCount = 2;
        rightLaser.SetPosition(0, rightController.transform.position);
        rightLaser.SetPosition(1, rightController.transform.position);
    }

    private void TurnOffLasers()
    {
        if (leftLaser)
        {
            Destroy(leftLaserObject);
        }

        if (rightLaser)
        {
            Destroy(rightLaserObject);
        }
    }

    private void CaptureTouchpadPressEvent(int ctrlIndex)
    {
        drawingControllers[ctrlIndex].touchpadHeld = true;
        drawingControllers[ctrlIndex].touchpadHoldTimer = 0;

        switch (captureType)
        {
            case CaptureTypes.None:
                break;

            case CaptureTypes.Free:
                // Start new line if touchpad pressed down.
                drawingControllers[ctrlIndex].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, false);
                drawingControllers[ctrlIndex].currentDrawing.desiredUnits = desiredUnits;
                if (!VRDesktopSwitcher.isDesktopEnabled())
                {
                    drawingControllers[ctrlIndex].currentDrawing.AddPoint(drawingControllers[ctrlIndex].controller.transform.position);
                }
                else
                {
                    mousePosition = Input.mousePosition;
                    mousePosition.z = 3f;
                    drawingControllers[ctrlIndex].currentDrawing.AddPoint(Camera.main.ScreenToWorldPoint(mousePosition));
                }
                drawingControllers[ctrlIndex].initializedLine = true;
                break;

            case CaptureTypes.Lines:
                drawingControllers[ctrlIndex].newLine = true;
                if (!drawingControllers[ctrlIndex].justSnappedLine)
                {
                    if (cableCutoff != 0)
                    {
                        if (drawingControllers[ctrlIndex].currentDrawing.GetDistance(desiredUnits, drawingControllers[ctrlIndex].controller.transform.position) >= cableCutoff)
                        {
                            break;
                        }
                    }
                    if (!VRDesktopSwitcher.isDesktopEnabled())
                    {
                        drawingControllers[ctrlIndex].currentDrawing.AddPoint(drawingControllers[ctrlIndex].controller.transform.position);
                    }
                    else
                    {
                        mousePosition = Input.mousePosition;
                        mousePosition.z = 3f;
                        drawingControllers[ctrlIndex].currentDrawing.AddPoint(Camera.main.ScreenToWorldPoint(mousePosition));
                    }
                }
                break;

            case CaptureTypes.Laser:
                drawingControllers[ctrlIndex].newLine = true;
                if (controllerRaycast[ctrlIndex].intersectionStatus)
                {
                    if (cableCutoff != 0)
                    {
                        if (drawingControllers[ctrlIndex].currentDrawing.GetDistance(desiredUnits, controllerRaycast[ctrlIndex].raycastPoint) >= cableCutoff)
                        {
                            break;
                        }
                    }
                    drawingControllers[ctrlIndex].currentDrawing.AddPoint(controllerRaycast[ctrlIndex].raycastPoint);
                }
                break;

            default:
                break;
        }
    }

    private void CaptureTouchpadReleaseEvent(int ctrlIndex)
    {
        switch (captureType)
        {
            case CaptureTypes.None:
                break;

            case CaptureTypes.Free:
                if (drawingControllers[ctrlIndex].currentDrawing != null)
                {
                    drawingControllers[ctrlIndex].currentDrawing.meshModel.name = "LineDrawing" + ++lineID;
                    MeshCollider coll = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<MeshCollider>();
                    coll.convex = true;
                    coll.isTrigger = true;
                    //VRTK.VRTK_InteractableObject iObj = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<VRTK.VRTK_InteractableObject>();
                    //iObj.disableWhenIdle = false;
                    //iObj.isGrabbable = false;
                    //iObj.isUsable = true;
                    //iObj.touchHighlightColor = highlightColor;
                    //iObj.enabled = true;
                    if (renderType == LineDrawing.RenderTypes.Cable)
                    {
                        drawingControllers[ctrlIndex].currentDrawing.meshModel.GetComponent<MeshRenderer>().enabled = false;
                    }

                    undoManager.AddAction(ProjectAction.AddDrawingAction(drawingControllers[ctrlIndex].currentDrawing.Serialize()),
                        ProjectAction.DeleteDrawingAction(drawingControllers[ctrlIndex].currentDrawing.meshModel.name));
                }
                break;

            case CaptureTypes.Lines:
                break;

            case CaptureTypes.Laser:
                break;

            default:
                break;
        }
        drawingControllers[ctrlIndex].touchpadHeld = false;
    }

    private void CaptureGripPressEvent(int ctrlIndex)
    {
        switch (captureType)
        {
            case CaptureTypes.None:
                break;

            case CaptureTypes.Free:
                break;

            case CaptureTypes.Lines:
                if (drawingControllers[ctrlIndex].newLine)
                {
                    if (drawingControllers[ctrlIndex].currentDrawing != null)
                    {
                        drawingControllers[ctrlIndex].currentDrawing.meshModel.name = "LineDrawing" + ++lineID;
                        MeshCollider coll = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<MeshCollider>();
                        coll.convex = true;
                        coll.isTrigger = true;
                        //VRTK.VRTK_InteractableObject iObj = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<VRTK.VRTK_InteractableObject>();
                        //iObj.disableWhenIdle = false;
                        //iObj.isGrabbable = false;
                        //iObj.isUsable = true;
                        //iObj.touchHighlightColor = highlightColor;
                        //iObj.enabled = true;
                        if (renderType == LineDrawing.RenderTypes.Cable)
                        {
                            drawingControllers[ctrlIndex].currentDrawing.meshModel.GetComponent<MeshRenderer>().enabled = false;
                        }

                        undoManager.AddAction(ProjectAction.AddDrawingAction(drawingControllers[ctrlIndex].currentDrawing.Serialize()),
                            ProjectAction.DeleteDrawingAction(drawingControllers[ctrlIndex].currentDrawing.meshModel.name));
                    }
                    drawingControllers[ctrlIndex].currentDrawing.DestroyPreviewLine();
                    if (drawingControllers[ctrlIndex].currentDrawing.GetNumPoints() > 0)
                    {   // If current one isn't empty.
                        drawingControllers[ctrlIndex].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
                        drawingControllers[ctrlIndex].currentDrawing.desiredUnits = desiredUnits;
                    }
                }
                drawingControllers[ctrlIndex].newLine = false;
                break;

            case CaptureTypes.Laser:
                if (drawingControllers[ctrlIndex].newLine)
                {
                    if (drawingControllers[ctrlIndex].currentDrawing != null)
                    {
                        drawingControllers[ctrlIndex].currentDrawing.meshModel.name = "LineDrawing" + ++lineID;
                        MeshCollider coll = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<MeshCollider>();
                        coll.convex = true;
                        coll.isTrigger = true;
                        //VRTK.VRTK_InteractableObject iObj = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<VRTK.VRTK_InteractableObject>();
                        //iObj.disableWhenIdle = false;
                        //iObj.isGrabbable = false;
                        //iObj.isUsable = true;
                        //iObj.touchHighlightColor = highlightColor;
                        //iObj.enabled = true;
                        if (renderType == LineDrawing.RenderTypes.Cable)
                        {
                            drawingControllers[ctrlIndex].currentDrawing.meshModel.GetComponent<MeshRenderer>().enabled = false;
                        }

                        undoManager.AddAction(ProjectAction.AddDrawingAction(drawingControllers[ctrlIndex].currentDrawing.Serialize()),
                            ProjectAction.DeleteDrawingAction(drawingControllers[ctrlIndex].currentDrawing.meshModel.name));
                    }
                    if (drawingControllers[ctrlIndex].currentDrawing.GetNumPoints() > 0)
                    {   // If current one isn't empty.
                        drawingControllers[ctrlIndex].currentDrawing.DestroyPreviewLine();
                        drawingControllers[ctrlIndex].currentDrawing = new LineDrawing(renderType, matToUse, highlightMat, cablePrefab, cornerPrefab, lineWidth, drawingContainer, true);
                        drawingControllers[ctrlIndex].currentDrawing.desiredUnits = desiredUnits;
                    }
                }
                drawingControllers[ctrlIndex].newLine = false;
                break;

            default:
                break;
        }
    }

    private void Update()
    {
        HandleControllerState(0);

        if (VRDesktopSwitcher.isVREnabled())
        {
            HandleControllerState(1);
        }

        if (!VRDesktopSwitcher.isDesktopEnabled())
        {
            if (controllerRaycast[0] && captureType == CaptureTypes.Laser)
            {
                if (controllerRaycast[0].intersectionStatus)
                {
                    SetLaserPosition(0, controllerRaycast[0].raycastPoint);
                }
                else
                {
                    SetLaserPosition(0, leftController.transform.position);
                }
            }

            if (controllerRaycast[1] && captureType == CaptureTypes.Laser)
            {
                if (controllerRaycast[1].intersectionStatus)
                {
                    SetLaserPosition(1, controllerRaycast[1].raycastPoint);
                }
                else
                {
                    SetLaserPosition(1, rightController.transform.position);
                }
            }
        }
    }

    private void HandleControllerState(int ctrlIndex)
    {
        switch (captureType)
        {
            case CaptureTypes.None:
                break;

            case CaptureTypes.Free:
                // Continue line if touchpad continued to be held.
                if (drawingControllers[ctrlIndex].touchpadHeld)
                {
                    if (cableCutoff != 0)
                    {
                        if (drawingControllers[ctrlIndex].currentDrawing.GetDistance(desiredUnits) >= cableCutoff)
                        {
                            while (drawingControllers[ctrlIndex].currentDrawing.GetDistance(desiredUnits) > cableCutoff)
                            {
                                drawingControllers[ctrlIndex].currentDrawing.RemovePointByIndex(drawingControllers[ctrlIndex].currentDrawing.GetNumPoints() - 1);
                                drawingControllers[ctrlIndex].touchpadHeld = false;
                            }
                            break;
                        }
                    }

                    if (drawingControllers[ctrlIndex].initializedLine)
                    {
                        if (VRDesktopSwitcher.isDesktopEnabled())
                        {
                            mousePosition = Input.mousePosition;
                            mousePosition.z = 3f;
                            drawingControllers[ctrlIndex].currentDrawing.AddPoint(Camera.main.ScreenToWorldPoint(mousePosition));
                        }
                        else
                        {
                            if (Vector3.Magnitude(drawingControllers[ctrlIndex].controller.transform.position - drawingControllers[ctrlIndex].currentDrawing.GetLastPoint()) >= 0.001f)
                            {
                                drawingControllers[ctrlIndex].currentDrawing.AddPoint(drawingControllers[ctrlIndex].controller.transform.position);
                            }
                        }
                    }
                }
                break;

            case CaptureTypes.Lines:
                if (drawingControllers[ctrlIndex].touchpadHeld)
                {
                    if (drawingControllers[ctrlIndex].touchpadHoldTimer == 19)
                    {
                        drawingControllers[ctrlIndex].snappingLine = true;
                        drawingControllers[ctrlIndex].touchpadHoldTimer = 0;
                    }

                    if (drawingControllers[ctrlIndex].snappingLine)
                    {
                        if (drawingControllers[ctrlIndex].previewLineTimer == 3)
                        {   // Only do every 4 cycles to spare CPU/GPU.
                            drawingControllers[ctrlIndex].previewLineTimer = 0;

                            if (cableCutoff != 0)
                            {
                                if (drawingControllers[ctrlIndex].currentDrawing.GetDistance(desiredUnits, drawingControllers[ctrlIndex].controller.transform.position) >= cableCutoff)
                                {
                                    break;
                                }
                            }

                            if (!VRDesktopSwitcher.isDesktopEnabled())
                            {
                                drawingControllers[ctrlIndex].snappedPos = drawingControllers[ctrlIndex].currentDrawing.SetPreviewLine(drawingControllers[ctrlIndex].controller.transform.position, true);
                            }
                            else
                            {
                                mousePosition = Input.mousePosition;
                                mousePosition.z = 3f;
                                drawingControllers[ctrlIndex].snappedPos = drawingControllers[ctrlIndex].currentDrawing.SetPreviewLine(Camera.main.ScreenToWorldPoint(mousePosition), true);
                            }
                        }
                        drawingControllers[ctrlIndex].previewLineTimer++;
                    }
                    else
                    {
                        drawingControllers[ctrlIndex].touchpadHoldTimer++;
                    }
                }
                else
                {
                    if (drawingControllers[ctrlIndex].snappingLine)
                    {   // Not holding button, snapping => capturing snapped line.
                        drawingControllers[ctrlIndex].currentDrawing.AddPoint(drawingControllers[ctrlIndex].snappedPos);
                        drawingControllers[ctrlIndex].snappingLine = false;
                        drawingControllers[ctrlIndex].justSnappedLine = true;
                        drawingControllers[ctrlIndex].waitAfterSnap = 0;
                    }
                    else if (drawingControllers[ctrlIndex].justSnappedLine)
                    {
                        if (drawingControllers[ctrlIndex].waitAfterSnap++ == 50)
                        {
                            drawingControllers[ctrlIndex].justSnappedLine = false;
                            drawingControllers[ctrlIndex].waitAfterSnap = 0;
                        }
                    }
                    else
                    {
                        // Not holding button, not snapping => previewing raw line.
                        if (drawingControllers[ctrlIndex].previewLineTimer == 3)
                        {   // Only do every 4 cycles to spare CPU/GPU.
                            drawingControllers[ctrlIndex].previewLineTimer = 0;

                            if (cableCutoff != 0)
                            {
                                if (drawingControllers[ctrlIndex].currentDrawing.GetDistance(desiredUnits, drawingControllers[ctrlIndex].controller.transform.position) >= cableCutoff)
                                {
                                    break;
                                }
                            }
                            if (VRDesktopSwitcher.isVREnabled())
                            {
                                drawingControllers[ctrlIndex].currentDrawing.SetPreviewLine(drawingControllers[ctrlIndex].controller.transform.position, false);
                            }
                            else
                            {
                                Vector3 tempMousePosition = Input.mousePosition;
                                tempMousePosition.z = 3f;
                                drawingControllers[ctrlIndex].currentDrawing.SetPreviewLine(Camera.main.ScreenToWorldPoint(tempMousePosition), false);
                            }
                        }
                        drawingControllers[ctrlIndex].previewLineTimer++;
                    }
                }
                break;

            case CaptureTypes.Laser:
                if (drawingControllers[ctrlIndex].previewLineTimer == 3)
                {   // Only do every 4 cycles to spare CPU/GPU.
                    drawingControllers[ctrlIndex].previewLineTimer = 0;

                    if (cableCutoff != 0)
                    {
                        if (drawingControllers[ctrlIndex].currentDrawing.GetDistance(desiredUnits, controllerRaycast[ctrlIndex].raycastPoint) >= cableCutoff)
                        {
                            break;
                        }
                    }
                    drawingControllers[ctrlIndex].currentDrawing.SetPreviewLine(controllerRaycast[ctrlIndex].raycastPoint, false);
                }
                drawingControllers[ctrlIndex].previewLineTimer++;
                break;

            default:
                break;
        }
    }
}