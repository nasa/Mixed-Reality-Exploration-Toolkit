// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.GMSEC;

/*  The "Advertiser" component informs other nodes of the presence of this "Master" node.
 *  It listens for and responds to GMSEC directive request messages.
 */
public class SessionAdvertiser : MonoBehaviour
{
    [Tooltip("The middleware type to use.")]
    public GMSECDefs.ConnectionTypes connectionType = GMSECDefs.ConnectionTypes.bolt;

    [Tooltip("The GMSEC server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The mission name to use.")]
    public string missionName = "UNSET";

    [Tooltip("The satellite name to use.")]
    public string satName = "UNSET";

    [Tooltip("Information about the session.")]
    public SessionInformation sessionInformation = new SessionInformation();

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // Keep track of if a subscription has been set up.
    private bool subscribed = false;

    public void Initialize()
    {
        if (gmsec == null)
        {
            gmsec = gameObject.AddComponent<MonoGMSEC>();
        }

        Debug.Log("[SessionAdvertiser] Initializing Advertiser");

        //gmsec.Initialize();

        //gmsec.CreateConfig();
        //gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
        //gmsec.AddToConfig("server", server);
        //gmsec.AddToConfig("GMSEC-REQ-RESP", "OPEN-RESP");   // Enable GMSEC open response feature.

        //gmsec.Connect();

        gmsec.Subscribe("GMSEC." + missionName + "." + satName + ".REQ.DIR.SEEKER.LIST");
        subscribed = true;
        Debug.Log("[SessionAdvertiser] Advertiser Initialized");
    }

    public void RespondToListRequest(GMSECMessage msg)
    {
        // Set Reply Subject.
        string rplSubject = "GMSEC." + missionName + "." + satName + ".RESP.DIR.ADVERTISER.1";

        // Create Reply Message with Header.
        /*GMSECMessage reply = new GMSECMessage(rplSubject, GMSECMessage.MessageKind.REPLY);
        reply.AddField("HEADER-VERSION", 2010f);
        reply.AddField("MESSAGE-TYPE", "RESP");
        reply.AddField("MESSAGE-SUBTYPE", "DIR");
        reply.AddField("CONTENT-VERSION", 2016f);

        // Add Fields to Reply Message.
        reply.AddField("RESPONSE-STATUS", (short) 1);
        reply.AddField("DATA", sessionInformation.sessionName + ";"
            + sessionInformation.groupName + ";" + sessionInformation.projectName);*/

        // Send Reply.
        //gmsec.Reply(msg, reply);

        gmsec.CreateNewMessage(rplSubject);
        gmsec.AddF32FieldToMessage("HEADER-VERSION", 2010);
        gmsec.AddStringFieldToMessage("MESSAGE-TYPE", "RESP");
        gmsec.AddStringFieldToMessage("MESSAGE-SUBTYPE", "DIR");
        gmsec.AddF32FieldToMessage("CONTENT-VERSION", 2016);
        gmsec.AddI16FieldToMessage("RESPONSE-STATUS", 1);
        gmsec.AddStringFieldToMessage("DATA", sessionInformation.sessionName + ";"
            + sessionInformation.groupName + ";" + sessionInformation.projectName
            + ";" + sessionInformation.numUsers);
        gmsec.PublishMessage();

        Debug.Log("[SessionAdvertiser] Session Information Sent");
    }

    void Start()
    {
        gmsec = gameObject.AddComponent<MonoGMSEC>();
    }

    /*void Update()
    {
        if (subscribed)
        {
            HandleListRequests();
        }
    }*/

    void HandleListRequests()
    {
        GMSECMessage msg = gmsec.Receive(0);
        if (msg != null)
        {
            //if (msg.GetKind() == GMSECMessage.MessageKind.REQUEST)
            {
                RespondToListRequest(msg);
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
    private string ConnectionTypeToString(GMSECDefs.ConnectionTypes rawConnType)
    {
        string connType = "gmsec_bolt";

        switch (rawConnType)
        {
            case GMSECDefs.ConnectionTypes.amq383:
                connType = "gmsec_activemq383";
                break;

            case GMSECDefs.ConnectionTypes.amq384:
                connType = "gmsec_activemq384";
                break;

            case GMSECDefs.ConnectionTypes.bolt:
                connType = "gmsec_bolt";
                break;

            case GMSECDefs.ConnectionTypes.mb:
                connType = "gmsec_mb";
                break;

            case GMSECDefs.ConnectionTypes.ws71:
                connType = "gmsec_websphere71";
                break;

            case GMSECDefs.ConnectionTypes.ws75:
                connType = "gmsec_websphere75";
                break;

            case GMSECDefs.ConnectionTypes.ws80:
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