using UnityEngine;

public class HTMLPointer : MonoBehaviour
{
    public ControllerUIRaycastDetectionManager raycastDetector;
    public GameObject controller;
    public VR_PointerMode pointerMode;
    public Material laserMaterial;

    private GameObject laserObject;
    private LineRenderer laser;
    private VR_PointerMode.PointerMode lastPointerMode = VR_PointerMode.PointerMode.Environment;

    void Update()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            if (raycastDetector.intersectionStatus)
            {
                if (raycastDetector.intersectingObject)
                {
                    if (raycastDetector.intersectingObject.GetComponentInChildren<ZenFulcrum.EmbeddedBrowser.Browser>() != null)
                    {
                        if (raycastDetector.intersectingObject.GetComponentInChildren<ZenFulcrum.EmbeddedBrowser.Browser>().EnableInput)
                        {
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
                        }
                        else
                        {
                            if (pointerMode.pointerMode == VR_PointerMode.PointerMode.Disabled)
                            {
                                pointerMode.SwitchMode(lastPointerMode);
                            }

                            if (laserObject)
                            {
                                Destroy(laserObject);
                            }
                            laserObject = null;
                        }
                    }
                }
            }
            else
            {
                if (pointerMode.pointerMode == VR_PointerMode.PointerMode.Disabled)
                {
                    pointerMode.SwitchMode(lastPointerMode);
                }

                if (laserObject)
                {
                    Destroy(laserObject);
                }
                laserObject = null;
            }
        }
    }
}