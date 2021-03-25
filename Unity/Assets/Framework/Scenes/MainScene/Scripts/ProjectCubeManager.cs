// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework.Interactable;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class ProjectCubeManager : Interactable
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

    public void StartTouching()
    {
        billboardImage.sprite = screenshot.sprite;
        billboardName.text = projectName.text;
        billboardEdited.text = lastEdited1.text;
    }

    public void StartUsing()
    {
        switch (cubeType)
        {
            case CubeType.Projects:
                MRET.ConfigurationManager.AddRecentProject(projectToOpen);
                modeNavigator.OpenProject(projectToOpen, false);
                break;

            case CubeType.Templates:
                MRET.ConfigurationManager.AddRecentTemplate(projectToOpen);
                modeNavigator.OpenProject(projectToOpen, false);
                break;

            case CubeType.Collaborative:
                MRET.ConfigurationManager.AddRecentCollaboration(projectToOpen);
                modeNavigator.OpenProject(projectToOpen, true);
                break;

            default:
                break;
        }
    }
}