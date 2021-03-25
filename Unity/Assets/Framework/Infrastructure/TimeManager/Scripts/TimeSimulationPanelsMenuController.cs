// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GSFC.ARVR.MRET.Time.Simulation
{
    /**
     * Time simulation panels menu controller
     * 
     * Controls the state of the panels associated with the time simulation.<br>
     * 
     * @author Jeffrey Hosler
     */
    public class TimeSimulationPanelsMenuController : MonoBehaviour
    {
        public static readonly string NAME = nameof(TimeSimulationPanelsMenuController);

        [Tooltip("The time panel to display that allows the user to enter the time simulation information")]
        public GameObject timeSimulationPanel;

        [Tooltip("The loading panel to display that allows the user to select a time simulation file containing the time simulation information")]
        public GameObject loadPanel;

        [Tooltip("The save panel to display that allows the user to save out the time simulation information to a file")]
        public GameObject savePanel;

        /**
         * Start is called before the first frame update
         */
        public void Start()
        {
            // Perform the initialization
            Initialize();
        }

        /**
         * Performs the initialization, checking for valid references
         */
        public void Initialize()
        {
            bool errorState = false;

            // Validate properties
            if (timeSimulationPanel == null)
            {
                Debug.LogError("[" + NAME + "]: Time Simulation Panel is not assigned.");
                errorState = true;
            }
            if (loadPanel == null)
            {
                Debug.LogError("[" + NAME + "]: Load Panel is not assigned.");
                errorState = true;
            }
            if (savePanel == null)
            {
                Debug.LogError("[" + NAME + "]: Save Panel is not assigned.");
                errorState = true;
            }

            // Open up the main time panel
            if (!errorState)
            {
                OpenMainPanel();
            }
        }

        /**
         * Opens the main panel, disabling the other panels
         */
        public void OpenMainPanel()
        {
            timeSimulationPanel.SetActive(true);
            loadPanel.SetActive(false);
            savePanel.SetActive(false);
        }

        /**
         * Opens the load panel, disabling the other panels
         */
        public void OpenLoadPanel()
        {
            timeSimulationPanel.SetActive(false);
            loadPanel.SetActive(true);
            savePanel.SetActive(false);
        }

        /**
         * Opens the save panel, disabling the other panels
         */
        public void OpenSavePanel()
        {
            timeSimulationPanel.SetActive(false);
            loadPanel.SetActive(false);
            savePanel.SetActive(true);
        }
    }

}