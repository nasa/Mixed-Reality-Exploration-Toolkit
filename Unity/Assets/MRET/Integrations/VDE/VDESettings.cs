// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Extensions.VDE
{
    public class VDESettings : ClientInterface<VDEInterfaceType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(VDESettings);

        public const string DEFAULT_PROTOCOL = "https://";
        public const string DEFAULT_SERVER = "vde.coda.ee/VDE";

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private VDEInterfaceType serializedVDEInterface;

        public bool standalone;
        public bool renderInCloud;
        public string nameOfBakedConfigResource;
        public string nameOfBakedEntitiesResource;
        public string nameOfBakedLinksResource;

        #region Serializable
        /// <seealso cref="ClientInterface{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(VDEInterfaceType serialized, SerializationState deserializationState)
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
            serializedVDEInterface = serialized;

            // Deserialize the VDE settings
            standalone = serializedVDEInterface.Standalone;
            renderInCloud = serializedVDEInterface.RenderInCloud;
            nameOfBakedConfigResource = serializedVDEInterface.BakedConfigResource;
            nameOfBakedEntitiesResource = serializedVDEInterface.BakedEntitiesResource;
            nameOfBakedLinksResource = serializedVDEInterface.BakedLinksResource;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="ClientInterface{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(VDEInterfaceType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the VDE settings
            serialized.Standalone = standalone;
            serialized.RenderInCloud = renderInCloud;
            serialized.BakedConfigResource = nameOfBakedConfigResource;
            serialized.BakedEntitiesResource = nameOfBakedEntitiesResource;
            serialized.BakedLinksResource = nameOfBakedLinksResource;

            // Save the final serialized reference
            serializedVDEInterface = serialized;

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

            // Make sure to trim off excess whitespace
            server = server.Trim();

            // Add the protocol
            if (!server.StartsWith(DEFAULT_PROTOCOL))
            {
                server = DEFAULT_PROTOCOL + server;
            }

            // Make sure to remove whitespace at beginning and end
            string result = server.Trim();
            if (port > 0)
            {
                // Add the port if specified
                result += ":" + port;
            }

            return result;
        }

        /// <seealso cref="ClientInterface{T}.PerformDisconnect"/>
        protected override bool PerformDisconnect()
        {
            bool result = false;
            try
            {
                // TODO: Disconnect

                // Mark as successful
                result = true;
            }
            catch (System.Exception e)
            {
                LogWarning("A problem occurred attempting to disconnect from VDE: " + e.Message, nameof(PerformDisconnect));
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
#if MRET_EXTENSION_VDE && !HOLOLENS_BUILD
                // TODO: Very temporary.
                foreach (Assets.VDE.VDE vde in FindObjectsOfType<Assets.VDE.VDE>())
                {
#if MRET_2021_OR_LATER
                    vde.Init(standalone, renderInCloud,
                        serverConnection, nameOfBakedConfigResource,
                        nameOfBakedEntitiesResource, nameOfBakedLinksResource);
#endif
                }

                // Mark as connected
                result = true;
#else
                LogWarning("VDE is unavailable", nameof(PerformConnect));
#endif
            }
            catch (System.Exception e)
            {
                LogWarning("A problem occurred connecting to VDE: " + e.Message, nameof(PerformConnect));
            }

            return result;
        }
        #endregion ClientInterface
    }
}