// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using GOV.NASA.GSFC.XR.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC
{
    public class GMSECBusToDataManager : ClientInterface<GMSECInterfaceType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(GMSECBusToDataManager);

        public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

        public const string DEFAULT_SERVER = "localhost";
        public const int DEFAULT_PORT = 9100;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private GMSECInterfaceType serializedGMSECInterface;

        [Tooltip("The middleware type to use.")]
        public ConnectionTypes connectionType = ConnectionTypes.bolt;

        [Tooltip("The subject to listen on.")]
        public string subject = "GMSEC.>";

        // A MonoGMSEC object is needed to interface with GMSEC.
        public MonoGMSEC gmsec { get; private set; }

        // Keep track of if a subscription has been set up.
        private bool subscribed = false;

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            if (state == IntegrityState.Success)
            {
                // Custom checks
            }

            return state;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Create our serializable GMSEC object
            gmsec = gameObject.GetComponent<MonoGMSEC>();
            if (gmsec == null)
            {
                gmsec = gameObject.AddComponent<MonoGMSEC>();
            }
        }

        /// <seealso cref="MRETBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // NOTE: This will already be throttled by the update frequency in the MRETUpdateBehaviour
            base.MRETUpdate();

            if (subscribed)
            {
                ReceiveMessage();
            }
        }
        #endregion MRETBehaviour

        #region Serializable
        /// <seealso cref="ClientInterface{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(GMSECInterfaceType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedGMSECInterface = serialized;

            // Deserialize the GMSEC settings
            SchemaUtil.DeserializeGMSECConnectionType(serialized.Connection, ref connectionType);
            subject = serializedGMSECInterface.Subject;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="ClientInterface{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(GMSECInterfaceType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the GMSEC settings
            GMSECConnectionType connection = serialized.Connection;
            SchemaUtil.SerializeGMSECConnectionType(connectionType, ref connection);
            serialized.Connection = connection;
            serialized.Subject = subject;

            // Save the final serialized reference
            serializedGMSECInterface = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        #region ClientInterface
        public override string GetConnectionProtocol()
        {
            string server = this.Server;
            int port = this.Port;

            if (string.IsNullOrEmpty(server))
            {
                server = DEFAULT_SERVER;
            }
            if (port <= 0)
            {
                port = DEFAULT_PORT;
            }

            // Make sure to remove whitespace at beginning and end
            return server.Trim() + ":" + port;
        }

        /// <seealso cref="ClientInterface{T}.PerformDisconnect"/>
        protected override bool PerformDisconnect()
        {
            bool result = false;
            try
            {
                // Disconnect
                gmsec.Disconnect();

                // Mark as sucecessful
                result = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[" + ClassName + "]->Disconnect: A problem occurred attempting to disconnect from GMSEC: " + e.Message);
            }

            return result;
        }

        /// <seealso cref="ClientInterface{T}.PerformConnect(string, X509Certificate2, X509Certificate2)"/>
        protected override bool PerformConnect(string serverConnection, X509Certificate2 serverCert = null, X509Certificate2 clientCert = null)
        {
            bool result = false;

            // Perform the connection
            try
            {
                Debug.Log("[" + ClassName + "] Initializing GMSEC");
                gmsec.Initialize();
                Debug.Log("[" + ClassName + "] GMSEC Initialized");

                Debug.Log("[" + ClassName + "] Setting up Config");
                gmsec.CreateConfig();
                gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
                gmsec.AddToConfig("server", serverConnection);
                Debug.Log("[" + ClassName + "] Config Initialized");

                Debug.Log("[" + ClassName + "] Connecting");
                gmsec.Connect();
                Debug.Log("[" + ClassName + "] Connected");

                Debug.Log("[" + ClassName + "] Subscribing");
                gmsec.Subscribe(subject);
                subscribed = true;
                Debug.Log("[" + ClassName + "] Subscribed");

                // Mark as connected
                result = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[" + ClassName + "]->Connect: A problem occurred connecting to GMSEC: " + e.Message);
            }

            return result;
        }
        #endregion ClientInterface

        /// <summary>
        /// Receives any subscribed messages from the message bus
        /// </summary>
        protected void ReceiveMessage()
        {
            // The Update is throttled to the desired frequesncy, so we
            // want to throw away all queued messages except the newest one
            GMSECMessage newestMsg = null;
            GMSECMessage queuedMsg = null;
            do
            {
                // Save the newest message
                newestMsg = queuedMsg;

                // Pop the next message off the queue
                queuedMsg = gmsec.Receive(0);
            } while (queuedMsg != null);

            // If we have a new message, process it
            if (newestMsg != null)
            {
                int fieldNum = 1;
                bool noMoreFields = false;
                while (!noMoreFields)
                {
                    GMSECMessage.Field nameField = newestMsg.GetStringField("MNEMONIC." + fieldNum + ".NAME");
                    if (nameField != null)
                    {
                        GMSECMessage.Field valueField = newestMsg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.RAW-VALUE");
                        if (valueField == null)
                        {
                            valueField = newestMsg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.EU-VALUE");
                            if (valueField == null)
                            {
                                valueField = newestMsg.GetField("MNEMONIC." + fieldNum + ".SAMPLE.1.TEXT-VALUE");
                            }
                        }

                        if (valueField != null)
                        {
                            MRET.DataManager.SaveValue(nameField.GetValueAsString(), valueField.GetValue());
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
                MRET.DataManager.SaveToCSV("values.csv");
            }
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
        #endregion Helpers

    }
}