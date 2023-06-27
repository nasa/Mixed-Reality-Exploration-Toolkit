// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.UI.Integrations.RMIT
{
    public class RMITPanelMenuController : MonoBehaviour
    {
        public static readonly string NAME = nameof(RMITPanelMenuController);
        public GameObject rmitSimulationPanel;
        public GameObject loadPanel;
        public Text selectedFileText;

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
            if (rmitSimulationPanel == null)
            {
                Debug.LogError("[" + NAME + "]: RMIT Simulation Panel is not assigned.");
                errorState = true;
            }
            if (loadPanel == null)
            {
                Debug.LogError("[" + NAME + "]: Load Panel is not assigned.");
                errorState = true;
            }

            // Open up the main RMIT panel
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
            rmitSimulationPanel.SetActive(true);
            loadPanel.SetActive(false);
        }

        /**
         * Opens the load panel, disabling the other panels
         */
        public void OpenLoadPanel()
        {
            rmitSimulationPanel.SetActive(false);
            loadPanel.SetActive(true);
        }

        public void CloseLoadPanel()
        {
            rmitSimulationPanel.SetActive(true);
            loadPanel.SetActive(false);
        }

        public void UpdateText(string newtext) 
        {
            selectedFileText.text = newtext;
        }
    }

}