// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// UserPointer
	///
	/// A user hand component pointer in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class UserPointer : UserHandComponent<UserPointerType>, IUserPointer<UserPointerType>
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(UserPointer);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UserPointerType serializedUserPointer;

        #region IUserHandComponent
        /// <seealso cref="IUserHandComponent.Initialize(User, UserHand)"/>
        public override void Initialize(User user, UserHand hand)
        {
            // Initialize the hand component
            base.Initialize(user, hand);

            // Get the remote hand line renderer reference
            if (!IsLocal)
            {
                LineRenderer = gameObject.GetComponent<LineRenderer>();
            }
        }
        #endregion IUserHandComponent

        #region IUserPointer
        /// <seealso cref="IUserPointer.LineRenderer"/>
        public LineRenderer LineRenderer { get; protected set; }

        /// <seealso cref="IUserPointer.EndPoint"/>
        public Vector3 EndPoint { get; set; }
        #endregion IUserPointer

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
        protected override IEnumerator Deserialize(UserPointerType serialized, SerializationState deserializationState)
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
            serializedUserPointer = serialized;

            // Deserialize the user hand component fields

            // Update the remote pointer
            if (!IsLocal)
            {
                Vector3[] positions = new Vector3[2];
                if (EndPoint == Vector3.zero)
                {
                    positions[0] = positions[1] = Vector3.zero;
                }
                else
                {
                    positions[0] = Vector3.zero;
                    positions[1] = transform.InverseTransformPoint(EndPoint);
                }
                LineRenderer.SetPositions(positions);
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="UserHandComponent{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(UserPointerType serialized, SerializationState serializationState)
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
            serializedUserPointer = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedPointer>();
        }

        /// <seealso cref="UserHandComponent{T}.CreateAndInitializeHandComponent{UHC}(User, UserHand, GameObject)"/>
        public static UserPointer CreateAndInitializeHandComponent(User user, UserHand hand,
            GameObject gameObject = null)
        {
            return CreateAndInitializeHandComponent<UserPointer>(user, hand, gameObject);
        }

        /// <seealso cref="UserHandComponent{T}.CreateAndDeserializeHandComponent{UHC}(User, UserHand, T, GameObject)"/>
        public static UserPointer CreateAndDeserializeHandComponent(User user, UserHand hand,
            UserPointerType serializedHandComponent, GameObject gameObject = null)
        {
            return CreateAndDeserializeHandComponent<UserPointer>(user, hand, serializedHandComponent, gameObject);
        }

        /// <seealso cref="UserHandComponent{T}.SynchronizeHandComponent{UHC}(User, UserHand, T, UHC, GameObject)"/>
        public static UserPointer SynchronizeHandComponent(User user, UserHand hand,
            UserPointerType serializedHandComponent, UserPointer userComponent, GameObject gameObject = null)
        {
            return SynchronizeHandComponent<UserPointer>(user, hand, serializedHandComponent, userComponent, gameObject);
        }
    }
}

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    public partial class UserPointerType : UserHandComponentType
    {
    }
}
