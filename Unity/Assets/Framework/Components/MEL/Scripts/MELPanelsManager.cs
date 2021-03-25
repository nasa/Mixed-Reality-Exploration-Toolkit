// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

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