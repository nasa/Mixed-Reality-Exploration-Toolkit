// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
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
	/// UserHand
	///
	/// A user hand in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class UserHand : UserComponent<UserHandType, InputHand>, IUserHand<UserHandType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UserHand);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UserHandType serializedUserHand;

        #region IUserHand
        /// <seealso cref="IUserHand.Controller"/>
        public IUserController Controller { get => _controller; }
        private UserController _controller;

        /// <seealso cref="IUserHand.Pointer"/>
        public IUserPointer Pointer { get => _pointer; }
        private UserPointer _pointer;

        /// <seealso cref="IUserPointer.Handedness"/>
        public InputHand.Handedness Handedness { get; set; }

        protected GameObject laserGO;

        public override void Initialize(User user, InputHand inputComponent)
        {
            base.Initialize(user, inputComponent);

            // Auto-setup our hand components, but only for local user. We need remote users
            // to provide us the user hand components for synchronization
            if (IsLocal)
            {
                // Controller
                _controller = UserController.CreateAndInitializeHandComponent(user, this);

                // Laser Pointer
                Transform laserTransform = transform.Find("Laser");
                if (laserTransform == null)
                {
                    laserGO = new GameObject("Laser");
                    laserGO.transform.parent = inputComponent.transform;
                }
                else
                {
                    laserGO = laserTransform.gameObject;
                }
                _pointer = UserPointer.CreateAndInitializeHandComponent(user, this, laserGO);
            }
        }
        #endregion IUserHand

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Initialize any properties
            Handedness = InputHand.Handedness.neutral;
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            // Explicitly release references
            Destroy(_controller);
            Destroy(_pointer);
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="UserComponent{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(UserHandType serialized, SerializationState deserializationState)
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
            serializedUserHand = serialized;

            // Deserialize the user hand fields

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="UserComponent{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(UserHandType serialized, SerializationState serializationState)
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
            serializedUserHand = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <seealso cref="IUserHand.SynchronizeController(UserControllerType)"/>
        public void SynchronizeController(UserControllerType serializedController)
        {
            // Check for valid hand
            if (InputComponent == null)
            {
                // Log a warning
                LogWarning("Remote user input rig hand is not initialized", nameof(SynchronizeController));
                return;
            }

            // Attempt to synchronize the component
            _controller = UserController.SynchronizeHandComponent(User as User, this, serializedController, _controller);
            if (_controller == null)
            {
                LogError("A problem was encountered synchronizing the remote user hand controller for user: " +
                    User.Alias, nameof(SynchronizeController));
            }
        }

        /// <seealso cref="IUserHand.SynchronizePointer(UserPointerType)"/>
        public void SynchronizePointer(UserPointerType serializedPointer)
        {
            // Check for valid hand
            if (InputComponent == null)
            {
                // Log a warning
                LogWarning("Remote user input rig hand is not initialized", nameof(SynchronizePointer));
                return;
            }

            // Attempt to synchronize the component
            _pointer = UserPointer.SynchronizeHandComponent(User as User, this, serializedPointer, _pointer, laserGO);
            if (_pointer == null)
            {
                LogError("A problem was encountered synchronizing the remote user hand pointer for user: " +
                    User.Alias, nameof(SynchronizePointer));
            }
        }

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedUserHand>();
        }

        /// <seealso cref="UserComponent{T, C}.CreateAndInitializeComponent{UC}(User, C, GameObject)"/>
        public static UserHand CreateAndInitializeComponent(User user, InputHand inputComponent,
            GameObject gameObject = null)
        {
            return CreateAndInitializeComponent<UserHand>(user, inputComponent, gameObject);
        }

        /// <seealso cref="UserComponent{T, C}.CreateAndDeserializeComponent{UC}(User, T, C, GameObject)"/>
        public static UserHand CreateAndDeserializeComponent(User user, UserHandType serializedUserComponent,
            InputHand inputComponent, GameObject gameObject = null)
        {
            return CreateAndDeserializeComponent<UserHand>(user, serializedUserComponent, inputComponent, gameObject);
        }

        /// <seealso cref="UserComponent{T, C}.SynchronizeComponent{UC}(User, T, UC, C, GameObject)"/>
        public static UserHand SynchronizeComponent(User user, UserHandType serializedUserComponent,
            UserHand userComponent, InputHand inputComponent, GameObject gameObject = null)
        {
            return SynchronizeComponent<UserHand>(user, serializedUserComponent, userComponent, inputComponent, gameObject);
        }

    }
}

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    public partial class UserHandType : UserComponentType
    {
        [XmlIgnore]
        public InputHand.Handedness Handedness = InputHand.Handedness.neutral;
    }
}
