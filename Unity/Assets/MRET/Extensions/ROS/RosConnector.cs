// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Threading;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Extensions.Ros.IK;
#if MRET_EXTENSION_ROSSHARP
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.ImportUrdf;
using RosSharp.Urdf;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Extensions.Ros
{
    public class RosConnector : ClientInterface<ROSInterfaceType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(RosConnector);

        public const string ROS_FILE_EXT = ".urdf";
        public const string DEFAULT_PROTOCOL = "ws://";
        public const string DEFAULT_SERVER = "192.168.0.1";
        public const int DEFAULT_PORT = 9090;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private ROSInterfaceType serializedROSInterface;

#if MRET_EXTENSION_ROSSHARP
        public RosSocket RosSocket { get; private set; }
        public RosSocket.SerializerEnum Serializer = RosSocket.SerializerEnum.Microsoft;
        public Protocol Protocol = Protocol.WebSocketNET;
#endif

        public string JointStateSubscriberTopic = "/joint_states";
        public string JointStatePublisherTopic = "/joint_states";
        public string RobotDescription = null;

        private string RosBridgeServerUrl = null;
        public ManualResetEvent ConnectionEstablished { get; set; }

        #region Serializable
#if MRET_EXTENSION_ROSSHARP
        /// <summary>
        /// Deserializes the ROS protocol type
        /// </summary>
        /// <param name="serializedProtocolType">The <code>ROSProtocolType</code> to deserialize</param>
        /// <param name="protocolType">The deserialized <code>Protocol</code></param>
        protected void DeserializeROSProtocolType(
            ROSProtocolType serializedProtocolType, ref Protocol protocolType)
        {
            // ROS protocol
            switch (serializedProtocolType)
            {
                case ROSProtocolType.WebSocketSharp:
                    protocolType = Protocol.WebSocketSharp;
                    break;

                case ROSProtocolType.WebSocketDotNET:
                default:
                    protocolType = Protocol.WebSocketNET;
                    break;
            }
        }

        /// <summary>
        /// Deserializes the ROS serializer type
        /// </summary>
        /// <param name="serializedSerializerType">The <code>ROSSerializerType</code> to deserialize</param>
        /// <param name="serializerType">The deserialized <code>SerializerEnum</code></param>
        protected void DeserializeROSSerializerType(
            ROSSerializerType serializedSerializerType, ref RosSocket.SerializerEnum serializerType)
        {
            // ROS protocol
            switch (serializedSerializerType)
            {
                case ROSSerializerType.Newtonsoft_BSON:
                    serializerType = RosSocket.SerializerEnum.Newtonsoft_BSON;
                    break;

                case ROSSerializerType.Newtonsoft_JSON:
                    serializerType = RosSocket.SerializerEnum.Newtonsoft_JSON;
                    break;

                case ROSSerializerType.Microsoft:
                default:
                    serializerType = RosSocket.SerializerEnum.Microsoft;
                    break;
            }
        }
#endif

        /// <seealso cref="ClientInterface{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(ROSInterfaceType serialized, SerializationState deserializationState)
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
            serializedROSInterface = serialized;

            // Deserialize the ROS settings

            // Make sure we have a valid description file
            string description = serializedROSInterface.RobotDescription;
            if (!File.Exists(description))
            {
                // It's not explicit so see if it's relative
                if (!Path.IsPathRooted(description))
                {
                    // Relative location so try the user directory
                    description = Path.Combine(MRET.ConfigurationManager.GetDatapath(), description);
                    if (!File.Exists(description))
                    {
                        // Error condition
                        deserializationState.Error("Supplied robot description file does not exist: " +
                            serializedROSInterface.RobotDescription);
                    }
                }
                else
                {
                    // Error condition
                    deserializationState.Error("Supplied robot description file does not exist: " +
                        serializedROSInterface.RobotDescription);
                }
            }

            // If we do not have a valid robot description file, abort
            if (deserializationState.IsError) yield break;

            // Make sure the description has is a valid URDF extension
            if (!Path.GetExtension(serializedROSInterface.RobotDescription).ToLower().Equals(ROS_FILE_EXT))
            {
                // Error condition
                deserializationState.Error("Supplied robot description file does not have a valid \"" +
                    ROS_FILE_EXT + "\" file extension: " + serializedROSInterface.RobotDescription);

                // Abort
                yield break;
            }

            // Get the full path for the description
            RobotDescription = Path.GetFullPath(description);

#if MRET_EXTENSION_ROSSHARP
            // Set the full path static variable of the importer to the parent directory of the robot description
            RuntimeImportManager.fullPath = Path.GetDirectoryName(RobotDescription);

            // Import the robot to the scene and return the UrdfRobot component for the JointStatePatcher to use
            UrdfRobot robot = RuntimeImportManager.Import(gameObject.transform, Path.GetFileName(RobotDescription));
            JointStatePatcher patcher = gameObject.AddComponent<JointStatePatcher>();
            patcher.UrdfRobot = robot;

            // Set all Rigidbodies in the robot to use kinematics.
            robot.SetRigidbodiesIsKinematic(true);

            // If a topic for the subscriber is given in the serialized ROS interface, create the subscriber component,
            // set the specified topic, and enable the JointStateWriters and subscribing joints from the patcher
            // (Calling the patcher method from the RosSharp.RosBridgeCommunication namespace)
            if (!string.IsNullOrEmpty(serializedROSInterface.JointStateSubscriberTopic))
            {
                JointStateSubscriber subscriber = gameObject.transform.GetComponent<JointStateSubscriber>();
                if (subscriber == null)
                {
                    subscriber = gameObject.AddComponent<JointStateSubscriber>();
                }

                // Assign the topic and subscribe
                subscriber.Topic = serializedROSInterface.JointStateSubscriberTopic;
                patcher.SetSubscribeJointStates(true);
            }
            else
            {
                patcher.SetSubscribeJointStates(false);
            }
            JointStateSubscriberTopic = serializedROSInterface.JointStateSubscriberTopic;

            // If a topic for the publisher is given in the Part XML, create the publisher component,
            // set the specified topic, and enable the JointStateReaders and publishing joints from the patcher
            // (Calling the patcher method from the RosSharp.RosBridgeCommunication namespace)
            if (!string.IsNullOrEmpty(serializedROSInterface.JointStatePublisherTopic))
            {
                JointStatePublisher publisher = gameObject.transform.GetComponent<JointStatePublisher>();
                if (publisher == null)
                {
                    publisher = gameObject.AddComponent<JointStatePublisher>();
                }

                // Assign the topic for publishing
                publisher.Topic = serializedROSInterface.JointStatePublisherTopic;
                patcher.SetPublishJointStates(true);

                // Set up the inverse kinematics for the robot arm
                // The robot will have a cube as a target which can be moved around in the scene view
                // and the joint states will be published as the robot moves
                CCDIKManager.InitializeCCDIK(robot);
            }
            else
            {
                patcher.SetPublishJointStates(false);
            }
            JointStatePublisherTopic = serializedROSInterface.JointStatePublisherTopic;

            // ROS protocol
            DeserializeROSProtocolType(serializedROSInterface.Protocol, ref Protocol);

            // ROS serializer
            DeserializeROSSerializerType(serializedROSInterface.Serializer, ref Serializer);
#endif

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

#if MRET_EXTENSION_ROSSHARP
        /// <summary>
        /// Serializes the ROS protocol type
        /// </summary>
        /// <param name="protocolType">The <code>Protocol</code> to serialize</param>
        /// <param name="serializedProtocolType">The serialized <code>ROSProtocolType</code></param>
        protected void SerializeROSProtocolType(
            Protocol protocolType, ref ROSProtocolType serializedProtocolType)
        {
            // ROS protocol
            switch (protocolType)
            {
                case Protocol.WebSocketSharp:
                    serializedProtocolType = ROSProtocolType.WebSocketSharp;
                    break;

                case Protocol.WebSocketNET:
                default:
                    serializedProtocolType = ROSProtocolType.WebSocketDotNET;
                    break;
            }
        }

        /// <summary>
        /// Serializes the ROS serilizer type
        /// </summary>
        /// <param name="serializerType">The <code>SerializerEnum</code> to serialize</param>
        /// <param name="serializedSerializerType">The serialized <code>ROSSerializerType</code></param>
        public static void SerializeROSSerializerType(
            RosSocket.SerializerEnum serializerType, ref ROSSerializerType serializedSerializerType)
        {
            // ROS serializer
            switch (serializerType)
            {
                case RosSocket.SerializerEnum.Newtonsoft_BSON:
                    serializedSerializerType = ROSSerializerType.Newtonsoft_BSON;
                    break;

                case RosSocket.SerializerEnum.Newtonsoft_JSON:
                    serializedSerializerType = ROSSerializerType.Newtonsoft_JSON;
                    break;

                case RosSocket.SerializerEnum.Microsoft:
                default:
                    serializedSerializerType = ROSSerializerType.Microsoft;
                    break;
            }
        }
#endif

        /// <seealso cref="ClientInterface{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(ROSInterfaceType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the ROS settings

            // ROS Robot Description
            serialized.RobotDescription = RobotDescription;

            // ROS Joint State Subscriber
            serialized.JointStateSubscriberTopic = JointStateSubscriberTopic;

            // ROS Joint State Publisher
            serialized.JointStatePublisherTopic = JointStatePublisherTopic;

            // ROS protocol
#if MRET_EXTENSION_ROSSHARP
            ROSProtocolType protocol = serialized.Protocol;
            SerializeROSProtocolType(Protocol, ref protocol);
            serialized.Protocol = protocol;
#endif

#if MRET_EXTENSION_ROSSHARP
            // ROS serializer
            ROSSerializerType serializer = serialized.Serializer;
            SerializeROSSerializerType(Serializer, ref serializer);
            serialized.Serializer = serializer;
#endif

            // Save the final serialized reference
            serializedROSInterface = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
#endregion Serializable

#region ClientInterface
        /// <seealso cref="ClientInterface{T}.GetConnectionProtocol"/>
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

            // Make sure to trim off excess whitespace
            server = server.Trim();

            // Add the protocol
            if (!server.StartsWith(DEFAULT_PROTOCOL))
            {
                server = DEFAULT_PROTOCOL + server;
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
#if MRET_EXTENSION_ROSSHARP
                // Disconnect
                RosSocket.Close();
#endif

                // Mark as successful
                result = true;
            }
            catch (System.Exception e)
            {
                LogWarning("A problem occurred attempting to close the ROS socket: " + e.Message, nameof(PerformDisconnect));
            }

            return result;
        }

        /// <seealso cref="ClientInterface{T}.PerformConnect(string, X509Certificate2, X509Certificate2)"/>
        protected override bool PerformConnect(string serverConnection, X509Certificate2 serverCert = null, X509Certificate2 clientCert = null)
        {
            bool result = false;

            // Attempt the connection
            try
            {
                Log("Connecting to ROS...", nameof(PerformConnect));

                // Save the URL
                RosBridgeServerUrl = serverConnection;

                // Thread the connection
                ConnectionEstablished = new ManualResetEvent(false);
                new Thread(AsynchronousConnect).Start();
            }
            catch (Exception e)
            {
                LogWarning("A problem occurred initializing the ROS connection: " + e.Message);
            }

            return result;
        }
