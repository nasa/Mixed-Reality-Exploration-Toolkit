// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Xml;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Components.AssetBundles;
using PointCloud;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using GSFC.ARVR.MRET.ProceduralObjectGeneration;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Infrastructure.Framework.Animation;
using GSFC.ARVR.MRET.Components.Notes;

namespace GSFC.ARVR.MRET.Common.Schemas
{
    public partial class PartType
    {
        [XmlIgnore]
        public bool loaded = false;
        [XmlIgnore]
        public Transform transform;
    }

    public class UnityProject : MonoBehaviour
    {
        public class VDESettings
        {
            public bool standalone;
            public bool renderInCloud;
            public string serverAddress;
            public string nameOfBakedConfigResource;
            public string nameOfBakedEntitiesResource;
            public string nameOfBakedLinksResource;
            public Assets.VDE.VDE.ConnectionType connectionType;

            public VDESettings(bool _standalone, bool _renderInCloud, string _serverAddress,
                string _bakedConfig, string _bakedEntities, string _bakedLinks, Assets.VDE.VDE.ConnectionType _connectionType)
            {
                standalone = _standalone;
                renderInCloud = _renderInCloud;
                serverAddress = _serverAddress;
                nameOfBakedConfigResource = _bakedConfig;
                nameOfBakedEntitiesResource = _bakedEntities;
                nameOfBakedLinksResource = _bakedLinks;
                connectionType = _connectionType;
            }
        }

        public static readonly string NAME = nameof(UnityProject);

        public static UnityProject instance;

        public GameObject lobbyArea;
        public GameObject projectObjectContainer, projectDrawingContainer, projectNoteContainer, dataSourcesContainer, animationPanelsContainer,
            pointCloudContainer, proceduralObjectGeneratorContainer, userContainer;
        public GameObject partPanelPrefab, grabCubePrefab, gmsecSourcePrefab, animationPanelPrefab;
        public Color partTouchHighlightColor;
        public float scaleMultiplier = 1;
        public bool collaborationEnabled = false;
        public SynchronizedUser synchedUser;

        // Performance Management
        private int updateCounter = 0;
        [Tooltip("Modulates the frequency of model updates published to the DataManager. The value represents a counter modulo to determine how many calls to Update will be skipped before publishing.")]
        public int updateRateModulo = 1;

        // Optional Modules.
        public DrawLineManager drawLineManager;
        public NoteManager noteManager;
        public MATLABClient matlabClient;
        public MATLABCommandHandler matlabCommandHandler;

        public string userAlias = "default";
        public SynchronizedUser.LabelColor userLabelColor;
        public Guid userUUID = Guid.NewGuid(),
            lcUUID = Guid.NewGuid(), rcUUID = Guid.NewGuid(),
            lpUUID = Guid.NewGuid(), rpUUID = Guid.NewGuid();

        public bool Loaded { get; private set; } = false;

        // TODO: This is very temporary.
        public VDESettings vdeSettings;

        // Loaded Project Information.
        private ProjectType currentProject = null;
        private ViewType loadedProject_currentView = null;
        private string loadedProject_description = null;
        private VirtualEnvironmentType loadedProject_environment = null;
        private List<PartType> loadedProject_parts = null;
        private List<GMSECSourceType> loadedProject_gmsecSources = null;

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
                Debug.Log("[UnityProject->LoadFromXML] " + e.ToString());
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
                Debug.Log("[UnityProject->SaveToXML] " + e.ToString());
                writer.Close();
            }

