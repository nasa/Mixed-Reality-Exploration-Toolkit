// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Xml;
using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Helpers;
using GOV.NASA.GSFC.XR.MRET.Animation;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms;
using GOV.NASA.GSFC.XR.MRET.Integrations.IoT;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Integrations.Matlab;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRC;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using GOV.NASA.GSFC.XR.MRET.Tools.ProceduralObjectGeneration;
using GOV.NASA.GSFC.XR.MRET.UI.Animation;
using GOV.NASA.GSFC.XR.MRET.UI.LoadingIndicator;
using GOV.NASA.GSFC.XR.MRET.Extensions.PointCloud;

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_1
{
    public partial class PartType
    {
        [XmlIgnore]
        public bool loaded = false;
        [XmlIgnore]
        public Transform transform;
    }
}

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Project.UnityProject) + " class")]
    public class UnityProjectDeprecated : MonoBehaviour
    {
        [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Extensions.VDE.VDESettings) + " class")]
        public class VDESettingsDeprecated
        {
            public bool standalone;
            public bool renderInCloud;
            public string serverAddress;
            public string nameOfBakedConfigResource;
            public string nameOfBakedEntitiesResource;
            public string nameOfBakedLinksResource;
#if MRET_EXTENSION_VDE
            public Assets.VDE.VDE.ConnectionType connectionType;
#endif
            public VDESettingsDeprecated(bool _standalone, bool _renderInCloud, string _serverAddress,
                string _bakedConfig, string _bakedEntities, string _bakedLinks
#if MRET_EXTENSION_VDE
                , Assets.VDE.VDE.ConnectionType _connectionType
#endif
                )
            {
                standalone = _standalone;
                renderInCloud = _renderInCloud;
                serverAddress = _serverAddress;
                nameOfBakedConfigResource = _bakedConfig;
                nameOfBakedEntitiesResource = _bakedEntities;
                nameOfBakedLinksResource = _bakedLinks;
#if MRET_EXTENSION_VDE
                connectionType = _connectionType;
#endif
            }
        }

        public static readonly string NAME = nameof(UnityProjectDeprecated);

        public static UnityProjectDeprecated instance;

        public GameObject lobbyArea;
        public GameObject projectObjectContainer, projectDrawingContainer, projectNoteContainer, dataSourcesContainer, animationPanelsContainer,
            pointCloudContainer, proceduralObjectGeneratorContainer, userContainer;
        public GameObject partPanelPrefab, grabCubePrefab, gmsecSourcePrefab, animationPanelPrefab;
        public Color partTouchHighlightColor;
        public float scaleMultiplier = 1;
        public bool collaborationEnabled = false;
        public SynchronizedUserDeprecated synchedUser;

        // Performance Management
        private int updateCounter = 0;
        [Tooltip("Modulates the frequency of model updates published to the DataManager. The value represents a counter modulo to determine how many calls to Update will be skipped before publishing.")]
        public int updateRateModulo = 1;

        // Optional Modules.
        public NoteManagerDeprecated noteManager;
        public MATLABClient matlabClient;
        public MATLABCommandHandler matlabCommandHandler;

        public string userAlias = "default";
        public SynchronizedUserDeprecated.LabelColor userLabelColor;
        public Guid userUUID = Guid.NewGuid(),
            lcUUID = Guid.NewGuid(), rcUUID = Guid.NewGuid(),
            lpUUID = Guid.NewGuid(), rpUUID = Guid.NewGuid();

        public bool Loaded { get; private set; } = false;

        // TODO: This is very temporary.
        public VDESettingsDeprecated vdeSettings;

        // Loaded Project Information.
        private ProjectType currentProject = null;
        private ViewType loadedProject_currentView = null;
        private string loadedProject_description = null;
        private VirtualEnvironmentType loadedProject_environment = null;
        private List<PartType> loadedProject_parts = null;
        private List<GMSECSourceType> loadedProject_gmsecSources = null;
        private List<IoTConnectionType> loadedProject_IoTConnections = null;
        private LoadingIndicatorManager loadingIndicatorManager = null;

        public void Initialize()
        {
            instance = this;

            // Locate the loading indicator
            loadingIndicatorManager = FindObjectOfType<LoadingIndicatorManager>();
        }

        public void LoadFromXML(string filePath)
        {
            // Deserialize Project File.
            XmlSerializer ser = new XmlSerializer(typeof(ProjectType));
            XmlReader reader = XmlReader.Create(filePath);
            try
            {
                ProjectType proj = (ProjectType)ser.Deserialize(reader);

                Deserialize(proj);
                reader.Close();
            }
            catch (Exception e)
            {
                Debug.Log("[UnityProjectDeprecated->LoadFromXML] " + e.ToString());
                reader.Close();
            }
        }

        public void SaveToXML(string filePath)
        {
            loadingIndicatorManager.ShowLoadingIndicator("Saving Project...");

            // Serialize to a Project File.
            XmlSerializer ser = new XmlSerializer(typeof(ProjectType));
            XmlWriter writer = XmlWriter.Create(filePath);
            try
            {
                ser.Serialize(writer, Serialize());
                writer.Close();
            }
            catch (Exception e)
            {
                Debug.Log("[UnityProjectDeprecated->SaveToXML] " + e.ToString());
                writer.Close();
            }

            loadingIndicatorManager.StopLoadingIndicator();
        }

        public void Unload()
        {
            loadingIndicatorManager.ShowLoadingIndicator("Unloading Project...");

            Debug.Log("Unloading project.");

            MRET.AutosaveManager.StopAutosave();

            // Leave collaboration session if in one.
            MRET.CollaborationManager.StopEngine();

            // Detatch all objects from the controllers.
            UngrabAllObjects();

            // Delete all GameObjects in the project.
            if (projectObjectContainer != null)
            {
                foreach (Transform projectAsset in projectObjectContainer.transform)
                {
                    Destroy(projectAsset.gameObject);
                }
            }

            // Delete all IoTClients from the IoTManagerDeprecated
            IoTManagerDeprecated.instance.ClearConnections();

            // Delete all Point Clouds in the project.
            if (pointCloudContainer != null)
            {
                foreach (Transform projectPC in pointCloudContainer.transform)
                {
                    Destroy(projectPC.gameObject);
                }
            }

            // Delete all Drawings in the project.
            if (projectDrawingContainer != null)
            {
                foreach (Transform projectDrawing in projectDrawingContainer.transform)
                {
                    Destroy(projectDrawing.gameObject);
                }
            }

            // Delete all Notes in the project.
            if (projectNoteContainer != null)
            {
                foreach (Transform projectNote in projectNoteContainer.transform)
                {
                    Destroy(projectNote.gameObject);
                }
            }

            // Delete all Animation Panels in the project.
            if (animationPanelsContainer != null)
            {
                foreach (Transform projectAnimation in animationPanelsContainer.transform)
                {
                    Destroy(projectAnimation.gameObject);
                }
            }

            // Delete all Procedural Object Generators in the project.
            if (proceduralObjectGeneratorContainer != null)
            {
                foreach (Transform proceduralObjectGenerator in proceduralObjectGeneratorContainer.transform)
                {
                    Destroy(proceduralObjectGenerator.gameObject);
                }
            }

            // Delete all Users in the project.
            if (userContainer != null)
            {
                foreach (Transform projectUser in userContainer.transform)
                {
                    Destroy(projectUser.gameObject);
                }
            }

            XRCManagerDeprecated.instance.CleanUp();

            // Show the Lobby area.
            if (MRET.InputRig.mode == InputRig.Mode.AR)
            {
                lobbyArea.SetActive(false);
            }
            else
            {
                lobbyArea.SetActive(true);
            }

            currentProject = null;

            MRET.AutosaveManager.DeleteAutosaves();

            loadingIndicatorManager.StopLoadingIndicator();
        }

        public ProjectType Serialize()
        {
            List<object> serializationItems = new List<object>();
            List<ItemsChoiceType3> serializationItemsName = new List<ItemsChoiceType3>();

            // Save Current View.
            if (loadedProject_currentView != null)
            {
                serializationItems.Add(loadedProject_currentView);
                serializationItemsName.Add(ItemsChoiceType3.CurrentView);
            }

            // Save Description.
            if (loadedProject_description != null)
            {
                serializationItems.Add(loadedProject_description);
                serializationItemsName.Add(ItemsChoiceType3.Description);
            }

            // Save Environment.
            if (loadedProject_environment != null)
            {
                serializationItems.Add(loadedProject_environment);
                serializationItemsName.Add(ItemsChoiceType3.Environment);
            }

            // Save Parts.
            if (projectObjectContainer != null)
            {
                List<PartType> partList = new List<PartType>();
                List<int> idList = new List<int>();
                int i = 0;
                Transform topLevelContainer = projectObjectContainer.transform;
                foreach (InteractablePartDeprecated part in projectObjectContainer.GetComponentsInChildren<InteractablePartDeprecated>())
                {
                    if (part.transform.parent == topLevelContainer)
                    {
                        partList.Add(part.Serialize());
                        idList.Add(++i);
                    }
                }
                PartsType parts = new PartsType();
                parts.Parts = partList.ToArray();
                parts.ID = idList.ToArray();
                serializationItems.Add(parts);
                serializationItemsName.Add(ItemsChoiceType3.Parts);
            }

            // Save Drawings.
            if (projectDrawingContainer != null)
            {
                List<DrawingType> drawingList = new List<DrawingType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (SceneObjects.Drawing.LineDrawingDeprecated lineDrawing in ProjectManager.SceneObjectManagerDeprecated.lineDrawings)
                {
                    List<Vector3Type> points = new List<Vector3Type>();
                    foreach (Vector3 point in lineDrawing.points)
                    {
                        Vector3 pt = lineDrawing.transform.TransformPoint(point);
                        points.Add(new Vector3Type()
                        {
                            X = pt.x,
                            Y = pt.y,
                            Z = pt.z
                        });
                    }
                    drawingList.Add(new DrawingType()
                    {
                        DesiredUnits = LineDrawingUnitsType.meters,
                        GUID = lineDrawing.uuid.ToString(),
                        Name = lineDrawing.name,
                        Points = points.ToArray(),
                        RenderType = lineDrawing is SceneObjects.Drawing.VolumetricDrawingDeprecated ? "cable" : "drawing",
                        Width = lineDrawing.GetWidth()
                    });
                    idList.Add(++i);
                }
                /*foreach (MeshLineRenderer projectDrawing in projectDrawingContainer.GetComponentsInChildren<MeshLineRenderer>())
                {
                    drawingList.Add(projectDrawing.drawingScript.Serialize());
                    idList.Add(++i);
                }*/
                DrawingsType drawings = new DrawingsType();
                drawings.Drawings = drawingList.ToArray();
                drawings.ID = idList.ToArray();
                serializationItems.Add(drawings);
                serializationItemsName.Add(ItemsChoiceType3.Drawings);
            }

            // Save GMSEC Sources.
            if (loadedProject_gmsecSources != null)
            {
                List<GMSECSourceType> gmsecSourceList = new List<GMSECSourceType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (GMSECSourceType gmsecSource in loadedProject_gmsecSources)
                {
                    gmsecSourceList.Add(gmsecSource);
                    idList.Add(++i);
                }
                GMSECSourcesType gmsecSources = new GMSECSourcesType();
                gmsecSources.GMSECSources = gmsecSourceList.ToArray();
                gmsecSources.ID = idList.ToArray();
                serializationItems.Add(gmsecSources);
                serializationItemsName.Add(ItemsChoiceType3.GMSECSources);
            }

            // Save IoT Sources.
            if (loadedProject_IoTConnections != null)
            {
                List<IoTConnectionType> IoTConnectionList = new List<IoTConnectionType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (IoTConnectionType IoTConnection in loadedProject_IoTConnections)
                {
                    IoTConnectionList.Add(IoTConnection);
                    idList.Add(++i);
                }
                IoTConnectionsType IoTConnections = new IoTConnectionsType();
                IoTConnections.IoTConnection = IoTConnectionList.ToArray();
                serializationItems.Add(IoTConnections);
                serializationItemsName.Add(ItemsChoiceType3.IoTConnections);
            }
            // Save Notes.
            if (projectNoteContainer != null)
            {
                List<NoteType> noteList = new List<NoteType>();
                List<int> idList = new List<int>();
                foreach (NoteDeprecated note in projectNoteContainer.GetComponentsInChildren<NoteDeprecated>())
                {
                    NoteType serializedNote = new NoteType();
                    serializedNote.Transform = new UnityTransformType();
                    serializedNote.Transform.Position = new Vector3Type();
                    serializedNote.Transform.Position.X = note.transform.position.x;
                    serializedNote.Transform.Position.Y = note.transform.position.y;
                    serializedNote.Transform.Position.Z = note.transform.position.z;
                    serializedNote.Transform.Rotation = new QuaternionType();
                    serializedNote.Transform.Rotation.X = note.transform.rotation.x;
                    serializedNote.Transform.Rotation.Y = note.transform.rotation.y;
                    serializedNote.Transform.Rotation.Z = note.transform.rotation.z;
                    serializedNote.Transform.Rotation.W = note.transform.rotation.w;
                    serializedNote.Title = note.titleText.text;
                    serializedNote.Details = note.informationText.text;
                    serializedNote.GUID = note.guid.ToString();

                    Vector3[][] serializedDrawings = note.SerializeDrawings();
                    serializedNote.Drawings = new NoteDrawingsType();
                    serializedNote.Drawings.NoteDrawings = new NoteDrawingType[serializedDrawings.Length];
                    for (int i = 0; i < serializedDrawings.Length; i++)
                    {
                        serializedNote.Drawings.NoteDrawings[i] = new NoteDrawingType();
                        serializedNote.Drawings.NoteDrawings[i].Points = new Vector3Type[serializedDrawings[i].Length];
                        for (int j = 0; j < serializedDrawings[i].Length; j++)
                        {
                            serializedNote.Drawings.NoteDrawings[i].Points[j] = new Vector3Type();
                            serializedNote.Drawings.NoteDrawings[i].Points[j].X = serializedDrawings[i][j].x;
                            serializedNote.Drawings.NoteDrawings[i].Points[j].Y = serializedDrawings[i][j].y;
                            serializedNote.Drawings.NoteDrawings[i].Points[j].Z = serializedDrawings[i][j].z;
                        }
                    }
                    noteList.Add(serializedNote);
                }
                NotesType notes = new NotesType();
                notes.Notes = noteList.ToArray();
                notes.ID = idList.ToArray();
                serializationItems.Add(notes);
                serializationItemsName.Add(ItemsChoiceType3.Notes);
            }

            // Save Animation Panels.
            if (animationPanelsContainer != null)
            {
                List<AnimationPanelType> serializedPanels = new List<AnimationPanelType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (AnimationPanelsMenuController panel in animationPanelsContainer.GetComponentsInChildren<AnimationPanelsMenuController>())
                {
                    AnimationPanelType serializedPanel = new AnimationPanelType();

                    if (panel.animationPanel)
                    {
                        AnimationMenuController menuContr = panel.animationPanel.GetComponent<AnimationMenuController>();
                        if (menuContr)
                        {
                            //serializedPanel.Animation =
                            //    menuContr.activeAnimation.Serialize(menuContr.loopToggle.isOn, menuContr.autoplayToggle.isOn);
                            serializedPanel.Position = new Vector3Type()
                            {
                                X = panel.transform.position.x,
                                Y = panel.transform.position.y,
                                Z = panel.transform.position.z
                            };
                            serializedPanel.Rotation = new QuaternionType()
                            {
                                X = panel.transform.rotation.x,
                                Y = panel.transform.rotation.y,
                                Z = panel.transform.rotation.z,
                                W = panel.transform.rotation.w
                            };
                        }
                    }

                    serializedPanels.Add(serializedPanel);
                    idList.Add(++i);
                }
                AnimationPanelsType panelsType = new AnimationPanelsType();
                panelsType.AnimationPanels = serializedPanels.ToArray();
                panelsType.index = idList.ToArray();
                serializationItems.Add(panelsType);
                serializationItemsName.Add(ItemsChoiceType3.AnimationPanels);
            }

            // Store in Serialization Destination.
            ProjectType serializationDestination = new ProjectType();
            serializationDestination.Items = serializationItems.ToArray();
            serializationDestination.ItemsElementName = serializationItemsName.ToArray();
            return serializationDestination;
        }

        public void Deserialize(ProjectType serializedProject)
        {
            // Hide the Lobby area.
            lobbyArea.SetActive(false);

            // Indicate that the project is loading.
            loadingIndicatorManager.ShowLoadingIndicator("Loading Project...");

            currentProject = serializedProject;

            loadedProject_gmsecSources = new List<GMSECSourceType>();
            loadedProject_IoTConnections = new List<IoTConnectionType>();

            try
            {
                if (serializedProject.Items.Length == serializedProject.ItemsElementName.Length)
                {
                    for (int i = 0; i < serializedProject.Items.Length; i++)
                    {
                        switch (serializedProject.ItemsElementName[i])
                        {
                            case ItemsChoiceType3.AnimationPanels:
                                AnimationPanelsType panels = (AnimationPanelsType)serializedProject.Items[i];

                                if (panels != null)
                                {
                                    if (panels.AnimationPanels != null)
                                    {
                                        foreach (AnimationPanelType panel in panels.AnimationPanels)
                                        {
                                            GameObject instantiatedPanel = Instantiate(animationPanelPrefab);
                                            instantiatedPanel.transform.position = new Vector3(panel.Position.X, panel.Position.Y, panel.Position.Z);
                                            instantiatedPanel.transform.rotation = new Quaternion(panel.Rotation.X, panel.Rotation.Y, panel.Rotation.Z, panel.Rotation.W);

                                            AnimationMenuController panelController = instantiatedPanel.GetComponentInChildren<AnimationMenuController>(true);
                                            if (panelController)
                                            {
                                                //MRETAnimationGroup anim = MRETAnimationGroup.Deserialize(panel.Animation);
                                                //panelController.loopToggle.isOn = anim.LoopCount > 1;
                                                //panelController.autoplayToggle.isOn = anim.Autostart;
                                                //panelController.SetAnimation(anim);
                                            }
                                        }
                                    }
                                }
                                break;

                            case ItemsChoiceType3.CurrentView:
                                loadedProject_currentView = (ViewType)serializedProject.Items[i];
                                /*Debug.Log("setting view");
                                ViewType viewToGet = (ViewType) serializedProject.Items[i];
                                Transform transformToSet = headsetFollower.transform.parent.parent;
                                transformToSet.position = new Vector3(viewToGet.Location.X, viewToGet.Location.Y, viewToGet.Location.Z);
                                transformToSet.rotation = Quaternion.Euler(viewToGet.Rotation.X, viewToGet.Rotation.Y, viewToGet.Rotation.Z);
                                transformToSet.localScale = new Vector3(1 / viewToGet.Zoom, 1 / viewToGet.Zoom, 1 / viewToGet.Zoom);*/
                                break;

                            case ItemsChoiceType3.Description:
                                loadedProject_description = (string)serializedProject.Items[i];
                                break;

                            case ItemsChoiceType3.Environment:
                                loadedProject_environment = (VirtualEnvironmentType)serializedProject.Items[i];

                                // Load the Scene.
                                StartCoroutine(InitializeLevelAsync(loadedProject_environment, true));

                                // Load the Skybox.
                                RenderSettings.skybox = Resources.Load("Skyboxes/" + loadedProject_environment.Skybox.MaterialName, typeof(Material)) as Material;

                                // Place the user.
                                float yOffset = 0f;
                                Transform transformToSet = MRET.InputRig.transform;
                                transformToSet.position = new Vector3(loadedProject_environment.DefaultUserTransform.Position.X,
                                    loadedProject_environment.DefaultUserTransform.Position.Y + yOffset, loadedProject_environment.DefaultUserTransform.Position.Z);
                                transformToSet.rotation = new Quaternion(
                                    loadedProject_environment.DefaultUserTransform.Rotation.X,
                                    loadedProject_environment.DefaultUserTransform.Rotation.Y,
                                    loadedProject_environment.DefaultUserTransform.Rotation.Z,
                                    loadedProject_environment.DefaultUserTransform.Rotation.W);
                                    //Quaternion.Euler(loadedProject_environment.DefaultUserTransform.Rotation.X,
                                    //    loadedProject_environment.DefaultUserTransform.Rotation.Y, loadedProject_environment.DefaultUserTransform.Rotation.Z);
                                transformToSet.localScale = new Vector3(loadedProject_environment.DefaultUserTransform.Scale.X,
                                    loadedProject_environment.DefaultUserTransform.Scale.Y, loadedProject_environment.DefaultUserTransform.Scale.Z);

                                // Apply locomotion settings.
                                if (loadedProject_environment.LocomotionSettings != null)
                                {
                                    if (loadedProject_environment.LocomotionSettings.ArmswingSpeed > 0)
                                    {
                                        MRET.LocomotionManager.ArmswingNormalMotionConstraintMultiplier
                                            = loadedProject_environment.LocomotionSettings.ArmswingSpeed;
                                    }
                                        
                                    if (loadedProject_environment.LocomotionSettings.FlySpeed > 0)
                                    {
                                        MRET.LocomotionManager.FlyingNormalMotionConstraintMultiplier
                                            = loadedProject_environment.LocomotionSettings.FlySpeed;
                                    }
                                        
                                    if (loadedProject_environment.LocomotionSettings.TeleportDistance > 0)
                                    {
                                        MRET.LocomotionManager.TeleportMaxDistance
                                            = loadedProject_environment.LocomotionSettings.TeleportDistance;
                                    }
                                        
                                    if (loadedProject_environment.LocomotionSettings.TouchpadSpeed > 0)
                                    {
                                        MRET.LocomotionManager.NavigationNormalMotionConstraintMultiplier
                                            = loadedProject_environment.LocomotionSettings.TouchpadSpeed;
                                    }
                                }
                                    
                                Debug.Log("Setting up controlled user...");
                                if (collaborationEnabled)
                                {
                                    synchedUser = transformToSet.GetComponentInChildren<Camera>().gameObject.AddComponent<SynchronizedUserDeprecated>();
                                    synchedUser.userObject = MRET.InputRig.gameObject;
                                    synchedUser.userAlias = userAlias;
                                    synchedUser.isControlled = true;
                                    synchedUser.uuid = userUUID;
                                    synchedUser.userLabelColor = userLabelColor;

                                    InputHand leftHand = MRET.InputRig.leftHand;
                                    InputHand rightHand = MRET.InputRig.rightHand;
                                    if (leftHand == null && rightHand == null)
                                    {
                                        List<InputHand> hands = MRET.InputRig.hands;
                                        if (hands.Count != 2)
                                        {
                                            Debug.LogError("Incorrect number of hands. Will not load project correctly.");
                                        }
                                        else
                                        {
                                            leftHand = hands[0];
                                            rightHand = hands[1];
                                        }
                                    }

                                    synchedUser.leftController = leftHand.gameObject.AddComponent<SynchronizedControllerDeprecated>();
                                    synchedUser.leftController.controllerSide = SynchronizedControllerDeprecated.ControllerSide.Left;
                                    synchedUser.leftController.synchronizedUser = synchedUser;
                                    synchedUser.leftController.uuid = lcUUID;

                                    GameObject leftLaser = new GameObject("Laser");
                                    leftLaser.transform.parent = leftHand.transform;
                                        
                                    synchedUser.leftController.pointer = leftLaser.AddComponent<SynchronizedPointerDeprecated>();
                                    synchedUser.leftController.pointer.synchronizedController = synchedUser.leftController;
                                    synchedUser.leftController.pointer.uuid = lpUUID;
                                    synchedUser.leftController.pointer.hand = leftHand;

                                    synchedUser.rightController = rightHand.gameObject.AddComponent<SynchronizedControllerDeprecated>();
                                    synchedUser.rightController.controllerSide = SynchronizedControllerDeprecated.ControllerSide.Right;
                                    synchedUser.rightController.synchronizedUser = synchedUser;
                                    synchedUser.rightController.uuid = rcUUID;

                                    GameObject rightLaser = new GameObject("Laser");
                                    rightLaser.transform.parent = rightHand.transform;
                                    synchedUser.rightController.pointer = rightLaser.AddComponent<SynchronizedPointerDeprecated>();
                                    synchedUser.rightController.pointer.synchronizedController = synchedUser.rightController;
                                    synchedUser.rightController.pointer.uuid = rpUUID;
                                    synchedUser.rightController.pointer.hand = rightHand;

                                    Debug.Log("Synchronized user added.");

                                    CollaborationManagerDeprecated collabMgr = FindObjectOfType<CollaborationManagerDeprecated>();
                                    if (collabMgr.engineType == CollaborationManagerDeprecated.EngineType.XRC)
                                    {
                                        Debug.Log("Engine is XRC. Adding synchronized user...");
                                        collabMgr.xrcManager.synchedUsers.Add(synchedUser);
                                        Debug.Log("Synchronized user added. User count is "
                                            + collabMgr.xrcManager.synchedUsers.Count + ".");
                                    }
                                    else
                                    {
                                        Debug.Log("Engine is Legacy. Adding synchronized user...");
                                        collabMgr.masterNode.synchronizedUsers.Add(synchedUser);
                                        Debug.Log("Synchronized user added. User count is "
                                            + collabMgr.masterNode.synchronizedUsers.Count);
                                    }
                                }

                                // Set the user's clipping planes.
                                Camera headsetCam = MRET.InputRig.activeCamera;
                                if (headsetCam != null)
                                {
                                    headsetCam.nearClipPlane = loadedProject_environment.ClippingPlanes.Near;
                                    headsetCam.farClipPlane = loadedProject_environment.ClippingPlanes.Far;
                                }

                                // Force realtime reflection probes to reinitialize.
                                foreach (ReflectionProbe rProbe in projectObjectContainer.GetComponentsInChildren<ReflectionProbe>())
                                {
                                    if (rProbe.mode == UnityEngine.Rendering.ReflectionProbeMode.Realtime)
                                    {
                                        rProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
                                        rProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                                    }
                                }
                                break;

                            case ItemsChoiceType3.Parts:
                                PartsType parts = (PartsType) serializedProject.Items[i];

                                // Load each part into the environment.
                                if (parts.Parts != null)
                                {
                                    foreach (PartType part in parts.Parts)
                                    {
                                        ProjectManager.PartManagerDeprecated.InstantiatePartInEnvironment(part, null);
                                    }
                                }
                                break;

                            case ItemsChoiceType3.Drawings:
                                DrawingsType drawings = (DrawingsType)serializedProject.Items[i];

                                // Load each drawing into the environment.
                                if (drawings != null && drawings.Drawings != null)
                                {
                                    foreach (DrawingType drawing in drawings.Drawings)
                                    {
                                        ProjectManager.SceneObjectManagerDeprecated.CreateLineDrawing(drawing.Name, null,
                                            Vector3.zero, Quaternion.identity, Vector3.one,
                                            drawing.RenderType.ToLower() == "drawing" || drawing.RenderType == "measurement"
                                                ? LineDrawingManagerDeprecated.DrawingType.Basic
                                                : LineDrawingManagerDeprecated.DrawingType.Volumetric,
                                            drawing.Width, Color.green, DeserializeVector3ArrayToList(drawing.Points).ToArray(),
                                            Guid.Parse(drawing.GUID));
                                        //drawLineManager.AddPredefinedDrawing(DeserializeVector3ArrayToList(drawing.Points),
                                        //    (LineDrawing.RenderTypes)Enum.Parse(typeof(LineDrawing.RenderTypes), drawing.RenderType.ToString()),
                                        //    (LineDrawing.unit)Enum.Parse(typeof(LineDrawing.unit), drawing.DesiredUnits.ToString()),
                                        //    drawing.Name, new Guid(drawing.GUID));
                                    }
                                }
                                break;

                            case ItemsChoiceType3.GMSECSources:
                                GMSECSourcesType gmsecSources = (GMSECSourcesType)serializedProject.Items[i];
                                if (gmsecSources.GMSECSources != null)
                                {
                                    foreach (GMSECSourceType gmsecSource in gmsecSources.GMSECSources)
                                    {
                                        loadedProject_gmsecSources.Add(gmsecSource);
                                        GameObject gmsecSourceObject = Instantiate(gmsecSourcePrefab);
                                        gmsecSourceObject.transform.SetParent(dataSourcesContainer.transform);
                                        GMSECBusToDataManager gmsecListener = gmsecSourceObject.GetComponent<GMSECBusToDataManager>();

                                        switch (gmsecSource.ConnectionType)
                                        {
                                            case "gmsec_activemq383":
                                                gmsecListener.connectionType = GMSECBusToDataManager.ConnectionTypes.amq383;
                                                break;

                                            case "gmsec_activemq384":
                                                gmsecListener.connectionType = GMSECBusToDataManager.ConnectionTypes.amq384;
                                                break;

                                            case "gmsec_mb":
                                                gmsecListener.connectionType = GMSECBusToDataManager.ConnectionTypes.mb;
                                                break;

                                            case "gmsec_websphere71":
                                                gmsecListener.connectionType = GMSECBusToDataManager.ConnectionTypes.ws71;
                                                break;

                                            case "gmsec_websphere75":
                                                gmsecListener.connectionType = GMSECBusToDataManager.ConnectionTypes.ws75;
                                                break;

                                            case "gmsec_websphere80":
                                                gmsecListener.connectionType = GMSECBusToDataManager.ConnectionTypes.ws80;
                                                break;

                                            case "gmsec_bolt":
                                            default:
                                                gmsecListener.connectionType = GMSECBusToDataManager.ConnectionTypes.bolt;
                                                break;
                                        }
                                        gmsecListener.Server = gmsecSource.Server;
                                        gmsecListener.subject = gmsecSource.Subject;

                                        // Convert the update frequency
                                        gmsecListener.updateRate = UpdateFrequency.HzCustom;
                                        gmsecListener.customRate = (gmsecSource.ReadFrequency != default) ? gmsecSource.ReadFrequency : int.MaxValue;
                                    }
                                }
                                break;

                            case ItemsChoiceType3.MatlabConnection:
                                MatlabConnectionType matlabConnection = (MatlabConnectionType)serializedProject.Items[i];

                                if (matlabConnection != null)
                                {
                                    if (matlabConnection.host == null)
                                    {
                                        matlabConnection.host = "127.0.0.1";
                                    }

                                    if (matlabConnection.port < 1 || matlabConnection.port > 65535)
                                    {
                                        matlabConnection.port = 25525;
                                    }

                                    if (matlabConnection.send || matlabConnection.receive)
                                    {
                                        matlabClient.Server = matlabConnection.host;
                                        matlabClient.Port = matlabConnection.port;

                                        if (matlabConnection.send)
                                        {
                                            matlabCommandHandler.EnableSender();
                                        }
                                        else
                                        {
                                            matlabCommandHandler.DisableSender();
                                        }

                                        if (matlabConnection.receive)
                                        {
                                            matlabCommandHandler.EnableReceiver();
                                        }
                                        else
                                        {
                                            matlabCommandHandler.DisableReceiver();
                                        }
                                    }
                                }
                                break;
                            case ItemsChoiceType3.IoTThings:
                                IoTThingsType iotThings = (IoTThingsType)serializedProject.Items[i];
                                IoTManagerDeprecated.instance.AddThings(iotThings);
                                break;
                            case ItemsChoiceType3.IoTConnections:
                                IoTConnectionsType iotConnections = (IoTConnectionsType)serializedProject.Items[i];
                                IoTManagerDeprecated.instance.AddConnections(iotConnections);
                                break;
                            case ItemsChoiceType3.Notes:
                                NotesType notes = (NotesType)serializedProject.Items[i];

                                // Load each note into the environment.
                                if (notes.Notes != null)
                                {
                                    foreach (NoteType note in notes.Notes)
                                    {
                                        Vector3Type notePosition = note.Transform.Position;
                                        QuaternionType noteRotation = note.Transform.Rotation;
                                        NoteDeprecated.NoteData noteData = new NoteDeprecated.NoteData();
                                        noteData.pos = new Vector3(notePosition.X, notePosition.Y, notePosition.Z);
                                        noteData.rot = new Quaternion(noteRotation.X, noteRotation.Y, noteRotation.Z, noteRotation.W);
                                        noteData.title = note.Title;
                                        noteData.information = note.Details;
                                        NoteDeprecated.fromSerializable(noteData, note.Drawings.NoteDrawings, noteManager.noteCount++, new Guid(note.GUID));
                                    }
                                }
                                break;

                            case ItemsChoiceType3.PointClouds:
                                StaticPointCloudsType pointClouds = (StaticPointCloudsType)serializedProject.Items[i];

                                // Load each point cloud into the environment.
                                if (pointClouds.StaticPointClouds != null)
                                {
#if MRET_EXTENSION_POINTCLOUDVIEWER
                                    foreach (StaticPointCloudType pointCloud in pointClouds.StaticPointClouds)
                                    {
                                        Vector3Type pcPosition = pointCloud.Position;
                                        QuaternionType pcRotation = pointCloud.Rotation;
                                        NonNegativeFloat3Type pcScale = pointCloud.Scale;
                                        string pcName = pointCloud.Name;
                                        string pcPath = pointCloud.Path;
                                        int pcIndex = pointCloud.LODIndex;
                                        NewPointCloudLoader.InstantiatePointCloud_s(
                                            new Vector3(pcPosition.X, pcPosition.Y, pcPosition.Z),
                                            new Quaternion(pcRotation.X, pcRotation.Y, pcRotation.Z, pcRotation.W),
                                            new Vector3(pcScale.X, pcScale.Y, pcScale.Z),
                                            pointCloud.Path, pointCloud.Name, null, pointCloudContainer);
                                    }
#else
                                    Debug.LogWarning("Point Cloud Viewer unavailable");
#endif
                                }
                                break;

                            case ItemsChoiceType3.ObjectGenerators:
                                ObjectGeneratorsType ogts = (ObjectGeneratorsType)serializedProject.Items[i];

                                // Load each object generator into the environment.
                                if (ogts != null)
                                {
                                    foreach (ObjectGeneratorType ogt in ogts.ObjectGenerator)
                                    {
                                        StartCoroutine(ProceduralObjectGeneratorManagerDeprecated.instance.AddGenerator(ogt));
                                    }
                                }
                                break;

                            case ItemsChoiceType3.VDEConnection:
                                VDEConnectionType vct = (VDEConnectionType)serializedProject.Items[i];
                                if (vct != null)
                                {
                                    if (string.IsNullOrEmpty(vct.serverURL))
                                    {
                                        vct.serverURL = "http://127.0.0.1/VDE";
                                    }

                                    if (string.IsNullOrEmpty(vct.bakedConfigResource))
                                    {
                                        vct.bakedConfigResource = "config";
                                    }

                                    if (string.IsNullOrEmpty(vct.bakedEntitiesResource))
                                    {
                                        vct.bakedEntitiesResource = "entities";
                                    }

                                    if (string.IsNullOrEmpty(vct.bakedLinksResource))
                                    {
                                        vct.bakedLinksResource = "links";
                                    }

#if MRET_EXTENSION_VDE
                                    Assets.VDE.VDE.ConnectionType connectionType = Assets.VDE.VDE.ConnectionType.GMSEC;
                                    if (vct.connectionType == VDEConnectionTypeType.SIGNALR)
                                    {
                                        connectionType = Assets.VDE.VDE.ConnectionType.SIGNALR;
                                    }
#endif

                                    vdeSettings = new VDESettingsDeprecated(vct.standalone, vct.renderInCloud,
                                        vct.serverURL, vct.bakedConfigResource,
                                        vct.bakedEntitiesResource, vct.bakedLinksResource
#if MRET_EXTENSION_VDE
                                        , connectionType
#endif
                                        );
                                }

                                StartCoroutine(InitializeVDE(vct));
                                break;

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("[UnityProjectDeprecated->Deserialize] Invalid Serialized Project.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[UnityProjectDeprecated->Deserialize] " + e.ToString());
            }

            // TODO: Very temporary.
            foreach (Assets.VDE.VDE vde in FindObjectsOfType<Assets.VDE.VDE>())
            {
                //vde.Init(vdeSettings.standalone, vdeSettings.renderInCloud,
                //    vdeSettings.serverURL, vdeSettings.nameOfBakedConfigResource,
                //    vdeSettings.nameOfBakedEntitiesResource, vdeSettings.nameOfBakedLinksResource);
            }

            // Indicate that project is done loading.
            loadingIndicatorManager.StopLoadingIndicator();
        }

        public void ReloadProject()
        {
            if (currentProject != null)
            {
                ProjectType projectToLoad = currentProject;
                Unload();
                Deserialize(projectToLoad);
            }
            else
            {
                Debug.LogError("[UnityProjectDeprecated->ReloadProject] No project to reload.");
            }
        }

        public void ResetUser()
        {
            if (loadedProject_environment != null)
            {
                // Place the user.
                float yOffset = 0f;
                Transform transformToSet = MRET.InputRig.transform;
                transformToSet.position = new Vector3(loadedProject_environment.DefaultUserTransform.Position.X,
                    loadedProject_environment.DefaultUserTransform.Position.Y + yOffset, loadedProject_environment.DefaultUserTransform.Position.Z);
                transformToSet.rotation = new Quaternion(
                    loadedProject_environment.DefaultUserTransform.Rotation.X,
                    loadedProject_environment.DefaultUserTransform.Rotation.Y,
                    loadedProject_environment.DefaultUserTransform.Rotation.Z,
                    loadedProject_environment.DefaultUserTransform.Rotation.W);
                //Quaternion.Euler(loadedProject_environment.DefaultUserTransform.Rotation.X,
                //    loadedProject_environment.DefaultUserTransform.Rotation.Y, loadedProject_environment.DefaultUserTransform.Rotation.Z);
                transformToSet.localScale = new Vector3(loadedProject_environment.DefaultUserTransform.Scale.X,
                    loadedProject_environment.DefaultUserTransform.Scale.Y, loadedProject_environment.DefaultUserTransform.Scale.Z);
            }
            else
            {
                Debug.LogError("[UnityProjectDeprecated->ResetUser] No environment settings.");
            }
        }

        // TODO: The parts should be indexed into a dictionary to enable efficient searching. Move to part manager.
        public InteractablePartDeprecated GetPartByUUID(Guid guidToGet)
        {
            foreach (InteractablePartDeprecated iPart in projectObjectContainer.GetComponentsInChildren<InteractablePartDeprecated>())
            {
                if (iPart.guid == guidToGet)
                {
                    return iPart;
                }
            }

            // Less efficient, if first check fails to find part.
            foreach (InteractablePartDeprecated iPart in FindObjectsOfType<InteractablePartDeprecated>())
            {
                if (iPart.guid == guidToGet)
                {
                    return iPart;
                }
            }

            return null;
        }

        public SynchronizedUserDeprecated GetUserByUUID(Guid guidToGet)
        {
            CollaborationManagerDeprecated collabMgr = FindObjectOfType<CollaborationManagerDeprecated>();
            if (collabMgr.engineType == CollaborationManagerDeprecated.EngineType.XRC)
            {
                return collabMgr.xrcManager.synchedUsers.Find(x => x.uuid == guidToGet);
            }
            else
            {
                return collabMgr.masterNode.synchronizedUsers.Find(x => x.uuid == guidToGet);
            }
        }

        public SynchronizedControllerDeprecated GetControllerByUUID(Guid guidToGet)
        {
            CollaborationManagerDeprecated collabMgr = FindObjectOfType<CollaborationManagerDeprecated>();
            if (collabMgr.engineType == CollaborationManagerDeprecated.EngineType.XRC)
            {
                foreach (SynchronizedUserDeprecated user in collabMgr.xrcManager.synchedUsers)
                {
                    if (user.leftController.uuid == guidToGet)
                    {
                        return user.leftController;
                    }
                    else if (user.rightController)
                    {
                        if (user.rightController.uuid == guidToGet)
                        {
                            return user.rightController;
                        }
                    }
                }
                return null;
            }
            else
            {
                foreach (SynchronizedUserDeprecated user in collabMgr.masterNode.synchronizedUsers)
                {
                    if (user.leftController.uuid == guidToGet)
                    {
                        return user.leftController;
                    }
                    else if (user.rightController.uuid == guidToGet)
                    {
                        return user.rightController;
                    }
                }
                return null;
            }
        }

        public Vector3 GlobalToLocalPosition(Vector3 globalPosition)
        {
            return transform.InverseTransformPoint(globalPosition);
        }

        public Vector3 LocalToGlobalPosition(Vector3 localPosition)
        {
            return transform.TransformPoint(localPosition);
        }

        // TODO: Need Scene Manager.
        void FinishSceneInstantiation(bool loaded, VirtualEnvironmentType environment)
        {
            if (loaded == false)
            {
                Debug.LogError("Error loading scene.");
                return;
            }

            Debug.Log("Scene " + environment.Name + " loaded. Setting up.");
            
            // Capture all GameObjects in scene and place them in hierarchy.
            GameObject[] goArray = UnityEngine.SceneManagement.SceneManager.GetSceneByName(environment.Name).GetRootGameObjects();
            if (goArray.Length > 0)
            {
                foreach (GameObject sceneObject in goArray)
                {
                    sceneObject.transform.SetParent(projectObjectContainer.transform);
                }
            }

            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetSceneByName(environment.Name));

            // Reload all reflection probes.
            ReflectionProbe[] refProbes = FindObjectsOfType<ReflectionProbe>();
            foreach (ReflectionProbe refProbe in refProbes)
            {
                if (refProbe.enabled)
                {
                    refProbe.enabled = false;
                    refProbe.enabled = true;
                }
            }

            // Reinitialize TouchControl dropdowns.
            foreach (DualAxisRotationControl control in FindObjectsOfType<DualAxisRotationControl>())
            {
                if (control.enabled)
                {
                    control.enabled = false;
                    control.enabled = true;
                }
            }

            // Reinitialize all renderers (Unity bug).
            foreach (Renderer renderer in projectObjectContainer.GetComponentsInChildren<Renderer>())
            {
                if (renderer.enabled)
                {
                    renderer.enabled = false;
                    renderer.enabled = true;
                }
            }

            loaded = true;
        }

        protected IEnumerator InitializeLevelAsync(VirtualEnvironmentType environment, bool isAdditive)
        {
            // Load level from assetBundle.
            Action<bool> action = (bool loaded) =>
            {
                FinishSceneInstantiation(loaded, environment);
            };
            AssetBundleHelper.Instance.LoadSceneAsync(environment.AssetBundle,
                environment.Name, isAdditive, action);

            yield return null;

        }

        private Vector3 DeserializeVector3(Vector3Type input)
        {
            if (input == null) return new Vector3();
            return new Vector3(input.X, input.Y, input.Z);
        }

        private Vector3[] DeserializeVector3Array(Vector3Type[] input)
        {
            List<Vector3> outVec = new List<Vector3>();
            if (input != null)
            {
                foreach (Vector3Type vec in input)
                {
                    if (vec != null)
                    {
                        outVec.Add(DeserializeVector3(vec));
                    }
                }
            }

            return outVec.ToArray();
        }

        private List<Vector3> DeserializeVector3ArrayToList(Vector3Type[] input)
        {
            List<Vector3> outVec = new List<Vector3>();
            if (input != null)
            {
                foreach (Vector3Type vec in input)
                {
                    if (vec != null)
                    {
                        outVec.Add(DeserializeVector3(vec));
                    }
                }
            }

            return outVec;
        }

        private void ApplyTelemetryTransformsToPart(GameObject obj, PartType part)
        {
            if (part == null)
            {
                return;
            }

            if (part.TelemetryTransforms == null)
            {
                return;
            }

            if (part.TelemetryTransforms.TelemetryTransform == null)
            {
                return;
            }

            foreach (TelemetryTransformType ttft in part.TelemetryTransforms.TelemetryTransform)
            {
                TelemetryTransform ttf = obj.AddComponent<TelemetryTransform>();

                // Set object to control.
                ttf.objectToControl = obj;

                // Set controlled attribute.
                switch (ttft.ControlledAttribute)
                {
                    case TelemetryTransformAttributeType.GlobalPosition:
                        ttf.attributeToControl = TelemetryTransform.TransformAttribute.GlobalPosition;
                        break;

                    case TelemetryTransformAttributeType.GlobalRotation:
                        ttf.attributeToControl = TelemetryTransform.TransformAttribute.GlobalRotation;
                        break;

                    case TelemetryTransformAttributeType.LocalPosition:
                        ttf.attributeToControl = TelemetryTransform.TransformAttribute.LocalPosition;
                        break;

                    case TelemetryTransformAttributeType.LocalRotation:
                        ttf.attributeToControl = TelemetryTransform.TransformAttribute.LocalRotation;
                        break;

                    case TelemetryTransformAttributeType.Scale:
                        ttf.attributeToControl = TelemetryTransform.TransformAttribute.Scale;
                        break;

                    case TelemetryTransformAttributeType.None:
                    default:
                        ttf.attributeToControl = TelemetryTransform.TransformAttribute.None;
                        break;
                }

                // Set point information.
                ttf.xPointName = ttft.XPointInfo.Name;
                ttf.useXPointValue = ttft.XPointInfo.UseValue;
                ttf.invertXPointValue = ttft.XPointInfo.Invert;
                ttf.xPointOffset = ttft.XPointInfo.Offset;
                ttf.yPointName = ttft.YPointInfo.Name;
                ttf.useYPointValue = ttft.YPointInfo.UseValue;
                ttf.invertYPointValue = ttft.YPointInfo.Invert;
                ttf.yPointOffset = ttft.YPointInfo.Offset;
                ttf.zPointName = ttft.ZPointInfo.Name;
                ttf.useZPointValue = ttft.ZPointInfo.UseValue;
                ttf.invertZPointValue = ttft.ZPointInfo.Invert;
                ttf.zPointOffset = ttft.ZPointInfo.Offset;
                ttf.wPointName = ttft.WPointInfo.Name;
                ttf.useWPointValue = ttft.WPointInfo.UseValue;
                ttf.invertWPointValue = ttft.WPointInfo.Invert;
                ttf.wPointOffset = ttft.WPointInfo.Offset;

                // Set radians.
                ttf.valuesAreInRadians = ttft.Radians;

                // Set quaternions.
                ttf.useQuaternions = ttft.Quaternions;

                // Set frequency.
                ttf.updateFrequency = ttft.Frequency;
            }

            return;
        }

        private IEnumerator InitializeVDE(VDEConnectionType vct)
        {
#if MRET_EXTENSION_VDE
            float timeout = UnityEngine.Time.realtimeSinceStartup + 60;
            while (timeout > UnityEngine.Time.realtimeSinceStartup)
            {
                Assets.VDE.VDE[] vde = FindObjectsOfType<Assets.VDE.VDE>();
                if (!(vde is null) && vde.Length > 0)
                {
                    Debug.Log("VDE found, initializing.");
                    vde[0].Init(vdeSettings.standalone, vdeSettings.renderInCloud,
                        vdeSettings.serverAddress, vdeSettings.nameOfBakedConfigResource,
                        vdeSettings.nameOfBakedEntitiesResource, vdeSettings.nameOfBakedLinksResource, Assets.VDE.VDE.InputSource.MRET, vdeSettings.connectionType);
                    break;
                }
                else
                {
                    Debug.Log("No VDE found yet.");
                }
                yield return new WaitForSeconds(1f);
            }
#else
            Debug.Log("VDE not installed.");
#endif
            yield return true;
        }

        private void UngrabAllObjects()
        {
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                hand.GrabComplete();
            }
        }

        void Update()
        {
            // Performance management
            updateCounter++;
            if (updateCounter >= updateRateModulo)
            {
                // Reset the update counter
                updateCounter = 0;

                // TODO: Not sure what this is used for, and can it be eliminated if we are now placing the transforms into the DataManager?
                scaleMultiplier = transform.localScale.x;
            }
        }

    }

}