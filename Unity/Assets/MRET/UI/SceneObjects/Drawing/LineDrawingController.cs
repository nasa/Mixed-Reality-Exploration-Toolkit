// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
#if HOLOLENS_BUILD
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Hololens;
#endif
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// 17 May 2022: Added data manager flag (DZB)
    /// </remarks>
	/// <summary>
	/// LineDrawingController controls the drawing and editing of LineDrawings.
    /// Author: Dylan Z. Baker
	/// </summary>
	public class LineDrawingController : MRETUpdateBehaviour
	{
        /// <summary>
        /// Minimum distance between points.
        /// </summary>
        private static readonly float minimumDrawDistance = 0.001f;

        /// <summary>
        /// Mode for drawing.
        /// </summary>
        public enum DrawingMode { None, Free, Straight, Laser }

        /// <summary>
        /// Mode for drawing.
        /// </summary>
        public DrawingMode drawingMode
        {
            get
            {
                drawingMode = _drawingMode;
                return _drawingMode;
            }

            set
            {
                switch (value)
                {
                    case DrawingMode.None:
                        if (_drawingMode == DrawingMode.Laser)
                        {
                            hand.ToggleDrawingPointerOff();
                        }
                        if (currentCursor != null)
                        {
                            Destroy(currentCursor);
                        }
                        MRET.DataManager.SaveValue(LineDrawingManager.ISDRAWINGFLAGKEY, false);
                        break;

                    case DrawingMode.Free:
                    case DrawingMode.Straight:
                        if (_drawingMode == DrawingMode.Laser)
                        {
                            hand.ToggleDrawingPointerOff();
                        }
                        if (currentCursor == null)
                        {
                            currentCursor = Instantiate(hand.drawingCursor);
                            currentCursor.transform.localScale = hand.drawingCursorScale;
                        }
                        MRET.DataManager.SaveValue(LineDrawingManager.ISDRAWINGFLAGKEY, true);
                        break;

                    case DrawingMode.Laser:
                        if (_drawingMode != DrawingMode.Laser)
                        {
                            hand.ToggleDrawingPointerOn();
                        }
                        MRET.DataManager.SaveValue(LineDrawingManager.ISDRAWINGFLAGKEY, true);
                        break;
                }
                _drawingMode = value;
            }
        }
        [SerializeField]
        private DrawingMode _drawingMode;

        /// <summary>
        /// Type of drawing.
        /// </summary>
        public DrawingRender3dType drawingType;

        /// <summary>
        /// Hand that is drawing.
        /// </summary>
        public InputHand hand;

        /// <summary>
        /// Width/diameter of the drawing.
        /// </summary>
        public float drawingWidth;

        /// <summary>
        /// Color of the drawing.
        /// </summary>
        public Color32 drawingColor;

        /// <summary>
        /// Cutoff for generated drawings.
        /// </summary>
        public float LengthLimit = -1;

        /// <summary>
        /// Drawing point visual that is being touched.
        /// </summary>
        public DrawingPointVisual touchingVisual = null;

        /// <summary>
        /// Offset to apply to the cursor.
        /// </summary>
        [Tooltip("Offset to apply to the cursor.")]
        public Vector3 cursorOffset = Vector3.zero;

		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(LineDrawingController);
			}
		}

        /// <summary>
        /// Whether or not adding to beginning.
        /// </summary>
        private bool AddingToBeginning = false;

        /// <summary>
        /// The current drawing cursor.
        /// </summary>
        private GameObject currentCursor = null;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
				
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

        protected override void MRETAwake()
        {
            base.MRETAwake();

            // DZB: Stopgap solution, want control mode handling.
            MRET.DataManager.SaveValue(LineDrawingManager.ISDRAWINGFLAGKEY, false);
        }

        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (currentCursor != null)
            {
                currentCursor.transform.position = hand.transform.TransformPoint(cursorOffset);
            }

            // Handle preview if applicable.
            if (currentDrawing != null)
            {
                if (drawingMode == DrawingMode.Straight || drawingMode == DrawingMode.Laser)
                {
                    if (latchedPoint.HasValue == false)
                    {
                        UpdatePreview();
                    }
                    else
                    {
                        UpdatePreview(latchedPoint);
                    }
                }
                else if (drawingMode == DrawingMode.Free)
                {
                    Vector3[] drawingPoints = currentDrawing.points;
                    if (Vector3.Magnitude(currentCursor == null ? hand.transform.position : currentCursor.transform.position
                        - drawingPoints[drawingPoints.Length - 1]) >= minimumDrawDistance)
                    {
                        AddPoint();
                    }
                }
            }
        }

