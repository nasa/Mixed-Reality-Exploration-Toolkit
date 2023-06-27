// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// MarkerFileBrowserHelper
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public class MarkerFileBrowserHelper : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(MarkerFileBrowserHelper);

        public FileBrowserManager fileBrowserManager;

        public void OpenMarkerFile()
        {
            if (!fileBrowserManager)
            {
                LogWarning("FileBrowserManager not set.", nameof(OpenMarkerFile));
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                Debug.LogWarning("No valid file selected.");
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogWarning("Unable to get selected file path.");
                return;
            }

            if (selectedFile is MarkerType)
            {
                ProjectManager.MarkerManager.InstantiateMarker((MarkerType)selectedFile, true);
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
                    LogWarning("Unable to find FileBrowserManager reference", nameof(MRETStart));
                }
            }

            fileBrowserManager.OpenDirectory(MRET.ConfigurationManager.defaultMarkerDirectory);
        }
    }
}
