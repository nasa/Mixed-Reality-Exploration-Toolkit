// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Components.Notes;

public class InteractableNote : SceneObject
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
        /*if (VRDesktopSwitcher.isDesktopEnabled())
        {
            foreach (VRTK_UICanvas canvas in GetComponentsInChildren<VRTK.VRTK_UICanvas>(true))
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Destroy(canvas);
            }
        }*/
    }


    private bool needToDisable = false, needToEnable = false;
    private int enableDelay = 0;
    private void Update()
    {
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

    public override void BeginGrab(InputHand hand)
    {
        base.BeginGrab(hand);

        note.DisableDrawing();
    }

    public override void EndGrab(InputHand hand)
    {
        base.EndGrab(hand);

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