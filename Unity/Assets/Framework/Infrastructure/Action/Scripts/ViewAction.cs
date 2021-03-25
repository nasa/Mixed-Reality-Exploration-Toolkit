// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class ViewAction : BaseAction
{
    public enum ViewActionType
    {
        RotateEnvironment, ScaleEnvironment, MoveUser, Unset
    };

    public enum EulerAxis { X, Y, Z };

    private GameObject projectRoot;

    private ViewActionType actionType = ViewActionType.Unset;
    private string userNameOfInterest;
    private Vector3 axisOfInterest;
    private Vector3 pointOfInterest;
    private float angleValue;
    private Vector3 positionValue;
    private Quaternion rotationValue;
    private Vector3 scaleValue;
    private MRETUserManager userManager;

    public ViewAction()
    {
        userManager = MRETUserManager.Get();
    }

    public static ViewAction RotateEnvironmentAction(EulerAxis axis, Vector3 referencePoint, float rotationInDegrees)
    {
        return new ViewAction()
        {
            actionType = ViewActionType.RotateEnvironment,
            axisOfInterest = (axis == EulerAxis.X) ? Vector3.right :
            (axis == EulerAxis.Y) ? Vector3.up : Vector3.forward,
            pointOfInterest = referencePoint,
            angleValue = rotationInDegrees
        };
    }

    public static ViewAction RotateEnvironmentAction(ActionTypeAxis axis, Vector3Type referencePoint, float rotationInDegrees)
    {
        return new ViewAction()
        {
            actionType = ViewActionType.RotateEnvironment,
            axisOfInterest = (axis == ActionTypeAxis.X) ? Vector3.right :
            (axis == ActionTypeAxis.Y) ? Vector3.up : Vector3.forward,
            pointOfInterest = DeserializeVector3_s(referencePoint),
            angleValue = rotationInDegrees
        };
    }

    public static ViewAction ScaleEnvironmentAction(float scaleToApply)
    {
        return new ViewAction()
        {
            actionType = ViewActionType.ScaleEnvironment,
            scaleValue = new Vector3(scaleToApply, scaleToApply, scaleToApply)
        };
    }

    public static ViewAction MoveUserAction(string userName, Vector3 pos, Quaternion rot, Vector3 scl)
    {
        return new ViewAction()
        {
            actionType = ViewActionType.MoveUser,
            positionValue = pos,
            rotationValue = rot,
            scaleValue = scl
        };
    }

    public static ViewAction MoveUserAction(string userName, Vector3Type pos, QuaternionType rot, Vector3Type scl)
    {
        return new ViewAction()
        {
            actionType = ViewActionType.MoveUser,
            positionValue = DeserializeVector3_s(pos),
            rotationValue = DeserializeQuaternion_s(rot),
            scaleValue = DeserializeVector3_s(scl)
        };
    }

    public void PerformAction()
    {
        switch (actionType)
        {
            case ViewActionType.RotateEnvironment:
                if (pointOfInterest != null)
                {
                    projectRoot.transform.RotateAround(pointOfInterest, axisOfInterest, angleValue);
                }
                break;

            case ViewActionType.ScaleEnvironment:
                if (scaleValue != null)
                {
                    projectRoot.transform.localScale = scaleValue;
                }
                break;

            case ViewActionType.MoveUser:
                if (userNameOfInterest != null)
                {
                    MRETUser userToMove = userManager.mretUsers.Find(x => x.userName == userNameOfInterest);
                    if (userToMove != null)
                    {
                        if (positionValue != null)
                        {
                            userToMove.transform.position = positionValue;
                        }

                        if (rotationValue != null)
                        {
                            userToMove.transform.rotation = rotationValue;
                        }

                        if (scaleValue != null)
                        {
                            userToMove.transform.localScale = scaleValue;
                        }
                    }
                }
                break;

            case ViewActionType.Unset:
            default:
                break;
        }
    }

    void Start()
    {
        projectRoot = FindObjectOfType<UnityProject>().gameObject;
    }

    public override ActionType Serialize()
    {
        ActionType sAction = new ActionType();

        switch (actionType)
        {
            case ViewActionType.MoveUser:
                sAction.Type = ActionTypeType.ApplyForce;
                break;

            case ViewActionType.RotateEnvironment:
                sAction.Type = ActionTypeType.ChangeParent;
                break;

            case ViewActionType.ScaleEnvironment:
                sAction.Type = ActionTypeType.SwitchGravityState;
                break;

            case ViewActionType.Unset:
            default:
                sAction.Type = ActionTypeType.Unset;
                break;
        }

        sAction.UserName = userNameOfInterest;
        sAction.Axis = (axisOfInterest == Vector3.right) ? ActionTypeAxis.X :
            (axisOfInterest == Vector3.up) ? ActionTypeAxis.Y : ActionTypeAxis.Z;
        sAction.ReferencePoint = SerializeVector3_s(pointOfInterest);
        sAction.RotationInDegrees = angleValue;
        sAction.Position = SerializeVector3_s(positionValue);
        sAction.Rotation = SerializeQuaternion_s(rotationValue);
        sAction.Scale = SerializeVector3_s(scaleValue);

        return sAction;
    }
}