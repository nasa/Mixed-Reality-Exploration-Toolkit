// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 26 Mar 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// MRETSerializableManager
	///
	/// Provides helper methods for MRET managers requiring instantiation
    /// and deserialization of serializable classes in MRET.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class MRETSerializableManager<M> : MRETUpdateManager<M>
        where M : IManager
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETSerializableManager<M>);

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

        /// <seealso cref="MRETUpdateSingleton{T}.Initialize"/>
        public override void Initialize()
		{
			// Take the inherited behavior
			base.Initialize();

			// TODO: Custom initialization (before deserialization)
			
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
        /// Instantiates a serializable object associated with the supplied serialized
        /// instance and adds it to the supplied game object.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>VersionedType</code> specifies
        ///     the serialized type being used to instantiate the serializable</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     serializable type being instantiated</typeparam>
        /// <param name="serialized">The serialized instance used to instantiate the serializable</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated serializable.
        ///     If not provided, one will be created</param>
        /// <param name="parent">The parent <code>Transform</code> of the supplied game object. If null, the
        ///     project object container will be used</param>
        /// <param name="onFinished">The optional <code>Action</code> to be triggered on asynchronous completion</param>
        private void InternalInstantiateSerializable<T, I>(T serialized, GameObject go, Transform parent,
            Action<I> onFinished = null)
            where T : VersionedType
            where I : IVersioned
        {
            StartCoroutine(InternalInstantiateSerializableAsync(serialized, go, parent, onFinished));
        }

        /// <summary>
        /// Asynchronously instantiates a serializable object associated with the supplied serialized
        /// instance and adds it to the supplied game object.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>VersionedType</code> specifies
        ///     the serialized type being used to instantiate the serializable</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     serializable type being instantiated</typeparam>
        /// <param name="serialized">The serialized instance used to instantiate the serializable</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated serializable.
        ///     If not provided, one will be created</param>
        /// <param name="parent">The parent <code>Transform</code> of the supplied game object. If null, the
        ///     project object container will be used</param>
        /// <param name="onFinished">The optional <code>Action</code> to be triggered on asynchronous completion</param>
        private IEnumerator InternalInstantiateSerializableAsync<T, I>(T serialized, GameObject go,
            Transform parent, Action<I> onFinished = null)
            where T : VersionedType
            where I : IVersioned
        {
            Log("Instantiation starting: " + typeof(T).Name);

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

            // Create a new instantiator
            SerializableInstantiation instantiator = go.AddComponent<SerializableInstantiation>();

            // Delegate the identifiable serialize action
            Action<I> SerializableLoadedAction = (I serializable) =>
            {
                // Surface to caller if specified
                onFinished?.Invoke(serializable);

                // Destroy the instantiator
                Destroy(instantiator);

                // Cleanup on failure
                if ((serializable == null) && goCreated)
                {
                    Destroy(go);
                }

                Log("Instantiation complete: " + typeof(T).Name);
            };

            try
            {
                instantiator.InstantiateSerializable(serialized, go, parent, SerializableLoadedAction);
            }
            catch (Exception e)
            {
                LogError("A problem occurred during instantiation of the serializable: " + e);
                onFinished?.Invoke(default);
            }

            yield return null;
        }

        /// <summary>
        /// Instantiates a serializable object associated with the supplied serialized
        /// instance and adds it to the suppplied container.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>VersionedType</code> specifies
        ///     the serialized type being used to instantiate the serializable</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     serializable type being instantiated</typeparam>
        /// <param name="serialized">The serialized instance to be used to instantiate the serializable</param>
        /// <param name="serializableGO">The optional <code>GameObject</code> that will contain the instantiated
        ///     serializable. If not provided, one will be created</param>
        /// <param name="serializableContainer">The container <code>Transform</code> for the instantiated serializable.
        ///     If null, the container supplied by the <code>GetDefaultProjectContainer</code> method will be used</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on completion</param>
        /// <param name="finishSerializableInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the serializable
        ///     instantiation. Called before the onLoaded action is called. If not specified, a default logging
        ///     behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the finishSerializableInstantiation
        ///     method to provide additional context</param>
        /// <seealso cref="GetDefaultSerializableContainer{T}(T)"/>
        protected void InstantiateSerializable<T, I>(T serialized, GameObject serializableGO = null,
            Transform serializableContainer = null, Action<I> onLoaded = null,
            FinishSerializableInstantiationDelegate<T, I> finishSerializableInstantiation = null,
            params object[] context)
            where T : VersionedType
            where I : IVersioned
        {
            // Make sure we have a default behavior when finishing the instantiation
            finishSerializableInstantiation ??= FinishSerializableInstantiation;

            // Delegate the deserialize action
            Action<I> InstantiatedSerializableAction = (I serializable) =>
            {
                // Complete the instantiation
                finishSerializableInstantiation?.Invoke(serialized, serializable, context);

                // Surface the on loaded event
                OnSerializableLoaded(serializable, onLoaded);
            };

            // Make sure we have a valid container reference
            serializableContainer = (serializableContainer == null)
                ? GetDefaultSerializableContainer(serialized)
                : serializableContainer;

            // Instantiate and load the serializable
            InternalInstantiateSerializable(serialized, serializableGO,
                serializableContainer, InstantiatedSerializableAction);
        }

        /// <summary>
        /// Instantiates an array of serializable object associated with the supplied serialized array
        /// instances and adds them to the container.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>VersionedType</code> specifies
        ///     the serialized type being used to instantiate the serializable</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     serializable type being instantiated</typeparam>
        /// <param name="serializedArray">The array of serialized instances to be used to instantiate the serializables</param>
        /// <param name="serializableGO">The optional <code>GameObject</code> that will contain the instantiated
        ///     serializables. If not provided, one will be created</param>
        /// <param name="serializableContainer">The container <code>Transform</code> for the instantiated serializable.
        ///     If null, the container supplied by the <code>GetDefaultProjectContainer</code> method will be used</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on completion</param>
        /// <param name="instantiateSerializable">The optional
        ///     <code>InstantiateSerializableDelegate</code> method to be called to instantiate the serializable.
        ///     Called for each serializable. If not specified, a default instantiation behavior will be used.</param>
        /// <param name="finishSerializableInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the serializable
        ///     instantiation. Called for each serializable. If not specified, a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the finishSerializableInstantiation method
        ///     to provide additional context</param>
        /// <seealso cref="GetDefaultSerializableContainer{T}(T)"/>
        protected void InstantiateSerializables<T, I>(T[] serializedArray, GameObject serializableGO = null,
            Transform serializableContainer = null, Action<I[]> onLoaded = null,
            InstantiateSerializableDelegate<T, I> instantiateSerializable = null,
            FinishSerializableInstantiationDelegate<T, I> finishSerializableInstantiation = null,
            params object[] context)
            where T : VersionedType
            where I : IVersioned
        {
            StartCoroutine(InstantiateSerializablesAsync(serializedArray, serializableGO, serializableContainer, onLoaded,
                instantiateSerializable, finishSerializableInstantiation, context));
        }

        /// <summary>
        /// Instantiates an array of serializable object associated with the supplied serialized array
        /// instances and adds them to the container.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>VersionedType</code> specifies
        ///     the serialized type being used to instantiate the serializable</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     serializable type being instantiated</typeparam>
        /// <param name="serializedArray">The array of serialized instances to be used to instantiate the serializables</param>
        /// <param name="serializableGO">The optional <code>GameObject</code> that will contain the instantiated
        ///     serializables. If not provided, one will be created</param>
        /// <param name="serializableContainer">The container <code>Transform</code> for the instantiated serializable.
        ///     If null, the container supplied by the <code>GetDefaultProjectContainer</code> method will be used</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on completion</param>
        /// <param name="instantiateSerializable">The optional
        ///     <code>InstantiateSerializableDelegate</code> method to be called to instantiate the serializable.
        ///     Called for each serializable. If not specified, a default instantiation behavior will be used.</param>
        /// <param name="finishSerializableInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the serializable
        ///     instantiation. Called for each serializable. If not specified, a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the finishSerializableInstantiation method
        ///     to provide additional context</param>
        /// <seealso cref="GetDefaultSerializableContainer{T}(T)"/>
        protected IEnumerator InstantiateSerializablesAsync<T, I>(T[] serializedArray, GameObject serializableGO = null,
            Transform serializableContainer = null, Action<I[]> onLoaded = null,
            InstantiateSerializableDelegate<T, I> instantiateSerializable = null,
            FinishSerializableInstantiationDelegate<T, I> finishSerializableInstantiation = null,
            params object[] context)
            where T : VersionedType
            where I : IVersioned
        {
            // Make sure we have a default behaviors for instantiation and finishing the instantiation
            instantiateSerializable ??= InstantiateSerializable;
            finishSerializableInstantiation ??= FinishSerializableInstantiation;

            // Maintain a list of serializables to supply at the end of instantiating all serializables
            List<I> serializables = new List<I>();

            // Instantiate all the serializables
            foreach (T serialized in serializedArray)
            {
                bool complete = false;

                Action<I> SerializableLoadedAction = (I serializable) =>
                {
                    // Check for success or failure
                    if (serializable != null)
                    {
                        serializables.Add(serializable);
                    }
                    else
                    {
                        string logWarning = "A problem was encountered instantiating the serializable";
                        if (serialized is IdentifiableType)
                        {
                            logWarning += ": " + (serialized as IdentifiableType).Name;
                        }

                        // Record the warning
                        LogWarning(logWarning);
                    }

                    // Mark as complete
                    complete = true;
                };

                // Instantiate the serializable
                instantiateSerializable(serialized, serializableGO, serializableContainer, SerializableLoadedAction,
                    finishSerializableInstantiation);

                // Wait for the instantiation to complete
                while (!complete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
            }

            // Surface the event
            OnSerializablesLoaded(serializables.ToArray(), onLoaded);
        }

        /// <seealso cref="FinishSerializableInstantiationDelegate{T, I}"/>
        private void FinishSerializableInstantiation<T, I>(T serializedObject, I instantiatedObject,
            params object[] context)
            where T : VersionedType
            where I : IVersioned
        {
            // Grab the name if available
            string objectName = (serializedObject is IdentifiableType) && !string.IsNullOrEmpty((serializedObject as IdentifiableType)?.Name)
                ? "\"" + (serializedObject as IdentifiableType).Name + "\" "
                : "";
            string objectType = typeof(I).Name;

            // Check for success or failure
            if (instantiatedObject != null)
            {
                Log(objectType + " " + objectName + "instantiation complete");
            }
            else
            {
                // Log the error
                LogError(objectType + " " + objectName + "instantiation failed");
            }
        }

        /// <summary>
        /// Defines a delegate for a function that instantiates a serializable object associated with the
        /// supplied serialized instance and adds it to the suppplied container.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>VersionedType</code> specifies
        ///     the serialized type being used to instantiate the serializable</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     serializable type being instantiated</typeparam>
        /// <param name="serialized">The serialized instance to be used to instantiate the serializable</param>
        /// <param name="serializableGO">The optional <code>GameObject</code> that will contain the instantiated
        ///     serializable. If not provided, one will be created</param>
        /// <param name="serializableContainer">The container <code>Transform</code> for the instantiated serializable.
        ///     If null, the container supplied by the <code>GetDefaultProjectContainer</code> method will be used</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on completion</param>
        /// <param name="finishSerializableInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the serializable
        ///     instantiation. Called before the onLoaded action is called. If not specified, a default logging
        ///     behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the finishSerializableInstantiation
        ///     method to provide additional context</param>
        /// <seealso cref="GetDefaultSerializableContainer{T}(T)"/>
        public delegate void InstantiateSerializableDelegate<T, I>(T serialized, GameObject serializableGO = null,
            Transform serializableContainer = null, Action<I> onLoaded = null,
            FinishSerializableInstantiationDelegate<T, I> finishSerializableInstantiation = null,
            params object[] context)
            where T : VersionedType
            where I : IVersioned;

        /// <summary>
        /// Defines a delegate for a function that is called to perform any configuration of the
        /// newly instantiated serializable.
        /// </summary>
        /// <typeparam name="T">The generic type parameter derived from <code>VersionedType</code> specifies
        ///     the serialized type used to instantiate the serializable</typeparam>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     instantiated serializable type</typeparam>
        /// <param name="serialized">The serialized instance used to instantiate the serializable</param>
        /// <param name="serializable">The newly instantiated serializable. Null if instantiation failed.</param>
        /// <param name="context">Optional context parameters to be supplied</param>
        public delegate void FinishSerializableInstantiationDelegate<T, I>(T serialized, I serializable,
            params object[] context)
            where T : VersionedType
            where I : IVersioned;

        /// <summary>
        /// Called to trigger the supplied action containing the newly instantiated serializable.
        /// </summary>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     instantiated serializable type</typeparam>
        /// <param name="serializable">The newly instantiated serializable. Will be null if instantiation
        ///     failed.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be called upon completion of loading
        ///     of the newly instantiated serializable.</param>
        private void OnSerializableLoaded<I>(I serializable, Action<I> onLoaded = null)
            where I : IVersioned
        {
            StartCoroutine(OnSerializableLoadedAsync(serializable, onLoaded));
        }

        /// <summary>
        /// Called to asynchronously trigger the supplied action containing the newly instantiated serializable.
        /// </summary>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     instantiated serializable type</typeparam>
        /// <param name="serializable">The newly instantiated serializable. Will be null if instantiation
        ///     failed.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be called upon completion of loading
        ///     of the newly instantiated serializable.</param>
        private IEnumerator OnSerializableLoadedAsync<I>(I serializable, Action<I> onLoaded = null)
            where I : IVersioned
        {
            // Notify caller the serializable loading is complete
            onLoaded?.Invoke(serializable);

            yield return null;
        }

        /// <summary>
        /// Called to trigger the supplied action containing the newly instantiated array of serializables.
        /// </summary>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     instantiated serializable type</typeparam>
        /// <param name="serializables">The newly instantiated array of serializables. Will be empty if instantiation
        ///     failed.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be called upon completion of loading
        ///     of the newly instantiated serializable.</param>
        private void OnSerializablesLoaded<I>(I[] serializables, Action<I[]> onLoaded = null)
            where I : IVersioned
        {
            StartCoroutine(OnSerializablesLoadedAsync(serializables, onLoaded));
        }

        /// <summary>
        /// Called to asynchronously trigger the supplied action containing the newly instantiated array of serializables.
        /// </summary>
        /// <typeparam name="I">The generic type parameter derived from <code>IVersioned</code> specifies the
        ///     instantiated serializable type</typeparam>
        /// <param name="serializable">The newly instantiated serializable. Will be null if instantiation
        ///     failed.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be called upon completion of loading
        ///     of the newly instantiated serializable.</param>
        private IEnumerator OnSerializablesLoadedAsync<I>(I[] serializables, Action<I[]> onLoaded = null)
            where I : IVersioned
        {
            // Notify caller the serializable loading is complete
            onLoaded?.Invoke(serializables);

            yield return null;
        }
        #endregion Serializable Instantiation

        /// <summary>
        /// Abstract method to supply the default serializable container transform for all instantiated
        /// serializables. Will be called if a container is not specified by the invoking class for
        /// any instantiation methods.
        /// </summary>
        /// <returns>The <code>Transform</code> defining the default serializable container</returns>
        protected abstract Transform GetDefaultSerializableContainer<T>(T serialized)
            where T : VersionedType;

    }
}
