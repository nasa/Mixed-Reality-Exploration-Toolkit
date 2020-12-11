using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;

public class ProjectSaveMenuManager : MonoBehaviour
{
    public Text saveProjectName;
    public WorldSpaceMenuManager menuMan;

    private UnityProject projectManager;
    private ConfigurationManager configManager;

    void Start()
    {
        projectManager = FindObjectOfType<UnityProject>();
        configManager = FindObjectOfType<ConfigurationManager>();
    }

    public void SaveProject()
    {
        if (saveProjectName.text == "" || saveProjectName.text == null)
        {
            Debug.LogWarning("[ProjectSaveMenuManager->SaveProject] No project name provided. Project not saved.");
            return;
        }
        else if (saveProjectName.text.Length >= 2)
        {
            if (saveProjectName.text.Substring(0, 2) == "C:")
            {
                projectManager.SaveToXML(saveProjectName.text + ".mtproj");
                menuMan.DimMenu();
                return;
            }
            else
            {
                if (configManager != null && projectManager != null)
                {
                    if (configManager.config != null)
                    {
                        if (configManager.config.ProjectsPath != null)
                        {
                            if (System.IO.Directory.Exists(Application.dataPath + "\\" + configManager.config.ProjectsPath))
                            {
                                projectManager.SaveToXML(Application.dataPath + "\\" +
                                    configManager.config.ProjectsPath + "\\" + saveProjectName.text + ".mtproj");
                                menuMan.DimMenu();
                                return;
                            }
                        }
                    }
                }
                Debug.LogWarning("[ProjectSaveMenuManager->SaveProject] Unable to find location to save project. Project not saved.");
            }
        }
    }
}