using UnityEngine.UI;
using VRTK;
using GSFC.ARVR.MRET.Common.Schemas;

public class ProjectCubeManager : VRTK_InteractableObject
{
    public enum CubeType { Projects, Templates, Collaborative };

    public Image screenshot;
    public Text projectName;
    public Text lastEdited1, lastEdited2;
    public ProjectInfo projectToOpen;
    public Image billboardImage;
    public Text billboardName, billboardEdited;
    public ModeNavigator modeNavigator;
    public UnityProject projectManager;
    public CubeType cubeType = CubeType.Projects;

    public override void StartTouching(VRTK_InteractTouch currentTouchingObject)
    {
        billboardImage.sprite = screenshot.sprite;
        billboardName.text = projectName.text;
        billboardEdited.text = lastEdited1.text;
    }

    public override void StartUsing(VRTK_InteractUse currentUsingObject)
    {
        switch (cubeType)
        {
            case CubeType.Projects:
                modeNavigator.configManager.AddRecentProject(projectToOpen);
                modeNavigator.OpenProject(projectToOpen, false);
                break;

            case CubeType.Templates:
                modeNavigator.configManager.AddRecentTemplate(projectToOpen);
                modeNavigator.OpenProject(projectToOpen, false);
                break;

            case CubeType.Collaborative:
                modeNavigator.configManager.AddRecentCollaboration(projectToOpen);
                modeNavigator.OpenProject(projectToOpen, true);
                break;

            default:
                break;
        }
    }
}