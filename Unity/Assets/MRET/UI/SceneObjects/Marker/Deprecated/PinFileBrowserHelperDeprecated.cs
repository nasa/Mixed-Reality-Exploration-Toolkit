// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.IO;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

/// <remarks>
/// History:
/// 3 October 2022: Created
/// </remarks>
///
/// <summary>
/// PinFileBrowserHelperDeprecated
///
/// Helps file browser find .mpin files
/// <Works with cref="FileBrowserManager"/>
///
/// Author: Sean Letavish
/// </summary>
/// 
namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Pin
{
    public class PinFileBrowserHelperDeprecated : MonoBehaviour
    {
        [Tooltip("Filebrowser manager: should also be attached to object")]
        public FileBrowserManager fileBrowserManager;

        //used to set pin name on pin panel
        PinPanelControllerDeprecated ppc;

        //Public strings to provide name and file path 
        public static string selectedPinName;

        public static object selectedPinFile;



        public void OpenPartFile()
        {
            if (!fileBrowserManager)
            {
                Debug.LogWarning(
                    "[PartFileBrowserHelper] FileBrowserManager not set.");
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            selectedPinFile = selectedFile;
            if (selectedFile == null)
            {
                Debug.LogWarning(
                    "[PartFileBrowserHelper] No valid file selected.");
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            selectedPinName = Path.GetFileName(selectedFilePath);
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogWarning(
                    "[PartFileBrowserHelper] Unable to get selected file path.");
                return;
            }

            if (selectedFile is PartType)
            {
                if (!PinPanelControllerDeprecated.setPinEnabled)
                {
                    foreach (PinMarkerDeprecated pm in MRET.InputRig.GetComponentsInChildren<PinMarkerDeprecated>())
                    {
                        Destroy(pm.gameObject);
                    }

                    ProjectManager.PinManagerDeprecated.InstantiatePin((PartType)selectedFile, null); //FIXME: PartType to be replaced with Pin/Marker Type
                    ProjectManager.PinManagerDeprecated.InstantiatePin((PartType)selectedFile, null);
                    ppc = MRET.InputRig.GetComponentInChildren<PinPanelControllerDeprecated>();
                    ppc.SetPinName();
                }
                else
                {
                    ProjectManager.PinManagerDeprecated.InstantiatePin((PartType)selectedFile, null);
                }
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

            fileBrowserManager.OpenDirectory(MRET.ConfigurationManager.defaultPartDirectory);
        }
    }
}
