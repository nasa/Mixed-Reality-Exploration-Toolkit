// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Concurrent;
using System.IO;
using GOV.NASA.GSFC.XR.Utilities.Json;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using GOV.NASA.GSFC.XR.MRET.UI.HUD;
using GOV.NASA.GSFC.XR.MRET.UI.Extensions.IoT;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Integrations.IoT.IoTManager) + " class")]
    public class IoTManagerDeprecated : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(IoTManagerDeprecated);
            }
        }

        public const string DEFAULT_SERVERS_FILE = ".iotServers";
        public const string DEFAULT_CLIENTS_FILE = ".iotClients";

        //This event instantiates a dialog box that describes the current connection error
        public GameObject connectionErrorEvent;
        //This object contains an IoTClientDeprecated script and is used for intantiating new connections
        public GameObject IoTConnectionPrefab;
        //This dictionary uses server names to find IoTClients
        public Dictionary<string, IoTClientDeprecated> IoTClients;
        //This dictionary uses ThingIDs to find IoTThingDeprecated objects
        public Dictionary<string, IoTThingDeprecated> IoTThings;
        //This queue stores active error prompts
        public Stack<GameObject> errorPrompts;
        public static IoTManagerDeprecated instance;
        public List<(string, string)> errorMessages;
        public ConcurrentQueue<(IoTClientDeprecated, Exception)> IoTClientErrors;
        public float timeBeforeError = 2;

        // Registered certificates keyed on server IP. Value is a cert
        private Dictionary<string, X509Certificate2> serverCerts;
        private Dictionary<string, X509Certificate2> clientCerts;

        private GameObject clientContainer;
        private GameObject errorContainer;

        private string serverCertificateStore;
        public string ServerCertificateStore
        {
            get => serverCertificateStore;
        }

        private string clientCertificateStore;
        public string ClientCertificateStore
        {
            get => clientCertificateStore;
        }

        //TODO:Relocate to MRET architecture
        public void DisplayConnectionError(IoTClientDeprecated client, Exception e)
        {
            if (e.Message == IoTClientDeprecated.DISCONNECTED_ERROR)
            {
                StartCoroutine(WaitForError(client));
            }
            else
            {
                errorMessages.Add((client.server, e.Message));
                string key = "GOV.NASA.GSFC.XR.MRET.IOT.ERROR";
                MRET.DataManager.SaveValue(new DataManager.DataValue(key, errorMessages));
                Vector3 offset = new Vector3(errorPrompts.Count * .05f, errorPrompts.Count * -.05f, 0);

                //Instantiate the error dialog box and set it to follow the user
                GameObject newErrorPrompt = Instantiate(connectionErrorEvent, errorContainer.transform);
                //Let the error prompt know the name of the server it is related to
                IoTErrorPromptDeprecated errorPrompt = newErrorPrompt.GetComponent<IoTErrorPromptDeprecated>();
                errorPrompt.client = client;
                if (e is IoTServerConnectionExceptionDeprecated)
                {
                    errorPrompt.Source = IoTErrorPromptDeprecated.ErrorSourceType.Server;
                }
                else
                {
                    errorPrompt.Source = IoTErrorPromptDeprecated.ErrorSourceType.Client;
                }
                if (errorPrompts.Count == 0)
                {
                    errorPrompt.SetInteractable(true);
                }
                else
                {
                    errorPrompt.SetInteractable(false);
                }
                newErrorPrompt.GetComponent<FrameHUD>().offset += offset;

                errorPrompts.Push(newErrorPrompt);
            }
        }

        //This is a coroutine that waits after a disconnect for a set duration before throwing an error
        private IEnumerator WaitForError(IoTClientDeprecated client)
        {
            yield return new WaitForSeconds(timeBeforeError);

            if (client != null)
            {
                if (!client.IsConnected)
                {
                    IoTClientErrors.Enqueue((client, new IoTClientConnectionExceptionDeprecated("Connection with " + client.server + " unexpectedly disconnected")));
                    Log("IoT Connection with: " + client.server + " disconnected", nameof(WaitForError));
                }
            }
        }

        public void CloseAllErrorPrompts()
        {
            foreach (GameObject prompt in errorPrompts)
            {
                Destroy(prompt);
            }
            errorPrompts.Clear();
        }

        //remove the topmost errorprompt
        public void RemoveTopErrorPrompt()
        {
            errorMessages.RemoveAt(0);
            GameObject topPrompt = errorPrompts.Pop();
            Destroy(topPrompt);
            SetFirstErrorPromptInteractable(true);
        }

        //set the interactability of the topmost error prompt
        private void SetFirstErrorPromptInteractable(bool interactable)
        {
            if (errorPrompts.Count > 0)
            {
                GameObject topPromptObject = errorPrompts.Peek();
                IoTErrorPromptDeprecated errorPrompt = topPromptObject.GetComponent<IoTErrorPromptDeprecated>();
                errorPrompt.SetInteractable(interactable);
            }
        }

        //Have Each IoTClientDeprecated connect, and create the testing output box.
        //If there is an error with the certificates create a dialog box saying so
        public void Connect()
        {
            if (IoTClients.Keys.Count > 0)
            {
                IoTClientDeprecated exceptionClient = null;
                try
                {
                    //clear current error messages so new ones can replace them
                    errorMessages = new List<(string, string)>();

                    //ThreadStart childref = new ThreadStart(CallToChildThread);
                    foreach (IoTClientDeprecated client in IoTClients.Values)
                    {
                        // Make sure we aren't already connected
                        if (!client.IsConnected)
                        {
                            exceptionClient = client;
                            Thread childThread = new Thread(IoTThreadConnect);
                            childThread.Start(client);
                        }
                    }

                    //TODO: remove this line and the variable outputBox since they are purely for testing:
                    //Instantiate(outputBox);
                }
                catch (Exception e)
                {
                    IoTClientErrors.Enqueue((exceptionClient, e));
                }
            }
        }

        //Connect to a specified server
        public void ConnectToServer(string server)
        {
            if (IoTClients.Keys.Count > 0)
            {
                IoTClientDeprecated exceptionClient = null;
                try
                {
                    IoTClientDeprecated client;
                    if (IoTClients.TryGetValue(server, out client))
                    {
                        exceptionClient = client;
                        Thread childThread = new Thread(IoTThreadConnect);
                        childThread.Start(client);
                    }
                }

                catch (Exception e)
                {
                    IoTClientErrors.Enqueue((exceptionClient, e));
                }
            }
        }

        private static void IoTThreadConnect(object client)
        {
            ((IoTClientDeprecated)client).Connect();
        }

        public bool GetThing(string ID, out IoTThingDeprecated output)
        {
            return IoTThings.TryGetValue(ID, out output);
        }

        public void AddThings(IoTThingsType iotThings)
        {
            if (iotThings.Thing != null)
            {
                foreach (IoTThingType IoTThings in iotThings.Thing)
                {
                    AddThing(IoTThings.ID, IoTThings.Name, IoTThings.Description, IoTThings.Payload);
                }
            }
        }

        //Add an IoTThingDeprecated to the hashtable so IoTClients can request it
        public void AddThing(string ID, string name, string description, IoTThingPayloadType payload)
        {
            IoTThingDeprecated thing = null;
            if (payload.Item is IoTThingPayloadPairsType)
            {
                IoTThingPayloadPairsType item = (IoTThingPayloadPairsType)payload.Item;
                thing = new IoTThingDeprecated(ID, name, description, true, new Dictionary<string, IoTThingValueDeprecated>());
                foreach (IoTThingPayloadPairType pair in item.Pair)
                {
                    string key = pair.Key;
                    IoTThingValueDeprecated value = GetIoTThingValue(pair.Value);
                    thing.AddPairsElement(key, value);
                }
            }
            else if (payload.Item is IoTThingPayloadValueType)
            {
                IoTThingPayloadValueType item = (IoTThingPayloadValueType)payload.Item;
                if (item != null)
                {
                    thing = new IoTThingDeprecated(ID, name, description, false, GetIoTThingValue(item));
                }
            }
            if (thing != null)
            {
                if (IoTThings.ContainsKey(ID))
                {
                    IoTThings.Remove(ID);
                }
                IoTThings.Add(ID, thing);
            }
        }

        //convert an IoTThingPayloadValueType to an IoTThingValueDeprecated
        private IoTThingValueDeprecated GetIoTThingValue(IoTThingPayloadValueType item)
        {
            IoTThingValueDeprecated thingValue = new IoTThingValueDeprecated();
            float thingLow = float.MinValue, thingHigh = float.MaxValue;
            thingValue.SetType(item.Type);
            if (item.Thresholds == null)
            {
                thingValue.defaultColor = ColorType.Black;
                thingValue.thresholds = new List<(float, float, ColorType)>() { (thingLow, thingHigh, ColorType.Black) };
            }
            else
            {
                thingValue.defaultColor = item.Thresholds.Color;
                thingValue.thresholds = new List<(float, float, ColorType)>();
                foreach (IoTThingPayloadValueThresholdType threshold in item.Thresholds.Threshold)
                {
                    thingLow = float.MinValue;
                    thingHigh = float.MaxValue;
                    if (threshold.LowSpecified)
                    {
                        thingLow = threshold.Low;
                    }
                    if (threshold.HighSpecified)
                    {
                        thingHigh = threshold.High;
                    }
                    thingValue.thresholds.Add((thingLow, thingHigh, threshold.Color));
                }
            }
            return thingValue;
        }

        public void ClearConnections()
        {
            if (IoTClients != null)
            {
                foreach (IoTClientDeprecated client in IoTClients.Values)
                {
                    Destroy(client);
                }
                IoTClients.Clear();
            }
            if (errorMessages != null)
            {
                errorMessages.Clear();
            }
            if (errorPrompts != null)
            {
                errorPrompts.Clear();
            }
        }

        private string GetFullPath(string file)
        {
            string result = file;

            if (!File.Exists(result))
            {
                // It's not explicit so see if it's relative
                if (!Path.IsPathRooted(result))
                {
                    // Relative location so try the user directory
                    string userPath = Path.Combine(MRET.ConfigurationManager.defaultUserDirectory, result);
                    if (File.Exists(userPath))
                    {
                        result = Path.GetFullPath(userPath);
                    }
                    else
                    {
                        throw new FileNotFoundException("Supplied file does not exist: " + result);
                    }
                }
                else
                {
                    throw new FileNotFoundException("Supplied file does not exist: " + result);
                }
            }

            return result;
        }

        /// <summary>
        /// Deserializes the supplied IoT connection into a X.509 server cert
        /// </summary>
        /// <param name="iotConnection">The serialized <code>IoTConnectionType</code></param>
        /// <returns>The created <code>X509Certificate2</code></returns>
        protected X509Certificate2 DeserializeServerCertificate(IoTConnectionType iotConnection)
        {
            X509Certificate2 result = null;

            try
            {
                // Obtain the server certificate
                switch (iotConnection.ItemElementName)
                {
                    case ItemChoiceType.ServerCertificate:
                        Log("Deserializing X.509 server CA cert from XML", nameof(DeserializeClientCertificate));
                        result = CertStringToCert(iotConnection.Item);
                        break;
                    case ItemChoiceType.ServerCertificateFile:
                        Log("Deserializing X.509 server CA from a file: " + iotConnection.Item1, nameof(DeserializeClientCertificate));
                        string caFile = GetFullPath(iotConnection.Item);
                        result = CertPathToCert(caFile);
                        break;
                    case ItemChoiceType.ServerCertificateStore:
                        if (string.IsNullOrEmpty(iotConnection.Item))
                        {
                            iotConnection.Item = ServerCertificateStore;
                        }
                        // This could be a custom cert store, so let's load it and just pull
                        // the entry for this server. If found, it will be added to our master
                        // list of certs later.
                        Log("Deserializing X.509 server CA cert from a store: " + iotConnection.Item1, nameof(DeserializeClientCertificate));
                        string jsonFile = GetFullPath(iotConnection.Item);
                        Dictionary<string, X509Certificate2> certs = LoadJsonCertStore(jsonFile);
                        certs.TryGetValue(iotConnection.Server, out result);
                        break;
                }

                // Log a loading status message
                if (result == null)
                {
                    LogWarning("X.509 server CA cert not specified", nameof(DeserializeServerCertificate));
                }
                else
                {
                    Log("X.509 server CA cert successfully loaded", nameof(DeserializeServerCertificate));
                }
            }
            catch (Exception e)
            {
                LogError("Connection failed due to a problem with the server CA cert: " + e.ToString(), nameof(DeserializeServerCertificate));
            }

            return result;
        }

        /// <summary>
        /// Deserializes the supplied IoT connection into a X.509 client cert
        /// </summary>
        /// <param name="iotConnection">The serialized <code>IoTConnectionType</code></param>
        /// <returns>The created <code>X509Certificate2</code></returns>
        protected X509Certificate2 DeserializeClientCertificate(IoTConnectionType iotConnection)
        {
            X509Certificate2 result = null;

            try
            {
                // Obtain the client certificate
                switch (iotConnection.Item1ElementName)
                {
                    case Item1ChoiceType.ClientCertificate:
                        Log("Deserializing X.509 client cert from XML", nameof(DeserializeClientCertificate));
                        result = CertStringToCert(iotConnection.Item1, X509KeyStorageFlags.PersistKeySet);
                        break;
                    case Item1ChoiceType.ClientCertificateFile:
                        Log("Deserializing X.509 client cert from a file: " + iotConnection.Item1, nameof(DeserializeClientCertificate));
                        string caFile = GetFullPath(iotConnection.Item1);
                        result = CertPathToCert(caFile, X509KeyStorageFlags.PersistKeySet);
                        break;
                    case Item1ChoiceType.ClientCertificateStore:
                        if (string.IsNullOrEmpty(iotConnection.Item1))
                        {
                            iotConnection.Item1 = ClientCertificateStore;
                        }
                        // This could be a custom cert store, so let's load it and just pull
                        // the entry for this server. If found, it will be added to our master
                        // list of certs later.
                        Log("Deserializing X.509 client cert from a store: " + iotConnection.Item1, nameof(DeserializeClientCertificate));
                        string jsonFile = GetFullPath(iotConnection.Item1);
                        Dictionary<string, X509Certificate2> certs = LoadJsonCertStore(jsonFile, X509KeyStorageFlags.PersistKeySet);
                        certs.TryGetValue(iotConnection.Server, out result);
                        break;
                }

                // Log a loading status message
                if (result == null)
                {
                    LogWarning("X.509 client cert not specified", nameof(DeserializeClientCertificate));
                }
                else
                {
                    Log("X.509 client cert successfully loaded", nameof(DeserializeClientCertificate));
                }
            }
            catch (Exception e)
            {
                LogError("Connection failed due to a problem with the client cert: " + e.ToString(), nameof(DeserializeClientCertificate));
            }

            return result;
        }

        /// <summary>
        /// Adds the supplied serialized connection
        /// </summary>
        /// <param name="iotConnection"></param>
        public void AddConnection(IoTConnectionType iotConnection)
        {
            if (!IoTConnectionPrefab)
            {
                LogError("Failed to add connection because the IoTConnectionPrefab is null", nameof(AddConnection));
                return;
            }

            // Instantiate the prefab for our clients
            GameObject iotConnectionObject = Instantiate(IoTConnectionPrefab, clientContainer.transform);

            // Make sure we reset the local transform to zero
            iotConnectionObject.transform.localPosition = Vector3.zero;
            iotConnectionObject.transform.localRotation = Quaternion.identity;

            // Obtain the IoTClientDeprecated reference
            IoTClientDeprecated iotClient = iotConnectionObject.gameObject.GetComponent<IoTClientDeprecated>();
            if (!iotClient)
            {
                LogError("Failed to add connection because the IoTConnectionPrefab does not contain an IoTClientDeprecated", nameof(AddConnection));
                Destroy(iotConnectionObject);
                return;
            }

            try
            {
                // Create the certs
                X509Certificate2 serverCert = DeserializeServerCertificate(iotConnection);
                if (serverCert != null)
                {
                    // Save the cert
                    StoreCert(serverCerts, serverCert);
                }

                X509Certificate2 clientCert = DeserializeClientCertificate(iotConnection);
                if (clientCert != null)
                {
                    // Save the cert
                    StoreCert(clientCerts, clientCert);
                }

                // Add the connection
                Add(iotClient, iotConnection.Name, iotConnection.Description,
                    iotConnection.Server, iotConnection.Port, iotConnection.Topics);
            }
            catch (Exception e)
            {
                LogError("Connection failed due to a problem with the specified cert: " + e.ToString(), nameof(AddConnection));
            }
        }

        /// <summary>
        /// Adds the supplied serialized connections
        /// </summary>
        /// <param name="iotConnections"></param>
        public void AddConnections(IoTConnectionsType iotConnections)
        {
            if (iotConnections.IoTConnection != null)
            {
                foreach (IoTConnectionType ioTConnection in iotConnections.IoTConnection)
                {
                    // Add the connection
                    AddConnection(ioTConnection);
                }
            }

            // Notify all the clients to connect
            Connect();
        }

        /// <summary>
        /// Add an IoTClientDeprecated that's already had its fields filled out
        /// </summary>
        /// <param name="client"></param>
        public void Add(IoTClientDeprecated client)
        {
            IoTClients.Add(client.server, client);
        }

        /// <summary>
        /// Create and Add an IoTClientDeprecated
        /// </summary>
        /// <param name="IoTClientDeprecated"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="topics"></param>
        /// <param name="CACert"></param>
        /// <param name="userCert"></param>
        public void Add(IoTClientDeprecated IoTClientDeprecated, string name, string description, string server, int port,
            IoTTopicType[] topics)
        {
            IoTClientDeprecated.name = name;
            IoTClientDeprecated.description = description;
            IoTClientDeprecated.server = server;
            IoTClientDeprecated.port = port;

            IoTClientDeprecated.topics = new Dictionary<string, IoTTopicType>();
            foreach (IoTTopicType element in topics)
            {
                IoTClientDeprecated.topics.Add(element.pattern, element);
            }

            IoTClients.Add(server, IoTClientDeprecated);
        }

        /// <summary>
        /// Adds the supplied server cert to the server cert store
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool AddServerCert(string file)
        {
            bool result = false;

            X509Certificate2 serverCert = null;
            try
            {
                // Create the cert
                serverCert = CertPathToCert(file);

                // Make sure we have a valid cert
                if (serverCert != null)
                {
                    // Store the cert
                    StoreCert(serverCerts, serverCert);

                    // Mark as successful
                    result = true;
                }
                else
                {
                    LogWarning("There was a problem creating the cert from the supplied file.",
                        nameof(AddServerCert));
                }
            }
            catch (Exception e)
            {
                LogWarning("There was a problem creating the cert from the supplied file: " +
                    e.ToString(), nameof(AddServerCert));
            }

            return result;
        }

        /// <summary>
        /// Adds the supplied client cert to the client cert store
        /// </summary>
        /// <param name="file">The cert file to add</param>
        /// <returns></returns>
        public bool AddClientCert(string file)
        {
            bool result = false;

            X509Certificate2 clientCert = null;
            try
            {
                // Create the cert
                clientCert = CertPathToCert(file, X509KeyStorageFlags.PersistKeySet);

                // Make sure we have a valid cert
                if (clientCert != null)
                {
                    // Store the cert
                    StoreCert(clientCerts, clientCert);

                    // Mark as successful
                    result = true;
                }
                else
                {
                    LogWarning("There was a problem creating the cert from the supplied file.",
                        nameof(AddClientCert));
                }
            }
            catch (Exception e)
            {
                LogWarning("There was a problem creating the cert from the supplied file: " +
                    e.ToString(), nameof(AddClientCert));
            }

            return result;
        }
        /// <summary>
        /// Obtains the registered X.509 cert for the supplied server key
        /// </summary>
        /// <param name="server">The server name for the client cert</param>
        /// <returns>The registered <code>X509Certificate2</code> representing the client cert
        ///     for the supplied server, or NULL if not found</returns>

        /// <summary>
        /// Obtains the X.509 cert for the supplied server key contained in the supplied cert store
        /// </summary>
        /// <param name="certStore">The <code>Dictionary<string, X509Certificate2></code> cert store to query</param>
        /// <param name="server">The server name for the X.509 cert to lookup</param>
        /// <returns>The <code>X509Certificate2</code> representing the cert
        ///     for the supplied server key, or NULL if not found</returns>
        protected X509Certificate2 GetCert(Dictionary<string, X509Certificate2> certStore, string server)
        {
            // Lookup the client cert
            X509Certificate2 result = null;
            certStore.TryGetValue(server, out result);

            return result;
        }

        /// <summary>
        /// Obtains the registered X.509 server cert for the supplied server key
        /// </summary>
        /// <param name="server">The server name for the server cert</param>
        /// <returns>The registered <code>X509Certificate2</code> representing the server cert
        ///     for the supplied server key, or NULL if not found</returns>
        public X509Certificate2 GetServerCert(string server)
        {
            // Lookup the server cert
            return GetCert(serverCerts, server);
        }

        /// <summary>
        /// Obtains the registered X.509 client cert for the supplied server key
        /// </summary>
        /// <param name="server">The server name for the client cert</param>
        /// <returns>The registered <code>X509Certificate2</code> representing the client cert
        ///     for the supplied server key, or NULL if not found</returns>
        public X509Certificate2 GetClientCert(string server)
        {
            // Lookup the client cert
            return GetCert(clientCerts, server);
        }

        /// <summary>
        /// Convert a certificate path to a certificate
        /// </summary>
        /// <param name="path"></param>
        /// <param name="keyStorageFlags"></param>
        /// <returns></returns>
        protected X509Certificate2 CertPathToCert(string path, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.DefaultKeySet)
        {
            return (new X509Certificate2(path, string.Empty, keyStorageFlags | X509KeyStorageFlags.Exportable));
        }

        /// <summary>
        /// Convert a certificate string to a certificate
        /// </summary>
        /// <param name="certStr"></param>
        /// <param name="keyStorageFlags"></param>
        /// <returns></returns>
        protected X509Certificate2 CertStringToCert(string certStr, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.DefaultKeySet)
        {
            // Remove the cert headers, if any, because they are not base64 characters and will error when converting
            string certData = Regex.Replace(Regex.Replace(certStr, @"\s+", string.Empty), @"-+[^-]+-+", string.Empty);

            // Create the X.509 cert. This will generate an exception if there's an issue
            X509Certificate2 result = new X509Certificate2(Convert.FromBase64String(certData), string.Empty, keyStorageFlags | X509KeyStorageFlags.Exportable);

            return result;
        }

        /// <summary>
        /// Convert a X.509 certificate to a string
        /// </summary>
        /// <param name="cert">The X.509 certificate as a <code>X509Certificate2</code></param>
        /// <param name="certContent">Identifies the content of the cert</param>
        /// <returns></returns>
        protected string CertToCertString(X509Certificate2 cert, X509ContentType certContent = X509ContentType.Cert)
        {
            string result = string.Empty;

            try
            {
                // Convert to a base64
                result = Convert.ToBase64String(cert.Export(certContent, string.Empty), Base64FormattingOptions.None);
            }
            catch (Exception e)
            {
                LogError("There was a problem converting the supplied cert to a string: " + e.ToString());
            }

            return result;
        }

        /// <summary>
        /// Stores the supplied X.509 certificate into the supplied certificate store
        /// </summary>
        /// <param name="certStore">The certificate store as a <code>Dictionary<string, X509Certificate2></code></param>
        /// <param name="cert">The <code>X509Certificate2</code> X.509 certificate to store</param>
        protected void StoreCert(Dictionary<string, X509Certificate2> certStore, X509Certificate2 cert)
        {
            string key = cert.GetNameInfo(X509NameType.SimpleName, true);

            // Remove an existing entry to avoid ArgumentException
            if (certStore.ContainsKey(key))
            {
                certStore.Remove(key);
            }

            // Add the new pair
            certStore.Add(key, cert);
        }

        /// <summary>
        /// Loads a JSON cert store and returns the X.509 certs as a dictionary keyed on server name.
        /// </summary>
        /// <param name="file">The cert store file to load</param>
        /// <param name="keyStorageFlags">The <code>X509KeyStorageFlags</code> indicating how the certs should be created</param>
        /// <returns>A <code>Dictionary<string, X509Certificate2></code> containing the loaded certs keyed on server name</returns>
        protected Dictionary<string, X509Certificate2> LoadJsonCertStore(string file, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.DefaultKeySet)
        {
            Dictionary<string, X509Certificate2> result = new Dictionary<string, X509Certificate2>();

            // Extract the contents of the file as text
            string jsonText = File.ReadAllText(file);

            // Deserialize the text to JSON
            Dictionary<string, string> jsonPairs;
            jsonPairs = JsonUtil.Deserialize<Dictionary<string, string>>(jsonText);
            foreach (KeyValuePair<string, string> jsonPair in jsonPairs)
            {
                // Convert the cert string to a cert and store it in the result
                X509Certificate2 cert = CertStringToCert(jsonPair.Value, keyStorageFlags);
                result.Add(jsonPair.Key, cert);
            }

            return result;
        }

        /// <summary>
        /// Saves the supplied cert dictionary to the specified cert store file.
        /// </summary>
        /// <param name="file">The cert store file</param>
        /// <param name="certPairs">The <code>Dictionary</code> of certs keyed on server name</param>
        /// <param name="certContent">The content of the cert pairs as a <code>X509ContentType</code></param>
        protected void SaveJsonCertStore(string file, Dictionary<string, X509Certificate2> certPairs, X509ContentType certContent = X509ContentType.Cert)
        {
            if (certPairs != null)
            {
                // Build the data to serialize to JSON
                Dictionary<string, string> jsonPairs = new Dictionary<string, string>();
                foreach (KeyValuePair<string, X509Certificate2> certPair in certPairs)
                {
                    // Convert the cert to a string
                    string certString = CertToCertString(certPair.Value, certContent);
                    jsonPairs.Add(certPair.Key, certString);
                }

                // Serialize thecontents of the file to text
                string json = JsonUtil.Serialize(jsonPairs);

                // Write the text to the file
                File.WriteAllText(file, json);
            }
            else
            {
                LogWarning("Supplied cert dictionary is null");
            }
        }

        /// <summary>
        /// Initializes this manager.
        /// </summary>
        public void Initialize()
        {
            instance = this;
            IoTClients = new Dictionary<string, IoTClientDeprecated>();
            errorMessages = new List<(string, string)>();
            IoTThings = new Dictionary<string, IoTThingDeprecated>();
            errorPrompts = new Stack<GameObject>();
            FrameHUD frameHUD = connectionErrorEvent.GetComponent<FrameHUD>();
            frameHUD.trackHeadset = MRET.InputRig.head.transform;
            IoTClientErrors = new ConcurrentQueue<(IoTClientDeprecated, Exception)>();

            // Create a container for IotClients
            clientContainer = new GameObject("Clients");
            clientContainer.transform.parent = transform;
            clientContainer.transform.localPosition = Vector3.zero;
            clientContainer.transform.localRotation = Quaternion.identity;
            clientContainer.transform.localScale = Vector3.one;

            // Create a container for IoTErrors
            errorContainer = new GameObject("Errors");
            errorContainer.transform.parent = transform;
            errorContainer.transform.localPosition = Vector3.zero;
            errorContainer.transform.localRotation = Quaternion.identity;
            errorContainer.transform.localScale = Vector3.one;
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Set the server cert store file and make sure it exists
            serverCertificateStore = Path.Combine(MRET.ConfigurationManager.defaultPreferencesDirectory, DEFAULT_SERVERS_FILE);
            if (!File.Exists(serverCertificateStore))
            {
                // Create the file
                FileStream fs = File.Create(serverCertificateStore);
                byte[] emptyJSON = Encoding.ASCII.GetBytes("{}");
                fs.Write(emptyJSON, 0, emptyJSON.Length);
                fs.Close();
            }

            // Load the server cert store file containing certs already registered
            try
            {
                serverCerts = LoadJsonCertStore(serverCertificateStore, X509KeyStorageFlags.DefaultKeySet);
            }
            catch (Exception e)
            {
                LogError("A problem occurred loading the server certificate store: " +
                    serverCertificateStore + "; " + e.ToString(), nameof(MRETStart));
                serverCerts = new Dictionary<string, X509Certificate2>();
            }

            // Set the client cert filename and make sure it exists
            clientCertificateStore = Path.Combine(MRET.ConfigurationManager.defaultPreferencesDirectory, DEFAULT_CLIENTS_FILE);
            if (!File.Exists(clientCertificateStore))
            {
                // Create the file
                FileStream fs = File.Create(clientCertificateStore);
                byte[] emptyJSON = Encoding.ASCII.GetBytes("{}");
                fs.Write(emptyJSON, 0, emptyJSON.Length);
                fs.Close();
            }

            // Load the client cert store file containing certs already registered
            try
            {
                clientCerts = LoadJsonCertStore(clientCertificateStore, X509KeyStorageFlags.PersistKeySet);
            }
            catch (Exception e)
            {
                LogError("A problem occurred loading the client certificate store: " +
                    clientCertificateStore + "; " + e.ToString(), nameof(MRETStart));
                clientCerts = new Dictionary<string, X509Certificate2>();
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            while (!IoTClientErrors.IsEmpty)
            {
                (IoTClientDeprecated, Exception) error;
                IoTClientErrors.TryDequeue(out error);
                DisplayConnectionError(error.Item1, error.Item2);
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Save the certs out to the JSON stores
            SaveJsonCertStore(ServerCertificateStore, serverCerts, X509ContentType.Cert);
            SaveJsonCertStore(ClientCertificateStore, clientCerts, X509ContentType.Pkcs12);
        }
    }
}