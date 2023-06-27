// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine.Events;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.Time
{
    /// <remarks>
    /// History:
    /// 3 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
    /// <summary>
    /// Time simulation file browser helper
    /// 
    /// Implements the file browser specifics for opening and saving time simulation files.<br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public class TimeSimulationFileBrowserHelper : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TimeSimulationFileBrowserHelper);

        public FileBrowserManager fileBrowserManager;
        public TimeSimulationMenuController menuController;

        public UnityEvent successEvent;
        public UnityEvent failEvent;

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            if (fileBrowserManager == null)
            {
                fileBrowserManager = GetComponent<FileBrowserManager>();
                if (fileBrowserManager == null)
                {
                    LogWarning("Unable to obtain a reference to the File Browser Manager");
                }
            }
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            if (fileBrowserManager != null)
            {
                fileBrowserManager.OpenDirectory(MRET.ConfigurationManager.defaultTimeSimulationDirectory);
            }
        }

        /// <summary>
        /// Opens a time simulation file and loads the settings into the menu controller
        /// </summary>
        public void OpenTimeSimulationFile()
        {
            if (!fileBrowserManager)
            {
                LogWarning("File Browser Manager not set.", nameof(OpenTimeSimulationFile));
                return;
            }

            if (!menuController)
            {
                LogWarning("Menu Controller not set.", nameof(OpenTimeSimulationFile));
                return;
            }

            // Attempt to deserialize the file into our serialized type object
            object serializedType = fileBrowserManager.GetSelectedFile();
            if (serializedType == null)
            {
                LogWarning("A problem occurred attempting to process the selected file.", nameof(OpenTimeSimulationFile));
                return;
            }

            if (serializedType is TimeSimulationType)
            {
                // Load the time simulation
                menuController.LoadTimeSimulation(serializedType as TimeSimulationType);
                successEvent.Invoke();
            }
        }

        /// <summary>
        /// Saves time simulation settings from the menu controller to an XML file
        /// </summary>
        public void SaveTimeSimulationFile()
        {
            if (!menuController)
            {
                LogWarning("Menu Controller not set.", nameof(SaveTimeSimulationFile));
                return;
            }

            // Obtain the current time simulation
            TimeSimulationType timeSimulationType = menuController.GetTimeSimulation();

            // Serialize out the time simulation to the file
            TimeSimulationFileSchema.ToXML(fileBrowserManager.GetSaveFilePath(), timeSimulationType);
            successEvent.Invoke();
        }

    }

}