using UnityEngine;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Common;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.Project
{
    public class ProjectFileBrowserHelper : MonoBehaviour
    {
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
                Debug.LogWarning(
                    "[ProjectFileBrowserHelper] FileBrowserManager not set.");
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                Debug.LogWarning(
                    "[ProjectFileBrowserHelper] No valid file selected.");
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogWarning(
                    "[ProjectFileBrowserHelper] Unable to get selected file path.");
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
                        Debug.LogError("[ProjectFileBrowserHelper] Collaboration Share Menu not assigned.");
                    }
                }
                else
                {
                    ModeNavigator.instance.OpenProject(
                    (ProjectType) selectedFile, selectedFilePath, false);
                }
                successEvent.Invoke();
            }
        }

        public void SaveProjectFile()
        {
            ProjectType pt = UnityProject.instance.Serialize();
            if (pt == null)
            {
                Debug.LogError(
                    "[ProjectFileBrowserHelper] Unable to serialize project.");
                return;
            }

            SchemaHandler.WriteXML(fileBrowserManager.GetSaveFilePath(), pt);
            successEvent.Invoke();
        }

        private void Start()
        {
            if (fileBrowserManager == null)
            {
                fileBrowserManager = GetComponent<FileBrowserManager>();
                if (fileBrowserManager == null)
                {
                    Debug.LogWarning(
                        "[ProjectFileBrowserHelper] Unable to find FileBrowserManager.");
                }
            }

            fileBrowserManager.OpenDirectory(
                templateBrowser ? ConfigurationManager.instance.defaultTemplateDirectory
                : ConfigurationManager.instance.defaultProjectDirectory);
        }
    }
}