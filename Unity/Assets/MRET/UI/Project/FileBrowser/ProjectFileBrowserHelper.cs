// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.Collaboration;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.Project.FileBrowser
{
    public class ProjectFileBrowserHelper : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ProjectFileBrowserHelper);

        public FileBrowserManager fileBrowserManager;
        public CollaborationShareMenuManager collaborationShareMenuManager;
        public bool collaboration;
        public bool templateBrowser;

        public UnityEvent successEvent;
        public UnityEvent failEvent;

        public void OpenProjectFile()
        {
            if (!fileBrowserManager)
            {
                LogWarning("FileBrowserManager not set.", nameof(OpenProjectFile));
                failEvent?.Invoke();
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                LogWarning("No valid file selected.", nameof(OpenProjectFile));
                failEvent?.Invoke();
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                LogWarning("Unable to get selected file path.", nameof(OpenProjectFile));
                failEvent?.Invoke();
                return;
            }

            if (selectedFile is ProjectType)
            {
                if (collaboration)
                {
                    if (collaborationShareMenuManager)
                    {
                        collaborationShareMenuManager.SetSelectedProject(
                            selectedFilePath, (ProjectType) selectedFile);
                    }
                    else
                    {
                        LogError("Collaboration Share Menu not assigned.", nameof(OpenProjectFile));
                        failEvent?.Invoke();
                        return;
                    }
                }
                else
                {
                    // Open the project file
                    MRET.ModeNavigator.OpenProject(
                        (ProjectType) selectedFile, selectedFilePath, false);
                }
            }
            else
            {
                LogError("Selected file is not a valid Project file: " + selectedFilePath, nameof(OpenProjectFile));
                failEvent?.Invoke();
                return;
            }

            // Success
            successEvent?.Invoke();
        }

        public void SaveProjectFile()
        {
            bool saved = MRET.ProjectManager.SaveToXML(fileBrowserManager.GetSaveFilePath());
            if (saved)
            {
                successEvent?.Invoke();
            }
            else
            {
                LogError("Unable to serialize project.");
                failEvent?.Invoke();
            }
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            if (fileBrowserManager == null)
            {
                fileBrowserManager = GetComponent<FileBrowserManager>();
                if (fileBrowserManager == null)
                {
                    LogWarning("Unable to find FileBrowserManager.", nameof(MRETStart));
                }
            }

            fileBrowserManager.OpenDirectory(
                templateBrowser
                    ? MRET.ConfigurationManager.defaultTemplateDirectory
                    : MRET.ConfigurationManager.defaultProjectDirectory);
        }
    }
}