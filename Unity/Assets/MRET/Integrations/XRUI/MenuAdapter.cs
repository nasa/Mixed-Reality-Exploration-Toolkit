// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.XRUI.ControllerMenu;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Locomotion;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Tools.Selection;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.UI.Cameras;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Feed;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.Eraser;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.Ruler;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRUI
{
    /// <remarks>
    /// History:
    /// 8 January 2021: Created
    /// </remarks>
    /// <summary>
    /// MenuAdapter is a basic script that creates the
    /// default controller menu layout programmatically.
    /// In the future, it will interface with XML.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class MenuAdapter : MonoBehaviour
    {
        /// <summary>
        /// The input rig.
        /// </summary>
        [Tooltip("The input rig.")]
        public InputRig inputRig;

        /// <summary>
        /// The hand for this menu.
        /// </summary>
        [Tooltip("The hand for this menu.")]
        public InputHand thisHand;

        /// <summary>
        /// The menu of interest.
        /// </summary>
        [Tooltip("The menu of interest.")]
        public ControllerMenu menu;

        /// <summary>
        /// The mode navigator.
        /// </summary>
        [Tooltip("The mode navigator.")]
        public ModeNavigator modeNavigator;

#region Hookups
        /// <summary>
        /// Icon for the create project button.
        /// </summary>
        [Tooltip("Icon for the create project button.")]
        public Sprite createProjectIcon;

        /// <summary>
        /// Event for the create project button.
        /// </summary>
        [Tooltip("Event for the create project button.")]
        public UnityEvent createProjectEvent;

        /// <summary>
        /// Icon for the open project button.
        /// </summary>
        [Tooltip("Icon for the open project button.")]
        public Sprite openProjectIcon;

        /// <summary>
        /// Event for the open project button.
        /// </summary>
        [Tooltip("Event for the open project button.")]
        public UnityEvent openProjectEvent;

        /// <summary>
        /// Icon for the save project button.
        /// </summary>
        [Tooltip("Icon for the save project button.")]
        public Sprite saveProjectIcon;

        /// <summary>
        /// Event for the save project button.
        /// </summary>
        [Tooltip("Event for the save project button.")]
        public UnityEvent saveProjectEvent;

        /// <summary>
        /// Icon for the reload project button.
        /// </summary>
        [Tooltip("Icon for the reload project button.")]
        public Sprite reloadProjectIcon;

        /// <summary>
        /// Event for the reload project button.
        /// </summary>
        [Tooltip("Event for the reload project button.")]
        public UnityEvent reloadProjectEvent;

        /// <summary>
        /// Icon for the reload user button.
        /// </summary>
        [Tooltip("Icon for the reload user button.")]
        public Sprite reloadUserIcon;

        /// <summary>
        /// Event for the reload user button.
        /// </summary>
        [Tooltip("Event for the reload user button.")]
        public UnityEvent reloadUserEvent;

        /// <summary>
        /// Icon for the character customization button.
        /// </summary>
        [Tooltip("Icon for the character customization button.")]
        public Sprite characterCustomizationIcon;

        /// <summary>
        /// Event for the character customization button.
        /// </summary>
        [Tooltip("Event for the character customization button.")]
        public UnityEvent characterCustomizationEvent;

        /// <summary>
        /// Icon for the collaboration join button.
        /// </summary>
        [Tooltip("Icon for the collaboration join button.")]
        public Sprite collaborationJoinIcon;

        /// <summary>
        /// Event for the collaboration join button.
        /// </summary>
        [Tooltip("Event for the collaboration join button.")]
        public UnityEvent collaborationJoinEvent;

        /// <summary>
        /// Icon for the collaboration share button.
        /// </summary>
        [Tooltip("Icon for the collaboration share button.")]
        public Sprite collaborationShareIcon;

        /// <summary>
        /// Event for the collaboration share button.
        /// </summary>
        [Tooltip("Event for the collaboration share button.")]
        public UnityEvent collaborationShareEvent;

        /// <summary>
        /// Icon for the collaboration coniguration button.
        /// </summary>
        [Tooltip("Icon for the collaboration configuration button.")]
        public Sprite collaborationConfigurationIcon;

        /// <summary>
        /// Event for the collaboration coniguration button.
        /// </summary>
        [Tooltip("Event for the collaboration configuration button.")]
        public UnityEvent collaborationConfigurationEvent;

        /// <summary>
        /// Event for the collaboration about button.
        /// </summary>
        [Tooltip("Event for the collaboration about button.")]
        public UnityEvent collaborationAboutEvent;

        /// <summary>
        /// Icon for the undo button.
        /// </summary>
        [Tooltip("Icon for the undo button.")]
        public Sprite undoIcon;

        /// <summary>
        /// Event for the undo button.
        /// </summary>
        [Tooltip("Event for the undo button.")]
        public UnityEvent undoEvent;

        /// <summary>
        /// Icon for the redo button.
        /// </summary>
        [Tooltip("Icon for the redo button.")]
        public Sprite redoIcon;

        /// <summary>
        /// Event for the redo button.
        /// </summary>
        [Tooltip("Event for the redo button.")]
        public UnityEvent redoEvent;

        /// <summary>
        /// Icon for the copy button.
        /// </summary>
        [Tooltip("Icon for the copy button.")]
        public Sprite copyIcon;

        /// <summary>
        /// Event for the copy button.
        /// </summary>
        [Tooltip("Event for the copy button.")]
        public UnityEvent copyEvent;

        /// <summary>
        /// Icon for the cut button.
        /// </summary>
        [Tooltip("Icon for the cut button.")]
        public Sprite cutIcon;

        /// <summary>
        /// Event for the cut button.
        /// </summary>
        [Tooltip("Event for the cut button.")]
        public UnityEvent cutEvent;

        /// <summary>
        /// Icon for the paste button.
        /// </summary>
        [Tooltip("Icon for the paste button.")]
        public Sprite pasteIcon;

        /// <summary>
        /// Event for the paste button.
        /// </summary>
        [Tooltip("Event for the paste button.")]
        public UnityEvent pasteEvent;

        /// <summary>
        /// Icon for the preferences button.
        /// </summary>
        [Tooltip("Icon for the preferences button.")]
        public Sprite preferencesIcon;

        /// <summary>
        /// Event for the preferences button.
        /// </summary>
        [Tooltip("Event for the preferences button.")]
        public UnityEvent preferencesEvent;

        /// <summary>
        /// Icon for the menu settings button.
        /// </summary>
        [Tooltip("Icon for the menu settings button.")]
        public Sprite menuSettingsIcon;

        /// <summary>
        /// Event for the menu settings button.
        /// </summary>
        [Tooltip("Event for the menu settings button.")]
        public UnityEvent menuSettingsEvent;

        /// <summary>
        /// Icon for the help button.
        /// </summary>
        [Tooltip("Icon for the help button.")]
        public Sprite helpIcon;

        /// <summary>
        /// Event for the help button.
        /// </summary>
        [Tooltip("Event for the help button.")]
        public UnityEvent helpEvent;

#if false
        /// <summary>
        /// Icon for the more button.
        /// </summary>
        [Tooltip("Icon for the more button.")]
        public Sprite moreIcon;

        /// <summary>
        /// Event for the more button.
        /// </summary>
        [Tooltip("Event for the more button.")]
        public UnityEvent moreEvent;
#endif

        /// <summary>
        /// Icon for the clipping planes button.
        /// </summary>
        [Tooltip("Icon for the clipping planes button.")]
        public Sprite clippingPlanesIcon;

        /// <summary>
        /// Event for the clipping planes button.
        /// </summary>
        [Tooltip("Event for the clipping planes button.")]
        public UnityEvent clippingPlanesEvent;

        /// <summary>
        /// Icon for the teleport toggle.
        /// </summary>
        [Tooltip("Icon for the teleport toggle.")]
        public Sprite teleportIcon;

        /// <summary>
        /// Event for the teleport toggle.
        /// </summary>
        [Tooltip("Event for the teleport toggle.")]
        public ToggleEvent teleportEvent;

        /// <summary>
        /// Icon for the fly toggle.
        /// </summary>
        [Tooltip("Icon for the fly toggle.")]
        public Sprite flyIcon;

        /// <summary>
        /// Event for the fly toggle.
        /// </summary>
        [Tooltip("Event for the fly toggle.")]
        public ToggleEvent flyEvent;

        /// <summary>
        /// Icon for the touchpad toggle.
        /// </summary>
        [Tooltip("Icon for the navigation toggle.")]
        public Sprite navigationIcon;

        /// <summary>
        /// Event for the touchpad toggle.
        /// </summary>
        [Tooltip("Event for the navigation toggle.")]
        public ToggleEvent navigationEvent;

        /// <summary>
        /// Icon for the armswing toggle.
        /// </summary>
        [Tooltip("Icon for the armswing toggle.")]
        public Sprite armswingIcon;

        /// <summary>
        /// Event for the armswing toggle.
        /// </summary>
        [Tooltip("Event for the armswing toggle.")]
        public ToggleEvent armswingEvent;

        /// <summary>
        /// Icon for the climb toggle.
        /// </summary>
        [Tooltip("Icon for the climb toggle.")]
        public Sprite climbIcon;

        /// <summary>
        /// Event for the climb toggle.
        /// </summary>
        [Tooltip("Event for the climb toggle.")]
        public ToggleEvent climbEvent;

        /// <summary>
        /// Icon for the rotate x toggle.
        /// </summary>
        [Tooltip("Icon for the rotate x toggle.")]
        public Sprite rotateXIcon;

        /// <summary>
        /// Event for the rotate x toggle.
        /// </summary>
        [Tooltip("Event for the rotate x toggle.")]
        public ToggleEvent rotateXEvent;

        /// <summary>
        /// Icon for the rotate y toggle.
        /// </summary>
        [Tooltip("Icon for the rotate y toggle.")]
        public Sprite rotateYIcon;

        /// <summary>
        /// Event for the rotate y toggle.
        /// </summary>
        [Tooltip("Event for the rotate y toggle.")]
        public ToggleEvent rotateYEvent;

        /// <summary>
        /// Icon for the rotate z toggle.
        /// </summary>
        [Tooltip("Icon for the rotate z toggle.")]
        public Sprite rotateZIcon;

        /// <summary>
        /// Event for the rotate z toggle.
        /// </summary>
        [Tooltip("Event for the rotate z toggle.")]
        public ToggleEvent rotateZEvent;

        /// <summary>
        /// Icon for the scale toggle.
        /// </summary>
        [Tooltip("Icon for the scale toggle.")]
        public Sprite scaleIcon;

        /// <summary>
        /// Event for the scale toggle.
        /// </summary>
        [Tooltip("Event for the scale toggle.")]
        public ToggleEvent scaleEvent;

        /// <summary>
        /// Icon for the HUD button.
        /// </summary>
        [Tooltip("Icon for the HUD button.")]
        public Sprite hudIcon;

        /// <summary>
        /// Event for the HUD button.
        /// </summary>
        [Tooltip("Event for the HUD button.")]
        public UnityEvent hudEvent;

        /// <summary>
        /// Icon for the display button.
        /// </summary>
        [Tooltip("Icon for the display button.")]
        public Sprite displayIcon;

        /// <summary>
        /// Event for the display button.
        /// </summary>
        [Tooltip("Event for the display button.")]
        public UnityEvent displayEvent;

        /// <summary>
        /// Icon for the photo camera toggle.
        /// </summary>
        [Tooltip("Icon for the photo camera toggle.")]
        public Sprite photoCameraIcon;

        /// <summary>
        /// Event for the photo camera toggle.
        /// </summary>
        [Tooltip("Event for the photo camera toggle.")]
        public ToggleEvent photoCameraEvent;

        /// <summary>
        /// Icon for the video camera toggle.
        /// </summary>
        [Tooltip("Icon for the video camera toggle.")]
        public Sprite videoCameraIcon;

        /// <summary>
        /// Event for the video camera toggle.
        /// </summary>
        [Tooltip("Event for the video camera toggle.")]
        public ToggleEvent videoCameraEvent;

        /// <summary>
        /// Icon for the no camera toggle.
        /// </summary>
        [Tooltip("Icon for the no camera toggle.")]
        public Sprite noCameraIcon;

        /// <summary>
        /// Event for the no camera toggle.
        /// </summary>
        [Tooltip("Event for the no camera toggle.")]
        public ToggleEvent noCameraEvent;

        /// <summary>
        /// Icon for the body camera toggle.
        /// </summary>
        [Tooltip("Icon for the body camera toggle.")]
        public Sprite bodyCameraIcon;

        /// <summary>
        /// Event for the body camera toggle.
        /// </summary>
        [Tooltip("Event for the body camera toggle.")]
        public ToggleEvent bodyCameraEvent;

        /// <summary>
        /// Icon for the SI ruler toggle.
        /// </summary>
        [Tooltip("Icon for the SI ruler toggle.")]
        public Sprite siRulerIcon;

        /// <summary>
        /// Event for the SI ruler toggle.
        /// </summary>
        [Tooltip("Event for the SI ruler toggle.")]
        public ToggleEvent siRulerEvent;

        /// <summary>
        /// Icon for the imperial ruler toggle.
        /// </summary>
        [Tooltip("Icon for the imperial ruler toggle.")]
        public Sprite imperialRulerIcon;

        /// <summary>
        /// Event for the imperial ruler toggle.
        /// </summary>
        [Tooltip("Event for the imperial ruler toggle.")]
        public ToggleEvent imperialRulerEvent;

        /// <summary>
        /// Icon for the no ruler toggle.
        /// </summary>
        [Tooltip("Icon for the no ruler toggle.")]
        public Sprite noRulerIcon;

        /// <summary>
        /// Event for the no ruler toggle.
        /// </summary>
        [Tooltip("Event for the no ruler toggle.")]
        public ToggleEvent noRulerEvent;

        /// <summary>
        /// Icon for the left panel toggle.
        /// </summary>
        [Tooltip("Icon for the left panel toggle.")]
        public Sprite leftPanelIcon;

        /// <summary>
        /// Event for the left panel toggle.
        /// </summary>
        [Tooltip("Event for the left panel toggle.")]
        public ToggleEvent leftPanelEvent;

        /// <summary>
        /// Icon for the right panel toggle.
        /// </summary>
        [Tooltip("Icon for the right panel toggle.")]
        public Sprite rightPanelIcon;

        /// <summary>
        /// Event for the right panel toggle.
        /// </summary>
        [Tooltip("Event for the right panel toggle.")]
        public ToggleEvent rightPanelEvent;

        /// <summary>
        /// Icon for the note tool toggle.
        /// </summary>
        [Tooltip("Icon for the note tool toggle.")]
        public Sprite noteToolIcon;

        /// <summary>
        /// Event for the note tool toggle.
        /// </summary>
        [Tooltip("Event for the note tool toggle.")]
        public ToggleEvent noteToolEvent;

        /// <summary>
        /// Event for the drawing button.
        /// </summary>
        [Tooltip("Event for the drawing button.")]
        public UnityEvent drawEvent;

        /// <summary>
        /// Icon for the annotations button.
        /// </summary>
        [Tooltip("Icon for the annotations button.")]
        public Sprite annotationsIcon;

        /// <summary>
        /// Event for the annotations button.
        /// </summary>
        [Tooltip("Event for the annotations button.")]
        public UnityEvent annotationsEvent;

        /// <summary>
        /// Icon for the objects submenu button.
        /// </summary>
        [Tooltip("Icon for the objects submenu button.")]
        public Sprite objectsSubmenuIcon;

        /// <summary>
        /// Prefab for the objects submenu panel.
        /// </summary>
        [Tooltip("Prefab for the objects submenu panel.")]
        public GameObject objectsSubmenuPrefab;

        /// <summary>
        /// Event for the IK button.
        /// </summary>
        [Tooltip("Event for the IK button.")]
        public UnityEvent ikEvent;

        /// <summary>
        /// Icon for the remote control button.
        /// </summary>
        [Tooltip("Icon for the remote control button.")]
        public Sprite remoteControlIcon;

        /// <summary>
        /// Event for the remote control button.
        /// </summary>
        [Tooltip("Event for the remote control button.")]
        public UnityEvent remoteControlEvent;

        /// <summary>
        /// Icon for the minimap button.
        /// </summary>
        [Tooltip("Icon for the minimap button.")]
        public Sprite minimapIcon;

        /// <summary>
        /// Event for the minimap button.
        /// </summary>
        [Tooltip("Event for the minimap button.")]
        public UnityEvent minimapEvent;

        /// <summary>
        /// Prefab for the data and documentation submenu panel.
        /// </summary>
        public GameObject dataAndDocumentationPrefab;

        /// <summary>
        /// Icon for the eraser toggle.
        /// </summary>
        [Tooltip("Icon for the eraser toggle.")]
        public Sprite eraserIcon;

        /// <summary>
        /// Event for the eraser toggle.
        /// </summary>
        [Tooltip("Event for the eraser toggle.")]
        public ToggleEvent eraserEvent;

        /// <summary>
        /// Icon for the time simulation button.
        /// </summary>
        [Tooltip("Icon for the time simulation button.")]
        public Sprite timeSimulationIcon;

        /// <summary>
        /// Event for the time simulation button.
        /// </summary>
        [Tooltip("Event for the time simulation button.")]
        public UnityEvent timeSimulationEvent;

        /// <summary>
        /// Icon for the animation button.
        /// </summary>
        [Tooltip("Icon for the animation button.")]
        public Sprite animationIcon;

        /// <summary>
        /// Event for the animation button.
        /// </summary>
        [Tooltip("Event for the animation button.")]
        public UnityEvent animationEvent;

        /// <summary>
        /// Event for the selection button.
        /// </summary>
        [Tooltip("Event for the selection button.")]
        public ToggleEvent selectionEvent;

        /// <summary>
        /// Event for the motion constraints toggle.
        /// </summary>
        [Tooltip("Event for the motion constraints toggle.")]
        public ToggleEvent motionConstraintsEvent;

        /// <summary>
        /// Icon for the exit button.
        /// </summary>
        [Tooltip("Icon for the exit button.")]
        public Sprite exitIcon;

        /// <summary>
        /// Event for the exit button.
        /// </summary>
        [Tooltip("Event for the exit button.")]
        public UnityEvent exitEvent;
#endregion

        public void Initialize()
        {
            // Blegh, disgusting. But it's just temporary, and this will be tied to parsing logic in a future build.

            menu.InitializeMenu();

            ControllerMenuPanel filePanel = menu.AddMenuPanel("File", true, false, true);
            filePanel.AddLabel("Project:");
            AddToButtonList(modeNavigator.newProj, filePanel.AddButton("Create", createProjectIcon, createProjectEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.openProj, filePanel.AddButton("Open", openProjectIcon, openProjectEvent, new Vector2(70, 70), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.saveProj, filePanel.AddButton("Save", saveProjectIcon, saveProjectEvent, new Vector2(70, 70), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.reloadProj, filePanel.AddButton("Reload", reloadProjectIcon, reloadProjectEvent, new Vector2(70, 70), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.reloadUser, filePanel.AddButton("Reset", reloadUserIcon, reloadUserEvent, new Vector2(70, 70), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.charCust, filePanel.AddButton("Edit Avatar", characterCustomizationIcon, characterCustomizationEvent, new Vector2(70, 70), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.exit, filePanel.AddButton("Exit", exitIcon, exitEvent, new Vector2(70, 70), ControllerMenuPanel.ButtonSize.Small));

            ControllerMenuPanel collaborationPanel = menu.AddMenuPanel("Collaboration", false, true, true);
            collaborationPanel.AddLabel("Collaboration:");
            AddToButtonList(modeNavigator.joinSess, collaborationPanel.AddButton("Join", collaborationJoinIcon, collaborationJoinEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.shareSess, collaborationPanel.AddButton("Share", collaborationShareIcon, collaborationShareEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.config, collaborationPanel.AddButton("Configuration", collaborationConfigurationIcon, collaborationConfigurationEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.about, collaborationPanel.AddButton("About", collaborationAboutEvent, ControllerMenuPanel.ButtonSize.Small));

            ControllerMenuPanel editPanel = menu.AddMenuPanel("Edit", false, true, true);
            editPanel.AddLabel("Actions:");
            AddToButtonList(modeNavigator.undo, editPanel.AddButton("Undo", undoIcon, undoEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.redo, editPanel.AddButton("Redo", redoIcon, redoEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            editPanel.AddLabel("Clipboard:");
            AddToButtonList(modeNavigator.copy, editPanel.AddButton("Copy", copyIcon, copyEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.cut, editPanel.AddButton("Cut", cutIcon, cutEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.paste, editPanel.AddButton("Paste", pasteIcon, pasteEvent, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small));
            editPanel.AddLabel("Settings:");
            AddToButtonList(modeNavigator.preferences, editPanel.AddButton("Preferences", preferencesIcon, preferencesEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.menuSettings, editPanel.AddButton("Menu Settings", menuSettingsIcon, menuSettingsEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.help, editPanel.AddButton("Help", helpIcon, helpEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            //AddToButtonList(modeNavigator.more, editPanel.AddButton("More", moreIcon, moreEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.clippingPlanes, editPanel.AddButton("Clipping Planes", clippingPlanesIcon, clippingPlanesEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));

            ControllerMenuPanel movementPanel = menu.AddMenuPanel("Movement", false, true, true);
            movementPanel.AddLabel("Locomotion:");
            UnityEvent teleportToggleEnableEvent = new UnityEvent();
            Toggle teleportToggle = movementPanel.AddToggle("Teleport", teleportIcon, teleportEvent, teleportToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            teleportToggleEnableEvent.AddListener(() => { SetToggle(teleportToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.teleportKey)); });
            AddToToggleList(modeNavigator.teleport, teleportToggle);
            UnityEvent flyToggleEnableEvent = new UnityEvent();
            Toggle flyToggle = movementPanel.AddToggle("Fly", flyIcon, flyEvent, flyToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            flyToggleEnableEvent.AddListener(() => { SetToggle(flyToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.flyKey)); });
            AddToToggleList(modeNavigator.fly, flyToggle);
            UnityEvent navigationToggleEnableEvent = new UnityEvent();
            Toggle navigationToggle = movementPanel.AddToggle("Navigation", navigationIcon, navigationEvent, navigationToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            navigationToggleEnableEvent.AddListener(() => { SetToggle(navigationToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.navigateKey)); });
            AddToToggleList(modeNavigator.touchpad, navigationToggle);
            UnityEvent armswingToggleEnableEvent = new UnityEvent();
            Toggle armswingToggle = movementPanel.AddToggle("Armswing", armswingIcon, armswingEvent, armswingToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            armswingToggleEnableEvent.AddListener(() => { SetToggle(armswingToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.armswingKey)); });
            AddToToggleList(modeNavigator.armswing, armswingToggle);
            UnityEvent climbToggleEnableEvent = new UnityEvent();
            Toggle climbToggle = movementPanel.AddToggle("Climb", climbIcon, climbEvent, climbToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            climbToggleEnableEvent.AddListener(() => { SetToggle(climbToggle, (bool)MRET.DataManager.FindPoint(LocomotionManager.climbKey)); });
            AddToToggleList(modeNavigator.climb, climbToggle);

            ControllerMenuPanel interfacePanel = menu.AddMenuPanel("Interface", false, true, true);
            interfacePanel.AddLabel("Rotate/Scale:");
            UnityEvent rotateXToggleEnableEvent = new UnityEvent();
            Toggle rotateXToggle = interfacePanel.AddToggle("RotX", rotateXIcon, rotateXEvent, rotateXToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            rotateXToggleEnableEvent.AddListener(() => { SetToggle(rotateXToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.rotateXKey)); });
            AddToToggleList(modeNavigator.rotX, rotateXToggle);
            UnityEvent rotateYToggleEnableEvent = new UnityEvent();
            Toggle rotateYToggle = interfacePanel.AddToggle("RotY", rotateYIcon, rotateYEvent, rotateYToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            rotateYToggleEnableEvent.AddListener(() => { SetToggle(rotateYToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.rotateYKey)); });
            AddToToggleList(modeNavigator.rotY, rotateYToggle);
            UnityEvent rotateZToggleEnableEvent = new UnityEvent();
            Toggle rotateZToggle = interfacePanel.AddToggle("RotZ", rotateZIcon, rotateZEvent, rotateZToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            rotateZToggleEnableEvent.AddListener(() => { SetToggle(rotateZToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.rotateZKey)); });
            AddToToggleList(modeNavigator.rotZ, rotateZToggle);
            UnityEvent scaleToggleEnableEvent = new UnityEvent();
            Toggle scaleToggle = interfacePanel.AddToggle("Scale", scaleIcon, scaleEvent, scaleToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            scaleToggleEnableEvent.AddListener(() => { SetToggle(scaleToggle, (bool) MRET.DataManager.FindPoint(LocomotionManager.scaleKey)); });
            AddToToggleList(modeNavigator.scale, scaleToggle);
            interfacePanel.AddLabel("User Interface:");
            AddToButtonList(modeNavigator.hud, interfacePanel.AddButton("HUD", hudIcon, hudEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.displays, interfacePanel.AddButton("Display", displayIcon, displayEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            UnityEvent motionConstraintsToggleEnableEvent = new UnityEvent();
            interfacePanel.AddLabel("Motion:");
            Toggle motionConstraintsToggle = interfacePanel.AddToggle("Motion Constraints",
                motionConstraintsToggleEnableEvent, motionConstraintsEvent, ControllerMenuPanel.ButtonSize.Small);
            motionConstraintsToggleEnableEvent.AddListener(() => {
                SetToggle(motionConstraintsToggle, (bool) MRET.DataManager.FindPoint(InteractableSceneObjectDeprecated.motionConstraintsKey));
            });
            AddToToggleList(modeNavigator.motionConstraints, motionConstraintsToggle);

            ControllerMenuPanel controlsPanel = menu.AddMenuPanel("Controls", false, true, true);
            controlsPanel.AddLabel("Camera:");
            UnityEvent cam0ToggleEnableEvent = new UnityEvent();
            UnityEvent cam1ToggleEnableEvent = new UnityEvent();
            UnityEvent cam2ToggleEnableEvent = new UnityEvent();
            System.Tuple<ToggleGroup, Toggle[]> cams =
                controlsPanel.AddToggleGroup("Cameras", new ControllerMenuPanel.ToggleInfo[] {
                new ControllerMenuPanel.ToggleInfo("Photo Camera", photoCameraIcon, photoCameraEvent, cam0ToggleEnableEvent, true, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small),
                new ControllerMenuPanel.ToggleInfo("Video Camera", videoCameraIcon, videoCameraEvent, cam1ToggleEnableEvent, true, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small),
                new ControllerMenuPanel.ToggleInfo("No Camera", noCameraIcon, noCameraEvent, cam2ToggleEnableEvent, true, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small)});
            if (cams.Item2.Length == 3)
            {
                AddToToggleList(modeNavigator.cameras, cams.Item2[0]);
                AddToToggleList(modeNavigator.cameras, cams.Item2[1]);
                AddToToggleList(modeNavigator.cameras, cams.Item2[2]);
                cam0ToggleEnableEvent.AddListener(() => { SetToggle(cams.Item2[0],
                    (bool) MRET.DataManager.FindPoint(CameraMenuController.imageCamera0Key) ||
                    (bool) MRET.DataManager.FindPoint(CameraMenuController.imageCamera1Key)); });
                cam1ToggleEnableEvent.AddListener(() => { SetToggle(cams.Item2[1],
                    (bool) MRET.DataManager.FindPoint(CameraMenuController.videoCamera0Key) ||
                    (bool) MRET.DataManager.FindPoint(CameraMenuController.videoCamera1Key));
                });
                cam0ToggleEnableEvent.AddListener(() => { SetToggle(cams.Item2[2],
                    !(bool) MRET.DataManager.FindPoint(CameraMenuController.imageCamera0Key) &&
                    !(bool) MRET.DataManager.FindPoint(CameraMenuController.imageCamera1Key) &&
                    !(bool) MRET.DataManager.FindPoint(CameraMenuController.videoCamera0Key) &&
                    !(bool) MRET.DataManager.FindPoint(CameraMenuController.videoCamera1Key));
                });
            }
            else
            {
                Debug.LogError("MenuAdapter: Error adding cameras.");
            }
            UnityEvent bodyCameraToggleEnableEvent = new UnityEvent();
            Toggle bodyCameraToggle = controlsPanel.AddToggle("Body Camera", bodyCameraIcon, bodyCameraEvent, bodyCameraToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            bodyCameraToggleEnableEvent.AddListener(() => { SetToggle(bodyCameraToggle, (bool) MRET.DataManager.FindPoint(CameraMenuController.bodyCameraKey)); });
            AddToToggleList(modeNavigator.cameras, scaleToggle);
            controlsPanel.AddLabel("Rulers:");
            UnityEvent rul0ToggleEnableEvent = new UnityEvent();
            UnityEvent rul1ToggleEnableEvent = new UnityEvent();
            UnityEvent rul2ToggleEnableEvent = new UnityEvent();
            System.Tuple<ToggleGroup, Toggle[]> ruls = 
                controlsPanel.AddToggleGroup("Rulers", new ControllerMenuPanel.ToggleInfo[] {
                new ControllerMenuPanel.ToggleInfo("SI Ruler", siRulerIcon, siRulerEvent, rul0ToggleEnableEvent, true, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small),
                new ControllerMenuPanel.ToggleInfo("Imperial Ruler", imperialRulerIcon, imperialRulerEvent, rul1ToggleEnableEvent, true, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small),
                new ControllerMenuPanel.ToggleInfo("No Ruler", noRulerIcon, noRulerEvent, rul2ToggleEnableEvent, true, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small)});
            if (ruls.Item2.Length == 3)
            {
                AddToToggleList(modeNavigator.rulers, ruls.Item2[0]);
                AddToToggleList(modeNavigator.rulers, ruls.Item2[1]);
                AddToToggleList(modeNavigator.rulers, ruls.Item2[2]);
                rul0ToggleEnableEvent.AddListener(() => { SetToggle(ruls.Item2[0],
                    (bool) MRET.DataManager.FindPoint(RulerMenuController.siRulerKey));
                });
                rul1ToggleEnableEvent.AddListener(() => { SetToggle(ruls.Item2[1],
                    (bool) MRET.DataManager.FindPoint(RulerMenuController.imperialUnitsKey));
                });
                rul0ToggleEnableEvent.AddListener(() => { SetToggle(ruls.Item2[2],
                    !(bool) MRET.DataManager.FindPoint(RulerMenuController.siRulerKey) &&
                    !(bool) MRET.DataManager.FindPoint(RulerMenuController.imperialUnitsKey));
                });
            }
            else
            {
                Debug.LogError("MenuAdapter: Error adding rulers.");
            }
            controlsPanel.AddLabel("Screen Panels:");
            UnityEvent leftPanelToggleEnableEvent = new UnityEvent();
            UnityEvent rightPanelToggleEnableEvent = new UnityEvent();
            Toggle leftPanelToggle = controlsPanel.AddToggle("Left Panel", leftPanelIcon,
                leftPanelEvent, leftPanelToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            Toggle rightPanelToggle = controlsPanel.AddToggle("Right Panel", rightPanelIcon,
                rightPanelEvent, rightPanelToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            leftPanelToggleEnableEvent.AddListener(() => {
                SetToggle(leftPanelToggle, (bool) MRET.DataManager.FindPoint(thisHand.handedness == InputHand.Handedness.right ?
                    FeedSourceMenuController.leftScreen1Key : FeedSourceMenuController.leftScreen0Key)); });
            rightPanelToggleEnableEvent.AddListener(() => {
                SetToggle(rightPanelToggle, (bool) MRET.DataManager.FindPoint(thisHand.handedness == InputHand.Handedness.right ?
                    FeedSourceMenuController.rightScreen1Key : FeedSourceMenuController.rightScreen0Key)); });
            AddToToggleList(modeNavigator.screens, leftPanelToggle);
            AddToToggleList(modeNavigator.screens, rightPanelToggle);

            ControllerMenuPanel toolsPanel = menu.AddMenuPanel("Tools", false, true, false);
            toolsPanel.AddLabel("Create:");
            UnityEvent noteToggleEnableEvent = new UnityEvent();
            Toggle noteToggle = toolsPanel.AddToggle("Note Tool", noteToolIcon, noteToolEvent,
                noteToggleEnableEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small);
            noteToggleEnableEvent.AddListener(() => {
                SetToggle(noteToggle, (bool) MRET.DataManager.FindPoint(NoteManager.notesKey));
            });
            AddToToggleList(modeNavigator.notes, noteToggle);
            AddToButtonList(modeNavigator.drawing, toolsPanel.AddButton("Draw", drawEvent, ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.annotations, toolsPanel.AddButton("Annotations", annotationsIcon, annotationsEvent, new Vector2(70, 70), ControllerMenuPanel.ButtonSize.Small));
            int osi = toolsPanel.AddSubmenu(objectsSubmenuPrefab);
            UnityEvent objectsSubmenuEvent = new UnityEvent();
            objectsSubmenuEvent.AddListener(()=> { toolsPanel.ToggleSubmenu(osi, true); });
            AddToButtonList(modeNavigator.objects, toolsPanel.AddButton("Objects Submenu", objectsSubmenuIcon, objectsSubmenuEvent, new Vector2(80, 80), ControllerMenuPanel.ButtonSize.Small));
            toolsPanel.AddLabel("Other:");
            AddToButtonList(modeNavigator.ik, toolsPanel.AddButton("IK", ikEvent, ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.remoteControl, toolsPanel.AddButton("Remote Control", remoteControlIcon, remoteControlEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.minimap, toolsPanel.AddButton("Minimap", minimapIcon, minimapEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            int ddsi = toolsPanel.AddSubmenu(dataAndDocumentationPrefab);
            UnityEvent dataAndDocumentationEvent = new UnityEvent();
            dataAndDocumentationEvent.AddListener(()=> { toolsPanel.ToggleSubmenu(ddsi, true); });
            AddToButtonList(modeNavigator.dataDoc, toolsPanel.AddButton("Data/Doc...", dataAndDocumentationEvent, ControllerMenuPanel.ButtonSize.Small));
            UnityEvent eraserToggleEnableEvent = new UnityEvent();
            Toggle eraserToggle = toolsPanel.AddToggle("Eraser Tool", eraserIcon, eraserEvent,
                eraserToggleEnableEvent, new Vector2(80, 80), ControllerMenuPanel.ButtonSize.Small);
            eraserToggleEnableEvent.AddListener(() => {
                SetToggle(eraserToggle, (bool) MRET.DataManager.FindPoint(EraserMenuController.eraserKey));
            });
            AddToToggleList(modeNavigator.eraser, eraserToggle);
            AddToButtonList(modeNavigator.timeSimulation, toolsPanel.AddButton("Time Simulation", timeSimulationIcon, timeSimulationEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            AddToButtonList(modeNavigator.animations, toolsPanel.AddButton("Animation", animationIcon, animationEvent, new Vector2(60, 60), ControllerMenuPanel.ButtonSize.Small));
            UnityEvent selectionToggleEnableEvent = new UnityEvent();
            Toggle selectionToggle = toolsPanel.AddToggle("Select...",
                selectionToggleEnableEvent, selectionEvent, ControllerMenuPanel.ButtonSize.Small);
            selectionToggleEnableEvent.AddListener(() => {
                SetToggle(noteToggle, (bool) MRET.DataManager.FindPoint(ControllerSelectionManager.selectionKey));
            });
            AddToToggleList(modeNavigator.selection, selectionToggle);

            menu.SelectMenuPanel(0);
        }

        public void MenuShown()
        {
            // Enable UI pointer always on, disallow teleport pointer on other hands.
            foreach (InputHand hand in inputRig.hands)
            {
                if (hand != thisHand)
                {
                    hand.ToggleUIPointerOn(true, false);
                    hand.BlockTeleport();
                    foreach (ControllerMenu menu in hand.GetComponentsInChildren<ControllerMenu>())
                    {
                        menu.HideMenu(false);
                    }
                }
                else
                {
                    hand.ToggleUIPointerOff(true);
                }
            }
        }

        public void MenuHidden()
        {
            // Disable UI pointer always on, allow teleport pointer on all hands.
            foreach (InputHand hand in inputRig.hands)
            {
                hand.ToggleUIPointerOn(true, false);
                hand.UnblockTeleport();
            }
        }

        private void AddToButtonList(List<Button> btnList, Button btn)
        {
            if (btnList == null)
            {
                btnList = new List<Button>();
            }
            btnList.Add(btn);
        }

        private void AddToToggleList(List<Toggle> tglList, Toggle tgl)
        {
            if (tglList == null)
            {
                tglList = new List<Toggle>();
            }
            tglList.Add(tgl);
        }

        private void SetToggle(Toggle tgl, bool active)
        {
            tgl.isOn = active;
        }
    }

    [System.Serializable]
    public class ToggleEvent : UnityEvent<bool> { }
}