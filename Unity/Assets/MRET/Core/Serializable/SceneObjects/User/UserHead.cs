// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// UserHead
	///
	/// A user head in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class UserHead : UserComponent<UserHeadType, InputHead>, IUserHead<UserHeadType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UserHead);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UserHeadType serializedUserHead;

        #region IUserHead
        #endregion IUserHead

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Initialize any properties
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="UserComponent{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(UserHeadType serialized, SerializationState deserializationState)
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
            serializedUserHead = serialized;

            // Deserialize the user body part fields

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="UserComponent{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(UserHeadType serialized, SerializationState serializationState)
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
            serializedUserHead = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedUserHead>();
        }

        /// <seealso cref="UserComponent{T, C}.CreateAndInitializeComponent{UC}(User, C, GameObject)"/>
        public static UserHead CreateAndInitializeComponent(User user, InputHead inputComponent,
            GameObject gameObject = null)
        {
            return CreateAndInitializeComponent<UserHead>(user, inputComponent, gameObject);
        }

        /// <seealso cref="UserComponent{T, C}.CreateAndDeserializeComponent{UC}(User, T, C, GameObject)"/>
        public static UserHead CreateAndDeserializeComponent(User user, UserHeadType serializedUserComponent,
            InputHead inputComponent, GameObject gameObject = null)
        {
            return CreateAndDeserializeComponent<UserHead>(user, serializedUserComponent, inputComponent, gameObject);
        }

        /// <seealso cref="UserComponent{T, C}.SynchronizeComponent{UC}(User, T, UC, C, GameObject)"/>
        public static UserHead SynchronizeComponent(User user, UserHeadType serializedUserComponent,
            UserHead userComponent, InputHead inputComponent, GameObject gameObject = null)
        {
            return SynchronizeComponent<UserHead>(user, serializedUserComponent, userComponent, inputComponent, gameObject);
        }

    }
}

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    public partial class UserHeadType : UserComponentType
    {
    }
}
