// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 20 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// Versioned object.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    /// <seealso cref="IVersioned{T}"/>
    /// <see cref="VersionedType"/>
    /// 
    public abstract class Versioned<T> : IVersioned<T>
        where T : VersionedType, new()
    {
        /// <summary>
        /// The name of this class. Subclasses should override this accessor for logging purposes.
        /// 
        /// Example:
        /// <code>
        ///     public override string ClassName => nameof(MyMRETBehaviourSubclass);
        /// </code>
        /// </summary>
        public abstract string ClassName
        {
            get;
        }

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedVersioned;

        /// <summary>
        /// The version of this <code>Versioned</code>.<br>
        /// 
        /// <returns>A <code>string</code></returns>
        /// </summary>
        public string version { get; private set; }

        /// <summary>
        /// Called to initialize this class instance
        /// </summary>
        protected virtual void Initialize()
        {
            // Set the defaults
            version = VersionedDefaults.VERSION;

#if DESERIALIZE_LOCK
            // Create the serialization lock
            deserializationLock = new UnityEngine.Object();
#endif
#if SERIALIZE_LOCK
            // Create the serialization lock
            serializationLock = new UnityEngine.Object();
#endif
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Versioned()
        {
            Initialize();
        }

        // Performance tracking
        private DateTime deserializationStart;
        private DateTime serializationStart;

#if DESERIALIZE_LOCK
        private UnityEngine.Object deserializationLock;
#endif
#if SERIALIZE_LOCK
        private UnityEngine.Object serializationLock;
#endif

        #region IVersioned<T>
        /// <seealso cref="IVersioned{T}.CreateSerializedType"/>
        public virtual T CreateSerializedType()
        {
            return new T();
        }

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        VersionedType IVersioned.CreateSerializedType() => CreateSerializedType();

#region Deserialization
        /// <summary>
        /// Completes the deserialization.
        /// </summary>
        /// <param name="successful">Indicates whether or not the deserialization was successfully completed</param>
        /// <param name="message">Contains an optional message if unsuccessful</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the deserialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        private void DeserializeComplete(bool successful, string message, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            DateTime deserializationStop = DateTime.Now;
            double elapsedMilliseconds = deserializationStop.Subtract(deserializationStart).TotalMilliseconds;

            // Log a message
            if (successful)
            {
                Log("Deserialization complete. Elapsed time: " + elapsedMilliseconds + "ms", nameof(DeserializeComplete));
            }
            else
            {
                string logMessage = "Deserialization failed";
                if (string.IsNullOrEmpty(message))
                {
                    logMessage += ": " + message;
                }
                LogWarning(logMessage, nameof(DeserializeComplete));
            }

            // Notify the action delegate
            onFinished?.Invoke(successful, message);
        }

        /// <summary>
        /// Performs sequential deserialization.
        /// </summary>
        /// <param name="serialized">The serialized object to deserialize into this object instance</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the deserialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        private void InternalDeserialize(T serialized, Action<bool, string> onFinished = null)
        {
#if DESERIALIZE_LOCK
            lock (deserializationLock)
            {
#endif
                // Initialize our deserialization state object
                SerializationState deserializationState = new SerializationState();

                // Perform the deserialization
                Deserialize(serialized, deserializationState);

                // Notify the action delegate
                onFinished?.Invoke(!deserializationState.IsError, deserializationState.ErrorMessage);
#if DESERIALIZE_LOCK
            }
#endif
        }

        /// <seealso cref="IVersioned{T}.Deserialize(T, Action{bool, string})"/>
        public void Deserialize(T serialized, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            deserializationStart = DateTime.Now;

            Action<bool, string> DeserializeAction = (bool successful, string message) =>
            {
                DeserializeComplete(successful, message, onFinished);
            };

            // Make sure to catch exceptions and finish gracefully
            try
            {
                InternalDeserialize(serialized, DeserializeAction);
            }
            catch (Exception e)
            {
                DeserializeComplete(false, e.Message, onFinished);
            }
        }

        /// <summary>
        /// Helper method to perform deserialization on this <code>Versioned</code>
        /// and log messages based upon the state of the deserialization process.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to deserialize into this <code>Versioned</code></param>
        /// <param name="deserializationState">Maintains the deserialization state</param>
        public void DeserializeWithLogging(T serialized, SerializationState deserializationState)
        {
#if DESERIALIZE_LOCK
            lock (deserializationLock)
            {
#endif
                // Perform the deserialization
                // Make sure to catch exceptions and finish gracefully
                try
                {
                    Deserialize(serialized, deserializationState);
                }
                catch (Exception e)
                {
                    deserializationState.Error(e.Message);
                }
#if DESERIALIZE_LOCK
            }
#endif

            // If the deserialization errored, log the error
            if (deserializationState.IsError)
            {
                // Log the error
                string message = "A problem occurred while deserializing";
                if (!string.IsNullOrEmpty(deserializationState.ErrorMessage))
                {
                    message += ": " + deserializationState.ErrorMessage;
                }
                LogError(message, nameof(DeserializeWithLogging));
            }
        }

        /// <summary>
        /// Deserializes the supplied serialized settings into this object instance.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to deserialize into this object instance</param>
        /// <param name="deserializationState">Maintains the deserialization state</param>
        protected virtual void Deserialize(T serialized, SerializationState deserializationState)
        {
            // Save the serialized reference
            serializedVersioned = serialized;

            // Deserialize the version
            version = serializedVersioned.version;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }
#endregion Deserialization

#region Serialization
        /// <summary>
        /// Completes the serialization.
        /// </summary>
        /// <param name="successful">Indicates whether or not the serialization was successfully completed</param>
        /// <param name="message">Contains an optional message if unsuccessful</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the serialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        private void SerializeComplete(bool successful, string message, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            DateTime serializationStop = DateTime.Now;
            double elapsedMilliseconds = serializationStop.Subtract(serializationStart).TotalMilliseconds;

            // Log a message
            if (successful)
            {
                Log("Serialization complete. Elapsed time: " + elapsedMilliseconds + "ms", nameof(SerializeComplete));
            }
            else
            {
                LogWarning("Serialization failed", nameof(SerializeComplete));
            }

            // Notify the action delegate
            onFinished?.Invoke(successful, message);
        }

        /// <summary>
        /// Performs sequential serialization.
        /// </summary>
        /// <param name="serialized">The serialized object to write the serialization of this object instance</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the serialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        private void InternalSerialize(T serialized, Action<bool, string> onFinished = null)
        {
#if SERIALIZE_LOCK
            lock (serializationLock)
            {
#endif
                // Initialize our serialization state object
                SerializationState serializationState = new SerializationState();

                // Perform the serialization
                Serialize(serialized, serializationState);

                // Notify the action delegate
                onFinished?.Invoke(!serializationState.IsError, serializationState.ErrorMessage);
#if SERIALIZE_LOCK
            }
#endif
        }

        /// <seealso cref="IVersioned{T}.Serialize(T, Action{bool, string})"/>
        public void Serialize(T serialized, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            serializationStart = DateTime.Now;

            Action<bool, string> SerializeAction = (bool successful, string message) =>
            {
                SerializeComplete(successful, message, onFinished);
            };

            // Make sure to catch exceptions and finish gracefully
            try
            {
                InternalSerialize(serialized, SerializeAction);
            }
            catch (Exception e)
            {
                SerializeComplete(false, e.Message, onFinished);
            }
        }

        /// <summary>
        /// Helper method to perform serialization on this <code>Versioned</code>
        /// and log messages based upon the state of the serialization process.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to store the supplied <code>Versioned</code> serialization</param>
        /// <param name="serializationState">Maintains the serialization state</param>
        public void SerializeWithLogging(T serialized, SerializationState serializationState)
        {
#if SERIALIZE_LOCK
            lock (serializationLock)
            {
#endif
                // Perform the serialization
                // Make sure to catch exceptions and finish gracefully
                try
                {
                    Serialize(serialized, serializationState);
                }
                catch (Exception e)
                {
                    serializationState.Error(e.Message);
                }
#if SERIALIZE_LOCK
            }
#endif

            // If the serialization errored, Log the error
            if (serializationState.IsError)
            {
                // Log the error
                string message = "A problem occurred while serializing";
                if (!string.IsNullOrEmpty(serializationState.ErrorMessage))
                {
                    message += ": " + serializationState.ErrorMessage;
                }
                LogError(message, nameof(SerializeWithLogging));
            }
        }

        /// <summary>
        /// Serializes this object instance into the supplied serialized object.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to write the serialization of this object instance</param>
        /// <param name="serializationState">Maintains the serialization state</param>
        protected virtual void Serialize(T serialized, SerializationState serializationState)
        {
            // Make sure we have a valid serialized reference
            if (serialized == null)
            {
                serialized = CreateSerializedType();
            }

            // Serialize the version
            serialized.version = version;

            // Save the final serialized reference
            serializedVersioned = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
#endregion Serialization
#endregion IVersioned<T>

#region IVersioned
        /// <seealso cref="Common.Schemas.IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(VersionedType serialized, Action<bool, string> onFinished = null)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="Common.Schemas.IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(VersionedType serialized, Action<bool, string> onFinished = null)
        {
            Serialize(serialized as T, onFinished);
        }
#endregion IVersioned

#region Logging
        /// <summary>
        /// Builds a standard MRET format console log message.
        /// </summary>
        /// <param name="details">The details of the message to log</param>
        /// <param name="callingMethod">The optional calling method. Use of <code>nameof(MyMethod)</code> is encouraged.</param>
        private string BuildLogMessage(string details, string callingMethod = null)
        {
            // Build the log message
            string logMessage = "[" + ClassName;
            if (!string.IsNullOrEmpty(callingMethod))
            {
                logMessage += "->" + callingMethod;
            }
            logMessage += "] " + details;

            return logMessage;
        }

        /// <summary>
        /// Provided to subclasses to provide a mecahnism to log a message to the console
        /// in a common format across MRET.
        /// </summary>
        /// <param name="details">The details of the message to log</param>
        /// <param name="callingMethod">The optional calling method. Use of <code>nameof(MyMethod)</code> is encouraged.</param>
        protected void Log(string details, string callingMethod = null)
        {
            // Build the log message
            string logMessage = BuildLogMessage(details, callingMethod);

            // Log the message
            Debug.Log(logMessage);
        }

        /// <summary>
        /// Provided to subclasses to provide a mecahnism to log a warning message to the console
        /// in a common format across MRET.
        /// </summary>
        /// <param name="details">The details of the message to log</param>
        /// <param name="callingMethod">The optional calling method. Use of <code>nameof(MyMethod)</code> is encouraged.</param>
        protected void LogWarning(string details, string callingMethod = null)
        {
            // Build the log message
            string logMessage = BuildLogMessage(details, callingMethod);

            // Log the warning message
            Debug.LogWarning(logMessage);
        }

        /// <summary>
        /// Provided to subclasses to provide a mecahnism to log a error message to the console
        /// in a common format across MRET.
        /// </summary>
        /// <param name="details">The details of the message to log</param>
        /// <param name="callingMethod">The optional calling method. Use of <code>nameof(MyMethod)</code> is encouraged.</param>
        protected void LogError(string details, string callingMethod = null)
        {
            // Build the log message
            string logMessage = BuildLogMessage(details, callingMethod);

            // Log the error message
            Debug.LogError(logMessage);
        }
#endregion Logging
    }

    /// <remarks>
    /// History:
    /// 20 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// Versioned object based on MRETUpdateBehaviour.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    /// <seealso cref="MRETBehaviour"/>
    /// <seealso cref="IVersioned{T}"/>
    /// <see cref="VersionedType"/>
    /// 
    public abstract class VersionedMRETBehaviour<T> : MRETUpdateBehaviour, IVersioned<T>
        where T : VersionedType, new()
    {
        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedVersioned;

        // Performance tracking
        private DateTime deserializationStart;
        private DateTime serializationStart;

#if DESERIALIZE_LOCK
        private UnityEngine.Object deserializationLock;
#endif
#if SERIALIZE_LOCK
        private UnityEngine.Object serializationLock;
#endif

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
            version = VersionedDefaults.VERSION;

#if DESERIALIZE_LOCK
            // Create the serialization lock
            deserializationLock = new UnityEngine.Object();
#endif
#if SERIALIZE_LOCK
            // Create the serialization lock
            serializationLock = new UnityEngine.Object();
#endif
        }
        #endregion MRETUpdateBehaviour

        #region IVersioned
        /// <seealso cref="IVersioned.InstanceType"/>
        public Type InstanceType
        {
            get => GetType();
        }

        /// <seealso cref="IVersioned.SerializedType"/>
        public Type SerializedType
        {
            get => CreateSerializedType().GetType();
        }

        /// <seealso cref="IVersioned.version"/>
        public string version { get; private set; }

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        public virtual T CreateSerializedType()
        {
            return new T();
        }

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        VersionedType IVersioned.CreateSerializedType() => CreateSerializedType();

