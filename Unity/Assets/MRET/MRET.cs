// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Threading;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.MRET.AutoSave;
using GOV.NASA.GSFC.XR.MRET.Avatar;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.HUD;
using GOV.NASA.GSFC.XR.MRET.IK;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Tools.Selection;
using GOV.NASA.GSFC.XR.MRET.Time;
using GOV.NASA.GSFC.XR.MRET.Locomotion;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRUI;
using GOV.NASA.GSFC.XR.MRET.Registry;
using GOV.NASA.GSFC.XR.MRET.UI.LoadingIndicator;
#if MRET_EXTENSION_EASYBUILDSYSTEM
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
#endif

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 1 February 2021: Created
    /// </remarks>
    /// <summary>
    /// MRET is the top-level script for all of MRET. It
    /// controls the sequence by which components are initialized
    /// and maintains references to key MRET scripts.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class MRET : MRETManager<MRET>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRET);

        /// <summary>
        /// File name for the kiosk mode file.
        /// </summary>
        [Tooltip("File name for the kiosk mode file.")]
        public static readonly string kioskModeFile = "kiosk.xml";

        /// <summary>
        /// Layer which raycasting is performed on.
        /// </summary>
        [Tooltip("Layer which raycasting is performed on.")]
        public static readonly int raycastLayer = 15;

        /// <summary>
        /// Layer which object previewing is performed on.
        /// </summary>
        [Tooltip("Layer which object previewing is performed on.")]
        public static readonly int previewLayer = 20;

        /// <summary>
        /// Default layer for things in MRET.
        /// </summary>
        [Tooltip("Default layer for things in MRET.")]
        public static readonly int defaultLayer = 0;

        public static bool IsMainThread { get => (mainThread != null) && mainThread.Equals(Thread.CurrentThread); }
        private static Thread mainThread;

        /// <summary>
        /// Instance of the UUID Registry.
        /// </summary>
        public static MRETUuidRegistryManager UuidRegistry => Instance.uuidRegistry;

        /// <summary>
        /// Instance of the Schema Handler.
        /// </summary>
        public static SchemaHandler SchemaHandler => Instance.schemaHandler;

        /// <summary>
        /// Instance of the Input Rig.
        /// </summary>
        public static InputRig InputRig => Instance.inputRig;

        /// <summary>
        /// Instance of the Loading Indicator Manager.
        /// </summary>
        public static LoadingIndicatorManager LoadingIndicatorManager => Instance.loadingIndicatorManager;

        /// <summary>
        /// Instance of the Data Manager.
        /// </summary>
        public static DataManager DataManager => Instance.dataManager;

        /// <summary>
        /// Instance of the Time Manager.
        /// </summary>
        public static TimeManager TimeManager => Instance.timeManager;

        /// <summary>
        /// Instance of the Avatar Manager.
        /// </summary>
        public static AvatarManager AvatarManager => Instance.avatarManager;

        /// <summary>
        /// Instance of the Locomotion Manager.
        /// </summary>
        public static LocomotionManager LocomotionManager => Instance.locomotionManager;

        /// <summary>
        /// Instance of the Collaboration Manager.
        /// </summary>
        public static CollaborationManager CollaborationManager => Instance.collaborationManager;

        /// <summary>
        /// Instance of the Selection Manager.
        /// </summary>
        public static SelectionManager SelectionManager => Instance.selectionManager;

        /// <summary>
        /// Easy Build System components.
        /// </summary>
#if MRET_EXTENSION_EASYBUILDSYSTEM
        public static BuildManager BuildManager => Instance.buildManager;
        public static BuildEvent BuildEvent => Instance.buildEvent;
