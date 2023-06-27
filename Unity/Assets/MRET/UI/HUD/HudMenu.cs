// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu;
using GOV.NASA.GSFC.XR.MRET.HUD;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Panel;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class HudMenu : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(HudMenu);

        /// <summary>
        /// Script that handles the mini HUD. Saves to the HudManager whenever navigated away from the EditSubMenu. 
        /// Loads from the HudManager whenever navigated to the EditSubMenu. Contains methods to add new displays and push them to the HUD. 
        /// </summary>

        [Tooltip("menu panel that holds mini HUD edit menu, use the edit sub menu panel of the HUDMenu")]
        public GameObject editSubMenu;
        [Tooltip("GameObject that is the parent to the mini HUD elements, located above canvases")]
        public GameObject miniHUD;
        [Tooltip("List of HUD displays")]
        public List<GameObject> localDisplayList = new List<GameObject>();
        [Tooltip("List for selecting groups of displays, not yet implemented")]
        public List<GameObject> selectedDisplays = new List<GameObject>();
        [Tooltip("Button to toggle hud on and off")]
        public Toggle hudToggle;
        [Tooltip("Adjust the scale of the HUD")]
        public Vector3 scale = new Vector3(0.86f, 0.86f, 0.86f);
        private HudManager hudManager;
        private bool updated = false;
        private Stack<HudAction> previousActions = new Stack<HudAction>();
        private Stack<HudAction> undoneActions = new Stack<HudAction>();

        enum HudAction
        {
            Add,
            Update,
            Toggle,
        }

        #region MRETBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            scale = new Vector3(0.86f, 0.86f, 0.86f);
            GameObject hudManagerObject = MRET.HudManager.gameObject;
            hudManager = MRET.HudManager;
            this.transform.SetParent(MRET.HudManager.transform);
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            //SaveHud();
            foreach (WorldSpaceMenuLoader menuLoader in transform.GetComponentsInChildren<WorldSpaceMenuLoader>(true))
            {
                menuLoader.DestroyMenu();
            }
        }
        #endregion MRETBehaviour

        // Gets the HUD from the hudManager
        public void GetHud()
        {
            ClearList();
            foreach (PanelType display in hudManager.hudPanels)
            {
                GetPanel();
            }
        }

        // Saves the HUD to the hudManager
        public void SaveHud()
        {
            hudManager.hudPanels.Clear();

            foreach (GameObject display in localDisplayList)
            {
                SaveDisplay(localDisplayList.IndexOf(display));
            }
        }

        // Clears the list of local displays
        public void ClearList()
        {
            foreach (GameObject display in localDisplayList)
            {
                Destroy(display);
            }
            localDisplayList.Clear();
        }

        // Called a bunch of times by GetHUD to get each display from the hudManager list
        public void GetPanel()
        {
            // Deserialize the serialized panel from our panel list into our display
            localDisplayList.Add(Instantiate(hudManager.displayPrefab, miniHUD.transform)); // has index displayNum
            int displayNum = localDisplayList.Count - 1;
            PanelType serializedPanel = hudManager.hudPanels[displayNum];
            GameObject display = localDisplayList[displayNum];

            // Add the panel component
            InteractablePanel panel = display.GetComponent<InteractablePanel>();
            if (panel == null)
            {
                panel = display.AddComponent<InteractablePanel>();
            }

            // TODO: The panel transform should be defined properly in the XML to be
            // "Local/Relative". If not, it will be placed at a global position. We
            // cannot assume what the user wants. They have to be specific in the XML.

            // Deserialize the panel
            panel.Deserialize(serializedPanel);

            // Now setup the feed and display
            PanelSwitcher feedSwitch = display.GetComponent<PanelSwitcher>();
            DisplayController dispControl = display.GetComponent<DisplayController>();

            // Display controller
            dispControl.type = DisplayController.Type.miniDisplay;
            dispControl.SetType();
            dispControl.displayNumber = displayNum;
            // Make sure the panel shares the display number for later serialization
            panel.Zorder = displayNum;

            // Feed Switcher
            FeedSource feedSource = panel.feedSource;
            feedSwitch.feedSource = feedSource;
            feedSwitch.Initialized = true;
            feedSwitch.SetPanelSource(feedSource);
        }

        // Called a bunch of times by SaveHud to save each display to the hudManager list
        void SaveDisplay(int displayNum)
        {
            // Serialize the panel in our display out into our panel list
            GameObject display = localDisplayList[displayNum];

            // Get the panel component
            InteractablePanel panel = display.GetComponent<InteractablePanel>();
            if (panel == null)
            {
                // This will be an empty panel, so we should warn the user
                panel = display.AddComponent<InteractablePanel>();
                LogWarning("The display panel component did not exist, so a default panel will be serialized.");
            }

            // Create the serialized representation of the panel
            PanelType serializedPanel = panel.CreateSerializedType();
            Action<bool, string> OnPanelSerializedAction = (bool successful, string message) =>
            {
                if (successful)
                {
                    // Override the zorder (just in case)
                    serializedPanel.Zorder = displayNum;

                    PanelSwitcher feedSwitch = display.GetComponent<PanelSwitcher>();
                    DisplayController dispControl = display.GetComponent<DisplayController>();

                    // Save the serialized panel
                    hudManager.hudPanels.Add(serializedPanel);
                }
                else
                {
                    LogWarning("There was a problem serializing the HUD panel: " + message);
                }
            };

            // Serialize the panel
            panel.Serialize(serializedPanel, OnPanelSerializedAction);
        }

        public void HideMenu()
        {
            SaveHud();
            gameObject.SetActive(false);
        }

        // Pushes the HUD from the mini HUD to the HUD
        public void UpdateHUD()
        {
            hudManager.HUD = Instantiate(miniHUD, hudManager.frameHUD.transform);
            hudManager.HUD.transform.localPosition = Vector3.zero;
            hudManager.HUD.transform.localRotation = Quaternion.identity;
            hudManager.HUD.transform.localScale = scale;
            Destroy(hudManager.HUD.GetComponent<Image>());

            DisplayController[] hudList = hudManager.HUD.GetComponentsInChildren<DisplayController>();
            foreach (DisplayController display in hudList)
            {
                display.type = DisplayController.Type.HUDDisplay;
                display.SetType(); // SetType only affects interaction and settings menus, not whats being displayed
                display.gameObject.GetComponent<PanelSwitcher>().Initialized = true;
                display.gameObject.GetComponent<PanelSwitcher>().SetPanelSource(display.gameObject.GetComponent<PanelSwitcher>().feedSource);
            }

            updated = true;
        }

        // Add a display to the mini HUD
        public void Add()
        {
            localDisplayList.Add(Instantiate(hudManager.displayPrefab, miniHUD.transform));
            int displayNum = localDisplayList.Count - 1;
            localDisplayList[displayNum].transform.localRotation = Quaternion.identity;

            DisplayController Display = localDisplayList[displayNum].GetComponent<DisplayController>();
            Display.displayNumber = displayNum;
            Display.type = DisplayController.Type.miniDisplay;
            Display.SetType();
        }

        public void Delete()
        {
            foreach (var deleteDisplay in selectedDisplays)
            {
                hudManager.hudPanels.RemoveAt(selectedDisplays.IndexOf(deleteDisplay));
            }
        }

        public void ToggleHud()
        {
            SwitchHUD(hudToggle.isOn);
        }

        public void SwitchHUD(bool on)
        {
            hudToggle.isOn = on;
            if (on)
            {
                UpdateHUD();
            }
            else
            {
                Destroy(hudManager.HUD);
            }
        }

        public void ThresholdSlide(Slider slider)
        {
            //hudManager.frameHUD.GetComponentInParent<FrameHUD>().threshold = slider.value;
        }

        public void SpeedSlide(Slider slider)
        {
            //hudManager.frameHUD.GetComponentInParent<FrameHUD>().speed = slider.value;
        }

        public void Select()
        {
        }

        // Feature to allow the user to undo/redo changes made to the HUD.
        public void Undo()
        {
        }

        public void Redo()
        {
        }

    }
}