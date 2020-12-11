using UnityEngine;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Common.Schemas;

public class ProjectCreateMenuManager : MonoBehaviour
{
    public ScrollListManager templateListDisplay;

    private UnityProject projectManager;
    private List<ProjectInfo> templates;
    private int currentSelection = -1;

    void Start()
    {
        projectManager = FindObjectOfType<UnityProject>();

        templateListDisplay.SetTitle("Environments");
        PopulateScrollList();
    }

    public void Create()
    {
        ModeNavigator modeNavigator = FindObjectOfType<ModeNavigator>();

        if (modeNavigator && currentSelection > -1)
        {
            modeNavigator.CreateProject(templates[currentSelection], false);
        }

        WorldSpaceMenuManager menuMan = GetComponent<WorldSpaceMenuManager>();
        if (menuMan)
        {
            menuMan.DimMenu();
        }
    }

    private void PopulateScrollList()
    {
        templateListDisplay.ClearScrollList();
        ConfigurationManager configManager = FindObjectOfType<ConfigurationManager>();
        if (configManager)
        {
            templates = configManager.templates;
            for (int i = 0; i < templates.Count; i++)
            {
                int indexToSelect = i;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                templateListDisplay.AddScrollListItem(templates[i].name, clickEvent);
            }
        }
    }

    private void SetActiveSelection(int listID)
    {
        currentSelection = listID;
        templateListDisplay.HighlightItem(listID);
    }

    private void OnEnable()
    {
        PopulateScrollList();
    }
}