#endif

        /// <summary>
        /// Instance of the Keyboard Manager.
        /// </summary>
        public static KeyboardManager KeyboardManager => Instance.keyboardManager;

        /// <summary>
        /// Instance of the MRET Autosave handler.
        /// </summary>
        public static AutosaveManager AutosaveManager => Instance.autosaveManager;

        /// <summary>
        /// Instance of the Mode Navigator.
        /// </summary>
        public static ModeNavigator ModeNavigator => Instance.modeNavigator;

        /// <summary>
        /// Instance of the Project Manager.
        /// </summary>
        public static ProjectManager ProjectManager => Instance.projectManager;

        /// <summary>
        /// Instance of the Project Manager.
        /// </summary>
        public static UnityProjectDeprecated ProjectDeprecated => Instance.projectDeprecated;

        /// <summary>
        /// Instance of the Configuration Manager.
        /// </summary>
        public static ConfigurationManager ConfigurationManager => Instance.configurationManager;

        /// <summary>
        /// Instance of the IoT Manager.
        /// </summary>
        public static HudManager HudManager => Instance.hudManager;

        /// <summary>
        /// Instance of the IK Manager.
        /// </summary>
        public static IKManager IKManager => Instance.ikManager;

        /// <summary>
        /// Instance of the Control Mode script.
        /// </summary>
        public static ControlMode ControlMode => Instance.controlMode;

        /// <summary>
        /// Material used to indicate collision.
        /// </summary>
        public static Material CollisionMaterial => Instance.collisionMaterial;

        /// <summary>
        /// Material used to indicate highlighting.
        /// </summary>
        public static Material HighlightMaterial => Instance.highlightMaterial;

        /// <summary>
        /// Material used to indicate selection.
        /// </summary>
        public static Material SelectMaterial => Instance.selectMaterial;

        /// <summary>
        /// Material used for showing limit states.
        /// </summary>
        public static Material LimitMaterial => Instance.limitMaterial;

        /// <summary>
        /// Material used for line drawings.
        /// </summary>
        public static Material LineDrawingMaterial => Instance.lineDrawingMaterial;

        /// <summary>
        /// Audio clip used to indicate collision.
        /// </summary>
        public static AudioClip CollisionSound => Instance.collisionSound;

        /// <summary>
        /// Instance of the UUID Registry.
        /// </summary>
        [Tooltip("Instance of the UUID Registry.")]
        public MRETUuidRegistryManager uuidRegistry;

        /// <summary>
        /// Instance of the Schema Handler.
        /// </summary>
        [Tooltip("Instance of the Schema Handler.")]
        public SchemaHandler schemaHandler;

        /// <summary>
        /// Instance of the Input Rig.
        /// </summary>
        [Tooltip("Instance of the Input Rig.")]
        public InputRig inputRig;

        /// <summary>
        /// Instance of the Loading Indicator Manager.
        /// </summary>
        [Tooltip("Instance of the Loading Indicator Manager.")]
        public LoadingIndicatorManager loadingIndicatorManager;

        /// <summary>
        /// Instance of the Data Manager.
        /// </summary>
        [Tooltip("Instance of the Data Manager.")]
        public DataManager dataManager;

        /// <summary>
        /// Instance of the Time Manager.
        /// </summary>
        [Tooltip("Instance of the Time Manager.")]
        public TimeManager timeManager;

        /// <summary>
        /// Instance of the Locomotion Manager.
        /// </summary>
        [Tooltip("Instance of the Locomotion Manager.")]
        public LocomotionManager locomotionManager;

        /// <summary>
        /// Instance of the Avatar Manager.
        /// </summary>
        [Tooltip("Instance of the Avatar Manager.")]
        public AvatarManager avatarManager;

        /// <summary>
        /// Instance of the Collaboration Manager.
        /// </summary>
        [Tooltip("Instance of the Collaboration Manager.")]
        public CollaborationManager collaborationManager;

        /// <summary>
        /// Instance of the Selection Manager.
        /// </summary>
        [Tooltip("Instance of the Selection Manager.")]
        public SelectionManager selectionManager;

        /// <summary>
        /// Instance of the Keyboard Manager.
        /// </summary>
        [Tooltip("Instance of the Keyboard Manager.")]
        public KeyboardManager keyboardManager;

        /// <summary>
        /// Instance of the MRET Autosave handler.
        /// </summary>
        [Tooltip("Instance of the Autosave Manager.")]
        public AutosaveManager autosaveManager;

        /// <summary>
        /// Instance of the Project Manager.
        /// </summary>
        [Tooltip("Instance of the Project Manager.")]
        public ProjectManager projectManager;

        /// <summary>
        /// Instance of the deprecated Project.
        /// </summary>
        [Tooltip("Instance of the Deprecatetd Project.")]
        public UnityProjectDeprecated projectDeprecated;

        /// <summary>
        /// Instance of the Configuration Manager.
        /// </summary>
        [Tooltip("Instance of the Configuration Manager.")]
        public ConfigurationManager configurationManager;

        /// <summary>
        /// Instance of the HUD Manager.
        /// </summary>
        [Tooltip("Instance of the HUD Manager.")]
        public HudManager hudManager;

        /// <summary>
        /// Instance of the IK Manager.
        /// </summary>
        [Tooltip("Instance of the IK Manager.")]
        public IKManager ikManager;

        /// <summary>
        /// Instance of the Mode Navigator.
        /// </summary>
        [Tooltip("Instance of the Mode Navigator.")]
        public ModeNavigator modeNavigator;

        /// <summary>
        /// Instance of the Control Mode script.
        /// </summary>
        [Tooltip("Instance of the Control Mode script.")]
        public ControlMode controlMode;

        /// <summary>
        /// Instance of the Kiosk Mode Loader.
        /// </summary>
        [Tooltip("Instance of the Kiosk Mode Loader.")]
        public KioskLoader kioskLoader;

        /// <summary>
        /// Material used to indicate collision.
        /// </summary>
        [Tooltip("Material used to indicate collision.")]
        public Material collisionMaterial;

        /// <summary>
        /// Material used to indicate highlighting.
        /// </summary>
        [Tooltip("Material used to indicate highlighting.")]
        public Material highlightMaterial;

        /// <summary>
        /// Material used to indicate selection.
        /// </summary>
        [Tooltip("Material used to indicate selection.")]
        public Material selectMaterial;

        /// <summary>
        /// Material used for showing limit states.
        /// </summary>
        [Tooltip("Material used for showing limit states.")]
        public Material limitMaterial;

        /// <summary>
        /// Material used for line drawings.
        /// </summary>
        [Tooltip("Material used for line drawings.")]
        public Material lineDrawingMaterial;

        /// <summary>
        /// Audio clip used to indicate collision.
        /// </summary>
        [Tooltip("Audio clip used to indicate collision.")]
        public AudioClip collisionSound;

        /// <summary>
        /// Easy Build System components.
        /// </summary>
