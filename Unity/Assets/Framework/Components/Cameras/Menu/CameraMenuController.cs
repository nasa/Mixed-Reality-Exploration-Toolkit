using UnityEngine;
using UnityEngine.UI;

public class CameraMenuController : MonoBehaviour
{
    public VRTK.SDK_BaseController.ControllerHand hand;
    public VRTK.VRTK_ControllerEvents controllerEvents;
    public GameObject imageCamera, videoCamera, cameraHolder;
    public CameraMenuController otherCameras;
    public ImageCameraCaptureManager imageCameraManager;
    public VideoCameraCaptureManager videoCameraManager;
    public ControlMode controlMode;
    public Toggle imageToggle, videoToggle, offToggle;
    public Vector3 imageCameraPosition, videoCameraPosition, imageCameraRotation, videoCameraRotation;
    public bool initialized = false;
    public GameObject menuPanel;
    public bool mostRecent = false;

    private bool initializingToggles = true;
    private int toggleCountDown = 0;
    private bool needToReinitializeToggles = false;

    public void Start()
    {
        if (!initialized && (!otherCameras || !otherCameras.initialized))
        {
            ExitMode();
            initialized = true;
        }
	}

    public void Update()
    {
        if (toggleCountDown > 0)
        {
            toggleCountDown--;
            if (toggleCountDown == 0)
            {
                if (needToReinitializeToggles)
                {
                    if (otherCameras)
                    {
                        if (otherCameras.imageToggle.isOn)
                        {
                            imageToggle.isOn = true;
                        }
                        else if (otherCameras.videoToggle.isOn)
                        {
                            videoToggle.isOn = true;
                        }
                        else
                        {
                            offToggle.isOn = true;
                        }
                    }
                    needToReinitializeToggles = false;
                }
                else
                {
                    if (hand == VRTK.SDK_BaseController.ControllerHand.Left)
                    {
                        if (imageCameraManager.capturingLeft || imageCameraManager.capturingRight)
                        {
                            initializingToggles = true;
                            if (otherCameras) otherCameras.initializingToggles = true;
                            imageToggle.isOn = true;
                            videoToggle.isOn = false;
                            offToggle.isOn = false;
                            initializingToggles = false;
                            if (otherCameras) otherCameras.initializingToggles = false;
                        }
                        else if (videoCameraManager.capturingLeft || videoCameraManager.capturingRight)
                        {
                            initializingToggles = true;
                            if (otherCameras) otherCameras.initializingToggles = true;
                            imageToggle.isOn = false;
                            videoToggle.isOn = true;
                            offToggle.isOn = false;
                            initializingToggles = false;
                            if (otherCameras) otherCameras.initializingToggles = false;
                        }
                        else
                        {
                            initializingToggles = true;
                            if (otherCameras) otherCameras.initializingToggles = true;
                            imageToggle.isOn = false;
                            videoToggle.isOn = false;
                            offToggle.isOn = true;
                            initializingToggles = false;
                            if (otherCameras) otherCameras.initializingToggles = false;
                        }
                    }
                    else
                    {
                        initializingToggles = true;
                        if (imageCameraManager.capturingRight || imageCameraManager.capturingLeft)
                        {
                            initializingToggles = true;
                            if (otherCameras) otherCameras.initializingToggles = true;
                            imageToggle.isOn = true;
                            videoToggle.isOn = false;
                            offToggle.isOn = false;
                            initializingToggles = false;
                            if (otherCameras) otherCameras.initializingToggles = false;
                        }
                        else if (videoCameraManager.capturingRight || videoCameraManager.capturingLeft)
                        {
                            initializingToggles = true;
                            if (otherCameras) otherCameras.initializingToggles = true;
                            imageToggle.isOn = false;
                            videoToggle.isOn = true;
                            offToggle.isOn = false;
                            initializingToggles = false;
                            if (otherCameras) otherCameras.initializingToggles = false;
                        }
                        else
                        {
                            initializingToggles = otherCameras.initializingToggles = true;
                            imageToggle.isOn = false;
                            videoToggle.isOn = false;
                            offToggle.isOn = true;
                            initializingToggles = otherCameras.initializingToggles = false;
                        }
                    }
                }
            }
        }
    }

