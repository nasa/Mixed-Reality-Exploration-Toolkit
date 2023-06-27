// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.HUD;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class HudOpenMenuManager : MonoBehaviour
    {
        public ScrollListManager hudListDisplay;

        private HudManager hudManager;
        private List<HudInfo> huds;
        private int currentSelection = -1;

        private HudMenu hudMenu;

        void Start()
        {
            hudManager = FindObjectOfType<HudManager>();
            hudMenu = FindObjectOfType<HudMenu>();
            hudMenu.gameObject.SetActive(false);
            hudListDisplay.SetTitle("Heads-Up Displays");
            PopulateScrollList();
        }

        private void OnDisable()
        {
            hudMenu.gameObject.SetActive(true);
            Destroy(gameObject);
        }



        public void Open()
        {
            hudMenu.gameObject.SetActive(true);
            if (currentSelection > -1)
            {
                hudManager.LoadFromXML(huds[currentSelection].hudFile);
            }
            Destroy(gameObject);
        }

        public void PopulateScrollList()
        {
            hudListDisplay.ClearScrollList();
            ConfigurationManager configManger = FindObjectOfType<ConfigurationManager>();
            if (configManger)
            {
                /*(huds = configManger.huds;
                for(int i=0; i < huds.Count; i++)
                {
                    int indexToSelect = i;
                    UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                    clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                    hudListDisplay.AddScrollListItem(huds[i].name, clickEvent);
                }*/
            }
        }

        public void SetActiveSelection(int listID)
        {
            currentSelection = listID;
            hudListDisplay.HighlightItem(listID);
        }

        private void OnEnable()
        {
            PopulateScrollList();
        }

    }
}