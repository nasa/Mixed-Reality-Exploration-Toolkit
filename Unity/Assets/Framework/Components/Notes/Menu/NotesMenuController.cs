// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GSFC.ARVR.MRET.Integrations.XRUI;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Components.Notes
{
    public class NotesMenuController : MenuController
    {
        public InputHand hand;

        public override void Initialize()
        {

        }

        public void SwitchNotes()
        {
            if (Infrastructure.Framework.MRET.NoteManager.creatingNotes == false)
            {
                Infrastructure.Framework.MRET.NoteManager.creatingNotes = true;
                Infrastructure.Framework.MRET.ControlMode.EnterNotesMode();
            }
            else
            {
                Infrastructure.Framework.MRET.NoteManager.creatingNotes = false;
                if (Infrastructure.Framework.MRET.ControlMode.activeControlType == ControlMode.ControlType.Notes)
                {
                    Infrastructure.Framework.MRET.ControlMode.DisableAllControlTypes();
                }
            }
        }

        public void SwitchNotes(bool on)
        {
            if (on)
            {
                Infrastructure.Framework.MRET.NoteManager.creatingNotes = true;
                Infrastructure.Framework.MRET.ControlMode.EnterNotesMode();
            }
            else
            {
                Infrastructure.Framework.MRET.NoteManager.creatingNotes = false;
                if (Infrastructure.Framework.MRET.ControlMode.activeControlType == ControlMode.ControlType.Notes)
                {
                    Infrastructure.Framework.MRET.ControlMode.DisableAllControlTypes();
                }
            }
        }

        public void CreateNote()
        {
            Infrastructure.Framework.MRET.NoteManager.CreateNote(hand.gameObject);
        }

        public void ExitMode()
        {
            Infrastructure.Framework.MRET.NoteManager.creatingNotes = false;
        }
    }
}