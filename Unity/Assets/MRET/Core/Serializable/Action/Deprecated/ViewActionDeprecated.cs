// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Action.IAction))]
    public class ViewActionDeprecated : BaseActionDeprecated
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

        public ViewActionDeprecated()
        {
        }

        public static ViewActionDeprecated RotateEnvironmentAction(EulerAxis axis, Vector3 referencePoint, float rotationInDegrees)
        {
            return new ViewActionDeprecated()
            {
                actionType = ViewActionType.RotateEnvironment,
                axisOfInterest = (axis == EulerAxis.X) ? Vector3.right :
                (axis == EulerAxis.Y) ? Vector3.up : Vector3.forward,
                pointOfInterest = referencePoint,
                angleValue = rotationInDegrees
            };
        }

        public static ViewActionDeprecated RotateEnvironmentAction(ActionTypeAxis axis, Vector3Type referencePoint, float rotationInDegrees)
        {
            return new ViewActionDeprecated()
            {
                actionType = ViewActionType.RotateEnvironment,
                axisOfInterest = (axis == ActionTypeAxis.X) ? Vector3.right :
                (axis == ActionTypeAxis.Y) ? Vector3.up : Vector3.forward,
                pointOfInterest = DeserializeVector3_s(referencePoint),
                angleValue = rotationInDegrees
            };
        }

        public static ViewActionDeprecated ScaleEnvironmentAction(float scaleToApply)
        {
            return new ViewActionDeprecated()
            {
                actionType = ViewActionType.ScaleEnvironment,
                scaleValue = new Vector3(scaleToApply, scaleToApply, scaleToApply)
            };
        }

        public static ViewActionDeprecated MoveUserAction(string userName, Vector3 pos, Quaternion rot, Vector3 scl)
        {
            return new ViewActionDeprecated()
            {
                actionType = ViewActionType.MoveUser,
                positionValue = pos,
                rotationValue = rot,
                scaleValue = scl
            };
        }

        public static ViewActionDeprecated MoveUserAction(string userName, Vector3Type pos, QuaternionType rot, NonNegativeFloat3Type scl)
        {
            return new ViewActionDeprecated()
            {
                actionType = ViewActionType.MoveUser,
                positionValue = DeserializeVector3_s(pos),
                rotationValue = DeserializeQuaternion_s(rot),
                scaleValue = DeserializeNonNegativeFloat3_s(scl)
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
                        User userToMove = new List<User>(MRET.UuidRegistry.RegisteredTypes<User>()).Find(x => x.name == userNameOfInterest);
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
            projectRoot = FindObjectOfType<UnityProjectDeprecated>().gameObject;
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
            sAction.Scale = SerializeNonNegativeFloat3_s(scaleValue);

            return sAction;
        }
    }
}