// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.XRUI;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Note
{
    public class NoteWritingController : MonoBehaviour
    {
        public InputHand hand;

        private ControllerUIRaycastDetectionManager raycastDetector;
        private InteractableNote lastTouchingNote;
        private bool isPressed, drawing = false;

        public void ProcessTouchpadPress()
        {
            drawing = true;
        }

        public void ProcessTouchpadRelease()
        {
            drawing = false;
        }

        void Start()
        {
            raycastDetector = gameObject.GetComponent<ControllerUIRaycastDetectionManager>();
        }

        void Update()
        {
            if (raycastDetector.raycastPoint != null)
            {
                if (raycastDetector.intersectingObject)
                {
                    if (raycastDetector.intersectingObject.transform.parent != null)
                    {
                        InteractableNote touchingNote = raycastDetector.intersectingObject.GetComponentInParent<InteractableNote>();
                        if (touchingNote)
                        {
                            if (touchingNote.canDraw)
                            {
                                // TODO: Polling is not currently implemented for "navigating"
                                if (drawing)
                                {
                                    if (touchingNote != lastTouchingNote)
                                    {
                                        if (lastTouchingNote)
                                        {
                                            StopDrawing(lastTouchingNote);
                                        }

                                        lastTouchingNote = touchingNote;
                                        touchingNote.StartDrawing(gameObject);
                                        isPressed = true;
                                    }
                                }
                                else if (isPressed)
                                {
                                    StopDrawing(touchingNote);
                                    StopDrawing(lastTouchingNote);
                                }
                            }
                            else
                            {
                                lastTouchingNote = null;
                                isPressed = false;
                            }
                        }
                        else if (isPressed)
                        {
                            StopDrawing(lastTouchingNote);
                        }
                    }
                    else if (isPressed)
                    {
                        StopDrawing(lastTouchingNote);
                    }
                }
                else if (isPressed)
                {
                    StopDrawing(lastTouchingNote);
                }
            }
            else if (isPressed)
            {
                StopDrawing(lastTouchingNote);
            }
        }

        private void StopDrawing(InteractableNote touchingNote)
        {
            if (touchingNote)
            {
                touchingNote.StopDrawing(gameObject);
                lastTouchingNote = null;
                isPressed = false;
            }
        }
    }
}