#if MRET_EXTENSION_EASYBUILDSYSTEM
        private BuildManager buildManager;
        private BuildEvent buildEvent;
#endif

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            string message =
                "Mixed Reality Exploration Toolkit (MRET) version: " + Application.version + "\n" +
                "Unity version: " + Application.unityVersion + "\n" +
                "Build UUID: " + Application.buildGUID + "\n" +
                "Platform: " + Application.platform + "\n" +
                "Is Editor: " + Application.isEditor;
            Log(message);

            Log("Initializing...");

            // Save the main thread reference because we cannot call some MonoBehaviour functions outside
            // of the main thread
            mainThread = Thread.CurrentThread;

            // TODO: Perform integrity checks? Where should those be done?

#if MRET_EXTENSION_EASYBUILDSYSTEM
            Log("Initializing the Easy Build System...", nameof(Initialize));
            {
                buildManager = FindObjectOfType<BuildManager>();
                if (buildManager == null)
                {
                    buildManager = gameObject.AddComponent<BuildManager>();
                }
                buildEvent = FindObjectOfType<BuildEvent>();
                if (buildEvent == null)
                {
                    buildEvent = gameObject.AddComponent<BuildEvent>();
                }
            }
            Log("Easy Build System initialized.", nameof(Initialize));
#else
            Log("Easy Build System is unavailable.", nameof(Initialize));
