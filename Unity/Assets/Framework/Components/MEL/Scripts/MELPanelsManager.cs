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