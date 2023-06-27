// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy;

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
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing.LineDrawingController))]
    public class LineDrawingControllerDeprecated : MRETUpdateBehaviour
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
                        MRET.DataManager.SaveValue(LineDrawingManagerDeprecated.ISDRAWINGFLAGKEY, false);
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
                        MRET.DataManager.SaveValue(LineDrawingManagerDeprecated.ISDRAWINGFLAGKEY, true);
                        break;

                    case DrawingMode.Laser:
                        if (_drawingMode != DrawingMode.Laser)
                        {
                            hand.ToggleDrawingPointerOn();
                        }
                        MRET.DataManager.SaveValue(LineDrawingManagerDeprecated.ISDRAWINGFLAGKEY, true);
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
        public LineDrawingManagerDeprecated.DrawingType drawingType;

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
        public float drawingCutoff = -1;

        /// <summary>
        /// Drawing point visual that is being touched.
        /// </summary>
        public DrawingPointVisualDeprecated touchingVisual = null;

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
				return nameof(LineDrawingControllerDeprecated);
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
            updateRate = UpdateFrequency.Hz20;

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
            if (currentLine != null)
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
                    if (Vector3.Magnitude(currentCursor == null ? hand.transform.position : currentCursor.transform.position
                        - currentLine.points[currentLine.numPoints - 1]) >= minimumDrawDistance)
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

                if (currentLine == null)
                {
                    if (touchingVisual != null && touchingVisual.isEndpoint)
                    {
                        currentLine = touchingVisual.drawing;
                    }
                    if (currentLine == null)
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
                    currentLine = touchingVisual.drawing;
                }
                if (currentLine == null)
                {
                    BeginNewLine();
                }
            }
            else if (drawingMode == DrawingMode.Straight ||
                drawingMode == DrawingMode.Laser)
            {
                if (currentLine == null)
                {
                    if (touchingVisual != null && touchingVisual.isEndpoint)
                    {
                        currentLine = touchingVisual.drawing;
                    }
                    if (currentLine == null)
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
            if (currentLine != null)
            {
                FinishLine();
            }
        }

#endregion

#region Creation

        /// <summary>
        /// The current line drawing being created.
        /// </summary>
        private LineDrawingDeprecated currentLine = null;

        /// <summary>
        /// Whether snapping on axes is on.
        /// </summary>
        private bool snappingOn = false;

        /// <summary>
        /// Begin a new line.
        /// </summary>
        private void BeginNewLine(Vector3? point = null)
        {
            if (currentLine != null)
            {
                FinishCurrentLine();
            }

            Vector3 pos = currentCursor == null ? hand.transform.position : currentCursor.transform.position;
            if (drawingMode == DrawingMode.Laser)
            {
                pos = GetPointerEnd();
            }

            Guid drawingUUID = Guid.NewGuid();
            currentLine = ProjectManager.SceneObjectManagerDeprecated.CreateLineDrawing(
                "LineDrawing-" + drawingUUID.ToString(), null, point.HasValue ? point.Value : pos,
                currentCursor == null ? hand.transform.rotation : currentCursor.transform.rotation,
                Vector3.one, drawingType, drawingWidth, drawingColor,
                new Vector3[] { Vector3.zero }, drawingUUID);
            currentLine.drawingCutoff = drawingCutoff;
        }

        /// <summary>
        /// Finish the current line.
        /// </summary>
        private void FinishCurrentLine()
        {
            DestroyPreview();
            ProjectManager.UndoManagerDeprecated.AddAction(ProjectActionDeprecated.AddDrawingAction(currentLine),
                ProjectActionDeprecated.DeleteDrawingAction(currentLine.name, currentLine.uuid.ToString()));
            currentLine = null;
        }

        /// <summary>
        /// Add the point where the hand is to the line.
        /// </summary>
        private void AddPoint()
        {
            if (currentLine == null)
            {
                Debug.LogError("[LineDrawingController->AddPoint] No current line.");
                return;
            }

            Vector3 pos = currentCursor == null ? hand.transform.position : currentCursor.transform.position;
            if (drawingMode == DrawingMode.Laser)
            {
                pos = GetPointerEnd();
            }

            Vector3 localPos = FitNewPointWithinCutoffBounds(currentLine.transform.InverseTransformPoint(pos), false);
            if (AddingToBeginning)
            {
                currentLine.AddPoint(localPos, 0);
            }
            else
            {
                currentLine.AddPoint(localPos);
            }
        }

#endregion

#region Edition

        /// <summary>
        /// Add to the line.
        /// </summary>
        /// <param name="lineToAddTo">Line to add to.</param>
        /// <param name="drawAtBeginning">Whether or not to add to beginning.</param>
        public void AddToLine(LineDrawingDeprecated lineToAddTo, bool drawAtBeginning = false)
        {
            if (currentLine != null)
            {
                Debug.LogError("[LineDrawingController->AddToLine] Already editing line.");
                return;
            }

            currentLine = lineToAddTo;
            AddingToBeginning = drawAtBeginning;
        }

        /// <summary>
        /// Finish adding to the line.
        /// </summary>
        public void FinishLine()
        {
            if (drawingMode == DrawingMode.Free)
            {
                if (currentLine != null)
                {
                    FinishCurrentLine();
                }
            }
            else if (drawingMode == DrawingMode.Straight ||
                drawingMode == DrawingMode.Laser)
            {
                if (currentLine != null)
                {
                    SavePreview();
                }
                snappingOn = false;
                holdPressFilter = true;
            }

            currentLine = null;
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
            if (currentLine == null)
            {
                Debug.LogError("[LineDrawingController->UpdatePreview] No current line.");
                return;
            }

            Vector3 pos = currentCursor == null ? hand.transform.position : currentCursor.transform.position;
            if (drawingMode == DrawingMode.Laser)
            {
                pos = GetPointerEnd();
            }

            Vector3 previewPoint = Vector3.zero;
            if (drawingMode == DrawingMode.Straight || drawingMode == DrawingMode.Laser)
            {
                if (snappingOn)
                {
                    previewPoint = GetSnappedPos(currentLine.points[currentLine.numPoints - 2],
                        currentLine.transform.InverseTransformPoint(point.HasValue ? point.Value : pos));
                }
                else
                {
                    previewPoint = currentLine.transform.InverseTransformPoint(point.HasValue ? point.Value : pos);
                }
            }

            if (previewIndex < 0)
            {
                // Need to create preview.
                Vector3 localPos = FitNewPointWithinCutoffBounds(previewPoint, false);
                if (AddingToBeginning)
                {
                    currentLine.AddPoint(previewPoint, 0);
                    previewIndex = 0;
                }
                else
                {
                    currentLine.AddPoint(localPos);
                    previewIndex = currentLine.numPoints - 1;
                }
            }
            else
            {
                currentLine.ReplacePoint(previewIndex, FitNewPointWithinCutoffBounds(previewPoint, true));
            }
        }

        /// <summary>
        /// Destroy the preview.
        /// </summary>
        private void DestroyPreview()
        {
            if (currentLine == null)
            {
                Debug.Log("[LineDrawingController->DestroyPreview] No current line.");
                return;
            }

            if (previewIndex < 0)
            {
                Debug.Log("[LineDrawingController->DestroyPreview] No current preview.");
                return;
            }

            currentLine.RemovePoint(previewIndex);
            previewIndex = -1;
        }

        /// <summary>
        /// Add the preview to the line.
        /// </summary>
        private void SavePreview()
        {
            if (currentLine == null)
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
            if (currentLine == null)
            {
                Debug.LogError("[LineDrawingController->GetRemainingDistance] No current line.");
                return float.NegativeInfinity;
            }
            if (currentLine.drawingCutoff <= 0)
            {
                return float.PositiveInfinity;
            }
            float currentLength = currentLine.length;
            if (previewIndex > 0)
            {
                currentLength = currentLength - Math.Abs(Vector3.Distance(
                    currentLine.points[previewIndex], currentLine.points[previewIndex - 1]));
            }
            else if (previewIndex == 0 && currentLine.points.Length > 1)
            {
                currentLength = currentLength - Math.Abs(Vector3.Distance(
                    currentLine.points[0], currentLine.points[1]));
            }
            if (currentLength >= currentLine.drawingCutoff)
            {
                return 0;
            }
            return currentLine.drawingCutoff - currentLength;
        }

        /// <summary>
        /// Fit the point into the cutoff size constraints.
        /// </summary>
        /// <param name="point">Point to add to line.</param>
        /// <param name="lastPointIsPreview">Whether the current last point in the line is a preview.</param>
        /// <returns>The point if it fits, a linearly interpolated point if not.</returns>
        private Vector3 FitNewPointWithinCutoffBounds(Vector3 point, bool lastPointIsPreview)
        {
            if (currentLine == null)
            {
                Debug.LogError("[LineDrawingController->FitNewPointWithinBounds] No current line.");
                return Vector3.zero;
            }

            if (currentLine.drawingCutoff < 0)
            {
                return point;
            }

            float currentLineLength = currentLine.length;
            if (lastPointIsPreview && previewIndex >= 0 && currentLine.points.Length > 1)
            {
                currentLineLength = currentLineLength - Math.Abs(Vector3.Distance(
                    currentLine.points[currentLine.numPoints - 2], currentLine.points[currentLine.numPoints - 1]));
            }

            int lastPointIndex = lastPointIsPreview ? currentLine.numPoints - 2 : currentLine.numPoints - 1;

            if (currentLineLength > currentLine.drawingCutoff)
            {
                Debug.LogError("[LineDrawingController->FitNewPointWithinBounds] Line already exceeds bounds.");
                return currentLine.points[lastPointIndex];
            }

            float proposedDistance = Math.Abs(Vector3.Distance(currentLine.points[lastPointIndex], point));
            float lengthWithNewPoint = Math.Abs(proposedDistance + currentLineLength);
            if (lengthWithNewPoint > currentLine.drawingCutoff)
            {
                float acceptableAmount = Math.Abs(currentLine.drawingCutoff - currentLineLength);
                float acceptablePercentage = acceptableAmount / proposedDistance;
                return Vector3.Lerp(currentLine.points[lastPointIndex], point, acceptablePercentage);
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