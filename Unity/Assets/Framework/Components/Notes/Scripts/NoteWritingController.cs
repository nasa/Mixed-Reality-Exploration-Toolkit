using UnityEngine;

public class NoteWritingController : MonoBehaviour
{
    private ControllerUIRaycastDetectionManager raycastDetector;
    private VRTK.VRTK_ControllerEvents controllerEvents;
    private Note lastTouchingNote;
    private bool isPressed;

	void Start()
    {
        raycastDetector = gameObject.GetComponent<ControllerUIRaycastDetectionManager>();
        controllerEvents = GetComponent<VRTK.VRTK_ControllerEvents>();
	}
	
	void Update()
    {
        if (raycastDetector.raycastPoint != null)
        {
            if (raycastDetector.intersectingObject)
            {
                if (raycastDetector.intersectingObject.transform.parent != null)
                {
                    Note touchingNote = raycastDetector.intersectingObject.transform.parent.GetComponent<Note>();
                    if (touchingNote)
                    {
                        if (touchingNote.canDraw)
                        {
                            if (controllerEvents.touchpadPressed)
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