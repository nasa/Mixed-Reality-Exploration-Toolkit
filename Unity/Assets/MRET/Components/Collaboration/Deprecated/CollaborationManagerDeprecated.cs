// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRC;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Collaboration.CollaborationManager))]
    public class CollaborationManagerDeprecated : MonoBehaviour
    {
        public static CollaborationManagerDeprecated instance;

        public enum EngineType { XRC, LegacyGMSEC };

        [Tooltip("The middleware type to use.")]
        public GMSECDefs.ConnectionTypes connectionType = GMSECDefs.ConnectionTypes.bolt;

        [Tooltip("The GMSEC server address and port number.")]
        public string server = "localhost:9100";

        [Tooltip("The mission name to use.")]
        public string missionName = "UNSET";

        [Tooltip("The satellite name to use.")]
        public string satName = "UNSET";

        public SynchronizationManagerDeprecated synchManager;
        public SessionMasterNode masterNode;
        public SessionSlaveNode slaveNode;
        public UnityProjectDeprecated projectManager;
        public ModeNavigator modeNavigator;
        public EngineType engineType = EngineType.XRC;
        public MonoGMSEC gmsec;
        public XRCManagerDeprecated xrcManager;
        public GameObject vrAvatarPrefab, desktopAvatarPrefab;
        public GameObject userContainer;

        private SessionAdvertiser sessionAdvertiser;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (gmsec == null)
            {
                gmsec = FindObjectOfType<MonoGMSEC>();
            }

            if (xrcManager == null)
            {
                xrcManager = FindObjectOfType<XRCManagerDeprecated>();
            }
        }

        public void EnterMasterMode(SessionInformation sessionInfo, string alias)
        {
            if (engineType == EngineType.LegacyGMSEC)
            {
                synchManager.enabled = true;
                slaveNode.enabled = false;
                masterNode.enabled = true;

                masterNode.StartRunning(sessionInfo, alias);

                sessionAdvertiser = gameObject.AddComponent<SessionAdvertiser>();
                synchManager.sessionAdvertiser = sessionAdvertiser;
                synchManager.masterNode = masterNode;
                sessionAdvertiser.sessionInformation = sessionInfo;
                sessionAdvertiser.connectionType = connectionType;
                sessionAdvertiser.server = server;
                sessionAdvertiser.missionName = missionName;
                sessionAdvertiser.satName = satName;
                sessionAdvertiser.Initialize();

                MRET.ProjectDeprecated.userAlias = alias;
                modeNavigator.OpenProject(sessionInfo.projectName, true);
            }
        }

        public void EnterSlaveMode(SessionInformation sessionInfo, string alias)
        {
            if (engineType == EngineType.LegacyGMSEC)
            {
                Destroy(sessionAdvertiser);

                synchManager.enabled = true;
                masterNode.enabled = false;
                slaveNode.enabled = true;

                slaveNode.Connect(sessionInfo, alias);

                MRET.ProjectDeprecated.userAlias = alias;
                modeNavigator.OpenProject(Application.dataPath + "/Projects/" + sessionInfo.projectName, true);
            }
        }
    }
}