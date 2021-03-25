// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Common;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.HUD
{
    public class HUDFileBrowserHelper : MonoBehaviour
    {
        public FileBrowserManager fileBrowserManager;

        public UnityEvent successEvent;
        public UnityEvent failEvent;

        public void OpenHUDFile()
        {
            if (!fileBrowserManager)
            {
                Debug.LogWarning(
                    "[HUDFileBrowserHelper] FileBrowserManager not set.");
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                Debug.LogWarning(
                    "[HUDFileBrowserHelper] No valid file selected.");
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogWarning(
                    "[HUDFileBrowserHelper] Unable to get selected file path.");
                return;
            }

            if (selectedFile is HUDType)
            {
                HudManager.instance.Load((HUDType) selectedFile);
                successEvent.Invoke();
            }
        }

        public void SaveHUDFile()
        {
            HUDType hud = HudManager.instance.Save();

            SchemaHandler.WriteXML(fileBrowserManager.GetSaveFilePath(), hud);
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
                        "[HUDFileBrowserHelper] Unable to find FileBrowserManager.");
                }
            }

            fileBrowserManager.OpenDirectory(ConfigurationManager.instance.defaultHUDDirectory);
        }
    }
}