// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class BaseAction : MonoBehaviour
{
    public static BaseAction Deserialize(ActionType serializedAction)
    {
        GameObject foundObj;
        Rigidbody foundRBody;

        switch (serializedAction.Type)
        {
            case ActionTypeType.AddDrawing:
                return ProjectAction.AddDrawingAction(serializedAction.Drawing);

            case ActionTypeType.AddNote:
                return ProjectAction.AddNoteAction(serializedAction.Note, serializedAction.NoteName, serializedAction.Position, serializedAction.Rotation);

            case ActionTypeType.AddNoteDrawing:
                return ProjectAction.AddNoteDrawingAction(serializedAction.NoteName, serializedAction.NoteDrawingName, serializedAction.NoteDrawing);

            case ActionTypeType.AddObject:
                return ProjectAction.AddObjectAction(serializedAction.Part, serializedAction.Position, serializedAction.Rotation, serializedAction.Scale,
                    new InteractablePart.InteractablePartSettings(serializedAction.Part.EnableInteraction[0],
                    serializedAction.Part.EnableCollisions[0], serializedAction.Part.EnableGravity[0]));

            case ActionTypeType.AddPointToLine:
                return ProjectAction.AddPointToLine();

            case ActionTypeType.ApplyForce:
                foundObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.RigidbodyName);
                if (foundObj)
                {
                    foundRBody = foundObj.GetComponent<Rigidbody>();
                    if (foundRBody)
                    {
                        return RigidbodyAction.ApplyForceAction(foundRBody, serializedAction.Force, serializedAction.ForceMode);
                    }
                }
                return null;

            case ActionTypeType.ChangeNoteState:
                return ProjectAction.ChangeNoteState();

            case ActionTypeType.ChangeNoteText:
                return ProjectAction.ChangeNoteTextAction(serializedAction.NoteName, serializedAction.Title, serializedAction.Content);

            case ActionTypeType.ChangeParent:
                GameObject parentObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.ParentObjectName);
                if (parentObj)
                {
                    GameObject childObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.ChildObjectName);
                    if (childObj)
                    {
                        return RigidbodyAction.ChangeParentAction(childObj.transform, parentObj.transform);
                    }
                }
                return null;

            case ActionTypeType.DeleteDrawing:
                return ProjectAction.DeleteDrawingAction(serializedAction.DrawingName);

            case ActionTypeType.DeleteNote:
                return ProjectAction.DeleteNoteAction(serializedAction.NoteName);

            case ActionTypeType.DeleteNoteDrawing:
                return ProjectAction.DeleteNoteDrawingAction(serializedAction.NoteName, serializedAction.NoteDrawingName);

            case ActionTypeType.DeleteObject:
                return ProjectAction.DeleteObjectAction(serializedAction.PartName);

            case ActionTypeType.DeletePointFromLine:
                return ProjectAction.DeletePointFromLine();

            case ActionTypeType.MoveNote:
                return ProjectAction.MoveNoteAction(serializedAction.NoteName, serializedAction.Position, serializedAction.Rotation);

            case ActionTypeType.MoveObject:
                return ProjectAction.MoveObjectAction(serializedAction.PartName, serializedAction.Position, serializedAction.Rotation);

            case ActionTypeType.MoveUser:
                return ViewAction.MoveUserAction(serializedAction.UserName, serializedAction.Position, serializedAction.Rotation, serializedAction.Scale);

            case ActionTypeType.RotateEnvironment:
                return ViewAction.RotateEnvironmentAction(serializedAction.Axis, serializedAction.ReferencePoint, serializedAction.RotationInDegrees);

            case ActionTypeType.ScaleEnvironment:
                return ViewAction.ScaleEnvironmentAction(serializedAction.ScaleToApply);

            case ActionTypeType.SetFinalIKPos:
                return ProjectAction.SetFinalIKPosAction(serializedAction.PartName, serializedAction.Position);

            case ActionTypeType.SetMatlabIKPos:
                return ProjectAction.SetMatlabIKPosAction();

            case ActionTypeType.SwitchGravityState:
                foundObj = GameObject.Find("LoadedProjects/GameObjects/" + serializedAction.RigidbodyName);
                if (foundObj)
                {
                    foundRBody = foundObj.GetComponent<Rigidbody>();
                    if (foundRBody)
                    {
                        return RigidbodyAction.SwitchGravityStateAction(foundRBody, serializedAction.IsOn);
                    }
                }
                return null;

            case ActionTypeType.SwitchPhysicsState:
                foundObj = GameObject.Find("LoadedProjects/GameObjects/" + serializedAction.RigidbodyName);
                if (foundObj)
                {
                    foundRBody = foundObj.GetComponent<Rigidbody>();
                    if (foundRBody)
                    {
                        return RigidbodyAction.SwitchPhysicsStateAction(foundRBody, serializedAction.IsOn);
                    }
                }
                return null;

            case ActionTypeType.AnnotationGoTo:
                return AnnotationAction.AnnotationGoToAction(serializedAction.AnnotationType, serializedAction.PartName, serializedAction.Index);

            case ActionTypeType.EndAnnotation:
                return AnnotationAction.EndAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

            case ActionTypeType.PauseAnnotation:
                return AnnotationAction.PauseAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

            case ActionTypeType.PlayAnnotation:
                return AnnotationAction.PlayAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

            case ActionTypeType.StartAnnotation:
                return AnnotationAction.StartAnnotationAction(serializedAction.AnnotationType, serializedAction.FileName, serializedAction.PartName);

            case ActionTypeType.StopAnnotation:
                return AnnotationAction.StopAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

            case ActionTypeType.Unset:
            default:
                return null;
        }
    }

    public virtual ActionType Serialize()
    {
        return new ActionType();
    }

