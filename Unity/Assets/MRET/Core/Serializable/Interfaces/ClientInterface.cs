// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.Interfaces
{
    /// <remarks>
    /// History:
    /// 3 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// Base scene object.<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class ClientInterface<T> : ThirdPartyInterface<T>, IClientInterface<T>
        where T : ClientInterfaceType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ClientInterface<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedClientInterface;

        #region IClientInterface
        /// <seealso cref="IClientInterface.DISCONNECT_ERROR"/>
        public string DISCONNECTED_ERROR
        {
            get => _disconnectError;
            protected set => _disconnectError = value;
        }
        private string _disconnectError = "Disconnected";

        /// <seealso cref="IClientInterface.CreateSerializedType"/>
        ClientInterfaceType IClientInterface.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IClientInterface.Server"/>
        public string Server { get; set; }

        /// <seealso cref="IClientInterface.Port"/>
        public int Port { get; set; }

        /// <seealso cref="IClientInterface.UsesCertificates"/>
        public bool UsesCertificates { get; protected set; }

        /// <seealso cref="IClientInterface.ConnectionTimeout"/>
        public float ConnectionTimeout { get; set; }

        /// <seealso cref="IClientInterface.Connected"/>
        public bool Connected { get; protected set; }

        /// <summary>
        /// Returns the server for connection string. Available for subclasses
        /// to override to provide formatting unique to the client, i.e. adding
        /// a prefix or combining "server:port".
        /// </summary>
        /// <returns>The full server connection string</returns>
        public virtual string GetConnectionProtocol()
        {
            return Server;
        }

        /// <summary>
        /// Called to connect to the server.
        /// </summary>
        public void Connect()
        {
            if (Connected)
            {
                LogWarning("Already connected", nameof(Connect));
                return;
            }

            // Perform the connection
            try
            {
                X509Certificate2 serverCert = null;
                X509Certificate2 clientCert = null;
                if (UsesCertificates)
                {
                    // Check for valid certs
                    serverCert = ProjectManager.InterfaceManager.GetServerCert(Server);
                    if (serverCert == null)
                    {
                        string errorMessage = "Server certificate is not registered. Please register a valid server cert for server: " + Server;
                        throw new ServerConnectionException(errorMessage);
                    }
                    clientCert = ProjectManager.InterfaceManager.GetClientCert(Server);
                    if (clientCert == null)
                    {
                        string errorMessage = "Client certificate is not registered. Please register a valid client cert for server: " + Server;
                        throw new ClientConnectionException(errorMessage);
                    }
                }

                // Obtain the connection protocol
                string serverConnection = GetConnectionProtocol();

                // Perform the connect
                Connected = PerformConnect(serverConnection, serverCert, clientCert);

                // Log a warning if the connection failed
                if (Connected)
                {
                    Log("Successfully connected to server: " + serverConnection, nameof(Connect));
                }
                else
                {
                    LogWarning("A problem occurred connecting to the server: " + serverConnection, nameof(Connect));
                }
            }
            catch (ServerConnectionException e)
            {
                // This sort of error probably has to do with the xml, or choosing the wrong server CA
                LogError("An issue with the server connection was encountered. Connection failed: " + e.Message,
                    nameof(Connect));
                ProjectManager.InterfaceManager.ReportError((this, e));
            }
            catch (ClientConnectionException e)
            {
                // This sort of error probably has to do with the xml, or choosing the wrong user certification
                LogError("An issue with the client connection was encountered. Connection failed: " + e.Message,
                    nameof(Connect));
                ProjectManager.InterfaceManager.ReportError((this, e));
            }
            catch (Exception e)
            {
                // This handles all other exceptions
                string message = "An issue establishing the connection was encountered. Connection failed: " + e.Message;
                ClientInterfaceException exception = new ClientInterfaceException(message);

                LogError("Connection failed: " + e.Message, nameof(Connect));
                ProjectManager.InterfaceManager.ReportError((this, exception));
            }
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            if (!Connected)
            {
                LogWarning("Not connected", nameof(Disconnect));
                return;
            }

            // Disconnect
            bool disconnected = PerformDisconnect();

            // Log a warning if the disconnect failed
            if (disconnected)
            {
                Log("Successfully disconnected from server: " + Server, nameof(Disconnect));
            }
            else
            {
                LogWarning("A problem occurred disconnecting from the server: " + Server, nameof(Disconnect));
            }

            // Mark as disconnected regardless of outcome
            Connected = false;
        }

        /// <seealso cref="IClientInterface.Deserialize(ClientInterfaceType, Action{bool, string})"/>
        void IClientInterface.Deserialize(ClientInterfaceType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IClientInterface.Serialize(ClientInterfaceType, Action{bool, string})"/>
        void IClientInterface.Serialize(ClientInterfaceType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IClientInterface

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

            // Set the defaults
            Server = "";
            Port = 0;
            UsesCertificates = false;
            Connected = false;
            float connectionTimeout = float.NaN;
            SchemaUtil.DeserializeFrequency(ClientInterfaceDefaults.CONNECTION_TIMEOUT_FREQUENCY, ref connectionTimeout);
            ConnectionTimeout = connectionTimeout;
            float connectionUpdate = float.NaN;
            SchemaUtil.DeserializeFrequency(ClientInterfaceDefaults.CONNECTION_UPDATE_FREQUENCY, ref connectionUpdate);
            updateRate = UpdateFrequency.HzCustom;
            customRate = connectionUpdate;
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Make sure to disconnect if connected
            if (Connected)
            {
                Disconnect();
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <summary>
        /// Deserializes the supplied serialized certificates into a X.509 server cert
        /// </summary>
        /// <param name="serializedCertificates">The serialized <code>ClientCertificatesType</code></param>
        /// <param name="deserializationState">The <code>ObjectSerializationState</code> to
        ///     populate with the state of the deserialization. The object reference will contain
        ///     the created <code>X509Certificate2</code> on success, or null on failure.</param>
        /// <returns>An <code>IEnumerator</code> for coroutine reentrance</returns>
        protected IEnumerator DeserializeServerCertificate(ClientCertificatesType serializedCertificates, ObjectSerializationState<X509Certificate2> deserializationState)
        {
            X509Certificate2 result = null;

            try
            {
                // Obtain the server certificate
                switch (serializedCertificates.ItemElementName)
                {
                    case ItemChoiceType.ServerCertificate:
                        if (string.IsNullOrEmpty(serializedCertificates.Item))
                        {
                            goto case ItemChoiceType.ServerCertificateStore;
                        }
                        Log("Deserializing X.509 server CA cert from XML", nameof(DeserializeServerCertificate));
                        result = ProjectManager.InterfaceManager.CertStringToCert(serializedCertificates.Item);
                        break;
                    case ItemChoiceType.ServerCertificateFile:
                        if (string.IsNullOrEmpty(serializedCertificates.Item))
                        {
                            goto case ItemChoiceType.ServerCertificateStore;
                        }
                        Log("Deserializing X.509 server CA from a file: " + serializedCertificates.Item, nameof(DeserializeServerCertificate));
                        string caFile = ProjectManager.InterfaceManager.GetFullCertPath(serializedCertificates.Item);
                        result = ProjectManager.InterfaceManager.CertPathToCert(caFile);
                        break;
                    case ItemChoiceType.ServerCertificateStore:
                        if (string.IsNullOrEmpty(serializedCertificates.Item))
                        {
                            serializedCertificates.Item = ProjectManager.InterfaceManager.ServerCertificateStore;
                        }
                        // This could be a custom cert store, so let's load it and just pull
                        // the entry for this server. If found, it will be added to our master
                        // list of certs later.
                        Log("Deserializing X.509 server CA cert from a store: " + serializedCertificates.Item, nameof(DeserializeServerCertificate));
                        string jsonFile = ProjectManager.InterfaceManager.GetFullCertPath(serializedCertificates.Item);
                        Dictionary<string, X509Certificate2> certs = ProjectManager.InterfaceManager.LoadJsonCertStore(jsonFile);
                        certs.TryGetValue(Server, out result);
                        break;
                }

                // Log a loading status message
                if (result == null)
                {
                    // Still valid, but user will be prompted
                    LogWarning("X.509 server CA cert not specified", nameof(DeserializeServerCertificate));
                }
                else
                {
                    Log("X.509 server CA cert successfully loaded", nameof(DeserializeServerCertificate));
                }
                deserializationState.obj = result;
            }
            catch (Exception e)
            {
                deserializationState.Error("Connection failed due to a problem with the server CA cert: " + e.ToString());
                yield break;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Deserializes the supplied serialized certificates into a X.509 client cert
        /// </summary>
        /// <param name="serializedCertificates">The serialized <code>ClientCertificatesType</code></param>
        /// <param name="deserializationState">The <code>ObjectSerializationState</code> to
        ///     populate with the state of the deserialization. The object reference will contain
        ///     the created <code>X509Certificate2</code> on success, or null on failure.</param>
        /// <returns>An <code>IEnumerator</code> for coroutine reentrance</returns>
        protected IEnumerator DeserializeClientCertificate(ClientCertificatesType serializedCertificates, ObjectSerializationState<X509Certificate2> deserializationState)
        {
            X509Certificate2 result = null;

            try
            {
                // Obtain the client certificate
                switch (serializedCertificates.Item1ElementName)
                {
                    case Item1ChoiceType.ClientCertificate:
                        if (string.IsNullOrEmpty(serializedCertificates.Item1))
                        {
                            goto case Item1ChoiceType.ClientCertificateStore;
                        }
                        Log("Deserializing X.509 client cert from XML", nameof(DeserializeClientCertificate));
                        result = ProjectManager.InterfaceManager.CertStringToCert(serializedCertificates.Item1, X509KeyStorageFlags.PersistKeySet);
                        break;
                    case Item1ChoiceType.ClientCertificateFile:
                        if (string.IsNullOrEmpty(serializedCertificates.Item1))
                        {
                            goto case Item1ChoiceType.ClientCertificateStore;
                        }
                        Log("Deserializing X.509 client cert from a file: " + serializedCertificates.Item1, nameof(DeserializeClientCertificate));
                        string caFile = ProjectManager.InterfaceManager.GetFullCertPath(serializedCertificates.Item1);
                        result = ProjectManager.InterfaceManager.CertPathToCert(caFile, X509KeyStorageFlags.PersistKeySet);
                        break;
                    case Item1ChoiceType.ClientCertificateStore:
                        if (string.IsNullOrEmpty(serializedCertificates.Item1))
                        {
                            serializedCertificates.Item1 = ProjectManager.InterfaceManager.ClientCertificateStore;
                        }
                        // This could be a custom cert store, so let's load it and just pull
                        // the entry for this server. If found, it will be added to our master
                        // list of certs later.
                        Log("Deserializing X.509 client cert from a store: " + serializedCertificates.Item1, nameof(DeserializeClientCertificate));
                        string jsonFile = ProjectManager.InterfaceManager.GetFullCertPath(serializedCertificates.Item1);
                        Dictionary<string, X509Certificate2> certs = ProjectManager.InterfaceManager.LoadJsonCertStore(jsonFile, X509KeyStorageFlags.PersistKeySet);
                        certs.TryGetValue(Server, out result);
                        break;
                }

                // Log a loading status message
                if (result == null)
                {
                    // Still valid, but user will be prompted
                    LogWarning("X.509 client cert not specified", nameof(DeserializeClientCertificate));
                }
                else
                {
                    Log("X.509 client cert successfully loaded", nameof(DeserializeClientCertificate));
                }
                deserializationState.obj = result;
            }
            catch (Exception e)
            {
                deserializationState.Error("Connection failed due to a problem with the client cert: " + e.ToString());
                yield break;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="ThirdPartyInterface{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(T serialized, SerializationState deserializationState)
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
            serializedClientInterface = serialized;

            // Deserialize the connection settings
            Server = serializedClientInterface.Server;
            Port = serializedClientInterface.Port;

            // Deserialize the connection timeout frequency
            if (serializedClientInterface.ConnectionTimeoutFrequency != null)
            {
                float connectionTimeout = float.NaN;
                SchemaUtil.DeserializeFrequency(serializedClientInterface.ConnectionTimeoutFrequency, ref connectionTimeout);
                if (!float.IsNaN(connectionTimeout))
                {
                    ConnectionTimeout = connectionTimeout;
                }
            }

            // Deserialize the update frequency
            if (serializedClientInterface.ConnectionUpdateFrequency != null)
            {
                updateRate = UpdateFrequency.HzCustom;
                customRate = serializedClientInterface.ConnectionUpdateFrequency.Value;
            }

            // Client Certificates (optional)
            UsesCertificates = false;
            if (serializedClientInterface.Certificates != null)
            {
                // Server cert
                {
                    ObjectSerializationState<X509Certificate2> serverCertDeserializationState = new ObjectSerializationState<X509Certificate2>();
                    StartCoroutine(DeserializeServerCertificate(serializedClientInterface.Certificates, serverCertDeserializationState));

                    // Wait for the coroutine to complete
                    while (!serverCertDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the deserialization state
                    deserializationState.Update(serverCertDeserializationState);

                    // If the deserialization failed, there's no point in continuing
                    if (deserializationState.IsError) yield break;

                    // Store the server cert
                    if (serverCertDeserializationState.obj != null)
                    {
                        ProjectManager.InterfaceManager.StoreServerCert(serverCertDeserializationState.obj);
                    }
                }

                // Client cert
                {
                    ObjectSerializationState<X509Certificate2> clientCertDeserializationState = new ObjectSerializationState<X509Certificate2>();
                    StartCoroutine(DeserializeClientCertificate(serializedClientInterface.Certificates, clientCertDeserializationState));

                    // Wait for the coroutine to complete
                    while (!clientCertDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the deserialization state
                    deserializationState.Update(clientCertDeserializationState);

                    // If the deserialization failed, there's no point in continuing
                    if (deserializationState.IsError) yield break;

                    // Store the client cert
                    if (clientCertDeserializationState.obj != null)
                    {
                        ProjectManager.InterfaceManager.StoreClientCert(clientCertDeserializationState.obj);
                    }
                }

                // Mark as using certificates
                UsesCertificates = true;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="ThirdPartyInterface{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the client connection settings
            serialized.Server = Server;
            serialized.Port = Port;

            // Serialize the connection timeout frequency (optional)
            FrequencyType serializedConnectionTimeoutFrequency = null;
            if (serializedClientInterface.ConnectionTimeoutFrequency != null)
            {
                serializedConnectionTimeoutFrequency = serializedClientInterface.ConnectionTimeoutFrequency;
                float connectionTimeout = ConnectionTimeout;
                SchemaUtil.SerializeFrequency(connectionTimeout, serializedConnectionTimeoutFrequency);
            }
            serialized.ConnectionTimeoutFrequency = serializedConnectionTimeoutFrequency;

            // Serialize the update frequency if custom
            // TODO: Possible candidate to move to SchemaUtil
            if (updateRate == UpdateFrequency.HzCustom)
            {
                if (serialized.ConnectionUpdateFrequency == null)
                {
                    serialized.ConnectionUpdateFrequency = new FrequencyType();
                }
                serialized.ConnectionUpdateFrequency.Value = customRate;
            }

            // Certificates
            ClientCertificatesType serializedCertificates = null;
            if (UsesCertificates)
            {
                // Create our serialized certificates struct
                serializedCertificates = new ClientCertificatesType();

                // Server cert
                {
                    // Start with our internal serialized client interface to serialize out the server cert
                    // using the original deserialized structure (if was provided during deserialization)
                    if ((serializedClientInterface != null) && (serializedClientInterface.Certificates != null) &&
                        !string.IsNullOrEmpty(serializedClientInterface.Certificates.Item) &&
                        !serializedClientInterface.Certificates.Item.Equals(ProjectManager.InterfaceManager.ServerCertificateStore))
                    {
                        // Use this server cert structure
                        serializedCertificates.Item = serializedClientInterface.Certificates.Item;
                        serializedCertificates.ItemElementName = serializedClientInterface.Certificates.ItemElementName;
                    }
                    else
                    {
                        // Default to the server store
                        serializedCertificates.Item = null;
                        serializedCertificates.ItemElementName = ItemChoiceType.ServerCertificateStore;
                    }
                }

                // Client cert
                {
                    // Start with our internal serialized client interface to serialize out the client cert
                    // using the original deserialized structure (if was provided during deserialization)
                    if ((serializedClientInterface != null) && (serializedClientInterface.Certificates != null) &&
                        !string.IsNullOrEmpty(serializedClientInterface.Certificates.Item1) &&
                        !serializedClientInterface.Certificates.Item1.Equals(ProjectManager.InterfaceManager.ClientCertificateStore))
                    {
                        // Use this server cert structure
                        serializedCertificates.Item1 = serializedClientInterface.Certificates.Item1;
                        serializedCertificates.Item1ElementName = serializedClientInterface.Certificates.Item1ElementName;
                    }
                    else
                    {
                        // Default to the client store
                        serializedCertificates.Item1 = null;
                        serializedCertificates.Item1ElementName = Item1ChoiceType.ClientCertificateStore;
                    }
                }
            }
            serialized.Certificates = serializedCertificates;

            // Save the final serialized reference
            serializedClientInterface = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <summary>
        /// Called to connect to the server.
        /// </summary>
        /// <param name="serverConnection">The server connection protocol</param>
        /// <param name="serverCert">The optional server certificate supplied when
        ///     <code>UsesCertificates</code> is true</param>
        /// <param name="clientCert">The optional client certificate supplied when
        ///     <code>UsesCertificates</code> is true</param>
        /// <returns>An indicator the connection was established</returns>
        /// <see cref="UsesCertificates"/>
        protected abstract bool PerformConnect(string serverConnection, X509Certificate2 serverCert = null, X509Certificate2 clientCert = null);

        /// <summary>
        /// Called to disconnect from the server.
        /// </summary>
        protected abstract bool PerformDisconnect();

    }

    public class ClientInterfaceDefaults
    {
        // We want to use the default values from the schema to keep in sync
        // TODO: CODEGEN defaults not supported for FrequencyType by current version of XSD Code Generator 
        public static readonly FrequencyType CONNECTION_TIMEOUT_FREQUENCY = new FrequencyType() { Value = 0.1f }; // new ClientInterfaceType().ConnectionTimeoutFrequency;
        public static readonly FrequencyType CONNECTION_UPDATE_FREQUENCY = new FrequencyType() { Value = 1f }; // new ClientInterfaceType().ConnectionUpdateFrequency;
    }
}
