// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Common;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.Part
{
    public class PartFileBrowserHelper : MonoBehaviour
    {
        public FileBrowserManager fileBrowserManager;

        public void OpenPartFile()
        {
            if (!fileBrowserManager)
            {
                Debug.LogWarning(
                    "[PartFileBrowserHelper] FileBrowserManager not set.");
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                Debug.LogWarning(
                    "[PartFileBrowserHelper] No valid file selected.");
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogWarning(
                    "[PartFileBrowserHelper] Unable to get selected file path.");
                return;
            }

            if (selectedFile is PartType)
            {
                Infrastructure.Framework.MRET.PartManager.InstantiatePart((PartType) selectedFile, null);
            }
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

            fileBrowserManager.OpenDirectory(ConfigurationManager.instance.defaultPartDirectory);
        }
    }
}