// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
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
	/// UserHandComponent
	///
	/// A user hand component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class UserHandComponent<T> : UserComponent<T, InputHand>, IUserHandComponent<T>
         where T : UserHandComponentType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UserHandComponent<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedUserHandComponent;

        #region IUserHandComponent
        /// <seealso cref="IUserHandComponent.Hand"/>
        public IUserHand Hand { get; protected set; }

        /// <seealso cref="IUserHandComponent.Initialize(User, UserHand)"/>
        public virtual void Initialize(User user, UserHand hand)
        {
            // Initialize the hand component
            base.Initialize(user, hand.InputComponent);

            // Assign the hand
            Hand = hand;
        }

        /// <seealso cref="IUserHandComponent.CreateSerializedType"/>
        UserHandComponentType IUserHandComponent.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IUserHandComponent.Deserialize(UserHandComponentType, Action{bool, string})"/>
        void IUserHandComponent.Deserialize(UserHandComponentType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IUserHandComponent.Serialize(UserHandComponentType, Action{bool, string})"/>
        void IUserHandComponent.Serialize(UserHandComponentType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IUserHandComponent

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
        /// <seealso cref="UserComponent{T}.Deserialize(T, SerializationState)"/>
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
            serializedUserHandComponent = serialized;

            // Deserialize the user hand component fields

            // Make sure the Hand ID matches
            IIdentifiable hand = MRET.UuidRegistry.Lookup(serialized.HandId);
            if ((hand == null) || !hand.uuid.Equals(Hand.uuid))
            {
                deserializationState.Error("Upplied User Hand ID does not match the hand for this component: " + serialized.HandId);
                yield break;
            }

            // Handedness
            if ((hand is InputHand) && ((hand as InputHand).handedness != serialized.Handedness))
            {
                deserializationState.Error("Upplied User Hand 'Handedness' does not match the handedness for this component: " + serialized.Handedness);
                yield break;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="UserComponent{T}.Serialize(T, SerializationState)"/>
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

            // Hand ID (use UUID to be sure of uniqueness)
            serialized.HandId = Hand.uuid.ToString();

            // Handedness
            serialized.Handedness = Hand.Handedness;

            // Save the final serialized reference
            serializedUserHandComponent = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        #region Synchronization
        /// <summary>
        /// Creates and initializes a user component. If a game object is supplied, the component
        /// will be created in that game object. Otherwise, it will be created in the game object
        /// attached to the supplied hand.
        /// </summary>
        /// <typeparam name="UHC">Specifies the <code>UserHandComponent</code> type to create</typeparam>
        /// <param name="user">The <code>User</code> associated with the user hand component</param>
        /// <param name="hand">The <code>UserHand</code> associated with the user hand component</param>
        /// <param name="gameObject">The optional <code>GameObject</code> to contain the new user hand
        ///     component</param>
        /// <returns>The new <code>UserHandComponent</code> instance</returns>
        public static UHC CreateAndInitializeHandComponent<UHC>(User user, UserHand hand,
            GameObject gameObject = null)
            where UHC : UserHandComponent<T>
        {
            UHC result = null;

            // Check assertions
            if ((user != null) && (hand != null))
            {
                // Create the component
                result = (gameObject != null) ? gameObject.AddComponent<UHC>() : hand.gameObject.AddComponent<UHC>();
                if (hand.IsLocal)
                {
                    result.updateRate = hand.updateRate;
                }
                result.Initialize(user, hand);
            }

            return result;
        }

        /// <summary>
        /// Creates a user hand component and deserializes the serialized user hand component into
        /// the new user hand component. If a game object is supplied, the component will be created
        /// in that game object. Otherwise, it will be created in the game object attached to the
        /// supplied hand.
        /// </summary>
        /// <typeparam name="UHC">Specifies the <code>UserHandComponent</code> type to create</typeparam>
        /// <param name="user">The <code>User</code> associated with the user hand component</param>
        /// <param name="hand">The <code>UserHand</code> associated with the user hand component</param>
        /// <param name="serializedHandComponent">The <code>UserHandComponentType</code> containing the
        ///     serialized user hand component to load</param>
        /// <param name="gameObject">The optional <code>GameObject</code> to contain the new user hand
        ///     component</param>
        /// <returns>The new <code>UserHandComponent</code> instance or null</returns>
        public static UHC CreateAndDeserializeHandComponent<UHC>(User user, UserHand hand,
            T serializedHandComponent, GameObject gameObject = null)
            where UHC : UserHandComponent<T>
        {
            // We need to create and initialize the hand component
            UHC result = CreateAndInitializeHandComponent<UHC>(user, hand, gameObject);

            // Action delegate on load
            Action<bool, string> OnHandComponentLoaded = (bool loaded, string message) =>
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
                    Debug.LogWarning("A problem was encountered deserializing the user hand component: " + message);
                }
            };

            // Deserialize the hand component and protect against exceptions
            try
            {
                result.Deserialize(serializedHandComponent, OnHandComponentLoaded);
            }
            catch (Exception e)
            {
                // Remove the reference
                Destroy(result);
                result = null;
                Debug.LogError("A problem was encountered deserializing the user hand component: " + e);
            }

            return result;
        }

        /// <summary>
        /// Attempts to synchronize the supplied remote user hand component. If the user hand
        /// component is null (not yet defined), one will be created, initialized, deserialized
        /// and enabled for synchronization. This function does nothing if the user hand component
        /// is associated with the local user.
        /// </summary>
        /// <typeparam name="UHC">Specifies the <code>UserHandComponent</code> type to create</typeparam>
        /// <param name="user">The <code>User</code> associated with the user hand component</param>
        /// <param name="hand">The <code>UserHand</code> associated with the user hand component</param>
        /// <param name="serializedHandComponent">The <code>UserHandComponentType</code> containing the
        ///     serialized user hand component to load</param>
        /// <param name="handComponent">The user hand component to synchronize. If null one will be
        ///     created</param>
        /// <param name="gameObject">The optional <code>GameObject</code> to contain the new user hand
        ///     component</param>
        /// <returns>The <code>UserHandComponent</code> instance reference (same as handComponent
        ///     if not null), or null if there was a problem creating the user hand component the first
        ///     time</returns>
        public static UHC SynchronizeHandComponent<UHC>(User user, UserHand hand, T serializedHandComponent,
            UHC handComponent, GameObject gameObject = null)
            where UHC : UserHandComponent<T>
        {
            UHC result = handComponent;

            // We don't allow for forced updates to the local user components
            if (user.IsLocal) return result;

            // Perform the synchronization
            if (result != null)
            {
                // Synchronize the user component
                result.Synchronize(serializedHandComponent);
            }
            else
            {
                // We need to create and deserialize the user component
                result = CreateAndDeserializeHandComponent<UHC>(user, hand, serializedHandComponent);
            }

            return result;
        }
        #endregion Synchronization
    }
}

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    public partial class UserHandComponentType : UserComponentType
    {
        [XmlIgnore]
        public InputHand.Handedness Handedness = InputHand.Handedness.neutral;

        [XmlIgnore]
        public string HandId = null;
    }
}
