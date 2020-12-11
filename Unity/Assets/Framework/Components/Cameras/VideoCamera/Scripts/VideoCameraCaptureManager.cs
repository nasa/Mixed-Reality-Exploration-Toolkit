using UnityEngine;

public class VideoCameraCaptureManager : MonoBehaviour
{
    public VRTK.VRTK_ControllerEvents leftControllerEvents, rightControllerEvents;
    public GameObject recordIcon;
    public bool capturingLeft = false, capturingRight = false;
    public RockVR.Video.VideoCaptureCtrl vidCaptureCtrl;

    private bool isRecording = false;

    public void Start()
    {
        if (VRDesktopSwitcher.isDesktopEnabled())
        {
            EventManager.OnLeftClick += ToggleVideoCapture;
        }
        else
        {
            leftControllerEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(ToggleVideoCapture);
            rightControllerEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(ToggleVideoCapture);
        }
    }

    public void ToggleVideoCapture(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.STARTED)
        {
            // Stop capturing.
            vidCaptureCtrl.StopCapture();
            recordIcon.SetActive(false);
        }
        else if (((e.controllerReference.hand == VRTK.SDK_BaseController.ControllerHand.Left && capturingLeft)
            || (e.controllerReference.hand == VRTK.SDK_BaseController.ControllerHand.Right && capturingRight))
            && (vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.STOPPED
            || vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.NOT_START
            || vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.FINISH))
        {
            // Start capturing.
            vidCaptureCtrl.StartCapture();
            recordIcon.SetActive(true);
        }
    }

    private void ToggleVideoCapture()
    {
        if (vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.STARTED)
        {
            // Stop capturing.
            vidCaptureCtrl.StopCapture();
            recordIcon.SetActive(false);
        }
        else if (vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.STOPPED
            || vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.NOT_START
            || vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.FINISH)
        {
            // Start capturing.
            vidCaptureCtrl.StartCapture();
            recordIcon.SetActive(true);
        }
    }
}