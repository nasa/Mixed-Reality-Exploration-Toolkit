using UnityEngine;
using GSFC.ARVR.GMSEC;

public class GMRPL : MonoBehaviour
{
    public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

    [Tooltip("The middleware type to use.")]
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    [Tooltip("The server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The subject to reply to.")]
    public string subject = "GMSEC.MISSION.SAT_ID.REQ.CMD.GMRPL";

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // Keep track of if a subscription has been set up.
    private bool subscribed = false;

    void Start()
    {
        Debug.Log("[GMRPL] Initializing GMSEC Object");
        gmsec = gameObject.AddComponent<MonoGMSEC>();
        gmsec.Initialize();
        Debug.Log("[GMRPL] GMSEC Initialized");

        Debug.Log("[GMRPL] Setting up Config");
        gmsec.CreateConfig();
        gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
        gmsec.AddToConfig("server", server);
        Debug.Log("[GMRPL] Config Initialized");

        Debug.Log("[GMRPL] Connecting");
        gmsec.Connect();
        Debug.Log("[GMRPL] Connected");

        Debug.Log("[GMRPL] Subscribing");
        gmsec.Subscribe(subject);
        subscribed = true;
        Debug.Log("[GMRPL] Subscribed");
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
            Debug.Log("[GMRPL] Message Received:\n" + msg.ToString());

            if (msg.GetKind() == GMSECMessage.MessageKind.REQUEST)
            {
                // Get Component.
                GMSECMessage.Field componentField = msg.GetStringField("COMPONENT");
                string component = (componentField == null) ? "UNKNOWN-COMPONENT" : componentField.GetValueAsString();

                // Set Status Code to 1 for Acknowledgement.
                string statusCode = "1";

                // Set Reply Subject.
                string rplSubject = "GMSEC.MISSION.SAT_ID.RESP." + component + "." + statusCode;

                // Create Reply Message.
                GMSECMessage reply = new GMSECMessage(rplSubject, GMSECMessage.MessageKind.REPLY);

                // Add Fields to Reply Message.
                reply.AddField("COMPONENT", "GMRPL");
                reply.AddField("ANSWER", "Sure looks like it!");

                // Log Reply Message.
                Debug.Log("[GMRPL] Replying with: " + reply.ToString());

                // Send Reply.
                gmsec.Reply(msg, reply);
            }
        }
    }

    // Disconnect should be called to clean up connection resources.
    void OnDestroy()
    {
        Debug.Log("[GMRPL] Destroying Connection");
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
}