#endif

            Log("Initializing UUID Registry...", nameof(Initialize));
            if (uuidRegistry == null)
            {
                uuidRegistry = FindObjectOfType<MRETUuidRegistryManager>();
                if (uuidRegistry == null)
                {
                    LogError("Fatal Error. Unable to initialize the UUID registry. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            uuidRegistry.Initialize();
            Log("UUID registry initialized.", nameof(Initialize));

            Log("Loading schemas...", nameof(Initialize));
            if (schemaHandler == null)
            {
                schemaHandler = FindObjectOfType<SchemaHandler>();
                if (schemaHandler == null)
                {
                    LogError("Fatal Error. Unable to initialize schemas. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            Log("Schemas loaded.", nameof(Initialize));

            Log("Starting loading indicator...", nameof(Initialize));
            if (loadingIndicatorManager == null)
            {
                loadingIndicatorManager = FindObjectOfType<LoadingIndicatorManager>();
                if (loadingIndicatorManager == null)
                {
                    LogError("Fatal Error. Unable to start loading indicator. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            loadingIndicatorManager.Initialize();
            Log("Loading indicator ready.", nameof(Initialize));

            Log("Initializing data manager...", nameof(Initialize));
            if (dataManager == null)
            {
                dataManager = FindObjectOfType<DataManager>();
                if (dataManager == null)
                {
                    LogError("Fatal Error. Unable to start data manager. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            dataManager.Initialize();
            Log("Data manager initialized.", nameof(Initialize));

            Log("Initializing time manager...", nameof(Initialize));
            if (timeManager == null)
            {
                timeManager = FindObjectOfType<TimeManager>();
                if (timeManager == null)
                {
                    LogError("Fatal Error. Unable to start time manager. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            timeManager.Initialize();
            Log("Time manager initialized.", nameof(Initialize));

            Log("Initializing configuration...", nameof(Initialize));
            if (configurationManager == null)
            {
                configurationManager = FindObjectOfType<ConfigurationManager>();
                if (configurationManager == null)
                {
                    LogError("Fatal Error. Unable to initialize configuration. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            configurationManager.Initialize();
            Log("Configuration initialized.", nameof(Initialize));

            Log("Initializing HUD system...", nameof(Initialize));
            if (hudManager == null)
            {
                hudManager = FindObjectOfType<HudManager>();
                if (hudManager == null)
                {
                    LogError("Fatal Error. Unable to initialize HUD system. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            hudManager.Initialize();
            Log("HUD system initialized.", nameof(Initialize));

            Log("Setting up input rig...", nameof(Initialize));
            if (inputRig == null)
            {
                inputRig = FindObjectOfType<InputRig>();
                if (inputRig == null)
                {
                    LogError("Fatal Error. Unable to set up input rig. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            inputRig.Initialize(InputHand.ControllerMode.Controller);
            Log("Input rig initialized.", nameof(Initialize));

            Log("Setting up avatar system...", nameof(Initialize));
            if (avatarManager == null)
            {
                avatarManager = FindObjectOfType<AvatarManager>();
                if (avatarManager == null)
                {
                    LogError("Fatal Error. Unable to set up the avatar system. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            avatarManager.Initialize();
            Log("Avatar system initialized.", nameof(Initialize));

            Log("Setting up collaboration system...", nameof(Initialize));
            if (collaborationManager == null)
            {
                collaborationManager = FindObjectOfType<CollaborationManager>();
                if (collaborationManager == null)
                {
                    LogError("Fatal Error. Unable to set up the collaboration system. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            collaborationManager.Initialize();
            Log("Collaboration system initialized.", nameof(Initialize));

            Log("Initializing user locomotion system...", nameof(Initialize));
            if (locomotionManager == null)
            {
                locomotionManager = FindObjectOfType<LocomotionManager>();
                if (locomotionManager == null)
                {
                    LogError("Fatal Error. Unable to initialize user locomotion system. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            locomotionManager.Initialize();
            Log("User locomotion system initialized.", nameof(Initialize));

            Log("Initializing keyboards...", nameof(Initialize));
            if (keyboardManager == null)
            {
                keyboardManager = FindObjectOfType<KeyboardManager>();
                if (keyboardManager == null)
                {
                    LogError("Fatal Error. Unable to initialize keyboards. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            keyboardManager.Initialize();
            Log("Keyboards initialized.", nameof(Initialize));

            Log("Initializing menu system...", nameof(Initialize));
            foreach (MenuAdapter adapter in FindObjectsOfType<MenuAdapter>())
            {
                adapter.Initialize();
                adapter.menu.HideMenu(false);
            }

            foreach (MenuController mc in FindObjectsOfType<MenuController>())
            {
                mc.Initialize();
            }
            Log("Menu system initialized.", nameof(Initialize));

            Log("Initializing mode navigation system...", nameof(Initialize));
            if (modeNavigator == null)
            {
                modeNavigator = FindObjectOfType<ModeNavigator>();
                if (modeNavigator == null)
                {
                    LogError("Fatal Error. Unable to initialize mode navigation system. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            modeNavigator.Initialize();
            Log("Mode navigation system initialized.", nameof(Initialize));

            Log("Initializing user control state machine...", nameof(Initialize));
            if (controlMode == null)
            {
                controlMode = FindObjectOfType<ControlMode>();
                if (ControlMode == null)
                {
                    LogError("Fatal Error. Unable to initialize user control state machine. Aborting...");
                    Exit();
                }
            }
            controlMode.Initialize();
            Log("User control state machine initialized.", nameof(Initialize));

            Log("Initializing selection management...", nameof(Initialize));
            if (selectionManager == null)
            {
                selectionManager = FindObjectOfType<SelectionManager>();
                if (selectionManager == null)
                {
                    LogError("Fatal Error. Unable to initialize selection manager. Aborting...");
                    Exit();
                }
            }
            foreach (ControllerSelectionManager csm in FindObjectsOfType<ControllerSelectionManager>())
            {
                csm.Initialize();
            }
            selectionManager.Initialize();
            Log("Selection management initialized.", nameof(Initialize));

            Log("Initializing Project Manager...", nameof(Initialize));
            if (projectManager == null)
            {
                projectManager = FindObjectOfType<ProjectManager>();
                if (projectManager == null)
                {
                    LogError("Fatal Error. Unable to initialize project. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            projectManager.Initialize();
            Log("Project manager initialized.", nameof(Initialize));

            Log("Initializing IK system...", nameof(Initialize));
            if (ikManager == null)
            {
                ikManager = FindObjectOfType<IKManager>();
                if (ikManager == null)
                {
                    LogError("Fatal Error. Unable to initialize IK system. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            ikManager.Initialize();
            Log("IK system initialized.", nameof(Initialize));

            Log("Initializing autosave...", nameof(Initialize));
            if (autosaveManager == null)
            {
                autosaveManager = FindObjectOfType<AutosaveManager>();
                if (autosaveManager == null)
                {
                    LogError("Fatal Error. Unable to initialize autosave manager. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            autosaveManager.Initialize();
            Log("Autosave initialized.", nameof(Initialize));

            Log("Initializing kiosk loader...", nameof(Initialize));
            if (kioskLoader == null)
            {
                kioskLoader = FindObjectOfType<KioskLoader>();
                if (kioskLoader == null)
                {
                    LogError("Fatal Error. Unable to initialize kiosk loader. Aborting...", nameof(Initialize));
                    Exit();
                }
            }
            Log("Kiosk Loader initialized.", nameof(Initialize));

            Log("Searching for kiosk file...", nameof(Initialize));
            string kioskFilePath = System.IO.Path.Combine(Application.dataPath, kioskModeFile);
            if (System.IO.File.Exists(kioskFilePath))
            {
                Log("Kiosk file detected at " + kioskFilePath + ". Loading Kiosk file...", nameof(Initialize));
                kioskLoader.LoadKioskMode(kioskFilePath);
                Log("Kiosk file loaded.", nameof(Initialize));
            }
            else
            {
                Log("No kiosk file detected. Loading lobby...", nameof(Initialize));
                modeNavigator.LoadLobby();
                Log("Lobby loaded.", nameof(Initialize));
            }

            Log("Initialized.");
        }

        /// <summary>
        /// Called to exit MRET.
        /// </summary>
        public static void Quit()
        {
            MRET.Instance.Exit();
        }

        /// <summary>
        /// Called to exit MRET.
        /// </summary>
        public void Exit()
        {
            // quit MRET
            Log("Exiting", nameof(Exit));

#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }
}