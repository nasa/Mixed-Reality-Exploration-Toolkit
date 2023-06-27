// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Note
{
    public class InteractableNoteDeprecated : InteractableSceneObjectDeprecated
    {
        private UndoManagerDeprecated undoManagerDeprecated;
        private Vector3 lastSavedPosition;
        private Quaternion lastSavedRotation;
        private NoteDeprecated note;

        public new static InteractableNoteDeprecated Create()
        {
            GameObject interactableNoteGameObject = new GameObject();
            return interactableNoteGameObject.AddComponent<InteractableNoteDeprecated>();
        }

        protected override void MRETStart()
        {
            base.MRETStart();

            undoManagerDeprecated = FindObjectOfType<UndoManagerDeprecated>();

            lastSavedPosition = transform.position;
            lastSavedRotation = transform.rotation;

            note = GetComponent<NoteDeprecated>();

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
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

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
                    undoManagerDeprecated.AddAction(ProjectActionDeprecated.MoveNoteAction(transform.name, transform.position, transform.rotation),
                        ProjectActionDeprecated.MoveNoteAction(transform.name, lastSavedPosition, lastSavedRotation));
                    lastSavedPosition = transform.position;
                    lastSavedRotation = transform.rotation;
                }
            }

            // This seems to be necessary to update the colliders.
            NoteManagerDeprecated noteMan = FindObjectOfType<NoteManagerDeprecated>();
            if (noteMan)
            {
                noteMan.ReinitializeNote(this);
            }
        }
    }
}