#endregion ClientInterface

        protected void AsynchronousConnect()
        {
#if MRET_EXTENSION_ROSSHARP
            // Setup the ROS bridge socket
            RosSocket = ConnectToRos(Protocol, RosBridgeServerUrl, OnConnect, OnDisconnect, Serializer);

            if (!ConnectionEstablished.WaitOne((int) (ConnectionTimeout * 1000f)))
            {
                LogWarning("Failed to connect to RosBridge at: " + RosBridgeServerUrl, nameof(AsynchronousConnect));
            }
#else
            LogWarning("RosSharp is unavailable.");
#endif
        }

#if MRET_EXTENSION_ROSSHARP
        public static RosSocket ConnectToRos(Protocol protocol, string serverUrl, EventHandler onConnect = null, EventHandler onDisconnect = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.Microsoft)
        {
            IProtocol protocolInitializer = ProtocolInitializer.GetProtocol(protocol, serverUrl);

            // Setup the events
            protocolInitializer.OnConnected += onConnect;
            protocolInitializer.OnClosed += onDisconnect;

            return new RosSocket(protocolInitializer, serializer);
        }
#endif

        private void OnConnect(object sender, EventArgs e)
        {
            ConnectionEstablished.Set();
            Log("Connected to RosBridge: " + RosBridgeServerUrl, nameof(OnConnect));
            Connected = true;
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            ConnectionEstablished.Reset();
            Log("Disconnected from RosBridge: " + RosBridgeServerUrl, nameof(OnDisconnect));
            Connected = false;
        }
    }
}