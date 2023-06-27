// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part.MEL
{
    public class MELPanelsManager : MonoBehaviour
    {
        public GameObject melGenerationPanel, melPanel;
        public MELMenuManager melMenuManager;

        void OnEnable()
        {
            OpenMELGenerationPanel();
        }

        public void OpenMELGenerationPanel()
        {
            melGenerationPanel.SetActive(true);
            melPanel.SetActive(false);
        }

        public void OpenMELPanel()
        {
            melGenerationPanel.SetActive(false);
            melPanel.SetActive(true);
            melMenuManager.Generate();
        }
    }
}