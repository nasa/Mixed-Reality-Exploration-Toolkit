// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Components.Camera
{
    public class VideoCameraCaptureManager : MonoBehaviour
    {
        public GameObject recordIcon;
        public bool capturingLeft = false, capturingRight = false;
        public RockVR.Video.VideoCaptureCtrl vidCaptureCtrl;

        public void ToggleVideoCapture(InputHand hand)
        {
            // TODO: Better way.
            if (gameObject.activeSelf == false)
            {
                return;
            }

            if (vidCaptureCtrl.status == RockVR.Video.VideoCaptureCtrlBase.StatusType.STARTED)
            {
                // Stop capturing.
                vidCaptureCtrl.StopCapture();
                recordIcon.SetActive(false);
            }
            else if (((hand.handedness == InputHand.Handedness.left && capturingLeft)
                || (hand.handedness == InputHand.Handedness.right && capturingRight)
                || hand.handedness == InputHand.Handedness.neutral)
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
}