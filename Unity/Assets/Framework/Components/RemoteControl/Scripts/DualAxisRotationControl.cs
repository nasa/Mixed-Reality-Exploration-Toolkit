// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

public class DualAxisRotationControl : MonoBehaviour
{
    public InputHand hand;
    public DualAxisRotatableObject rotatingObject;
    public DualAxisRotationControl otherControl;

    private float currentX, currentY;
    private float xObjRotation;
    private bool isTouching = false;

    void FixedUpdate()
    {
        if (isTouching && rotatingObject)
        {
            currentX = hand.navigateValue.x;
            currentY = hand.navigateValue.y;
            Debug.Log(currentX + " " + currentY);
            if (Math.Abs(currentX) > 0.3f)
            {
                Debug.Log("1");
                if (rotatingObject.horizontalObject != null)
                {
                    Debug.Log("2");
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
                    {
                        Debug.Log("3");
                        // Rotate Object.
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
            MRET.ControlMode.DisableAllControlTypes();
        }
        else
        {
            if (otherControl)
            {
                otherControl.SetRotatingObject(null);
            }

            SetRotatingObject(darObj);
            xObjRotation = Mathf.Clamp(0, rotatingObject.minX, rotatingObject.maxX);
            MRET.ControlMode.EnterDualAxisRotationMode();
        }
    }

    public void SetRotatingObject(DualAxisRotatableObject darObj)
    {
        rotatingObject = darObj;
    }

    private void DoTouchpadAxisChanged(Vector2 touchpadAxis)
    {
        currentX = touchpadAxis.x;
        currentY = touchpadAxis.y;
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

    public void DoTouchpadTouchStart()
    {
        isTouching = true;
    }

    public void DoTouchpadTouchEnd()
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