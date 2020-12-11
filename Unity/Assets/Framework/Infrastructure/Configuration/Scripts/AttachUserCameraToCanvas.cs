using UnityEngine;

/**
 * Attaches the user camera to a canvas. If a canvas is not specified, the parent will
 * be queried for a canvas to assign the camera.</br>
 */
public class AttachUserCameraToCanvas : MonoBehaviour
{
    [Tooltip("The canvas to assign the user camera. If not specified, the parent will be queried for a canvas.")]
    Canvas canvas;

    [Tooltip("The canvas plane distance.")]
    public int planeDistance = 1;

    // Start is called before the first frame update
    void Start()
    {
        // Check if a canvas was assigned
        if (canvas == null)
        {
            // Try to locate a canvas in the parent
            canvas = GetComponentInParent<Canvas>();
        }

        // Make sure we have a valid canvas
        if (canvas != null)
        {
            // Obtain a reference to the session manager which should have the headset follower assigned
            SessionManager sessionManager = SessionManager.instance;

            // Assign the camera
            canvas.worldCamera = sessionManager.UserCamera;
            canvas.planeDistance = planeDistance;
        }
    }

}