            loadingIndicatorManager.StopLoadingIndicator();
        }

        public void Unload()
        {
            loadingIndicatorManager.ShowLoadingIndicator("Unloading Project...");

            Debug.Log("Unloading project.");

            // Leave collaboration session if in one.
#if !HOLOLENS_BUILD
            if (XRC.XRCUnity.IsSessionActive)
            {
                ARVR.XRC.XRCInterface.LeaveSession();
            }
#endif

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

            XRC.XRCManager.instance.CleanUp();

            // Show the Lobby area.
            if (Infrastructure.Framework.MRET.InputRig.mode == InputRig.Mode.AR)
            {
                lobbyArea.SetActive(false);
            }
            else
            {
                lobbyArea.SetActive(true);
            }

            currentProject = null;

            loadingIndicatorManager.StopLoadingIndicator();
        }

        public ProjectType Serialize()
        {
            List<object> serializationItems = new List<object>();
            List<ItemsChoiceType> serializationItemsName = new List<ItemsChoiceType>();

            // Save Current View.
            if (loadedProject_currentView != null)
            {
                serializationItems.Add(loadedProject_currentView);
                serializationItemsName.Add(ItemsChoiceType.CurrentView);
            }

            // Save Description.
            if (loadedProject_description != null)
            {
                serializationItems.Add(loadedProject_description);
                serializationItemsName.Add(ItemsChoiceType.Description);
            }

            // Save Environment.
            if (loadedProject_environment != null)
            {
                serializationItems.Add(loadedProject_environment);
                serializationItemsName.Add(ItemsChoiceType.Environment);
            }

            // Save Parts.
            if (projectObjectContainer != null)
            {
                List<PartType> partList = new List<PartType>();
                List<int> idList = new List<int>();
                int i = 0;
                Transform topLevelContainer = projectObjectContainer.transform;
                foreach (InteractablePart part in projectObjectContainer.GetComponentsInChildren<InteractablePart>())
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
                serializationItemsName.Add(ItemsChoiceType.Parts);
            }

            // Save Drawings.
            if (projectDrawingContainer != null)
            {
                List<DrawingType> drawingList = new List<DrawingType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (Components.LineDrawing.LineDrawing lineDrawing in Infrastructure.Framework.MRET.SceneObjectManager.lineDrawings)
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
                        RenderType = lineDrawing is Components.LineDrawing.VolumetricDrawing ? "cable" : "drawing",
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
                serializationItemsName.Add(ItemsChoiceType.Drawings);
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
                serializationItemsName.Add(ItemsChoiceType.GMSECSources);
            }

            // Save Notes.
            if (projectNoteContainer != null)
            {
                List<NoteType> noteList = new List<NoteType>();
                List<int> idList = new List<int>();
                foreach (Note note in projectNoteContainer.GetComponentsInChildren<Note>())
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
                serializationItemsName.Add(ItemsChoiceType.Notes);
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
                serializationItemsName.Add(ItemsChoiceType.AnimationPanels);
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

            try
            {
                if (serializedProject.Items.Length == serializedProject.ItemsElementName.Length)
                {
                    for (int i = 0; i < serializedProject.Items.Length; i++)
                    {
                        switch (serializedProject.ItemsElementName[i])
                        {
                            case ItemsChoiceType.AnimationPanels:
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

                            case ItemsChoiceType.CurrentView:
                                loadedProject_currentView = (ViewType)serializedProject.Items[i];
                                /*Debug.Log("setting view");
                                ViewType viewToGet = (ViewType) serializedProject.Items[i];
                                Transform transformToSet = headsetFollower.transform.parent.parent;
                                transformToSet.position = new Vector3(viewToGet.Location.X, viewToGet.Location.Y, viewToGet.Location.Z);
                                transformToSet.rotation = Quaternion.Euler(viewToGet.Rotation.X, viewToGet.Rotation.Y, viewToGet.Rotation.Z);
                                transformToSet.localScale = new Vector3(1 / viewToGet.Zoom, 1 / viewToGet.Zoom, 1 / viewToGet.Zoom);*/
                                break;

                            case ItemsChoiceType.Description:
                                loadedProject_description = (string)serializedProject.Items[i];
                                break;

                            case ItemsChoiceType.Environment:
                                loadedProject_environment = (VirtualEnvironmentType)serializedProject.Items[i];

                                // Load the Scene.
                                StartCoroutine(InitializeLevelAsync(loadedProject_environment, true));

                                // Load the Skybox.
                                RenderSettings.skybox = Resources.Load("Skyboxes/" + loadedProject_environment.Skybox.MaterialName, typeof(Material)) as Material;

                                // Place the user.
                                float yOffset = 0f;
                                Transform transformToSet = MRET.Infrastructure.Framework.MRET.InputRig.transform;
                                transformToSet.position = new Vector3(loadedProject_environment.DefaultUserTransform.Position.X,
                                    loadedProject_environment.DefaultUserTransform.Position.Y + yOffset, loadedProject_environment.DefaultUserTransform.Position.Z);
                                transformToSet.rotation = Quaternion.Euler(loadedProject_environment.DefaultUserTransform.Rotation.X,
                                    loadedProject_environment.DefaultUserTransform.Rotation.Y, loadedProject_environment.DefaultUserTransform.Rotation.Z);
                                transformToSet.localScale = new Vector3(loadedProject_environment.DefaultUserTransform.Scale.X,
                                    loadedProject_environment.DefaultUserTransform.Scale.Y, loadedProject_environment.DefaultUserTransform.Scale.Z);

                                // Apply locomotion settings.
                                if (loadedProject_environment.LocomotionSettings != null)
                                {
                                    if (loadedProject_environment.LocomotionSettings.ArmswingSpeed > 0)
                                    {
                                        Infrastructure.Framework.MRET.LocomotionManager.ArmswingNormalMotionConstraintMultiplier
                                            = loadedProject_environment.LocomotionSettings.ArmswingSpeed;
                                    }
                                        
                                    if (loadedProject_environment.LocomotionSettings.FlySpeed > 0)
                                    {
                                        Infrastructure.Framework.MRET.LocomotionManager.FlyingNormalMotionConstraintMultiplier
                                            = loadedProject_environment.LocomotionSettings.FlySpeed;
                                    }
                                        
                                    if (loadedProject_environment.LocomotionSettings.TeleportDistance > 0)
                                    {
                                        Infrastructure.Framework.MRET.LocomotionManager.TeleportMaxDistance
                                            = loadedProject_environment.LocomotionSettings.TeleportDistance;
                                    }
                                        
                                    if (loadedProject_environment.LocomotionSettings.TouchpadSpeed > 0)
                                    {
                                        Infrastructure.Framework.MRET.LocomotionManager.NavigationNormalMotionConstraintMultiplier
                                            = loadedProject_environment.LocomotionSettings.TouchpadSpeed;
                                    }

                                    if (loadedProject_environment.LocomotionSettings.ClimbSpeed > 0)
                                    {
                                        Infrastructure.Framework.MRET.LocomotionManager.ClimbingNormalMotionConstraintMultiplier
                                            = loadedProject_environment.LocomotionSettings.ClimbSpeed;
                                    }
                                }
                                    
                                Debug.Log("Setting up controlled user...");
                                if (collaborationEnabled)
                                {
                                    synchedUser = transformToSet.GetComponentInChildren<Camera>().gameObject.AddComponent<SynchronizedUser>();
                                    synchedUser.userObject = Infrastructure.Framework.MRET.InputRig.gameObject;
                                    synchedUser.userAlias = userAlias;
                                    synchedUser.isControlled = true;
                                    synchedUser.uuid = userUUID;
                                    synchedUser.userLabelColor = userLabelColor;

                                    InputHand leftHand = Infrastructure.Framework.MRET.InputRig.leftHand;
                                    InputHand rightHand = Infrastructure.Framework.MRET.InputRig.rightHand;
                                    if (leftHand == null && rightHand == null)
                                    {
                                        List<InputHand> hands = Infrastructure.Framework.MRET.InputRig.hands;
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

                                    synchedUser.leftController = leftHand.gameObject.AddComponent<SynchronizedController>();
                                    synchedUser.leftController.controllerSide = SynchronizedController.ControllerSide.Left;
                                    synchedUser.leftController.synchronizedUser = synchedUser;
                                    synchedUser.leftController.uuid = lcUUID;

                                    GameObject leftLaser = new GameObject("Laser");
                                    leftLaser.transform.parent = leftHand.transform;
                                        
                                    synchedUser.leftController.pointer = leftLaser.AddComponent<SynchronizedPointer>();
                                    synchedUser.leftController.pointer.synchronizedController = synchedUser.leftController;
                                    synchedUser.leftController.pointer.uuid = lpUUID;
                                    synchedUser.leftController.pointer.hand = leftHand;

                                    synchedUser.rightController = rightHand.gameObject.AddComponent<SynchronizedController>();
                                    synchedUser.rightController.controllerSide = SynchronizedController.ControllerSide.Right;
                                    synchedUser.rightController.synchronizedUser = synchedUser;
                                    synchedUser.rightController.uuid = rcUUID;

                                    GameObject rightLaser = new GameObject("Laser");
                                    rightLaser.transform.parent = rightHand.transform;
                                    synchedUser.rightController.pointer = rightLaser.AddComponent<SynchronizedPointer>();
                                    synchedUser.rightController.pointer.synchronizedController = synchedUser.rightController;
                                    synchedUser.rightController.pointer.uuid = rpUUID;
                                    synchedUser.rightController.pointer.hand = rightHand;

                                    Debug.Log("Synchronized user added.");

                                    CollaborationManager collabMgr = FindObjectOfType<CollaborationManager>();
                                    if (collabMgr.engineType == CollaborationManager.EngineType.XRC)
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
                                Camera headsetCam = Infrastructure.Framework.MRET.InputRig.activeCamera;
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

                            case ItemsChoiceType.Parts:
                                PartsType parts = (PartsType) serializedProject.Items[i];

                                // Load each part into the environment.
                                if (parts.Parts != null)
                                {
                                    foreach (PartType part in parts.Parts)
                                    {
                                        Infrastructure.Framework.MRET.PartManager.InstantiatePartInEnvironment(part, null);
                                    }
                                }
                                break;

                            case ItemsChoiceType.Drawings:
                                DrawingsType drawings = (DrawingsType)serializedProject.Items[i];

                                // Load each drawing into the environment.
                                if (drawings != null && drawings.Drawings != null)
                                {
                                    foreach (DrawingType drawing in drawings.Drawings)
                                    {
                                        Infrastructure.Framework.MRET.SceneObjectManager.CreateLineDrawing(drawing.Name, null,
                                            Vector3.zero, Quaternion.identity, Vector3.one,
                                            drawing.RenderType.ToLower() == "drawing" || drawing.RenderType == "measurement"
                                            ? Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Basic
                                            : Infrastructure.Framework.LineDrawing.LineDrawingManager.DrawingType.Volumetric,
                                            drawing.Width, Color.green, DeserializeVector3ArrayToList(drawing.Points).ToArray(),
                                            Guid.Parse(drawing.GUID));
                                        //drawLineManager.AddPredefinedDrawing(DeserializeVector3ArrayToList(drawing.Points),
                                        //    (LineDrawing.RenderTypes)Enum.Parse(typeof(LineDrawing.RenderTypes), drawing.RenderType.ToString()),
                                        //    (LineDrawing.unit)Enum.Parse(typeof(LineDrawing.unit), drawing.DesiredUnits.ToString()),
                                        //    drawing.Name, new Guid(drawing.GUID));
                                    }
                                }
                                break;

                            case ItemsChoiceType.GMSECSources:
                                GMSECSourcesType gmsecSources = (GMSECSourcesType)serializedProject.Items[i];
#if !HOLOLENS_BUILD
                                if (gmsecSources.GMSECSources != null)
                                {
                                    foreach (GMSECSourceType gmsecSource in gmsecSources.GMSECSources)
                                    {
                                        loadedProject_gmsecSources.Add(gmsecSource);
                                        GameObject gmsecSourceObject = Instantiate(gmsecSourcePrefab);
                                        gmsecSourceObject.transform.SetParent(dataSourcesContainer.transform);
                                        GMSECBusToDataManager gmsecListener = gmsecSourceObject.GetComponent<GMSECBusToDataManager>();

                                        gmsecListener.dataManager = Infrastructure.Framework.MRET.DataManager;
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
                                        gmsecListener.server = gmsecSource.Server;
                                        gmsecListener.subject = gmsecSource.Subject;
                                        if (gmsecSource.ReadFrequency > 0 && gmsecSource.ReadFrequency <= 2048)
                                        {
                                            gmsecListener.messageReadFrequency = gmsecSource.ReadFrequency;
                                        }
                                        else
                                        {
                                            gmsecListener.messageReadFrequency = 1;
                                        }

                                    }
                                }
#endif
                                break;

                            case ItemsChoiceType.MatlabConnection:
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
                                        matlabClient.host = matlabConnection.host;
                                        matlabClient.port = matlabConnection.port;
                                        matlabClient.Initialize();

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

                            case ItemsChoiceType.Notes:
                                NotesType notes = (NotesType)serializedProject.Items[i];

                                // Load each note into the environment.
                                if (notes.Notes != null)
                                {
                                    foreach (NoteType note in notes.Notes)
                                    {
                                        Vector3Type notePosition = note.Transform.Position;
                                        QuaternionType noteRotation = note.Transform.Rotation;
                                        Note.NoteData noteData = new Note.NoteData();
                                        noteData.pos = new Vector3(notePosition.X, notePosition.Y, notePosition.Z);
                                        noteData.rot = new Quaternion(noteRotation.X, noteRotation.Y, noteRotation.Z, noteRotation.W);
                                        noteData.title = note.Title;
                                        noteData.information = note.Details;
                                        Note.fromSerializable(noteData, note.Drawings.NoteDrawings, noteManager.noteCount++, new Guid(note.GUID));
                                    }
                                }
                                break;

                            case ItemsChoiceType.PointClouds:
                                StaticPointCloudsType pointClouds = (StaticPointCloudsType)serializedProject.Items[i];

                                // Load each point cloud into the environment.
                                if (pointClouds.StaticPointClouds != null)
                                {
                                    foreach (StaticPointCloudType pointCloud in pointClouds.StaticPointClouds)
                                    {
                                        Vector3Type pcPosition = pointCloud.Position;
                                        QuaternionType pcRotation = pointCloud.Rotation;
                                        Vector3Type pcScale = pointCloud.Scale;
                                        string pcName = pointCloud.Name;
                                        string pcPath = pointCloud.Path;
                                        int pcIndex = pointCloud.LODIndex;
                                        NewPointCloudLoader.InstantiatePointCloud_s(
                                            new Vector3(pcPosition.X, pcPosition.Y, pcPosition.Z),
                                            new Quaternion(pcRotation.X, pcRotation.Y, pcRotation.Z, pcRotation.W),
                                            new Vector3(pcScale.X, pcScale.Y, pcScale.Z),
                                            pointCloud.Path, pointCloud.Name, null, pointCloudContainer);
                                    }
                                }
                                break;

                            case ItemsChoiceType.ObjectGenerators:
                                ObjectGeneratorsType ogts = (ObjectGeneratorsType)serializedProject.Items[i];

                                // Load each object generator into the environment.
                                if (ogts != null)
                                {
                                    foreach (ObjectGeneratorType ogt in ogts.ObjectGenerators)
                                    {
                                        StartCoroutine(ProceduralObjectGeneratorManager.instance.AddGenerator(ogt));
                                    }
                                }
                                break;

                            case ItemsChoiceType.VDEConnection:
                                VDEConnectionType vct = (VDEConnectionType)serializedProject.Items[i];
                                if (vct != null)
                                {
                                    if (string.IsNullOrEmpty(vct.serverAddress))
                                    {
                                        vct.serverAddress = "http://127.0.0.1/VDE";
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

                                    vdeSettings = new VDESettings(vct.standalone, vct.renderInCloud,
                                        vct.serverAddress, vct.bakedConfigResource,
                                        vct.bakedEntitiesResource, vct.bakedLinksResource, vct.connectionType);
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
                    Debug.Log("[UnityProject->Deserialize] Invalid Serialized Project.");
                }
            }
            catch (Exception e)
            {
                Debug.Log("[UnityProject->Deserialize] " + e.ToString());
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
                Debug.LogError("[UnityProject->ReloadProject] No project to reload.");
            }
        }

        public void ResetUser()
        {
            if (loadedProject_environment != null)
            {
                // Place the user.
                float yOffset = 0f;
                Transform transformToSet = Infrastructure.Framework.MRET.InputRig.transform;
                transformToSet.position = new Vector3(loadedProject_environment.DefaultUserTransform.Position.X,
                    loadedProject_environment.DefaultUserTransform.Position.Y + yOffset, loadedProject_environment.DefaultUserTransform.Position.Z);
                transformToSet.rotation = Quaternion.Euler(loadedProject_environment.DefaultUserTransform.Rotation.X,
                    loadedProject_environment.DefaultUserTransform.Rotation.Y, loadedProject_environment.DefaultUserTransform.Rotation.Z);
                transformToSet.localScale = new Vector3(loadedProject_environment.DefaultUserTransform.Scale.X,
                    loadedProject_environment.DefaultUserTransform.Scale.Y, loadedProject_environment.DefaultUserTransform.Scale.Z);
            }
            else
            {
                Debug.LogError("[UnityProject->ResetUser] No environment settings.");
            }
        }

        // TODO: The parts should be indexed into a dictionary to enable efficient searching. Move to part manager.
        public InteractablePart GetPartByUUID(Guid guidToGet)
        {
            foreach (InteractablePart iPart in projectObjectContainer.GetComponentsInChildren<InteractablePart>())
            {
                if (iPart.guid == guidToGet)
                {
                    return iPart;
                }
            }

            // Less efficient, if first check fails to find part.
            foreach (InteractablePart iPart in FindObjectsOfType<InteractablePart>())
            {
                if (iPart.guid == guidToGet)
                {
                    return iPart;
                }
            }

            return null;
        }

        public SynchronizedUser GetUserByUUID(Guid guidToGet)
        {
            CollaborationManager collabMgr = FindObjectOfType<CollaborationManager>();
            if (collabMgr.engineType == CollaborationManager.EngineType.XRC)
            {
                return collabMgr.xrcManager.synchedUsers.Find(x => x.uuid == guidToGet);
            }
            else
            {
                return collabMgr.masterNode.synchronizedUsers.Find(x => x.uuid == guidToGet);
            }
        }

        public SynchronizedController GetControllerByUUID(Guid guidToGet)
        {
            CollaborationManager collabMgr = FindObjectOfType<CollaborationManager>();
            if (collabMgr.engineType == CollaborationManager.EngineType.XRC)
            {
                foreach (SynchronizedUser user in collabMgr.xrcManager.synchedUsers)
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
                foreach (SynchronizedUser user in collabMgr.masterNode.synchronizedUsers)
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
            AssetBundleHelper.instance.LoadSceneAsync(Application.dataPath
                + "/StreamingAssets/Windows/" + environment.AssetBundle, environment.Name, isAdditive, action);

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
            yield return true;
        }

        private void UngrabAllObjects()
        {
            foreach (InputHand hand in Infrastructure.Framework.MRET.InputRig.hands)
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