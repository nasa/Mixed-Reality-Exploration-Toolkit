// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing.Legacy;
using GOV.NASA.GSFC.XR.MRET.Extensions.PointCloud;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy
{
    public class DrawLineManager : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DrawLineManager);

        public const string ISDRAWINGFLAGKEY = "MRET.INTERNAL.DRAWING.ACTIVE";

        // Public Enumerations.
        public enum CaptureTypes { None, Free, Lines, Laser };

        // Public Declarations.
        public GameObject cablePrefab, cornerPrefab, panelPrefab;
        public GameObject drawingContainer;
        public List<LaserDrawingRaycastManager> controllerRaycast;
        public Material cableMat, drawingMat, measurementMat, highlightMat;
        public CaptureTypes captureType = CaptureTypes.None;
        public DrawingRender3dType renderType = DrawingRender3dType.Basic;
        public LengthUnitType desiredUnits = LengthUnitType.Meter;
        public float cableCutoff = 0, lineWidth = 0.005f;
        public Material laserMaterial;
        public Material highlightMaterial;

        // Private Classes.
        private class DrawingController
        {
            public InputHand controller;
            public LineDrawing currentDrawing;
            public int previewLineTimer = 0, touchpadHoldTimer = 0, waitAfterSnap = 0;
            public Vector3 snappedPos;
            public bool touchpadHeld = false, initializedLine = false, newLine = true, snappingLine = false, justSnappedLine = false;
        }

        // Private Declarations.
        private Material matToUse;
        private DrawingController[] drawingControllers = new DrawingController[2];
        private long lineID = 0;
        private GameObject leftLaserObject, rightLaserObject;
        private LineRenderer leftLaser, rightLaser;
        private Vector3 mousePosition;

        // Exit line drawing without setting the global control mode.
        public void ExitMode()
        {
            TurnOffLasers();
            captureType = CaptureTypes.None;
            ExitDrawings();
        }

        #region MRETBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            if (state == IntegrityState.Success)
            {
                if (cablePrefab == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Cable Prefab not assigned.");
                    state = IntegrityState.Failure;
                }
                else if (cornerPrefab == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Corner Prefab not assigned.");
                    state = IntegrityState.Failure;
                }
                else if (panelPrefab == null)
                {
                    Debug.LogError("[" + ClassName + "->IntegrityCheck] Panel Prefab not assigned.");
                    state = IntegrityState.Failure;
                }
            }
            return state;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Make sure we have a valid drawing container
            if (drawingContainer == null)
            {
                drawingContainer = ProjectManager.DrawingsContainer;
            }

            foreach (InputHand hand in MRET.InputRig.hands)
            {
                if (hand.handedness == InputHand.Handedness.left || hand.handedness == InputHand.Handedness.neutral)
                {
                    drawingControllers[0] = new DrawingController();
                    drawingControllers[0].controller = hand;
                }
                else if (hand.handedness == InputHand.Handedness.right)
                {
                    drawingControllers[1] = new DrawingController();
                    drawingControllers[1].controller = hand;
                }
            }

            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, false);
        }
        #endregion MRETBehaviour

        public void Initialize()
        {

        }

        protected LineDrawing CreateDrawing(DrawingRender3dType rType, Material lineMat, bool preview)
        {
            GameObject drawingGO = new GameObject("Drawing");
            drawingGO.transform.parent = drawingContainer.transform;
            LineDrawing drw = drawingGO.AddComponent<LineDrawing>();
            drw.Initialize(rType, lineMat, highlightMat,
                cablePrefab, cornerPrefab, lineWidth, drawingContainer, preview);

            return drw;
        }

        protected LineDrawing CreateDrawing(Drawing3dType serializedDrawing, Material lineMat, bool preview)
        {
            GameObject drawingGO = new GameObject("Drawing");
            drawingGO.transform.parent = drawingContainer.transform;
            LineDrawing drw = drawingGO.AddComponent<LineDrawing>();
            drw.Initialize(serializedDrawing, lineMat, highlightMat,
                cablePrefab, cornerPrefab, lineWidth, drawingContainer, preview);

            return drw;
        }

        // This method adds a new line drawing with all of its points in one step.
        public void AddPredefinedDrawing(Drawing3dType serializedDrawing)
        {
            // Get the material and instantiate the line.
            Material lineMat = drawingMat;
            switch (serializedDrawing.Type)
            {
                case DrawingRender3dType.Volumetric:
                    lineMat = cableMat;
                    break;

                case DrawingRender3dType.Basic:
                    lineMat = drawingMat;
                    break;

                default:
                    lineMat = drawingMat;
                    break;
            }

            // Create the drawing
            LineDrawing drw = CreateDrawing(serializedDrawing, lineMat, false);

            // Filter out small drawings.
            if (drw.GetDistance(LengthUnitType.Meter) < 0.001f)
            {
                Destroy(drw.meshModel);
            }
        }

        // This method adds multiple new line drawings with all of their points in one step.
        public void AddPredefinedDrawings(Drawing3dType[] serializedDrawings)
        {
            // Add all the drawings
            foreach (Drawing3dType drawing in serializedDrawings)
            {
                AddPredefinedDrawing(drawing);
            }
        }

        public void StartFreeformDrawings()
        {
            TurnOffLasers();
            matToUse = drawingMat;
            renderType = DrawingRender3dType.Basic;
            captureType = CaptureTypes.Free;
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartStraightDrawings()
        {
            TurnOffLasers();
            matToUse = drawingMat;
            renderType = DrawingRender3dType.Basic;
            captureType = CaptureTypes.Lines;

            for (int i = 0; i < 2; i++)
            {
                if (drawingControllers[i] != null)
                {
                    drawingControllers[i].currentDrawing = CreateDrawing(renderType, matToUse, true);
                    drawingControllers[i].currentDrawing.DesiredUnits = desiredUnits;
                }
            }
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartLaserDrawings()
        {
            TurnOnLasers();
            matToUse = drawingMat;
            renderType = DrawingRender3dType.Basic;
            captureType = CaptureTypes.Laser;

            for (int i = 0; i < 2; i++)
            {
                if (drawingControllers[i] != null)
                {
                    drawingControllers[i].currentDrawing = CreateDrawing(renderType, matToUse, true);
                    drawingControllers[i].currentDrawing.DesiredUnits = desiredUnits;
                }
            }
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartFreeformCables()
        {
            TurnOffLasers();
            matToUse = cableMat;
            renderType = DrawingRender3dType.Volumetric;
            captureType = CaptureTypes.Free;
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartStraightCables()
        {
            TurnOffLasers();
            matToUse = cableMat;
            renderType = DrawingRender3dType.Volumetric;
            captureType = CaptureTypes.Lines;

            for (int i = 0; i < 2; i++)
            {
                if (drawingControllers[i] != null)
                {
                    drawingControllers[i].currentDrawing = CreateDrawing(renderType, matToUse, true);
                    drawingControllers[i].currentDrawing.DesiredUnits = desiredUnits;
                }
            }
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartLaserCables()
        {
            TurnOnLasers();
            matToUse = cableMat;
            renderType = DrawingRender3dType.Volumetric;
            captureType = CaptureTypes.Laser;

            for (int i = 0; i < 2; i++)
            {
                if (drawingControllers[i] != null)
                {
                    drawingControllers[i].currentDrawing = CreateDrawing(renderType, matToUse, true);
                    drawingControllers[i].currentDrawing.DesiredUnits = desiredUnits;
                }
            }
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartFreeformMeasurements()
        {
            TurnOffLasers();
            matToUse = measurementMat;
            renderType = DrawingRender3dType.Basic;// Measurement;
            captureType = CaptureTypes.Free;
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartStraightMeasurements()
        {
            TurnOffLasers();
            matToUse = measurementMat;
            renderType = DrawingRender3dType.Basic;// Measurement;
            captureType = CaptureTypes.Lines;

            for (int i = 0; i < 2; i++)
            {
                if (drawingControllers[i] != null)
                {
                    drawingControllers[i].currentDrawing = CreateDrawing(renderType, matToUse, true);
                    drawingControllers[i].currentDrawing.DesiredUnits = desiredUnits;
                }
            }
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void StartLaserMeasurements()
        {
            TurnOnLasers();
            matToUse = measurementMat;
            renderType = DrawingRender3dType.Basic;// Measurement;
            captureType = CaptureTypes.Laser;

            for (int i = 0; i < 2; i++)
            {
                if (drawingControllers[i] != null)
                {
                    drawingControllers[i].currentDrawing = CreateDrawing(renderType, matToUse, true);
                    drawingControllers[i].currentDrawing.DesiredUnits = desiredUnits;
                }
            }
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, true);
        }

        public void ExitDrawings()
        {
            TurnOffLasers();
            captureType = CaptureTypes.None;
            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(ISDRAWINGFLAGKEY, false);
        }

        public void LTouchpadPressed()
        {
            CaptureTouchpadPressEvent(0);
        }

        public void RTouchpadPressed()
        {
            CaptureTouchpadPressEvent(1);
        }

        public void LTouchpadReleased()
        {
            CaptureTouchpadReleaseEvent(0);
        }

        public void RTouchpadReleased()
        {
            CaptureTouchpadReleaseEvent(1);
        }

        public void LGripPressed()
        {
            CaptureGripPressEvent(0);
        }

        public void RGripPressed()
        {
            CaptureGripPressEvent(1);
        }

        private void SetLaserPosition(int index, Vector3 posToSet)
        {
            if (index == 0 && leftLaser && drawingControllers[0] != null)
            {
                leftLaser.SetPosition(0, drawingControllers[0].controller.transform.position);
                leftLaser.SetPosition(1, posToSet);
            }
            else if (index == 1 && rightLaser && drawingControllers[1] != null)
            {
                rightLaser.SetPosition(0, drawingControllers[1].controller.transform.position);
                rightLaser.SetPosition(1, posToSet);
            }
        }

        private void TurnOnLasers()
        {
            TurnOffLasers();

            if (drawingControllers[0] != null)
            {
                leftLaserObject = new GameObject("DrawingLaser");
                leftLaserObject.transform.SetParent(drawingControllers[0].controller.transform);
                leftLaser = leftLaserObject.AddComponent<LineRenderer>();
                leftLaser.widthMultiplier = 0.0025f;
                leftLaser.material = laserMaterial;
                leftLaser.useWorldSpace = true;
                leftLaser.positionCount = 2;
                leftLaser.SetPosition(0, drawingControllers[0].controller.transform.position);
                leftLaser.SetPosition(1, drawingControllers[0].controller.transform.position);
            }

            if (drawingControllers[1] != null)
            {
                rightLaserObject = new GameObject("DrawingLaser");
                rightLaserObject.transform.SetParent(drawingControllers[1].controller.transform);
                rightLaser = rightLaserObject.AddComponent<LineRenderer>();
                rightLaser.widthMultiplier = 0.0025f;
                rightLaser.material = laserMaterial;
                rightLaser.useWorldSpace = true;
                rightLaser.positionCount = 2;
                rightLaser.SetPosition(0, drawingControllers[1].controller.transform.position);
                rightLaser.SetPosition(1, drawingControllers[1].controller.transform.position);
            }
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
                    drawingControllers[ctrlIndex].currentDrawing = CreateDrawing(renderType, matToUse, false);
                    drawingControllers[ctrlIndex].currentDrawing.DesiredUnits = desiredUnits;
                    drawingControllers[ctrlIndex].currentDrawing.AddPoint(drawingControllers[ctrlIndex].controller.transform.position);
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

                        drawingControllers[ctrlIndex].currentDrawing.AddPoint(drawingControllers[ctrlIndex].controller.transform.position);
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
                    if (drawingControllers[ctrlIndex].currentDrawing.meshModel != null)
                    {
                        drawingControllers[ctrlIndex].currentDrawing.meshModel.name = "LineDrawing" + ++lineID;
                        MeshCollider coll = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<MeshCollider>();
                        coll.convex = true;
                        coll.isTrigger = true;
                        InteractableDrawingPanel drw = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<InteractableDrawingPanel>();
                        drw.Grabbable = false;
                        drw.Usable = true;
                        drw.HighlightMaterial = highlightMaterial;
                        if (renderType == DrawingRender3dType.Volumetric)
                        {
                            drawingControllers[ctrlIndex].currentDrawing.meshModel.GetComponent<MeshRenderer>().enabled = false;
                        }

                        // Serialize out the current drawing
                        var serializedDrawing = drawingControllers[ctrlIndex].currentDrawing.CreateSerializedType();
                        drawingControllers[ctrlIndex].currentDrawing.Serialize(serializedDrawing);

                        // Create the add action
                        ProjectManager.UndoManager.AddAction(
                            new AddSceneObjectAction(serializedDrawing),
                            new DeleteIdentifiableObjectAction(serializedDrawing.ID));
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
                        if (drawingControllers[ctrlIndex].currentDrawing.meshModel != null)
                        {
                            drawingControllers[ctrlIndex].currentDrawing.meshModel.name = "LineDrawing" + ++lineID;
                            MeshCollider coll = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<MeshCollider>();
                            coll.convex = true;
                            coll.isTrigger = true;
                            InteractableDrawingPanel drw = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<InteractableDrawingPanel>();
                            drw.Grabbable = false;
                            drw.Usable = true;
                            drw.HighlightMaterial = highlightMaterial;
                            if (renderType == DrawingRender3dType.Volumetric)
                            {
                                drawingControllers[ctrlIndex].currentDrawing.meshModel.GetComponent<MeshRenderer>().enabled = false;
                            }

                            // Serialize out the current drawing
                            var serializedDrawing = drawingControllers[ctrlIndex].currentDrawing.CreateSerializedType();
                            drawingControllers[ctrlIndex].currentDrawing.Serialize(serializedDrawing);

                            // Create the add action
                            ProjectManager.UndoManager.AddAction(
                                new AddSceneObjectAction(serializedDrawing),
                                new DeleteIdentifiableObjectAction(serializedDrawing.ID));
                        }
                        drawingControllers[ctrlIndex].currentDrawing.DestroyPreviewLine();
                        if (drawingControllers[ctrlIndex].currentDrawing.GetNumPoints() > 0)
                        {   // If current one isn't empty.
                            drawingControllers[ctrlIndex].currentDrawing = CreateDrawing(renderType, matToUse, true);
                            drawingControllers[ctrlIndex].currentDrawing.DesiredUnits = desiredUnits;
                        }
                    }
                    drawingControllers[ctrlIndex].newLine = false;
                    break;

                case CaptureTypes.Laser:
                    if (drawingControllers[ctrlIndex].newLine)
                    {
                        if (drawingControllers[ctrlIndex].currentDrawing.meshModel != null)
                        {
                            drawingControllers[ctrlIndex].currentDrawing.meshModel.name = "LineDrawing" + ++lineID;
                            MeshCollider coll = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<MeshCollider>();
                            coll.convex = true;
                            coll.isTrigger = true;

                            InteractableDrawingPanel drw = drawingControllers[ctrlIndex].currentDrawing.meshModel.AddComponent<InteractableDrawingPanel>();
                            drw.Grabbable = false;
                            drw.Usable = true;
                            drw.HighlightMaterial = highlightMaterial;
                            if (renderType == DrawingRender3dType.Volumetric)
                            {
                                drawingControllers[ctrlIndex].currentDrawing.meshModel.GetComponent<MeshRenderer>().enabled = false;
                            }

                            // Serialize out the current drawing
                            var serializedDrawing = drawingControllers[ctrlIndex].currentDrawing.CreateSerializedType();
                            drawingControllers[ctrlIndex].currentDrawing.Serialize(serializedDrawing);

                            // Create the add action
                            ProjectManager.UndoManager.AddAction(
                                new AddSceneObjectAction(serializedDrawing),
                                new DeleteIdentifiableObjectAction(serializedDrawing.ID));
                        }
                        if (drawingControllers[ctrlIndex].currentDrawing.GetNumPoints() > 0)
                        {   // If current one isn't empty.
                            drawingControllers[ctrlIndex].currentDrawing.DestroyPreviewLine();
                            drawingControllers[ctrlIndex].currentDrawing = CreateDrawing(renderType, matToUse, true);
                            drawingControllers[ctrlIndex].currentDrawing.DesiredUnits = desiredUnits;
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
            if (drawingControllers[1] != null)
            {
                HandleControllerState(1);
            }

            if (controllerRaycast[0] && captureType == CaptureTypes.Laser && drawingControllers[0] != null)
            {
                if (controllerRaycast[0].intersectionStatus)
                {
                    SetLaserPosition(0, controllerRaycast[0].raycastPoint);
                }
                else
                {
                    SetLaserPosition(0, drawingControllers[0].controller.transform.position);
                }
            }

            if (drawingControllers[1] != null)
            {
                if (controllerRaycast[1] && captureType == CaptureTypes.Laser && drawingControllers[1] != null)
                {
                    if (controllerRaycast[1].intersectionStatus)
                    {
                        SetLaserPosition(1, controllerRaycast[1].raycastPoint);
                    }
                    else
                    {
                        SetLaserPosition(1, drawingControllers[1].controller.transform.position);
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
                            if (Vector3.Magnitude(drawingControllers[ctrlIndex].controller.transform.position - drawingControllers[ctrlIndex].currentDrawing.GetLastPoint()) >= 0.001f)
                            {
                                drawingControllers[ctrlIndex].currentDrawing.AddPoint(drawingControllers[ctrlIndex].controller.transform.position);
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

                                drawingControllers[ctrlIndex].snappedPos = drawingControllers[ctrlIndex].currentDrawing.SetPreviewLine(drawingControllers[ctrlIndex].controller.transform.position, true);
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
                                drawingControllers[ctrlIndex].currentDrawing.SetPreviewLine(drawingControllers[ctrlIndex].controller.transform.position, false);
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
}