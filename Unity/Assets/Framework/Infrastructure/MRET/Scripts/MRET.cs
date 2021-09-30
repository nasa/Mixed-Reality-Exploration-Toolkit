// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Common;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Time;
using GSFC.ARVR.MRET.Selection;
using PointCloudViewer;
using GSFC.ARVR.MRET.ProceduralObjectGeneration;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Infrastructure.Framework.Locomotion;
using GSFC.ARVR.MRET.Integrations.XRUI;
using GSFC.ARVR.MRET.XRC;
using GSFC.ARVR.MRET.Components.Keyboard;
using GSFC.ARVR.MRET.Components.Notes;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Components.IK;

namespace GSFC.ARVR.MRET.Infrastructure.Framework
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
    public class MRET : MonoBehaviour
    {
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

        /// <summary>
        /// Instance of the MRET object.
        /// </summary>
        private static MRET instance;

        /// <summary>
        /// Instance of the Schema Handler.
        /// </summary>
        public static SchemaHandler SchemaHandler
        {
            get
            {
                return instance.schemaHandler;
            }
        }

        /// <summary>
        /// Instance of the Input Rig.
        /// </summary>
        public static InputRig InputRig
        {
            get
            {
                return instance.inputRig;
            }
        }

        /// <summary>
        /// Instance of the Data Manager.
        /// </summary>
        public static DataManager DataManager
        {
            get
            {
                return instance.dataManager;
            }
        }

        /// <summary>
        /// Instance of the Time Manager.
        /// </summary>
        public static TimeManager TimeManager
        {
            get
            {
                return instance.timeManager;
            }
        }

        /// <summary>
        /// Instance of the Locomotion Manager.
        /// </summary>
        public static LocomotionManager LocomotionManager
        {
            get
            {
                return instance.locomotionManager;
            }
        }

        /// <summary>
        /// Instance of the Selection Manager.
        /// </summary>
        public static SelectionManager SelectionManager
        {
            get
            {
                return instance.selectionManager;
            }
        }

        /// <summary>
        /// Instance of the Point Cloud Manager.
        /// </summary>
        public static PointCloudManager PointCloudManager
        {
            get
            {
                return instance.pointCloudManager;
            }
        }

        /// <summary>
        /// Instance of the Procedural Object Generator Manager.
        /// </summary>
        public static ProceduralObjectGeneratorManager ProceduralObjectGeneratorManager
        {
            get
            {
                return instance.proceduralObjectGeneratorManager;
            }
        }

        /// <summary>
        /// Instance of the Build Manager.
        /// </summary>
        public static BuildManager BuildManager
        {
            get
            {
                return instance.buildManager;
            }
        }

        /// <summary>
        /// Instance of the XRC Manager.
        /// </summary>
        public static XRCManager XRCManager
        {
            get
            {
                return instance.xrcManager;
            }
        }

        /// <summary>
        /// Instance of the Keyboard Manager.
        /// </summary>
        public static KeyboardManager KeyboardManager
        {
            get
            {
                return instance.keyboardManager;
            }
        }

        /// <summary>
        /// Instance of the Note Manager.
        /// </summary>
        public static NoteManager NoteManager
        {
            get
            {
                return instance.noteManager;
            }
        }

        /// <summary>
        /// Instance of the Project Manager.
        /// </summary>
        public static UnityProject Project
        {
            get
            {
                return instance.project;
            }
        }

        /// <summary>
        /// Instance of the Part Manager.
        /// </summary>
        public static PartManager PartManager
        {
            get
            {
                return instance.partManager;
            }
        }

        /// <summary>
        /// Instance of the Configuration Manager.
        /// </summary>
        public static ConfigurationManager ConfigurationManager
        {
            get
            {
                return instance.configurationManager;
            }
        }

        /// <summary>
        /// Instance of the IK Manager.
        /// </summary>
        public static IKManager IKManager
        {
            get
            {
                return instance.ikManager;
            }
        }

        /// <summary>
        /// Instance of the Control Mode script.
        /// </summary>
        public static ControlMode ControlMode
        {
            get
            {
                return instance.controlMode;
            }
        }

        /// <summary>
        /// List of materials to select from to apply to parts by default.
        /// </summary>
        public static List<Material> DefaultPartMaterials
        {
            get
            {
                return instance.defaultPartMaterials;
            }
        }

        /// <summary>
        /// Material used to indicate collision.
        /// </summary>
        public static Material CollisionMaterial
        {
            get
            {
                return instance.collisionMaterial;
            }
        }

        /// <summary>
        /// Material used to indicate highlighting.
        /// </summary>
        public static Material HighlightMaterial
        {
            get
            {
                return instance.highlightMaterial;
            }
        }

        /// <summary>
        /// Material used to indicate selection.
        /// </summary>
        public static Material SelectMaterial
        {
            get
            {
                return instance.selectMaterial;
            }
        }

        /// <summary>
        /// Audio clip used to indicate collision.
        /// </summary>
        public static AudioClip CollisionSound
        {
            get
            {
                return instance.collisionSound;
            }
        }

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        public static GameObject DrawingPanelPrefab
        {
            get
            {
                return instance.drawingPanelPrefab;
            }
        }

        /// <summary>
        /// Container for parts/objects.
        /// </summary>
        public static GameObject ObjectPlacementContainer
        {
            get
            {
                return instance.objectPlacementContainer;
            }
        }

        /// <summary>
        /// Whether or not the part panel is allowed to be shown.
        /// </summary>
        public static bool PartPanelEnabled
        {
            get
            {
                return instance.partPanelEnabled;
            }
            set
            {
                instance.partPanelEnabled = value;
            }
        }

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
        /// Instance of the Selection Manager.
        /// </summary>
        [Tooltip("Instance of the Selection Manager.")]
        public SelectionManager selectionManager;

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
        /// Instance of the Build Manager.
        /// </summary>
        [Tooltip("Instance of the Build Manager.")]
        public BuildManager buildManager;

        /// <summary>
        /// Instance of the XRC Manager.
        /// </summary>
        [Tooltip("Instance of the XRC Manager.")]
        public XRCManager xrcManager;

        /// <summary>
        /// Instance of the Keyboard Manager.
        /// </summary>
        [Tooltip("Instance of the Keyboard Manager.")]
        public KeyboardManager keyboardManager;

        /// <summary>
        /// Instance of the Note Manager.
        /// </summary>
        [Tooltip("Instance of the Note Manager.")]
        public NoteManager noteManager;

        /// <summary>
        /// Instance of the Project Manager.
        /// </summary>
        [Tooltip("Instance of the Project Manager.")]
        public UnityProject project;

        /// <summary>
        /// Instance of the Part Manager.
        /// </summary>
        [Tooltip("Instance of the Part Manager.")]
        public PartManager partManager;

        /// <summary>
        /// Instance of the Configuration Manager.
        /// </summary>
        [Tooltip("Instance of the Configuration Manager.")]
        public ConfigurationManager configurationManager;

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
        /// List of materials to select from to apply to parts by default.
        /// </summary>
        [Tooltip("List of materials to select from to apply to parts by default.")]
        public List<Material> defaultPartMaterials;

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
        /// Audio clip used to indicate collision.
        /// </summary>
        [Tooltip("Audio clip used to indicate collision.")]
        public AudioClip collisionSound;

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        [Tooltip("Prefab to use for drawing panels.")]
        public GameObject drawingPanelPrefab;

        /// <summary>
        /// Container for parts/objects.
        /// </summary>
        [Tooltip("Container for parts/objects.")]
        public GameObject objectPlacementContainer;

        /// <summary>
        /// Whether or not the part panel is allowed to be shown.
        /// </summary>
        [Tooltip("Whether or not the part panel is allowed to be shown.")]
        public bool partPanelEnabled = true;

        /// <summary>
        /// This is the very first thing run.
        /// </summary>
        void Awake()
        {
            instance = this;

            Debug.Log("[MRET] Mixed Reality Exploration Toolkit version " + Application.version + ".");
            Debug.Log("[MRET] Unity version: " + Application.unityVersion + ". Build UUID: " + Application.buildGUID
                + ". Is Editor: " + Application.isEditor + ". Platform: " + Application.platform + ".");

            Debug.Log("[MRET] Initializing...");
            Initialize();
            Debug.Log("[MRET] Initialized.");
        }

        /// <summary>
        /// Perform complete initialization of MRET.
        /// It attempts to resolve all references
        /// and perform initialization. If any
        /// references are unresolved, MRET will exit.
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[MRET] Loading schemas...");
            if (schemaHandler == null)
            {
                schemaHandler = FindObjectOfType<SchemaHandler>();
                if (schemaHandler == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize schemas. Aborting...");
                    Application.Quit();
                }
            }
            schemaHandler.InitializeSchemas();
            Debug.Log("[MRET] Schemas loaded.");

            Debug.Log("[MRET] Starting Data Manager...");
            if (dataManager == null)
            {
                dataManager = FindObjectOfType<DataManager>();
                if (dataManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to start data manager. Aborting...");
                    Application.Quit();
                }
            }
            dataManager.Initialize();
            Debug.Log("[MRET] Data Manager ready.");

            Debug.Log("[MRET] Starting Time Manager...");
            if (timeManager == null)
            {
                timeManager = FindObjectOfType<TimeManager>();
                if (timeManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to start time manager. Aborting...");
                    Application.Quit();
                }
            }
            timeManager.Initialize();
            Debug.Log("[MRET] Time Manager ready.");

            Debug.Log("[MRET] Initializing Configuration Manager...");
            if (configurationManager == null)
            {
                configurationManager = FindObjectOfType<ConfigurationManager>();
                if (configurationManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize configuration manager. Aborting...");
                    Application.Quit();
                }
            }
            configurationManager.Initialize();
            Debug.Log("[MRET] Configuration Manager initialized.");

            Debug.Log("[MRET] Setting up input rig...");
            if (inputRig == null)
            {
                inputRig = FindObjectOfType<InputRig>();
                if (schemaHandler == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to set up input rig. Aborting...");
                    Application.Quit();
                }
            }
            // Initialize the rig
            inputRig.Initialize(configurationManager.config.Avatars, InputHand.ControllerMode.Controller);

            foreach (MenuAdapter adapter in FindObjectsOfType<MenuAdapter>())
            {
                adapter.Initialize();
                adapter.menu.HideMenu(false);
            }

            foreach (MenuController mc in FindObjectsOfType<MenuController>())
            {
                mc.Initialize();
            }

            foreach (ControllerSelectionManager csm in FindObjectsOfType<ControllerSelectionManager>())
            {
                csm.Initialize();
            }
            Debug.Log("[MRET] Input rig ready.");

            Debug.Log("[MRET] Initializing Selection Manager...");
            if (selectionManager == null)
            {
                selectionManager = FindObjectOfType<SelectionManager>();
                if (selectionManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize selection manager. Aborting...");
                    Application.Quit();
                }
            }
            selectionManager.Initialize();
            Debug.Log("[MRET] Selection Manager initialized.");

            Debug.Log("[MRET] Loading Point Cloud Manager...");
            if (pointCloudManager == null)
            {
                pointCloudManager = FindObjectOfType<PointCloudManager>();
                if (pointCloudManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to load point cloud manager. Aborting...");
                    Application.Quit();
                }
                // Not our script, can't control initialization.
            }
            Debug.Log("[MRET] Point Cloud Manager ready.");

            Debug.Log("[MRET] Initializing Control Mode...");
            if (controlMode == null)
            {
                controlMode = FindObjectOfType<ControlMode>();
                if (ControlMode == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize control mode. Aborting...");
                    Application.Quit();
                }
            }
            controlMode.Initialize();
            Debug.Log("[MRET] Control Mode initialized.");

            Debug.Log("[MRET] Initializing Mode Navigator...");
            if (modeNavigator == null)
            {
                modeNavigator = FindObjectOfType<ModeNavigator>();
                if (modeNavigator == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize mode navigator. Aborting...");
                    Application.Quit();
                }
            }
            modeNavigator.Initialize();
            Debug.Log("[MRET] Mode Navigator initialized.");

            Debug.Log("[MRET] Initializing Locomotion Manager...");
            if (locomotionManager == null)
            {
                locomotionManager = FindObjectOfType<LocomotionManager>();
                if (locomotionManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize locomotion manager. Aborting...");
                    Application.Quit();
                }
            }
            locomotionManager.Initialize();
            Debug.Log("[MRET] Locomotion Manager initialized.");

            Debug.Log("[MRET] Initializing XRC Manager...");
            if (xrcManager == null)
            {
                xrcManager = FindObjectOfType<XRCManager>();
                if (xrcManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize xrc manager. Aborting...");
                    Application.Quit();
                }
            }
            xrcManager.Initialize();
            Debug.Log("[MRET] XRC Manager initialized.");

            Debug.Log("[MRET] Initializing Keyboard Manager...");
            if (keyboardManager == null)
            {
                keyboardManager = FindObjectOfType<KeyboardManager>();
                if (keyboardManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize keyboard manager. Aborting...");
                    Application.Quit();
                }
            }
            keyboardManager.Initialize();
            Debug.Log("[MRET] Keyboard Manager initialized.");

            Debug.Log("[MRET] Initializing Note Manager...");
            if (noteManager == null)
            {
                noteManager = FindObjectOfType<NoteManager>();
                if (noteManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize note manager. Aborting...");
                    Application.Quit();
                }
            }
            noteManager.Initialize();
            Debug.Log("[MRET] Note Manager initialized.");

            Debug.Log("[MRET] Initializing Project Manager...");
            if (project == null)
            {
                project = FindObjectOfType<UnityProject>();
                if (project == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize project. Aborting...");
                    Application.Quit();
                }
            }
            project.Initialize();
            Debug.Log("[MRET] Project initialized.");

            Debug.Log("[MRET] Initializing Part Manager...");
            if (partManager == null)
            {
                partManager = FindObjectOfType<PartManager>();
                if (partManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize part manager. Aborting...");
                    Application.Quit();
                }
            }
            partManager.Initialize();
            Debug.Log("[MRET] Part Manager initialized.");

            Debug.Log("[MRET] Initializing IK Manager...");
            if (ikManager == null)
            {
                ikManager = FindObjectOfType<IKManager>();
                if (ikManager == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize IK manager. Aborting...");
                    Application.Quit();
                }
            }
            ikManager.Initialize();
            Debug.Log("[MRET] IK Manager initialized.");

            Debug.Log("[MRET] Initializing Kiosk Loader...");
            if (kioskLoader == null)
            {
                kioskLoader = FindObjectOfType<KioskLoader>();
                if (kioskLoader == null)
                {
                    Debug.LogError("[MRET] Fatal Error. Unable to initialize Kiosk loader. Aborting...");
                    Application.Quit();
                }
            }
            Debug.Log("[MRET] Kiosk Loader initialized.");

            Debug.Log("[MRET] Searching for Kiosk File...");
            string kioskFilePath = System.IO.Path.Combine(Application.dataPath, kioskModeFile);
            if (System.IO.File.Exists(kioskFilePath))
            {
                Debug.Log("[MRET] Kiosk Mode File detected at " + kioskFilePath + ". Loading Kiosk Mode.");
                kioskLoader.LoadKioskMode(kioskFilePath);
                Debug.Log("[MRET] Kiosk Mode File loaded.");
            }
            else
            {
                Debug.Log("[MRET] No Kiosk Mode File Detected. Loading Lobby.");
                modeNavigator.LoadLobby();
                Debug.Log("[MRET] Lobby loaded.");
            }
        }
    }
}