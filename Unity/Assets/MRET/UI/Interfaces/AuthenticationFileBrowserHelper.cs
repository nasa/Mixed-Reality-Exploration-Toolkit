// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.Interfaces
{
    public class AuthenticationFileBrowserHelper : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AuthenticationFileBrowserHelper);

        public const string DEFAULT_SERVER_CERT_FILE_EXTENSION = ".CRT";
        public const string DEFAULT_CLIENT_CERT_FILE_EXTENSION = ".PFX";

        public FileBrowserManager fileBrowserManager;
        public UnityEvent successEvent;
        public UnityEvent failEvent;

        public void OpenAuthenticationFile()
        {
            if (!fileBrowserManager)
            {
                LogWarning("FileBrowserManager not set.", nameof(OpenAuthenticationFile));
                failEvent?.Invoke();
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                LogWarning("Unable to get selected file path.", nameof(OpenAuthenticationFile));
                failEvent?.Invoke();
                return;
            }

            if (string.Equals(
                selectedFilePath.Substring(selectedFilePath.Length-4),
                fileBrowserManager.usedFileExtension, System.StringComparison.OrdinalIgnoreCase))
            {
                // Server cert
                if (fileBrowserManager.usedFileExtension.ToUpper() == DEFAULT_SERVER_CERT_FILE_EXTENSION)
                {
                    if (ProjectManager.InterfaceManager.AddServerCert(selectedFilePath))
                    {
                        ProjectManager.InterfaceManager.Connect();
                        successEvent?.Invoke();
                    }
                    else
                    {
                        LogWarning("There was a problem adding the selected server cert.", nameof(OpenAuthenticationFile));
                        failEvent?.Invoke();
                    }
                }

                // Client
                if (fileBrowserManager.usedFileExtension.ToUpper() == DEFAULT_CLIENT_CERT_FILE_EXTENSION)
                {
                    if (ProjectManager.InterfaceManager.AddClientCert(selectedFilePath))
                    {
                        ProjectManager.InterfaceManager.Connect();
                        successEvent?.Invoke();
                    }
                    else
                    {
                        LogWarning("There was a problem adding the selected client cert.", nameof(OpenAuthenticationFile));
                        failEvent?.Invoke();
                    }
                }
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
                    LogWarning("Unable to find FileBrowserManager.", nameof(MRETStart));
                }
            }

            // Set the file browser to the default user directory
            fileBrowserManager.OpenDirectory(MRET.ConfigurationManager.defaultUserDirectory);
        }
    }
}