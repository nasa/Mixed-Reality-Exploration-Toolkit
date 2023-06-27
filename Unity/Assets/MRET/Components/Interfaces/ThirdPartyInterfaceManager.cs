// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Json;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Integrations.IoT;
using GOV.NASA.GSFC.XR.MRET.Integrations.Matlab;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.HUD;
using GOV.NASA.GSFC.XR.MRET.UI.Interfaces;

namespace GOV.NASA.GSFC.XR.MRET.Interfaces
{
    public class ThirdPartyInterfaceManager : MRETSerializableManager<ThirdPartyInterfaceManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ThirdPartyInterfaceManager);

        public const string DEFAULT_SERVERS_FILE = ".mretServers";
        public const string DEFAULT_CLIENTS_FILE = ".mretClients";

        /// <summary>
        /// Instance of the IoT Manager.
        /// </summary>
        [Tooltip("Instance of the IoT Manager.")]
        public IoTManager iotManager;

        /// <summary>
        /// Instance of the IoT Manager.
        /// </summary>
        [Tooltip("Instance of the Deprecated IoT Manager.")]
        public IoTManagerDeprecated iotManagerDeprecated;

        // Prefabs
        public GameObject matlabPrefab;
        public GameObject gmsecPrefab;

        /// <summary>
        /// Instance of the IoT Manager.
        /// </summary>
        public static IoTManager IoTManager => Instance.iotManager;

        /// <summary>
        /// Instance of the IoT Manager.
        /// </summary>
        public static IoTManagerDeprecated IoTManagerDeprecated => Instance.iotManagerDeprecated;

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            Log("Initializing IoT Manager...", nameof(Initialize));
            if (iotManager == null)
            {
                iotManager = FindObjectOfType<IoTManager>();
                if (iotManager == null)
                {
                    LogError("Fatal Error. Unable to initialize IoT manager. Aborting...", nameof(Initialize));
                    MRET.Quit();
                }
            }
            iotManager.Initialize();
            Log("IoT Manager initialized.", nameof(Initialize));

            // Required game objects
            if (gmsecPrefab == null)
            {
                LogError("Fatal Error. " + nameof(gmsecPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (matlabPrefab == null)
            {
                LogError("Fatal Error. " + nameof(matlabPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (generalErrorDialogPrefab == null)
            {
                LogError("Fatal Error. " + nameof(generalErrorDialogPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (clientErrorDialogPrefab == null)
            {
                LogError("Fatal Error. " + nameof(clientErrorDialogPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }

            // Create our internal structures
            errorMessages = new List<object>();
            errorDialogs = new Stack<GameObject>();
            thirdPartyInterfaceErrors = new ConcurrentQueue<(IThirdPartyInterface, ThirdPartyInterfaceException)>();

            // Create a container for Errors
            errorDialogContainer = new GameObject("Errors");
            errorDialogContainer.transform.parent = transform;
            errorDialogContainer.transform.localPosition = Vector3.zero;
            errorDialogContainer.transform.localRotation = Quaternion.identity;
            errorDialogContainer.transform.localScale = Vector3.one;
        }

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            if (state == IntegrityState.Success)
            {
                if (gmsecPrefab == null)
                {
                    LogError("GMSEC prefab not assigned.", nameof(IntegrityCheck));
                    state = IntegrityState.Failure;
                }
                if (matlabPrefab == null)
                {
                    LogError("Matlab prefab not assigned.", nameof(IntegrityCheck));
                    state = IntegrityState.Failure;
                }
                if (generalErrorDialogPrefab == null)
                {
                    LogError("Error dialog prefab not assigned.", nameof(IntegrityCheck));
                    state = IntegrityState.Failure;
                }
                if (clientErrorDialogPrefab == null)
                {
                    LogError("Error dialog prefab not assigned.", nameof(IntegrityCheck));
                    state = IntegrityState.Failure;
                }
            }

            return state;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Set the server cert store file and make sure it exists
            _serverCertificateStore = Path.Combine(MRET.ConfigurationManager.defaultPreferencesDirectory, DEFAULT_SERVERS_FILE);
            if (!File.Exists(_serverCertificateStore))
            {
                // Create the file
                FileStream fs = File.Create(_serverCertificateStore);
                byte[] emptyJSON = Encoding.ASCII.GetBytes("{}");
                fs.Write(emptyJSON, 0, emptyJSON.Length);
                fs.Close();
            }

            // Load the server cert store file containing certs already registered
            try
            {
                serverCerts = LoadJsonCertStore(_serverCertificateStore, X509KeyStorageFlags.DefaultKeySet);
            }
            catch (Exception e)
            {
                LogError("A problem occurred loading the server certificate store: " +
                    _serverCertificateStore + "; " + e.ToString(), nameof(MRETStart));
                serverCerts = new Dictionary<string, X509Certificate2>();
            }

            // Set the client cert filename and make sure it exists
            _clientCertificateStore = Path.Combine(MRET.ConfigurationManager.defaultPreferencesDirectory, DEFAULT_CLIENTS_FILE);
            if (!File.Exists(_clientCertificateStore))
            {
                // Create the file
                FileStream fs = File.Create(_clientCertificateStore);
                byte[] emptyJSON = Encoding.ASCII.GetBytes("{}");
                fs.Write(emptyJSON, 0, emptyJSON.Length);
                fs.Close();
            }

            // Load the client cert store file containing certs already registered
            try
            {
                clientCerts = LoadJsonCertStore(_clientCertificateStore, X509KeyStorageFlags.PersistKeySet);
            }
            catch (Exception e)
            {
                LogError("A problem occurred loading the client certificate store: " +
                    _clientCertificateStore + "; " + e.ToString(), nameof(MRETStart));
                clientCerts = new Dictionary<string, X509Certificate2>();
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // Display all the errors in the queue
            while (!thirdPartyInterfaceErrors.IsEmpty)
            {
                // Dequeue the error and process it
                if (thirdPartyInterfaceErrors.TryDequeue(out (IThirdPartyInterface, ThirdPartyInterfaceException) error))
                {
                    StartCoroutine(WaitForError(error.Item1, error.Item2));
                }
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Close all connections
            CloseConnections();

            // Save the certs out to the JSON stores
            SaveJsonCertStore(ServerCertificateStore, serverCerts, X509ContentType.Cert);
            SaveJsonCertStore(ClientCertificateStore, clientCerts, X509ContentType.Pkcs12);
        }
        #endregion MRETUpdateBehaviour

        #region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            return ProjectManager.InterfacesContainer.transform;
        }

        /// <summary>
        /// Create a GMSEC game object containing the GMSEC interface from a prefab.
        /// </summary>
        /// <param name="interfaceName">Name of the GMSEC interface</param>
        /// <param name="container">The container (parent) for the interface</param>
        /// <returns>The instantiated <code>GameObject</code></returns>
        protected GameObject CreateGMSECGameObject(string interfaceName, Transform container = null)
        {
            // Make sure we have a valid container reference
            container = (container == null) ? ProjectManager.InterfacesContainer.transform : container;

            // Instantiate the prefab
            GameObject gmsecGO = Instantiate(gmsecPrefab, container);

            // Get the GMSEC interface reference
            GMSECBusToDataManager gmsec = gmsecGO.GetComponent<GMSECBusToDataManager>();
            if (gmsec == null)
            {
                gmsec = gmsecGO.AddComponent<GMSECBusToDataManager>();
                // TODO: If we have to add the GMSEC data source, do we need to set
                // up any internal prefab references?
            }

            // Generate a better ID for the GMSEC interface for serialization
            gmsec.id = MRET.UuidRegistry.CreateUniqueIDFromName(interfaceName);

            // Rename the game object
            gmsecGO.name = interfaceName;

            return gmsecGO;
        }

        /// <summary>
        /// Create a Matlab game object containing the Matlab interface from a prefab.
        /// </summary>
        /// <param name="interfaceName">Name of the Matlab interface</param>
        /// <param name="container">The container (parent) for the interface</param>
        /// <returns>The instantiated <code>GameObject</code></returns>
        protected GameObject CreateMatlabGameObject(string interfaceName, Transform container = null)
        {
            // Make sure we have a valid container reference
            container = (container == null) ? ProjectManager.InterfacesContainer.transform : container;

            // Instantiate the prefab
            GameObject matlabGO = Instantiate(matlabPrefab, container);

            // Get the Matlab interface reference
            MATLABClient matlab = matlabGO.GetComponent<MATLABClient>();
            if (matlab == null)
            {
                matlab = matlabGO.AddComponent<MATLABClient>();
                // TODO: If we have to add the Matlab client, do we need to set
                // up any internal prefab references?
            }

            // Generate a better ID for the Matlab interface for serialization
            matlab.id = MRET.UuidRegistry.CreateUniqueIDFromName(interfaceName);

            // Rename the game object
            matlabGO.name = interfaceName;

            return matlabGO;
        }

        /// <summary>
        /// Instantiates a GMSEC interface from the supplied serialized interface.
        /// </summary>
        /// <param name="serializedInterface">The <code>GMSECInterfaceType</code> class instance
        ///     containing the serialized representation of the interface to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     interface. If not provided, one will be created</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     interface. If null, the project interface container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishInterfaceInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     interface instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishInterfaceInstantiation method to provide additional context</param>
        protected void InstantiateInterface(GMSECInterfaceType serializedInterface,
            Transform container = null, Action<IThirdPartyInterface> onLoaded = null,
            FinishSerializableInstantiationDelegate<InterfaceType, IThirdPartyInterface> finishInterfaceInstantiation = null,
            params object[] context)
        {
            if (!gmsecPrefab)
            {
                LogError("Failed to add interface because the " + nameof(gmsecPrefab) + " is null",
                    nameof(InstantiateInterface));
                return;
            }

            // Create the GMSEC game object from the prefab
            GameObject gmsecGO = CreateGMSECGameObject("GMSEC", container);

            // Instantiate and deserialize
            InstantiateSerializable(serializedInterface, gmsecGO, container, onLoaded,
                finishInterfaceInstantiation, context);
        }

        /// <summary>
        /// Instantiates an IoT interface from the supplied serialized interface.
        /// </summary>
        /// <param name="serializedInterface">The <code>IoTInterfaceType</code> class instance
        ///     containing the serialized representation of the interface to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     interface. If not provided, one will be created</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     interface. If null, the project interface container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishInterfaceInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     interface instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishInterfaceInstantiation method to provide additional context</param>
        protected void InstantiateInterface(IoTInterfaceType serializedInterface,
            Transform container = null, Action<IThirdPartyInterface> onLoaded = null,
            FinishSerializableInstantiationDelegate<InterfaceType, IThirdPartyInterface> finishInterfaceInstantiation = null,
            params object[] context)
        {
            // Instantiate and load the new interface
            IoTManager.InstantiateIoTConnection(serializedInterface, container, onLoaded);
        }

        /// <summary>
        /// Instantiates a Matlab interface from the supplied serialized interface.
        /// </summary>
        /// <param name="serializedInterface">The <code>MatlabInterfaceType</code> class instance
        ///     containing the serialized representation of the interface to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     interface. If not provided, one will be created</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     interface. If null, the project interface container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishInterfaceInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     interface instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishInterfaceInstantiation method to provide additional context</param>
        protected void InstantiateInterface(MatlabInterfaceType serializedInterface,
            Transform container = null, Action<IThirdPartyInterface> onLoaded = null,
            FinishSerializableInstantiationDelegate<InterfaceType, IThirdPartyInterface> finishInterfaceInstantiation = null,
            params object[] context)
        {
            if (!matlabPrefab)
            {
                LogError("Failed to add interface because the " + nameof(matlabPrefab) + " is null",
                    nameof(InstantiateInterface));
                return;
            }

            // Create the Matlab game object from the prefab
            GameObject matlabGO = CreateMatlabGameObject("Matlab", container);

            // Instantiate and load the new interface
            InstantiateSerializable(serializedInterface, matlabGO, container, onLoaded,
                finishInterfaceInstantiation, context);
        }

        /// <summary>
        /// Instantiates a ROS interface from the supplied serialized interface.
        /// </summary>
        /// <param name="serializedInterface">The <code>ROSInterfaceType</code> class instance
        ///     containing the serialized representation of the interface to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     interface. If not provided, one will be created</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     interface. If null, the project interface container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishInterfaceInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     interface instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishInterfaceInstantiation method to provide additional context</param>
        protected void InstantiateInterface(ROSInterfaceType serializedInterface, GameObject go = null,
            Transform container = null, Action<IThirdPartyInterface> onLoaded = null,
            FinishSerializableInstantiationDelegate<InterfaceType, IThirdPartyInterface> finishInterfaceInstantiation = null,
            params object[] context)
        {
            // Instantiate and load the new interface
            InstantiateSerializable(serializedInterface, go, container, onLoaded,
                finishInterfaceInstantiation, context);
        }

        /// <summary>
        /// Instantiates a VDE interface from the supplied serialized interface.
        /// </summary>
        /// <param name="serializedInterface">The <code>VDEInterfaceType</code> class instance
        ///     containing the serialized representation of the interface to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     interface. If not provided, one will be created</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     interface. If null, the project interface container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishInterfaceInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     interface instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishInterfaceInstantiation method to provide additional context</param>
        protected void InstantiateInterface(VDEInterfaceType serializedInterface, GameObject go = null,
            Transform container = null, Action<IThirdPartyInterface> onLoaded = null,
            FinishSerializableInstantiationDelegate<InterfaceType, IThirdPartyInterface> finishInterfaceInstantiation = null,
            params object[] context)
        {
            // Instantiate and load the new interface
            InstantiateSerializable(serializedInterface, go, container, onLoaded,
                finishInterfaceInstantiation, context);
        }

        /// <summary>
        /// Instantiates the third party interface from the supplied serialized interface.
        /// </summary>
        /// <param name="serializedInterface">The <code>InterfaceType</code> class instance
        ///     containing the serialized representation of the interface to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     interface. If not provided, one will be created</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     interface. If null, the project interface container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishInterfaceInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     interface instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishInterfaceInstantiation method to provide additional context</param>
        public void InstantiateInterface(InterfaceType serializedInterface, GameObject go = null,
            Transform container = null, Action<IThirdPartyInterface> onLoaded = null,
            FinishSerializableInstantiationDelegate<InterfaceType, IThirdPartyInterface> finishInterfaceInstantiation = null,
            params object[] context)
        {
            // Instantiate and load the new interface
            if (serializedInterface is GMSECInterfaceType)
            {
                InstantiateInterface(serializedInterface as GMSECInterfaceType, container, onLoaded,
                    finishInterfaceInstantiation, context);
            }
            else if (serializedInterface is IoTInterfaceType)
            {
                InstantiateInterface(serializedInterface as IoTInterfaceType, container, onLoaded,
                    finishInterfaceInstantiation, context);
            }
            else if (serializedInterface is MatlabInterfaceType)
            {
                InstantiateInterface(serializedInterface as MatlabInterfaceType, container, onLoaded,
                    finishInterfaceInstantiation, context);
            }
            else if (serializedInterface is ROSInterfaceType)
            {
                InstantiateInterface(serializedInterface as ROSInterfaceType, go, container, onLoaded,
                    finishInterfaceInstantiation, context);
            }
            else if (serializedInterface is VDEInterfaceType)
            {
                InstantiateInterface(serializedInterface as VDEInterfaceType, go, container, onLoaded,
                    finishInterfaceInstantiation, context);
            }
            else
            {
                LogWarning("Unknown interface type: " + serializedInterface);
                onLoaded?.Invoke(null);
            }
        }

        /// <summary>
        /// Instantiates an array of third party interfaces from the supplied serialized array of
        /// interfaces.
        /// </summary>
        /// <param name="serializedInterfaces">The array of <code>InterfaceType</code> class instances
        ///     containing the serialized representations of the interfaces to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     interfaces. If not provided, one will be created for each interface</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     interfaces. If null, the project interface container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishInterfaceInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish each
        ///     interface instantiation. Called for each instantiated interface. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishInterfaceInstantiation method to provide additional context</param>
        public void InstantiateInterfaces(InterfaceType[] serializedInterfaces, GameObject go = null,
            Transform container = null, Action<IThirdPartyInterface[]> onLoaded = null,
            FinishSerializableInstantiationDelegate<InterfaceType, IThirdPartyInterface> finishInterfaceInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize
            InstantiateSerializables(serializedInterfaces, go, container, onLoaded,
                InstantiateInterface, finishInterfaceInstantiation, context);
        }
        #endregion Serializable Instantiation

        #region Connection
        /// <summary>
        /// Performs a connect of all client interfaces
        /// </summary>
        public void Connect()
        {
            // Get all of the registered client interfaces
            IClientInterface[] clientInterfaces = MRET.UuidRegistry.RegisteredTypes<IClientInterface>();

            IThirdPartyInterface exceptionClient = null;
            try
            {
                // Clear current error messages so new ones can replace them
                errorMessages.Clear();

                // Start threads to connect each client
                foreach (IClientInterface client in clientInterfaces)
                {
                    // Make sure we aren't already connected
                    if (!client.Connected)
                    {
                        exceptionClient = client;
                        Thread childThread = new Thread(ClientInterfaceConnect);
                        childThread.Start(client);
                    }
                }
            }
            catch (Exception e)
            {
                // This handles all other exceptions
                string message = "A problem was encountered attempting to connect the client: " + e.ToString();
                LogError(message);

                ClientInterfaceException exception = new ClientInterfaceException(message);
                ReportError((exceptionClient, exception));
            }
        }

        /// <summary>
        /// Performs a connect of all client interfaces associate with the supplied server
        /// </summary>
        /// <param name="server">The server to connect</param>
        public void ConnectToServer(string server)
        {
            // Get all of the registered client interfaces
            IClientInterface[] clientInterfaces = MRET.UuidRegistry.RegisteredTypes<IClientInterface>();

            IThirdPartyInterface exceptionClient = null;
            foreach (IClientInterface clientInterface in clientInterfaces)
            {
                if (!string.IsNullOrEmpty(clientInterface.Server) &&
                    !string.IsNullOrEmpty(server) &&
                    (clientInterface.Server == server))
                {
                    try
                    {
                        Thread childThread = new Thread(ClientInterfaceConnect);
                        childThread.Start(clientInterface);
                    }
                    catch (Exception e)
                    {
                        // This handles all other exceptions
                        string message = "A problem was encountered attempting to connect the client: " + e.ToString();
                        LogError(message);

                        ClientInterfaceException exception = new ClientInterfaceException(message);
                        ReportError((exceptionClient, exception));
                    }
                }
            }
        }

        /// <summary>
        /// Performs a connect on the supplied client
        /// </summary>
        /// <param name="client"></param>
        private static void ClientInterfaceConnect(object client)
        {
            if (client is IClientInterface)
            {
                (client as IClientInterface).Connect();
            }
        }

        /// <summary>
        /// Closes all connections and clears all errors
        /// </summary>
        public void CloseConnections()
        {
            // Destroy all of the registered third party interfaces
            IThirdPartyInterface[] thirdPartyInterfaces = MRET.UuidRegistry.RegisteredTypes<IThirdPartyInterface>();
            foreach (IThirdPartyInterface thirdPartyInterface in thirdPartyInterfaces)
            {
                Destroy(thirdPartyInterface.gameObject);
            }

            // Clear the errors
            if (errorMessages != null)
            {
                errorMessages.Clear();
            }

            // Close all error dialogs
            CloseAllErrorDialogs();
        }
        #endregion Connection

        #region Certificates
        // Registered certificates keyed on server IP. Value is a cert
        private Dictionary<string, X509Certificate2> serverCerts;
        private Dictionary<string, X509Certificate2> clientCerts;

        public string ServerCertificateStore { get => _serverCertificateStore; }
        private string _serverCertificateStore;

        public string ClientCertificateStore { get => _clientCertificateStore; }
        private string _clientCertificateStore;

        /// <summary>
        /// Obtains the full path of the supplied certificate file
        /// </summary>
        /// <param name="file">The certificate file</param>
        /// <returns>The full file path of the supplied certificate file</returns>
        /// <exception cref="FileNotFoundException">If the file was not found</exception>
        public string GetFullCertPath(string file)
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
        /// Adds the supplied server cert to the server cert store
        /// </summary>
        /// <param name="file">The server certificate file to add</param>
        /// <returns>An indicator the server certificate was added</returns>
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
                    StoreServerCert(serverCert);

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
        /// <param name="file">The client certificate file to add</param>
        /// <returns>An indicator the client certificate was added</returns>
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
                    StoreClientCert(clientCert);

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
        /// Obtains the X.509 cert for the supplied server key contained in the supplied cert store
        /// </summary>
        /// <param name="certStore">The <code>Dictionary<string, X509Certificate2></code> cert store to query</param>
        /// <param name="server">The server name for the X.509 cert to lookup</param>
        /// <returns>The <code>X509Certificate2</code> representing the cert
        ///     for the supplied server key, or NULL if not found</returns>
        protected X509Certificate2 GetCert(Dictionary<string, X509Certificate2> certStore, string server)
        {
            // Lookup the cert
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
        public X509Certificate2 CertPathToCert(string path, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.DefaultKeySet)
        {
            return (new X509Certificate2(path, string.Empty, keyStorageFlags | X509KeyStorageFlags.Exportable));
        }

        /// <summary>
        /// Convert a certificate string to a certificate
        /// </summary>
        /// <param name="certStr"></param>
        /// <param name="keyStorageFlags"></param>
        /// <returns></returns>
        public X509Certificate2 CertStringToCert(string certStr, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.DefaultKeySet)
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
            if (cert == null)
            {
                LogError("Supplied cert is null", nameof(StoreCert));
                return;
            }

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
        /// Stores the supplied X.509 certificate into the server certificate store
        /// </summary>
        /// <param name="cert">The <code>X509Certificate2</code> X.509 certificate to store</param>
        public void StoreServerCert(X509Certificate2 cert)
        {
            StoreCert(serverCerts, cert);
        }

        /// <summary>
        /// Stores the supplied X.509 certificate into the client certificate store
        /// </summary>
        /// <param name="cert">The <code>X509Certificate2</code> X.509 certificate to store</param>
        public void StoreClientCert(X509Certificate2 cert)
        {
            StoreCert(clientCerts, cert);
        }

        /// <summary>
        /// Loads a JSON cert store and returns the X.509 certs as a dictionary keyed on server name.
        /// </summary>
        /// <param name="file">The cert store file to load</param>
        /// <param name="keyStorageFlags">The <code>X509KeyStorageFlags</code> indicating how the certs should be created</param>
        /// <returns>A <code>Dictionary<string, X509Certificate2></code> containing the loaded certs keyed on server name</returns>
        public Dictionary<string, X509Certificate2> LoadJsonCertStore(string file, X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.DefaultKeySet)
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
        #endregion Certificates

        #region Error Handling
        public const string INTERFACE_ERROR_KEY = "GOV.NASA.GSFC.XR.MRET.INTERFACE.ERROR";

        [Tooltip("Prefab displayed when general errors are encountered.")]
        public GameObject generalErrorDialogPrefab;
        [Tooltip("Prefab displayed when client errors are encountered.")]
        public GameObject clientErrorDialogPrefab;
        [Tooltip("The maxmimum allowed distance between the desired offset of the dialog " +
            "and the actual offset of the dialog before the dialog is repositioned to the " +
            "desired offset in front of the tracked headset.")]
        public float errorDialogThreshold = 0.2f;
        [Tooltip("The desired offset from the tracket headset.")]
        public Vector3 errorDialogOffset = new Vector3(0, 0, 0.5f);
        private GameObject errorDialogContainer;
        private Stack<GameObject> errorDialogs;
        private List<object> errorMessages;
        private ConcurrentQueue<(IThirdPartyInterface, ThirdPartyInterfaceException)> thirdPartyInterfaceErrors;

        //This is a coroutine that waits after a disconnect for a set duration before throwing an error
        private IEnumerator WaitForError(IThirdPartyInterface thirdPartyinterface, ThirdPartyInterfaceException exception)
        {
            yield return new WaitForSeconds(exception.reportingDelaySeconds);

            if (thirdPartyinterface != null)
            {
                if (thirdPartyinterface is IClientInterface)
                {
                    DisplayClientInterfaceError(thirdPartyinterface as IClientInterface, exception);
                }
                else
                {
                    DisplayThirdPartyInterfaceError(thirdPartyinterface, exception);
                }
            }
        }

        /// <summary>
        /// Called to report an error
        /// </summary>
        /// <param name="error"></param>
        public void ReportError((IThirdPartyInterface, ThirdPartyInterfaceException) error)
        {
            thirdPartyInterfaceErrors.Enqueue(error);
        }

        /// <summary>
        /// Removes the topmost error dialog
        /// </summary>
        public void RemoveTopinterfaceErrorDialog()
        {
            errorMessages.RemoveAt(0);
            GameObject topPrompt = errorDialogs.Pop();
            Destroy(topPrompt);
            SetFirstErrorDialogInteractable(true);
        }

        /// <summary>
        /// Destroys all error dialogs and clears the queue
        /// </summary>
        public void CloseAllErrorDialogs()
        {
            foreach (GameObject errorDialog in errorDialogs)
            {
                Destroy(errorDialog);
            }
            errorDialogs.Clear();
        }

        /// <summary>
        /// Sets the interactability of the topmost error dialog
        /// </summary>
        /// <param name="interactable">Indicates the interactability of the topmost dialog</param>
        private void SetFirstErrorDialogInteractable(bool interactable)
        {
            if (errorDialogs.Count > 0)
            {
                GameObject topPromptObject = errorDialogs.Peek();
                ThirdPartyInterfaceErrorDialogController errorPrompt = topPromptObject.GetComponent<ThirdPartyInterfaceErrorDialogController>();
                errorPrompt.SetInteractable(interactable);
            }
        }

        /// <summary>
        /// Configures the HUD for the error dialog
        /// </summary>
        /// <param name="errorDialog"></param>
        private void ConfigureErrorDialogHUD(GameObject errorDialog)
        {
            // Base the offset on the number of error dialogs
            Vector3 offset = new Vector3(errorDialogs.Count * .05f, errorDialogs.Count * -.05f, 0);

            // Get the frame HUD reference
            FrameHUD frameHUD = errorDialog.GetComponent<FrameHUD>();
            if (frameHUD == null)
            {
                frameHUD = errorDialog.AddComponent<FrameHUD>();
            }

            // Configure the frame HUD
            frameHUD.offsetThreshold = errorDialogThreshold;
            frameHUD.offset = errorDialogOffset + offset;
        }

        /// <summary>
        /// Displays a generic third party interface error
        /// </summary>
        /// <param name="thirdPartyInterface">The <code>IThirdPartyInterface</code> triggering the error</param>
        /// <param name="exception">The <code>ThirdPartyInterfaceException</code> associated with the error</param>
        protected virtual void DisplayThirdPartyInterfaceError(IThirdPartyInterface thirdPartyInterface, ThirdPartyInterfaceException exception)
        {
            // Store the error message
            errorMessages.Add(exception.Message);
            MRET.DataManager.SaveValue(new DataManager.DataValue(INTERFACE_ERROR_KEY, errorMessages));

            // Instantiate the error dialog box and set it to follow the user
            GameObject errorDialog = Instantiate(generalErrorDialogPrefab, errorDialogContainer.transform);

            // Initialize the error dialog
            ThirdPartyInterfaceErrorDialogController errorPrompt = errorDialog.GetComponent<ThirdPartyInterfaceErrorDialogController>();
            errorPrompt.thirdPartyInterface = thirdPartyInterface;
            errorPrompt.SetInteractable(errorDialogs.Count == 0);

            // Configure the HUD
            ConfigureErrorDialogHUD(errorDialog);

            // Push the dialog onto the stack
            errorDialogs.Push(errorDialog);
        }

        /// <summary>
        /// Displays a generic client interface error
        /// </summary>
        /// <param name="clientInterface">The <code>IClientInterface</code> triggering the error</param>
        /// <param name="exception">The <code>ThirdPartyInterfaceException</code> associated with the error</param>
        protected virtual void DisplayClientInterfaceError(IClientInterface clientInterface, ThirdPartyInterfaceException exception)
        {
            // Store the ValueTuple with the server and error message
            errorMessages.Add((clientInterface.Server, exception.Message));
            MRET.DataManager.SaveValue(new DataManager.DataValue(INTERFACE_ERROR_KEY, errorMessages));

            // Instantiate the error dialog box and set it to follow the user
            GameObject errorDialog = Instantiate(clientErrorDialogPrefab, errorDialogContainer.transform);

            // Initialize the error dialog
            ClientInterfaceErrorDialogController errorPrompt = errorDialog.GetComponent<ClientInterfaceErrorDialogController>();
            errorPrompt.thirdPartyInterface = clientInterface;
            if (exception is ServerConnectionException)
            {
                errorPrompt.Source = ClientInterfaceErrorDialogController.ErrorSourceType.Server;
            }
            else
            {
                errorPrompt.Source = ClientInterfaceErrorDialogController.ErrorSourceType.Client;
            }
            errorPrompt.SetInteractable(errorDialogs.Count == 0);

            // Configure the HUD
            ConfigureErrorDialogHUD(errorDialog);

            // Push the dialog onto the stack
            errorDialogs.Push(errorDialog);
        }
        #endregion Error Handling
    }
}