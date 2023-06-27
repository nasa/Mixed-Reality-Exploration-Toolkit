// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 29 December 2021: Created
    /// </remarks>
	/// <summary>
	/// DrawingPointVisualDeprecated is a class that provides functionality for LineDrawing
    /// point visualizations.
    /// Author: Dylan Z. Baker
	/// </summary>
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.DrawingPointVisual))]
    public class DrawingPointVisualDeprecated : SceneObjectDeprecated
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(DrawingPointVisualDeprecated);
			}
		}

        /// <summary>
        /// The drawing associated with this visual.
        /// </summary>
        public LineDrawingDeprecated drawing { get; private set; }

        /// <summary>
        /// Whether this point is an endpoint.
        /// </summary>
        public bool isEndpoint { get; private set; }

        /// <summary>
        /// Whether this point is the first point.
        /// </summary>
        public bool isFirstPoint { get; private set; }

        /// <summary>
        /// Action to call when a point is changed.
        /// </summary>
        private Action<Vector3, InputHand> pointChangeAction;

        /// <summary>
        /// Action to call when a point is done being changed.
        /// </summary>
        private Action<Vector3, InputHand> pointChangeEndAction;

        /// <summary>
        /// Action to call when appending to a line.
        /// </summary>
        private Action<Vector3, InputHand> lineAppendAction;

        /// <summary>
        /// Action to call when done appending to a line.
        /// </summary>
        private Action<Vector3, InputHand, DrawingPointVisualDeprecated> lineAppendEndAction;

        /// <summary>
        /// The hand currently grabbing the point.
        /// </summary>
        private InputHand grabbingHand = null;

        /// <summary>
        /// The hand currently touching the point.
        /// </summary>
        private InputHand _touchingHand = null;

        /// <summary>
        /// Whether or not the line is being appended to.
        /// </summary>
        private bool lineAppending = false;

        /// <summary>
        /// The currently touching endpoint visual.
        /// </summary>
        private DrawingPointVisualDeprecated touchingVisual = null;

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

        /// <summary>
        /// Initializes the visual.
        /// </summary>
        /// <param name="pointChangeAction">Action to call when the point is changed.</param>
        /// <param name="pointChangeEndAction">Action to call when the point is done being changed.</param>
        /// <param name="lineAppendAction">Action to call when a line is being appended.</param>
        /// <param name="lineAppendEndAction">Action to call when the line is done being appended.</param>
        /// <param name="isEndpoint">Whether the point is an endpoint.</param>
        /// <param name="isFirstPoint">Whether the point is the first point.</param>
        /// <param name="drawing">The drawing associated with this visual.</param>
        public void Initialize(Action<Vector3, InputHand> pointChangeAction,
            Action<Vector3, InputHand> pointChangeEndAction,
            Action<Vector3, InputHand> lineAppendAction,
            Action<Vector3, InputHand, DrawingPointVisualDeprecated> lineAppendEndAction,
            bool isEndpoint, bool isFirstPoint, LineDrawingDeprecated drawing)
        {
            this.pointChangeAction = pointChangeAction;
            this.pointChangeEndAction = pointChangeEndAction;
            this.lineAppendAction = lineAppendAction;
            this.lineAppendEndAction = lineAppendEndAction;
            this.isEndpoint = isEndpoint;
            this.isFirstPoint = isFirstPoint;
            this.drawing = drawing;
        }

        public void StartTouching(DrawingPointVisualDeprecated touching)
        {
            touchingVisual = touching;
        }

        public void StopTouching(DrawingPointVisualDeprecated touching)
        {
            if (touchingVisual == touching)
            {
                touchingVisual = null;
            }
        }

        /// <summary>
        /// Called when a grab has begun.
        /// </summary>
        /// <param name="hand">Hand that is grabbing.</param>
        public override void BeginGrab(InputHand hand)
        {
            //if (!isEndpoint)
            {
                base.BeginGrab(hand);
                if (grabbingHand != hand)
                {
                    grabbingHand = hand;
                }
            }
        }

        /// <summary>
        /// Called when a grab has ended.
        /// </summary>
        /// <param name="hand">Hand that was grabbing</param>
        public override void EndGrab(InputHand hand)
        {
            //if (!isEndpoint)
            {
                base.EndGrab(hand);
                if (grabbingHand == hand)
                {
                    if (pointChangeEndAction != null)
                    {
                        pointChangeEndAction.Invoke(hand.transform.position, hand);
                    }
                    grabbingHand = null;
                }
            }
        }

        /// <summary>
        /// Called when a touch has begun.
        /// </summary>
        /// <param name="hand">Hand that is touching.</param>
        protected override void BeginTouch(InputHand hand)
        {
            base.BeginTouch(hand);
            HandInteractorDeprecated hi = hand.GetComponent<HandInteractorDeprecated>();
            if (hi != null)
            {
                SceneObjectDeprecated[] otherInteractables = hi.touchedObjects;
                foreach (SceneObjectDeprecated otherInteractable in otherInteractables)
                {
                    if (otherInteractable is DrawingPointVisualDeprecated)
                    {
                        if (((DrawingPointVisualDeprecated) otherInteractable).drawing != drawing)
                        {
                            ((DrawingPointVisualDeprecated) otherInteractable).StartTouching(this);
                        }
                    }
                }
            }

            if (_touchingHand != null)
            {
                EndTouching(hand);
            }
            _touchingHand = hand;
            StartTouching(hand);
        }

        /// <summary>
        /// Called when a touch has ended.
        /// </summary>
        protected override void EndTouch()
        {
            base.EndTouch();

            if (_touchingHand != null)
            {
                HandInteractorDeprecated hi = _touchingHand.GetComponent<HandInteractorDeprecated>();
                if (hi != null)
                {
                    SceneObjectDeprecated[] otherInteractables = hi.touchedObjects;
                    foreach (SceneObjectDeprecated otherInteractable in otherInteractables)
                    {
                        if (otherInteractable is DrawingPointVisualDeprecated)
                        {
                            ((DrawingPointVisualDeprecated) otherInteractable).StopTouching(this);
                        }
                    }
                }

                EndTouching(_touchingHand);
            }
            _touchingHand = null;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            DrawingPointVisualDeprecated otherVisual = other.GetComponent<DrawingPointVisualDeprecated>();
            if (otherVisual != null)
            {
                if (otherVisual.isEndpoint)
                {
                    touchingVisual = otherVisual;
                }
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);
            DrawingPointVisualDeprecated otherVisual = other.GetComponent<DrawingPointVisualDeprecated>();
            if (otherVisual != null)
            {
                if (otherVisual == touchingVisual)
                {
                    touchingVisual = null;
                }
            }
        }

        /// <summary>
        /// The last touching hand.
        /// </summary>
        private InputHand lastTouchingHand = null;

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
		{
			base.MRETUpdate();
            if (isEndpoint)
            {
                if (_touchingHand != null)
                {
                    if (_touchingHand.navigatePressing)
                    {
                        if (!lineAppending)
                        {
                            lineAppending = true;
                            lineAppendAction.Invoke(_touchingHand.transform.position, _touchingHand);
                            lastTouchingHand = _touchingHand;
                        }
                    }
                    else
                    {
                        if (lineAppending)
                        {
                            lineAppending = false;
                            lineAppendEndAction.Invoke(_touchingHand.transform.position, _touchingHand, touchingVisual);
                            lastTouchingHand = null;
                        }
                    }
                }
                else if (lastTouchingHand != null)
                {
                    if (!lastTouchingHand.navigatePressing)
                    {
                        lineAppending = false;
                        lineAppendEndAction.Invoke(lastTouchingHand.transform.position, lastTouchingHand, touchingVisual);
                        lastTouchingHand = null;
                    }
                }
                /*else
                {
                    if (lineAppending)
                    {
                        if (_touchingHand != null)
                        {
                            if (_touchingHand.navigatePressing)
                            {
                                lineAppendAction.Invoke(_touchingHand.transform.position, _touchingHand);
                            }
                            else
                            {
                                lineAppending = false;
                                lineAppendEndAction.Invoke(lastTouchingHand.transform.position, lastTouchingHand, touchingVisual);
                                lastTouchingHand = null;
                            }
                        }
                        else
                        {
                            lineAppending = false;
                            Debug.Log("hey");
                            lineAppendEndAction.Invoke(lastTouchingHand.transform.position, lastTouchingHand, touchingVisual);
                            lastTouchingHand = null;
                        }
                    }
                }*/
            }
            //else
            {
                if (grabbingHand != null && pointChangeAction != null)
                {
                    pointChangeAction.Invoke(grabbingHand.transform.position, grabbingHand);
                }
            }
		}

        /// <summary>
        /// Start touching on the provided hand.
        /// </summary>
        /// <param name="hand">Hand that is touching.</param>
        private void StartTouching(InputHand hand)
        {
            if (hand == null)
            {
                Debug.LogError("[DrawingPointVisualDeprecated->StartTouching] No hand.");
                return;
            }

            LineDrawingControllerDeprecated lineDrawingController =
                hand.gameObject.GetComponent<LineDrawingControllerDeprecated>();
            if (lineDrawingController == null)
            {
                Debug.LogError("[DrawingPointVisualDeprecated->StartTouching] No drawing controller.");
                return;
            }
            lineDrawingController.touchingVisual = this;
        }

        /// <summary>
        /// End touching on the provided hand.
        /// </summary>
        /// <param name="hand">Hand that is no longer touching.</param>
        private void EndTouching(InputHand hand)
        {
            if (hand == null)
            {
                Debug.LogError("[DrawingPointVisualDeprecated->EndTouching] No hand.");
                return;
            }

            LineDrawingController lineDrawingController =
                hand.gameObject.GetComponent<LineDrawingController>();
            if (lineDrawingController == null)
            {
                Debug.LogError("[DrawingPointVisualDeprecated->EndTouching] No drawing controller.");
                return;
            }
            if (lineDrawingController.touchingVisual == this)
            {
                lineDrawingController.touchingVisual = null;
            }
        }
	}
}