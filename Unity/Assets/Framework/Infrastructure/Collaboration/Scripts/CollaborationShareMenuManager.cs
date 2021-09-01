// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GSFC.ARVR.MRET.XRC;
using GSFC.ARVR.MRET.Project;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Components.UI;

public class CollaborationShareMenuManager : MonoBehaviour
{
    public Text sessionText, groupText, aliasText;
    public VR_InputField projectText;
    public Button projectButton;
    public CollaborationManager collaborationManager;
    public WorldSpaceMenuLoader fileBrowserLoader;
    public ProjectFileBrowserHelper pfbh;
    public GameObject mainMenu;

    private List<ProjectInfo> projects;
    private ProjectType projToOpen;

    void Start()
    {
        collaborationManager = FindObjectOfType<CollaborationManager>();
    }

    public void OpenFileBrowser()
    {
        pfbh.collaborationShareMenuManager = this;
        pfbh.gameObject.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void CloseFileBrowser()
    {
        pfbh.gameObject.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void SetSelectedProject(string path, ProjectType project)
    {
        if (System.IO.File.Exists(path))
        {
            projectText.text = path;
            projToOpen = project;
        }
    }

    public void Create()
    {
#if !HOLOLENS_BUILD
        if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
        {
            if (projToOpen != null && System.IO.File.Exists(projectText.text))
            {
                string projectName = System.IO.Path.GetFileNameWithoutExtension(projectText.text);
                string groupName = groupText.text == "" ? "New_Group" : groupText.text;
                string sessName = sessionText.text == "" ? "New_Session" : sessionText.text;

                XRCUnity.ShutDown();

                int idx = collaborationManager.server.LastIndexOf(':');
                if (idx == -1)
                {
                    Debug.LogError("Invalid Server Format.");
                    return;
                }
                string[] serverParts = { collaborationManager.server.Substring(0, idx),
                collaborationManager.server.Substring(idx + 1) };

                XRCUnity.Initialize(serverParts[0], int.Parse(serverParts[1]),
                    XRCManager.GMSECToXRCConnType(collaborationManager.connectionType),
                    collaborationManager.missionName,
                    collaborationManager.satName, projectName, groupName, sessName);

                collaborationManager.xrcManager.StartXRC();

                UnityProject.instance.userAlias = aliasText.text == ""
                    ? "New_User" : aliasText.text;
                UnityProject.instance.userLabelColor =
                    SynchronizedUser.LabelColorFromString(SynchronizedUser.GetRandomColor());
                ModeNavigator.instance.OpenProject(projToOpen, projectText.text, true);

                SynchronizedUser sU = collaborationManager.xrcManager.GetControlledUser();

                XRCUnity.StartSession(UnityProject.instance.userUUID.ToString(),
                    UnityProject.instance.userAlias,
                    (int) SynchronizedUser.UserType.VR,
                    UnityProject.instance.userLabelColor.ToString(),
                    UnityProject.instance.lcUUID.ToString(),
                    UnityProject.instance.rcUUID.ToString(),
                    UnityProject.instance.lpUUID.ToString(),
                    UnityProject.instance.rpUUID.ToString()); // TODO type

                XRCManager.AddControllersToSession(UnityProject.instance.userAlias,
                    UnityProject.instance.userUUID.ToString(),
                    UnityProject.instance.lcUUID.ToString(),
                    UnityProject.instance.rcUUID.ToString(),
                    UnityProject.instance.lpUUID.ToString(),
                    UnityProject.instance.rpUUID.ToString());
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(projectText.text))
            {
                SessionInformation sessionInfo = new SessionInformation();
                sessionInfo.sessionName = sessionText.text;
                if (sessionText.text == "")
                {
                    sessionInfo.sessionName = "New_Session";
                }

                sessionInfo.groupName = groupText.text;
                if (groupText.text == "")
                {
                    sessionInfo.groupName = "New_Group";
                }

                sessionInfo.numUsers = 1;

                sessionInfo.projectName = projectText.text;

                if (aliasText.text == "")
                {
                    collaborationManager.EnterMasterMode(sessionInfo, "New_User");
                }
                else
                {
                    collaborationManager.EnterMasterMode(sessionInfo, aliasText.text);
                }
            }
        }
#endif
    }
}