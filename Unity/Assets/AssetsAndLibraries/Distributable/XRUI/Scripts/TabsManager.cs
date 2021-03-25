// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TabsManager : MonoBehaviour
{
    public List<Button> tabButtons = new List<Button>();
    public List<GameObject> tabPanels = new List<GameObject>();

    private int lastHighlightedTabIndex = -1;
    private ColorBlock lastHighlightedTabColorBlock;

	void Start ()
    {
        SetActiveTab(0);
	}

    public void SetActiveTab(int tabNumber)
    {
        RevertLastActiveTab();
        if (tabButtons[tabNumber])
        {
            lastHighlightedTabIndex = tabNumber;
            lastHighlightedTabColorBlock = tabButtons[tabNumber].colors;
            ColorBlock tempColors = tabButtons[tabNumber].colors;
            tempColors.normalColor = tabButtons[tabNumber].colors.pressedColor;
            tabButtons[tabNumber].colors = tempColors;
        }

        for (int i = 0; i < tabPanels.Count; i++)
        {
            if (i != tabNumber)
            {
                tabPanels[i].SetActive(false);
            }
        }

        if (tabPanels[tabNumber])
        {
            tabPanels[tabNumber].SetActive(true);
        }
    }

    private void RevertLastActiveTab()
    {
        if (lastHighlightedTabIndex != -1)
        {
            tabButtons[lastHighlightedTabIndex].colors = lastHighlightedTabColorBlock;
            lastHighlightedTabIndex = -1;
        }
    }
}