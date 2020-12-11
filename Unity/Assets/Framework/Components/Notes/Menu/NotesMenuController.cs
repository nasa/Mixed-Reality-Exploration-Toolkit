using UnityEngine;
using UnityEngine.UI;

public class NotesMenuController : MonoBehaviour
{
    public Toggle noteToggle;
    public NoteManager noteManager;
    public ControlMode controlMode;

    private int toggleCountDown = 0;

    public void Update()
    {
        if (toggleCountDown > 0)
        {
            toggleCountDown--;
            if (toggleCountDown == 0)
            {
                noteToggle.isOn = noteManager.creatingNotes;
            }
        }
    }

    public void OnEnable()
    {
        toggleCountDown = 3;
    }

    public void OnDisable()
    {
        toggleCountDown = 3;
    }

    public void ToggleNotes()
    {
        if (toggleCountDown == 0)
        {
            SwitchNotes(!noteManager.creatingNotes);
        }
    }

    public void SwitchNotes(bool on)
    {
        if (on)
        {
            noteManager.creatingNotes = true;
            controlMode.EnterNotesMode();
        }
        else
        {
            noteManager.creatingNotes = false;
            if (controlMode.activeControlType == ControlMode.ControlType.Notes)
            {
                controlMode.DisableAllControlTypes();
            }
        }
    }

    public void ExitMode()
    {
        if (noteToggle)
        {
            noteToggle.isOn = false;
            noteManager.creatingNotes = false;
        }
    }
}