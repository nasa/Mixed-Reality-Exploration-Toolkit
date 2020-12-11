using UnityEngine;
using VRTK;
using GSFC.ARVR.MRET.Common.Schemas;

public class GrabProjectOpener : VRTK_InteractableObject
{
    [Tooltip("Tooltip item to show when user is interacting with object.")]
    public GameObject tooltipItem;

    [Tooltip("Mode navigator/manager to change modes with.")]
    public ModeNavigator modeNavigator;

    [Tooltip("Project Manager to use to load project.")]
    public UnityProject projectManager;

    [Tooltip("Name of the XML project file.")]
    public string projectToOpen;

    public override void StartTouching(VRTK_InteractTouch currentTouchingObject)
    {
        tooltipItem.SetActive(true);
    }

    public override void StopTouching(VRTK_InteractTouch currentTouchingObject)
    {
        tooltipItem.SetActive(false);
    }

    public override void StartUsing(VRTK_InteractUse currentUsingObject)
    {
        modeNavigator.OpenProject(Application.dataPath + "/" + projectToOpen, false);
    }
}