// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    public class RigidbodyActionDeprecated : BaseActionDeprecated
    {
        public enum RigidbodyActionType
        {
            SwitchPhysicsState, SwitchGravityState, ChangeParent, ApplyForce, Unset
        };

        private RigidbodyActionType actionType = RigidbodyActionType.Unset;
        private Rigidbody rigidbodyOfInterest;
        private bool booleanValue;
        private Transform childTransform;
        private Transform parentTransform;
        private Vector3 forceValue;
        private ForceMode forceModeValue;

        public static RigidbodyActionDeprecated SwitchPhysicsStateAction(Rigidbody rigidbody, bool isOn)
        {
            return new RigidbodyActionDeprecated()
            {
                actionType = RigidbodyActionType.SwitchPhysicsState,
                rigidbodyOfInterest = rigidbody,
                booleanValue = isOn
            };
        }

        public static RigidbodyActionDeprecated SwitchGravityStateAction(Rigidbody rigidbody, bool isOn)
        {
            return new RigidbodyActionDeprecated()
            {
                actionType = RigidbodyActionType.SwitchPhysicsState,
                rigidbodyOfInterest = rigidbody,
                booleanValue = isOn
            };
        }

        public static RigidbodyActionDeprecated ChangeParentAction(Transform childObject, Transform parentObject)
        {
            return new RigidbodyActionDeprecated()
            {
                actionType = RigidbodyActionType.ChangeParent,
                childTransform = childObject,
                parentTransform = parentObject
            };
        }

        public static RigidbodyActionDeprecated ApplyForceAction(Rigidbody rigidbody, Vector3 force, ForceMode mode)
        {
            return new RigidbodyActionDeprecated()
            {
                actionType = RigidbodyActionType.ApplyForce,
                rigidbodyOfInterest = rigidbody,
                forceValue = force,
                forceModeValue = mode
            };
        }

        public static RigidbodyActionDeprecated ApplyForceAction(Rigidbody rigidbody, Vector3Type force, ActionTypeForceMode mode)
        {
            return new RigidbodyActionDeprecated()
            {
                actionType = RigidbodyActionType.ApplyForce,
                rigidbodyOfInterest = rigidbody,
                forceValue = DeserializeVector3_s(force),
                forceModeValue = DeserializeForceMode_s(mode)
            };
        }

        public void PerformAction()
        {
            switch (actionType)
            {
                case RigidbodyActionType.SwitchPhysicsState:
                    if (rigidbodyOfInterest != null)
                    {
                        rigidbodyOfInterest.isKinematic = !booleanValue;
                    }
                    break;

                case RigidbodyActionType.SwitchGravityState:
                    if (rigidbodyOfInterest != null)
                    {
                        if (rigidbodyOfInterest.isKinematic || !booleanValue)
                        {
                            rigidbodyOfInterest.useGravity = false;
                        }
                        else if (!rigidbodyOfInterest.isKinematic)
                        {
                            rigidbodyOfInterest.useGravity = true;
                        }
                    }
                    break;

                case RigidbodyActionType.ChangeParent:
                    if (childTransform != null && parentTransform != null)
                    {
                        childTransform.SetParent(parentTransform);
                    }
                    break;

                case RigidbodyActionType.ApplyForce:
                    if (rigidbodyOfInterest != null && forceValue != null)
                    {
                        rigidbodyOfInterest.AddForce(forceValue, forceModeValue);
                    }
                    break;

                case RigidbodyActionType.Unset:
                default:
                    break;
            }
        }

        public override ActionType Serialize()
        {
            ActionType sAction = new ActionType();

            switch (actionType)
            {
                case RigidbodyActionType.ApplyForce:
                    sAction.Type = ActionTypeType.ApplyForce;
                    break;

                case RigidbodyActionType.ChangeParent:
                    sAction.Type = ActionTypeType.ChangeParent;
                    break;

                case RigidbodyActionType.SwitchGravityState:
                    sAction.Type = ActionTypeType.SwitchGravityState;
                    break;

                case RigidbodyActionType.SwitchPhysicsState:
                    sAction.Type = ActionTypeType.SwitchPhysicsState;
                    break;

                case RigidbodyActionType.Unset:
                default:
                    sAction.Type = ActionTypeType.Unset;
                    break;
            }

            if (rigidbodyOfInterest) sAction.RigidbodyName = rigidbodyOfInterest.name;
            sAction.IsOn = booleanValue;
            if (childTransform) sAction.ChildObjectName = childTransform.name;
            if (parentTransform) sAction.ParentObjectName = parentTransform.name;
            sAction.Force = SerializeVector3_s(forceValue);
            sAction.ForceMode = SerializeForceMode_s(forceModeValue);

            return sAction;
        }
    }
}