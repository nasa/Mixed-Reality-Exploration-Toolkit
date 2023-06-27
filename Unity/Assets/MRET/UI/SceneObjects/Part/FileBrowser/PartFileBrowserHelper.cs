// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part.FileBrowser
{
    public class PartFileBrowserHelper : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PartFileBrowserHelper);

        public FileBrowserManager fileBrowserManager;
        public Button rmitButton;

        public void OpenPartFile()
        {
            if (!fileBrowserManager)
            {
                LogWarning("FileBrowserManager not set.", nameof(OpenPartFile));
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                LogWarning("No valid file selected.", nameof(OpenPartFile));
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                LogWarning("Unable to get selected file path.", nameof(OpenPartFile));
                return;
            }

            if (selectedFile is PartType)
            {
                ProjectManager.PartManager.InstantiatePart((PartType) selectedFile, true);
            }
        }

        private void OnEnable()
        {
            if (rmitButton != null)
            {
                rmitButton.interactable = MRET.ConfigurationManager.RMITAvailable;
            }
            else
            {
                LogWarning("RMIT button reference is not set", nameof(OnEnable));
            }
        }

        protected override void MRETStart()
        {
            base.MRETStart();

            if (fileBrowserManager == null)
            {
                fileBrowserManager = GetComponent<FileBrowserManager>();
                if (fileBrowserManager == null)
                {
                    LogWarning("Unable to find FileBrowserManager reference.", nameof(MRETStart));
                }
            }

            fileBrowserManager.OpenDirectory(MRET.ConfigurationManager.defaultPartDirectory);
        }
    }
}