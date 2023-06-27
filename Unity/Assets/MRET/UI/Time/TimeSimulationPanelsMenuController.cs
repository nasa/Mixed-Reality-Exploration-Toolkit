// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.Time
{
    /// <summary>
    /// Time simulation panels menu controller
    /// 
    /// Controls the state of the panels associated with the time simulation.<br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TimeSimulationPanelsMenuController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TimeSimulationPanelsMenuController);

        [Tooltip("The time panel to display that allows the user to enter the time simulation information")]
        public GameObject timeSimulationPanel;

        [Tooltip("The loading panel to display that allows the user to select a time simulation file containing the time simulation information")]
        public GameObject loadPanel;

        [Tooltip("The save panel to display that allows the user to save out the time simulation information to a file")]
        public GameObject savePanel;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Perform the initialization
            Initialize();
        }

        /// <summary>
        /// Performs the initialization, checking for valid references
        /// </summary>
        public void Initialize()
        {
            bool errorState = false;

            // Validate properties
            if (timeSimulationPanel == null)
            {
                LogError("Time Simulation Panel is not assigned.", nameof(Initialize));
                errorState = true;
            }
            if (loadPanel == null)
            {
                LogError("Load Panel is not assigned.", nameof(Initialize));
                errorState = true;
            }
            if (savePanel == null)
            {
                LogError("Save Panel is not assigned.", nameof(Initialize));
                errorState = true;
            }

            // Open up the main time panel
            if (!errorState)
            {
                OpenMainPanel();
            }
        }

        /// <summary>
        /// Opens the main panel, disabling the other panels
        /// </summary>
        public void OpenMainPanel()
        {
            timeSimulationPanel.SetActive(true);
            loadPanel.SetActive(false);
            savePanel.SetActive(false);
        }

        /// <summary>
        /// Opens the load panel, disabling the other panels
        /// </summary>
        public void OpenLoadPanel()
        {
            timeSimulationPanel.SetActive(false);
            loadPanel.SetActive(true);
            savePanel.SetActive(false);
        }

        /// <summary>
        /// Opens the save panel, disabling the other panels
        /// </summary>
        public void OpenSavePanel()
        {
            timeSimulationPanel.SetActive(false);
            loadPanel.SetActive(false);
            savePanel.SetActive(true);
        }
    }

}