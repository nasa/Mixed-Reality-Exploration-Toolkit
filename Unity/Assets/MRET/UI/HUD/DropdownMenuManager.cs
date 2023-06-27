// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.HUD;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class DropdownMenuManager : MonoBehaviour
    {
        public ScrollListManager projectListDisplay;

        private List<ProjectInfo> projects;
        private int currentSelection = -1;

        void Start()
        {
            projectListDisplay.SetTitle("Project Name");
            PopulateScrollList();
        }

        public void Open()
        {
            ModeNavigator modeNavigator = FindObjectOfType<ModeNavigator>();

            if (modeNavigator && currentSelection > -1)
            {
                modeNavigator.OpenProject(projects[currentSelection], false);
            }

            WorldSpaceMenuManager menuMan = GetComponent<WorldSpaceMenuManager>();
            if (menuMan)
            {
                menuMan.DimMenu();
            }
        }

        private void PopulateScrollList()
        {
            projectListDisplay.ClearScrollList();
            ConfigurationManager configManager = FindObjectOfType<ConfigurationManager>();
            if (configManager)
            {
                /*projects = configManager.projects;
                for (int i = 0; i < projects.Count; i++)
                {
                    int indexToSelect = i;
                    UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                    clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                    projectListDisplay.AddScrollListItem(projects[i].name, clickEvent);
                }*/
            }
        }

        private void SetActiveSelection(int listID)
        {
            currentSelection = listID;
            projectListDisplay.HighlightItem(listID);
        }

        private void OnEnable()
        {
            PopulateScrollList();
        }
    }
}