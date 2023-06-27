// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.GMSEC;

public class GMPUB : MonoBehaviour
{
    public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };
    
    [Tooltip("The middleware type to use.")]
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    [Tooltip("The server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The subject to publish to.")]
    public string subject = "GMSEC.TEST";

    [Tooltip("Whether or not to publish as a oneshot. If set to false, messages will be published at repeatInterval")]
    public bool oneShot = true;

    [Tooltip("In seconds.")]
    public float repeatInterval = 1;

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // This counter will keep track of the number of messages published.
    private int counter = 0;

    // This will allow us to test signed values.
    private sbyte negativeMultiplier = -1;

    void Start ()
    {
        Debug.Log("[GMPUB] Initializing GMSEC Object");
        gmsec = gameObject.AddComponent<MonoGMSEC>();
        gmsec.Initialize();
        Debug.Log("[GMPUB] GMSEC Initialized");

        Debug.Log("[GMPUB] Setting up Config");
        gmsec.CreateConfig();
        gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
        gmsec.AddToConfig("server", server);
        Debug.Log("[GMPUB] Config Initialized");

        Debug.Log("[GMPUB] Connecting");
        gmsec.Connect();
        Debug.Log("[GMPUB] Connected");

        if (oneShot)
        {
            PublishMessage();
        }
        else
        {
            InvokeRepeating("PublishMessage", 0.1f, repeatInterval);
        }
    }
	
	void PublishMessage()
    {
        counter++;
        negativeMultiplier = System.Convert.ToSByte(-1 * negativeMultiplier);

        // Test each field type.
        gmsec.CreateNewMessage(subject);
        gmsec.AddBinaryFieldToMessage("BINARY-FIELD", (byte) 123, 1);
        gmsec.AddBoolFieldToMessage("BOOLEAN-FIELD", true);
        gmsec.AddCharFieldToMessage("CHAR-FIELD", 'a');
        gmsec.AddF32FieldToMessage("F32-FIELD", 1 + (float) 1 / counter);
        gmsec.AddF64FieldToMessage("F64-FIELD", 1 + (double) 1 / counter);
        gmsec.AddI8FieldToMessage("I8-FIELD", System.Convert.ToSByte((sbyte) counter * negativeMultiplier));
        gmsec.AddI16FieldToMessage("I16-FIELD", System.Convert.ToInt16((short) counter * -1 * negativeMultiplier));
        gmsec.AddI32FieldToMessage("I32-FIELD", (int) counter * negativeMultiplier);
        gmsec.AddI64FieldToMessage("I64-FIELD", (long) counter * -1 * negativeMultiplier);
        gmsec.AddU8FieldToMessage("U8-FIELD", (byte) counter);
        gmsec.AddU16FieldToMessage("U16-FIELD", (ushort) counter);
        gmsec.AddU32FieldToMessage("U32-FIELD", (uint) counter);
        gmsec.AddU64FieldToMessage("U64-FIELD", (ulong) counter);
        Debug.Log("[GMPUB] Publishing Message");
        gmsec.PublishMessage();
    }

    // Disconnect should be called to clean up connection resources.
    void OnDestroy()
    {
        Debug.Log("[GMPUB] Destroying Connection");
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