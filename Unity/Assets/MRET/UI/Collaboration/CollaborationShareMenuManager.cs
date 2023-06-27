// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.Project.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.Collaboration
{
    public class CollaborationShareMenuManager : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(CollaborationShareMenuManager);

        public const string DEFAULT_ALIAS = "New_User";

        public Text sessionText, groupText, aliasText;
        public VR_InputField projectText;
        public Button projectButton;
        public WorldSpaceMenuLoader fileBrowserLoader;
        public ProjectFileBrowserHelper pfbh;
        public GameObject mainMenu;

        private CollaborationManager collaborationManager;
        private List<ProjectInfo> projects;
        private ProjectType projToOpen;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            collaborationManager = MRET.CollaborationManager;
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
            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
            {
                if (projToOpen != null && System.IO.File.Exists(projectText.text))
                {
                    string projectName = System.IO.Path.GetFileNameWithoutExtension(projectText.text);
                    string groupName = groupText.text == "" ? "New_Group" : groupText.text;
                    string sessName = sessionText.text == "" ? "New_Session" : sessionText.text;

                    collaborationManager.StopEngine();

                    int idx = collaborationManager.server.LastIndexOf(':');
                    if (idx == -1)
                    {
                        LogError("Invalid Server Format.", nameof(Create));
                        return;
                    }
                    string[] serverParts = { collaborationManager.server.Substring(0, idx),
                collaborationManager.server.Substring(idx + 1) };

                    collaborationManager.InitializeEngine(
                        serverParts[0], int.Parse(serverParts[1]),
                        projectName, groupName, sessName);

                    collaborationManager.StartEngine();

                    MRET.ModeNavigator.OpenProject(projToOpen, projectText.text, true);

                    IUser localUser = collaborationManager.GetLocalUser();
                    if (localUser != null)
                    {
                        // Override the user alias if supplied.
                        // Text field takes precedence, then existing user alias, then default
                        localUser.Alias = !string.IsNullOrEmpty(aliasText.ToString()) ? aliasText.ToString() :
                            !string.IsNullOrEmpty(localUser.Alias) ? localUser.Alias : DEFAULT_ALIAS;

                        // Start the session
                        collaborationManager.StartSession(localUser);
                    }
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
        }
    }
}