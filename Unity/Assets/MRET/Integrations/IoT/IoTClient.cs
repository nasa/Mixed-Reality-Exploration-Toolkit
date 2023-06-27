// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

#if MRET_EXTENSION_M2MQTT
// nuget> Install-Package M2Mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    public class IoTClient : ClientInterface<IoTInterfaceType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IoTClient);

        public const string IOT_DISCONNECTED_ERROR = "Temporarily disconnected";
        public const string DEFAULT_SERVER = "mosquitto.labs.appdat.jsc.nasa.gov";
        public const int DEFAULT_PORT = 8883;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private IoTInterfaceType serializedIoTInterface;

        [Tooltip("Event for the Authentication button.")]
        public UnityEvent AuthenticationEvent;

        // This dictionary uses MQTT topic (referred to as Pattern in the xml) to find the IoTTopicType that matches
        private Dictionary<string, IoTTopicType> topics;

        // MQTT
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

        /// <seealso cref="IClientInterface.Connected"/>
        /* This might shortcircuit the logic in parent
        new public bool Connected
        {
            get => base.Connected && IsConnected;
            protected set => base.Connected = value;
        }
        */

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Initialize the topics dictionary
            topics = new Dictionary<string, IoTTopicType>();
        }
#endregion MRETUpdateBehaviour

#region Serializable
        /// <seealso cref="ClientInterface{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(IoTInterfaceType serialized, SerializationState deserializationState)
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
            serializedIoTInterface = serialized;

            // Deserialize the IoTClient settings

            // Topics (optional)
            topics.Clear();
            if ((serializedIoTInterface.Topics != null) && (serializedIoTInterface.Topics.Length > 0))
            {
                foreach (IoTTopicType element in serializedIoTInterface.Topics)
                {
                    // Make sure the specified IoTThing exists
                    IIdentifiable identifiable = MRET.UuidRegistry.GetByID(element.ThingID);
                    if (identifiable is IoTThing)
                    {
                        topics.Add(element.pattern, element);
                    }
                    else
                    {
                        deserializationState.Error("Topic references an IoTThing that doesn't exist: " + element.ThingID);
                        yield break;
                    }
                }
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="ClientInterface{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(IoTInterfaceType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the IoTClient settings

            // Topics (optional)
            serialized.Topics = null;
            if (topics.Count > 0)
            {
                List<IoTTopicType> serializedTopics = new List<IoTTopicType>();
                foreach (IoTTopicType element in topics.Values)
                {
                    // TODO: If the Thing ID has changed for any reason, this likely won't
                    // deserialize back in the next time. We should figure out a better
                    // storage mechanism where we (perhaps) save a reference to the IoTThing.
                    serializedTopics.Add(element);
                }
                serialized.Topics = serializedTopics.ToArray();
            }

            // Save the final serialized reference
            serializedIoTInterface = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
#endregion Serializable

#region ClientInterface
        /// <seealso cref="ClientInterface{T}.PerformDisconnect"/>
        protected override bool PerformDisconnect()
        {
            bool result = false;
            try
            {
#if MRET_EXTENSION_M2MQTT
                if (IsConnected)
                {
                    // Unregister event callbacks
                    client.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
                    client.ConnectionClosed -= Client_MqttConnectionClosed;

                    // Disconnect
                    client.Disconnect();
                }
                client = null;
#endif

                // Mark as disconnected
                result = true;
            }
            catch (Exception e)
            {
                LogError(e.ToString(), nameof(PerformDisconnect));
            }

            return result;
        }

        /// <seealso cref="ClientInterface{T}.PerformConnect(string)"/>
        protected override bool PerformConnect(string serverConnection, X509Certificate2 serverCert, X509Certificate2 clientCert)
        {
            Log("Establishing MQTT client connection", nameof(PerformConnect));

            // Make the connection
#if MRET_EXTENSION_M2MQTT
            clientid = clientCert.GetNameInfo(X509NameType.SimpleName, false);
            client = new MqttClient(Server, Port, true, serverCert, clientCert, MqttSslProtocols.TLSv1_2, RemoteCertificateValidationCallback);
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.ConnectionClosed += Client_MqttConnectionClosed;
            client.Connect(clientid);

            Log("MQTT client connection initiated", nameof(PerformConnect));

            Log("Subscribing to MQTT topics", nameof(PerformConnect));

            // Subscribe to every topic specified
            if (topics.Count == 0)
            {
                // Subscribe to all topics with a wildcard
                client.Subscribe(new string[] { "#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            else
            {
                // Explicitly subscribe to each specified topic
                foreach (string pattern in topics.Keys)
                {
                    client.Subscribe(new string[] { pattern }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                }
            }

            Log("Subscribed to MQTT topics", nameof(PerformConnect));

            // Report as connected
            return true;
#else
            LogWarning("M2MQTT not installed", nameof(PerformConnect));
            return false;
#endif
        }
        #endregion ClientInterface

#if MRET_EXTENSION_M2MQTT
        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        //Whenever a message is recieved from a subscribed topic save its payload to the data manager
        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
#if MRET_DEBUG
            Debug.Log("\nMessage Received");
            Debug.Log($"Topic: {e.Topic} | Payload: {System.Text.Encoding.UTF8.GetString(e.Message)} | QoS: {e.QosLevel}");
#endif

            string payloadKey = InteractableSceneObject.DATA_POINT_KEY_PREFIX + e.Topic;
            payloadKey = payloadKey.Replace('/', '.').ToUpper();

            string checkInKey = "GOV.NASA.GSFC.XR.MRET.IOT.CHECKIN." + Server;
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

            // Extract the payload JSON string
            string payload = System.Text.Encoding.UTF8.GetString(e.Message);

            IoTPayload payloadClass = new IoTPayload(topic.ThingID, payload);
            payloadClass.WriteToDataManager(payloadKey, thingIDKey);
            MRET.DataManager.SaveValue(new DataManager.DataValue(checkInKey, client.IsConnected));
        }

        // Whenever the connection closes unexpectedly create the error-dialog box that the InterfaceManager uses
        private void Client_MqttConnectionClosed(object sender, EventArgs e)
        {
            ProjectManager.InterfaceManager.ReportError((this, new ServerDisconnectException(DISCONNECTED_ERROR, 2)));
        }
#endif

        /// <summary>
        /// Create an IoTClient from a prefab
        /// </summary>
        /// <param name="clientName">Name of the client</param>
        /// <param name="clientPrefab">The client prefab to instantiate</param>
        /// <param name="container">The container (parent) for the client</param>
        /// <returns>The instantiated <code>IoTClient</code></returns>
        public static IoTClient Create(string clientName, GameObject clientPrefab, Transform container = null)
        {
            // Make sure we have a valid container reference
            container = (container == null) ? ProjectManager.InterfacesContainer.transform : container;

            // Instantiate the prefab
            GameObject clientGO = Instantiate(clientPrefab, container);

            // Get the client reference
            IoTClient note = clientGO.GetComponent<IoTClient>();
            if (note == null)
            {
                note = clientGO.AddComponent<IoTClient>();
                // TODO: If we have to add the client, we need to set
                // the client references to the correct cclient prefab
                // gameobjects to work.
            }

            // Generate a better ID for the client for serialization
            note.id = MRET.UuidRegistry.CreateUniqueIDFromName(clientName);

            // Rename the game object
            clientGO.name = clientName;

            return note;
        }

    }

}