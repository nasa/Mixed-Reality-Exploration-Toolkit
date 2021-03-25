// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Common;
using GSFC.ARVR.MRET.Common.Schemas.TimeSimulationTypes;

namespace GSFC.ARVR.MRET.Time.Simulation
{
    /**
     * Time simulation file browser helper
     * 
     * Implements the file browser specifics for opening and saving time simulation files.<br>
     * 
     * @author Jeffrey Hosler
     */
    public class TimeSimulationFileBrowserHelper : MonoBehaviour
    {
        public static readonly string NAME = nameof(TimeSimulationFileBrowserHelper);

        public FileBrowserManager fileBrowserManager;
        public TimeSimulationMenuController menuController;

        public UnityEvent successEvent;
        public UnityEvent failEvent;

        /**
         * Opens a time simulation file and loads the settings into the menu controller
         */
        public void OpenTimeSimulationFile()
        {
            if (!fileBrowserManager)
            {
                Debug.LogWarning("[" + NAME + "] File Browser Manager not set.");
                return;
            }

            if (!menuController)
            {
                Debug.LogWarning("[" + NAME + "] Menu Controller not set.");
                return;
            }

            // Attempt to deserialize the file into our serialized type object
            object serializedType = fileBrowserManager.GetSelectedFile();
            if (serializedType == null)
            {
                Debug.LogWarning("[" + NAME + "] A problem occurred attempting to process the selected file");
                return;
            }

            if (serializedType is TimeSimulationType)
            {
                // Load the time simulation
                menuController.LoadTimeSimulation(serializedType as TimeSimulationType);
                successEvent.Invoke();
            }
        }

        /**
         * Saves time simulation settings from the menu controller to an XML file
         */
        public void SaveTimeSimulationFile()
        {
            if (!menuController)
            {
                Debug.LogWarning("[" + NAME + "] Menu Controller not set.");
                return;
            }

            // Obtain the current time simulation
            TimeSimulationType timeSimulationType = menuController.GetTimeSimulation();

            // Serialize out the time simulation to the file
            TimeSimulationFileSchema.ToXML(fileBrowserManager.GetSaveFilePath(), timeSimulationType);
            successEvent.Invoke();
        }

        /**
         * Awake is called once during initialization
         */
        private void Awake()
        {
            if (fileBrowserManager == null)
            {
                fileBrowserManager = GetComponent<FileBrowserManager>();
                if (fileBrowserManager == null)
                {
                    Debug.LogWarning("[" + NAME + "] Unable to obtain a reference to the File Browser Manager");
                }
            }
        }

        /**
         * Start is called before the first frame update
         */
        private void Start()
        {
            if (fileBrowserManager != null)
            {
                fileBrowserManager.OpenDirectory(ConfigurationManager.instance.defaultTimeSimulationDirectory);
            }
        }
    }

}