#region Events

        /// <summary>
        /// Latched point (smoothing out delay
        /// determining double press vs. single press).
        /// </summary>
        private Vector3? latchedPoint = null;

        private bool holdPressFilter = false;

        /// <summary>
        /// Event called when selected.
        /// </summary>
        public void OnSelect()
        {
            if (drawingMode == DrawingMode.Straight ||
                drawingMode == DrawingMode.Laser)
            {
                if (holdPressFilter)
                {
                    holdPressFilter = false;
                    return;
                }

                if (currentDrawing == null)
                {
                    if (touchingVisual != null && touchingVisual.isEndpoint)
                    {
                        currentDrawing = touchingVisual.drawing;
                    }
                    if (currentDrawing == null)
                    {
                        BeginNewLine(latchedPoint);
                    }
                    latchedPoint = null;
                }
                else
                {
                    UpdatePreview(latchedPoint);
                    SavePreview();
                    latchedPoint = null;
                }
            }
        }

        public void OnPossibleSelect()
        {
            if (drawingMode == DrawingMode.Straight)
            {
                latchedPoint = currentCursor == null ? hand.transform.position :currentCursor.transform.position;
            }
            else if (drawingMode == DrawingMode.Laser)
            {
                latchedPoint = GetPointerEnd();
            }
        }

        /// <summary>
        /// Event called on double select.
        /// </summary>
        public void OnDoubleSelect()
        {
            if (drawingMode == DrawingMode.Straight ||
                drawingMode == DrawingMode.Laser)
            {
                FinishCurrentLine();
                latchedPoint = null;
            }
        }

        /// <summary>
        /// Event called on hold.
        /// </summary>
        public void OnHold()
        {
            if (drawingMode == DrawingMode.Free)
            {
                if (touchingVisual != null && touchingVisual.isEndpoint)
                {
                    currentDrawing = touchingVisual.drawing;
                }
                if (currentDrawing == null)
                {
                    BeginNewLine();
                }
            }
            else if (drawingMode == DrawingMode.Straight ||
                drawingMode == DrawingMode.Laser)
            {
                if (currentDrawing == null)
                {
                    if (touchingVisual != null && touchingVisual.isEndpoint)
                    {
                        currentDrawing = touchingVisual.drawing;
                    }
                    if (currentDrawing == null)
                    {
                        BeginNewLine(latchedPoint);
                    }
                }
                else
                {
                    if (latchedPoint != null)
                    {
                        UpdatePreview(latchedPoint);
                        latchedPoint = null;
                    }
                    SavePreview();
                }
                snappingOn = true;
            }
        }

        /// <summary>
        /// Event called when hold has ended.
        /// </summary>
        public void OnUnhold()
        {
            if (currentDrawing != null)
            {
                FinishLine();
            }
        }

#endregion

