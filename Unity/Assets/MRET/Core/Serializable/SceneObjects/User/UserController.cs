// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// UserController
	///
	/// A user controller in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class UserController : UserHandComponent<UserControllerType>, IUserController<UserControllerType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UserController);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UserControllerType serializedUserController;

        #region IUserController
        #endregion IUserController

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
        /// <seealso cref="UserHandComponent{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(UserControllerType serialized, SerializationState deserializationState)
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
            serializedUserController = serialized;

            // Deserialize the user hand component fields

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="UserHandComponent{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(UserControllerType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedUserController = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedController>();
        }

        /// <seealso cref="UserHandComponent{T}.CreateAndInitializeHandComponent{UHC}(User, UserHand, GameObject)"/>
        public static UserController CreateAndInitializeHandComponent(User user, UserHand hand,
            GameObject gameObject = null)
        {
            return CreateAndInitializeHandComponent<UserController>(user, hand, gameObject);
        }

        /// <seealso cref="UserHandComponent{T}.CreateAndDeserializeHandComponent{UHC}(User, UserHand, T, GameObject)"/>
        public static UserController CreateAndDeserializeHandComponent(User user, UserHand hand,
            UserControllerType serializedHandComponent, GameObject gameObject = null)
        {
            return CreateAndDeserializeHandComponent<UserController>(user, hand, serializedHandComponent, gameObject);
        }

        /// <seealso cref="UserHandComponent{T}.SynchronizeHandComponent{UHC}(User, UserHand, T, UHC, GameObject)"/>
        public static UserController SynchronizeHandComponent(User user, UserHand hand,
            UserControllerType serializedHandComponent, UserController userComponent, GameObject gameObject = null)
        {
            return SynchronizeHandComponent<UserController>(user, hand, serializedHandComponent, userComponent, gameObject);
        }
    }
}

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    public partial class UserControllerType : UserHandComponentType
    {
    }
}
