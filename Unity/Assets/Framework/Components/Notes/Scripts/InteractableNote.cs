using UnityEngine;
using VRTK;

public class InteractableNote : VRTK_InteractableObject
{
    private UndoManager undoManager;
    private Vector3 lastSavedPosition;
    private Quaternion lastSavedRotation;
    private Note note;

    void Start()
    {
        undoManager = FindObjectOfType<UndoManager>();

        lastSavedPosition = transform.position;
        lastSavedRotation = transform.rotation;

        note = GetComponent<Note>();

        // If in desktop mode, remove VRTK UI Canvases and add Graphic Raycaster.
        if (VRDesktopSwitcher.isDesktopEnabled())
        {
            foreach (VRTK_UICanvas canvas in GetComponentsInChildren<VRTK.VRTK_UICanvas>(true))
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Destroy(canvas);
            }
        }
    }


    private bool needToDisable = false, needToEnable = false;
    private int enableDelay = 0;
    protected override void Update()
    {
        base.Update();

        if (needToDisable)
        {
            gameObject.SetActive(false);
            needToDisable = false;
            needToEnable = true;
        }

        if (needToEnable)
        {
            if (enableDelay > 100)
            {
                gameObject.SetActive(true);
                needToEnable = false;
                enableDelay = 0;
            }
            else
            {
                enableDelay++;
            }
        }
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectGrabbed(e);

        note.DisableDrawing();

        foreach (KeyboardManager keyboard in GetComponentsInChildren<KeyboardManager>())
        {
            keyboard.Close();
        }
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUngrabbed(e);

        note.EnableDrawing();

        if (lastSavedPosition != null && lastSavedRotation != null)
        {
            if (transform.position != lastSavedPosition && transform.rotation != lastSavedRotation)
            {
                undoManager.AddAction(ProjectAction.MoveNoteAction(transform.name, transform.position, transform.rotation),
                    ProjectAction.MoveNoteAction(transform.name, lastSavedPosition, lastSavedRotation));
                lastSavedPosition = transform.position;
                lastSavedRotation = transform.rotation;
            }
        }

        // This seems to be necessary to update the colliders.
        NoteManager noteMan = FindObjectOfType<NoteManager>();
        if (noteMan)
        {
            noteMan.ReinitializeNote(this);
        }
    }
}