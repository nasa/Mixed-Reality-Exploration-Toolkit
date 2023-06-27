// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 28 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SerializableInstantiation
	///
	/// Performs instantiation and deserialization of a serializable object given a serializable type.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class SerializableInstantiation : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(SerializableInstantiation);

        public bool Running => _instantiatingAndLoading || _loading;

        private bool _instantiatingAndLoading;
        private bool _loading;

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
			// Take the inherited behavior
			base.MRETAwake();

            // TODO: Custom initialization (before deserialization)
            _instantiatingAndLoading = false;
            _loading = false;
        }
		
		/// <seealso cref="MRETBehaviour.MRETStart"/>
		protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

			// TODO: Custom initialization (after deserialization)
			
		}

        #region Serializable Instantiation
        /// <summary>
        /// Gets a serializable component of the specified typeparam <code>I</code> from the supplied
        /// game object, or creates (instantiates) one if component doesn't exist. If the supplied
        /// typeparam represents an interface, this method uses the supplied serialized instance of
        /// typeparam <code>T</code> to lookup the registered class that implements the interface
        /// from the configuration manager typemap.
        /// </summary>
        /// <typeparam name="T">The serialized type derived from <code>VersionedType</code></typeparam>
        /// <typeparam name="I">The resultant class type that descends from a class that implements
        ///     <code>IVersioned</code></typeparam>
        /// <param name="obj">The <code>GameObject</code> to get the component instance of typeparam
        ///     <code>I</code></param>
        /// <param name="serialized">The serialized instance used to obtain the type key for the
        ///     lookup table</param>
        /// <returns></returns>
        public I GetComponent<T, I>(GameObject obj, T serialized)
            where T : VersionedType
            where I : IVersioned
        {
            I result = default;

            // Get the serializable type to load from the typemap
            Type serializedType = serialized.GetType();
            Type serializableType = typeof(I);
            if (serializableType.IsInterface)
            {
                // We were supplied an interface type to create, so lookup the implementing
                // class for the serialized type
                serializableType = MRET.ConfigurationManager.LookupSerializableType(serializedType);
            }

            // Make sure the type is a subclass of Component before continuing
            if (serializableType.IsSubclassOf(typeof(Component)))
            {
                // Attempt to get the component
                Component comp = obj.GetComponent(serializableType);
                if (comp == null)
                {
                    // It didn't exist, so add the component
                    comp = obj.AddComponent(serializableType);
                }

                // Make sure the result is the correct type
                if (comp is I)
                {
                    result = (I)(comp as IVersioned);
                }
                else
                {
                    LogError("Serializable type does not implement " + typeof(I));
                    Destroy(comp);
                }
            }
            else
            {
                LogError("Serializable type does not derive from " + typeof(Component) + ": " + serializableType);
            }

            return result;
        }

        public IIdentifiable CreateComponent(GameObject obj, Type serializable)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Type serializableType = assembly.GetType(serializable.ToString(), true);
            return obj.AddComponent(serializableType) as IIdentifiable;
        }

        /// <summary>
        /// Instantiates a serializable object associated with the supplied serialized
        /// instance and adds it to the supplied game object.
        /// </summary>
        /// <typeparam name="T">The serialized type derived from <code>VersionedType</code></typeparam>
        /// <typeparam name="I">The resultant class type that descends from a class that implements
        ///     <code>IVersioned</code></typeparam>
        /// <param name="serialized">The serialized instance associated with the type parameter</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated serializable.
        ///     If not provided, one will be created</param>
        /// <param name="parent">The parent <code>Transform</code> of the supplied game object. If null, the
        ///     project scene objects container will be used</param>
        /// <param name="onFinished">The optional <code>Action</code> to be triggered on asynchronous completion</param>
        public void InstantiateSerializable<T, I>(T serialized, GameObject go, Transform parent,
            Action<I> onFinished = null)
            where T : VersionedType
            where I : IVersioned
        {
            _instantiatingAndLoading = true;

            StartCoroutine(InstantiateSerializableAsync(serialized, go, parent, onFinished));
        }

        /// <summary>
        /// Asynchronously instantiates a serializable object associated with the supplied serialized
        /// instance and adds it to the supplied game object.
        /// </summary>
        /// <typeparam name="T">The serialized type derived from <code>VersionedType</code></typeparam>
        /// <typeparam name="I">The resultant class type that descends from a class that implements
        ///     <code>IVersioned</code></typeparam>
        /// <param name="serialized">The serialized instance associated with the type parameter</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated serializable.
        ///     If not provided, one will be created</param>
        /// <param name="parent">The parent <code>Transform</code> of the supplied game object. If null, the
        ///     project scene objects container will be used</param>
        /// <param name="onFinished">The optional <code>Action</code> to be triggered on asynchronous completion</param>
        protected IEnumerator InstantiateSerializableAsync<T, I>(T serialized, GameObject go = null,
            Transform parent = null, Action<I> onFinished = null)
            where T : VersionedType
            where I : IVersioned
        {
            try
            {
                // Create the game object if not supplied
                bool goCreated = false;
                if (go == null)
                {
                    // Create the name of the game object by using the serialized type and truncating the "Type"
                    string objName = typeof(T).Name.Replace("Type", "");

                    // Instantiate empty game object
                    go = new GameObject(objName);
                    goCreated = true;
                }

                // Delegate the identifiable serialize action
                Action<I> SerializableLoadedAction = (I serializable) =>
                {
                    // Surface to caller if specified
                    onFinished?.Invoke(serializable);

                    // Cleanup on failure
                    if ((serializable == null) && goCreated)
                    {
                        Destroy(go);
                    }

                    _instantiatingAndLoading = false;
                };

                // Get the serializable instance
                I serializable = GetComponent<T, I>(go, serialized);
                if (serializable != null)
                {
                    // Load the serializable with the serialized information
                    LoadSerializable(serialized, serializable, parent, SerializableLoadedAction);
                }
                else
                {
                    LogError("A problem occurred trying to get the serializable",
                        nameof(InstantiateSerializableAsync));
                    onFinished?.Invoke(default);
                    yield break;
                }
            }
            catch (Exception e)
            {
                LogError("A problem occurred trying to instantiate the serializable: " + e, nameof(InstantiateSerializableAsync));
                onFinished?.Invoke(default);
                yield break;
            }

            yield return null;
        }

        /// <summary>
        /// Performs loading (deserialization) of the supplied serialized instance into
        /// the supplied serializable instance.
        /// </summary>
        /// <typeparam name="T">The serialized type derived from <code>VersionedType</code></typeparam>
        /// <typeparam name="I">The resultant class type that descends from a class that implements
        ///     <code>IVersioned</code></typeparam>
        /// <param name="serialized">The serialized instance associated with the type parameter</param>
        /// <param name="parent">The optional <code>Transform</code> that will parent the instantiated serializable.
        ///     If not provided, one will be created</param>
        /// <param name="parent">The parent <code>Transform</code> of the supplied game object. If null, the
        ///     project scene objects container will be used</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be triggered on asynchronous completion</param>
        public void LoadSerializable<T, I>(T serialized, I serializable, Transform parent,
            Action<I> onLoaded = null)
            where T : VersionedType
            where I : IVersioned
        {
            if (serializable != null)
            {
                _loading = true;

                // Delegate the serializable loaded action
                Action<bool, string> OnSerializableLoadedAction = (bool loaded, string message) =>
                {
                    if (loaded)
                    {
                        // Set the parent if applicable
                        if (serializable is Component)
                        {
                            // Make sure we have a valid parent. Default is the scene objects container
                            Transform parentContainer = (parent != null) ? parent : ProjectManager.SceneObjectContainer.transform;

                            Component serializableComponent = serializable as Component;
                            // serializableComponent.gameObject.transform.localPosition = Vector3.zero;
                            // serializableComponent.gameObject.transform.localRotation = Quaternion.identity;
                            // serializableComponent.gameObject.transform.localScale = Vector3.one;
                            serializableComponent.gameObject.transform.parent = parentContainer;
                        }

                        OnSerializableLoaded(serializable, message, onLoaded);
                    }
                    else
                    {
                        LogError(message, nameof(LoadSerializable));
                        onLoaded?.Invoke(default);
                    }

                    _loading = false;
                };

                // Create the serializable and deserialize
                try
                {
                    // Deserialize the serializable
                    serializable.Deserialize(serialized, OnSerializableLoadedAction);
                }
                catch (Exception e)
                {
                    LogError("A problem occurred while deserializing the serializable object: " + e,
                        nameof(LoadSerializable));
                    onLoaded?.Invoke(default);
                }
            }
            else
            {
                LogError("The supplied serializable object is null", nameof(LoadSerializable));
                onLoaded?.Invoke(default);
            }
        }

        /// <summary>
        /// Triggered upon loading (deserialization) completion.
        /// </summary>
        /// <typeparam name="I">The serializable class type descending from a class that implements
        ///     <code>IVersioned</code></typeparam>
        /// <param name="serializable">The serialzable class instance that was loaded, or null on failure</param>
        /// <param name="message">May contain an error message on a loading error</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be triggered on asynchronous completion</param>
        protected void OnSerializableLoaded<I>(I serializable,
            string message, Action<I> onLoaded = null)
            where I : IVersioned
        {
            StartCoroutine(OnSerializableLoadedAsync(serializable, message, onLoaded));
        }

        /// <summary>
        /// Asynchronously triggered upon loading (deserialization) completion.
        /// </summary>
        /// <typeparam name="I">The serializable class type descending from a class that implements
        ///     <code>IVersioned</code></typeparam>
        /// <param name="serializable">The serialzable class instance that was loaded, or null on failure</param>
        /// <param name="message">May contain an error message on a loading error</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be triggered on asynchronous completion</param>
        protected IEnumerator OnSerializableLoadedAsync<I>(I serializable,
            string message, Action<I> onLoaded = null)
            where I : IVersioned
        {
            if (serializable == null)
            {
                string logMessage = "The serializable loading failed";
                if (!string.IsNullOrEmpty(message))
                {
                    logMessage += ": " + message;
                }
                LogError(logMessage, nameof(OnSerializableLoadedAsync));
            }

            // Notify
            onLoaded?.Invoke(serializable);

            yield return null;
        }
        #endregion Serializable Instantiation
    }
}
