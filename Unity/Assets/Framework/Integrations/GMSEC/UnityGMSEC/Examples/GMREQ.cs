// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.GMSEC;

public class GMREQ : MonoBehaviour
{
#if !HOLOLENS_BUILD
    public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

    [Tooltip("The middleware type to use.")]
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    [Tooltip("The server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The subject to request on.")]
    public string subject = "GMSEC.MISSION.SAT_ID.REQ.CMD.GMRPL";

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // A thread to handle incoming requests asynchronously.
    private System.Threading.Thread requestThread;

    void Start()
    {
        Debug.Log("[GMREQ] Initializing GMSEC Object");
        gmsec = gameObject.AddComponent<MonoGMSEC>();
        gmsec.Initialize();
        Debug.Log("[GMREQ] GMSEC Initialized");

        Debug.Log("[GMREQ] Setting up Config");
        gmsec.CreateConfig();
        gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
        gmsec.AddToConfig("server", server);
        Debug.Log("[GMREQ] Config Initialized");

        Debug.Log("[GMREQ] Connecting");
        gmsec.Connect();
        Debug.Log("[GMREQ] Connected");

        requestThread = new System.Threading.Thread(new System.Threading.ThreadStart(Request));
        requestThread.Start();
    }

    void Request()
    {
        Debug.Log("[GMREQ] Requesting");
        GMSECMessage req = new GMSECMessage(subject, GMSECMessage.MessageKind.REQUEST);
        req.AddField("QUESTION", "Does the request/reply functionality still work?");
        req.AddField("COMPONENT", "GMREQ");
        Debug.Log("Request Message: " + req.ToString());
        GMSECMessage rpl = gmsec.Request(req, 10000);
        Debug.Log("[GMREQ] Got Reply: " + rpl.ToString());
    }

    // Disconnect should be called to clean up connection resources.
    void OnDestroy()
    {
        requestThread.Abort(); ;
        Debug.Log("[GMREQ] Destroying Connection");
        gmsec.Disconnect();
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
#endif
}