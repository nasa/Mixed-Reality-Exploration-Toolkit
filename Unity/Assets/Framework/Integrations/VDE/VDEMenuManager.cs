/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */

#if MRET_2021_OR_LATER
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Components.UI;

public class VDEMenuManager : MonoBehaviour
{
    public Dropdown collaborationEngine;
    public VR_InputField serverAddressInput;
    public Dropdown connectionTypeDropdown;

    public string VDEServerURI;
    public bool standalone;
    public int standaloneDataset = 0;

    private CollaborationManager collabManager;
    private SynchronizationManager syncManager;
    private SessionSlaveNode slaveNode;
    private SessionMasterNode masterNode;

	void Start ()
    {
        collabManager = FindObjectOfType<CollaborationManager>();
        syncManager = FindObjectOfType<SynchronizationManager>();
        slaveNode = FindObjectOfType<SessionSlaveNode>();
        masterNode = FindObjectOfType<SessionMasterNode>();

        serverAddressInput.text = "https://vde.coda.ee/VDE";
	}

    public void UpdateStandaloneDataset()
    {
        standaloneDataset = collaborationEngine.value;
        /*
        if (collaborationEngine.value == 0)
        {
            collabManager.engineType = CollaborationManager.EngineType.XRC;
        }
        else
        {
            collabManager.engineType = CollaborationManager.EngineType.LegacyGMSEC;
        }
        */
    }

    public void UpdateServer()
    {
        VDEServerURI = serverAddressInput.text;
        /*
        collabManager.server = serverAddressInput.text;
        syncManager.server = serverAddressInput.text;
        slaveNode.server = serverAddressInput.text;
        masterNode.server = serverAddressInput.text;
        */
    }

    public void UpdateConnectionType()
    {
        switch (connectionTypeDropdown.value)
        {
            case 0:
                standalone = true;
                break;
            default:
                standalone = false;
                break;
            /*
            case 0:
                collabManager.connectionType = GMSECDefs.ConnectionTypes.bolt;
                syncManager.connectionType = SynchronizationManager.ConnectionTypes.bolt;
                slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.bolt;
                masterNode.connectionType = SessionMasterNode.ConnectionTypes.bolt;
                break;

            case 1:
                collabManager.connectionType = GMSECDefs.ConnectionTypes.mb;
                syncManager.connectionType = SynchronizationManager.ConnectionTypes.mb;
                slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.mb;
                masterNode.connectionType = SessionMasterNode.ConnectionTypes.mb;
                break;

            case 2:
                collabManager.connectionType = GMSECDefs.ConnectionTypes.amq383;
                syncManager.connectionType = SynchronizationManager.ConnectionTypes.amq383;
                slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.amq383;
                masterNode.connectionType = SessionMasterNode.ConnectionTypes.amq383;
                break;

            case 3:
                collabManager.connectionType = GMSECDefs.ConnectionTypes.amq384;
                syncManager.connectionType = SynchronizationManager.ConnectionTypes.amq384;
                slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.amq384;
                masterNode.connectionType = SessionMasterNode.ConnectionTypes.amq384;
                break;

            default:
                collabManager.connectionType = GMSECDefs.ConnectionTypes.bolt;
                syncManager.connectionType = SynchronizationManager.ConnectionTypes.bolt;
                slaveNode.connectionType = SessionSlaveNode.ConnectionTypes.bolt;
                masterNode.connectionType = SessionMasterNode.ConnectionTypes.bolt;
                break;
            */
        }
    }
    public void LoadVDE()
    {
        //TextAsset VDEMRET = (TextAsset)Resources.Load("VDE");

        System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(GSFC.ARVR.MRET.Common.Schemas.ProjectFileSchema));
        //ser.Deserialize(VDEMRET.text);
        //ModeNavigator.instance.OpenProject((GSFC.ARVR.MRET.Common.Schemas.ProjectType)GSFC.ARVR.MRET.Common.SchemaHandler.ReadXML(VDEMRET.text), ".", false);
        ModeNavigator.instance.OpenProject((GSFC.ARVR.MRET.Common.Schemas.ProjectType)GSFC.ARVR.MRET.Common.SchemaHandler.ReadXML("a:\\coda\\MRET.20210427\\Assets\\VDE.mret"), ".", false);
    }
}
#endif