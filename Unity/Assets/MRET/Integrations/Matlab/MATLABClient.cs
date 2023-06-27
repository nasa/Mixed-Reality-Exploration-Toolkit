// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.Matlab
{
    public class MATLABClient : ClientInterface<MatlabInterfaceType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MATLABClient);

        public string DEFAULT_SERVER = "127.0.0.1";
        public int DEFAULT_PORT = 25525;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private MatlabInterfaceType serializedMatlabInterface;

        public MATLABCommandHandler commandHandler { get; private set; }

        public bool send
        {
            get
            {
                return commandHandler.sendingEnabled;
            }
            set
            {
                if (value)
                {
                    commandHandler.EnableSender();
                }
                else
                {
                    commandHandler.DisableSender();
                }
            }
        }

        public bool receive
        {
            get
            {
                return commandHandler.receivingEnabled;
            }
            set
            {
                if (value)
                {
                    commandHandler.EnableReceiver();
                }
                else
                {
                    commandHandler.DisableReceiver();
                }
            }
        }

        private TcpClient client;
        private NetworkStream stream;

        #region MRETBehaviour
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

        protected override void MRETStart()
        {
            base.MRETStart();

            // Create our serializable GMSEC object
            commandHandler = gameObject.GetComponent<MATLABCommandHandler>();
            if (commandHandler == null)
            {
                commandHandler = gameObject.AddComponent<MATLABCommandHandler>();
            }

            // Default command handler settings
            commandHandler.matlabClient = this;
        }

        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // TODO: Make sure the command handler is sharing our update frequency
            // until this design is revisited for only one update implementation
            commandHandler.updateRate = updateRate;
            commandHandler.customRate = customRate;
        }

        #endregion MRETBehaviour

        #region Serializable
        /// <seealso cref="ClientInterface{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(MatlabInterfaceType serialized, SerializationState deserializationState)
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
            serializedMatlabInterface = serialized;

            // Deserialize the command type
            SchemaUtil.DeserializeMatlabCommandType(serialized.CommandType, ref commandHandler.commandType);

            // Deserialize the Matlab client settings
            send = serialized.Send;
            receive = serialized.Receive;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="ClientInterface{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(MatlabInterfaceType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize out the command type
            MatlabCommandType commandType = serialized.CommandType;
            SchemaUtil.SerializeMatlabCommandType(commandHandler.commandType, ref commandType);
            serialized.CommandType = commandType;

            // Serialize the Matlab client settings
            serialized.Send = send;
            serialized.Receive = receive;

            // Save the final serialized reference
            serializedMatlabInterface = serialized;

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
                // Disconnect
                DestroyConnection();

                // Mark as sucecessful
                result = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[" + ClassName + "]->Disconnect: A problem occurred attempting to disconnect from Matlab: " + e.Message);
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
                // Set up TCPClient.
                client = new TcpClient(serverConnection, Port);

                // Connect to server.
                byte[] data = System.Text.Encoding.UTF8.GetBytes("Connecting\n");
                stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Debug.Log("[" + ClassName + "] Connected!");

                // Mark as connected
                result = true;
            }
            catch (Exception e)
            {
                Debug.Log("[" + ClassName + "] A problem was encountered connecting to Matlab: " + e.ToString());
            }

            return result;
        }
        #endregion ClientInterface

        public void Send(string content)
        {
            SendString(content);
        }

        public string Receive()
        {
            return ReceiveString();
        }

        public LegacyMATLABCommand ReceiveLegacyCommand()
        {
            if (client.Connected)
            {
                string[] receivedElements = ReceiveString().Split(new string[] { ":;" }, StringSplitOptions.None);
                if (receivedElements.Length > 1)
                {
                    LegacyMATLABCommand commandToReturn = new LegacyMATLABCommand();
                    commandToReturn.command = receivedElements[0];
                    commandToReturn.destination = receivedElements[1] == "." ? null : receivedElements[1];

                    commandToReturn.arguments = new List<string>();
                    for (int i = 2; i < receivedElements.Length; i++)
                    {
                        commandToReturn.arguments.Add(receivedElements[i]);
                    }
                    return commandToReturn;
                }
            }
            return null;
        }

        public JSONMATLABResponse ReceiveJSONResponse()
        {
            if (client.Connected)
            {
                string receivedCommand = ReceiveString();
                if (receivedCommand != null)
                {
                    return JSONMATLABResponse.FromJSON(receivedCommand);
                }
            }
            return null;
        }

        private void DestroyConnection()
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (client != null)
                {
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Debug.Log("[MATLABClient->DestroyConnection] " + e.ToString());
            }
        }

        private void SendString(string stringToSend)
        {
            try
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(stringToSend + "\n");
                stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Debug.Log("[MATLABClient->SendString] Sent!");
            }
            catch (Exception e)
            {
                Debug.Log("[MATLABClient->SendString] " + e.ToString());
            }
        }

        private string ReceiveString()
        {
            string returnString = "*error*";
            try
            {
                byte[] data = new byte[1024];
                stream = client.GetStream();
                stream.ReadTimeout = 500;

                int bytes = stream.Read(data, 0, data.Length);
                returnString = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                Debug.Log("got " + returnString);
                Debug.Log("[MATLABClient->ReceiveString] Received!");
            }
            catch (Exception e)
            {
                Debug.Log("[MATLABClient->ReceiveString] " + e.ToString());
            }

            return returnString;
        }
    }
}