#region Creation

        /// <summary>
        /// The current line drawing being created.
        /// </summary>
        private IInteractable3dDrawing currentDrawing = null;

        /// <summary>
        /// Whether snapping on axes is on.
        /// </summary>
        private bool snappingOn = false;

        /// <summary>
        /// Begin a new line.
        /// </summary>
        private void BeginNewLine(Vector3? point = null)
        {
            if (currentDrawing != null)
            {
                FinishCurrentLine();
            }
            //On the hololens use the position of the index finger instead of the position of the hand
#if HOLOLENS_BUILD
            FingerTracker fingerTracker = hand.GetComponentInChildren<FingerTracker>();
            Vector3 pos = fingerTracker.transform.position;
#endif
#if !HOLOLENS_BUILD
            Vector3 pos = currentCursor == null ? hand.transform.position : currentCursor.transform.position;
#endif
            if (drawingMode == DrawingMode.Laser)
            {
                pos = GetPointerEnd();
            }

            currentDrawing = ProjectManager.DrawingManager.CreateDrawing(
                "LineDrawing", null, point.HasValue ? point.Value : pos,
                currentCursor == null ? hand.transform.rotation : currentCursor.transform.rotation,
                Vector3.one, drawingType, drawingWidth, drawingColor,
                new Vector3[] { Vector3.zero });
            currentDrawing.LengthLimit = LengthLimit;
        }

        /// <summary>
        /// Finish the current line.
        /// </summary>
        private void FinishCurrentLine()
        {
            DestroyPreview();
            if (currentDrawing != null)
            {
                var serializedDrawing = currentDrawing.CreateSerializedType();
                Action<bool, string> SerializeAction = (bool successful, string message) =>
                {
                    if (successful)
                    {
                        // Record the action
                        ProjectManager.UndoManager.AddAction(
                            new AddSceneObjectAction(serializedDrawing),
                            new DeleteIdentifiableObjectAction(serializedDrawing));
                    }
                    else
                    {
                        LogWarning("Serialization of the drawing failed", nameof(FinishCurrentLine));
                    }
                };

                // Perform the serialization
                currentDrawing.Serialize(serializedDrawing, SerializeAction);

                currentDrawing = null;
            }
        }

        /// <summary>
        /// Add the point where the hand is to the line.
        /// </summary>
        private void AddPoint()
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[LineDrawingController->AddPoint] No current line.");
                return;
            }

            //On the hololens use the position of the index finger instead of the position of the hand
#if HOLOLENS_BUILD
            FingerTracker fingerTracker = hand.GetComponentInChildren<FingerTracker>();
            Vector3 pos = fingerTracker.transform.position;
#endif
#if !HOLOLENS_BUILD
            Vector3 pos = currentCursor == null ? hand.transform.position : currentCursor.transform.position;
#endif
            if (drawingMode == DrawingMode.Laser)
            {
                pos = GetPointerEnd();
            }

            Vector3 localPos = FitNewPointWithinCutoffBounds(currentDrawing.transform.InverseTransformPoint(pos), false);
            if (AddingToBeginning)
            {
                currentDrawing.InsertPoint(0, localPos);
            }
            else
            {
                currentDrawing.AddPoint(localPos);
            }
        }

#endregion

#region Edition

        /// <summary>
        /// Add to the line.
        /// </summary>
        /// <param name="lineToAddTo">Line to add to.</param>
        /// <param name="drawAtBeginning">Whether or not to add to beginning.</param>
        public void AddToLine(Interactable3dDrawing drawingToAddTo, bool drawAtBeginning = false)
        {
            if (currentDrawing != null)
            {
                Debug.LogError("[LineDrawingController->AddToLine] Already editing line.");
                return;
            }

            currentDrawing = drawingToAddTo;
            AddingToBeginning = drawAtBeginning;
        }

        /// <summary>
        /// Finish adding to the line.
        /// </summary>
        public void FinishLine()
        {
            if (drawingMode == DrawingMode.Free)
            {
                if (currentDrawing != null)
                {
                    FinishCurrentLine();
                }
            }
            else if (drawingMode == DrawingMode.Straight ||
                drawingMode == DrawingMode.Laser)
            {
                if (currentDrawing != null)
                {
                    SavePreview();
                }
                snappingOn = false;
                holdPressFilter = true;
            }

            currentDrawing = null;
            AddingToBeginning = false;
        }

