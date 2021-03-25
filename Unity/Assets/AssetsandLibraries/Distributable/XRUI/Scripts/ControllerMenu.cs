// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GSFC.ARVR.XRUI.ControllerMenu
{
    /// <remarks>
    /// History:
    /// 5 January 2021: Created
    /// </remarks>
    /// <summary>
    /// ControllerMenu is the class for managing a
    /// controller-attached menu. It can have 0 or more
    /// ControllerMenuPanel instances under it.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class ControllerMenu : MonoBehaviour
    {
        /// <summary>
        /// Prefab for a menu panel.
        /// </summary>
        [Tooltip("Prefab for a menu panel.")]
        public GameObject menuPanelPrefab;

        /// <summary>
        /// Prefab for a menu bar button.
        /// </summary>
        [Tooltip("Prefab for a menu bar button.")]
        public GameObject menuBarButtonPrefab;

        /// <summary>
        /// The parent object for menu panels.
        /// </summary>
        [Tooltip("The parent object for menu panels.")]
        public GameObject menuObject;

        /// <summary>
        /// Menu bar object.
        /// </summary>
        [Tooltip("Menu bar object.")]
        public GameObject menuBarObject;

        /// <summary>
        /// Object to rotate towards.
        /// </summary>
        [Tooltip("Object to rotate towards.")]
        public Transform rotateTowards;

        /// <summary>
        /// Event to call when menu is shown.
        /// </summary>
        [Tooltip("Event to call when menu is shown.")]
        public UnityEvent onShow;

        /// <summary>
        /// Event to call when menu is hidden.
        /// </summary>
        [Tooltip("Event to call when menu is hidden.")]
        public UnityEvent onHide;

        /// <summary>
        /// Position for the active panel.
        /// </summary>
        [Tooltip("Position for the active panel.")]
        public Vector3 activePanelPosition = new Vector3(5556356, 162801.2f, -22473.51f);

        /// <summary>
        /// Offset for secondary panels.
        /// </summary>
        [Tooltip("Offset for secondary panels.")]
        public Vector3 secondaryPanelOffset = new Vector3(6286, 0, 0);

        /// <summary>
        /// Rotation to apply to a panel.
        /// </summary>
        [Tooltip("Rotation to apply to a panel.")]
        public Vector3 panelRotation;

        /// <summary>
        /// Scale for the active panel.
        /// </summary>
        [Tooltip("Scale for the active panel.")]
        public float activePanelScale = 66.107f;

        /// <summary>
        /// Scale for secondary panels.
        /// </summary>
        [Tooltip("Scale for secondary panels.")]
        public float secondaryPanelScale = 60;

        /// <summary>
        /// Loaded menu panels.
        /// </summary>
        private List<ControllerMenuPanel> menuPanels;

        /// <summary>
        /// Index of the active panel.
        /// </summary>
        private int activePanelIndex;

        /// <summary>
        /// Menu bar buttons.
        /// </summary>
        private List<Button> menuBarButtons;

        /// <summary>
        /// Whether or not the menu is dimmed.
        /// </summary>
        private bool menuDimmed = false;

        /// <summary>
        /// Must be called when first instantiating menu and
        /// when reinitializing it.
        /// </summary>
        public void InitializeMenu()
        {
            if (menuPanels != null)
            {
                foreach (ControllerMenuPanel panel in menuPanels)
                {
                    Destroy(panel.gameObject);
                }
            }

            menuPanels = new List<ControllerMenuPanel>();

            if (menuBarButtons != null)
            {
                foreach (Button button in menuBarButtons)
                {
                    Destroy(button);
                }
            }

            menuBarButtons = new List<Button>();
        }

        /// <summary>
        /// Toggle menu on or off.
        /// </summary>
        public void ToggleMenu()
        {
            ToggleMenu(!menuObject.activeSelf);
        }

        /// <summary>
        /// Toggle menu on or off.
        /// </summary>
        /// <param name="active">Desired state.</param>
        public void ToggleMenu(bool active)
        {
            if (active)
            {
                ShowMenu();
            }
            else
            {
                HideMenu();
            }
        }

        /// <summary>
        /// Show the menu.
        /// </summary>
        /// <param name="invokeEvent">Whether or not to invoke an OnShow event.</param>
        public void ShowMenu(bool invokeEvent = true)
        {
            if (invokeEvent)
            {
                onShow.Invoke();
            }
            menuObject.SetActive(true);
        }

        /// <summary>
        /// Hide the menu.
        /// </summary>
        /// <param name="invokeEvent">Whether or not to invoke an OnHide event.</param>
        public void HideMenu(bool invokeEvent = true)
        {
            if (invokeEvent)
            {
                onHide.Invoke();
            }
            menuObject.SetActive(false);

            // Destroy all world space menus that have been loaded.
            foreach (WorldSpaceMenuLoader menuLoader
                in transform.GetComponentsInChildren<WorldSpaceMenuLoader>(true))
            {
                menuLoader.DestroyMenu();
            }
            menuDimmed = false;
        }

        /// <summary>
        /// Dim the menu.
        /// </summary>
        public void DimMenu()
        {
            menuObject.SetActive(false);
            menuDimmed = true;
        }

        /// <summary>
        /// UnDim the menu.
        /// </summary>
        public void UnDimMenu()
        {
            if (menuDimmed)
            {
                menuObject.SetActive(true);
                menuDimmed = false;
            }
        }

        /// <summary>
        /// UnDim a specific menu.
        /// </summary>
        /// <param name="menu"></param>
        public void UnDimMenu(GameObject menu)
        {
            if (menuDimmed)
            {
                menu.SetActive(true);
                menuDimmed = false;
            }
        }

        /// <summary>
        /// Whether or not the menu is dimmed.
        /// </summary>
        /// <returns></returns>
        public bool IsDimmed()
        {
            return menuDimmed;
        }

        /// <summary>
        /// Set a menu panel to be active.
        /// </summary>
        /// <param name="panel">Reference to the panel to activate.</param>
        public void SelectMenuPanel(ControllerMenuPanel panel)
        {
            if (menuPanels.Contains(panel))
            {
                SelectMenuPanel(menuPanels.IndexOf(panel));
            }
            else
            {
                Debug.LogError("ControllerMenu: Panel not in menu.");
            }
        }

        /// <summary>
        /// Set a menu panel to be active.
        /// </summary>
        /// <param name="index">Index of the panel to activate.</param>
        public void SelectMenuPanel(int index)
        {
            if (index > activePanelIndex)
            {
                for (int i = activePanelIndex; i < index; i++)
                {
                    NextMenuPanel();
                }
            }
            else if (index < activePanelIndex)
            {
                for (int i = activePanelIndex; i > index; i--)
                {
                    PreviousMenuPanel();
                }
            }
        }

        /// <summary>
        /// Go to the previous menu panel.
        /// </summary>
        public void PreviousMenuPanel()
        {
            if (activePanelIndex - 1 < 0)
            {
                //Debug.LogError("ControllerMenu: Can't go to previous menu panel.");
                return;
            }

            // Animate panel transition.
            StartCoroutine(SwitchPanel(true));

            // Select the corresponding menu bar button.
            SelectMenuBarButton(--activePanelIndex);
        }

        /// <summary>
        /// Go to the next menu panel.
        /// </summary>
        public void NextMenuPanel()
        {
            if (activePanelIndex + 1 > menuPanels.Count - 1)
            {
                //Debug.LogError("ControllerMenu: Can't go to next menu panel.");
                return;
            }

            // Animate panel transition.
            StartCoroutine(SwitchPanel(false));

            // Select the corresponding menu bar button.
            SelectMenuBarButton(++activePanelIndex);
        }

        /// <summary>
        /// Add a menu panel.
        /// </summary>
        /// <param name="panelName">Name of the panel.</param>
        /// <param name="homeButton">Whether or not to show the home button.</param>
        /// <param name="previousButton">Whether or not to show the previous button.</param>
        /// <param name="nextButton">Whether or not to show the next button.</param>
        /// <returns>A reference to the controller menu panel.</returns>
        public ControllerMenuPanel AddMenuPanel(string panelName, bool homeButton = false,
            bool previousButton = true, bool nextButton = true, UnityEvent onEnableEvent = null)
        {
            // Load prefab.
            GameObject newPanel = Instantiate(menuPanelPrefab);
            newPanel.transform.SetParent(menuObject.transform);
            newPanel.transform.localRotation = Quaternion.Euler(panelRotation);

            if (newPanel == null)
            {
                Debug.LogError("ControllerMenu: Error adding panel.");
                return null;
            }

            ControllerMenuPanel menuPanel = newPanel.GetComponent<ControllerMenuPanel>();
            if (menuPanel == null)
            {
                Debug.LogError("ControllerMenu: Panel script not found.");
                return null;
            }

            // Initialize.
            menuPanel.panelName = panelName;
            menuPanel.Initialize(homeButton, previousButton, nextButton);

            // Set previous, next buttons.
            menuPanel.previousButton.onClick = new Button.ButtonClickedEvent();
            menuPanel.previousButton.onClick.AddListener(() => { PreviousMenuPanel(); });
            menuPanel.nextButton.onClick = new Button.ButtonClickedEvent();
            menuPanel.nextButton.onClick.AddListener(() => { NextMenuPanel(); });

            menuPanels.Add(menuPanel);

            // Add menu bar button.
            AddMenuBarButton(menuPanels.IndexOf(menuPanel));

            // Make selected.
            SelectMenuPanel(menuPanels.IndexOf(menuPanel));

            return menuPanel;
        }

        /// <summary>
        /// Remove a menu panel.
        /// </summary>
        /// <param name="panelToRemove">A reference to the panel to remove.</param>
        public void RemoveMenuPanel(ControllerMenuPanel panelToRemove)
        {
            if (menuPanels.Contains(panelToRemove))
            {
                menuPanels.Remove(panelToRemove);
                Destroy(panelToRemove);
            }
            else
            {
                Debug.LogError("ControllerMenu: Panel not found in loaded panels.");
            }
        }

        /// <summary>
        /// Add a button to the menu bar.
        /// </summary>
        /// <param name="buttonIndex">Index to add at.</param>
        /// <returns>A reference to the new button.</returns>
        private Button AddMenuBarButton(int buttonIndex)
        {
            GameObject newMenuButtonObject = Instantiate(menuBarButtonPrefab);
            newMenuButtonObject.transform.SetParent(menuBarObject.transform);

            if (newMenuButtonObject == null)
            {
                Debug.LogError("ControllerMenu: Error adding menu button.");
                return null;
            }

            newMenuButtonObject.transform.localPosition = Vector3.zero;
            newMenuButtonObject.transform.localScale = Vector3.one;
            newMenuButtonObject.transform.localRotation = Quaternion.identity;

            Button newMenuButton = newMenuButtonObject.GetComponent<Button>();
            if (newMenuButton == null)
            {
                Debug.LogError("MenuButton: Button script not found.");
                return null;
            }

            newMenuButton.onClick.AddListener(() => SelectMenuPanel(buttonIndex));

            menuBarButtons.Add(newMenuButton);

            return newMenuButton;
        }

        /// <summary>
        /// Visually set a menu bar button to active.
        /// </summary>
        /// <param name="index">Index of the button.</param>
        private void SelectMenuBarButton(int index)
        {
            for (int i = 0; i < menuBarButtons.Count; i++)
            {
                if (i == index)
                {
                    menuBarButtons[i].interactable = false;
                }
                else
                {
                    menuBarButtons[i].interactable = true;
                }
            }
        }

        /// <summary>
        /// Ultimate zoom scale.
        /// </summary>
        private float targetZoomScale = 1f;

        /// <summary>
        /// Tween the menu in an animated fashion.
        /// </summary>
        /// <param name="show">Show or hide.</param>
        /// <returns>Context.</returns>
        private IEnumerator TweenMenuScale(bool show)
        {
            if (show)
            {
                int i = 0;
                ToggleMenu(true);
                menuObject.transform.localScale = Vector3.zero;
                Vector3 frameDifference = new Vector3(4f, 4f, 4f);
                while (i < 250 && menuObject.transform.localScale.x < targetZoomScale)
                {
                    menuObject.transform.localScale += frameDifference * Time.deltaTime * targetZoomScale;
                    yield return true;
                    i++;
                }
                menuObject.transform.localScale = new Vector3(targetZoomScale, targetZoomScale, targetZoomScale);
            }
            else
            {
                int i = 0;
                menuObject.transform.localScale = new Vector3(targetZoomScale, targetZoomScale, targetZoomScale);
                Vector3 frameDifference = new Vector3(4f, 4f, 4f);
                while (i < 250 && menuObject.transform.localScale.x > 0)
                {
                    menuObject.transform.localScale -= frameDifference * Time.deltaTime * targetZoomScale;
                    yield return true;
                    i++;
                }
                menuObject.transform.localScale = Vector3.zero;
                ToggleMenu(false);
            }
            StopCoroutine("TweenMenuScale");
        }

        /// <summary>
        /// Steps to perform each panel switch in.
        /// </summary>
        private int panelSwitchSteps = 20;

        /// <summary>
        /// Switch one panel.
        /// </summary>
        /// <param name="previous">Whether to switch to the previous panel.</param>
        /// <returns></returns>
        IEnumerator SwitchPanel(bool previous)
        {
            ControllerMenuPanel origPanel = menuPanels[activePanelIndex];
            ControllerMenuPanel finalPanel = null;
            ControllerMenuPanel otherPanel = null;

            Vector3 origPanelDest = activePanelPosition;
            Vector3 finalPanelOrig = activePanelPosition;

            if (previous)
            {
                if (activePanelIndex < 1)
                {
                    Debug.LogError("ControllerMenu: No previous panel.");
                    yield break;
                }

                finalPanel = menuPanels[activePanelIndex - 1];
                origPanelDest = new Vector3(activePanelPosition.x + secondaryPanelOffset.x,
                    activePanelPosition.y + secondaryPanelOffset.y, activePanelPosition.z + secondaryPanelOffset.z);
                finalPanelOrig = new Vector3(activePanelPosition.x - secondaryPanelOffset.x,
                    activePanelPosition.y + secondaryPanelOffset.y, activePanelPosition.z - secondaryPanelOffset.z);

                // If there is another panel to show in the background to the
                // left of the final active panel, set it up.
                if (activePanelIndex - 2 >= 0)
                {
                    otherPanel = menuPanels[activePanelIndex - 2];
                }
            }
            else
            {
                if (activePanelIndex >= menuPanels.Count)
                {
                    Debug.LogError("ControllerMenu: No next panel.");
                    yield break;
                }

                finalPanel = menuPanels[activePanelIndex + 1];
                origPanelDest = new Vector3(activePanelPosition.x - secondaryPanelOffset.x,
                    activePanelPosition.y + secondaryPanelOffset.y, activePanelPosition.z - secondaryPanelOffset.z);
                finalPanelOrig = new Vector3(activePanelPosition.x + secondaryPanelOffset.x,
                    activePanelPosition.y + secondaryPanelOffset.y, activePanelPosition.z + secondaryPanelOffset.z);

                // If there is another panel to show in the background to the
                // left of the final active panel, set it up.
                if (activePanelIndex + 2 < menuPanels.Count)
                {
                    otherPanel = menuPanels[activePanelIndex + 2];
                }
            }

            if (origPanel == null || finalPanel == null)
            {
                Debug.LogError("ControllerMenu: Error switching panels.");
                yield break;
            }

            for (int i = 0; i < panelSwitchSteps; i++)
            {
                // Position.
                origPanel.transform.localPosition = Vector3.Lerp(activePanelPosition, origPanelDest, (float) i / panelSwitchSteps);
                finalPanel.transform.localPosition = Vector3.Lerp(finalPanelOrig, activePanelPosition, (float) i / panelSwitchSteps);

                // Scale.
                origPanel.transform.localScale = Vector3.Lerp(new Vector3(activePanelScale, activePanelScale, activePanelScale),
                    new Vector3(secondaryPanelScale, secondaryPanelScale, secondaryPanelScale), (float) i / panelSwitchSteps);
                finalPanel.transform.localScale = Vector3.Lerp(new Vector3(secondaryPanelScale, secondaryPanelScale, secondaryPanelScale),
                    new Vector3(activePanelScale, activePanelScale, activePanelScale), (float) i / panelSwitchSteps);

                // Move to front.
                if (i > panelSwitchSteps / 2)
                {
                    finalPanel.transform.SetAsLastSibling();
                }

                yield return new WaitForFixedUpdate();
            }

            // Add other panel.
            if (otherPanel)
            {
                otherPanel.transform.localPosition = finalPanelOrig;
                otherPanel.transform.localScale = new Vector3(secondaryPanelScale, secondaryPanelScale, secondaryPanelScale);
            }

            // Set final positions/scales.
            origPanel.transform.localPosition = origPanelDest;
            finalPanel.transform.localPosition = activePanelPosition;
            origPanel.transform.localScale = new Vector3(secondaryPanelScale, secondaryPanelScale, secondaryPanelScale);
            finalPanel.transform.localScale = new Vector3(activePanelScale, activePanelScale, activePanelScale);
            finalPanel.transform.SetAsLastSibling();
        }

        private void Start()
        {
            ToggleMenu(false);
            onHide.Invoke();
        }

        private void Update()
        {
            if (menuObject.activeSelf)
            {
                if (rotateTowards != null && rotateTowards.position - transform.position != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation((rotateTowards.position - transform.position) * -1, Vector3.up);
                }
            }
        }
    }
}