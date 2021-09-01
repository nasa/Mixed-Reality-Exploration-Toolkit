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
    /// <summary>
    /// i would love to use "Project Settings > XR Plugin Management > Oculus > Stereo Rendering Mode: Single Pass Instantiated"
    /// but no: because in that case the text would only be rendered for the left eye, as there are no shaders for fonts that would work correctly, if using the textMessPro.    
    /// 20200626
    /// 
    /// thats still an issue as of 20200801. 
    /// even worse - the last version thats able to build with multipass is 2020.1.0b16, as in the release, "SPI" wont render anything to the right eye.
    /// 
    /// </summary>
#if MRET_2021_OR_LATER
    public class VDE : GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject.SceneObject
#else
    public class VDE : MonoBehaviour
#endif
    {
#if DUH
        public Valve.VR.SteamVR_ActionSet activateActionSetOnAttach = Valve.VR.SteamVR_Input.GetActionSet("/actions/VDE", false);
#endif

        /// <summary>
        /// if this is checked in the gameobject, VDE will not try to connect to the VDE server, but rather load conf AND demo dataset using Resources.LoadAsync
        /// </summary>
        public bool standalone;
        public string serverURL = "https://vde.coda.ee/VDE";
        public string nameOfBakedConfigResource = "config";
        public string nameOfBakedEntitiesResource = "entities";
        public string nameOfBakedLinksResource = "links";

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

        public Material[] node, group, edge;

        public enum InputSource
        {
            QuestHands,
            RiftPseudoHands,
            MagicalLeapingHands,
            SteamingPseudoHands,
            MRET
        }

        internal Data data;
        internal Controller controller;
        internal System.Random rando = new System.Random();
        internal Connection connection;

        private Log log = new Log("VDE");
#pragma warning disable IDE0052 // Remove unread private members
        private bool showingStartupMessage;
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning disable CS0414
        private bool awakened, started;
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
            "middleFingerTip"
        };
        private Dictionary<string, int> layers = new Dictionary<string, int> {
            { "grabbable", 14 },
            { "gazable", 10 },
            { "blade", 11 },
            { "suusapoks", 12 },
            { "pointable", 13 }
        };
        private Vector3 localScaleInPreviousFrame;


        private void OnApplicationQuit()
        {
            data.forrestIsRunning = false;
        }
#if MRET_2021_OR_LATER
        public void Init(
            bool standalone = true,
            bool renderInCloud = false,
            string serverURL = "https://vde.coda.ee/VDE",
            string nameOfBakedConfigResource = "config",
            string nameOfBakedEntitiesResource = "entities",
            string nameOfBakedLinksResource = "links"            
            )
        {
            this.standalone = standalone;
            this.renderInCloud = renderInCloud;
            this.serverURL = serverURL;
            this.nameOfBakedConfigResource = nameOfBakedConfigResource;
            this.nameOfBakedEntitiesResource = nameOfBakedEntitiesResource;
            this.nameOfBakedLinksResource = nameOfBakedLinksResource;
        }
#endif
        private void Awake()
        {
#if UNITY_2019_4_17
            Init(false, false, "https://a.whitest.house/VDE");
#endif
#if MRET_2021_OR_LATER
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
#endif

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

            GetParameters();
            Tools.CreateTagsAndLayers(tags,layers);
            localScaleInPreviousFrame = transform.lossyScale;

            awakened = true;
            log.Entry("VDE has Awakend!");
        }
        /// <summary>
        /// check if camera, hud etc are attached to the VDE gameobject. if not, "do something about it".
        /// </summary>
        private void CheckAssignments()
        {
            // this would fail miserably. even if usableCamera IS indeed null, this comparison still thinks, its not. hence the "try" hack below.
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
                        if(!VDEHUD.TryGetComponent(out hud))
                        {
                            log.Entry("Unable to create VDE.HUD, because instantiated VDEHUD doesnt contain the Hud script.. ?");
                        }
                        else if (!usableCamera.GetComponentInChildren<HUD>())
                        {
                            VDEHUD.transform.SetParent(usableCamera.transform.parent);
                            hud.vde = this;
                            VDEHUD.gameObject.SetActive(true);
                            hud.SetBoardTextTo("VDE is loading..");
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
        }
        private void GetParameters()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 2 && (arguments[1] == "--server" || arguments[1] == "-server" || arguments[1] == "-s"))
            {
                serverURL = arguments[2];
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
            log.Entry("setPositionAndScale: changing scale from " + transform.localScale.ToString() + " to " + defaultScale.ToString());
            transform.localScale = defaultScale;
            transform.localPosition = dashboardCenter;
            log.Entry("setPositionAndScale: scale now " + transform.localScale.ToString());
        }

        private void Update()
        {
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
            // need to get rid of the default InteractablePart collider and rigidbox.
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
            if (!started && awakened && (transform.parent is null || transform.parent.name == expectedNameOfParent))
            {
                if (standalone || backendWithBakedData)
                {
                    data.LoadLocalConfigAndData();
                    if (backendWithBakedData)
                    {
                        connection = new Connection(data);
                    }
                }
                else
                {
                    connection = new Connection(data);
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
                data.LoadLocalConfigAndData();
                if (backendWithBakedData)
                {
                    connection = new Connection(data);
                }
            }
            else
            {
                connection = new Connection(data);
            }
        }
#endif
        private void OnDestroy()
        {
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

        internal void Quit()
        {
            data.forrestIsRunning = false;
            if (!(connection is null))
            {
                connection.Disconnect();
            }
#if UNITY_STANDALONE
            Application.Quit(0);
#endif
#if UNITY_EDITOR
            //UnityEditor.EditorApplication.Exit(0);
#endif
        }
    }
}