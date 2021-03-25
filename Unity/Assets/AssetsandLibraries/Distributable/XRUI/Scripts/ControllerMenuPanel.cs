// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GSFC.ARVR.XRUI.ControllerMenu
{
    /// <remarks>
    /// History:
    /// 5 January 2021: Created
    /// </remarks>
    /// <summary>
    /// ControllerMenuPanel is the class for managing a
    /// controller-attached menu panel. It can contain labels
    /// and buttons.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class ControllerMenuPanel : MonoBehaviour
    {
        /// <summary>
        /// Contains information about a dynamically-configured toggle.
        /// </summary>
        public class ToggleInfo
        {
            /// <summary>
            /// Name of the toggle.
            /// </summary>
            public string name;

            /// <summary>
            /// Sprite for the toggle.
            /// </summary>
            public Sprite sprite;

            /// <summary>
            /// OnChange event for the toggle.
            /// </summary>
            public UnityEvent<bool> onChange;

            /// <summary>
            /// Event to call when panel containing toggle is enabled.
            /// </summary>
            public UnityEvent onEnableEvent;

            /// <summary>
            /// Whether or not to show sprite on the toggle.
            /// </summary>
            public bool showImage;

            /// <summary>
            /// Size for the icon.
            /// </summary>
            public Vector2 iconSize;

            /// <summary>
            /// Size of the toggle.
            /// </summary>
            public ButtonSize size;

            /// <summary>
            /// Constructor for a ToggleInfo class.
            /// </summary>
            /// <param name="Name">Name of the toggle.</param>
            /// <param name="Sprite">Sprite for the toggle.</param>
            /// <param name="OnChange">OnChange event for the toggle.</param>
            /// <param name="ShowImage">Whether or not to show the sprite on the toggle.</param>
            /// <param name="Size">Size of the toggle.</param>
            public ToggleInfo(string Name, Sprite Sprite, UnityEvent<bool> OnChange, UnityEvent OnEnableEvent,
                bool ShowImage = false, Vector2? IconSize = null, ButtonSize Size = ButtonSize.Small)
            {
                name = Name;
                sprite = Sprite;
                onChange = OnChange;
                onEnableEvent = OnEnableEvent;
                showImage = ShowImage;
                size = Size;

                if (IconSize.HasValue)
                {
                    iconSize = IconSize.Value;
                }
                else
                {
                    iconSize = Size == ButtonSize.Small ?
                        new Vector2(90, 90) : new Vector2(190, 90);
                }
            }
        }

        /// <summary>
        /// Size of a button.
        /// </summary>
        public enum ButtonSize { Small, Wide }

        /// <summary>
        /// Name for the panel.
        /// </summary>
        [Tooltip("Name for the panel.")]
        public string panelName;

        /// <summary>
        /// Prefab for a small button.
        /// </summary>
        [Tooltip("Prefab for a small button.")]
        public GameObject smallButtonPrefab;

        /// <summary>
        /// Prefab for a wide button.
        /// </summary>
        [Tooltip("Prefab for a wide button.")]
        public GameObject wideButtonPrefab;

        /// <summary>
        /// Prefab for a small toggle.
        /// </summary>
        [Tooltip("Prefab for a small toggle.")]
        public GameObject smallTogglePrefab;

        /// <summary>
        /// Prefab for a wide toggle.
        /// </summary>
        [Tooltip("Prefab for a wide toggle.")]
        public GameObject wideTogglePrefab;

        /// <summary>
        /// Prefab for a label.
        /// </summary>
        [Tooltip("Prefab for a label.")]
        public GameObject labelPrefab;

        /// <summary>
        /// Body of the menu panel.
        /// </summary>
        [Tooltip("Body of the menu panel.")]
        public GameObject panelBody;

        /// <summary>
        /// Text object for the title.
        /// </summary>
        [Tooltip("Text object for the title.")]
        public Text titleText;

        /// <summary>
        /// Home button.
        /// </summary>
        [Tooltip("Home button.")]
        public Button homeButton;

        /// <summary>
        /// Previous button.
        /// </summary>
        [Tooltip("Previous button.")]
        public Button previousButton;

        /// <summary>
        /// Next button.
        /// </summary>
        [Tooltip("Next button.")]
        public Button nextButton;

        /// <summary>
        /// The tooltip text box.
        /// </summary>
        [Tooltip("The tooltip text box.")]
        public Text tooltipText;

        /// <summary>
        /// Start position of items on the canvas.
        /// </summary>
        [Tooltip("Start position of items on the canvas.")]
        public Vector2 canvasStart = new Vector2(-175, 200);

        /// <summary>
        /// Horizontal spacing for items.
        /// </summary>
        [Tooltip("Horizontal spacing for items.")]
        public float horizontalSpacing = 100f;

        /// <summary>
        /// Vertical spacing for items.
        /// </summary>
        [Tooltip("Vertical spacing for items.")]
        public float verticalSpacing = -100f;

        /// <summary>
        /// Number of buttons per row.
        /// </summary>
        [Tooltip("Number of buttons per row.")]
        [Range(2, 16)]
        public int buttonsPerRow = 4;

        /// <summary>
        /// Current buttons and toggles on menu panel.
        /// </summary>
        private List<GameObject> buttons;

        /// <summary>
        /// Current labels on menu panel.
        /// </summary>
        private List<GameObject> labels;

        /// <summary>
        /// Current submenus on the menu panel.
        /// </summary>
        private List<GameObject> submenus;

        /// <summary>
        /// Events to call when panel is enabled.
        /// </summary>
        private List<UnityEvent> enableEvents;

        /// <summary>
        /// Number of rows.
        /// </summary>
        private float rowCount;

        /// <summary>
        /// Number of columns on current row.
        /// </summary>
        private int columnCount;

        /// <summary>
        /// Is panel initializing?
        /// </summary>
        private bool panelInitializing = false;

        /// <summary>
        /// Perform initialization of this menu panel.
        /// </summary>
        /// <param name="homeButtonEnabled">Whether or not to enable the home button.</param>
        /// <param name="previousButtonEnabled">Whether or not to enable the previous button.</param>
        /// <param name="nextButtonEnabled">Whether or not to enable the next button.</param>
        /// <param name="enableEvent">Event to call on panel enable.</param>
        public void Initialize(bool homeButtonEnabled,
            bool previousButtonEnabled = true, bool nextButtonEnabled = true)
        {
            if (buttons != null)
            {
                foreach (GameObject button in buttons)
                {
                    Destroy(button);
                }
            }
            buttons = new List<GameObject>();

            if (labels != null)
            {
                foreach (GameObject label in labels)
                {
                    Destroy(label);
                }
            }
            labels = new List<GameObject>();

            if (submenus != null)
            {
                foreach (GameObject submenu in submenus)
                {
                    Destroy(submenu);
                }
            }
            submenus = new List<GameObject>();

            enableEvents = new List<UnityEvent>();

            rowCount = 0;
            columnCount = 0;

            titleText.text = name = panelName;

            homeButton.gameObject.SetActive(homeButtonEnabled);

            previousButton.gameObject.SetActive(previousButtonEnabled);
            nextButton.gameObject.SetActive(nextButtonEnabled);
        }

        /// <summary>
        /// Add a button to this panel.
        /// </summary>
        /// <param name="buttonName">Name of the button.</param>
        /// <param name="buttonSprite">Sprite for the button.</param>
        /// <param name="onClick">OnClick event for the button.</param>
        /// <param name="iconSize">Size for the icon.</param>
        /// <param name="size">Size of the button.</param>
        /// <returns>A reference to the button or null.</returns>
        public Button AddButton(string buttonName, Sprite buttonSprite,
            UnityEvent onClick, Vector2? iconSize = null, ButtonSize size = ButtonSize.Small)
        {
            return AddButton(buttonName, buttonSprite, onClick, true, iconSize, size);
        }

        /// <summary>
        /// Add a button to this panel.
        /// </summary>
        /// <param name="buttonName">Name of the button.</param>
        /// <param name="onClick">OnClick event for the button.</param>
        /// <param name="size">Size of the button.</param>
        /// <returns>A reference to the button or null.</returns>
        public Button AddButton(string buttonName, UnityEvent onClick, ButtonSize size = ButtonSize.Small)
        {
            return AddButton(buttonName, null, onClick, false, null, size);
        }

        /// <summary>
        /// Add a button to this panel.
        /// </summary>
        /// <param name="buttonName">Name of the button.</param>
        /// <param name="buttonSprite">Sprite for the button.</param>
        /// <param name="onClick">OnClick event for the button.</param>
        /// <param name="showImage">Whether or not to use the image for the button.</param>
        /// <param name="iconSize">Size for the icon.</param>
        /// <param name="size">Size of the button.</param>
        /// <returns>A reference to the button or null.</returns>
        public Button AddButton(string buttonName, Sprite buttonSprite, UnityEvent onClick,
            bool showImage = false, Vector2? iconSize = null, ButtonSize size = ButtonSize.Small)
        {
            // Add button.
            GameObject newButton = null;
            if (size == ButtonSize.Small)
            {
                newButton = Instantiate(smallButtonPrefab);
            }
            else if (size == ButtonSize.Wide)
            {
                newButton = Instantiate(wideButtonPrefab);
            }

            if (newButton == null)
            {
                Debug.LogError("ControllerMenuPanel: Error adding button.");
                return null;
            }

            buttons.Add(newButton);

            // Place button.
            if (size == ButtonSize.Small && columnCount >= buttonsPerRow
                || size == ButtonSize.Wide & columnCount >= buttonsPerRow - 1)
            {
                columnCount = 0;
                rowCount++;
            }

            newButton.transform.SetParent(panelBody.transform);
            if (size == ButtonSize.Small)
            {
                newButton.transform.localPosition = new Vector3(
                    canvasStart.x + columnCount * horizontalSpacing, canvasStart.y + rowCount * verticalSpacing, 0);
            }
            else if (size == ButtonSize.Wide)
            {
                newButton.transform.localPosition = new Vector3(
                    canvasStart.x + (columnCount + 0.5f) * horizontalSpacing, canvasStart.y + rowCount * verticalSpacing, 0);
            }

            newButton.transform.localRotation = Quaternion.identity;
            newButton.transform.localScale = Vector3.one;

            if (size == ButtonSize.Small)
            {
                columnCount++;
            }
            else if (size == ButtonSize.Wide)
            {
                columnCount += 2;
            }

            // Set click event.
            Button buttonObj = newButton.GetComponent<Button>();
            if (buttonObj == null)
            {
                Debug.LogError("ControllerMenuPanel: Error setting button.");
                return null;
            }

            Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
            clickEvent.AddListener(() => { if (onClick != null) onClick.Invoke(); });
            buttonObj.onClick = clickEvent;

            // Set image or text on button.
            Image imageObj = null;
            foreach (Image img in newButton.GetComponentsInChildren<Image>())
            {
                if (img.gameObject != newButton)
                {
                    imageObj = img;
                    break;
                }
            }
            
            if (imageObj == null)
            {
                Debug.LogError("ControllerMenuPanel: Error setting image.");
                return null;
            }

            Text textObj = newButton.GetComponentInChildren<Text>();
            if (textObj == null)
            {
                Debug.LogError("ControllerMenuPanel: Error setting text.");
                return null;
            }

            if (showImage)
            {
                imageObj.gameObject.SetActive(true);
                textObj.gameObject.SetActive(false);

                imageObj.sprite = buttonSprite;

                if (iconSize.HasValue)
                {
                    imageObj.rectTransform.sizeDelta = iconSize.Value;
                }
                else
                {
                    imageObj.rectTransform.sizeDelta = size == ButtonSize.Small ?
                        new Vector2(90, 90) : new Vector2(190, 90);
                }
            }
            else
            {
                imageObj.gameObject.SetActive(false);
                textObj.gameObject.SetActive(true);

                textObj.text = buttonName;
            }

            // Set up tooltip manager.
            MenuToolTipManager ttm = newButton.GetComponent<MenuToolTipManager>();
            if (ttm)
            {
                ttm.controllerMenuPanel = this;
                ttm.InfoTextBox = tooltipText;
                ttm.TooltipText = buttonName;
            }

            return buttonObj;
        }

        /// <summary>
        /// Add a toggle to this panel.
        /// </summary>
        /// <param name="info">Toggle Information.</param>
        /// <returns>A reference to the toggle or null.</returns>
        public Toggle AddToggle(ToggleInfo info)
        {
            return AddToggle(info.name, info.sprite, info.onChange,
                info.onEnableEvent, info.showImage, info.iconSize, info.size);
        }

        /// <summary>
        /// Add a toggle to this panel.
        /// </summary>
        /// <param name="toggleName">Name of the toggle.</param>
        /// <param name="toggleSprite">Sprite for the toggle.</param>
        /// <param name="onChange">OnChange event for the toggle.</param>
        /// <param name="iconSize">Size for the icon.</param>
        /// <param name="size">Size of the toggle.</param>
        /// <returns>A reference to the toggle or null.</returns>
        public Toggle AddToggle(string toggleName, Sprite toggleSprite, UnityEvent<bool> onChange,
            UnityEvent onEnable, Vector2? iconSize = null, ButtonSize size = ButtonSize.Small)
        {
            return AddToggle(toggleName, toggleSprite, onChange, onEnable, true, iconSize, size);
        }

        /// <summary>
        /// Add a toggle to this panel.
        /// </summary>
        /// <param name="toggleName">Name of the toggle.</param>
        /// <param name="onChange">OnChange event for the toggle.</param>
        /// <param name="size">Size of the toggle.</param>
        /// <returns>A reference to the toggle or null.</returns>
        public Toggle AddToggle(string toggleName,
            UnityEvent onEnable, UnityEvent<bool> onChange, ButtonSize size = ButtonSize.Small)
        {
            return AddToggle(toggleName, null, onChange, onEnable, false, null, size);
        }

        /// <summary>
        /// Add a toggle to this panel.
        /// </summary>
        /// <param name="toggleName">Name of the toggle.</param>
        /// <param name="toggleSprite">Sprite for the toggle.</param>
        /// <param name="onChange">OnChange event for the toggle.</param>
        /// <param name="showImage">Whether or not to use the image for the toggle.</param>
        /// <param name="iconSize">Size for the icon.</param>
        /// <param name="size">Size of the toggle.</param>
        /// <returns>A reference to the toggle or null.</returns>
        public Toggle AddToggle(string toggleName, Sprite toggleSprite, UnityEvent<bool> onChange,
            UnityEvent onEnable, bool showImage = false, Vector2? iconSize = null, ButtonSize size = ButtonSize.Small)
        {
            // Add button.
            GameObject newToggle = null;
            if (size == ButtonSize.Small)
            {
                newToggle = Instantiate(smallTogglePrefab);
            }
            else if (size == ButtonSize.Wide)
            {
                newToggle = Instantiate(wideTogglePrefab);
            }

            if (newToggle == null)
            {
                Debug.LogError("ControllerMenuPanel: Error adding toggle.");
                return null;
            }

            buttons.Add(newToggle);

            // Place toggle.
            if (size == ButtonSize.Small && columnCount >= buttonsPerRow
                || size == ButtonSize.Wide & columnCount >= buttonsPerRow - 1)
            {
                columnCount = 0;
                rowCount++;
            }

            newToggle.transform.SetParent(panelBody.transform);
            if (size == ButtonSize.Small)
            {
                newToggle.transform.localPosition = new Vector3(
                    canvasStart.x + columnCount * horizontalSpacing, canvasStart.y + rowCount * verticalSpacing, 0);
            }
            else if (size == ButtonSize.Wide)
            {
                newToggle.transform.localPosition = new Vector3(
                    canvasStart.x + (columnCount + 0.5f) * horizontalSpacing, canvasStart.y + rowCount * verticalSpacing, 0);
            }

            newToggle.transform.localRotation = Quaternion.identity;
            newToggle.transform.localScale = Vector3.one;

            if (size == ButtonSize.Small)
            {
                columnCount++;
            }
            else if (size == ButtonSize.Wide)
            {
                columnCount += 2;
            }

            // Set change event.
            Toggle toggleObj = newToggle.GetComponent<Toggle>();
            if (toggleObj == null)
            {
                Debug.LogError("ControllerMenuPanel: Error setting toggle.");
                return null;
            }

            Toggle.ToggleEvent changeEvent = new Toggle.ToggleEvent();
            changeEvent.AddListener((x) => { if (onChange != null) OnTogglePress(toggleObj, onChange); });
            toggleObj.onValueChanged = changeEvent;

            enableEvents.Add(onEnable);

            // Set image or text on toggle.
            Image imageObj = null;
            foreach (Image img in newToggle.GetComponentsInChildren<Image>())
            {
                if (img.gameObject != newToggle)
                {
                    imageObj = img;
                    break;
                }
            }

            if (imageObj == null)
            {
                Debug.LogError("ControllerMenuPanel: Error setting image.");
                return null;
            }

            Text textObj = newToggle.GetComponentInChildren<Text>();
            if (textObj == null)
            {
                Debug.LogError("ControllerMenuPanel: Error setting text.");
                return null;
            }

            if (showImage)
            {
                imageObj.gameObject.SetActive(true);
                textObj.gameObject.SetActive(false);

                imageObj.sprite = toggleSprite;

                if (iconSize.HasValue)
                {
                    imageObj.rectTransform.sizeDelta = iconSize.Value;
                }
                else
                {
                    imageObj.rectTransform.sizeDelta = size == ButtonSize.Small ?
                        new Vector2(90, 90) : new Vector2(190, 90);
                }
            }
            else
            {
                imageObj.gameObject.SetActive(false);
                textObj.gameObject.SetActive(true);

                textObj.text = toggleName;
            }

            // Set up tooltip manager.
            MenuToolTipManager ttm = newToggle.GetComponent<MenuToolTipManager>();
            if (ttm)
            {
                ttm.controllerMenuPanel = this;
                ttm.InfoTextBox = tooltipText;
                ttm.TooltipText = toggleName;
            }

            return toggleObj;
        }

        /// <summary>
        /// Add a toggle group to this panel.
        /// </summary>
        /// <param name="groupName">Name for the toggle group.</param>
        /// <param name="toggles">Array of toggles.</param>
        /// <returns>A reference to the toggle group and toggles or null.</returns>
        public System.Tuple<ToggleGroup, Toggle[]> AddToggleGroup(string groupName, ToggleInfo[] toggles)
        {
            List<Toggle> toggleGroupToggles = new List<Toggle>();

            // Add the toggles.
            foreach (ToggleInfo info in toggles)
            {
                Toggle newToggle = AddToggle(info);
                if (newToggle == null)
                {
                    Debug.LogError("ControllerMenuPanel: Error adding toggle to toggle group.");
                    return null;
                }

                toggleGroupToggles.Add(newToggle);
            }

            // Add toggles to group.
            GameObject tgGO = new GameObject(groupName);
            tgGO.transform.SetParent(transform);
            ToggleGroup group = tgGO.AddComponent<ToggleGroup>();
            group.allowSwitchOff = false;
            foreach (Toggle toggle in toggleGroupToggles)
            {
                toggle.group = group;
            }

            return new System.Tuple<ToggleGroup, Toggle[]>(group, toggleGroupToggles.ToArray());
        }

        /// <summary>
        /// Add a label to this panel.
        /// </summary>
        /// <param name="text">Text to put on the label.</param>
        /// <returns>A refernece to the text object or null.</returns>
        public Text AddLabel(string text)
        {
            // Add label.
            GameObject newLabel = null;
            newLabel = Instantiate(labelPrefab);

            if (newLabel == null)
            {
                Debug.LogError("ControllerMenuPanel: Error adding label.");
                return null;
            }

            labels.Add(newLabel);

            // Place label.
            if (columnCount > 0)
            {
                rowCount += 0.82f;
                columnCount = 0;
            }
            else if (rowCount == 0)
            {
                // If label is at top, move up.
                rowCount -= 0.13f;
            }

            newLabel.transform.SetParent(panelBody.transform);
            newLabel.transform.localPosition = new Vector3(
                0, canvasStart.y + rowCount * verticalSpacing, 0);
            newLabel.transform.localRotation = Quaternion.identity;
            newLabel.transform.localScale = Vector3.one;

            rowCount += 0.795f;
            columnCount = 0;

            // Add text.
            Text textObj = newLabel.GetComponentInChildren<Text>();
            if (textObj)
            {
                textObj.text = text;
            }
            else
            {
                Debug.LogError("ControllerMenuPanel: Error setting text.");
                return null;
            }

            return textObj;
        }

        /// <summary>
        /// Add a submenu to the menu panel.
        /// </summary>
        /// <param name="prefab">Prefab for the submenu.</param>
        /// <returns>Index of the submenu.</returns>
        public int AddSubmenu(GameObject prefab)
        {
            GameObject submenu = Instantiate(prefab);
            submenu.transform.SetParent(transform);
            submenu.transform.localPosition = new Vector3(0, 0, -10);
            submenu.transform.localRotation = Quaternion.identity;
            submenu.transform.localScale = Vector3.one;
            submenu.transform.SetAsLastSibling();
            submenu.SetActive(false);

            submenus.Add(submenu);
            int idx = submenus.IndexOf(submenu);
            ControllerSubmenuPanel smp = submenu.GetComponent<ControllerSubmenuPanel>();
            if (smp != null && !panelInitializing)
            {
                smp.closeButton.onClick.AddListener(() => { ToggleSubmenu(idx, false); });
            }

            return idx;
        }

        /// <summary>
        /// Enable or disable a submenu.
        /// </summary>
        /// <param name="submenuIndex">Index of the submenu.</param>
        /// <param name="active">Whether or not to enable it.</param>
        public void ToggleSubmenu(int submenuIndex, bool active)
        {
            if (submenus.Count > submenuIndex && submenuIndex > -1)
            {
                submenus[submenuIndex].SetActive(active);
            }
            else
            {
                Debug.LogError("ControllerMenuPanel: Invalid submenu index.");
            }
        }

        private void OnEnable()
        {
            panelInitializing = true;

            if (enableEvents != null)
            {
                foreach (UnityEvent enableEvent in enableEvents)
                {
                    if (enableEvent != null)
                    {
                        enableEvent.Invoke();
                    }
                }
            }

            panelInitializing = false;
        }

        private void OnTogglePress(Toggle toggle, UnityEvent<bool> onChange)
        {
            if (panelInitializing)
            {
                return;
            }

            onChange.Invoke(toggle.isOn);
        }
    }
}