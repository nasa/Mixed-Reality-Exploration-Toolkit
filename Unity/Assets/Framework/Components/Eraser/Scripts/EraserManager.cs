using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class EraserManager : MonoBehaviour
{
    public VRTK.VRTK_ControllerEvents leftController, rightController;
    public VRTK.VRTK_InteractTouch leftTouch, rightTouch;
    public GameObject leftEraserObject, rightEraserObject;
    public bool canErase
    {
        get
        {
            return eraserActive;
        }
    }

    private bool eraserActive = false;
    private VRTK.VRTK_InteractableObject leftTouching, rightTouching;
    private UndoManager undoManager;
    private ProjectAction leftUndo, rightUndo, leftRedo, rightRedo;

    public void Enable()
    {
        leftEraserObject.SetActive(true);
        if (rightEraserObject) rightEraserObject.SetActive(true);
        eraserActive = true;
    }

    public void Disable()
    {
        leftEraserObject.SetActive(false);
        if (rightEraserObject) rightEraserObject.SetActive(false);
        eraserActive = false;
    }

	void Start()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            leftController.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(LTouchpadPressed);
            rightController.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(RTouchpadPressed);
            leftTouch.ControllerStartTouchInteractableObject += new VRTK.ObjectInteractEventHandler(LControllerTouched);
            rightTouch.ControllerStartTouchInteractableObject += new VRTK.ObjectInteractEventHandler(RControllerTouched);
            leftTouch.ControllerUntouchInteractableObject += new VRTK.ObjectInteractEventHandler(LControllerUnTouched);
            rightTouch.ControllerUntouchInteractableObject += new VRTK.ObjectInteractEventHandler(RControllerUnTouched);
        }

        undoManager = FindObjectOfType<UndoManager>();
    }

    void LTouchpadPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (eraserActive)
        {
            if (leftTouching)
            {
                undoManager.AddAction(leftRedo, leftUndo);
                Destroy(leftTouching.gameObject);
            }
        }
    }

    void RTouchpadPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (eraserActive)
        {
            if (rightTouching)
            {
                undoManager.AddAction(rightRedo, rightUndo);
                Destroy(rightTouching.gameObject);
            }
        }
    }

    void LControllerTouched(object sender, VRTK.ObjectInteractEventArgs e)
    {
        if (eraserActive)
        {
            if (e.target != null)
            {
                InteractablePart iPart = e.target.GetComponentInParent<InteractablePart>();
                if (iPart != null)
                {
                    PartType serializedPart = iPart.Serialize();
                    leftUndo = ProjectAction.AddObjectAction(serializedPart, iPart.gameObject.transform.position,
                        iPart.gameObject.transform.rotation,
                        new Vector3(serializedPart.PartTransform.Scale.X, serializedPart.PartTransform.Scale.Y,
                        serializedPart.PartTransform.Scale.Z),
                        new InteractablePart.InteractablePartSettings(serializedPart.EnableInteraction[0],
                        serializedPart.EnableCollisions[0], serializedPart.EnableGravity[0]));
                    leftRedo = ProjectAction.DeleteObjectAction(iPart.gameObject.name);

                    leftTouching = iPart;
                }

                MeshLineRenderer mRend = e.target.GetComponentInParent<MeshLineRenderer>();
                if (mRend != null)
                {
                    leftUndo = ProjectAction.AddDrawingAction(mRend.drawingScript.Serialize());
                    leftRedo = ProjectAction.DeleteDrawingAction(mRend.gameObject.name, mRend.drawingScript.guid.ToString());

                    leftTouching = e.target.GetComponent<VRTK.VRTK_InteractableObject>();
                }

                Note note = e.target.GetComponentInParent<Note>();
                if (note != null)
                {
                    leftUndo = ProjectAction.AddNoteAction(note.ToNoteType(),
                        note.name, note.transform.position, note.transform.rotation);
                    leftRedo = ProjectAction.DeleteNoteAction(note.name);

                    leftTouching = e.target.GetComponent<VRTK.VRTK_InteractableObject>();
                }
            }
        }
    }

    void RControllerTouched(object sender, VRTK.ObjectInteractEventArgs e)
    {
        if (eraserActive)
        {
            if (e.target != null)
            {
                InteractablePart iPart = e.target.GetComponentInParent<InteractablePart>();
                if (iPart != null)
                {
                    PartType serializedPart = iPart.Serialize();
                    rightUndo = ProjectAction.AddObjectAction(serializedPart, iPart.gameObject.transform.position,
                        iPart.gameObject.transform.rotation,
                        new Vector3(serializedPart.PartTransform.Scale.X, serializedPart.PartTransform.Scale.Y,
                        serializedPart.PartTransform.Scale.Z),
                        new InteractablePart.InteractablePartSettings(serializedPart.EnableInteraction[0],
                        serializedPart.EnableCollisions[0], serializedPart.EnableGravity[0]));
                    rightRedo = ProjectAction.DeleteObjectAction(iPart.gameObject.name);

                    rightTouching = iPart;
                }

                MeshLineRenderer mRend = e.target.GetComponentInParent<MeshLineRenderer>();
                if (mRend != null)
                {
                    rightUndo = ProjectAction.AddDrawingAction(mRend.drawingScript.Serialize());
                    rightRedo = ProjectAction.DeleteDrawingAction(mRend.name, mRend.drawingScript.guid.ToString());

                    rightTouching = e.target.GetComponent<VRTK.VRTK_InteractableObject>();
                }

                Note note = e.target.GetComponentInParent<Note>();
                if (note != null)
                {
                    rightUndo = ProjectAction.AddNoteAction(note.ToNoteType(),
                        note.name, note.transform.position, note.transform.rotation);
                    rightRedo = ProjectAction.DeleteNoteAction(note.name);

                    rightTouching = e.target.GetComponent<VRTK.VRTK_InteractableObject>();
                }
            }
        }
    }

    void LControllerUnTouched(object sender, VRTK.ObjectInteractEventArgs e)
    {
        if (eraserActive)
        {
            leftUndo = leftRedo = null;
            leftTouching = null;
        }
    }

    void RControllerUnTouched(object sender, VRTK.ObjectInteractEventArgs e)
    {
        if (eraserActive)
        {
            rightUndo = rightRedo = null;
            rightTouching = null;
        }
    }
}