// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Integrations.XRUI;

namespace GSFC.ARVR.MRET.Components.Camera
{
    public class CameraMenuController : MenuController
    {
        /*
         * For switching between cameras on the SpectatorRendererManager call spectatorManager.SwitchCamera(index_of_camera_to_switch_to)
         * 
         * "index_of_camera_to_switch_to" is defined in the SpectatorRendererManager in the scene under 
         * MRET/Managers/SpectatorRendererManager. Ensure index 0 is always the Rig head location
         * it can be intrepreted as the "default" location for the camera to be rendered from.
         */


        public static readonly string imageCamera0Key = "MRET.INTERNAL.TOOLS.CAMERA.IMAGE.0";
        public static readonly string imageCamera1Key = "MRET.INTERNAL.TOOLS.CAMERA.IMAGE.1";
        public static readonly string videoCamera0Key = "MRET.INTERNAL.TOOLS.CAMERA.VIDEO.0";
        public static readonly string videoCamera1Key = "MRET.INTERNAL.TOOLS.CAMERA.VIDEO.1";
        public static readonly string bodyCameraKey = "MRET.INTERNAL.TOOLS.CAMERA.BODY.0";

        [Header("Camera Game Objects")]
        public GameObject imageCamera;
        public GameObject videoCamera;
        public GameObject bodyCamera;
        public GameObject cameraHolder;
        public GameObject bodyCameraHolder;


        [Header("Camera Managers")]
        public CameraMenuController otherCameras;
        public ImageCameraCaptureManager imageCameraManager;
        public VideoCameraCaptureManager videoCameraManager;
        public VideoCameraCaptureManager bodyCameraManager;
        //TODO: make sure this is changed to a virtual Camera Manager
        //public VirtualCameraManager spectatorManager;


        [Header("Other Options")]
        public InputHand hand;
        public Vector3 imageCameraPosition, videoCameraPosition, bodyCameraPosition,
            imageCameraRotation, videoCameraRotation, bodyCameraRotation;
        public bool initialized = false;
        public bool mostRecent = false;

        public override void Initialize()
        {
            Infrastructure.Framework.MRET.DataManager.SaveValue(imageCamera0Key, false);
            Infrastructure.Framework.MRET.DataManager.SaveValue(imageCamera1Key, false);
            Infrastructure.Framework.MRET.DataManager.SaveValue(videoCamera0Key, false);
            Infrastructure.Framework.MRET.DataManager.SaveValue(videoCamera1Key, false);
            Infrastructure.Framework.MRET.DataManager.SaveValue(bodyCameraKey, false);

            mostRecent = true;
            if (otherCameras) otherCameras.mostRecent = false;

            if (!initialized && (!otherCameras || !otherCameras.initialized))
            {
                ExitMode();
                initialized = true;
            }
        }

        public void ToggleImageCamera()
        {
            EnableImageCamera();
        }

        public void ToggleVideoCamera()
        {
            EnableVideoCamera();
        }

        public void ToggleBodyCamera()
        {
        
                
            if ((bool) Infrastructure.Framework.MRET.DataManager.FindPoint(bodyCameraKey) == false)
            {
                EnableBodyCamera();
            }
            else
            {
                DisableBodyCamera();
            }
            
        }

        public void ToggleCamerasOff()
        {
            DisableAllCameras();
        }

        public void DisableAllCameras()
        {
            if (imageCamera.activeSelf || videoCamera.activeSelf)
            {
                imageCamera.SetActive(false);
                videoCamera.SetActive(false);
                videoCameraManager.capturingLeft = videoCameraManager.capturingRight = false;
                Infrastructure.Framework.MRET.ControlMode.DisableAllControlTypes();


            }

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera0Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera1Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera0Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera1Key, false));
        }

        // Exit camera without setting the global control mode.
        public void ExitMode()
        {
            imageCamera.SetActive(false);
            videoCamera.SetActive(false);
            videoCameraManager.capturingLeft = videoCameraManager.capturingRight = false;

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera0Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera1Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera0Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera1Key, false));

            //spectatorManager.SwitchCamera(0);
        }

        public void EnableImageCamera()
        {
            if (otherCameras) otherCameras.ExitMode();
            imageCamera.SetActive(true);
            videoCamera.SetActive(false);

            videoCameraManager.capturingLeft = videoCameraManager.capturingRight = false;
            Infrastructure.Framework.MRET.ControlMode.EnterCameraMode();
            imageCamera.transform.SetParent(cameraHolder.transform);
            imageCamera.transform.localPosition = imageCameraPosition;
            imageCamera.transform.localRotation = Quaternion.Euler(imageCameraRotation);

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera0Key, hand.handedness == InputHand.Handedness.left));
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera1Key, hand.handedness == InputHand.Handedness.right));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera0Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera1Key, false));
        }

        public void EnableVideoCamera()
        {
            if (otherCameras) otherCameras.ExitMode();
            imageCamera.SetActive(false);
            videoCamera.SetActive(true);

            if (hand.handedness == InputHand.Handedness.left)
            {
                videoCameraManager.capturingLeft = true;
                videoCameraManager.capturingRight = false;
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                videoCameraManager.capturingLeft = false;
                videoCameraManager.capturingRight = true;
            }

            Infrastructure.Framework.MRET.ControlMode.EnterCameraMode();
            videoCamera.transform.SetParent(cameraHolder.transform);
            videoCamera.transform.localPosition = videoCameraPosition;
            videoCamera.transform.localRotation = Quaternion.Euler(videoCameraRotation);

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera0Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(imageCamera1Key, false));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera0Key, hand.handedness == InputHand.Handedness.left));
            DataManager.instance.SaveValue(new DataManager.DataValue(videoCamera1Key, hand.handedness == InputHand.Handedness.right));

            //Index of the Camera is set in the SpectatorRendererManager in the Attachment points

        }

        public void EnableBodyCamera()
        {
            bodyCamera.SetActive(true);
            /*
            if (hand.handedness == InputHand.Handedness.left)
            {
                bodyCameraManager.capturingLeft = true;
                bodyCameraManager.capturingRight = false;
            }
            else if (hand.handedness == InputHand.Handedness.right)
            {
                bodyCameraManager.capturingLeft = false;
                bodyCameraManager.capturingRight = true;
            }
            */

            bodyCamera.transform.SetParent(bodyCameraHolder.transform);
            bodyCamera.transform.localPosition = bodyCameraPosition;
            bodyCamera.transform.localRotation = Quaternion.Euler(bodyCameraRotation);

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(bodyCameraKey, true));
            


        }

        public void DisableBodyCamera()
        {
            bodyCamera.SetActive(false);
            
            //Body camera doesn't have a manager on it currently calling this would call errors
            //uncomment this section of code when future implimentation of the body camera includes a dedicated manager
            //bodyCameraManager.capturingLeft = bodyCameraManager.capturingRight = false;

            // Save to DataManager.
            DataManager.instance.SaveValue(new DataManager.DataValue(bodyCameraKey, false));

        }
    }
}