    public void OnEnable()
    {
        toggleCountDown = 3;
        if (otherCameras)
        {
            if (otherCameras.mostRecent)
            {
                needToReinitializeToggles = true;
            }
            else
            {
                needToReinitializeToggles = false;
            }
        }
        mostRecent = true;
        if (otherCameras) otherCameras.mostRecent = false;
    }

    public void OnDisable()
    {
        toggleCountDown = -1;
    }

    public void ToggleImageCamera()
    {
        if (!initializingToggles && imageToggle.isOn && toggleCountDown == 0 && menuPanel.activeSelf)
        {
            EnableImageCamera();
        }
    }

    public void ToggleVideoCamera()
    {
        if (!initializingToggles && videoToggle.isOn && toggleCountDown == 0 && menuPanel.activeSelf)
        {
            EnableVideoCamera();
        }
    }

    public void ToggleCamerasOff()
    {
        if (!initializingToggles && offToggle.isOn && toggleCountDown == 0 && menuPanel.activeSelf)
        {
            DisableAllCameras();
        }
    }

    public void DisableAllCameras()
    {
        if (imageCamera.activeSelf || videoCamera.activeSelf && toggleCountDown == 0)
        {
            if (!menuPanel.activeInHierarchy || toggleCountDown > 0)
            {
                return;
            }
            imageCamera.SetActive(false);
            imageCameraManager.capturingLeft = imageCameraManager.capturingRight = false;
            videoCamera.SetActive(false);
            videoCameraManager.capturingLeft = videoCameraManager.capturingRight = false;
            controlMode.DisableAllControlTypes();
        }
    }

    // Exit camera without setting the global control mode.
    public void ExitMode()
    {
        imageCamera.SetActive(false);
        imageCameraManager.capturingLeft = imageCameraManager.capturingRight = false;
        videoCamera.SetActive(false);
        videoCameraManager.capturingLeft = videoCameraManager.capturingRight = false;
        initializingToggles = true;
        offToggle.isOn = true;
        initializingToggles = false;
    }

    public void EnableImageCamera()
    {
        if (toggleCountDown == 0)
        {
            if (otherCameras) otherCameras.ExitMode();
            imageCamera.SetActive(true);

            if (hand == VRTK.SDK_BaseController.ControllerHand.Left)
            {
                imageCameraManager.capturingLeft = true;
                imageCameraManager.capturingRight = false;
            }
            else
            {
                imageCameraManager.capturingLeft = false;
                imageCameraManager.capturingRight = true;
            }

            videoCamera.SetActive(false);
            videoCameraManager.capturingLeft = videoCameraManager.capturingRight = false;
            controlMode.EnterCameraMode();
            imageCamera.transform.SetParent(cameraHolder.transform);
            imageCamera.transform.localPosition = imageCameraPosition;
            imageCamera.transform.localRotation = Quaternion.Euler(imageCameraRotation);
        }
    }

    public void EnableVideoCamera()
    {
        if (toggleCountDown == 0)
        {
            if (otherCameras) otherCameras.ExitMode();
            imageCamera.SetActive(false);
            imageCameraManager.capturingLeft = false;
            imageCameraManager.capturingRight = false;
            videoCamera.SetActive(true);

            if (hand == VRTK.SDK_BaseController.ControllerHand.Left)
            {
                videoCameraManager.capturingLeft = true;
                videoCameraManager.capturingRight = false;
            }
            else
            {
                videoCameraManager.capturingLeft = false;
                videoCameraManager.capturingRight = true;
            }

            controlMode.EnterCameraMode();
            videoCamera.transform.SetParent(cameraHolder.transform);
            videoCamera.transform.localPosition = videoCameraPosition;
            videoCamera.transform.localRotation = Quaternion.Euler(videoCameraRotation);
        }
    }
}