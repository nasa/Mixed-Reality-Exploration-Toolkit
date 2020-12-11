using System;
using UnityEngine;

public class DualAxisRotationControl : MonoBehaviour
{
    public VRTK.VRTK_ControllerEvents cEvents;
    public DualAxisRotatableObject rotatingObject;
    public ControlMode controlMode;
    public DualAxisRotationControl otherControl;

    bool desktop;

    private float currentX, currentY;
    private float xObjRotation;
    private bool isTouching = false;

    void Start()
    {
        foreach (ControllerMenuManager man in FindObjectsOfType<ControllerMenuManager>())
        {
            if (man.IsDimmed())
            {
                cEvents = man.GetComponentInParent<VRTK.VRTK_ControllerEvents>();
            }
        }

        controlMode = FindObjectOfType<ControlMode>();
        desktop = VRDesktopSwitcher.isDesktopEnabled();

        if (VRDesktopSwitcher.isDesktopEnabled())
        {
            EventManager.OnLeftClick += DoTouchpadTouchStart;
            EventManager.OnLeftClickUp += DoTouchpadTouchEnd;
            EventManager.TKeyPressed += DoTouchpadAxisChangedN;
            EventManager.GKeyPressed += DoTouchpadAxisChangedS;
            EventManager.FKeyPressed += DoTouchpadAxisChangedE;
            EventManager.HKeyPressed += DoTouchpadAxisChangedW;
        }
        else
        {
            cEvents.TouchpadAxisChanged += new VRTK.ControllerInteractionEventHandler(DoTouchpadAxisChanged);
            cEvents.TouchpadTouchStart += new VRTK.ControllerInteractionEventHandler(DoTouchpadTouchStart);
            cEvents.TouchpadTouchEnd += new VRTK.ControllerInteractionEventHandler(DoTouchpadTouchEnd);
        }
    }

    void FixedUpdate()
    {
        if (isTouching && rotatingObject)
        {
            if (Math.Abs(currentX) > 0.3f)
            {
                if (rotatingObject.horizontalObject != null)
                {
                    if (rotatingObject.horizontalTransformItem == DualAxisRotatableObject.ControlAttribute.Position)
                    {   // Slide object.
                        Vector3 currentPos = rotatingObject.horizontalObject.transform.localPosition;
                        float displacement = rotatingObject.motionSpeed * currentX;
                        switch (rotatingObject.horizontalObjectAxis)
                        {
                            case DualAxisRotatableObject.RotationAxis.X:
                                rotatingObject.horizontalObject.transform.localPosition =
                                    new Vector3(currentPos.x + displacement, currentPos.y, currentPos.z);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Y:
                                rotatingObject.horizontalObject.transform.localPosition =
                                    new Vector3(currentPos.x, currentPos.y + displacement, currentPos.z);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Z:
                                rotatingObject.horizontalObject.transform.localPosition =
                                    new Vector3(currentPos.x, currentPos.y, currentPos.z + displacement);
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {   // Rotate Object.
                        switch (rotatingObject.horizontalObjectAxis)
                        {
                            case DualAxisRotatableObject.RotationAxis.X:
                                rotatingObject.horizontalObject.Rotate(currentX * rotatingObject.motionSpeed, 0, 0);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Y:
                                rotatingObject.horizontalObject.Rotate(0, currentX * rotatingObject.motionSpeed, 0);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Z:
                                rotatingObject.horizontalObject.Rotate(0, 0, currentX * rotatingObject.motionSpeed);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            if (Math.Abs(currentY) > 0.3f)
            {
                if (rotatingObject.verticalObject != null)
                {
                    if (rotatingObject.verticalTransformItem == DualAxisRotatableObject.ControlAttribute.Position)
                    {   // Slide object.
                        Vector3 currentPos = rotatingObject.verticalObject.transform.localPosition;
                        float displacement = rotatingObject.motionSpeed * currentY;
                        switch (rotatingObject.verticalObjectAxis)
                        {
                            case DualAxisRotatableObject.RotationAxis.X:
                                rotatingObject.verticalObject.transform.localPosition =
                                    new Vector3(currentPos.x + displacement, currentPos.y, currentPos.z);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Y:
                                rotatingObject.verticalObject.transform.localPosition =
                                    new Vector3(currentPos.x, currentPos.y + displacement, currentPos.z);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Z:
                                rotatingObject.verticalObject.transform.localPosition =
                                    new Vector3(currentPos.x, currentPos.y, currentPos.z + displacement);
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {   // Rotate Object.
                        switch (rotatingObject.verticalObjectAxis)
                        {
                            case DualAxisRotatableObject.RotationAxis.X:
                                rotatingObject.verticalObject.Rotate(currentY * rotatingObject.motionSpeed, 0, 0);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Y:
                                rotatingObject.verticalObject.Rotate(0, currentY * rotatingObject.motionSpeed, 0);
                                break;

                            case DualAxisRotatableObject.RotationAxis.Z:
                                rotatingObject.verticalObject.Rotate(0, 0, currentY * rotatingObject.motionSpeed);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }
    }

    // Exit dual axis rotation without setting the global control mode.
    public void ExitMode()
    {
        if (rotatingObject)
        {
            SetRotatingObject(null);
        }
    }

    public void SelectRotatingObject(DualAxisRotatableObject darObj)
    {
        if (darObj == null)
        {
            SetRotatingObject(null);
            controlMode.DisableAllControlTypes();
        }
        else
        {
            if (otherControl)
            {
                otherControl.SetRotatingObject(null);
            }

            SetRotatingObject(darObj);
            xObjRotation = Mathf.Clamp(0, rotatingObject.minX, rotatingObject.maxX);
            controlMode.EnterDualAxisRotationMode();
        }
    }

    public void SetRotatingObject(DualAxisRotatableObject darObj)
    {
        rotatingObject = darObj;
    }

    private void DoTouchpadAxisChanged(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        currentX = e.touchpadAxis.x;
        currentY = e.touchpadAxis.y;
    }

    private void DoTouchpadAxisChangedN()
    {
        currentY = 1.0f;
        currentX = 0.0f;
    }

    private void DoTouchpadAxisChangedS()
    {
        currentY = -1.0f;
        currentX = 0.0f;
    }

    private void DoTouchpadAxisChangedE()
    {
        currentX = 1.0f;
        currentY = 0.0f;
    }

    private void DoTouchpadAxisChangedW()
    {
        currentX = -1.0f;
        currentY = 0.0f;
    }

    private void DoTouchpadTouchStart()
    {
        isTouching = true;
    }

    private void DoTouchpadTouchEnd()
    {
        isTouching = false;
    }

    private void DoTouchpadTouchStart(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        isTouching = true;
    }

    private void DoTouchpadTouchEnd(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        isTouching = false;
    }

#region Helpers
    private float NormalizeAroundZero(float rawAngle)
    {
        if (rawAngle < 180 && rawAngle > -180)
        {
            return rawAngle;
        }
        else if (rawAngle > 180)
        {
            while (rawAngle > 180)
            {
                rawAngle -= 360;
            }
            return rawAngle;
        }
        else
        {
            while (rawAngle < -180)
            {
                rawAngle += 360;
            }
            return rawAngle;
        }
    }

    private DualAxisRotatableObject[] GetDualAxisRotatableObjects()
    {
        return FindObjectsOfType<DualAxisRotatableObject>();
    }
#endregion
}