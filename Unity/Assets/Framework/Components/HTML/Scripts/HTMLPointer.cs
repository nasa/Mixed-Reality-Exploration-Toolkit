// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

public class HTMLPointer : MonoBehaviour
{
    public ControllerUIRaycastDetectionManager raycastDetector;
    public GameObject controller;
    public InputHand hand;
    public Material laserMaterial;

    private GameObject laserObject;
    private LineRenderer laser;

    void Update()
    {
        if (raycastDetector.intersectionStatus)
        {
            if (raycastDetector.intersectingObject)
            {
#if !HOLOLENS_BUILD
                if (raycastDetector.intersectingObject.GetComponentInChildren<ZenFulcrum.EmbeddedBrowser.Browser>() != null)
                {
                    if (raycastDetector.intersectingObject.GetComponentInChildren<ZenFulcrum.EmbeddedBrowser.Browser>().EnableInput)
                    {
                        // TODO.
                        /*
                        if (!laserObject)
                        {
                            if (pointerMode.pointerMode != VR_PointerMode.PointerMode.Disabled)
                            {
                                lastPointerMode = pointerMode.pointerMode;
                                pointerMode.SwitchMode(VR_PointerMode.PointerMode.Disabled);
                            }

                            laserObject = new GameObject("htmlLaser");
                            laserObject.transform.SetParent(controller.transform);
                            laser = laserObject.AddComponent<LineRenderer>();
                            laser.widthMultiplier = 0.0025f;
                            laser.material = laserMaterial;
                            laser.useWorldSpace = true;
                            laser.positionCount = 2;
                            laser.SetPosition(0, controller.transform.position);
                            laser.SetPosition(1, raycastDetector.raycastPoint);
                        }
                        else
                        {
                            if (pointerMode.pointerMode != VR_PointerMode.PointerMode.Disabled)
                            {
                                lastPointerMode = pointerMode.pointerMode;
                                pointerMode.SwitchMode(VR_PointerMode.PointerMode.Disabled);
                            }

                            laser.SetPosition(0, controller.transform.position);
                            laser.SetPosition(1, raycastDetector.raycastPoint);
                        }
                        */
                    }
                    else
                    {
                        /*
                        if (pointerMode.pointerMode == VR_PointerMode.PointerMode.Disabled)
                        {
                            pointerMode.SwitchMode(lastPointerMode);
                        }

                        if (laserObject)
                        {
                            Destroy(laserObject);
                        }
                        laserObject = null;
                        */
                    }
                }
#endif
            }
        }
        else
        {
            // TODO.
            /*
            if (pointerMode.pointerMode == VR_PointerMode.PointerMode.Disabled)
            {
                pointerMode.SwitchMode(lastPointerMode);
            }

            if (laserObject)
            {
                Destroy(laserObject);
            }
            laserObject = null;
            */
        }
    }
}