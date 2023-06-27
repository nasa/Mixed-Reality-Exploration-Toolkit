// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    public class IoTManager : MRETSerializableManager<IoTManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IoTManager);

        //This object contains an IoTClient script and is used for intantiating new connections
        public GameObject IoTConnectionPrefab;

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();
        }

        #region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            // IoTManager handles twoserializable types, so we have to see which one the base
            // class is asking for
            Transform result;
            if (serialized is IoTClient)
            {
                // IoTClient
                result = ProjectManager.InterfacesContainer.transform;
            }
            else
            {
                // IoTThingType
                result = ProjectManager.IoTThingsContainer.transform;
            }

            return result;
        }

        #region IoTThings
        /// <summary>
        /// Instantiates the IoT thing from the supplied serialized IoT thing.
        /// </summary>
        /// <param name="serializedIoTThing">The <code>IoTThingType</code> class instance
        ///     containing the serialized representation of the IoT thing to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     IoT thing. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     IoT thing. If null, the project IoTThings container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishIoTThingInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     IoT thing instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishIoTThingInstantiation method to provide additional context</param>
        public void InstantiateIoTThing(IoTThingType serializedIoTThing, GameObject go = null,
            Transform container = null, Action<IIoTThing> onLoaded = null,
            FinishSerializableInstantiationDelegate<IoTThingType, IIoTThing> finishIoTThingInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize
            InstantiateSerializable(serializedIoTThing, go, container, onLoaded,
                finishIoTThingInstantiation, context);
        }

        /// <summary>
        /// Instantiates an array of IoT things from the supplied serialized array of IoT things.
        /// </summary>
        /// <param name="serializedIoTThings">The array of <code>IoTThingType</code> class instances
        ///     containing the serialized representations of the IoT things to instantiate.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     IoT things. If not provided, one will be created for each IoT thing</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated IoT things.
        ///     If null, the project IoTThings container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishIoTThingInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish each IoT thing
        ///     instantiation. Called for each instantiated IoT thing. If not specified, a default logging
        ///     behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the finishIoTThingInstantiation
        ///     method to provide additional context.</param>
        public void InstantiateIoTThings(IoTThingType[] serializedIoTThings, GameObject go = null,
            Transform container = null, Action<IIoTThing[]> onLoaded = null,
            FinishSerializableInstantiationDelegate<IoTThingType, IIoTThing> finishIoTThingInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize
            InstantiateSerializables(serializedIoTThings, go, container, onLoaded,
                InstantiateIoTThing, finishIoTThingInstantiation, context);
        }
        #endregion IoTThings

        #region IoTConnections
        /// <summary>
        /// Instantiates the IoT connection from the supplied serialized IoT connection.
        /// </summary>
        /// <param name="serializedConnection">The <code>IoTInterfaceType</code> class instance
        ///     containing the serialized representation of the IoT connection to instantiate</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     IoT connection. If null, the container supplied by the
        ///     <code>GetDefaultProjectContainer</code> method will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <seealso cref="GetDefaultSerializableContainer{T}(T)"/>
        public void InstantiateIoTConnection(IoTInterfaceType serializedConnection,
            Transform container = null, Action<IoTClient> onLoaded = null)
        {
            if (!IoTConnectionPrefab)
            {
                LogError("Failed to instantiate connection because the " + nameof(IoTConnectionPrefab) + " is null",
                    nameof(InstantiateIoTConnection));
                return;
            }

            // Create the client from the prefab
            GameObject clientGO = null;
            IoTClient iotClient = IoTClient.Create("IoTConnection", IoTConnectionPrefab, container);
            if (iotClient != null)
            {
                clientGO = iotClient.gameObject;
            }

            // Instantiate and deserialize
            InstantiateSerializable(serializedConnection, clientGO, container, onLoaded);
        }
        #endregion IoTConnections
        #endregion Serializable Instantiation

        /// <summary>
        /// Obtains the IoT thing associated with the supplied ID
        /// </summary>
        /// <param name="ID">The ID to query</param>
        /// <param name="output">The <code>IIoTThing</code> associated with the supplied ID.
        ///     Null if not found.</param>
        /// <returns></returns>
        public bool GetThing(string ID, out IIoTThing output)
        {
            output = null;
            IIdentifiable identifiable = MRET.UuidRegistry.GetByID(ID);
            if (identifiable is IIoTThing)
            {
                output = identifiable as IIoTThing;
            }
            return (output != null);
        }

    }
}