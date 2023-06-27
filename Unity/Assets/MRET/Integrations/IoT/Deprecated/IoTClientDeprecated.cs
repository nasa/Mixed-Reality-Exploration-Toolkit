// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

#if MRET_EXTENSION_M2MQTT
// nuget> Install-Package M2Mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Exceptions;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Integrations.IoT.IoTClient) + " class")]
    public class IoTClientDeprecated : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IoTClientDeprecated);

        public const string DISCONNECTED_ERROR = "Temporarily disconnected";

        public string description;
        public string server;
        public int port;
        //This dictionary uses MQTT topic (referred to as Pattern in the xml) to find the IoTTopicType that matches
        public Dictionary<string, IoTTopicType> topics;
//        public X509Certificate serverCert;
//        public X509Certificate2 clientCert;

        [Tooltip("Event for the Authentication button.")]
        public UnityEvent AuthenticationEvent;

#if MRET_EXTENSION_M2MQTT
        private MqttClient client;
#endif
        private string clientid;

        /// <summary>
        /// Indicates if this client is connected
        /// </summary>
        public bool IsConnected =>
#if MRET_EXTENSION_M2MQTT
            ((client != null) && client.IsConnected);
#else
            false;
#endif

        //Create a client and connect it to the specifed broker.
        //Then Subscribe to all specified topics
        //If there is an exception, create the same error-dialog box that the IoTManagerDeprecated makes
        public void Connect()
        {
            try
            {
#if MRET_EXTENSION_M2MQTT
                // Check for valid certs
                X509Certificate2 serverCert = IoTManagerDeprecated.instance.GetServerCert(server);
                if (serverCert == null)
                {
                    string errorMessage = "Server certificate is not registered. Please register a valid server cert for server: " + server;
                    throw new IoTServerConnectionExceptionDeprecated(errorMessage);
                }
                X509Certificate2 clientCert = IoTManagerDeprecated.instance.GetClientCert(server);
                if (clientCert == null)
                {
                    string errorMessage = "Client certificate is not registered. Please register a valid client cert for server: " + server;
                    throw new IoTClientConnectionExceptionDeprecated(errorMessage);
                }

                // Make the connection
                clientid = clientCert.GetNameInfo(X509NameType.SimpleName, false); ;
                client = new MqttClient(server, port, true, serverCert, clientCert, MqttSslProtocols.TLSv1_2, RemoteCertificateValidationCallback);
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                client.ConnectionClosed += Client_MqttConnectionClosed;
                client.Connect(clientid);

                // Subscribe to every topic specified
                foreach (string pattern in topics.Keys)
                {
                    client.Subscribe(new string[] { pattern }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                }
#else
                Log("M2MQTT not installed", nameof(Connect));
#endif
            }
            catch (IoTServerConnectionExceptionDeprecated e)
            {
                // This sort of error probably has to do with the xml, or choosing the wrong server CA
                Debug.LogError("An issue with the server connection was encountered. Connection failed: " + e.ToString());
                IoTManagerDeprecated.instance.IoTClientErrors.Enqueue((this, e));
            }
            catch (IoTClientConnectionExceptionDeprecated e)
            {
                // This sort of error probably has to do with the xml, or choosing the wrong user certification
                Debug.LogError("An issue with the client connection was encountered. Connection failed: " + e.ToString());
                IoTManagerDeprecated.instance.IoTClientErrors.Enqueue((this, e));
            }
            catch (Exception e)
            {
                // This handles all other exceptions
                Debug.LogError("Connection failed: " + e.ToString());
                IoTManagerDeprecated.instance.IoTClientErrors.Enqueue((this, e));
            }
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

#if MRET_EXTENSION_M2MQTT
        //Whenever a message is recieved from a subscribed topic save its payload to the data manager
        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
#if MRET_DEBUG
            Debug.Log("\nMessage Received");
            Debug.Log($"Topic: {e.Topic} | Payload: {System.Text.Encoding.UTF8.GetString(e.Message)} | QoS: {e.QosLevel}");
#endif

            string payloadKey = "GOV.NASA.GSFC.XR.MRET.IOT.PAYLOAD." + e.Topic;
            payloadKey = payloadKey.Replace('/', '.').ToUpper();

            string checkInKey = "GOV.NASA.GSFC.XR.MRET.IOT.CHECKIN." + server;
            checkInKey = checkInKey.Replace('/', '.').ToUpper();

            string thingIDKey = "GOV.NASA.GSFC.XR.MRET.IOT.ID." + e.Topic;
            thingIDKey = thingIDKey.Replace('/', '.').ToUpper();

            IoTTopicType topic;
            string topicString = e.Topic;
            while (!topics.TryGetValue(topicString, out topic))
            {
                int topicSplit = topicString.LastIndexOf("/");
                //If the topic ends in / or /# cut off that part and replace the preceding /topic with /#
                if (topicString.Length <= topicSplit + 1 || topicString[topicSplit + 1] == '#')
                {
                    topicString = topicString.Substring(0, topicSplit);
                    topicSplit = topicString.LastIndexOf("/");
                }
                topicString = topicString.Substring(0, topicSplit) + "/#";
            }

            IoTPayload payloadClass = new IoTPayload(topic.ThingID, e.Message);
            payloadClass.WriteToDataManager(payloadKey, thingIDKey);
            MRET.DataManager.SaveValue(new DataManager.DataValue(checkInKey, client.IsConnected));
        }

        //Whenever the connection closes unexpectedly create the error-dialog box that the IoTManagerDeprecated uses
        public void Client_MqttConnectionClosed(object sender, EventArgs e)
        {
            IoTManagerDeprecated.instance.IoTClientErrors.Enqueue((this, new IoTClientConnectionExceptionDeprecated(DISCONNECTED_ERROR)));
        }
#endif

        private void DestroyConnection()
        {
            try
            {
                if (IsConnected)
                {
#if MRET_EXTENSION_M2MQTT
                    client.Disconnect();
#endif
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[IoTClientDeprecated->DestroyConnection] " + e.ToString());
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            DestroyConnection();
        }
    }

}