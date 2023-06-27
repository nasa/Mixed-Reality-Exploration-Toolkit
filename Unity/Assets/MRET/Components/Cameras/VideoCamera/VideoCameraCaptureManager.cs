// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

#if MRET_EXTENSION_ROCKVR && !HOLOLENS_BUILD
using RockVR.Video;
#endif
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.Cameras
{
    public class VideoCameraCaptureManager : MonoBehaviour
    {
        public GameObject recordIcon;
        public bool capturingLeft = false, capturingRight = false;
#if MRET_EXTENSION_ROCKVR && !HOLOLENS_BUILD
        public VideoCaptureCtrl vidCaptureCtrl;
#endif

        public void ToggleVideoCapture(InputHand hand)
        {
#if MRET_EXTENSION_ROCKVR && !HOLOLENS_BUILD
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
#endif
        }

        private void ToggleVideoCapture()
        {
#if MRET_EXTENSION_ROCKVR && !HOLOLENS_BUILD
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
#endif
        }
    }
}