#endregion

#region Preview

        /// <summary>
        /// Index of the preview point.
        /// </summary>
        private int previewIndex = -1;

        /// <summary>
        /// Update the preview.
        /// </summary>
        private void UpdatePreview(Vector3? point = null)
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[LineDrawingController->UpdatePreview] No current line.");
                return;
            }

            //On the hololens use the position of the index finger instead of the position of the hand
#if HOLOLENS_BUILD
            FingerTracker fingerTracker = hand.GetComponentInChildren<FingerTracker>();
            Vector3 pos = fingerTracker.transform.position;
#endif
#if !HOLOLENS_BUILD
            Vector3 pos = currentCursor == null ? hand.transform.position : currentCursor.transform.position;
#endif
            if (drawingMode == DrawingMode.Laser)
            {
                pos = GetPointerEnd();
            }

            Vector3 previewPoint = Vector3.zero;
            if (drawingMode == DrawingMode.Straight || drawingMode == DrawingMode.Laser)
            {
                if (snappingOn && currentDrawing.points.Length >= 2)
                {
                    Vector3[] drawingPoints = currentDrawing.points;
                    previewPoint = GetSnappedPos(drawingPoints[drawingPoints.Length - 2],
                        currentDrawing.transform.InverseTransformPoint(point.HasValue ? point.Value : pos));
                }
                else
                {
                    previewPoint = currentDrawing.transform.InverseTransformPoint(point.HasValue ? point.Value : pos);
                }
            }

            if (previewIndex < 0)
            {
                // Need to create preview.
                Vector3 localPos = FitNewPointWithinCutoffBounds(previewPoint, false);
                if (AddingToBeginning)
                {
                    currentDrawing.InsertPoint(0, previewPoint);
                    previewIndex = 0;
                }
                else
                {
                    currentDrawing.AddPoint(localPos);
                    previewIndex = currentDrawing.points.Length - 1;
                }
            }
            else
            {
                currentDrawing.ReplacePoint(previewIndex, FitNewPointWithinCutoffBounds(previewPoint, true));
            }
        }

        /// <summary>
        /// Destroy the preview.
        /// </summary>
        private void DestroyPreview()
        {
            if (currentDrawing == null)
            {
                Debug.Log("[LineDrawingController->DestroyPreview] No current line.");
                return;
            }

            if (previewIndex < 0)
            {
                Debug.Log("[LineDrawingController->DestroyPreview] No current preview.");
                return;
            }

            currentDrawing.RemovePoint(previewIndex);
            previewIndex = -1;
        }

        /// <summary>
        /// Add the preview to the line.
        /// </summary>
        private void SavePreview()
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[LineDrawingController->SavePreview] No current line.");
                return;
            }

            if (previewIndex < 0)
            {
                Debug.LogError("[LineDrawingController->SavePreview] No current preview.");
                return;
            }

            previewIndex = -1;
        }

#endregion

