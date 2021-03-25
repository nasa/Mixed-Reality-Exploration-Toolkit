// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class ModeNavigator : MonoBehaviour
{
    public static ModeNavigator instance;
    
    public UnityProject projectManager;
    public List<Button> newProj, openProj, saveProj, home,
        joinSess, shareSess, config, about,
        undo, redo,
        cut, copy, paste,
        preferences, menuSettings, help, more, clippingPlanes,
        hud, displays,
        drawing, annotations, objects, outlineView,
        ik, remoteControl, minimap, dataDoc, animations,
        timeSimulation;
    public List<Toggle> teleport, fly, touchpad, armswing,
        rotX, rotY, rotZ, scale,
        notes, eraser, selection,
        cameras, rulers, screens;

    public UnityEvent lobbyModeInitializationEvents;

    public void Initialize()
    {
        instance = this;
	}

    public void LoadLobby()
    {
        MRET.ControlMode.DisableAllControlTypes();
        EnterLobbyMode();
        projectManager.Unload();
        ResetLobbyPosition();
        HideLoadingIndicator();
    }

    public void CreateProject(string templateFile, bool collaborationEnabled)
    {
        try
        {
            MRET.ConfigurationManager.AddRecentTemplate(templateFile);
            projectManager.collaborationEnabled = collaborationEnabled;
            MRET.ControlMode.DisableAllControlTypes();
            projectManager.Unload();
            projectManager.LoadFromXML(templateFile);
            EnterProjectMode();
        }
        catch (Exception e)
        {
            Debug.Log("[ModeNavigator] " + e.ToString());
        }
    }

    public void CreateProject(ProjectInfo templateToOpen, bool collaborationEnabled)
    {
        try
        {
            MRET.ConfigurationManager.AddRecentTemplate(templateToOpen);
            projectManager.collaborationEnabled = collaborationEnabled;
            MRET.ControlMode.DisableAllControlTypes();
            projectManager.Unload();
            projectManager.LoadFromXML(templateToOpen.projFile);
            EnterProjectMode();
        }
        catch (Exception e)
        {
            Debug.Log("[ModeNavigator] " + e.ToString());
        }
    }

    public void OpenProject(ProjectType project, string projectFile, bool collaborationEnabled)
    {
        try
        {
            MRET.ConfigurationManager.AddRecentProject(projectFile);
            projectManager.collaborationEnabled = collaborationEnabled;
            MRET.ControlMode.DisableAllControlTypes();
            projectManager.Unload();
            projectManager.Deserialize(project);
            EnterProjectMode();
        }
        catch (Exception e)
        {
            Debug.Log("[ModeNavigator] " + e.ToString());
        }
    }

    public void OpenProject(string projectFile, bool collaborationEnabled)
    {
        try
        {
            MRET.ConfigurationManager.AddRecentProject(projectFile);
            projectManager.collaborationEnabled = collaborationEnabled;
            MRET.ControlMode.DisableAllControlTypes();
            projectManager.Unload();
            projectManager.LoadFromXML(projectFile);
            EnterProjectMode();
        }
        catch (Exception e)
        {
            Debug.Log("[ModeNavigator] " + e.ToString());
        }
    }

    public void OpenProject(ProjectInfo projectToOpen, bool collaborationEnabled)
    {
        try
        {
            MRET.ConfigurationManager.AddRecentProject(projectToOpen);
            projectManager.collaborationEnabled = collaborationEnabled;
            MRET.ControlMode.DisableAllControlTypes();
            projectManager.Unload();
            projectManager.LoadFromXML(projectToOpen.projFile);
            EnterProjectMode();
        }
        catch (Exception e)
        {
            Debug.Log("[ModeNavigator] " + e.ToString());
        }
    }

    private void ResetLobbyPosition()
    {
        Transform cameraRig = MRET.InputRig.transform;
        cameraRig.position = new Vector3(3.5f, 0.17f, -3.25f);
        cameraRig.rotation = Quaternion.identity;
        cameraRig.localScale = Vector3.one;
    }

    private void HideLoadingIndicator()
    {
        LoadingIndicatorManager loadingIndicatorManager = FindObjectOfType<LoadingIndicatorManager>();
        if (loadingIndicatorManager)
        {
            loadingIndicatorManager.StopLoadingIndicator();
        }
    }

    public void EnableAllControls()
    {
        // Enable all menu options.
        foreach (Button btn in newProj)
        {
            btn.interactable = true;
        }

        foreach (Button btn in openProj)
        {
            btn.interactable = true;
        }

        foreach (Button btn in saveProj)
        {
            btn.interactable = true;
        }

        foreach (Button btn in home)
        {
            btn.interactable = true;
        }

        foreach (Button btn in joinSess)
        {
            btn.interactable = true;
        }

        foreach (Button btn in shareSess)
        {
            btn.interactable = true;
        }

        foreach (Button btn in config)
        {
            btn.interactable = true;
        }

        foreach (Button btn in about)
        {
            btn.interactable = true;
        }

        foreach (Button btn in undo)
        {
            btn.interactable = true;
        }

        foreach (Button btn in redo)
        {
            btn.interactable = true;
        }

        foreach (Button btn in cut)
        {
            btn.interactable = true;
        }

        foreach (Button btn in copy)
        {
            btn.interactable = true;
        }

        foreach (Button btn in paste)
        {
            btn.interactable = true;
        }

        foreach (Button btn in preferences)
        {
            btn.interactable = true;
        }

        foreach (Button btn in menuSettings)
        {
            btn.interactable = true;
        }

        foreach (Button btn in help)
        {
            btn.interactable = true;
        }

        /*foreach (Button btn in more)
        {
            btn.interactable = true;
        }*/

        foreach (Button btn in clippingPlanes)
        {
            btn.interactable = true;
        }

        foreach (Toggle tgl in teleport)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in fly)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in touchpad)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in armswing)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in rotX)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in rotY)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in rotZ)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in scale)
        {
            tgl.interactable = true;
        }

        foreach (Button btn in hud)
        {
            btn.interactable = true;
        }

        foreach (Button btn in displays)
        {
            btn.interactable = true;
        }

        foreach (Toggle tgl in notes)
        {
            tgl.interactable = true;
        }

        foreach (Button btn in drawing)
        {
            btn.interactable = true;
        }

        foreach (Button btn in animations)
        {
            btn.interactable = true;
        }

        foreach (Button btn in timeSimulation)
        {
            btn.interactable = true;
        }

        foreach (Toggle tgl in selection)
        {
            tgl.interactable = true;
        }

        foreach (Button btn in ik)
        {
            btn.interactable = true;
        }

        foreach (Button btn in annotations)
        {
            btn.interactable = true;
        }

        foreach (Button btn in objects)
        {
            btn.interactable = true;
        }

        foreach (Button btn in outlineView)
        {
            btn.interactable = true;
        }

        foreach (Button btn in remoteControl)
        {
            btn.interactable = true;
        }

        foreach (Button btn in minimap)
        {
            btn.interactable = true;
        }

        foreach (Button btn in dataDoc)
        {
            btn.interactable = true;
        }

        foreach (Toggle tgl in eraser)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in cameras)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in rulers)
        {
            tgl.interactable = true;
        }

        foreach (Toggle tgl in screens)
        {
            tgl.interactable = true;
        }
    }

    public void SetMenuControl(string control, bool enabled)
    {
        switch (control)
        {
            case "NewProject":
                foreach (Button btn in newProj)
                {
                    btn.interactable = enabled;
                }
                break;

            case "OpenProject":
                foreach (Button btn in openProj)
                {
                    btn.interactable = enabled;
                }
                break;

            case "SaveProject":
                foreach (Button btn in saveProj)
                {
                    btn.interactable = enabled;
                }
                break;
                
            case "Home":
                foreach (Button btn in home)
                {
                    btn.interactable = enabled;
                }
                break;

            case "JoinSession":
                foreach (Button btn in joinSess)
                {
                    btn.interactable = enabled;
                }
                break;

            case "ShareSession":
                foreach (Button btn in shareSess)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Configuration":
                foreach (Button btn in config)
                {
                    btn.interactable = enabled;
                }
                break;

            case "About":
                foreach (Button btn in about)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Undo":
                foreach (Button btn in undo)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Redo":
                foreach (Button btn in redo)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Cut":
                foreach (Button btn in cut)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Copy":
                foreach (Button btn in copy)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Paste":
                foreach (Button btn in paste)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Preferences":
                foreach (Button btn in preferences)
                {
                    btn.interactable = enabled;
                }
                break;

            case "MenuSettings":
                foreach (Button btn in menuSettings)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Help":
                foreach (Button btn in help)
                {
                    btn.interactable = enabled;
                }
                break;

            /*case "More":
                foreach (Button btn in more)
                {
                    btn.interactable = enabled;
                }
                break;*/

            case "ClippingPlanes":
                foreach (Button btn in clippingPlanes)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Teleport":
                foreach (Toggle tgl in teleport)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Fly":
                foreach (Toggle tgl in fly)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Touchpad":
                foreach (Toggle tgl in touchpad)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Armswing":
                foreach (Toggle tgl in armswing)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "RotateX":
                foreach (Toggle tgl in rotX)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "RotateY":
                foreach (Toggle tgl in rotY)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "RotateZ":
                foreach (Toggle tgl in rotZ)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Scale":
                foreach (Toggle tgl in scale)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "HUD":
                foreach (Button btn in hud)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Displays":
                foreach (Button btn in displays)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Notes":
                foreach (Toggle tgl in notes)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Drawing":
                foreach (Button btn in drawing)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Animations":
                foreach (Button btn in animations)
                {
                    btn.interactable = enabled;
                }
                break;

            case "TimeSimulation":
                foreach (Button btn in timeSimulation)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Selection":
                foreach (Toggle tgl in selection)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "IK":
                foreach (Button btn in ik)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Annotations":
                foreach (Button btn in annotations)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Objects":
                foreach (Button btn in objects)
                {
                    btn.interactable = enabled;
                }
                break;

            case "OutlineView":
                foreach (Button btn in outlineView)
                {
                    btn.interactable = enabled;
                }
                break;

            case "RemoteControl":
                foreach (Button btn in remoteControl)
                {
                    btn.interactable = enabled;
                }
                break;
            
            case "Minimap":
                foreach (Button btn in minimap)
                {
                    btn.interactable = enabled;
                }
                break;
            
            case "DataDocumentation":
                foreach (Button btn in dataDoc)
                {
                    btn.interactable = enabled;
                }
                break;

            case "Eraser":
                foreach (Toggle tgl in eraser)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Cameras":
                foreach (Toggle tgl in cameras)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Rulers":
                foreach (Toggle tgl in rulers)
                {
                    tgl.interactable = enabled;
                }
                break;

            case "Screens":
                foreach (Toggle tgl in screens)
                {
                    tgl.interactable = enabled;
                }
                break;

            default:
                Debug.LogWarning("[SetMenuControl] Invalid control " + control);
                break;
        }
    }
    
