// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Components.Notes;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Components.LineDrawing;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class ProjectAction : BaseAction
{
    public enum ProjectActionType
    {
        MoveObject, AddObject, DeleteObject, UpdateObjectSettings,
        SetParent,
        AddDrawing, DeleteDrawing, AddNote, DeleteNote, MoveNote,
        ChangeNoteText, ChangeNoteState,
        AddNoteDrawing, DeleteNoteDrawing,
        SetFinalIKPos, SetMatlabIKPos,
        AddPointToLine, DeletePointFromLine, Unset
    };

    private ProjectActionType actionType = ProjectActionType.Unset;
    private string partNameOfInterest;
    private string guidOfInterest;
    private PartType partOfInterest;
    private DrawingType drawingOfInterest;
    private string drawingNameOfInterest;
    private NoteType noteOfInterest;
    private string noteNameOfInterest;
    private Vector3[] noteDrawingOfInterest;
    private string noteDrawingNameOfInterest;
    private Vector3 positionValue;
    private Quaternion rotationValue;
    private Vector3 scaleValue;
    private string titleValue, contentValue;
    private InteractablePart.InteractablePartSettings partSettings;
    private string parentGUIDOfInterest;

    public static ProjectAction MoveObjectAction(string partName, Vector3 pos, Quaternion rot, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.MoveObject,
            partNameOfInterest = partName,
            positionValue = pos,
            rotationValue = rot,
            guidOfInterest = guid
        };
    }

    public static ProjectAction MoveObjectAction(string partName, Vector3Type pos, QuaternionType rot, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.MoveObject,
            partNameOfInterest = partName,
            positionValue = DeserializeVector3_s(pos),
            rotationValue = DeserializeQuaternion_s(rot),
            guidOfInterest = guid
        };
    }

    public static ProjectAction AddObjectAction(PartType part, Vector3 pos, Quaternion rot, Vector3 scl,
        InteractablePart.InteractablePartSettings settings, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddObject,
            partOfInterest = part,
            positionValue = pos,
            rotationValue = rot,
            scaleValue = scl,
            partSettings = settings,
            guidOfInterest = guid
        };
    }

    public static ProjectAction AddObjectAction(PartType part, Vector3Type pos, QuaternionType rot, Vector3Type scl,
        InteractablePart.InteractablePartSettings settings, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddObject,
            partOfInterest = part,
            positionValue = DeserializeVector3_s(pos),
            rotationValue = DeserializeQuaternion_s(rot),
            scaleValue = DeserializeVector3_s(scl),
            partSettings = settings,
            guidOfInterest = guid
        };
    }

    public static ProjectAction DeleteObjectAction(string partName, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.DeleteObject,
            partNameOfInterest = partName,
            guidOfInterest = guid
        };
    }

    public static ProjectAction UpdateObjectSettingsAction(string partName,
        InteractablePart.InteractablePartSettings settings, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.UpdateObjectSettings,
            partNameOfInterest = partName,
            partSettings = settings,
            guidOfInterest = guid
        };
    }

    public static ProjectAction SetParentAction(string partName,
        string parentGUID, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.SetParent,
            partNameOfInterest = partName,
            parentGUIDOfInterest = parentGUID,
            guidOfInterest = guid
        };
    }

    public static ProjectAction AddDrawingAction(DrawingType drawing)
    {
        //foreach (GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing ld in MRET.SceneObjectManager.lineDrawings)
        {
            //if (ld.uuid.ToString() == drawing.GUID)
            {
                return new ProjectAction()
                {
                    actionType = ProjectActionType.AddDrawing,
                    drawingOfInterest = drawing,
                    guidOfInterest = drawing.GUID
                };
            }
        }
    }

    public static ProjectAction AddDrawingAction(GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing drawing)
    {
        List<Vector3> pts = new List<Vector3>();
        foreach (Vector3 pt in drawing.points)
        {
            pts.Add(drawing.transform.TransformPoint(pt));
        }
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddDrawing,
            drawingOfInterest = new DrawingType()
            {
                DesiredUnits = LineDrawingUnitsType.meters,
                GUID = drawing.uuid.ToString(),
                Name = drawing.name,
                Points = SerializeVector3Array_s(pts.ToArray()),
                RenderType = drawing is VolumetricDrawing ? "cable" : "drawing",
                Width = drawing.GetWidth()
            },
            guidOfInterest = drawing.uuid.ToString()
        };
    }

    public static ProjectAction DeleteDrawingAction(string drawingName, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.DeleteDrawing,
            drawingNameOfInterest = drawingName,
            guidOfInterest = guid
        };
    }

    public static ProjectAction AddNoteAction(NoteType note, string noteName, Vector3 pos, Quaternion rot, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddNote,
            noteNameOfInterest = noteName,
            noteOfInterest = note,
            positionValue = pos,
            rotationValue = rot,
            guidOfInterest = guid
        };
    }

    public static ProjectAction AddNoteAction(NoteType note, string noteName, Vector3Type pos, QuaternionType rot)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddNote,
            noteNameOfInterest = noteName,
            noteOfInterest = note,
            positionValue = DeserializeVector3_s(pos),
            rotationValue = DeserializeQuaternion_s(rot)
        };
    }

    public static ProjectAction DeleteNoteAction(string noteName, string guid = null)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.DeleteNote,
            noteNameOfInterest = noteName,
            guidOfInterest = guid
        };
    }

    public static ProjectAction MoveNoteAction(string noteName, Vector3 pos, Quaternion rot)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.MoveNote,
            noteNameOfInterest = noteName,
            positionValue = pos,
            rotationValue = rot
        };
    }

    public static ProjectAction MoveNoteAction(string noteName, Vector3Type pos, QuaternionType rot)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.MoveNote,
            noteNameOfInterest = noteName,
            positionValue = DeserializeVector3_s(pos),
            rotationValue = DeserializeQuaternion_s(rot)
        };
    }

    public static ProjectAction ChangeNoteTextAction(string noteName, string title, string content)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.ChangeNoteText,
            noteNameOfInterest = noteName,
            titleValue = title,
            contentValue = content
        };
    }

    public static ProjectAction AddNoteDrawingAction(string noteName, string noteDrawingName, Vector3[] noteDrawing)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddNoteDrawing,
            noteNameOfInterest = noteName,
            noteDrawingNameOfInterest = noteDrawingName,
            noteDrawingOfInterest = noteDrawing
        };
    }

    public static ProjectAction AddNoteDrawingAction(string noteName, string noteDrawingName, Vector3Type[] noteDrawing)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddNoteDrawing,
            noteNameOfInterest = noteName,
            noteDrawingNameOfInterest = noteDrawingName,
            noteDrawingOfInterest = DeserializeVector3Array_s(noteDrawing)
        };
    }

    public static ProjectAction DeleteNoteDrawingAction(string noteName, string noteDrawingName)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.DeleteNoteDrawing,
            noteNameOfInterest = noteName,
            noteDrawingNameOfInterest = noteDrawingName
        };
    }

    public static ProjectAction SetFinalIKPosAction(string partName, Vector3 pos)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.SetFinalIKPos,
            partNameOfInterest = partName,
            positionValue = pos
        };
    }

    public static ProjectAction SetFinalIKPosAction(string partName, Vector3Type pos)
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.SetFinalIKPos,
            partNameOfInterest = partName,
            positionValue = DeserializeVector3_s(pos)
        };
    }

    public static ProjectAction SetMatlabIKPosAction()
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.SetMatlabIKPos
        };
    }

    public static ProjectAction ChangeNoteState()
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.ChangeNoteState
        };
    }

    public static ProjectAction AddPointToLine()
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.AddPointToLine
        };
    }

    public static ProjectAction DeletePointFromLine()
    {
        return new ProjectAction()
        {
            actionType = ProjectActionType.DeletePointFromLine
        };
    }

    public void PerformAction(bool allowDuplicates = false)
    {
        switch (actionType)
        {
            case ProjectActionType.MoveObject:
                if (partNameOfInterest != null && positionValue != null && rotationValue != null)
                {
                    InteractablePart iPart = null;
                    if (!string.IsNullOrEmpty(guidOfInterest))
                    {
                        iPart = UnityProject.instance.GetPartByUUID(Guid.Parse(guidOfInterest));
                    }
                    else
                    {
                        GameObject part = GameObject.Find("LoadedProject/GameObjects/" + partNameOfInterest);
                        if (part)
                        {
                            iPart = part.GetComponent<InteractablePart>();
                        }
                    }

                    if (iPart)
                    {
                        iPart.transform.position = positionValue;
                        iPart.transform.rotation = rotationValue;
                    }
                }
                break;

            case ProjectActionType.AddObject:
                if (partOfInterest != null
                    && positionValue != null && rotationValue != null && scaleValue != null && partSettings != null)
                {
                    GameObject part = GameObject.Find("LoadedProject/GameObjects/" + partOfInterest.Name);

                    // Check for duplicates.
                    if (!allowDuplicates && part)
                    {
                        InteractablePart iPart = part.GetComponent<InteractablePart>();
                        if (iPart)
                        {
                            if (iPart.guid.ToString() == partOfInterest.GUID)
                            {
                                part.transform.position = new Vector3(positionValue.x, positionValue.y, positionValue.z);
                                break;
                            }
                        }
                    }

                    // Otherwise, instantiate new part.
                    partOfInterest.PartTransform = new UnityTransformType();

                    partOfInterest.PartTransform.Position = new Vector3Type()
                    {
                        X = positionValue.x,
                        Y = positionValue.y,
                        Z = positionValue.z
                    };

                    partOfInterest.PartTransform.Rotation = new QuaternionType()
                    {
                        X = rotationValue.x,
                        Y = rotationValue.y,
                        Z = rotationValue.z,
                        W = rotationValue.w
                    };

                    partOfInterest.PartTransform.Scale = new Vector3Type()
                    {
                        X = scaleValue.x,
                        Y = scaleValue.y,
                        Z = scaleValue.z
                    };

                    partOfInterest.EnableInteraction = new bool[] { partSettings.interactionEnabled };
                    partOfInterest.EnableCollisions = new bool[] { partSettings.collisionEnabled };
                    partOfInterest.EnableGravity = new bool[] { partSettings.gravityEnabled };
                    partOfInterest.ChildParts = new PartsType(); // TODO.
                    TimeStampUtility.LogTime("PerformAction 7");
                    PartManager pman = FindObjectOfType<PartManager>();
                    TimeStampUtility.LogTime("PerformAction 8");
                    if (pman)
                    {
                        pman.InstantiatePartInEnvironment(partOfInterest, null);
                        TimeStampUtility.LogTime("PerformAction 9");
                    }
                }
                break;

            case ProjectActionType.DeleteObject:
                if (partNameOfInterest != null)
                {
                    InteractablePart iPart = null;
                    if (!string.IsNullOrEmpty(guidOfInterest))
                    {
                        iPart = UnityProject.instance.GetPartByUUID(Guid.Parse(guidOfInterest));
                    }
                    else
                    {
                        GameObject part = GameObject.Find("LoadedProject/GameObjects/" + partNameOfInterest);
                        if (part)
                        {
                            iPart = part.GetComponent<InteractablePart>();
                        }
                    }

                    if (iPart)
                    {
                        Destroy(iPart.gameObject);
                    }
                }
                break;

            case ProjectActionType.UpdateObjectSettings:
                if (partOfInterest != null && partSettings != null)
                {
                    InteractablePart iPart = null;
                    Rigidbody rBody = null;
                    if (!string.IsNullOrEmpty(guidOfInterest))
                    {
                        iPart = UnityProject.instance.GetPartByUUID(Guid.Parse(guidOfInterest));
                    }
                    else
                    {
                        GameObject part = GameObject.Find("LoadedProject/GameObjects/" + partNameOfInterest);
                        if (part)
                        {
                            iPart = part.GetComponent<InteractablePart>();
                            rBody = part.GetComponent<Rigidbody>();
                        }
                    }

                    if (iPart && rBody)
                    {
                        iPart.grabbable = partSettings.interactionEnabled;
                        rBody.isKinematic = !partSettings.collisionEnabled;
                        rBody.useGravity = partSettings.gravityEnabled;
                    }
                }
                break;

            case ProjectActionType.SetParent:
                if (!string.IsNullOrEmpty(guidOfInterest) && !string.IsNullOrEmpty(parentGUIDOfInterest))
                {
                    InteractablePart iPart = null;
                    if (!string.IsNullOrEmpty(guidOfInterest))
                    {
                        iPart = UnityProject.instance.GetPartByUUID(Guid.Parse(guidOfInterest));
                    }
                    else
                    {
                        GameObject part = GameObject.Find("LoadedProject/GameObjects/" + partNameOfInterest);
                        if (part)
                        {
                            iPart = part.GetComponent<InteractablePart>();
                        }
                    }

                    // Don't continue if interactable part wasn't found.
                    if (iPart == null)
                    {
                        break;
                    }

                    // If parent is root.
                    if (parentGUIDOfInterest == "ROOT")
                    {
                        iPart.transform.SetParent(UnityProject.instance.projectObjectContainer.transform);
                        break;
                    }

                    // If parent is an InteractablePart.
                    InteractablePart parentIPart = UnityProject.instance.GetPartByUUID(Guid.Parse(parentGUIDOfInterest));
                    if (parentIPart)
                    {
                        iPart.transform.SetParent(parentIPart.transform);
                        break;
                    }

                    // If parent is a user.
                    SynchronizedUser parentUser = UnityProject.instance.GetUserByUUID(Guid.Parse(parentGUIDOfInterest));
                    if (parentUser)
                    {
                        iPart.transform.SetParent(parentUser.transform);
                        break;
                    }

                    // If parent is a controller.
                    SynchronizedController parentController = UnityProject.instance.GetControllerByUUID(Guid.Parse(parentGUIDOfInterest));
                    if (parentController)
                    {
                        iPart.transform.SetParent(parentController.transform);
                        break;
                    }
                }
                break;

            case ProjectActionType.AddDrawing:
                if (drawingOfInterest != null)
                {
                    /*DrawLineManager dlm = FindObjectOfType<DrawLineManager>();
                    if (dlm)
                    {
                        GameObject drawing = GameObject.Find("LoadedProject/Drawings/" + drawingOfInterest.Name);
                        UnityProject proj = FindObjectOfType<UnityProject>();

                        // Check for duplicates.
                        if (proj != null && !allowDuplicates && drawing != null)
                        {
                            if (drawing.transform.parent == proj.projectDrawingContainer.transform)
                            {
                                break;
                            }
                        }

                        dlm.AddPredefinedDrawing(DeserializeVector3ArrayToList(drawingOfInterest.Points),
                            (LineDrawing.RenderTypes)Enum.Parse(typeof(LineDrawing.RenderTypes),
                            drawingOfInterest.RenderType.ToString()), (LineDrawing.unit)Enum.Parse(typeof(LineDrawing.unit),
                            drawingOfInterest.DesiredUnits.ToString()), drawingOfInterest.Name, new Guid(drawingOfInterest.GUID));
                    }*/
                    //GameObject drawing = GameObject.Find("LoadedProject/Parts/LineDrawing-" + drawingOfInterest.Name);
                    //    UnityProject proj = FindObjectOfType<UnityProject>();

                    GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing drawing = null;
                    foreach (SceneObject so in FindObjectsOfType<SceneObject>())
                    {
                        if (so.uuid == Guid.Parse(guidOfInterest))
                        {
                            if (so is GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing)
                            {
                                drawing = (GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing) so;
                            }
                        }
                    }

                    // Check for duplicates.
                    if (MRET.Project != null && !allowDuplicates && drawing != null)
                    {
                        if (drawing.transform.parent == MRET.Project.projectDrawingContainer.transform)
                        {
                            break;
                        }
                    }

                    MRET.SceneObjectManager.CreateLineDrawing(drawingOfInterest.Name, null,
                        Vector3.zero, Quaternion.identity, Vector3.one,
                        drawingOfInterest.RenderType == "cable" ?
                        GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Volumetric :
                        GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Basic,
                        drawingOfInterest.Width, Color.green, DeserializeVector3Array_s(drawingOfInterest.Points),
                        Guid.Parse(drawingOfInterest.GUID));
                }
                break;

            case ProjectActionType.DeleteDrawing:
                if (guidOfInterest != null)
                {
                    Guid guidToCheck = Guid.NewGuid();
                    if (Guid.TryParse(guidOfInterest, out guidToCheck))
                    {
                        /*foreach (LineDrawing drw in UnityProject.instance.projectDrawingContainer.GetComponentsInChildren<LineDrawing>())
                        {
                            if (drw.guid == guidToCheck)
                            {
                                Destroy(drw.meshModel.gameObject);
                            }
                        }*/
                        foreach (GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing drawing in
                            FindObjectsOfType<GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing>())
                        {
                            if (drawing.uuid == guidToCheck)
                            {
                                MRET.SceneObjectManager.DestroySceneObject(guidToCheck);
                            }
                        }
                    }
                }
                if (drawingNameOfInterest != null)
                {
                    GameObject line = GameObject.Find("LoadedProject/Drawings/" + drawingNameOfInterest);
                    if (line)
                    {
                        Destroy(line);
                    }
                }
                break;

            case ProjectActionType.AddNote:
                if (noteOfInterest != null && positionValue != null && rotationValue != null)
                {
                    // Check for duplicates.
                    bool duplicateFound = false;
                    if (!allowDuplicates)
                    {
                        foreach (Transform noteObj in GameObject.Find("LoadedProject/Notes/").transform)
                        {
                            Note note = noteObj.GetComponent<Note>();
                            if (note != null)
                            {
                                if (note.guid.ToString() == noteOfInterest.GUID)
                                {
                                    note.transform.position = positionValue;
                                    note.transform.rotation = rotationValue;
                                    note.textBeingAutoChanged = true;
                                    note.titleText.text = "";
                                    note.titleText.text = titleValue;
                                    note.informationText.text = "";
                                    note.informationText.text = contentValue;
                                    note.textBeingAutoChanged = false;

                                    foreach (Transform nDraw in note.GetComponent<Note>().drawingContainer.transform)
                                    {
                                        Destroy(nDraw.gameObject);
                                    }
                                    duplicateFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    // Otherwise, add new 
                    if (!duplicateFound)
                    {
                        NoteManager noteMan = FindObjectOfType<NoteManager>();
                        Note.NoteData noteData = new Note.NoteData()
                        {
                            pos = new Vector3(positionValue.x, positionValue.y, positionValue.z),
                            rot = new Quaternion(rotationValue.x, rotationValue.y, rotationValue.z, rotationValue.w),
                            title = noteOfInterest.Title,
                            information = noteOfInterest.Details
                        };

                        // DZB quick hack this needs to be refactored.
                        if (noteOfInterest.Drawings == null)
                        {
                            noteOfInterest.Drawings = new NoteDrawingsType();
                            noteOfInterest.Drawings.NoteDrawings = new NoteDrawingType[0];
                        }

                        Note.fromSerializable(noteData, noteOfInterest.Drawings.NoteDrawings, noteNameOfInterest, new Guid(guidOfInterest));
                    }
                }
                break;

            case ProjectActionType.DeleteNote:
                if (noteNameOfInterest != null)
                {
                    GameObject note = GameObject.Find("LoadedProject/Notes/" + noteNameOfInterest);
                    if (note)
                    {
                        Destroy(note);
                    }
                }
                break;

            case ProjectActionType.MoveNote:
                if (noteNameOfInterest != null && positionValue != null && rotationValue != null)
                {
                    GameObject obj = GameObject.Find("LoadedProject/Notes/" + noteNameOfInterest);
                    if (obj)
                    {
                        if (obj.GetComponent<Note>())
                        {
                            obj.transform.position = positionValue;
                            obj.transform.rotation = rotationValue;
                        }
                    }
                }
                break;

            case ProjectActionType.ChangeNoteText:
                if (noteNameOfInterest != null && titleValue != null && contentValue != null)
                {
                    GameObject obj = GameObject.Find("LoadedProject/Notes/" + noteNameOfInterest);
                    if (obj)
                    {
                        Note noteToEdit = obj.GetComponent<Note>();
                        if (noteToEdit)
                        {
                            noteToEdit.textBeingAutoChanged = true;
                            noteToEdit.titleText.text = titleValue;
                            noteToEdit.informationText.text = contentValue;
                            noteToEdit.textBeingAutoChanged = false;
                        }
                    }
                }
                break;

            case ProjectActionType.AddNoteDrawing:
                if (noteNameOfInterest != null && noteDrawingNameOfInterest != null && noteDrawingOfInterest != null)
                {
                    GameObject obj = GameObject.Find("LoadedProject/Notes/" + noteNameOfInterest);
                    if (obj)
                    {
                        Note noteToEdit = obj.GetComponent<Note>();
                        if (noteToEdit)
                        {
                            GameObject draw = new GameObject(noteDrawingNameOfInterest);
                            draw.transform.SetParent(noteToEdit.drawingContainer.transform);
                            LineRenderer lineRend = draw.AddComponent<LineRenderer>();
                            lineRend.widthMultiplier = 0.0025f;
                            lineRend.colorGradient = noteToEdit.lineColor;
                            lineRend.useWorldSpace = false;
                            lineRend.positionCount = 0;

                            foreach (Vector3 point in noteDrawingOfInterest)
                            {
                                lineRend.positionCount++;
                                lineRend.SetPosition(lineRend.positionCount - 1, point);
                            }

                            // TODO: Move this to the Note.
                            //FindObjectOfType<UndoManager>().AddAction(AddNoteDrawingAction(noteNameOfInterest, noteDrawingNameOfInterest, noteDrawingOfInterest),
                            //    DeleteNoteDrawingAction(noteNameOfInterest, noteDrawingNameOfInterest));
                        }
                    }
                }
                break;

            case ProjectActionType.DeleteNoteDrawing:
                if (noteNameOfInterest != null && noteDrawingNameOfInterest != null)
                {
                    GameObject note = GameObject.Find("LoadedProject/Notes/" + noteNameOfInterest);
                    if (note)
                    {
                        Note noteScript = note.GetComponent<Note>();
                        if (noteScript)
                        {
                            Transform noteDrawing = noteScript.drawingContainer.transform.Find(noteDrawingNameOfInterest);
                            if (noteDrawing)
                            {
                                Destroy(noteDrawing.gameObject);
                            }
                        }
                    }
                }
                break;

            case ProjectActionType.SetFinalIKPos:
                if (partNameOfInterest != null && positionValue != null)
                {
                    GameObject part = GameObject.Find("LoadedProject/GameObjects/" + partNameOfInterest);
                    if (part)
                    {
                        RootMotion.FinalIK.CCDIK finalIKScript = part.GetComponentInChildren<RootMotion.FinalIK.CCDIK>(true);
                        if (finalIKScript)
                        {
                            finalIKScript.solver.SetIKPosition(positionValue);
                            if (!finalIKScript.enabled)
                            {
                                IKInteractionManager ikMan = FindObjectOfType<IKInteractionManager>();
                                if (ikMan)
                                {
                                    ikMan.ToggleFinalIKScript(finalIKScript);
                                }
                            }
                        }
                    }
                }
                break;

            case ProjectActionType.SetMatlabIKPos:
                break;

            case ProjectActionType.Unset:
            default:
                break;
        }
    }

    public override ActionType Serialize()
    {
        ActionType sAction = new ActionType();

        switch (actionType)
        {
            case ProjectActionType.AddDrawing:
                sAction.Type = ActionTypeType.AddDrawing;
                break;

            case ProjectActionType.AddNote:
                sAction.Type = ActionTypeType.AddNote;
                break;

            case ProjectActionType.AddNoteDrawing:
                sAction.Type = ActionTypeType.AddNoteDrawing;
                break;

            case ProjectActionType.AddObject:
                sAction.Type = ActionTypeType.AddObject;
                break;

            case ProjectActionType.AddPointToLine:
                sAction.Type = ActionTypeType.AddPointToLine;
                break;

            case ProjectActionType.ChangeNoteState:
                sAction.Type = ActionTypeType.ChangeNoteState;
                break;

            case ProjectActionType.ChangeNoteText:
                sAction.Type = ActionTypeType.ChangeNoteText;
                break;

            case ProjectActionType.DeleteDrawing:
                sAction.Type = ActionTypeType.DeleteDrawing;
                break;

            case ProjectActionType.DeleteNote:
                sAction.Type = ActionTypeType.DeleteNote;
                break;

            case ProjectActionType.DeleteNoteDrawing:
                sAction.Type = ActionTypeType.DeleteNoteDrawing;
                break;

            case ProjectActionType.DeleteObject:
                sAction.Type = ActionTypeType.DeleteObject;
                break;

            case ProjectActionType.DeletePointFromLine:
                sAction.Type = ActionTypeType.DeletePointFromLine;
                break;

            case ProjectActionType.MoveNote:
                sAction.Type = ActionTypeType.MoveNote;
                break;

            case ProjectActionType.MoveObject:
                sAction.Type = ActionTypeType.MoveObject;
                break;

            case ProjectActionType.SetFinalIKPos:
                sAction.Type = ActionTypeType.SetFinalIKPos;
                break;

            case ProjectActionType.SetMatlabIKPos:
                sAction.Type = ActionTypeType.SetMatlabIKPos;
                break;

            case ProjectActionType.Unset:
            default:
                sAction.Type = ActionTypeType.Unset;
                break;
        }

        sAction.PartName = partNameOfInterest;
        sAction.Part = partOfInterest;

        // If partOfInterest is not set, try to find it.
        if (sAction.Part == null)
        {
            if (!string.IsNullOrEmpty(guidOfInterest))
            {
                InteractablePart iPart = null;
                iPart = UnityProject.instance.GetPartByUUID(Guid.Parse(guidOfInterest));
                if (iPart)
                {
                    sAction.Part = iPart.Serialize();
                }
            }
        }

        if (sAction.Part != null && partSettings != null)
        {
            sAction.Part.EnableInteraction = new bool[] { partSettings.interactionEnabled };
            sAction.Part.EnableCollisions = new bool[] { partSettings.collisionEnabled };
            sAction.Part.EnableGravity = new bool[] { partSettings.gravityEnabled };
        }
        sAction.Drawing = drawingOfInterest;
        sAction.DrawingName = drawingNameOfInterest;
        sAction.Note = noteOfInterest;
        sAction.NoteName = noteNameOfInterest;
        if (noteDrawingOfInterest != null) sAction.NoteDrawing = SerializeVector3Array_s(noteDrawingOfInterest);
        sAction.NoteDrawingName = noteDrawingNameOfInterest;
        sAction.Position = SerializeVector3_s(positionValue);
        sAction.Rotation = SerializeQuaternion_s(rotationValue);
        sAction.Scale = SerializeVector3_s(scaleValue);
        sAction.Title = titleValue;
        sAction.Content = contentValue;
        sAction.UUID = guidOfInterest;

        return sAction;
    }
}