#region Snapping

        /// <summary>
        /// Get the snapped position.
        /// </summary>
        /// <param name="startPoint">Starting point.</param>
        /// <param name="endPoint">Ending point.</param>
        /// <returns>Ending point snapped on axis with starting point.</returns>
        private Vector3 GetSnappedPos(Vector3 startPoint, Vector3 endPoint)
        {
            Vector3 snappedEndPoint = endPoint;

            float deltaX = endPoint.x - startPoint.x;
            float deltaY = endPoint.y - startPoint.y;
            float deltaZ = endPoint.z - startPoint.z;
            float deltaXMag = Math.Abs(deltaX);
            float deltaYMag = Math.Abs(deltaY);
            float deltaZMag = Math.Abs(deltaZ);

            double thetaXY = NormalizeAngle((float) Math.Atan(deltaYMag / deltaXMag));
            double thetaXZ = NormalizeAngle((float) Math.Atan(deltaZMag / deltaXMag));
            double thetaYZ = NormalizeAngle((float) Math.Atan(deltaZMag / deltaYMag));

            if (thetaXY < DegreesToRadians(22.5f))
            {   // On X-axis in this direction. Y remains constant.
                snappedEndPoint.y = startPoint.y;

                if (thetaXZ < DegreesToRadians(22.5f))
                {   // On X-axis in this direction. Z remains constant.
                    snappedEndPoint.z = startPoint.z;
                }
                else if (thetaXZ < DegreesToRadians(67.5f))
                {   // On 45-degree line betw X-Z axes in this direction.
                    float greaterDelta = (deltaZMag > deltaXMag) ? deltaZMag : deltaXMag;

                    snappedEndPoint.x = (deltaX > 0) ? startPoint.x + greaterDelta : startPoint.x - greaterDelta;
                    snappedEndPoint.z = (deltaZ > 0) ? startPoint.z + greaterDelta : startPoint.z - greaterDelta;
                }
                else
                {   // On Z-axis in this direction. X remains constant.
                    snappedEndPoint.x = startPoint.x;
                }
            }
            else if (thetaXY < DegreesToRadians(67.5f))
            {   // On 45-degree line betw X-Y axes in this direction.
                float greaterDelta = (deltaYMag > deltaXMag) ? deltaYMag : deltaXMag;

                snappedEndPoint.x = (deltaX > 0) ? startPoint.x + greaterDelta : startPoint.x - greaterDelta;
                snappedEndPoint.y = (deltaY > 0) ? startPoint.y + greaterDelta : startPoint.y - greaterDelta;

                if (thetaYZ < DegreesToRadians(22.5f))
                {   // On Y-axis in this direction. Z remains constant.
                    snappedEndPoint.z = startPoint.z;
                }
                else if (thetaYZ < DegreesToRadians(67.5f))
                {   // On 45-degree line betw X-Z axes in this direction.
                    greaterDelta = (deltaZMag > deltaXMag) ?
                        ((deltaZMag > deltaYMag) ? deltaZMag : deltaYMag) :
                        ((deltaXMag > deltaYMag) ? deltaXMag : deltaYMag);

                    snappedEndPoint.x = (deltaX > 0) ? startPoint.x + greaterDelta : startPoint.x - greaterDelta;
                    snappedEndPoint.y = (deltaY > 0) ? startPoint.y + greaterDelta : startPoint.y - greaterDelta;
                    snappedEndPoint.z = (deltaZ > 0) ? startPoint.z + greaterDelta : startPoint.z - greaterDelta;
                }
                else
                {   // On Z-axis in this direction. Y remains constant.
                    snappedEndPoint.y = startPoint.y;
                }
            }
            else
            {   // On Y-axis in this direction. X remains constant.
                snappedEndPoint.x = startPoint.x;

                if (thetaYZ < DegreesToRadians(22.5f))
                {   // On Y-axis in this direction. Z remains constant.
                    snappedEndPoint.z = startPoint.z;
                }
                else if (thetaYZ < DegreesToRadians(67.5f))
                {   // On 45-degree line betw X-Z axes in this direction.
                    float greaterDelta = (deltaZMag > deltaXMag) ? deltaZMag : deltaXMag;

                    snappedEndPoint.y = (deltaY > 0) ? startPoint.y + greaterDelta : startPoint.y - greaterDelta;
                    snappedEndPoint.z = (deltaZ > 0) ? startPoint.z + greaterDelta : startPoint.z - greaterDelta;
                }
                else
                {   // On Z-axis in this direction. Y remains constant.
                    snappedEndPoint.y = startPoint.y;
                }
            }

            return snappedEndPoint;
        }

        /// <summary>
        /// Comverts degrees to radians.
        /// </summary>
        /// <param name="degreeValue">Degree value.</param>
        /// <returns>Radians value.</returns>
        private float DegreesToRadians(float degreeValue)
        {
            return (float) Math.PI * degreeValue / 180;
        }

        private float NormalizeAngle(float angleInRads)
        {
            while (angleInRads < 0)
            {
                angleInRads += 2 * (float) Math.PI;
            }

            while (angleInRads > 2 * (float) Math.PI)
            {
                angleInRads -= 2 * (float) Math.PI;
            }

            return angleInRads;
        }

