// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class HUDFileBrowserHelper : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(HUDFileBrowserHelper);

        public FileBrowserManager fileBrowserManager;

        public UnityEvent successEvent;
        public UnityEvent failEvent;

        public void OpenHUDFile()
        {
            if (!fileBrowserManager)
            {
                Debug.LogWarning(
                    "[" + ClassName + "] FileBrowserManager not set.");
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                Debug.LogWarning(
                    "[" + ClassName + "] No valid file selected.");
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogWarning(
                    "[" + ClassName + "] Unable to get selected file path.");
                return;
            }

            if (selectedFile is HUDType)
            {
                MRET.HudManager.Load((HUDType) selectedFile);
                successEvent.Invoke();
            }
        }

        public void SaveHUDFile()
        {
            HUDType hud = MRET.HudManager.Save();

            SchemaHandler.WriteXML(fileBrowserManager.GetSaveFilePath(), hud);
            successEvent.Invoke();
        }

        protected override void MRETStart()
        {
            base.MRETStart();

            if (fileBrowserManager == null)
            {
                fileBrowserManager = GetComponent<FileBrowserManager>();
                if (fileBrowserManager == null)
                {
                    Debug.LogWarning(
                        "[" + ClassName + "] Unable to find FileBrowserManager.");
                }
            }

            fileBrowserManager.OpenDirectory(MRET.ConfigurationManager.defaultHUDDirectory);
        }
    }
}