#region TypeSerialization
    protected static Vector3Type[] SerializeVector3Array_s(Vector3[] input)
    {
        Vector3Type[] outList = new Vector3Type[input.Length];

        for (int i = 0; i < input.Length; i++)
            foreach (Vector3 vec in input)
            {
                outList[i] = SerializeVector3_s(input[i]);
            }

        return outList;
    }

    protected static Vector3Type SerializeVector3_s(Vector3 input)
    {
        return new Vector3Type()
        {
            X = input.x,
            Y = input.y,
            Z = input.z
        };
    }

    protected static QuaternionType SerializeQuaternion_s(Quaternion input)
    {
        return new QuaternionType()
        {
            X = input.x,
            Y = input.y,
            Z = input.z,
            W = input.w
        };
    }

    protected static ActionTypeForceMode SerializeForceMode_s(ForceMode input)
    {
        switch (input)
        {
            case ForceMode.Acceleration:
                return ActionTypeForceMode.Acceleration;

            case ForceMode.Force:
                return ActionTypeForceMode.Force;

            case ForceMode.Impulse:
                return ActionTypeForceMode.Impulse;

            case ForceMode.VelocityChange:
                return ActionTypeForceMode.VelocityChange;

            default:
                return ActionTypeForceMode.Force;
        }
    }
#endregion

#region TypeDeserialization
    protected List<Vector3> DeserializeVector3ArrayToList(Vector3Type[] input)
    {
        List<Vector3> outVec = new List<Vector3>();

        foreach (Vector3Type vec in input)
        {
            outVec.Add(DeserializeVector3(vec));
        }

        return outVec;
    }

    protected Vector3[] DeserializeVector3Array(Vector3Type[] input)
    {
        Vector3[] outList = new Vector3[input.Length];

        for (int i = 0; i < input.Length; i++)
        foreach (Vector3Type vec in input)
        {
            outList[i] = DeserializeVector3(input[i]);
        }

        return outList;
    }

    protected Vector3 DeserializeVector3(Vector3Type input)
    {
        return new Vector3(input.X, input.Y, input.Z);
    }

    protected Quaternion DeserializeQuaternion(QuaternionType input)
    {
        return new Quaternion(input.X, input.Y, input.Z, input.W);
    }

    protected ForceMode DeserializeForceMode(ActionTypeForceMode input)
    {
        switch (input)
        {
            case ActionTypeForceMode.Acceleration:
                return ForceMode.Acceleration;

            case ActionTypeForceMode.Force:
                return ForceMode.Force;

            case ActionTypeForceMode.Impulse:
                return ForceMode.Impulse;

            case ActionTypeForceMode.VelocityChange:
                return ForceMode.VelocityChange;

            default:
                return ForceMode.Force;
        }
    }

    protected static List<Vector3> DeserializeVector3ArrayToList_s(Vector3Type[] input)
    {
        List<Vector3> outVec = new List<Vector3>();

        foreach (Vector3Type vec in input)
        {
            outVec.Add(DeserializeVector3_s(vec));
        }

        return outVec;
    }

    protected static Vector3[] DeserializeVector3Array_s(Vector3Type[] input)
    {
        Vector3[] outList = new Vector3[input.Length];

        for (int i = 0; i < input.Length; i++)
            foreach (Vector3Type vec in input)
            {
                outList[i] = DeserializeVector3_s(input[i]);
            }

        return outList;
    }

    protected static Vector3 DeserializeVector3_s(Vector3Type input)
    {
        return new Vector3(input.X, input.Y, input.Z);
    }

    protected static Quaternion DeserializeQuaternion_s(QuaternionType input)
    {
        return new Quaternion(input.X, input.Y, input.Z, input.W);
    }

    protected static ForceMode DeserializeForceMode_s(ActionTypeForceMode input)
    {
        switch (input)
        {
            case ActionTypeForceMode.Acceleration:
                return ForceMode.Acceleration;

            case ActionTypeForceMode.Force:
                return ForceMode.Force;

            case ActionTypeForceMode.Impulse:
                return ForceMode.Impulse;

            case ActionTypeForceMode.VelocityChange:
                return ForceMode.VelocityChange;

            default:
                return ForceMode.Force;
        }
    }
#endregion
}