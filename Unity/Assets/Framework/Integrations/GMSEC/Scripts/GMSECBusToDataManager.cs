// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.GMSEC;

public class GMSECBusToDataManager : MonoBehaviour
{
#if !HOLOLENS_BUILD
    public DataManager dataManager;

    public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

    [Tooltip("The middleware type to use.")]
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    [Tooltip("The server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The subject to listen on.")]
    public string subject = "GMSEC.>";

    [Tooltip("Throttling. Will only read one out of every of these number of messages. Set to 1 to not skip messages.")]
    [Range(1, 2048)]
    public int messageReadFrequency = 1;

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // Keep track of if a subscription has been set up.
    private bool subscribed = false;

    // Keep track of messages to skip.
    private int messageReadCounter = 0;

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
        if (messageReadCounter++ < messageReadFrequency)
        {
            gmsec.Receive(0);
            return;
        }

        GMSECMessage msg = gmsec.Receive(0);
        if (msg != null)
        {
            int fieldNum = 1;
            bool noMoreFields = false;
            while (!noMoreFields)
            {
                GMSECMessage.Field nameField = msg.GetStringField("MNEMONIC." + fieldNum + ".NAME");
                if (nameField != null)
                {
                    GMSECMessage.Field valueField = msg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.RAW-VALUE");
                    if (valueField == null)
                    {
                        valueField = msg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.EU-VALUE");
                        if (valueField == null)
                        {
                            valueField = msg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.TEXT-VALUE");
                        }
                    }
                    object value = valueField.GetValue();

                    object redHighValue = null, redLowValue = null, yellowHighValue = null, yellowLowValue = null;
                    bool redHigh = false, redLow = false;
                    GMSECMessage.Field redHighField = msg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.RED-HIGH");
                    if (redHighField != null)
                    {
                        redHigh = (bool) redHighField.GetValue();
                        if (redHigh)
                        {
                            redHighValue = (float) value - 1;
                        }
                        else
                        {
                            redHighValue = (float) value + 1;
                        }
                    }

                    GMSECMessage.Field redLowField = msg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.RED-LOW");
                    if (redLowField != null)
                    {
                        redLow = (bool) redLowField.GetValue();
                        if (redLow)
                        {
                            redLowValue = (float) value + 1;
                        }
                        else
                        {
                            redLowValue = (float) value - 1;
                        }
                    }

                    GMSECMessage.Field yellowHighField = msg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.YELLOW-HIGH");
                    if (yellowHighField != null)
                    {
                        bool yellowHigh = (bool) yellowHighField.GetValue();
                        if (yellowHigh && (redHigh == false))
                        {
                            yellowHighValue = (float) value - 1;
                        }
                        else
                        {
                            yellowHighValue = (float) value + 1;
                        }
                    }

                    GMSECMessage.Field yellowLowField = msg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.YELLOW-LOW");
                    if (yellowLowField != null)
                    {
                        bool yellowLow = (bool) yellowLowField.GetValue();
                        if (yellowLow && (redLow == false))
                        {
                            yellowLowValue = (float) value + 1;
                        }
                        else
                        {
                            yellowLowValue = (float) value - 1;
                        }
                    }

                    if (valueField != null)
                    {
                        dataManager.SaveValue(nameField.GetValueAsString(), value,
                            "none", null, redLowValue, yellowLowValue, yellowHighValue, redHighValue);
                    }
                    else
                    {
                        Debug.Log("INVALID " + nameField);
                        Debug.LogWarning("[GMSECBusToDataManager->ReceiveMessage] Invalid Mnemonic Message Detected. Skipping mnemonic value.");
                    }
                }
                else
                {
                    noMoreFields = true;
                }
                fieldNum++;
            }
            dataManager.SaveToCSV("values.csv");
            messageReadCounter = 0;
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