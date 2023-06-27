// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Time;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// 20 December 2022: Refactored for schema updates (JCH)
    /// </remarks>
	///
	/// <summary>
	/// UnityProject
	///
	/// A project in MRET
	///
    /// Author: Dylan Baker
    /// Author: Jeffrey Hosler (Refactor)
	/// </summary>
	/// 
    public class UnityProject : VersionedMRETBehaviour<ProjectType>, IUnityProject<ProjectType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UnityProject);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private ProjectType serializedProject;

        #region IUnityProject
        /// <seealso cref="IUnityProject.description"/>
        public string description { get; set; }

        /// <seealso cref="IUnityProject.Changed"/>
        public bool Changed { get; private set; } // TODO: Not implemented, currently only returns true.

        /// <seealso cref="IUnityProject.ScaleMultiplier"/>
        public float ScaleMultiplier { get; set; }

        /// <seealso cref="IUnityProject.Environment"/>
        public IEnvironment Environment { get => _environment; }
        private Environment _environment;

        /// <seealso cref="IUnityProject.User"/>
        public IUser User { get => _user; }
        private User _user;

        /// <seealso cref="IUnityProject.TimeSimulation"/>
        public ITimeSimulation TimeSimulation { get => _timeSimulation; }
        private TimeSimulation _timeSimulation;

        /// <seealso cref="IUnityProject.Content"/>
        public IProjectContent Content { get => _content; }
        private ProjectContent _content;

        /// <seealso cref="IUnityProject.Interfaces"/>
        public GameObject Interfaces { get => _interfaces; }
        private GameObject _interfaces;

        /// <seealso cref="IUnityProject.CreateSerializedType"/>
        ProjectType IUnityProject.CreateSerializedType() => CreateSerializedType();
        #endregion IUnityProject

        // Loaded Project Information
        private ViewType loadedProject_currentView = null;

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Interfaces
            _interfaces = new GameObject("Interfaces");
            _interfaces.transform.parent = gameObject.transform;
            _interfaces.transform.localPosition = Vector3.zero;
            _interfaces.transform.localRotation = Quaternion.identity;
            _interfaces.transform.localScale = Vector3.one;

            // Assign the default property settings
            description = "";
            Changed = true; // TODO: Once implemented, this would start as false;
            ScaleMultiplier = transform.localScale.x;
            transform.hasChanged = false;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // FIXME: This needs to be moved to the ConfigurationManager or the PreferenceManager
            // and should be assigned at MRET startup. Not at project load.
            // Obtain the user reference and initialize with the input rig
            _user = MRET.InputRig.GetComponentInChildren<User>();
            if (_user == null)
            {
                _user = MRET.InputRig.gameObject.AddComponent<User>();
            }
            _user.updateRate = UpdateFrequency.Hz10; // Set the update rate to some reasonable default
            _user.Initialize(true, MRET.InputRig);
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // Update the scale multiplier if the transform has changed
            if (transform.hasChanged)
            {
                // TODO: Is there a better implementation for this?
                ScaleMultiplier = transform.localScale.x;
                transform.hasChanged = false;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            if (_environment != null) Destroy(_environment.gameObject);
            if (_content != null) Destroy(_content.gameObject);
            if (_interfaces != null) Destroy(_interfaces);
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        #region Deserialization
        /// <summary>
        /// Asynchronously deserializes the supplied serialized user transform and updates the supplied
        /// state with the result.
        /// </summary>
        /// <param name="serializedTransform">The serialized <code>TransformType</code> transform</param>
        /// <param name="transformDeserializationState">The <code>SerializationState</code> to populate
        ///     with the result.</param>
        /// 
        /// <see cref="TransformType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeUserTransform(TransformType serializedTransform, SerializationState transformDeserializationState)
        {
            // Transform is optional, but we still need to deserialize the default settings
            if (serializedTransform is null)
            {
                // Create the defaults
                serializedTransform = new TransformType();
            }

            // Deserialize the user transform
            SchemaUtil.DeserializeTransform(serializedTransform, MRET.InputRig.gameObject);

            // Mark as complete
            transformDeserializationState.complete = true;

            yield return null;
        }

        /// Deserializes the supplied serialized interfaces into an array of third party interfaces, and
        /// updates the supplied state with the resultant array.
        /// </summary>
        /// <param name="serializedInterfaces">The <code>InterfaceType</code> array representing the
        ///     serialized interfaces</param>
        /// <param name="deserializationState">The <code>SerializationState</code> to populate
        ///     with the result.</param>
        /// 
        /// <see cref="InterfaceType"/>
        /// <see cref="SerializationState"/>
        protected virtual IEnumerator DeserializeInterfaces(InterfaceType[] serializedInterfaces, SerializationState deserializationState)
        {
            Action<IThirdPartyInterface[]> OnInterfacesLoadedAction = (IThirdPartyInterface[] loadedInterfaces) =>
            {
                if ((loadedInterfaces == null) || (loadedInterfaces.Length == 0))
                {
                    // Record the error
                    deserializationState.Error("A problem was encountered instantiating the third party interfaces");
                }

                // Mark as complete
                deserializationState.complete = true;
            };

            // Instantiate and deserialize the interfaces
            ThirdPartyInterfaceManager.Instance.InstantiateInterfaces(serializedInterfaces,
                null, _interfaces.transform, OnInterfacesLoadedAction);

            // Wait for completion
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            yield return null;
        }

        /// <seealso cref="Versioned{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(ProjectType serialized, SerializationState deserializationState)
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
            serializedProject = serialized;

            // Initialize from the serialized project

            // Load the description (optional)
            if (!string.IsNullOrEmpty(serializedProject.Description))
            {
                description = serializedProject.Description;
            }

            // Destroy the old environment
            if (_environment != null) Destroy(_environment.gameObject);

            // Environment deserialization
            {
                // Deserialize the Environment (required)
                VersionedSerializationState<Environment> environmentDeserializationState = new VersionedSerializationState<Environment>();
                StartCoroutine(DeserializeVersioned(serializedProject.Environment, null, gameObject.transform, environmentDeserializationState));

                // Wait for the coroutine to complete
                while (!environmentDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(environmentDeserializationState);

                // Make sure the resultant environment is not null
                if (environmentDeserializationState.versioned is null)
                {
                    deserializationState.Error("Deserialized environment cannot be null, denoting a possible internal issue.");
                }

                // If the environment deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Assign the environment
                _environment = environmentDeserializationState.versioned;

                // Clear the state
                deserializationState.Clear();
            }

            // User Transform deserialization (optional, but still deserialize defaults)
            {
                // Use the supplied serialized structure or a default
                TransformType serializedUserTransform = serializedProject.UserTransform ?? new TransformType();

                // Deserialize the User Transform (required)
                SerializationState transformDeserializationState = new SerializationState();
                StartCoroutine(DeserializeUserTransform(serializedUserTransform, transformDeserializationState));

                // Wait for the coroutine to complete
                while (!transformDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(transformDeserializationState);

                // If the user deserialization failed, there's no point in continuing
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // User deserialization
            /* TODO: Save for applying user preferences
            {
                // Deserialize the User (required)
                SerializationState userDeserializationState = new SerializationState();
                StartCoroutine(_user.DeserializeWithLogging(serializedProject.User, userDeserializationState));

                // Wait for the coroutine to complete
                while (!userDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(userDeserializationState);

                // If the user deserialization failed, there's no point in continuing
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }
            */

            // Destroy the old content
            _timeSimulation = null;

            // Time simulation deserialization (optional)
            if (serializedProject.Time != null)
            {
                // Create a new time simulation
                _timeSimulation = new TimeSimulation();

                SerializationState timeDeserializationState = new SerializationState();
                _timeSimulation.DeserializeWithLogging(serializedProject.Time, timeDeserializationState);

                // If the time deserialization failed, log a message but continue
                if (deserializationState.IsError)
                {
                    LogWarning("A problem was encountered while deserializing the time simulation: " +
                        deserializationState.ErrorMessage);
                    _timeSimulation = null;
                }
                else
                {
                    // Configure the time manager
                    _timeSimulation.ConfigureTimeManager(true);
                }

                // Clear the state
                deserializationState.Clear();
            }

            // Destroy the old content
            if (_content != null) Destroy(_content.gameObject);

            // Content deserialization (optional)
            if (serializedProject.Content != null)
            {
                // Deserialize the content
                VersionedSerializationState<ProjectContent> contentDeserializationState = new VersionedSerializationState<ProjectContent>();
                StartCoroutine(DeserializeVersioned(serializedProject.Content, null, gameObject.transform, contentDeserializationState));

                // Wait for the coroutine to complete
                while (!contentDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(contentDeserializationState);

                // Make sure the resultant project content type is the correct type
                if (contentDeserializationState.versioned is null)
                {
                    deserializationState.Error("Deserialized child parts group cannot be null, denoting a possible internal issue.");
                }

                // If the deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Assign the content
                _content = contentDeserializationState.versioned;

                // Clear the state
                deserializationState.Clear();
            }

            // Destroy the old interfaces
            if (_interfaces != null)
            {
                IThirdPartyInterface[] thirdPartInterfaces = _interfaces.GetComponentsInChildren<IThirdPartyInterface>();
                foreach (IThirdPartyInterface thirdPartInterface in thirdPartInterfaces)
                {
                    Destroy(thirdPartInterface.gameObject);
                }
            }

            // Interfaces (optional)
            if ((serializedProject.Interfaces != null) && (serializedProject.Interfaces.Length > 0))
            {
                // Deserialize the interfaces
                SerializationState interfacesDeserializationState = new SerializationState();
                StartCoroutine(DeserializeInterfaces(serializedProject.Interfaces, interfacesDeserializationState));

                // Wait for the coroutine to complete
                while (!interfacesDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(interfacesDeserializationState);

                // If the deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }
        #endregion Deserialization

        #region Serialization
        /// <summary>
        /// Asynchronously serializes the user transform into the supplied serialized transform and
        /// updates the supplied state with the serialization state.
        /// </summary>
        /// <param name="serializedTransform">The serialized <code>TransformType</code> to populate with the transform</param>
        /// <param name="transformSerializationState">The <code>SerializationState</code> to populate with the serialization state.</param>
        /// 
        /// <see cref="TransformType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator SerializeUserTransform(TransformType serializedTransform, SerializationState transformSerializationState)
        {
            // Transform settings are optional
            if (serializedTransform != null)
            {
                // Serialize the transform
                SchemaUtil.SerializeTransform(serializedTransform, MRET.InputRig.gameObject);
            }

            // Mark as complete
            transformSerializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Versioned{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(ProjectType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the description (optional)
            if (!string.IsNullOrEmpty(description))
            {
                serialized.Description = description;
            }

            // Serialize the environment (required)
            if (_environment != null)
            {
                // Use the original structure or a default
                serialized.Environment = (serializedProject != null) ?
                    serializedProject.Environment :
                    _environment.CreateSerializedType();

                // Serialize the environment
                SerializationState environmentSerializationState = new SerializationState();
                StartCoroutine(_environment.SerializeWithLogging(serialized.Environment, environmentSerializationState));

                // Wait for the coroutine to complete
                while (!environmentSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(environmentSerializationState);

                // If the content serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }
            else
            {
                // Report the error
                serializationState.Error("Required environment reference is null");
                yield break;
            }

            // Serialize the user transform (optional)
            {
                // Start with our internal serialized transform to serialize out the transform
                // using the original deserialized structure (if was provided during deserialization)
                // or just use the default. We want to serialize and then check for all defaults.
                serialized.UserTransform = (serializedProject != null) ?
                    serializedProject.UserTransform :
                    new TransformType();

                // Serialize the transform
                SerializationState transformSerializationState = new SerializationState();
                StartCoroutine(SerializeUserTransform(serialized.UserTransform, transformSerializationState));

                // Wait for the coroutine to complete
                while (!transformSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(transformSerializationState);

                // If the serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Don't serialize out anything if all defaults
                if ((serialized.UserTransform != null) &&
                    (serialized.UserTransform.Position == null) &&
                    (serialized.UserTransform.Item == null) &&
                    (serialized.UserTransform.Item1 == null))
                {
                    // Set to null for defaults
                    serialized.UserTransform = null;
                }

                // Clear the state
                serializationState.Clear();
            }

            // Serialize the user (required)
            /* TODO: Save for user preference serialization (saving)
            if (_user != null)
            {
                // Use the original structure or a default
                serialized.User = (serializedProject != null) ?
                    serializedProject.User :
                    _user.CreateSerializedType();

                // Serialize the user
                SerializationState userSerializationState = new SerializationState();
                StartCoroutine(_user.SerializeWithLogging(serialized.User, userSerializationState));

                // Wait for the coroutine to complete
                while (!userSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(userSerializationState);

                // If the content serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }
            else
            {
                // Report the error
                serializationState.Error("Required user reference is null");
                yield break;
            }
            */

            // Serialize the time simulation (optional)
            if (_timeSimulation != null)
            {
                // Use the original structure or a default
                serialized.Time = (serializedProject != null) ?
                    serializedProject.Time :
                    _timeSimulation.CreateSerializedType();

                // Serialize the time
                SerializationState timeSerializationState = new SerializationState();
                _timeSimulation.SerializeWithLogging(serialized.Time, timeSerializationState);

                // Record the serialization state
                serializationState.Update(timeSerializationState);

                // If the content serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }

            // Serialize the content (optional)
            if (_content != null)
            {
                // Use the original structure or a default
                serialized.Content = (serializedProject != null) ?
                    serializedProject.Content :
                    _content.CreateSerializedType();

                // Serialize the content
                SerializationState contentState = new SerializationState();
                StartCoroutine(_content.SerializeWithLogging(serialized.Content, contentState));

                // Wait for the coroutine to complete
                while (!contentState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(contentState);

                // If the content serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Clear the state
                serializationState.Clear();
            }

            // Serialize the interfaces (optional)
            serialized.Interfaces = null;
            if (_interfaces != null)
            {
                List<InterfaceType> serializedInterfaces = new List<InterfaceType>();

                // Serialize the interfaces
                IThirdPartyInterface[] thirdPartyInterfaces = _interfaces.GetComponentsInChildren<IThirdPartyInterface>();
                foreach (IThirdPartyInterface thirdPartyInterface in thirdPartyInterfaces)
                {
                    // Create the empty serialized type to load
                    var serializedInterface = thirdPartyInterface.CreateSerializedType();

                    SerializationState interfaceSerializationState = new SerializationState();
                    Action<bool, string> OnInterfaceSerializedAction = (bool loaded, string message) =>
                    {
                        if (loaded)
                        {
                            serializedInterfaces.Add(serializedInterface);

                            // Mark as complete
                            interfaceSerializationState.complete = true;
                        }
                        else
                        {
                            // Record the error
                            interfaceSerializationState.Error("A problem was encountered " +
                                "serializing the interface: " + message);
                        }
                    };

                    // Serialize the interface
                    thirdPartyInterface.Serialize(serializedInterface, OnInterfaceSerializedAction);

                    // Wait for the serialization to complete
                    while (!interfaceSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(interfaceSerializationState);

                    // If the content serialization failed, there's no point in continuing. Something is wrong
                    if (serializationState.IsError) yield break;
                }

                // Assign the interfaces
                serialized.Interfaces = serializedInterfaces.Count > 0 ? serializedInterfaces.ToArray() : null;

                // Clear the state
                serializationState.Clear();
            }

            // Save the final serialized reference
            serializedProject = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization
        #endregion Serializable
    }

}