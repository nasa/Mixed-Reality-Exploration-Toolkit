using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;

public class ProjectMenuController : MonoBehaviour
{
    public ConfigurationManager configManager;
    public Dropdown projectDropdown;
    public ModeNavigator modeNavigator;
    public Text saveProjectName;
    public UnityProject projectManager;

    private List<ProjectInfo> availableProjects = new List<ProjectInfo>();

	public void Start ()
    {
        SetProjectDropdownOptions();

        // Handle Project Updates.
        projectDropdown.onValueChanged.AddListener(delegate
        {
            SetProject();
        });
    }

    public void SetProject()
    {
        if (projectDropdown.value == 0)
        {
            modeNavigator.LoadLobby();
        }
        else
        {
            modeNavigator.OpenProject(availableProjects[projectDropdown.value - 1], false);
        }
    }

    public void SaveProject()
    {
        if (saveProjectName.text == "" || saveProjectName.text == null)
        {
            Debug.LogWarning("[ProjectMenuController->SaveProject] No project name provided. Project not saved.");
            return;
        }
        else if (saveProjectName.text.Length >= 2)
        {
            if (saveProjectName.text.Substring(0, 2) == "C:")
            {
                projectManager.SaveToXML(saveProjectName.text + ".mtproj");
                return;
            }
            else
            {
                if (configManager.config != null)
                {
                    if (configManager.config.ProjectsPath != null)
                    {
                        if (System.IO.Directory.Exists(Application.dataPath + Path.DirectorySeparatorChar + configManager.config.ProjectsPath))
                        {
                            projectManager.SaveToXML(Application.dataPath + Path.DirectorySeparatorChar +
                                configManager.config.ProjectsPath + Path.DirectorySeparatorChar + saveProjectName.text + ".mtproj");
                            return;
                        }
                    }
                }
                Debug.LogWarning("[ProjectMenuController->SaveProject] Unable to find location to save project. Project not saved.");
            }
        }
    }

    private void SetProjectDropdownOptions()
    {
        List<string> dropdownLabels = new List<string>();
        dropdownLabels.Add("Lobby");

        List<ProjectInfo> projects = configManager.projects;
        foreach (ProjectInfo project in projects)
        {
            dropdownLabels.Add(project.name);
        }

        projectDropdown.ClearOptions();
        projectDropdown.AddOptions(dropdownLabels);
        availableProjects = projects;
        projectDropdown.RefreshShownValue();
    }

    private void OnEnable()
    {
        SetProjectDropdownOptions();
    }
}