#region Serialization
        /// <summary>
        /// Completes the deserialization.
        /// </summary>
        /// <param name="successful">Indicates whether or not the deserialization was successfully completed</param>
        /// <param name="message">Contains an optional message if unsuccessful</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the deserialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        private void DeserializeComplete(bool successful, string message, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            DateTime deserializationStop = DateTime.Now;
            double elapsedMilliseconds = deserializationStop.Subtract(deserializationStart).TotalMilliseconds;

            // Log a message
            if (successful)
            {
                Log("Deserialization complete. Elapsed time: " + elapsedMilliseconds + "ms", nameof(DeserializeComplete));
            }
            else
            {
                string logMessage = "Deserialization failed";
                if (string.IsNullOrEmpty(message))
                {
                    logMessage += ": " + message;
                }
                LogWarning(logMessage, nameof(DeserializeComplete));
            }

            // Notify the action delegate
            onFinished?.Invoke(successful, message);
        }

        /// <summary>
        /// Performs asynchronous deserialization with caching.
        /// </summary>
        /// <param name="serialized">The serialized object to deserialize into this object instance</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the deserialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        /// <returns>The <code>IEnumerator</code> used for reentry during reentrant "coroutine" processing</returns>
        private IEnumerator InternalDeserialize(T serialized, Action<bool, string> onFinished = null)
        {
#if DESERIALIZE_LOCK
            lock (deserializationLock)
            {
#endif
                // Initialize our deserialization state object
                SerializationState deserializationState = new SerializationState();

                // Deserialize as a new coroutine in case there's a break on error
                StartCoroutine(Deserialize(serialized, deserializationState));

                // Wait for the coroutine to complete
                while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Notify the action delegate
                onFinished?.Invoke(!deserializationState.IsError, deserializationState.ErrorMessage);
#if DESERIALIZE_LOCK
            }
#endif

            yield return null;
        }

        /// <seealso cref="IVersioned{T}.Deserialize(T, Action{bool, string})"/>
        public void Deserialize(T serialized, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            deserializationStart = DateTime.Now;

            Action<bool, string> DeserializeAction = (bool successful, string message) =>
            {
                DeserializeComplete(successful, message, onFinished);
            };

            // Make sure to catch exceptions and finish gracefully
            try
            {
                StartCoroutine(InternalDeserialize(serialized, DeserializeAction));
            }
            catch (Exception e)
            {
                DeserializeComplete(false, e.Message, onFinished);
            }
        }

        /// <summary>
        /// Reentrant helper method to perform deserialization on this <code>VersionedMRETBehaviour</code>
        /// and log messages based upon the state of the deserialization process.<br>
        /// 
        /// NOTE: This function will not break out of the reentrant processing so that the caller can decide
        /// how to manage an error condition.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to deserialize into this <code>VersionedMRETBehaviour</code></param>
        /// <param name="serializationState">Maintains the deserialization state</param>
        /// <returns>The <code>IEnumerator</code> used for reentry during reentrant "coroutine" processing</returns>
        public IEnumerator DeserializeWithLogging(T serialized, SerializationState deserializationState)
        {
#if DESERIALIZE_LOCK
            lock (deserializationLock)
            {
#endif
                // Make sure to catch exceptions and finish gracefully
                try
                {
                    // Serialize as a new coroutine in case there's a break on error
                    StartCoroutine(Deserialize(serialized, deserializationState));
                }
                catch (Exception e)
                {
                    deserializationState.Error(e.Message);
                }

                // Wait for the coroutine to complete
                while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
#if DESERIALIZE_LOCK
            }
#endif

            // If the deserialization errored, log the error
            if (deserializationState.IsError)
            {
                // Log the error
                string message = "A problem occurred while deserializing";
                if (!string.IsNullOrEmpty(deserializationState.ErrorMessage))
                {
                    message += ": " + deserializationState.ErrorMessage;
                }
                LogError(message, nameof(DeserializeWithLogging));

                // Abort coroutine
                yield break;
            }

            yield return null;
        }

        /// <summary>
        /// Deserializes the supplied serialized settings into this object instance.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to deserialize into this object instance</param>
        /// <param name="deserializationState">Maintains the deserialization state</param>
        /// <returns>The <code>IEnumerator</code> used for reentry during reentrant "coroutine" processing</returns>
        protected virtual IEnumerator Deserialize(T serialized, SerializationState deserializationState)
        {
            // Save the serialized reference
            serializedVersioned = serialized;

            // Deserialize the version
            version = serializedVersioned.version;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Completes the serialization.
        /// </summary>
        /// <param name="successful">Indicates whether or not the serialization was successfully completed</param>
        /// <param name="message">Contains an optional message if unsuccessful</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the serialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        private void SerializeComplete(bool successful, string message, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            DateTime serializationStop = DateTime.Now;
            double elapsedMilliseconds = serializationStop.Subtract(serializationStart).TotalMilliseconds;

            // Log a message
            if (successful)
            {
                Log("Serialization complete. Elapsed time: " + elapsedMilliseconds + "ms", nameof(SerializeComplete));
            }
            else
            {
                LogWarning("Serialization failed", nameof(SerializeComplete));
            }

            // Notify the action delegate
            onFinished?.Invoke(successful, message);
        }

        /// <summary>
        /// Performs asynchronous serialization.
        /// </summary>
        /// <param name="serialized">The serialized object to write the serialization of this object instance</param>
        /// <param name="onFinished">
        ///     The <code>Action</code> called when the serialization is complete.
        ///     arg1: A boolean field indicates successful completion.
        ///     arg2: A string field containing an optional message if unsuccessful.
        /// </param>
        /// <returns>The <code>IEnumerator</code> used for reentry during reentrant "coroutine" processing</returns>
        private IEnumerator InternalSerialize(T serialized, Action<bool, string> onFinished = null)
        {
#if SERIALIZE_LOCK
            lock (serializationLock)
            {
#endif
                // Initialize our serialization state object
                SerializationState serializationState = new SerializationState();

                // Serialize as a new coroutine in case there's a break on error
                StartCoroutine(Serialize(serialized, serializationState));

                // Wait for the coroutine to complete
                while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Notify the action delegate
                onFinished?.Invoke(!serializationState.IsError, serializationState.ErrorMessage);
#if SERIALIZE_LOCK
            }
#endif

            yield return null;
        }

        /// <seealso cref="IVersioned{T}.Serialize(T, Action{bool, string})"/>
        public void Serialize(T serialized, Action<bool, string> onFinished = null)
        {
            // Performance tracking
            serializationStart = DateTime.Now;

            Action<bool, string> SerializeAction = (bool successful, string message) =>
            {
                SerializeComplete(successful, message, onFinished);
            };

            // Make sure to catch exceptions and finish gracefully
            try
            {
                StartCoroutine(InternalSerialize(serialized, SerializeAction));
            }
            catch (Exception e)
            {
                SerializeComplete(false, e.Message, onFinished);
            }
        }

        /// <summary>
        /// Reentrant helper method to perform serialization on this <code>VersionedMRETBehaviour</code>
        /// and log messages based upon the state of the deserialization process.<br>
        /// 
        /// NOTE: This function will not break out of the reentrant processing so that the caller can decide
        /// how to manage an error condition.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to store the supplied <code>VersionedMRETBehaviour</code> serialization</param>
        /// <param name="serializationState">Maintains the serialization state</param>
        /// <returns>The <code>IEnumerator</code> used for reentry during reentrant "coroutine" processing</returns>
        public IEnumerator SerializeWithLogging(T serialized, SerializationState serializationState)
        {
#if SERIALIZE_LOCK
            lock (serializationLock)
            {
#endif
                // Make sure to catch exceptions and finish gracefully
                try
                {
                    // Serialize as a new coroutine in case there's a break on error
                    StartCoroutine(Serialize(serialized, serializationState));
                }
                catch (Exception e)
                {
                    serializationState.Error(e.Message);
                }

                // Wait for the coroutine to complete
                while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
#if SERIALIZE_LOCK
            }
#endif

            // If the serialization errored, Log the error
            if (serializationState.IsError)
            {
                // Log the error
                string message = "A problem occurred while serializing";
                if (!string.IsNullOrEmpty(serializationState.ErrorMessage))
                {
                    message += ": " + serializationState.ErrorMessage;
                }
                LogError(message, nameof(SerializeWithLogging));
            }

            yield return null;
        }

        /// <summary>
        /// Serializes this object instance into the supplied serialized object.<br>
        /// </summary>
        /// <param name="serialized">The serialized object to write the serialization of this object instance</param>
        /// <param name="serializationState">Maintains the serialization state</param>
        /// <returns>The <code>IEnumerator</code> used for reentry during reentrant "coroutine" processing</returns>
        protected virtual IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            // Make sure we have a valid serialized reference
            if (serialized == null)
            {
                serialized = CreateSerializedType();
            }

            // Serialize the version
            serialized.version = version;

            // Save the final serialized reference
            serializedVersioned = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization
        #endregion IVersioned<T>

        #region IVersioned
        /// <seealso cref="Common.Schemas.IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(VersionedType serialized, Action<bool, string> onFinished = null)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="Common.Schemas.IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(VersionedType serialized, Action<bool, string> onFinished = null)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IVersioned

        #region Coroutine Helpers
        /// <summary>
        /// Defines a delegate for a function that instantiates and deserializes a serializable from a serialized
        /// class instance representation.
        /// </summary>
        /// <typeparam name="VT">Specifies the <code>VersionedType</code> containing the serialized
        ///     representation</typeparam>
        /// <typeparam name="VI">Specifies the <code>IVersioned</code> serializable type to instantiate
        ///     and deserialize</typeparam>
        /// <param name="serializedVersionedType">The <code>VersionedType</code> containing the serialized
        ///     representation of the serializable</param>
        /// <param name="go">The <code>GameObject</code> to add the serialzable upon instantiation, or null
        ///     if one should be created as part of the instantiation process</param>
        /// <param name="parent">The parent <code>Transform</code> for the game object</param>
        /// <param name="onFinished">The <code>Action</code> to be called upon completion. The
        ///     argument will be the deserialized instance, or null on failure.</param>
        protected delegate void InstantiateAndDeserialize<VT, VI>(VT serializedVersionedType, GameObject go, Transform parent,
            Action<VI> onFinished = null)
            where VT : VersionedType
            where VI : IVersioned<VT>;

        /// <summary>
        /// Defines a delegate for a function that instantiates and deserializes an array of serializables
        /// from an array of serialized class instance representations.
        /// </summary>
        /// <typeparam name="VT">Specifies the <code>VersionedType</code> containing the serialized
        ///     representation</typeparam>
        /// <typeparam name="VI">Specifies the <code>IVersioned</code> serializable type to instantiate
        ///     and deserialize</typeparam>
        /// <param name="serializedVersionedTypeArray">The array of <code>VersionedType</code> containing the
        ///     serialized representation of the serializables in the array</param>
        /// <param name="go">The <code>GameObject</code> to add the serialzable upon instantiation, or null
        ///     if one should be created as part of the instantiation process</param>
        /// <param name="parent">The parent <code>Transform</code> for the game object</param>
        /// <param name="onFinished">The <code>Action</code> to be called upon completion. The
        ///     argument will be the deserialized instance, or null on failure.</param>
        protected delegate void InstantiateAndDeserializeArray<VT, VI>(VT[] serializedVersionedTypeArray,
            GameObject go, Transform parent,
            Action<VI[]> onFinished = null)
            where VT : VersionedType
            where VI : IVersioned<VT>;

        /// <summary>
        /// Deserializes the supplied serialized versioned type into a serializable versioned class
        /// instance. Available for subclasses.
        /// </summary>
        /// <typeparam name="VT">Specifies the <code>VersionedType</code> of the serialized class
        ///     representation</typeparam>
        /// <typeparam name="VI">Specifies the <code>IVersioned</code> serializable type to instantiate
        ///     and deserialize</typeparam>
        /// <param name="serializedVersionedType">The <code>VersionedType</code> containing the serialized
        ///     representation of the serializable</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated serializable.
        ///     If not provided, one will be created</param>
        /// <param name="parent">The parent <code>Transform</code> of the supplied game object. If null, the
        ///     project object container will be used</param>
        /// <param name="deserializationState">A <code>VersionedSerializationState</code> containing
        ///     the state of the deserialization process and the serializable of type <code>VI</code>
        ///     on success. Null on failure</param>
        /// <returns>An <code>IEnumerator</code> used for reentrance in coroutines</returns>
        protected virtual IEnumerator DeserializeVersioned<VT, VI>(VT serializedVersionedType,
            GameObject go, Transform parent, VersionedSerializationState<VI> deserializationState,
            InstantiateAndDeserialize<VT, VI> instantiateAndDeserialize = null)
            where VT : VersionedType
            where VI : IVersioned<VT>
        {
            Action<VI> OnSerializableLoadedAction = (VI loadedSerializable) =>
            {
                if (loadedSerializable != null)
                {
                    // Assign the object
                    deserializationState.versioned = loadedSerializable;

                    // Mark as complete
                    deserializationState.complete = true;
                }
                else
                {
                    // Record the error
                    deserializationState.Error("A problem was encountered deserializing the serialzable type: " + nameof(VI));
                }
            };

            // Instantiate and deserialize the serializable
            if (instantiateAndDeserialize == null)
            {
                // Default to the standard implementation
                MRET.ProjectManager.InstantiateObject(serializedVersionedType,
                    go, parent, OnSerializableLoadedAction);
            }
            else
            {
                instantiateAndDeserialize(serializedVersionedType,
                    go, parent, OnSerializableLoadedAction);
            }

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            yield return null;
        }

        /// <summary>
        /// Deserializes the supplied serialized versioned type array into an array of serializable
        /// versioned class instances. Available for subclasses.
        /// </summary>
        /// <typeparam name="VT">Specifies the <code>VersionedType</code> of the serialized class
        ///     representation</typeparam>
        /// <typeparam name="VI">Specifies the <code>IVersioned</code> serializable type to instantiate
        ///     and deserialize</typeparam>
        /// <param name="serializedVersionedArrayType">The array of <code>VersionedType</code>
        ///     containing the serialized representation of each serializable</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated serializables.
        ///     If not provided, one will be created</param>
        /// <param name="parent">The parent <code>Transform</code> of the supplied game object. If null, the
        ///     project object container will be used</param>
        /// <param name="deserializationState">A <code>VersionedArraySerializationState</code> containing
        ///     the state of the array deserialization process and the serializable array of type <code>VI[]</code>
        ///     on success. Null on failure</param>
        /// <returns>An <code>IEnumerator</code> used for reentrance in coroutines</returns>
        protected virtual IEnumerator DeserializeVersionedArray<VT, VI>(VT[] serializedVersionedArrayType,
            GameObject go, Transform parent, VersionedArraySerializationState<VI> deserializationState,
            InstantiateAndDeserialize<VT, VI> instantiateAndDeserialize = null)
            where VT : VersionedType
            where VI : IVersioned<VT>
        {
            Action<VI[]> OnSerialzableArrayLoadedAction = (VI[] loadedSerializableArray) =>
            {
                if ((loadedSerializableArray == null) || (loadedSerializableArray.Length == 0))
                {
                    // Assign the array
                    deserializationState.versionedArray = loadedSerializableArray;

                    // Mark as complete
                    deserializationState.complete = true;
                }
                else
                {
                    // Record the error
                    deserializationState.Error("A problem was encountered deserializing the serialzable type array: " + typeof(VI[]).Name);
                }
            };

            // Maintain a list of serializable instances to supply at the end of instantiating all serializables
            List<VI> serializableList = new List<VI>();

            // Instantiate all the serializables
            foreach (VT serializedVersionedType in serializedVersionedArrayType)
            {
                VersionedSerializationState<VI> serializableDeserializationState = new VersionedSerializationState<VI>();
                StartCoroutine(DeserializeVersioned(serializedVersionedType, go, parent, serializableDeserializationState, instantiateAndDeserialize));

                // Wait for the coroutine to complete
                while (!serializableDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(serializableDeserializationState);

                // Make sure the resultant child parts type is not null
                if (serializableDeserializationState.versioned is null)
                {
                    deserializationState.Error("Deserialized serializable cannot be null, denoting a possible internal issue.");
                }

                // If the group deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Add the serializable
                serializableList.Add(serializableDeserializationState.versioned);

                // Clear the state
                deserializationState.Clear();
            }

            // Wait for completion
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            yield return null;
        }

        #endregion Coroutine Helpers
    }

    /// <summary>
    /// Used to keep the default values from the schema in sync
    /// </summary>
    public class VersionedDefaults
    {
        public static readonly string VERSION = new VersionedType().version;
    }

    #region State
    /// <remarks>
    /// History:
    /// 20 September 2021: Created (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// SerializationState
    /// 
    /// Used within the class hierarchy to track the serialization state
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    /// <seealso cref="IVersioned{T}.Deserialize(T, Action{bool, string})"/>
    /// <seealso cref="IVersioned{T}.Serialize(T, Action{bool, string})"/>
    /// 
    public class SerializationState
    {
        public bool complete = false;
        private bool error = false;
        private string errorMessage = "";

        public bool IsComplete { get => complete; }
        public bool IsError { get => error; }
        public string ErrorMessage { get => errorMessage; }

        /// <summary>
        /// Clears the serialization state
        /// </summary>
        public void Clear()
        {
            complete = false;
            error = false;
            errorMessage = "";
        }

        /// <summary>
        /// Changes the state to an error condition.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="complete">Indicates if the serialization should be marked as complete</param>
        public void Error(string message, bool complete = true)
        {
            error = true;
            errorMessage = message;
            this.complete = complete;
        }

        /// <summary>
        /// Updates the serialization state with the supplied serialization state.
        /// </summary>
        /// <param name="fromState">The <code>SerializationState</code> from which to update</param>
        public void Update(SerializationState fromState, bool completeOnError = true)
        {
            error = fromState.error;
            errorMessage = fromState.errorMessage;
            if (completeOnError)
            {
                complete = error;
            }
        }
    }

    /// <summary>
    /// Used when an object is created in the deseralization process and supplied to deserialization
    /// functions for creation
    /// </summary>
    /// <typeparam name="O">Specifies the object type being created</typeparam>
    public class ObjectSerializationState<O> : SerializationState
    {
        public O obj = default;
    }

    /// <summary>
    /// Used when a versioned instance is created in the deseralization process and supplied to deserialization
    /// functions for creation
    /// </summary>
    /// <typeparam name="V">Specifies the <code>IVersioned</code> object instance type being created</typeparam>
    public class VersionedSerializationState<V> : SerializationState
        where V : IVersioned
    {
        public V versioned = default;
    }

    /// <summary>
    /// Used when a versioned array instance is created in the deseralization process and supplied to deserialization
    /// functions for creation
    /// </summary>
    /// <typeparam name="V">Specifies the <code>IVersioned</code> object instance types being created in the array</typeparam>
    public class VersionedArraySerializationState<V> : SerializationState
        where V : IVersioned
    {
        public V[] versionedArray = default;
    }
    #endregion State

}
