// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Action.IAction))]
    public class BaseActionDeprecated : MonoBehaviour
    {
        public static BaseActionDeprecated Deserialize(ActionType serializedAction)
        {
            GameObject foundObj;
            Rigidbody foundRBody;

            switch (serializedAction.Type)
            {
                case ActionTypeType.AddDrawing:
                    return ProjectActionDeprecated.AddDrawingAction(serializedAction.Drawing);

                case ActionTypeType.AddNote:
                    return ProjectActionDeprecated.AddNoteAction(serializedAction.Note, serializedAction.NoteName, serializedAction.Position, serializedAction.Rotation);

                case ActionTypeType.AddNoteDrawing:
                    return ProjectActionDeprecated.AddNoteDrawingAction(serializedAction.NoteName, serializedAction.NoteDrawingName, serializedAction.NoteDrawing);

                case ActionTypeType.AddObject:
                    return ProjectActionDeprecated.AddObjectAction(serializedAction.Part, serializedAction.Position, serializedAction.Rotation, serializedAction.Scale,
                        new InteractablePartDeprecated.InteractablePartSettings(serializedAction.Part.EnableInteraction,
                        serializedAction.Part.EnableCollisions, serializedAction.Part.EnableGravity));

                case ActionTypeType.AddPointToLine:
                    return ProjectActionDeprecated.AddPointToLine();

                case ActionTypeType.ApplyForce:
                    foundObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.RigidbodyName);
                    if (foundObj)
                    {
                        foundRBody = foundObj.GetComponent<Rigidbody>();
                        if (foundRBody)
                        {
                            return RigidbodyActionDeprecated.ApplyForceAction(foundRBody, serializedAction.Force, serializedAction.ForceMode);
                        }
                    }
                    return null;

                case ActionTypeType.ChangeNoteState:
                    return ProjectActionDeprecated.ChangeNoteState();

                case ActionTypeType.ChangeNoteText:
                    return ProjectActionDeprecated.ChangeNoteTextAction(serializedAction.NoteName, serializedAction.Title, serializedAction.Content);

                case ActionTypeType.ChangeParent:
                    GameObject parentObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.ParentObjectName);
                    if (parentObj)
                    {
                        GameObject childObj = GameObject.Find("LoadedProject/GameObjects/" + serializedAction.ChildObjectName);
                        if (childObj)
                        {
                            return RigidbodyActionDeprecated.ChangeParentAction(childObj.transform, parentObj.transform);
                        }
                    }
                    return null;

                case ActionTypeType.DeleteDrawing:
                    return ProjectActionDeprecated.DeleteDrawingAction(serializedAction.DrawingName);

                case ActionTypeType.DeleteNote:
                    return ProjectActionDeprecated.DeleteNoteAction(serializedAction.NoteName);

                case ActionTypeType.DeleteNoteDrawing:
                    return ProjectActionDeprecated.DeleteNoteDrawingAction(serializedAction.NoteName, serializedAction.NoteDrawingName);

                case ActionTypeType.DeleteObject:
                    return ProjectActionDeprecated.DeleteObjectAction(serializedAction.PartName);

                case ActionTypeType.DeletePointFromLine:
                    return ProjectActionDeprecated.DeletePointFromLine();

                case ActionTypeType.MoveNote:
                    return ProjectActionDeprecated.MoveNoteAction(serializedAction.NoteName, serializedAction.Position, serializedAction.Rotation);

                case ActionTypeType.MoveObject:
                    return ProjectActionDeprecated.MoveObjectAction(serializedAction.PartName, serializedAction.Position, serializedAction.Rotation);

                case ActionTypeType.MoveUser:
                    return ViewActionDeprecated.MoveUserAction(serializedAction.UserName, serializedAction.Position, serializedAction.Rotation, serializedAction.Scale);

                case ActionTypeType.RotateEnvironment:
                    return ViewActionDeprecated.RotateEnvironmentAction(serializedAction.Axis, serializedAction.ReferencePoint, serializedAction.RotationInDegrees);

                case ActionTypeType.ScaleEnvironment:
                    return ViewActionDeprecated.ScaleEnvironmentAction(serializedAction.ScaleToApply);

                case ActionTypeType.SetFinalIKPos:
                    return ProjectActionDeprecated.SetFinalIKPosAction(serializedAction.PartName, serializedAction.Position);

                case ActionTypeType.SetMatlabIKPos:
                    return ProjectActionDeprecated.SetMatlabIKPosAction();

                case ActionTypeType.SwitchGravityState:
                    foundObj = GameObject.Find("LoadedProjects/GameObjects/" + serializedAction.RigidbodyName);
                    if (foundObj)
                    {
                        foundRBody = foundObj.GetComponent<Rigidbody>();
                        if (foundRBody)
                        {
                            return RigidbodyActionDeprecated.SwitchGravityStateAction(foundRBody, serializedAction.IsOn);
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
                            return RigidbodyActionDeprecated.SwitchPhysicsStateAction(foundRBody, serializedAction.IsOn);
                        }
                    }
                    return null;

                case ActionTypeType.AnnotationGoTo:
                    return AnnotationActionDeprecated.AnnotationGoToAction(serializedAction.AnnotationType, serializedAction.PartName, serializedAction.Index);

                case ActionTypeType.EndAnnotation:
                    return AnnotationActionDeprecated.EndAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

                case ActionTypeType.PauseAnnotation:
                    return AnnotationActionDeprecated.PauseAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

                case ActionTypeType.PlayAnnotation:
                    return AnnotationActionDeprecated.PlayAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

                case ActionTypeType.StartAnnotation:
                    return AnnotationActionDeprecated.StartAnnotationAction(serializedAction.AnnotationType, serializedAction.FileName, serializedAction.PartName);

                case ActionTypeType.StopAnnotation:
                    return AnnotationActionDeprecated.StopAnnotationAction(serializedAction.AnnotationType, serializedAction.PartName);

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

        protected static NonNegativeFloat3Type SerializeNonNegativeFloat3_s(Vector3 input)
        {
            return new NonNegativeFloat3Type()
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

        protected static Vector3 DeserializeNonNegativeFloat3_s(NonNegativeFloat3Type input)
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
}