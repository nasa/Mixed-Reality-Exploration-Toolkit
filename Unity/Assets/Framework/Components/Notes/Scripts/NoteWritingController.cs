// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Components.Notes
{
    public class NoteWritingController : MonoBehaviour
    {
        public InputHand hand;

        private ControllerUIRaycastDetectionManager raycastDetector;
        private Note lastTouchingNote;
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
                        Note touchingNote = raycastDetector.intersectingObject.GetComponentInParent<Note>();
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

        private void StopDrawing(Note touchingNote)
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