// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.MRET.AutoSave;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    public class ModeNavigator : MRETSingleton<ModeNavigator>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ModeNavigator);

        public List<Button> newProj, openProj, saveProj, reloadProj, reloadUser, charCust, home,
            joinSess, shareSess, config, about,
            undo, redo,
            cut, copy, paste,
            preferences, menuSettings, help, more, clippingPlanes,
            hud, displays,
            drawing, annotations, objects, outlineView,
            ik, remoteControl, minimap, dataDoc, animations,
            timeSimulation, exit;
        public List<Toggle> teleport, fly, touchpad, armswing, climb,
            rotX, rotY, rotZ, scale,
            notes, eraser, selection, motionConstraints,
            cameras, rulers, screens;

        //PROJECT AUTOSAVE - private variables to store when interrupted by autosave dialog
        private int loadType = -1;
        private ProjectType storedProjectType;
        private string storedProjectFile;
        private ProjectInfo storedProjectInfo;
        //private bool storedCollabStatus;

        public UnityEvent lobbyModeInitializationEvents;

        public void LoadLobby()
        {
            MRET.ControlMode.DisableAllControlTypes();
            MRET.ProjectManager.Unload();
            EnterLobbyMode();
            ResetRig();
        }

        public void CreateProject(string templateFile, bool collaborationEnabled)
        {
            try
            {
                // PROJECT AUTOSAVE - stop autosave
                MRET.AutosaveManager.StopAutosave();
                MRET.ConfigurationManager.AddRecentTemplate(templateFile);
                //MRET.ProjectManager.collaborationEnabled = collaborationEnabled;
                MRET.ControlMode.DisableAllControlTypes();
                MRET.ProjectManager.Unload();
                MRET.ProjectManager.LoadFromXML(templateFile);
                EnterProjectMode();
            }
            catch (Exception e)
            {
                LogError("An error occurred creating the project: " + e.Message, nameof(CreateProject));
            }
        }

        public void CreateProject(ProjectInfo templateToOpen, bool collaborationEnabled)
        {
            try
            {
                // PROJECT AUTOSAVE - stop autosave
                MRET.AutosaveManager.StopAutosave();
                MRET.ConfigurationManager.AddRecentTemplate(templateToOpen);
                //MRET.ProjectManager.collaborationEnabled = collaborationEnabled;
                MRET.ControlMode.DisableAllControlTypes();
                MRET.ProjectManager.Unload();
                MRET.ProjectManager.LoadFromXML(templateToOpen.projFile);
                EnterProjectMode();
            }
            catch (Exception e)
            {
                LogError("An error occurred creating the project: " + e.Message, nameof(CreateProject));
            }
        }

        public void OpenProject(ProjectType project, string projectFile, bool collaborationEnabled)
        {
            try
            {
                // PROJECT AUTOSAVE - stop autosave
                MRET.AutosaveManager.StopAutosave();
                // PROJECT AUTOSAVE - store filename
                MRET.AutosaveManager.storedFilename = projectFile;
                // store variables for use in OpenProject()
                loadType = 1;
                storedProjectType = project;
                storedProjectFile = projectFile;
                //storedCollabStatus = collaborationEnabled;
                // PROJECT AUTOSAVE - check for existing project autosave, prompt user if found
                if (MRET.AutosaveManager.DoesProjectAutosaveExist())
                {
                    //call LoadDetect from our child
                    AutosaveDetector ad = gameObject.GetComponent(typeof(AutosaveDetector)) as AutosaveDetector;
                    ad.LoadDetect();
                }
                else
                {
                    OpenProject();
                }
            }
            catch (Exception e)
            {
                LogError("An error occurred opening the project: " + e.Message, nameof(OpenProject));
            }
        }

        public void OpenProject(string projectFile, bool collaborationEnabled)
        {
            try
            {
                // PROJECT AUTOSAVE - stop autosave
                MRET.AutosaveManager.StopAutosave();
                // PROJECT AUTOSAVE - store filename
                MRET.AutosaveManager.storedFilename = projectFile;
                // store variables for use in OpenProject()
                loadType = 2;
                storedProjectFile = projectFile;
                //storedCollabStatus = collaborationEnabled;
                // PROJECT AUTOSAVE - check for existing project autosave, prompt user if found
                if (MRET.AutosaveManager.DoesProjectAutosaveExist())
                {
                    //call LoadDetect from our child
                    AutosaveDetector ad = gameObject.GetComponent(typeof(AutosaveDetector)) as AutosaveDetector;
                    ad.LoadDetect();
                }
                else
                {
                    OpenProject();
                }
            }
            catch (Exception e)
            {
                LogError("An error occurred opening the project: " + e.Message, nameof(OpenProject));
            }
        }

        public void OpenProject(ProjectInfo projectToOpen, bool collaborationEnabled)
        {
            try
            {
                // PROJECT AUTOSAVE - stop autosave
                MRET.AutosaveManager.StopAutosave();
                // PROJECT AUTOSAVE - store filename
                MRET.AutosaveManager.storedFilename = projectToOpen.projFile;
                // store variables for use in OpenProject()
                loadType = 3;
                storedProjectInfo = projectToOpen;
                //storedCollabStatus = collaborationEnabled;
                // PROJECT AUTOSAVE - check for existing project autosave, prompt user if found
                if (MRET.AutosaveManager.DoesProjectAutosaveExist())
                {
                    //call LoadDetect from the AutosaveDetector
                    AutosaveDetector ad = gameObject.GetComponent(typeof(AutosaveDetector)) as AutosaveDetector;
                    ad.LoadDetect();
                }
                else
                {
                    OpenProject();
                }
            }
            catch (Exception e)
            {
                LogError("An error occurred opening the project: " + e.Message, nameof(OpenProject));
            }
        }

        public void OpenProject(string projectFile) //exclusively used by autosave loads
        {
            try
            {
                // PROJECT AUTOSAVE - stop autosave
                MRET.AutosaveManager.StopAutosave();
                // PROJECT AUTOSAVE - store filename
                MRET.AutosaveManager.storedFilename = projectFile;
                // store variables for use in OpenProject()
                loadType = 1;
                storedProjectFile = projectFile;
                storedProjectType = (ProjectType)ProjectFileSchema.FromXML(projectFile);
                //storedCollabStatus = false;
                // PROJECT AUTOSAVE - bypass autosave check as we are trying to load an autosave that would be detected
                OpenProject();
            }
            catch (Exception e)
            {
                LogError("An error occurred opening the project: " + e.Message, nameof(OpenProject));
            }
        }

        public void OpenProject()
        {
            // Jordan's note - migrated the main parts of each OpenProject method to here to allow
            //                 autosave to interrupt the loading process if user decides to load the
            //                 autosave instead of the last saved version.
            try
            {
                switch (loadType)
                {
                    case 1:
                        MRET.ConfigurationManager.AddRecentProject(storedProjectFile);
                        //MRET.ProjectManager.collaborationEnabled = storedCollabStatus;
                        MRET.ControlMode.DisableAllControlTypes();
                        MRET.ProjectManager.Unload();
                        MRET.ProjectManager.InstantiateProject(storedProjectType);
                        EnterProjectMode();
                        break;

                    case 2:
                        MRET.ConfigurationManager.AddRecentProject(storedProjectFile);
                        //MRET.ProjectManager.collaborationEnabled = storedCollabStatus;
                        MRET.ControlMode.DisableAllControlTypes();
                        MRET.ProjectManager.Unload();
                        MRET.ProjectManager.LoadFromXML(storedProjectFile);
                        EnterProjectMode();
                        break;

                    case 3:
                        MRET.ConfigurationManager.AddRecentProject(storedProjectInfo);
                        //MRET.ProjectManager.collaborationEnabled = storedCollabStatus;
                        MRET.ControlMode.DisableAllControlTypes();
                        MRET.ProjectManager.Unload();
                        MRET.ProjectManager.LoadFromXML(storedProjectInfo.projFile);
                        EnterProjectMode();
                        break;

                    default:
                        Debug.LogError("[ModeNavigator] - Invalid load type in private int loadType.");
                        break;
                }
            }
            catch (Exception e)
            {
                LogError("An error occurred opening the project: " + e.Message, nameof(OpenProject));
            }
        }

        private void ResetRig()
        {
            // Move the rig to the origin
            Transform cameraRig = MRET.InputRig.transform;
            cameraRig.position = new Vector3(0f, 0f, 0f);
            cameraRig.rotation = Quaternion.identity;
            cameraRig.localScale = Vector3.one;

            // Restore gravity to the default setting
            MRET.LocomotionManager.DisableGravity();
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

            foreach (Button btn in reloadProj)
            {
                btn.interactable = true;
            }

            foreach (Button btn in reloadUser)
            {
                btn.interactable = true;
            }

            foreach (Button btn in charCust)
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

            foreach (Toggle tgl in climb)
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

            foreach (Toggle tgl in motionConstraints)
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

                case "ReloadProject":
                    foreach (Button btn in reloadProj)
                    {
                        btn.interactable = enabled;
                    }
                    break;

                case "Reload":
                    foreach (Button btn in reloadUser)
                    {
                        btn.interactable = enabled;
                    }
                    break;

                case "Edit Avatar":
                    foreach (Button btn in charCust)
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

                case "Climb":
                    foreach (Toggle tgl in climb)
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

                case "MotionConstraints":
                    foreach (Toggle tgl in motionConstraints)
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
                    LogWarning("Invalid control: " + control, nameof(SetMenuControl));
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

            // Disable all menu options and deselect all toggles except for project loading and settings.
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

            foreach (Button btn in reloadProj)
            {
                btn.interactable = false;
            }

            foreach (Button btn in reloadUser)
            {
                btn.interactable = false;
            }

            foreach (Button btn in charCust)
            {
                btn.interactable = true;
            }

            foreach (Button btn in exit)
            {
                btn.interactable = true;
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
                tgl.isOn = true;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in fly)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in touchpad)
            {
                tgl.interactable = true;
            }

            foreach (Toggle tgl in armswing)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in climb)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in rotX)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in rotY)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in rotZ)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in scale)
            {
                tgl.isOn = false;
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
                tgl.isOn = false;
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
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in motionConstraints)
            {
                tgl.isOn = false;
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
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in cameras)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in rulers)
            {
                tgl.isOn = false;
                tgl.interactable = false;
            }

            foreach (Toggle tgl in screens)
            {
                tgl.isOn = false;
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
}