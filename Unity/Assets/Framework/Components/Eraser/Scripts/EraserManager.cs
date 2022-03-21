// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Components.Notes;

public class EraserManager : MonoBehaviour
{
    public GameObject leftEraserObject, rightEraserObject;
    public bool canErase { get; private set; } = false;

    private SceneObject leftTouching, rightTouching;
    private UndoManager undoManager;
    private ProjectAction leftUndo, rightUndo, leftRedo, rightRedo;

    public void Enable()
    {
        leftEraserObject.SetActive(true);
        if (rightEraserObject) rightEraserObject.SetActive(true);
        canErase = true;
    }

    public void Disable()
    {
        leftEraserObject.SetActive(false);
        if (rightEraserObject) rightEraserObject.SetActive(false);
        canErase = false;
    }

	void Start()
    {
        undoManager = FindObjectOfType<UndoManager>();
    }

    public void LTouchpadPressed()
    {
        if (canErase)
        {
            if (leftTouching)
            {
                undoManager.AddAction(leftRedo, leftUndo);

                // Destroy all children first.
                foreach (Transform t in leftTouching.gameObject.transform)
                {
                    Destroy(t.gameObject);
                }

                Destroy(leftTouching.gameObject);
            }
        }
    }

    public void RTouchpadPressed()
    {
        if (canErase)
        {
            if (rightTouching)
            {
                undoManager.AddAction(rightRedo, rightUndo);

                // Destroy all children first.
                foreach (Transform t in rightTouching.gameObject.transform)
                {
                    Destroy(t.gameObject);
                }

                Destroy(rightTouching.gameObject);
            }
        }
    }

    public void LControllerTouched(GameObject go)
    {
        if (canErase)
        {
            if (go != null)
            {
                InteractablePart iPart = go.GetComponentInParent<InteractablePart>();
                if (iPart != null)
                {
                    PartType serializedPart = iPart.Serialize();
                    leftUndo = ProjectAction.AddObjectAction(serializedPart, iPart.gameObject.transform.position,
                        iPart.gameObject.transform.rotation,
                        new Vector3(serializedPart.PartTransform.Scale.X, serializedPart.PartTransform.Scale.Y,
                        serializedPart.PartTransform.Scale.Z),
                        new InteractablePart.InteractablePartSettings(serializedPart.EnableInteraction[0],
                        serializedPart.EnableCollisions[0], serializedPart.EnableGravity[0]));
                    leftRedo = ProjectAction.DeleteObjectAction(iPart.gameObject.name, serializedPart.GUID);

                    leftTouching = iPart;
                }

                /*MeshLineRenderer mRend = go.GetComponentInParent<MeshLineRenderer>();
                if (mRend != null)
                {
                    leftUndo = ProjectAction.AddDrawingAction(mRend.drawingScript.Serialize());
                    leftRedo = ProjectAction.DeleteDrawingAction(mRend.gameObject.name, mRend.drawingScript.guid.ToString());

                    leftTouching = go.GetComponent<SceneObject>();
                }*/
                GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing lDraw =
                    go.GetComponentInParent<GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing>();
                if (lDraw != null)
                {
                    leftUndo = ProjectAction.AddDrawingAction(lDraw);
                    leftRedo = ProjectAction.DeleteDrawingAction(lDraw.name, lDraw.uuid.ToString());

                    leftTouching = lDraw;
                }

                Note note = go.GetComponentInParent<Note>();
                if (note != null)
                {
                    leftUndo = ProjectAction.AddNoteAction(note.ToNoteType(),
                        note.name, note.transform.position, note.transform.rotation);
                    leftRedo = ProjectAction.DeleteNoteAction(note.name, note.guid.ToString());

                    leftTouching = go.GetComponentInParent<SceneObject>();
                }
            }
        }
    }

    public void RControllerTouched(GameObject go)
    {
        if (canErase)
        {
            if (go != null)
            {
                InteractablePart iPart = go.GetComponentInParent<InteractablePart>();
                if (iPart != null)
                {
                    if (!iPart.locked)
                    {
                        PartType serializedPart = iPart.Serialize();
                        rightUndo = ProjectAction.AddObjectAction(serializedPart, iPart.gameObject.transform.position,
                            iPart.gameObject.transform.rotation,
                            new Vector3(serializedPart.PartTransform.Scale.X, serializedPart.PartTransform.Scale.Y,
                            serializedPart.PartTransform.Scale.Z),
                            new InteractablePart.InteractablePartSettings(serializedPart.EnableInteraction[0],
                            serializedPart.EnableCollisions[0], serializedPart.EnableGravity[0]));
                        rightRedo = ProjectAction.DeleteObjectAction(iPart.gameObject.name, serializedPart.GUID);

                        rightTouching = iPart;
                    }
                }

                /*MeshLineRenderer mRend = go.GetComponentInParent<MeshLineRenderer>();
                if (mRend != null)
                {
                    rightUndo = ProjectAction.AddDrawingAction(mRend.drawingScript.Serialize());
                    rightRedo = ProjectAction.DeleteDrawingAction(mRend.name, mRend.drawingScript.guid.ToString());

                    rightTouching = go.GetComponent<SceneObject>();
                }*/
                GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing lDraw =
                    go.GetComponentInParent<GSFC.ARVR.MRET.Components.LineDrawing.LineDrawing>(true);
                if (lDraw != null)
                {
                    rightUndo = ProjectAction.AddDrawingAction(lDraw);
                    rightRedo = ProjectAction.DeleteDrawingAction(lDraw.name, lDraw.uuid.ToString());

                    rightTouching = lDraw;
                }

                Note note = go.GetComponentInParent<Note>();
                if (note != null)
                {
                    if (!note.locked)
                    {
                        rightUndo = ProjectAction.AddNoteAction(note.ToNoteType(),
                            note.name, note.transform.position, note.transform.rotation);
                        rightRedo = ProjectAction.DeleteNoteAction(note.name, note.guid.ToString());

                        rightTouching = go.GetComponentInParent<SceneObject>();
                    }
                }
            }
        }
    }

    public void LControllerUnTouched()
    {
        if (canErase)
        {
            leftUndo = leftRedo = null;
            leftTouching = null;
        }
    }

    public void RControllerUnTouched()
    {
        if (canErase)
        {
            rightUndo = rightRedo = null;
            rightTouching = null;
        }
    }
}