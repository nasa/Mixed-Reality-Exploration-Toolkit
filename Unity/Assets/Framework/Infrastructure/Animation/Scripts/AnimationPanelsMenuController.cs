using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class AnimationPanelsMenuController : MonoBehaviour
{
    public GameObject animationPanel, loadPanel, savePanel;

    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        UnityProject proj = FindObjectOfType<UnityProject>();
        if (proj)
        {
            transform.SetParent(proj.animationPanelsContainer.transform);
        }
        OpenMainPanel();
    }

    public void OpenMainPanel()
    {
        animationPanel.SetActive(true);
        loadPanel.SetActive(false);
        savePanel.SetActive(false);
    }

    public void OpenLoadPanel()
    {
        animationPanel.SetActive(false);
        loadPanel.SetActive(true);
        savePanel.SetActive(false);
    }

    public void OpenSavePanel()
    {
        animationPanel.SetActive(false);
        loadPanel.SetActive(false);
        savePanel.SetActive(true);
    }
}