// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// UserComponent
	///
	/// A user component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class UserComponent<T, C> : SceneObject<T>, IUserComponent<T, C>
        where T : UserComponentType, new()
        where C : Component
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UserComponent<T, C>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedUserComponent;

        #region IUserComponent
        /// <seealso cref="IUser.IsLocal"/>
        public bool IsLocal => (User != null) ? User.IsLocal : false;

        /// <seealso cref="IUserComponent.User"/>
        public IUser User { get; protected set; }

        /// <seealso cref="IUserComponent.InputComponent"/>
        Component IUserComponent.InputComponent => InputComponent;

        /// <seealso cref="IUserComponent{T, C}.InputComponent"/>
        public C InputComponent { get; protected set; }

        /// <seealso cref="IUserComponent.Initialize(User, Component)"/>
        void IUserComponent.Initialize(User user, Component inputComponent)
        {
            Initialize(user, (C)inputComponent);
        }

        /// <seealso cref="ISynchronized{T}.Initialize(T)"/>
        public virtual void Initialize(User user, C inputComponent)
        {
            // Assign the user
            User = user;

            // Assign the input component
            InputComponent = inputComponent;
        }

        /// <seealso cref="ISceneObject.CreateSerializedType"/>
        UserComponentType IUserComponent.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="ISceneObject.Deserialize(SceneObjectType, Action{bool, string})"/>
        void IUserComponent.Deserialize(UserComponentType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="ISceneObject.Serialize(SceneObjectType, Action{bool, string})"/>
        void IUserComponent.Serialize(UserComponentType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IUserComponent

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Initialize any properties
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="SceneObject{T}.Deserialize(T, SerializationState)"/>
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
            serializedUserComponent = serialized;

            // Deserialize the user component fields

            // Make sure the User ID matches
            IIdentifiable user = MRET.UuidRegistry.Lookup(serialized.UserId);
            if ((user == null) || !user.uuid.Equals(User.uuid))
            {
                deserializationState.Error("Upplied User ID does not match the User for this component: " + serialized.UserId);
                yield break;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="SceneObject{T}.Serialize(T, SerializationState)"/>
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

            // User ID (use UUID to be sure of uniqueness)
            serialized.UserId = User.uuid.ToString();

            // Save the final serialized reference
            serializedUserComponent = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        #region Synchronization
        /// <summary>
        /// Creates and initializes a user component. If a game object is supplied, the component
        /// will be created in that game object. Otherwise, it will be created in the game object
        /// attached to the supplied input component, i.e. InputHead.gameObject, etc.
        /// </summary>
        /// <typeparam name="UC">Specifies the <code>UserComponent</code> type to create</typeparam>
        /// <param name="user">The <code>IUser</code> associated with the component</param>
        /// <param name="inputComponent">The input rig component associated with the user component,
        ///     i.e. InputHead</param>
        /// <param name="gameObject">The optional <code>GameObject</code> to contain the new user
        ///     component</param>
        /// <returns>The new <code>UserComponent</code> instance</returns>
        public static UC CreateAndInitializeComponent<UC>(User user, C inputComponent,
            GameObject gameObject = null)
            where UC : UserComponent<T, C>
        {
            UC result = null;

            // Check assertions
            if ((user != null) && (inputComponent != null))
            {
                // Create the component
                result = (gameObject != null) ? gameObject.AddComponent<UC>() : inputComponent.gameObject.AddComponent<UC>();
                if (user.IsLocal)
                {
                    result.updateRate = user.updateRate;
                }
                result.Initialize(user, inputComponent);
            }

            return result;
        }

        /// <summary>
        /// Creates a user component and deserializes the serialized user component into the new
        /// user component. If a game object is supplied, the component will be created in that
        /// game object. Otherwise, it will be created in the game object attached to the supplied
        /// input component, i.e. InputHead.gameObject, etc.
        /// </summary>
        /// <typeparam name="UC">Specifies the <code>UserComponent</code> type to create</typeparam>
        /// <param name="user">The <code>IUser</code> associated with the component</param>
        /// <param name="serializedUserComponent">The <code>UserComponentType</code> containing the
        ///     serialized user component to load</param>
        /// <param name="inputComponent">The input rig component associated with the user component,
        ///     i.e. InputHead</param>
        /// <param name="gameObject">The optional <code>GameObject</code> to contain the new user
        ///     component</param>
        /// <returns>The new <code>UserComponent</code> instance or null</returns>
        public static UC CreateAndDeserializeComponent<UC>(User user, T serializedUserComponent,
            C inputComponent, GameObject gameObject = null)
            where UC : UserComponent<T, C>
        {
            // We need to create and initialize the component
            UC result = CreateAndInitializeComponent<UC>(user, inputComponent, gameObject);

            // Make sure the user component was actually created
            if (result != null)
            {
                // Action delegate on load
                Action<bool, string> OnComponentLoaded = (bool loaded, string message) =>
                {
                    // Make sure the user component loaded
                    if (loaded)
                    {
                        // Enable collaboration
                        result.SynchronizationEnabled = MRET.CollaborationManager.CollaborationEnabled;
                    }
                    else
                    {
                        // Remove the reference
                        Destroy(result);
                        result = null;
                        Debug.LogWarning("A problem was encountered deserializing the user component: " + message);
                    }
                };

                // Deserialize the component and protect against exceptions
                try
                {
                    result.Deserialize(serializedUserComponent, OnComponentLoaded);
                }
                catch (Exception e)
                {
                    // Remove the reference
                    Destroy(result);
                    result = null;
                    Debug.LogError("A problem was encountered deserializing the user component: " + e);
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to synchronize the supplied remote user component. If the user component is
        /// null (not yet defined), one will be created, initialized, deserialized and enabled
        /// for synchronization. This function does nothing if the user component is associated
        /// with the local user.
        /// </summary>
        /// <typeparam name="UC">Specifies the <code>UserComponent</code> type to create</typeparam>
        /// <param name="user">The <code>IUser</code> associated with the component</param>
        /// <param name="serializedUserComponent">The <code>UserComponentType</code> containing the
        ///     serialized user component to load</param>
        /// <param name="userComponent">The user component to synchronize. If null one will be
        ///     created</param>
        /// <param name="inputComponent">The input rig component associated with the user component,
        ///     i.e. InputHead</param>
        /// <param name="gameObject">The optional <code>GameObject</code> to contain the new user
        ///     component</param>
        /// <returns>The <code>UserComponent</code> instance reference (same as userComponent
        ///     if not null), or null if there was a problem creating the user component the first
        ///     time</returns>
        public static UC SynchronizeComponent<UC>(User user, T serializedUserComponent, UC userComponent,
            C inputComponent, GameObject gameObject = null)
            where UC : UserComponent<T, C>
        {
            UC result = userComponent;

            // We don't allow for forced updates to the local user components
            if (user.IsLocal) return result;

            // Perform the synchronization
            if (result != null)
            {
                // Synchronize the user component
                result.Synchronize(serializedUserComponent);
            }
            else
            {
                // We need to create and deserialize the user component
                result = CreateAndDeserializeComponent<UC>(user, serializedUserComponent, inputComponent);
            }

            return result;
        }
        #endregion Synchronization

    }
}

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    public partial class UserComponentType : SceneObjectType
    {
        [XmlIgnore]
        public string UserId = null;
    }
}
