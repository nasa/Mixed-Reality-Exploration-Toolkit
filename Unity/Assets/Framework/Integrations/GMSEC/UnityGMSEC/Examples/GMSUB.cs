// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.GMSEC;

public class GMSUB : MonoBehaviour
{
#if !HOLOLENS_BUILD
    public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

    [Tooltip("The middleware type to use.")]
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    [Tooltip("The server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The subject to listen on.")]
    public string subject = "GMSEC.>";

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // Keep track of if a subscription has been set up.
    private bool subscribed = false;

    void Start()
    {
        Debug.Log("[GMSUB] Initializing GMSEC Object");
        gmsec = gameObject.AddComponent<MonoGMSEC>();
        gmsec.Initialize();
        Debug.Log("[GMSUB] GMSEC Initialized");
        
        Debug.Log("[GMSUB] Setting up Config");
        gmsec.CreateConfig();
        gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
        gmsec.AddToConfig("server", server);
        Debug.Log("[GMSUB] Config Initialized");

        Debug.Log("[GMSUB] Connecting");
        gmsec.Connect();
        Debug.Log("[GMSUB] Connected");

        Debug.Log("[GMSUB] Subscribing");
        gmsec.Subscribe(subject);
        subscribed = true;
        Debug.Log("[GMSUB] Subscribed");
    }

    void Update()
    {
        if (subscribed)
        {
            ReceiveMessage();
        }
    }

    void ReceiveMessage()
    {
        GMSECMessage msg = gmsec.Receive(0);
        if (msg != null)
        {
            Debug.Log("[GMSUB] Message Received:\n" + msg.ToString());
        }
    }

    // Disconnect should be called to clean up connection resources.
    void OnDestroy()
    {
        Debug.Log("[GMSUB] Destroying Connection");
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