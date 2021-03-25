// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class RigidbodyAction : BaseAction
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

    public static RigidbodyAction SwitchPhysicsStateAction(Rigidbody rigidbody, bool isOn)
    {
        return new RigidbodyAction()
        {
            actionType = RigidbodyActionType.SwitchPhysicsState,
            rigidbodyOfInterest = rigidbody,
            booleanValue = isOn
        };
    }

    public static RigidbodyAction SwitchGravityStateAction(Rigidbody rigidbody, bool isOn)
    {
        return new RigidbodyAction()
        {
            actionType = RigidbodyActionType.SwitchPhysicsState,
            rigidbodyOfInterest = rigidbody,
            booleanValue = isOn
        };
    }

    public static RigidbodyAction ChangeParentAction(Transform childObject, Transform parentObject)
    {
        return new RigidbodyAction()
        {
            actionType = RigidbodyActionType.ChangeParent,
            childTransform = childObject,
            parentTransform = parentObject
        };
    }

    public static RigidbodyAction ApplyForceAction(Rigidbody rigidbody, Vector3 force, ForceMode mode)
    {
        return new RigidbodyAction()
        {
            actionType = RigidbodyActionType.ApplyForce,
            rigidbodyOfInterest = rigidbody,
            forceValue = force,
            forceModeValue = mode
        };
    }

    public static RigidbodyAction ApplyForceAction(Rigidbody rigidbody, Vector3Type force, ActionTypeForceMode mode)
    {
        return new RigidbodyAction()
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