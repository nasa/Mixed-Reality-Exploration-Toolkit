// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

public class RecentProjectsManager : MonoBehaviour
{
    public enum CubeType { Projects, Templates, Collaborative };

    public ProjectCubeManager cube1, cube2, cube3;
    public Image billboardImage;
    public Text billboardName, billboardEdited;
    public CubeType cubeType = CubeType.Projects;

    private ConfigurationManager configManager;

	void Start()
    {
        SetUpProjectCubes();
    }

    private void SetUpProjectCubes()
    {
        configManager = FindObjectOfType<ConfigurationManager>();

        ProjectInfo[] projectFiles = (cubeType == CubeType.Projects) ? configManager.GetRecentProjects()
            : (cubeType == CubeType.Templates) ? configManager.GetRecentTemplates() : configManager.GetRecentCollaborations();

        if (projectFiles.Length >= 1)
        {
            cube1.gameObject.SetActive(true);
            cube1.projectName.text = projectFiles[0].name;
            cube1.lastEdited1.text = cube1.lastEdited2.text = "Last Edited: " + projectFiles[0].timeStamp.ToString();
            cube1.projectToOpen = projectFiles[0];
            cube1.billboardImage = billboardImage;
            cube1.billboardName = billboardName;
            cube1.billboardEdited = billboardEdited;
            if (projectFiles[0].thumbnail != null)
            {
                cube1.screenshot.sprite = Sprite.Create(projectFiles[0].thumbnail,
                    new Rect(0, 0, projectFiles[0].thumbnail.width, projectFiles[0].thumbnail.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            cube1.gameObject.SetActive(false);
        }

        if (projectFiles.Length >= 2)
        {
            cube2.gameObject.SetActive(true);
            cube2.projectName.text = projectFiles[1].name;
            cube2.lastEdited1.text = cube2.lastEdited2.text = "Last Edited: " + projectFiles[1].timeStamp.ToString();
            cube2.projectToOpen = projectFiles[1];
            cube2.billboardImage = billboardImage;
            cube2.billboardName = billboardName;
            cube2.billboardEdited = billboardEdited;
            if (projectFiles[1].thumbnail != null)
            {
                cube2.screenshot.sprite = Sprite.Create(projectFiles[1].thumbnail,
                    new Rect(0, 0, projectFiles[1].thumbnail.width, projectFiles[1].thumbnail.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            cube2.gameObject.SetActive(false);
        }

        if (projectFiles.Length >= 3)
        {
            cube3.gameObject.SetActive(true);
            cube3.projectName.text = projectFiles[2].name;
            cube3.lastEdited1.text = cube3.lastEdited2.text = "Last Edited: " + projectFiles[2].timeStamp.ToString();
            cube3.projectToOpen = projectFiles[2];
            cube3.billboardImage = billboardImage;
            cube3.billboardName = billboardName;
            cube3.billboardEdited = billboardEdited;
            if (projectFiles[2].thumbnail != null)
            {
                cube3.screenshot.sprite = Sprite.Create(projectFiles[2].thumbnail,
                    new Rect(0, 0, projectFiles[2].thumbnail.width, projectFiles[2].thumbnail.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            cube3.gameObject.SetActive(false);
        }
    }
}