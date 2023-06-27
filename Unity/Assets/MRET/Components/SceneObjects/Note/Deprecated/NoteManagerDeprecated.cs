// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Note
{
    public class NoteManagerDeprecated : MonoBehaviour
    {
        public static readonly string notesKey = "MRET.INTERNAL.TOOLS.NOTES";

        public bool creatingNotes
        {
            set
            {
                MRET.DataManager.SaveValue(new DataManager.DataValue(notesKey, value));
            }
            get
            {
                return (bool)MRET.DataManager.FindPoint(notesKey);
            }
        }
        public static GameObject notePrefab;
        public GameObject _notePrefab;
        public GameObject leftController, rightController;
        public int noteCount = 0;

        private UndoManagerDeprecated undoManager;

        public void Initialize()
        {
            notePrefab = _notePrefab;
            undoManager = FindObjectOfType<UndoManagerDeprecated>();
            creatingNotes = false;
        }

        public void CreateNote(GameObject device)
        {
            if (creatingNotes)
            {
                Vector3 pos = device.transform.position;
                Quaternion rot = device.transform.rotation;
                NoteDeprecated n = NoteDeprecated.MakeNote(notePrefab, pos, rot, noteCount++);
                n.guid = System.Guid.NewGuid();
                n.Maximize();

                NoteType nType = new NoteType()
                {
                    GUID = n.guid.ToString(),
                    Title = "",
                    Details = "",
                    Drawings = new NoteDrawingsType()
                    {
                        ID = new int[0],
                        NoteDrawings = new NoteDrawingType[0]
                    },
                    State = NoteTypeState.Maximized,
                    Transform = new UnityTransformType()
                    {
                        Position = new Vector3Type()
                        {
                            X = pos.x,
                            Y = pos.y,
                            Z = pos.z
                        },
                        Rotation = new QuaternionType()
                        {
                            X = rot.x,
                            Y = rot.y,
                            Z = rot.z
                        },
                        Scale = new NonNegativeFloat3Type()
                        {
                            X = 1,
                            Y = 1,
                            Z = 1
                        }
                    }
                };
                undoManager.AddAction(
                    ProjectActionDeprecated.AddNoteAction(nType, "Note" + (noteCount - 1), pos, rot),
                    ProjectActionDeprecated.DeleteNoteAction("Note" + (noteCount - 1), nType.GUID));
            }
        }

#region NoteReinitialization
        private class NoteInitializationInfo
        {
            public GameObject noteObject;
            public bool needToDisable = false;
            public bool needToEnable = false;
            public int enableCountDown = 0;
        }

        private List<NoteInitializationInfo> notesToReInit = new List<NoteInitializationInfo>();

        public void ReinitializeNote(InteractableNoteDeprecated noteToReinitialize)
        {
            notesToReInit.Add(new NoteInitializationInfo()
            {
                noteObject = noteToReinitialize.gameObject,
                needToDisable = true,
                needToEnable = false,
                enableCountDown = 0
            });
        }

        void Update()
        {
            List<int> noteInfoToRemove = new List<int>();
            foreach (NoteInitializationInfo noteToReInit in notesToReInit)
            {
                if (noteToReInit.needToDisable)
                {
                    noteToReInit.noteObject.SetActive(false);
                    noteToReInit.needToDisable = false;
                    noteToReInit.needToEnable = true;
                }
                else if (noteToReInit.needToEnable)
                {
                    if (noteToReInit.enableCountDown-- < 0)
                    {
                        noteToReInit.noteObject.SetActive(true);
                        noteToReInit.needToEnable = false;
                    }
                }
            }

            foreach (int noteIndex in noteInfoToRemove)
            {
                notesToReInit.RemoveAt(noteIndex);
            }
        }
#endregion
    }
}