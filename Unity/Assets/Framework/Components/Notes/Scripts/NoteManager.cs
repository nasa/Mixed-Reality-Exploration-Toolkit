using UnityEngine;
using VRTK;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Common.Schemas;

public class NoteManager : MonoBehaviour
{
    public bool creatingNotes = false;
	public static GameObject notePrefab;
	public GameObject _notePrefab;
    public GameObject leftController, rightController;
    public int noteCount = 0;

    private UndoManager undoManager;

    void Start()
	{
		notePrefab = _notePrefab;

        if (!VRDesktopSwitcher.isDesktopEnabled())
        {
            leftController.GetComponent<VRTK_ControllerEvents>().TouchpadPressed += new ControllerInteractionEventHandler(TouchpadPressed);
            rightController.GetComponent<VRTK_ControllerEvents>().TouchpadPressed += new ControllerInteractionEventHandler(TouchpadPressed);
        }
        else
        {
            EventManager.OnLeftClick += TouchpadPressed;
        }

        undoManager = FindObjectOfType<UndoManager>();
    }

    private void TouchpadPressed()
    {
        if (creatingNotes)
        {
            GameObject control = GameObject.Find("FPSController");
            Vector3 pos = new Vector3(control.transform.position.x,
                control.transform.position.y + 1, control.transform.position.z + 1);
            Quaternion rot = new Quaternion(0f, 0f, 0f, 0f);
            Note n = Note.MakeNote(notePrefab, pos, rot, noteCount++);
            n.guid = System.Guid.NewGuid();
            n.Maximize();
            creatingNotes = false;

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
                    Scale = new Vector3Type()
                    {
                        X = 1,
                        Y = 1,
                        Z = 1
                    }
                }
            };
            undoManager.AddAction(ProjectAction.AddNoteAction(nType, "Note" + (noteCount - 1), pos, rot), ProjectAction.DeleteNoteAction("Note" + (noteCount - 1)));
        }
    }

    private void TouchpadPressed (object sender, ControllerInteractionEventArgs e)
	{
        if (creatingNotes)
        {
            Vector3 pos = e.controllerReference.actual.transform.position;
            Quaternion rot = Quaternion.Euler(0, e.controllerReference.actual.transform.rotation.eulerAngles.y, 0);
            Note n = Note.MakeNote(notePrefab, pos, rot, noteCount++);
            n.guid = System.Guid.NewGuid();
            n.Maximize();
            creatingNotes = false;

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
                    Scale = new Vector3Type()
                    {
                        X = 1,
                        Y = 1,
                        Z = 1
                    }
                }
            };
            undoManager.AddAction(ProjectAction.AddNoteAction(nType, "Note" + (noteCount - 1), pos, rot), ProjectAction.DeleteNoteAction("Note" + (noteCount - 1)));
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

    public void ReinitializeNote(InteractableNote noteToReinitialize)
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