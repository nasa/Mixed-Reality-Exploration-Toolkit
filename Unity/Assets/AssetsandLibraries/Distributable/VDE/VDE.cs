/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */

using Assets.VDE.Communication;
using Assets.VDE.UI;
using Assets.VDE.UI.Hands;
using Assets.VDE.UI.HUD;
using Assets.VDE.UI.Input;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.VDE
{
#if MRET_2021_OR_LATER
    public class VDE : GOV.NASA.GSFC.XR.MRET.MRETUpdateBehaviour
#else
    public class VDE : MonoBehaviour
#endif
    {
#if DO_STEAM
        public Valve.VR.SteamVR_ActionSet activateActionSetOnAttach = Valve.VR.SteamVR_Input.GetActionSet("/actions/VDE", false);
#endif

        /// <summary>
        /// if this is checked in the gameobject, VDE will not try to connect to the VDE server, but rather load conf AND demo dataset using Resources.LoadAsync
        /// </summary>
        public bool standalone;
        [Tooltip("The URI for VDES (https://vde.coda.ee/VDE) or address and port number for GMSEC (localhost:9100)")]
        public string serverAddress = "https://vde.coda.ee/VDE";
        public ConnectionType connectionType = ConnectionType.GMSEC;
        public string nameOfBakedConfigResource = "config";
        public string nameOfBakedEntitiesResource = "entities";
        public string nameOfBakedLinksResource = "links";
        public string nameOfBakedPositionsResource = "positions";

#if !MRET_2021_OR_LATER
        public bool renderInCloud = true;
#else
        public bool renderInCloud = false;
#endif
        /// <summary>
        /// used only for crapturing demo dataset from server
        /// </summary>
        public bool cacheResponseFromServer;
        public bool debug;
        /// <summary>
        /// if this is checked in the gameobject, VDE will connect to the VDE server, but load conf AND demo dataset using Resources.LoadAsync
        /// </summary>
        public bool backendWithBakedData;
        public GameObject VDEHUD;
        public GameObject VDEHandText;
        internal HUD hud;
        public Font font;
        public Lazer lazer;
        public Camera usableCamera;
        public Hand.Which primaryHand;
        public InputSource inputSource;

        public Material visibleMaterial;
        public PhysicMaterial physicMaterial;

        public TextMeshPro startupMessage;
        public GameObject startupScreen;

        public GameObject edgeTemplate;
        public GameObject nodeShapeTemplate;
        public GameObject groupShapeTemplate;
#if DOTNETWINRT_PRESENT
        public GameObject RightHand;
#endif
        public Material[] node, group, edge;

        public enum InputSource
        {
            QuestHands,
            RiftPseudoHands,
            MagicalLeapingHands,
            SteamingPseudoHands,
            MRET,
            HoloLens
        }
        public enum ConnectionType
        {
            SIGNALR,
            GMSEC
        }

        internal Data data;
        internal Controller controller;
        internal System.Random rando = new System.Random();
        internal SignalRConnection signalRConnection;
#if MRET_2021_OR_LATER
        internal GMSECConnection gmsecConnection;
#endif
        private Log log = new Log("VDE");
#pragma warning disable IDE0052 // Remove unread private members
        private bool showingStartupMessage;
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning disable CS0414
        private bool awakened, initialized, started;
        private string expectedNameOfParent = "Parts";// "GameObjects";
        private string tryingToStartUnder = "";
#pragma warning restore CS0414
        private List<string> Lazarevid = new List<string> { "RightHand Controller", "LeftHand Controller" };
        private List<string> tags = new List<string> { 
            "edge",
            "node",
            "nodeGroup",
            "blade",
            "bladeText",
            "gazable",
            "indexFingerTip",
            "middleFingerTip",
            "wrist"
        };
        private Dictionary<string, int> layers = new Dictionary<string, int> {
            { "grabbable", 14 },
            { "gazable", 10 },
            { "blade", 11 },
            { "suusapoks", 12 },
            { "pointable", 13 }
        };
        internal Dictionary<string, shaderKeywordType> shaderKeywordsToEnable = new Dictionary<string, shaderKeywordType> {
            { "_EmissiveExposureWeight", shaderKeywordType.floater },
            { "_Color", shaderKeywordType.colour },
            { "Color", shaderKeywordType.colour },
            { "_MainColor", shaderKeywordType.colour },
            { "MainColor", shaderKeywordType.colour },
            { "_UnlitColor", shaderKeywordType.colour },
            { "UnlitColor", shaderKeywordType.colour },
            { "_TintColor", shaderKeywordType.colour },
            { "TintColor", shaderKeywordType.colour },
            { "_Albedo", shaderKeywordType.colour },
            { "Albedo", shaderKeywordType.colour },
            { "_BaseColor", shaderKeywordType.colour },
            { "BaseColor", shaderKeywordType.colour },
            { "_EmissiveColor", shaderKeywordType.colour },
            { "EmissiveColor", shaderKeywordType.colour },
            { "_EmissionColor", shaderKeywordType.colour },
        };
        internal enum shaderKeywordType
        {
            colour,
            floater
        }
        internal Vector3 localScaleInPreviousFrame;

        private void OnApplicationQuit()
        {
            data.forrestIsRunning = false;
        }

#if MRET_2021_OR_LATER
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(VDE);

        public void Init(
            bool standalone = false,
            bool renderInCloud = false,
            string serverAddress = "https://vde.coda.ee/VDE",
            string nameOfBakedConfigResource = "config",
            string nameOfBakedEntitiesResource = "entities",
            string nameOfBakedLinksResource = "links",
            InputSource inputSource = InputSource.MRET,
            ConnectionType connectionType = ConnectionType.GMSEC
            )
        {
            this.standalone = standalone;
            this.renderInCloud = renderInCloud;
            this.serverAddress = serverAddress;
            this.nameOfBakedConfigResource = nameOfBakedConfigResource;
            this.nameOfBakedEntitiesResource = nameOfBakedEntitiesResource;
            this.nameOfBakedLinksResource = nameOfBakedLinksResource;
            this.inputSource = InputSource.MRET;
            this.connectionType = connectionType;
            initialized = true;
        }
#endif

#if MRET_2021_OR_LATER
        protected override void MRETAwake()
        {
            //UnityEditor.PlayerSettings.SetManagedStrippingLevel(UnityEditor.BuildTargetGroup.Lumin, UnityEditor.ManagedStrippingLevel.Disabled);
            base.MRETAwake();
#else
        private void Awake()
        {
#endif
            GameObject deplorable = GameObject.Find("Directional Light From Below");
            if (!(deplorable is null))
            {
                deplorable.SetActive(false);
            }
            inputSource = InputSource.MRET;
            if (!(transform.parent is null) && transform.parent.name != expectedNameOfParent)
            {
                log.Entry("VDE tried to awaken under " + transform.parent.name + ", " + (standalone ? "" : " not ") + "in standalone mode. But that is different parent than the expected one: " + expectedNameOfParent + ". Hence VDE stops here.");
                return;
            }
            log.Entry("VDE is Awakening" + (standalone ? " " : " not ") + "in standalone mode under: " + ((transform.parent is null) ? "null" : transform.parent.name));

            // to avoid collisions with existing objects while the datashape adjusts, send it to the "cloud"
            if (renderInCloud)
            {
                SetPositionAndScale(transform.localPosition + new Vector3(0, 33, 0), transform.localScale);
            }

            initialized = true;

#if STEAM_VR
            activateActionSetOnAttach.Activate();
#endif
            CheckAssignments();
            CheckTemplates();
            data = new Data(this);
            GameObject linksGameObject = new GameObject("Links");
            linksGameObject.transform.SetParent(transform);

            data.SetLinksContainer(linksGameObject);
            data.SetMessenger(gameObject.AddComponent<Messenger>());
            data.UI.SetNF(gameObject.AddComponent<Factory>());
            data.UI.SetFPS(gameObject.AddComponent<FramesPerSecond>());

            controller = gameObject.AddComponent<Controller>(); 
            controller.Init(this);

#if !MRET_2021_OR_LATER
            GetParameters();
#endif
            Tools.CreateTagsAndLayers(tags,layers);
            localScaleInPreviousFrame = transform.lossyScale;
#if DOTNETWINRT_PRESENT
            //startupMessage.text = "Virtual Data Explorer\nYou have a choice now: either enjoy the baked in demo dataset or connect to a VDE Server that’s serving your data.\nWould you choose to connect to a server, press the Connect button.Otherwise: Load Demo.";
            showingStartupMessage = true;
#endif
            awakened = true;
            DontDestroyOnLoad(this.gameObject);
            log.Entry("VDE has Awakend!");
        }
        /// <summary>
        /// check if camera, hud etc are attached to the VDE gameobject. if not, "do something about it".
        /// </summary>
        private void CheckAssignments()
        {
            // that IF would fail miserably: even if usableCamera IS indeed null, this comparison still thinks, its not. hence the "try" hack below.
            //if (usableCamera is null)
            try
            {
                float whatever = usableCamera.transform.position.x;
            }
            catch (Exception exe)
            {
                log.Entry("UsableCamera is not explicitly specified in VDE GameObject: " + exe.Message);
                try
                {
                    usableCamera = UnityEngine.Camera.main;
                }
                catch (Exception exes)
                {
                    log.Entry("Unable to get UnityEngine.Camera.main for UsableCamera: " + exes.Message);
                }
            }

            if (hud is null)
            {
                try
                {
                    if (!(usableCamera is null))
                    {
                        if (!(VDEHUD is null))
                        {
                            if (!VDEHUD.TryGetComponent(out hud))
                            {
                                log.Entry("Unable to create VDE.HUD, because instantiated VDEHUD doesnt contain the Hud script.. ?");
                            }
                            else if (!usableCamera.GetComponentInChildren<HUD>())
                            {
                                VDEHUD.transform.SetParent(usableCamera.transform.parent);
                                hud.vde = this;
                                VDEHUD.SetActive(true);
#if !DOTNETWINRT_PRESENT
                                hud.SetBoardTextTo("VDE is loading..");
#endif
                                data.burnOnDestruction.Add(VDEHUD);
                                log.Entry("Set VDEHUD parent to: " + usableCamera.name);
                            }
                            else
                            {
                                log.Entry(usableCamera.name + " already seems to have VDEHUD.. ?");
                            }
                        }
                        else
                        {
                            log.Entry("VDEHUD is undefined.");
                        }
                    } 
                    else
                    {
                        log.Entry("Unable to create VDE.HUD, because usableCamera is null, void, absent, etc.");
                    }
                }
                catch (Exception exe)
                {
                    log.Entry("Unable to find VDE.HUD, because: " + exe.Message);
                }
            }
            if (lazer is null)
            {
                try
                {
                    foreach (string potentialLazarev in Lazarevid)
                    {
                        GameObject controller = GameObject.Find(potentialLazarev);
                        try
                        {
                            float whatever = controller.transform.position.x;
                            lazer = controller.AddComponent<Lazer>();
                            break;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception exe)
                {
                    log.Entry("Unable to sharpen The Lazer: " + exe.Message);
                }
            }
        }

        /// <summary>
        /// check if the templates are attached to the VDE gameobject. if not, create some.
        /// </summary>
        private void CheckTemplates()
        {
            try
            {
                GameObject test = Instantiate(nodeShapeTemplate);
                test.name = "test";
                Destroy(test);
            }
            catch (UnassignedReferenceException)
            {
                nodeShapeTemplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nodeShapeTemplate.name = "VDE shape";
                Rigidbody rigid = nodeShapeTemplate.AddComponent<Rigidbody>();
                BoxCollider collie = nodeShapeTemplate.GetComponent<BoxCollider>();
                MeshRenderer renderer = nodeShapeTemplate.GetComponent<MeshRenderer>();

                renderer.material = visibleMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

                collie.material = physicMaterial;
                collie.isTrigger = true;
                collie.enabled = false;

                rigid.useGravity = false;
                rigid.isKinematic = true;
                rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;

                nodeShapeTemplate.SetActive(false);
            }

            try
            {
                GameObject test = Instantiate(edgeTemplate);
                test.name = "test";
                Destroy(test);
            }
            catch (UnassignedReferenceException)
            {
                edgeTemplate = new GameObject("VDE edge");
                edgeTemplate.AddComponent<Link>();

                edgeTemplate.SetActive(false);
            }


            Renderer nodeShapeTemplateRenderer = nodeShapeTemplate.GetComponent<Renderer>();
            Renderer edgeTemplateRenderer = edgeTemplate.GetComponent<Renderer>();
            foreach (KeyValuePair<string, shaderKeywordType> wordkey in shaderKeywordsToEnable)
            {
                nodeShapeTemplateRenderer.material.EnableKeyword(wordkey.Key);
                edgeTemplateRenderer.material.EnableKeyword(wordkey.Key);
            }
        }
        private void GetParameters()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 2 && (arguments[1] == "--server" || arguments[1] == "-server" || arguments[1] == "-s"))
            {
                serverAddress = arguments[2];
            }
            if (arguments.Length > 1 && (arguments[1] == "--local" || arguments[1] == "-local" || arguments[1] == "-l"))
            {
                standalone = true;
            }
            if (arguments.Length > 1 && arguments[1] == "--help")
            {
                startupMessage.text = "To run with the baked-in demo dataset, use the --local parameter.\n\nTo connect to a VDE server, specify the VDE URL with the --server parameter.\nFor example:\n\n\tvde.exe --server https://vde.coda.ee/VDE \n\nTo escape from here, try.. the ESC key for example.";
                showingStartupMessage = true;
            }
        }

        internal void SetPositionAndScale(Vector3 dashboardCenter, Vector3 defaultScale)
        {
            log.Entry("setPositionAndScale: changing scale from " + (transform.localScale * 100).ToString() + " to " + (defaultScale * 100).ToString());
            transform.localScale = defaultScale;
            transform.localPosition = dashboardCenter;
            log.Entry("setPositionAndScale: scale now " + (transform.localScale * 100).ToString());
        }

#if MRET_2021_OR_LATER
        protected override void MRETUpdate()
        {
            base.MRETUpdate();
#else
        private void Update()
        {
#endif
            if (transform.lossyScale != localScaleInPreviousFrame)
            {
                log.Entry("scale changed from " + localScaleInPreviousFrame.ToString() + " to " + transform.localScale.ToString());

                // whether there is any of those components initialized already or not, we'll try to notify 'em.
                try { if (data.layouts.current.ready) data.layouts.current.AdjustJointsToScale(transform.lossyScale.x); } catch (Exception) { }

                localScaleInPreviousFrame = transform.lossyScale;
            }
#if MRET_2021_OR_LATER
            if (!(transform.parent is null) && transform.parent.name != expectedNameOfParent)
            {
                log.Entry("VDE tried to awaken under " + transform.parent.name + ", " + (standalone ? "" : " not ") + "in standalone mode. But that is different parent than the expected one: " + expectedNameOfParent + ". Hence VDE stops here.");// + activateActionSetOnAttach.fullPath.ToString());
                return;
            }
            // need to get rid of the default InteractableSceneObject collider and rigidbox.
            if (gameObject.TryGetComponent(out BoxCollider poks) && poks.enabled)
            {
                poks.enabled = false;
                poks.isTrigger = true;
            }
            if (gameObject.TryGetComponent(out Rigidbody rigiid) && rigiid.useGravity)
            {
                rigiid.useGravity = false;
                rigiid.mass = 0;
                rigiid.isKinematic = false;
            }
            if (!started && awakened && initialized && (transform.parent is null || transform.parent.name == expectedNameOfParent))
            {
                if (standalone || backendWithBakedData)
                {
                    log.Entry("Loading baked in config (" + standalone + "/" + backendWithBakedData + ")");
                    data.LoadLocalConfigAndData();
                    if (backendWithBakedData)
                    {
                        Connect();
                    }
                }
                else
                {
                    Connect();
                }
                transform.rotation = Quaternion.LookRotation(Vector3.zero, Vector3.up);
                started = true;
                if (transform.parent is null)
                {
                    Debug.LogWarning("Parentless VDE has started.");
                }
                else
                {
                    log.Entry("VDE has started under: " + transform.parent.name);
                }
            }
            else if(!started)
            {
                if (transform.parent is null)
                {
                    Debug.LogWarning("Parentless VDE has NOT yet started.");
                }
                else if (tryingToStartUnder != transform.parent.name)
                {
                    log.Entry("VDE is trying to start under: " + transform.parent.name);
                    tryingToStartUnder = transform.parent.name;
                }
            }
#endif
        }

#if !MRET_2021_OR_LATER
        void Start()
        {
            if (showingStartupMessage)
            {
                // just.. sit there.
            }
            else if (standalone || backendWithBakedData)
            {
                LoadLocalConfigAndDemoData();
                if (backendWithBakedData)
                {
                    Connect();
                }
            }
            else
            {
                Connect();
            }
        }
#endif

        public void LoadLocalConfigAndDemoData()
        {
            data.LoadLocalConfigAndData();
        }
        public void LoadLocalLargeConfigAndDemoData()
        {
            nameOfBakedEntitiesResource = "entities.default";
            nameOfBakedLinksResource = "links.default";
            data.LoadLocalConfigAndData();
        }
        public void Connect()
        {
            switch (connectionType)
            {
                case ConnectionType.SIGNALR:
                    signalRConnection = new SignalRConnection(data);
                    break;
                case ConnectionType.GMSEC:
#if MRET_2021_OR_LATER
                    gmsecConnection = new GMSECConnection(data);
#endif
                    break;
                default:
                    signalRConnection = new SignalRConnection(data);
                    break;
            }
        }

#if MRET_2021_OR_LATER
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();
#else
        private void OnDestroy()
        {
#endif
            data.forrestIsRunning = false;
            data.DestroyDestroyables();
            CheckForrest();
        }

        internal void CheckForrest()
        {
            if (!data.forrestIsRunning)
            {
                Quit();
            }
        }

        public void Quit()
        {
            data.forrestIsRunning = false;
            if (!(signalRConnection is null))
            {
                signalRConnection.Disconnect();
            }
            // if left without standalone ifdef, this WILL destroy (parts of) the unity project.
#if !UNITY_EDITOR
            Application.Quit(0);
            Application.Quit();
            Application.Unload();
            Environment.Exit(1);

            try
            {
	            foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName("Virtual Data Explorer.exe"))
                {
                    data.messenger.Post(new Message()
                    {
                        HUDEvent = HUD.Event.SetText,
                        message = "killing: " + proc.Id
                    });
                    proc.Kill();
                }
                foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcesses())
                {
                    data.messenger.Post(new Message()
                    {
                        HUDEvent = HUD.Event.SetText,
                        message = "found: " + proc.ProcessName + "(" + proc.Id + "); "
                    });
                }
            }
            catch(Exception ex) 
            {
                data.messenger.Post(new Message()
                {
                    HUDEvent = HUD.Event.SetText,
                    message = "Theres no escape from here!"
                });
            }

#if UNITY_EDITOR
            //UnityEditor.EditorApplication.Exit(0);
#endif
#endif
        }
    }
}