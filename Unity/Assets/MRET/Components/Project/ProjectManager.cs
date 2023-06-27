// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Helpers;
using GOV.NASA.GSFC.XR.MRET.Anchors;
using GOV.NASA.GSFC.XR.MRET.Animation;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Marker;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.PointCloud;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Tools.ProceduralObjectGeneration;
using GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms;
using GOV.NASA.GSFC.XR.MRET.Extensions.GLTF;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// ProjectManager
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
    public class ProjectManager : MRETSerializableManager<ProjectManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ProjectManager);

        public GameObject lobbyArea;
        public GameObject projectContainer,
            dataSourcesContainer, animationPanelsContainer,
            pointCloudContainer, proceduralObjectGeneratorContainer;
        public GameObject objectConfigurationPanelPrefab;

        /// <summary>
        /// Instance of the Anchor Manager.
        /// </summary>
        [Tooltip("Instance of the Anchor Manager.")]
        public AnchorManager anchorManager;

        /// <summary>
        /// Instance of the Animation Manager.
        /// </summary>
        [Tooltip("Instance of the Animation Manager.")]
        public MRETAnimationManager animationManager;

        /// <summary>
        /// Instance of the Line Drawing Manager.
        /// </summary>
        [Tooltip("Instance of the Drawing Manager.")]
        public LineDrawingManager drawingManager;

        /// <summary>
        /// Instance of the Marker Manager.
        /// </summary>
        [Tooltip("Instance of the Marker Manager.")]
        public MarkerManager markerManager;

        /// <summary>
        /// Instance of the NoteDeprecated Manager.
        /// </summary>
        [Tooltip("Instance of the Note Manager.")]
        public NoteManager noteManager;

        /// <summary>
        /// Instance of the Part Manager.
        /// </summary>
        [Tooltip("Instance of the Part Manager.")]
        public PartManager partManager;

        /// <summary>
        /// Instance of the deprecated Part Manager.
        /// </summary>
        [Tooltip("Instance of the deprecated Part Manager.")]
        public PartManagerDeprecated partManagerDeprecated;

        /// <summary>
        /// Instance of the deprecated Pin Manager.
        /// </summary>
        [Tooltip("Instance of the deprecated Pin Manager.")]
        public PinManagerDeprecated pinManagerDeprecated;

        /// <summary>
        /// Instance of the Point Cloud Manager.
        /// </summary>
        [Tooltip("Instance of the Point Cloud Manager.")]
        public PointCloudManager pointCloudManager;

        /// <summary>
        /// Instance of the Procedural Object Generator Manager.
        /// </summary>
        [Tooltip("Instance of the Procedural Object Generator Manager.")]
        public ProceduralObjectGeneratorManager proceduralObjectGeneratorManager;

        /// <summary>
        /// Instance of the Scene Object Manager.
        /// </summary>
        [Tooltip("Instance of the Scene Object Manager.")]
        public SceneObjectManager sceneObjectManager;

        /// <summary>
        /// Instance of the deprecated Scene Object Manager.
        /// </summary>
        [Tooltip("Instance of the Scene Object Manager.")]
        public SceneObjectManagerDeprecated sceneObjectManagerDeprecated;

        /// <summary>
        /// Instance of the Third Party Interface Manager.
        /// </summary>
        [Tooltip("Instance of the Third Party Interface Manager.")]
        public ThirdPartyInterfaceManager interfaceManager;

        /// <summary>
        /// Instance of the Undo Manager.
        /// </summary>
        [Tooltip("Instance of the Undo Manager.")]
        public UndoManager undoManager;

        /// <summary>
        /// Instance of the deprecated Undo Manager.
        /// </summary>
        [Tooltip("Instance of the deprecated Undo Manager.")]
        public UndoManagerDeprecated undoManagerDeprecated;

        /// <summary>
        /// Instance of the Asset Bundle Helper.
        /// </summary>
        [Tooltip("Instance of the Asset Bundle Helper.")]
        public AssetBundleHelper assetBundleHelper;

        /// <summary>
        /// Instance of the GLTF Helper.
        /// </summary>
        [Tooltip("Instance of the GLTF Helper.")]
        public GltfExt gltfHelper;

        /// <summary>
        /// Instance of the Anchor Manager.
        /// </summary>
        public static AnchorManager AnchorManager => Instance.anchorManager;

        /// <summary>
        /// Instance of the Animation Manager.
        /// </summary>
        public static MRETAnimationManager AnimationManager => Instance.animationManager;

        /// <summary>
        /// Instance of the Line Drawing Manager.
        /// </summary>
        public static LineDrawingManager DrawingManager => Instance.drawingManager;

        /// <summary>
        /// Instance of the Marker Manager.
        /// </summary>
        public static MarkerManager MarkerManager => Instance.markerManager;

        /// <summary>
        /// Instance of the Note Manager.
        /// </summary>
        public static NoteManager NoteManager => Instance.noteManager;

        /// <summary>
        /// Instance of the Part Manager.
        /// </summary>
        public static PartManager PartManager => Instance.partManager;

        /// <summary>
        /// Instance of the deprecated Part Manager.
        /// </summary>
        public static PartManagerDeprecated PartManagerDeprecated => Instance.partManagerDeprecated;

        /// <summary>
        /// Instance of the deprecated Part Manager.
        /// </summary>
        public static PinManagerDeprecated PinManagerDeprecated => Instance.pinManagerDeprecated;

        /// <summary>
        /// Instance of the Point Cloud Manager.
        /// </summary>
        public static PointCloudManager PointCloudManager => Instance.pointCloudManager;

        /// <summary>
        /// Instance of the Procedural Object Generator Manager.
        /// </summary>
        public static ProceduralObjectGeneratorManager ProceduralObjectGeneratorManager => Instance.proceduralObjectGeneratorManager;

        /// <summary>
        /// Instance of the scene object manager.
        /// </summary>
        public static SceneObjectManager SceneObjectManager => Instance.sceneObjectManager;

        /// <summary>
        /// Instance of the deprecated scene object manager.
        /// </summary>
        public static SceneObjectManagerDeprecated SceneObjectManagerDeprecated => Instance.sceneObjectManagerDeprecated;

        /// <summary>
        /// Instance of the third party interface manager.
        /// </summary>
        public static ThirdPartyInterfaceManager InterfaceManager => Instance.interfaceManager;

        /// <summary>
        /// Instance of the Undo Object Manager.
        /// </summary>
        public static UndoManager UndoManager => Instance.undoManager;

        /// <summary>
        /// Instance of the deprecated Undo Object Manager.
        /// </summary>
        public static UndoManagerDeprecated UndoManagerDeprecated => Instance.undoManagerDeprecated;

        /// <summary>
        /// Instance of the Asset Bundle Helper.
        /// </summary>
        public static AssetBundleHelper AssetBundleHelper => Instance.assetBundleHelper;

        /// <summary>
        /// Instance of the Asset Bundle Helper.
        /// </summary>
        public static GltfExt GLTFHelper => Instance.gltfHelper;

        /// <summary>
        /// Lobby area
        /// </summary>
        public static GameObject Lobby => Instance.lobbyArea;

        /// <summary>
        /// Container for the project
        /// </summary>
        public static GameObject ProjectContainer => Instance.projectContainer;

        /// <summary>
        /// Container for project scene objects
        /// </summary>
        public static GameObject SceneObjectContainer => Project?.Content?.SceneObjects;

        /// <summary>
        /// Container for project parts
        /// </summary>
        public static GameObject PartsContainer => Project?.Content?.Parts;

        /// <summary>
        /// Container for project markers
        /// </summary>
        public static GameObject MarkersContainer => Project?.Content?.Markers;

        /// <summary>
        /// Container for project notes
        /// </summary>
        public static GameObject NotesContainer => Project?.Content?.Notes;

        /// <summary>
        /// Container for project drawings
        /// </summary>
        public static GameObject DrawingsContainer => Project?.Content?.Drawings;

        /// <summary>
        /// Container for project point clouds
        /// </summary>
        public static GameObject PointCloudContainer => Project?.Content?.PointClouds;

        /// <summary>
        /// Container for project IoTThings
        /// </summary>
        public static GameObject IoTThingsContainer => Project?.Content?.IoTThings;

        /// <summary>
        /// Container for 3rd party interfaces
        /// </summary>
        public static GameObject InterfacesContainer => Project?.Interfaces;

        public static IUnityProject Project { get; private set; }

        public static bool Loaded => (Project != default);

        /// <summary>
        /// List of materials to select from to apply to objects by default.
        /// </summary>
        public static List<Material> DefaultPartMaterials => Instance.defaultPartMaterials;

        /// <summary>
        /// List of materials to select from to apply to objects by default.
        /// </summary>
        [Tooltip("List of materials to select from to apply to objects by default.")]
        public List<Material> defaultPartMaterials;

        /// <summary>
        /// Whether or not the object panel is allowed to be shown.
        /// </summary>
        public static bool ObjectConfigurationPanelEnabled
        {
            get => Instance.objectConfigurationPanelEnabled;
            set => Instance.objectConfigurationPanelEnabled = value;
        }

        /// <summary>
        /// Whether or not the object configuration panel is allowed to be shown.
        /// </summary>
        [Tooltip("Whether or not the object configuration panel is allowed to be shown.")]
        public bool objectConfigurationPanelEnabled = true;

        /// <summary>
        /// Object configuration panel prefab
        /// </summary>
        public static GameObject ObjectConfigurationPanelPrefab => Instance.objectConfigurationPanelPrefab;

        /// <seealso cref="MRETUpdateSingleton{T}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            // Helpers
            Log("Initializing Asset Bundle Helper...", nameof(Initialize));
            if (assetBundleHelper == null)
            {
                assetBundleHelper = FindObjectOfType<AssetBundleHelper>();
                if (assetBundleHelper == null)
                {
                    LogError("Fatal Error. Unable to initialize asset bundle helper. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            assetBundleHelper.Initialize();
            Log("Asset Bundle Helper initialized.", nameof(Initialize));

            Log("Initializing GLTF Helper...", nameof(Initialize));
            if (gltfHelper == null)
            {
                gltfHelper = FindObjectOfType<GltfExt>();
                if (gltfHelper == null)
                {
                    LogError("Fatal Error. Unable to initialize GLTF helper. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            gltfHelper.Initialize();
            Log("GLTF Helper initialized.", nameof(Initialize));

            // Initialize the project managers
#if HOLOLENS_BUILD
            Log("Initializing Anchor Manager...", nameof(Initialize));
            if (anchorManager == null)
            {
                anchorManager = FindObjectOfType<AnchorManager>();
                if (anchorManager == null)
                {
                    LogError("Fatal Error. Unable to initialize anchor manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            anchorManager.Initialize();
            Log("Anchor Manager initialized.", nameof(Initialize));
#endif

            // Initialize the project managers
            Log("Initializing Animation Manager...", nameof(Initialize));
            if (animationManager == null)
            {
                animationManager = FindObjectOfType<MRETAnimationManager>();
                if (animationManager == null)
                {
                    LogError("Fatal Error. Unable to initialize animation manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            animationManager.Initialize();
            Log("Animation Manager initialized.", nameof(Initialize));

            Log("Initializing Drawing Manager...", nameof(Initialize));
            if (drawingManager == null)
            {
                drawingManager = FindObjectOfType<LineDrawingManager>();
                if (drawingManager == null)
                {
                    LogError("Fatal Error. Unable to initialize drawing manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            drawingManager.Initialize();
            Log("Drawing Manager initialized.", nameof(Initialize));

            Log("Initializing Marker Manager...", nameof(Initialize));
            if (markerManager == null)
            {
                markerManager = FindObjectOfType<MarkerManager>();
                if (markerManager == null)
                {
                    LogError("Fatal Error. Unable to initialize marker manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            markerManager.Initialize();
            Log("Marker Manager initialized.", nameof(Initialize));

            Log("Initializing Note Manager...", nameof(Initialize));
            if (noteManager == null)
            {
                noteManager = FindObjectOfType<NoteManager>();
                if (noteManager == null)
                {
                    LogError("Fatal Error. Unable to initialize note manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            noteManager.Initialize();
            Log("Note Manager initialized.", nameof(Initialize));

            Log("Initializing Part Manager...", nameof(Initialize));
            if (partManager == null)
            {
                partManager = FindObjectOfType<PartManager>();
                if (partManager == null)
                {
                    LogError("Fatal Error. Unable to initialize part manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            partManager.Initialize();
            Log("Part Manager initialized.", nameof(Initialize));

            Log("Loading Point Cloud Manager...", nameof(Initialize));
            if (pointCloudManager == null)
            {
                pointCloudManager = FindObjectOfType<PointCloudManager>();
                if (pointCloudManager == null)
                {
                    LogError("Fatal Error. Unable to load point cloud manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
                pointCloudManager.Initialize();
            }
            Log("Point Cloud Manager initialized.", nameof(Initialize));

            Log("Initializing Procedural Object Generator...", nameof(Initialize));
            if (proceduralObjectGeneratorManager == null)
            {
                proceduralObjectGeneratorManager = FindObjectOfType<ProceduralObjectGeneratorManager>();
                if (proceduralObjectGeneratorManager == null)
                {
                    LogError("Fatal Error. Unable to initialize the procedural object generator. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            proceduralObjectGeneratorManager.Initialize();
            Log("Procedural Object Generator initialized.", nameof(Initialize));

            Log("Setting up scene object manager...", nameof(Initialize));
            if (sceneObjectManager == null)
            {
                sceneObjectManager = FindObjectOfType<SceneObjectManager>();
                if (sceneObjectManager == null)
                {
                    LogError("Fatal Error. Unable to set up the scene object manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            sceneObjectManager.Initialize();
            Log("Scene Object Manager initialized.", nameof(Initialize));

            Log("Setting up third party interface manager...", nameof(Initialize));
            if (interfaceManager == null)
            {
                interfaceManager = FindObjectOfType<ThirdPartyInterfaceManager>();
                if (interfaceManager == null)
                {
                    LogError("Fatal Error. Unable to set up the third party interface manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            interfaceManager.Initialize();
            Log("Third Party Interface Manager initialized.", nameof(Initialize));

            Log("Initializing Undo Manager...", nameof(Initialize));
            if (undoManager == null)
            {
                undoManager = FindObjectOfType<UndoManager>();
                if (undoManager == null)
                {
                    LogError("Fatal Error. Unable to initialize undo manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            undoManager.Initialize();
            Log("Undo Manager initialized.", nameof(Initialize));

            // Required game objects
            if (lobbyArea == null)
            {
                LogError("Fatal Error. Lobby area is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }

            if (objectConfigurationPanelPrefab == null)
            {
                LogError("Fatal Error. Object panel configuration prefab is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
        }

#region MRETBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            if (state == IntegrityState.Success)
            {
                if (objectConfigurationPanelPrefab == null)
                {
                    LogError("Object Configuration Panel Prefab not assigned.", nameof(IntegrityCheck));
                    state = IntegrityState.Failure;
                }
                else if (projectContainer == null)
                {
                    LogError("Project Container not assigned.", nameof(IntegrityCheck));
                    state = IntegrityState.Failure;
                }
            }

            return state;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Make sure we have required containers
            if (projectContainer == null)
            {
                projectContainer = new GameObject("LoadedProject");
                projectContainer.transform.parent = transform;
            }
            if (dataSourcesContainer == null)
            {
                dataSourcesContainer = new GameObject("DataSources");
                dataSourcesContainer.transform.parent = transform;
            }
            if (animationPanelsContainer == null)
            {
                animationPanelsContainer = new GameObject("Animations");
                animationPanelsContainer.transform.parent = transform;
            }
            if (pointCloudContainer == null)
            {
                pointCloudContainer = new GameObject("PointClouds");
                pointCloudContainer.transform.parent = transform;
            }
            if (proceduralObjectGeneratorContainer == null)
            {
                proceduralObjectGeneratorContainer = new GameObject("GeneratedObjects");
                proceduralObjectGeneratorContainer.transform.parent = transform;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            // Perform am organized cleanup
            Cleanup();
        }
#endregion MRETBehaviour

        public void LoadFromXML(string filePath)
        {
            try
            {
                // Deserialize the file into our deserialized type class
                ProjectType serializedProject = ProjectFileSchema.FromXML(filePath) as ProjectType;

                // Instantiate the project
                InstantiateProject(serializedProject);
            }
            catch (Exception e)
            {
                LogWarning("A problem was encountered loading the XML file: " + e.ToString(), nameof(LoadFromXML));
            }
        }

        public bool SaveToXML(string filePath)
        {
            bool result = false;

            if (Loaded)
            {
                MRET.LoadingIndicatorManager.ShowLoadingIndicator("Saving Project...");

                try
                {
                    // Setup our serialized action which will write the result to a file if successful
                    var serializedProject = Project.CreateSerializedType();
                    Action<bool, string> SerializedAction = (bool serialized, string message) =>
                    {
                        if (serialized)
                        {
                            // Write out the project XML file
                            ProjectFileSchema.ToXML(filePath, serializedProject);
                            result = true;
                        }
                        else
                        {
                            string logMessage = "A problem occurred serializing the project";
                            if (!string.IsNullOrEmpty(message))
                            {
                                logMessage += ": " + message;
                            }
                            LogWarning(logMessage, nameof(SaveToXML));
                        }
                    };

                    // Serialize the project
                    Project.Serialize(serializedProject, SerializedAction);
                }
                catch (Exception e)
                {
                    LogWarning("A problem was encountered saving the XML file: " + e.ToString(), nameof(SaveToXML));
                }

                MRET.LoadingIndicatorManager.StopLoadingIndicator();
            }
            else
            {
                LogWarning("Project reference is null", nameof(SaveToXML));
            }

            return result;
        }

#region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            Transform result;
            if (serialized is ProjectType)
            {
                // IoTClient
                result = ProjectContainer.transform;
            }
            else
            {
                // IoTThingType
                result = SceneObjectContainer.transform;
            }

            return result;
        }

        /// <summary>
        /// Instantiates the project from the supplied serialized project.
        /// </summary>
        /// <param name="serializedProject">The <code>ProjectType</code> class instance
        ///     containing the serialized representation of the project to instantiate</param>
        public void InstantiateProject(ProjectType serializedProject)
        {
            // Halt autosave while instantiating the project
            MRET.AutosaveManager.StopAutosave();

            // Destroy the old project reference
            Destroy((UnityEngine.Object)Project);

            // Hide the Lobby area.
            lobbyArea.SetActive(false);

            // Indicate that the project is loading
            MRET.LoadingIndicatorManager.ShowLoadingIndicator("Loading Project...");

            // Instantiate and deserialize
            InstantiateSerializable<ProjectType, UnityProject>(serializedProject, projectContainer,
                projectContainer.transform, null, FinishProjectInstantiation);
        }

        /// <seealso cref="MRETSerializableManager{M}.FinishSerializableInstantiation{T, I}"/>
        private void FinishProjectInstantiation(ProjectType serializedProject, UnityProject instantiatedProject,
            params object[] context)
        {
            // Grab the description if available
            string description = string.IsNullOrEmpty(serializedProject?.Description)
                ? "" :
                "\"" + serializedProject.Description + "\" ";

            // Check for success or failure
            if (instantiatedProject != default)
            {
                Log("Project " + description + "instantiation complete", nameof(FinishProjectInstantiation));

                // Assign the Project
                Project = instantiatedProject;

                // Establish the interface connections
                InterfaceManager.Connect();

                // Reinitialize TouchControl dropdowns.
                foreach (DualAxisRotationControl control in FindObjectsOfType<DualAxisRotationControl>())
                {
                    if (control.enabled)
                    {
                        control.enabled = false;
                        control.enabled = true;
                    }
                }

                // Resume autosave on successsful loading of project
                MRET.AutosaveManager.StartAutosave();
            }
            else
            {
                // Log the error
                LogError("Project " + description + "instantiation failed", nameof(FinishProjectInstantiation));
            }

            // Indicate that project is done loading.
            MRET.LoadingIndicatorManager.StopLoadingIndicator();
        }

        /// <summary>
        /// Instantiates a MRET object associated with the supplied serialized instance and adds it to
        /// the supplied container.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>IdentifiableType</code> specifies
        ///     the serialized type being used to instantiate the MRET object.</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IIdentifiable</code> specifies the
        ///     MRET object type being instantiated.</typeparam>
        /// <param name="serialized">The serialized instance to be used to configure the instantiated MRET
        ///     object.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     MRET object. If not provided, one will be created.</param>
        /// <param name="container">The optional container (parent) <code>Transform</code> for the instantiated
        ///     MRET object. If null, the generic scene object container method will be used (not recommended
        ///     since this could affect project serialization. Caller should explicitly specify the container
        ///     for this MRET object).</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishObjectInstantiation">The optional <code>FinishSerializableInstantiationDelegate</code>
        ///     method to be called to finish the serializable instantiation. Called before the onLoaded action
        ///     is called.</param>
        /// <param name="context">Optional context parameters to be supplied to the finishObjectInstantiation
        ///     method to provide additional context</param>
        /// <seealso cref="MRETSerializableManager{M}.FinishSerializableInstantiation{T, I}"/>
        public void InstantiateObject<T, I>(T serialized, GameObject go = null,
            Transform container = null, Action<I> onLoaded = null,
            FinishSerializableInstantiationDelegate<T, I> finishObjectInstantiation = null,
            params object[] context)
            where T : VersionedType
            where I : IVersioned
        {
            // Instantiate the object
            InstantiateSerializable(serialized, go, container, onLoaded, finishObjectInstantiation, context);
        }
#endregion Serializable Instantiation

        /// <summary>
        /// Ungrabs any objects grabbed by the hands
        /// </summary>
        private void UngrabAllObjects()
        {
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                hand.GrabComplete();
            }
        }

        /// <summary>
        /// Cleans up the project by closing connections and releasing references
        /// </summary>
        private void Cleanup()
        {
            // Notify managers to clear caches
            InterfaceManager.CloseConnections();

            // Detatch all objects from the controllers.
            UngrabAllObjects();

            /* TODO:
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
            */

            // Destroy the project
            Destroy((UnityEngine.Object)Project);
        }

        /// <summary>
        /// Unloads the current project and returns to the lobby
        /// </summary>
        public void Unload()
        {
            MRET.LoadingIndicatorManager.ShowLoadingIndicator("Unloading Project...");

            Log("Unloading project.", nameof(Unload));

            // Stop the autosaving
            MRET.AutosaveManager.StopAutosave();

            // Stop the collaboration engine
            if (MRET.CollaborationManager.IsEngineStarted)
            {
                MRET.CollaborationManager.StopEngine();
            }

            // Notify the HUD manager to cleanup screen panels
            MRET.HudManager.CleanupScreenPanels();

            // Perform an organized cleanup
            Cleanup();

            // Remove all autosaves
            MRET.AutosaveManager.DeleteAutosaves();

            // Show the Lobby area.
            if (MRET.InputRig.mode == InputRig.Mode.AR)
            {
                lobbyArea.SetActive(false);
            }
            else
            {
                lobbyArea.SetActive(true);
            }

            MRET.LoadingIndicatorManager.StopLoadingIndicator();
        }

        public Vector3 GlobalToLocalPosition(Vector3 globalPosition)
        {
            // TODO: Is projectContainer the reference point we want to use?
            return projectContainer.transform.InverseTransformPoint(globalPosition);
        }

        public Vector3 LocalToGlobalPosition(Vector3 localPosition)
        {
            // TODO: Is projectContainer the reference point we want to use?
            return projectContainer.transform.TransformPoint(localPosition);
        }
    }

}
