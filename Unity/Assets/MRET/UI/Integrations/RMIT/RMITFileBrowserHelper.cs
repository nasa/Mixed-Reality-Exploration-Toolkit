// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.Integrations.RMIT;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.Integrations.RMIT
{
    public class RMITFileBrowserHelper : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(RMITFileBrowserHelper);

        public FileBrowserManager fileBrowserManager;
        public RMITController rmitController;
        public RMITPanelMenuController menuPanelController;

        public void OpenRMITFile()
        {
            if (!fileBrowserManager)
            {
                LogWarning("File Browser Manager not set.", nameof(OpenRMITFile));
                return;
            }

            if (!rmitController)
            {
                LogWarning("Menu Controller not set.", nameof(OpenRMITFile));
                return;
            }

            // Assign the source file
            rmitController.SourceFile = fileBrowserManager.GetSelectedFilePath();

            // update the current file browser text
            menuPanelController.UpdateText(fileBrowserManager.GetSelectedFilePath());
        }
    }
}