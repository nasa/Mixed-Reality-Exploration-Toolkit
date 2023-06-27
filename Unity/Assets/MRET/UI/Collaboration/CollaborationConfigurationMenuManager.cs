// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC.Collaboration;

namespace GOV.NASA.GSFC.XR.MRET.UI.Collaboration
{
    public class CollaborationConfigurationMenuManager : MonoBehaviour
    {
        public Dropdown collaborationEngine;
        public VR_InputField serverAddressInput;
        public Dropdown connectionTypeDropdown;

        private CollaborationManager collabManager;
        private SynchronizationManagerDeprecated syncManager;
        private SessionSlaveNode slaveNode;
        private SessionMasterNode masterNode;

        void Start()
        {
            collabManager = FindObjectOfType<CollaborationManager>();
            syncManager = FindObjectOfType<SynchronizationManagerDeprecated>();
            slaveNode = FindObjectOfType<SessionSlaveNode>();
            masterNode = FindObjectOfType<SessionMasterNode>();

            serverAddressInput.text = collabManager.server;
        }

        public void UpdateEngine()
        {
            if (collaborationEngine.value == 0)
            {
                collabManager.engineType = CollaborationManager.EngineType.XRC;
            }
            else
            {
                collabManager.engineType = CollaborationManager.EngineType.LegacyGMSEC;
            }
        }

        public void UpdateServer()
        {
            collabManager.server = serverAddressInput.text;
            syncManager.server = serverAddressInput.text;
            slaveNode.server = serverAddressInput.text;
            masterNode.server = serverAddressInput.text;
        }

        public void UpdateConnectionType()
        {
            switch (connectionTypeDropdown.value)
            {
                case 0:
                    collabManager.connectionType = GMSECDefs.ConnectionTypes.bolt;
                    syncManager.connectionType = SynchronizationManagerDeprecated.ConnectionTypes.bolt;
                    slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.bolt;
                    masterNode.connectionType = SessionMasterNode.ConnectionTypes.bolt;
                    break;

                case 1:
                    collabManager.connectionType = GMSECDefs.ConnectionTypes.mb;
                    syncManager.connectionType = SynchronizationManagerDeprecated.ConnectionTypes.mb;
                    slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.mb;
                    masterNode.connectionType = SessionMasterNode.ConnectionTypes.mb;
                    break;

                case 2:
                    collabManager.connectionType = GMSECDefs.ConnectionTypes.amq383;
                    syncManager.connectionType = SynchronizationManagerDeprecated.ConnectionTypes.amq383;
                    slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.amq383;
                    masterNode.connectionType = SessionMasterNode.ConnectionTypes.amq383;
                    break;

                case 3:
                    collabManager.connectionType = GMSECDefs.ConnectionTypes.amq384;
                    syncManager.connectionType = SynchronizationManagerDeprecated.ConnectionTypes.amq384;
                    slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.amq384;
                    masterNode.connectionType = SessionMasterNode.ConnectionTypes.amq384;
                    break;

                default:
                    collabManager.connectionType = GMSECDefs.ConnectionTypes.bolt;
                    syncManager.connectionType = SynchronizationManagerDeprecated.ConnectionTypes.bolt;
                    slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.bolt;
                    masterNode.connectionType = SessionMasterNode.ConnectionTypes.bolt;
                    break;
            }
        }
    }
}