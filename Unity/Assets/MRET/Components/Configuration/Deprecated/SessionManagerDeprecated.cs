// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Time;

namespace GOV.NASA.GSFC.XR.MRET.Configuration
{
    /**
     * Defines the MRET session manager. This class contains common (system-wide) GameObject references
     * and component/script accessor properites. This class should be used by all MRET scripts that require
     * access to these objects, components and scripts in order to centralize access and special logic in a
     * single place across MRET.
     * 
     * All public properties may be assigned in the Unity editor. If not assigned, this class will attempt
     * to locate the reference by name (defined as public constants) during the initialization process. If
     * the reference cannot be obtained via name, a warning withh be logged and processing will continue with
     * a null reference.
     * 
     * A few important notes about access to the references in this class:
     * 
     *  - The GameObjects, Components and Scripts are initialized in the Awake method of this class. Access to
     *    the field references by MonoBehavior scripts should NOT be done in their own Awake method due to the
     *    indeterministic (by design) ordering of Awake calls in Unity for MonoBehavior scripts. Access to the field
     *    references should only be performed in Start or Update methods.
     *  - Not all fields are guaranteed to have a value. Some values are not relevant for Desktop mode, for example.
     *    If a field is not set, it will be null.
     * 
     * The common GameObjects, Components and scripts include, but are not limited to:
     * 
     *      Property                    Description
     *      -----------------------     ----------------------------------------
     *      sessionConfiguration        The <code>SessionConfiguration</code> containing common session
     *                                  configuration settings (VR/Desktop)
     *      configurationManager        The <code>ConfigurationManager</code> containing the system-wide
     *                                  MRET configuration settings (VR/Desktop)
     *      collaborationManager        The <code>CollaborationManager</code> containing the collaboration
     *                                  settings for MRET (VR/Desktop)
     *      dataManager                 The global <code>DataManager</code> used for sharing telemetry between
     *                                  MRET scripts (VR/Desktop)
     *      timeManager                 The global <code>TimeManager</code> used to control the simulated
     *                                  MRET system time (VR/Desktop)
     *      loadedProject               The current loaded MRET project (VR/Desktop)
     *      playArea                    The play area for the user (VR)
     *      headsetFollower             The current, local session user (VR/Desktop)
     *      leftController              The local user left controller (VR)
     *      rightController             The local user right controller (VR)
     * 
     *      Property Accessor           Description
     *      ----------------------      ----------------------------------------
     *      UnityProject                The <code>UnityProject</code> script associated with the loaded MRET
     *                                  project (VR/Desktop)
     *      BasicTeleport               The <code>VRTK_BasicTeleport</code> script associated with the user
     *                                  play area (VR)
     *      BodyPhysics                 The <code>VRTK_BodyPhysics</code> script associated with the user
     *                                  play area (VR)
     *      MoveInPlace                 The <code>VRTK_MoveInPlace</code> script associated with the user
     *                                  play area (VR)
     *      UserCamera                  The <code>Camera</code> associated with the user's view (VR/Desktop)
     *      PointerLeft                 The <code>VRTK_Pointer</code> associated with the user's left controller
     *                                  (VR)
     *      UIPointerLeft               The <code>VRTK_UIPointer</code> associated with the user's left controller
     *                                  (VR)
     *      PointerRight                The <code>VRTK_Pointer</code> associated with the user's right controller
     *                                  (VR)
     *      UIPointerRight              The <code>VRTK_UIPointer</code> associated with the user's right controller
     *                                  (VR)
     * 
     * @see SessionConfiguration
     * @see ConfigurationManager
     * @see CollaborationManager
     * @see DataManager
     * @see TimeManager
     * @see UnityProject
     * @see VRTK_BasicTeleport
     * @see VRTK_BodyPhysics
     * @see VRTK_MoveInPlace
     * @see VRTK_Pointer
     * @see VRTK_UIPointer
     * @see Camera
     * 
     * @author Jeffrey Hosler
     * Edits:
     * 18 February 2021: Removing fields that are no longer needed.
     */
    [Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.MRET))]
    public class SessionManagerDeprecated : MonoBehaviour
    {
        public static readonly string NAME = nameof(SessionManagerDeprecated);

        public static SessionManagerDeprecated instance;

        public const string SESSION_OBJECT_NAME_COLLABORATION_MANAGER = "CollaborationManager";
        public const string SESSION_OBJECT_NAME_DATA_MANAGER = "DataManager";
        public const string SESSION_OBJECT_NAME_TIME_MANAGER = "TimeManager";
        public const string SESSION_OBJECT_NAME_DISPLAY_MODE_CONTROLLER = "DesktopSwitcher";
        public const string SESSION_OBJECT_NAME_LOADED_PROJECT = "LoadedProject";
        public const string SESSION_OBJECT_NAME_PLAY_AREA = "PlayArea";
        public const string SESSION_OBJECT_NAME_HEADSET_FOLLOWER = "HeadsetFollower";
        public const string SESSION_OBJECT_NAME_LEFT_CONTROLLER = "LeftController";
        public const string SESSION_OBJECT_NAME_RIGHT_CONTROLLER = "RightController";

        // Configuration
        [Tooltip("The SessionConfiguration controlling the session configuration of MRET")]
        public SessionConfiguration sessionConfiguration;
        [Tooltip("The ConfigurationManager controlling the global configuration of MRET")]
        public ConfigurationManager configurationManager;
        [Tooltip("The CollaborationManager controlling the collaboration capabilities of MRET")]
        public CollaborationManagerDeprecated collaborationManager;

        // Modules
        [Tooltip("The session DataManager to use for storing and retrieving information and telemetry across MRET")]
        public DataManager dataManager;
        [Tooltip("The session TimeManager to use for managing the simulated time across MRET")]
        public TimeManager timeManager;

        // GameObjects
        [Tooltip("The UnityProject controlling the current loaded project of MRET")]
        public GameObject loadedProject;
        [Tooltip("The session PlayArea for the user")]
        public GameObject playArea;
        [Tooltip("The session user")]
        public GameObject headsetFollower;
        [Tooltip("The session user left controller")]
        public GameObject leftController;
        [Tooltip("The session user left controller")]
        public GameObject rightController;

        // Private fields for property access to specific scripts
        #region SCRIPT_ACCESS

        // LoadedProject scripts
        #region SCRIPT_ACCESS_LOADED_PROJECT

        private UnityProjectDeprecated unityProject;
        public UnityProjectDeprecated UnityProject
        {
            get
            {
                return unityProject;
            }
        }

        #endregion //SCRIPT_ACCESS_LOADED_PROJECT

        // PlayArea scripts
        #region SCRIPT_ACCESS_PLAY_AREA

        public static InputRig inputRig
        {
            get
            {
                return MRET.InputRig;
            }
        }

        /*private VRTK.VRTK_BasicTeleport basicTeleport;
        public VRTK.VRTK_BasicTeleport BasicTeleport
        {
            get
            {
                return basicTeleport;
            }
        }*/

        /*private VRTK.VRTK_BodyPhysics bodyPhysics;
        public VRTK.VRTK_BodyPhysics BodyPhysics
        {
            get
            {
                return bodyPhysics;
            }
        }*/

        /*private VRTK.VRTK_MoveInPlace moveInPlace;
        public VRTK.VRTK_MoveInPlace MoveInPlace
        {
            get
            {
                return moveInPlace;
            }
        }*/

        #endregion //SCRIPT_ACCESS_PLAY_AREA

        // LeftController scripts
        #region SCRIPT_ACCESS_LEFT_CONTROLLER

        /*private VRTK.VRTK_Pointer pointerLeft;
        public VRTK.VRTK_Pointer PointerLeft
        {
            get
            {
                return pointerLeft;
            }
        }*/

        /*private VRTK.VRTK_UIPointer uiPointerLeft;
        public VRTK.VRTK_UIPointer UIPointerLeft
        {
            get
            {
                return uiPointerLeft;
            }
        }*/

        #endregion //SCRIPT_ACCESS_LEFT_CONTROLLER

        // RightController scripts
        #region SCRIPT_ACCESS_RIGHT_CONTROLLER

        /*private VRTK.VRTK_Pointer pointerRight;
        public VRTK.VRTK_Pointer PointerRight
        {
            get
            {
                return pointerRight;
            }
        }*/

        /*private VRTK.VRTK_UIPointer uiIPointerRight;
        public VRTK.VRTK_UIPointer UIPointerRight
        {
            get
            {
                return uiIPointerRight;
            }
        }*/

        #endregion //SCRIPT_ACCESS_RIGHT_CONTROLLER

        #endregion //SCRIPT_ACCESS

        /**
         * Called by Unity as part of the MonoBehavior instantiation process.
         * Guaranteed to be called only once and before any Start methods are called.
         */
        void Awake()
        {
            instance = this;

            // Initialize the session objects
            InitializeSessionObjects();
        }

        /**
         * Called by Unity as part of the MonoBehavior instantiation process.
         * Guaranteed to be called after the Awake methods are called and before the first
         * frame update.
         */
        void Start()
        {
        }

        #region INITIALIZATION_METHODS

        /**
         * Initializes the session objects for public access
         */
        protected virtual void InitializeSessionObjects()
        {
            // Initialize the configuration objects
            InitializeConfigurationManager();
            InitializeSessionConfiguration();
            InitializeCollaborationManager();

            // Initialize the Modules
            InitializeDataManager();
            InitializeTimeManager();

            // Initialize the GameObjects
            InitializeLoadedProject();
            InitializePlayArea();
            InitializeHeadsetFollower();
            InitializeControllers();
        }

        /**
         * Initializes the ConfigurationManager for public access
         */
        protected virtual void InitializeConfigurationManager()
        {
            // Obtain a reference to the ConfigurationManager if not assigned
            if (configurationManager == null)
            {
                configurationManager = GetComponentInChildren<ConfigurationManager>();
            }
        }

        /**
         * Initializes the SessionConfiguration for public access
         */
        protected virtual void InitializeSessionConfiguration()
        {
            // Obtain a reference to the SessionConfiguration
            if (sessionConfiguration == null)
            {
                sessionConfiguration = GetComponentInChildren<SessionConfiguration>();
            }
        }

        /**
         * Initializes the CollaborationManager for public access
         */
        protected virtual void InitializeCollaborationManager()
        {
            // Obtain a reference to the CollaborationManager
            if (collaborationManager == null)
            {
                // We need to locate the correct CollaborationManager, so we will get the correct named game object
                // and extract the component.
                // NOTE: We cannot reference CollaborationManager.instance variable here because the static
                // initialization may not have occurred yet. Finds are valid.
                GameObject collaborationManagerObject = GameObject.Find(SESSION_OBJECT_NAME_COLLABORATION_MANAGER);
                if (collaborationManagerObject != null)
                {
                    collaborationManager = collaborationManagerObject.GetComponentInChildren<CollaborationManagerDeprecated>();
                }
            }

            // Check for a valid DataManager reference
            if (collaborationManager == null)
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_COLLABORATION_MANAGER);
            }
        }

        /**
         * Initializes the DataManager for public access
         */
        protected virtual void InitializeDataManager()
        {
            // Obtain a reference to the DataManager if not assigned
            if (dataManager == null)
            {
                // We need to locate the correct DataManager, so we will get the correct named game object
                // and extract the component
                GameObject dataManagerObject = GameObject.Find(SESSION_OBJECT_NAME_DATA_MANAGER);
                if (dataManagerObject != null)
                {
                    dataManager = dataManagerObject.GetComponentInChildren<DataManager>();
                }
            }

            // Check for a valid DataManager reference
            if (dataManager == null)
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_DATA_MANAGER);
            }
        }

        /**
         * Initializes the TimeManager for public access
         */
        protected virtual void InitializeTimeManager()
        {
            // Obtain a reference to the TimeManager if not assigned
            if (timeManager == null)
            {
                // We need to locate the correct TimeManager, so we will get the correct named game object and extract the component
                GameObject timeManagerObject = GameObject.Find(SESSION_OBJECT_NAME_TIME_MANAGER);
                if (timeManagerObject != null)
                {
                    timeManager = timeManagerObject.GetComponentInChildren<TimeManager>();
                }
            }

            // Check for a valid TimeManager reference
            if (timeManager == null)
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_TIME_MANAGER);
            }
        }

        /**
         * Initializes the LoadedProject for public access
         */
        protected virtual void InitializeLoadedProject()
        {
            // Obtain a reference to the LoadedProject
            if (loadedProject == null)
            {
                loadedProject = GameObject.Find(SESSION_OBJECT_NAME_LOADED_PROJECT);
            }

            // Initialize the LoadedProject component references
            if (loadedProject != null)
            {
                // Obtain a reference to the LoadedProject scripts
                unityProject = loadedProject.GetComponentInChildren<UnityProjectDeprecated>();
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_LOADED_PROJECT);
            }

            // Check for a valid UnityProject reference
            if (unityProject == null)
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining the UnityProject reference");
            }
        }

        /**
         * Initializes the PlayArea for public access
         */
        protected virtual void InitializePlayArea()
        {
            // Obtain a reference to the PlayArea
            if (playArea == null)
            {
                playArea = GameObject.Find(SESSION_OBJECT_NAME_PLAY_AREA);
            }

            // Initialize the PlayArea component references
            if (playArea != null)
            {
                // Obtain a reference to the PlayArea scripts
                //basicTeleport = playArea.GetComponentInChildren<VRTK.VRTK_BasicTeleport>();
                //bodyPhysics = playArea.GetComponentInChildren<VRTK.VRTK_BodyPhysics>();
                //moveInPlace = playArea.GetComponentInChildren<VRTK.VRTK_MoveInPlace>();
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_PLAY_AREA);
            }
        }

        /**
         * Initializes the HeadsetFollower for public access
         */
        protected virtual void InitializeHeadsetFollower()
        {
            // Obtain a reference to the HeadsetFollower
            if (headsetFollower == null)
            {
                headsetFollower = GameObject.Find(SESSION_OBJECT_NAME_HEADSET_FOLLOWER);
            }

            // Initialize the HeadsetFollower component references
            if (headsetFollower != null)
            {
                // Obtain a reference to the HeadsetFollower scripts
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_HEADSET_FOLLOWER);
            }
        }

        /**
         * Initializes the Controllers for public access
         */
        protected virtual void InitializeControllers()
        {
            // Obtain a reference to the LeftController
            if (leftController == null)
            {
                leftController = GameObject.Find(SESSION_OBJECT_NAME_LEFT_CONTROLLER);
            }

            // Initialize the LeftController component references
            if (leftController != null)
            {
                // Obtain a reference to the LeftController scripts
                //pointerLeft = leftController.GetComponentInChildren<VRTK.VRTK_Pointer>();
                //uiPointerLeft = leftController.GetComponentInChildren<VRTK.VRTK_UIPointer>();
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_LEFT_CONTROLLER);
            }

            // Obtain a reference to the RightController
            if (rightController == null)
            {
                rightController = GameObject.Find(SESSION_OBJECT_NAME_RIGHT_CONTROLLER);
            }

            // Initialize the RightController component references
            if (rightController != null)
            {
                // Obtain a reference to the RightController scripts
                //pointerRight = rightController.GetComponentInChildren<VRTK.VRTK_Pointer>();
                //uiIPointerRight = rightController.GetComponentInChildren<VRTK.VRTK_UIPointer>();
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: A problem occurred obtaining a session object reference: " +
                    SESSION_OBJECT_NAME_RIGHT_CONTROLLER);
            }
        }

        #endregion // INITIALIZATION_METHODS
    }
}