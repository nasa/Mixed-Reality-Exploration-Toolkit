// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 29 December 2021: Created
    /// </remarks>
	/// <summary>
	/// DrawingPointVisual is a class that provides functionality for an Interactable3dDrawing
    /// point visualizations.
    /// Author: Dylan Z. Baker
	/// </summary>
	public class DrawingPointVisual : InteractableSceneObject<InteractableSceneObjectType>
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(DrawingPointVisual);

        /// <summary>
        /// The drawing associated with this visual.
        /// </summary>
        public Interactable3dDrawing drawing { get; private set; }

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
        private Action<Vector3, InputHand, DrawingPointVisual> lineAppendEndAction;

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
        private DrawingPointVisual touchingVisual = null;

        #region MRETUpdateBehaviour
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

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Initialize defaults
        }

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
        #endregion MRETUpdateBehaviour

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
            Action<Vector3, InputHand, DrawingPointVisual> lineAppendEndAction,
            bool isEndpoint, bool isFirstPoint, Interactable3dDrawing drawing)
        {
            this.pointChangeAction = pointChangeAction;
            this.pointChangeEndAction = pointChangeEndAction;
            this.lineAppendAction = lineAppendAction;
            this.lineAppendEndAction = lineAppendEndAction;
            this.isEndpoint = isEndpoint;
            this.isFirstPoint = isFirstPoint;
            this.drawing = drawing;
        }

        public void StartTouching(DrawingPointVisual touching)
        {
            touchingVisual = touching;
        }

        public void StopTouching(DrawingPointVisual touching)
        {
            if (touchingVisual == touching)
            {
                touchingVisual = null;
            }
        }

        /// <seealso cref="InteractableSceneObject{T}.AfterBeginGrab(InputHand)"/>
        protected override void AfterBeginGrab(InputHand hand)
        {
            base.AfterBeginGrab(hand);

            //if (!isEndpoint)
            {
                if (grabbingHand != hand)
                {
                    grabbingHand = hand;
                }
            }
        }

        /// <seealso cref="InteractableSceneObject{T}.AfterEndGrab(InputHand)"/>
        protected override void AfterEndGrab(InputHand hand)
        {
            base.AfterEndGrab(hand);

            //if (!isEndpoint)
            {
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

        /// <seealso cref="InteractableSceneObject{T}.BeginTouch(InputHand)"/>
        protected override void AfterBeginTouch(InputHand hand)
        {
            base.AfterBeginTouch(hand);
            HandInteractor hi = hand.GetComponent<HandInteractor>();
            if (hi != null)
            {
                IInteractable[] otherInteractables = hi.touchedObjects;
                foreach (IInteractable otherInteractable in otherInteractables)
                {
                    if (otherInteractable is DrawingPointVisual otherVisual)
                    {
                        if (otherVisual.isEndpoint)
                        {
                            touchingVisual = otherVisual;
                        }

                        if (otherVisual.drawing != drawing)
                        {
                            otherVisual.StartTouching(this);
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

        /// <seealso cref="InteractableSceneObject{T}.EndTouch(InputHand)"/>
        protected override void AfterEndTouch(InputHand hand)
        {
            base.AfterEndTouch(hand);

            if (_touchingHand != null)
            {
                HandInteractor hi = _touchingHand.GetComponent<HandInteractor>();
                if (hi != null)
                {
                    IInteractable[] otherInteractables = hi.touchedObjects;
                    foreach (IInteractable otherInteractable in otherInteractables)
                    {
                        if (otherInteractable is DrawingPointVisual otherVisual)
                        {
                            otherVisual.StopTouching(this);
                            touchingVisual = null;
                        }
                    }
                }

                EndTouching(_touchingHand);
            }
            _touchingHand = null;
        }

        /// <summary>
        /// The last touching hand.
        /// </summary>
        private InputHand lastTouchingHand = null;

        /// <summary>
        /// Start touching on the provided hand.
        /// </summary>
        /// <param name="hand">Hand that is touching.</param>
        private void StartTouching(InputHand hand)
        {
            if (hand == null)
            {
                Debug.LogError("[DrawingPointVisual->StartTouching] No hand.");
                return;
            }

            LineDrawingController lineDrawingController =
                hand.gameObject.GetComponent<LineDrawingController>();
            if (lineDrawingController == null)
            {
                Debug.LogError("[DrawingPointVisual->StartTouching] No drawing controller.");
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
                Debug.LogError("[DrawingPointVisual->EndTouching] No hand.");
                return;
            }

            LineDrawingController lineDrawingController =
                hand.gameObject.GetComponent<LineDrawingController>();
            if (lineDrawingController == null)
            {
                Debug.LogError("[DrawingPointVisual->EndTouching] No drawing controller.");
                return;
            }
            if (lineDrawingController.touchingVisual == this)
            {
                lineDrawingController.touchingVisual = null;
            }
        }
	}
}