// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GSFC.ARVR.GMSEC;

public class SessionSeeker : MonoBehaviour
{
    public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

    [Tooltip("The middleware type to use.")]
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    [Tooltip("The server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The mission name to use.")]
    public string missionName = "UNSET";

    [Tooltip("The satellite name to use.")]
    public string satName = "UNSET";

    [Tooltip("The alias to use.")]
    public string alias = "New_User";

    [Tooltip("Available sessions found by the seeker.")]
    public List<SessionInformation> availableSessions = new List<SessionInformation>();

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // A thread to handle incoming requests asynchronously.
    private System.Threading.Thread requestThread;

    void Start()
    {
        Debug.Log("[SessionSeeker] Initializing Seeker");
        gmsec = gameObject.AddComponent<MonoGMSEC>();
        gmsec.Initialize();
        gmsec.CreateConfig();
        gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
        gmsec.AddToConfig("server", server);
        //gmsec.AddToConfig("GMSEC-REQ-RESP", "OPEN-RESP");   // Enable GMSEC open response feature.
        Debug.Log("[SessionSeeker] Seeker Initialized");
    }

    public void SearchForSessions()
    {
        if (gmsec == null)
        {
            gmsec = gameObject.AddComponent<MonoGMSEC>();
            gmsec.Initialize();

            gmsec.CreateConfig();
            gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
            gmsec.AddToConfig("server", server);
            //gmsec.AddToConfig("GMSEC-REQ-RESP", "OPEN-RESP");   // Enable GMSEC open response feature.
        }

        Debug.Log("[SessionSeeker] Connecting to GMSEC bus and searching for sessions");
        gmsec.Connect();

        requestThread = new System.Threading.Thread(new System.Threading.ThreadStart(RequestSessionInfo));
        requestThread.Start();
    }

    private void RequestSessionInfo()
    {
        string hostName = "unknown";

        if (gmsec == null)
        {
            Debug.Log("null");
        }

        try
        {
            hostName = System.Net.Dns.GetHostName();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[SessionSeeker] " + e.ToString());
        }

        gmsec.Subscribe("GMSEC.*.*.RESP.DIR.ADVERTISER.>");

        // Create Request Message with Header.
        gmsec.CreateNewMessage("GMSEC." + missionName + "." + satName
            + ".REQ.DIR.SEEKER.LIST");
        gmsec.AddF32FieldToMessage("HEADER-VERSION", 2010);
        gmsec.AddStringFieldToMessage("MESSAGE-TYPE", "REQ");
        gmsec.AddStringFieldToMessage("MESSAGE-SUBTYPE", "DIR");
        gmsec.AddF32FieldToMessage("CONTENT-VERSION", 2016);
        gmsec.AddStringFieldToMessage("USER", alias + "." + hostName);
        gmsec.AddStringFieldToMessage("DIRECTIVE-STRING", "LISTSESSIONS");
        gmsec.AddBoolFieldToMessage("RESPONSE", true);
        gmsec.PublishMessage();

        for (int i = 0; i < 200; i++)
        {
            GMSECMessage rpl = gmsec.Receive(10);

            if (rpl != null)
            {
                string[] messageData = rpl.GetField("DATA").GetValueAsString().Split(';');
                if (messageData.Length == 4)
                {
                    SessionInformation sessionInfo = new SessionInformation();
                    sessionInfo.sessionName = messageData[0];
                    sessionInfo.groupName = messageData[1];
                    sessionInfo.projectName = messageData[2];
                    sessionInfo.numUsers = System.Convert.ToInt32(messageData[3]);
                    availableSessions.Add(sessionInfo);
                }
            }
        }

        //gmsec.Disconnect();

        foreach (SessionInformation sess in availableSessions)
        {
            Debug.Log(sess.sessionName + " " + sess.projectName + " " + sess.groupName + " " + sess.numUsers);
        }
    }

    // Disconnect should be called to clean up connection resources.
    void OnDestroy()
    {
        if (requestThread != null)
        {
            requestThread.Abort();
        }
        Debug.Log("[GMREQ] Destroying Connection");
        //gmsec.Disconnect();
    }

#region Helpers
    private string ConnectionTypeToString(ConnectionTypes rawConnType)
    {
        string connType = "gmsec_bolt";

        switch (rawConnType)
        {
            case ConnectionTypes.amq383:
                connType = "gmsec_activemq383";
                break;

            case ConnectionTypes.amq384:
                connType = "gmsec_activemq384";
                break;

            case ConnectionTypes.bolt:
                connType = "gmsec_bolt";
                break;

            case ConnectionTypes.mb:
                connType = "gmsec_mb";
                break;

            case ConnectionTypes.ws71:
                connType = "gmsec_websphere71";
                break;

            case ConnectionTypes.ws75:
                connType = "gmsec_websphere75";
                break;

            case ConnectionTypes.ws80:
                connType = "gmsec_websphere80";
                break;

            default:
                connType = "gmsec_bolt";
                break;
        }

        return connType;
    }
#endregion
}