// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

/*
 * 
 * This file inherits from the FirstPersonController class which is
 * apart of the FPSController prefab which (included with standard assets)
 * This extends the functionality to cover teleporting, flying (implemented in base class),
 * and zooming in and out. 
 * 
 */

using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.Legacy
{
    public class DesktopController : MonoBehaviour
    {

        //private vars
        private RaycastHit rayHit;
        private Vector3 newPos;
        private bool teleport = false;
        private bool zoomEnabled = false;
        private float minFov = 15.0f;
        private float maxFov = 90.0f;
        private float sensitivity = 10.0f;

        void Start()
        {
            EventManager.OnRightClick += teleportPlayer;
            EventManager.NegativeScrollWheel += zoomOut;
            EventManager.PositiveScrollWheel += zoomIn;

            newPos = transform.position;

        }

        //function to enable teleporting if the button in the controller menu is toggled
        public void enableTeleport()
        {

            /*if (VRDesktopSwitcher.isDesktopEnabled())
            {
                if (teleport)
                {
                    disableTeleport();
                }
                else
                {
                    teleport = true;
                }
            }*/

        }

        //function to disable teleporting
        void disableTeleport()
        {
            teleport = false;
        }

        //function to enable zooming
        public void enableZoom()
        {
            zoomEnabled = true;
        }

        //function to disable zooming
        void disableZoom()
        {
            zoomEnabled = false;
        }

        //function for teleportation
        void teleportPlayer()
        {
            if (teleport)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out rayHit))
                {
                    newPos = rayHit.point;
                    transform.position = new Vector3(newPos.x, 2.0f, newPos.z);
                }
            }
        }

        //function to zoom out
        void zoomOut()
        {
            if (zoomEnabled)
            {
                float fov = Camera.main.fieldOfView;
                fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity;
                fov = Mathf.Clamp(fov, minFov, maxFov);
                Camera.main.fieldOfView = fov;
                Camera.main.orthographicSize = Camera.main.orthographicSize += 1;
            }

        }

        //function to zoom in
        void zoomIn()
        {
            if (zoomEnabled)
            {
                float fov = Camera.main.fieldOfView;
                fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity;
                fov = Mathf.Clamp(fov, minFov, maxFov);
                Camera.main.fieldOfView = fov;

                if (Camera.main.orthographicSize > 1)
                {
                    Camera.main.orthographicSize = Camera.main.orthographicSize -= 1;
                }
            }
        }

    }

}


