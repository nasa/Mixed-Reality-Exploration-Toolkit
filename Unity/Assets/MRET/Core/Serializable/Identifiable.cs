// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 3 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// Identifiable object.<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="XRObject{T}"/>
	/// 
	public abstract class Identifiable<T> : XRObject<T>, IIdentifiable<T>
        where T : IdentifiableType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(Identifiable<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedIdentifiable;

        /// <summary>
        /// The synchronization component used for collaboration
        /// </summary>
        private ISynchronized synchronized = null;

        #region IIdentifiable
        /// <seealso cref="IIdentifiable.CreateSerializedType"/>
        IdentifiableType IIdentifiable.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IIdentifiable.uuid"/>
        public Guid uuid
        {
            get => _uuid;
            set
            {
                if ((value != null) && (value != Guid.Empty))
                {
                    // Make sure the UUID is not already in the registry
                    if (MRET.UuidRegistry.GetByUUID(value) == null)
                    {
                        // Unregister our existing entry
                        MRET.UuidRegistry.Unregister(_uuid);

                        // Assign the UUID
                        _uuid = value;

                        // Register ourself with MRET
                        MRET.UuidRegistry.Register(this);
                    }
                    else
                    {
                        LogWarning("Supplied UUID already exists in the registry. " +
                            "Invalidating attempt to set UUID: " + value, nameof(uuid));
                    }
                }
                else
                {
                    LogWarning("Supplied UUID is invalid: " + value, nameof(uuid));
                }
            }
        }
        private Guid _uuid;

        /// <seealso cref="IIdentifiable.id"/>
        public string id
        {
            get => _id;
            set
            {
                // Make sure the ID is not already in the registry
                if (MRET.UuidRegistry.GetByID(value) == null)
                {
                    // Assign the ID
                    _id = value;
                }
                else
                {
                    LogWarning("Supplied ID already exists in the registry. " +
                        "Invalidating attempt to set ID: " + value, nameof(id));
                }
            }
        }
        private string _id;

        /// <seealso cref="IIdentifiable.name"/>
        //public string name { get; set; }

        /// <seealso cref="IIdentifiable.description"/>
        public string description
        {
            get => _description;
            set => _description = value;
        }

        [SerializeField]
        [Tooltip("The description of the object")]
        private string _description = "";

        /// <seealso cref="IIdentifiable.SynchronizationEnabled"/>
        public bool SynchronizationEnabled
        {
            get
            {
                bool result = false;
                if (synchronized is Behaviour)
                {
                    result = (synchronized as Behaviour).enabled;
                }
                return result;
            }
            set
            {
                if (synchronized is Behaviour)
                {
                    (synchronized as Behaviour).enabled = value;
                }
            }
        }

        /// <seealso cref="IIdentifiable.Synchronize(IdentifiableType, Action{bool, string})"/>
        public void Synchronize(IdentifiableType serialized, Action<bool, string> onFinished = null)
        {
            Synchronize(serialized as T, onFinished);
        }

        /// <seealso cref="IIdentifiable{T}.Synchronize(T, Action{bool, string})"/>
        public void Synchronize(T serialized, Action<bool, string> onFinished = null)
        {
            Action<bool, string> SynchronizeAction = (bool serialized, string message) =>
            {
                if (!serialized)
                {
                    LogWarning("Synchronization failed: " + message, nameof(Synchronize));
                }

                // Restore synchronization
                synchronized.Resume();
            };

            // Pause synchronization
            synchronized.Pause();

            // Deserialize
            Deserialize(serialized, SynchronizeAction);
        }

        /// <seealso cref="IIdentifiable.Deserialize(IdentifiableType, Action{bool, string})"/>
        void IIdentifiable.Deserialize(IdentifiableType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IIdentifiable.Serialize(IdentifiableType, Action{bool, string})"/>
        void IIdentifiable.Serialize(IdentifiableType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IIdentifiable

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

            // Create a unique ID based on the name and assign a new UUID
            id = MRET.UuidRegistry.CreateUniqueIDFromName(name);
            uuid = Guid.NewGuid(); // <= This will auto-register with the MRET registry
//            description = "";

            // Create the synchronization object disabled
            synchronized = CreateSynchronizedObject();
            synchronized.Initialize(this);

            // Initialize collaboration
            SynchronizationEnabled = MRET.CollaborationManager.CollaborationEnabled;
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Destroy the synchronized object
            Destroy(synchronized as UnityEngine.Object);

            // Unregister with MRET
            if (MRET.UuidRegistry != null)
            {
                MRET.UuidRegistry.Unregister(uuid);
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="XRObject{T}.Deserialize(T, SerializationState)"/>
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
            serializedIdentifiable = serialized;

            // Deserialize the identifiable fields

            // Deserialize the name
            name = serializedIdentifiable.Name;

            // Update the indicator
            MRET.LoadingIndicatorManager.UpdateDetail("Deserializing " + name + "...");

            // Deserialize the unique ID, but make sure it is unique by asking the registry.
            if (!string.IsNullOrEmpty(serializedIdentifiable.ID))
            {
                // The registry will append a number if it is already registered
                id = MRET.UuidRegistry.CreateUniqueID(serializedIdentifiable.ID);
            }
            else
            {
                // The registry will base the unique id on the name
                id = MRET.UuidRegistry.CreateUniqueIDFromName(name);
            }

            // Deserialize the UUID (optional)
            if (!string.IsNullOrEmpty(serializedIdentifiable.UUID))
            {
                // UUID is optional, so keep our generated UUID if not defined.
                // We don't want to register an empty UUID.
                uuid = new Guid(serializedIdentifiable.UUID);
            }

            // Deserialize the human readable identification
            description = serializedIdentifiable.Description;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="XRObject{T}.Serialize(T, SerializationState)"/>
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

            // Serialize out the identifiable fields
            string uuidStr = uuid.ToString();
            serialized.UUID = !string.IsNullOrEmpty(uuidStr) ? uuidStr : null;
            serialized.Name = name;
            serialized.ID = id;
            serialized.Description = !string.IsNullOrEmpty(description) ? description : null;

            // TODO: Register GUID with DataManager?

            // Save the final serialized reference
            serializedIdentifiable = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <summary>
        /// Called to create the synchronized object associated with this identifiable object.
        /// Subclasses should override for custom synchronization behavior.
        /// </summary>
        /// 
        /// <seealso cref="ISynchronized"/>
        protected virtual ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<Synchronized>();
        }

    }

    /// <summary>
    /// Provides an implementation for the abstract Identifiable class
    /// </summary>
    public class Identifiable : Identifiable<IdentifiableType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(Identifiable);
    }

    /// <summary>
    /// Used to keep the default values from the schema in sync
    /// </summary>
    public class IdentifiableDefaults : XRObjectDefaults
    {
    }

}
