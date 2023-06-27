// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRUI;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Note
{
    public class NotesMenuController : MenuController
    {
        public InputHand hand;

        public override void Initialize()
        {

        }

        public void SwitchNotes()
        {
            if (ProjectManager.NoteManager.creatingNotes == false)
            {
                ProjectManager.NoteManager.creatingNotes = true;
                MRET.ControlMode.EnterNotesMode();
            }
            else
            {
                ProjectManager.NoteManager.creatingNotes = false;
                if (MRET.ControlMode.activeControlType == ControlMode.ControlType.Notes)
                {
                    MRET.ControlMode.DisableAllControlTypes();
                }
            }
        }

        public void SwitchNotes(bool on)
        {
            if (on)
            {
                ProjectManager.NoteManager.creatingNotes = true;
                MRET.ControlMode.EnterNotesMode();
            }
            else
            {
                ProjectManager.NoteManager.creatingNotes = false;
                if (MRET.ControlMode.activeControlType == ControlMode.ControlType.Notes)
                {
                    MRET.ControlMode.DisableAllControlTypes();
                }
            }
        }

        public void CreateNote()
        {
            Vector3 position = hand.transform.position;
            Quaternion rotation = hand.transform.rotation;
            ProjectManager.NoteManager.CreateNote("Note", position, rotation);
        }

        public void ExitMode()
        {
            ProjectManager.NoteManager.creatingNotes = false;
        }
    }
}