#region ModeInitializers
    private void EnterLobbyMode()
    {
        if (lobbyModeInitializationEvents != null)
        {
            lobbyModeInitializationEvents.Invoke();
        }

        // Disable all menu options except for project loading and settings.
        foreach (Button btn in newProj)
        {
            btn.interactable = true;
        }

        foreach (Button btn in openProj)
        {
            btn.interactable = true;
        }

        foreach (Button btn in saveProj)
        {
            btn.interactable = false;
        }

        foreach (Button btn in home)
        {
            btn.interactable = false;
        }

        foreach (Button btn in joinSess)
        {
            btn.interactable = true;
        }

        foreach (Button btn in shareSess)
        {
            btn.interactable = true;
        }

        foreach (Button btn in config)
        {
            btn.interactable = true;
        }

        foreach (Button btn in about)
        {
            btn.interactable = true;
        }

        foreach (Button btn in undo)
        {
            btn.interactable = false;
        }

        foreach (Button btn in redo)
        {
            btn.interactable = false;
        }

        foreach (Button btn in cut)
        {
            btn.interactable = false;
        }

        foreach (Button btn in copy)
        {
            btn.interactable = false;
        }

        foreach (Button btn in paste)
        {
            btn.interactable = false;
        }

        foreach (Button btn in preferences)
        {
            btn.interactable = true;
        }

        foreach (Button btn in menuSettings)
        {
            btn.interactable = true;
        }

        foreach (Button btn in help)
        {
            btn.interactable = true;
        }

        /*foreach (Button btn in more)
        {
            btn.interactable = true;
        }*/

        foreach (Button btn in clippingPlanes)
        {
            btn.interactable = false;
        }

        foreach (Toggle tgl in teleport)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in fly)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in touchpad)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in armswing)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in rotX)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in rotY)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in rotZ)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in scale)
        {
            tgl.interactable = false;
        }

        foreach (Button btn in hud)
        {
            btn.interactable = false;
        }

        foreach (Button btn in displays)
        {
            btn.interactable = false;
        }

        foreach (Toggle tgl in notes)
        {
            tgl.interactable = false;
        }

        foreach (Button btn in drawing)
        {
            btn.interactable = false;
        }

        foreach (Button btn in animations)
        {
            btn.interactable = false;
        }

        foreach (Button btn in timeSimulation)
        {
            btn.interactable = false;
        }

        foreach (Toggle tgl in selection)
        {
            tgl.interactable = false;
        }

        foreach (Button btn in ik)
        {
            btn.interactable = false;
        }

        foreach (Button btn in annotations)
        {
            btn.interactable = false;
        }

        foreach (Button btn in objects)
        {
            btn.interactable = false;
        }

        foreach (Button btn in remoteControl)
        {
            btn.interactable = false;
        }

        foreach (Button btn in minimap)
        {
            btn.interactable = false;
        }

        foreach (Button btn in dataDoc)
        {
            btn.interactable = false;
        }

        foreach (Toggle tgl in eraser)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in cameras)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in rulers)
        {
            tgl.interactable = false;
        }

        foreach (Toggle tgl in screens)
        {
            tgl.interactable = false;
        }
    }

    private void EnterProjectMode()
    {
        // Enable all menu options.
        EnableAllControls();
    }
#endregion
}