#endregion

#region Cutoff

        /// <summary>
        /// Get the remaining cutoff distance.
        /// </summary>
        /// <returns>Remaining cutoff distance.</returns>
        public float GetRemainingDistance()
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[LineDrawingController->GetRemainingDistance] No current line.");
                return float.NegativeInfinity;
            }
            if (currentDrawing.LengthLimit <= 0)
            {
                return float.PositiveInfinity;
            }
            float currentLength = currentDrawing.length;
            if (previewIndex > 0)
            {
                currentLength = currentLength - Math.Abs(Vector3.Distance(
                    currentDrawing.points[previewIndex], currentDrawing.points[previewIndex - 1]));
            }
            else if (previewIndex == 0 && currentDrawing.points.Length > 1)
            {
                currentLength = currentLength - Math.Abs(Vector3.Distance(
                    currentDrawing.points[0], currentDrawing.points[1]));
            }
            if (currentLength >= currentDrawing.LengthLimit)
            {
                return 0;
            }
            return currentDrawing.LengthLimit - currentLength;
        }

        /// <summary>
        /// Fit the point into the cutoff size constraints.
        /// </summary>
        /// <param name="point">Point to add to line.</param>
        /// <param name="lastPointIsPreview">Whether the current last point in the line is a preview.</param>
        /// <returns>The point if it fits, a linearly interpolated point if not.</returns>
        private Vector3 FitNewPointWithinCutoffBounds(Vector3 point, bool lastPointIsPreview)
        {
            if (currentDrawing == null)
            {
                LogError("No current drawing.", nameof(FitNewPointWithinCutoffBounds));
                return Vector3.zero;
            }

            if (currentDrawing.LengthLimit < 0)
            {
                return point;
            }

            // Work with a copy of the drawing points
            Vector3[] drawingPoints = currentDrawing.points;

            float currentLineLength = currentDrawing.length;
            if (lastPointIsPreview && previewIndex >= 0 && drawingPoints.Length > 1)
            {
                currentLineLength = currentLineLength - Math.Abs(Vector3.Distance(
                    drawingPoints[drawingPoints.Length - 2], drawingPoints[drawingPoints.Length - 1]));
            }

            int lastPointIndex = lastPointIsPreview ? drawingPoints.Length - 2 : drawingPoints.Length - 1;

            if (currentLineLength > currentDrawing.LengthLimit)
            {
                LogError("Line already exceeds bounds.", nameof(FitNewPointWithinCutoffBounds));
                return drawingPoints[lastPointIndex];
            }

            float proposedDistance = Math.Abs(Vector3.Distance(drawingPoints[lastPointIndex], point));
            float lengthWithNewPoint = Math.Abs(proposedDistance + currentLineLength);
            if (lengthWithNewPoint > currentDrawing.LengthLimit)
            {
                float acceptableAmount = Math.Abs(currentDrawing.LengthLimit - currentLineLength);
                float acceptablePercentage = acceptableAmount / proposedDistance;
                return Vector3.Lerp(drawingPoints[lastPointIndex], point, acceptablePercentage);
            }
            else
            {
                return point;
            }
        }

        #endregion

        #region Pointer

        private Vector3 GetPointerEnd()
        {
            if (hand.drawingCursor != null)
            {
                return hand.pointerEnd;
            }

            return Vector3.zero;
        }

        #endregion
    }
}