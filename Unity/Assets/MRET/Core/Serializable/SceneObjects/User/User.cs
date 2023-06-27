// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.User
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// User
	///
	/// A user in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public class User : SceneObject<UserType>, IUser<UserType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(User);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UserType serializedUser;

        // Prefix of all keys
        public const string KEY_PREFIX = "GOV.NASA.GSFC.XR.MRET." + nameof(User);

        // DataManager keys
        public const string KEY_USER_HEIGHT_M = KEY_PREFIX + ".HEIGHT.METERS";
        public const string KEY_USER_WIDTH_M = KEY_PREFIX + ".WIDTH.METERS";
        public const string KEY_USER_ORIENTATION = KEY_PREFIX + ".ORIENTATION";
        public const string KEY_USER_POSITION = KEY_PREFIX + ".POSITION";
        public const string KEY_USER_ROTATION = KEY_PREFIX + ".ROTATION";
        public const string KEY_USER_EULERANGLES = KEY_PREFIX + ".EULERANGLES";
        public const string KEY_USER_LOCALPOSITION = KEY_PREFIX + ".LOCALPOSITION";
        public const string KEY_USER_LOCALSCALE = KEY_PREFIX + ".LOCALSCALE";
        public const string KEY_USER_LOCALROTATION = KEY_PREFIX + ".LOCALROTATION";
        public const string KEY_USER_LOCALEULERANGLES = KEY_PREFIX + ".LOCALEULERANGLES";
        public const string KEY_USER_EYE_POSITION = KEY_PREFIX + ".EYE.POSITION";
        public const string KEY_USER_EYE_LOCALPOSITION = KEY_PREFIX + ".EYE.LOCALPOSITION";

        /// Properties
        #region PROPERTIES

        /// The user's head offset from the body collider
        private float headOffset = 0f;
        public float HeadOffset
        {
            get
            {
                return (MRET.InputRig.head.Collider ? headOffset : 0f);
            }
        }

        /// Local space properties
        #region LOCAL_SPACE

        /// Local space component properties
        #region LOCAL_SPACE_COMPONENTS

        /// The user head height in local space
        public float LocalHeadHeight
        {
            get
            {
                return LocalHeadDimensions.y;
            }
        }

        /// The user head width in local space
        public float LocalHeadWidth
        {
            get
            {
                return LocalHeadDimensions.x;
            }
        }

        /// The user body height in local space
        public float LocalBodyHeight
        {
            get
            {
                return LocalBodyDimensions.y;
            }
        }

        /// The user body width in local space
        public float LocalBodyWidth
        {
            get
            {
                return LocalBodyDimensions.x;
            }
        }

        /// The user head dimensions in local space
        public Vector3 LocalHeadDimensions
        {
            get
            {
                float height = 0f;
                float width = 0f;

                Collider headCollider = MRET.InputRig.head.Collider;
                if (headCollider is CapsuleCollider)
                {
                    CapsuleCollider capsuleCollider = (headCollider as CapsuleCollider);
                    width = capsuleCollider.radius * 2f;
                    height = capsuleCollider.height;
                }
                else if (headCollider is CharacterController)
                {
                    CharacterController charCollider = (headCollider as CharacterController);
                    width = charCollider.radius * 2f;
                    height = charCollider.height;
                }

                return new Vector3(width, height, width);
            }
        }

        /// The user foot dimensions in local space
        public Vector3 LocalBodyDimensions
        {
            get
            {
                float height = 0f;
                float width = 0f;

                Collider bodyCollider = MRET.InputRig.body.collider;
                if (bodyCollider is CapsuleCollider)
                {
                    CapsuleCollider capsuleCollider = (bodyCollider as CapsuleCollider);
                    width = capsuleCollider.radius * 2f;
                    height = capsuleCollider.height;
                }
                else if (bodyCollider is CharacterController)
                {
                    CharacterController charCollider = (bodyCollider as CharacterController);
                    width = charCollider.radius * 2f;
                    height = charCollider.height;
                }

                return new Vector3(width, height, width);
            }
        }

        #endregion LOCAL_SPACE_COMPONENTS

        /// The user height in local space
        public float LocalHeight
        {
            get
            {
                return HeadOffset + LocalHeadHeight + LocalBodyHeight;
            }
        }

        /// The user width in local space
        public float LocalWidth
        {
            get
            {
                return (LocalBodyWidth > LocalHeadWidth) ? LocalBodyWidth : LocalHeadWidth;
            }
        }

        #endregion LOCAL_SPACE

        /// World space properties
        #region WORLD_SPACE

        /// World space component properties
        #region WORLD_SPACE_COMPONENTS

        /// The user head height in world space
        public float WorldHeadHeight
        {
            get
            {
                return WorldHeadDimensions.y;
            }
        }

        /// The user head width in world space
        public float WorldHeadWidth
        {
            get
            {
                return WorldHeadDimensions.x;
            }
        }

        /// The user body height in world space
        public float WorldBodyHeight
        {
            get
            {
                return WorldBodyDimensions.y;
            }
        }

        /// The user body width in world space
        public float WorldBodyWidth
        {
            get
            {
                return WorldBodyDimensions.x;
            }
        }

        /// The user head dimensions in world space
        public Vector3 WorldHeadDimensions
        {
            get
            {
                return MRET.InputRig.head.Collider.transform.TransformVector(LocalHeadDimensions);
            }
        }

        /// The user body dimensions in world space
        public Vector3 WorldBodyDimensions
        {
            get
            {
                return MRET.InputRig.body.collider.transform.TransformVector(LocalBodyDimensions);
            }
        }

        #endregion WORLD_SPACE_COMPONENTS

        /// The user height in world space
        public float WorldHeight
        {
            get
            {
                return HeadOffset + WorldHeadHeight + WorldBodyHeight;
            }
        }

        /// The user width in world space
        public float WorldWidth
        {
            get
            {
                return (WorldBodyWidth > WorldHeadWidth) ? WorldBodyWidth : WorldHeadWidth;
            }
        }

        #endregion WORLD_SPACE

        #endregion PROPERTIES

        [Tooltip("The DataManager to use for storing the user telemetry. If not supplied, the MRET DataManager will be used")]
        public DataManager dataManager;

        protected InputRig inputRig;

        #region IUser
        /// <seealso cref="IUser.IsLocal"/>
        public bool IsLocal { get; protected set; }

        /// <seealso cref="IUser.Alias"/>
        public string Alias { get; set; }

        /// <seealso cref="IUser.Color"/>
        public Color32 Color { get; set; }

        /// <seealso cref="IUser.Head"/>
        public IUserHead Head { get => _head; }
        private UserHead _head;

        /// <seealso cref="IUser.Torso"/>
        public IUserTorso Torso { get => _torso; }
        private UserTorso _torso;

        /// <seealso cref="IUser.Hands"/>
        public IUserHand[] Hands { get => _hands.ToArray(); }
        private List<UserHand> _hands;

        public Text AliasLabel { get; protected set; }

        /// <seealso cref="IUser.Initialize(bool)"/>
        public virtual void Initialize(bool isLocal, InputRig inputRig)
        {
            // Initialize whether we are local or not
            IsLocal = isLocal;

            // Assign the rig
            this.inputRig = inputRig;
            if (inputRig)
            {
                // Auto-setup our components, but only for local user. We need remote users
                // to provide us the user components for synchronization
                if (IsLocal)
                {
                    // Obtain the user component references
                    Log("Setting up local user components", nameof(Initialize));

                    // Head
                    Destroy(_head);
                    _head = UserHead.CreateAndInitializeComponent(this, inputRig.head);

                    // Torso
                    Destroy(_torso);
                    _torso = UserTorso.CreateAndInitializeComponent(this, inputRig.body);

                    // Hands
                    DestroyHands();
                    foreach (InputHand hand in inputRig.hands)
                    {
                        UserHand userHand = UserHand.CreateAndInitializeComponent(this, hand);
                        if (userHand != null)
                        {
                            _hands.Add(userHand);
                        }
                    }
                }
                else
                {
                    // Get the remote user label
                    AliasLabel = inputRig.GetComponentInChildren<Text>();
                }
            }
            else
            {
                // Log a warning
                LogWarning("User input rig is null", nameof(Initialize));
            }
        }
        #endregion IUser

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETUpdateBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // Initialize our hand list
            _hands = new List<UserHand>();

            // Initialize any properties
            IsLocal = false;
            Alias = UserDefaults.ALIAS;
            Color = UserDefaults.COLOR;
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Make sure we have a data manager
            if (dataManager == null)
            {
                dataManager = MRET.DataManager;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            // Explicitly release references
            Destroy(_head);
            Destroy(_torso);
            DestroyHands();
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            // Make sure we are only updating the local user. We don't want to report local
            // telemetry if this is a remote user. 
            if (IsLocal)
            {
                // Make sure we have a DataManager reference. Otherwise, we will skip this logic altogether
                if (dataManager != null)
                {
                    // Store the user dimensions
                    dataManager.SaveValue(KEY_USER_HEIGHT_M, WorldHeight);
                    dataManager.SaveValue(KEY_USER_WIDTH_M, WorldWidth);

                    // Get the user transform
                    Transform userTransform = MRET.InputRig.transform;
                    if (userTransform != null)
                    {
                        // Store the user transforms into the DataManager

                        // World transforms
                        dataManager.SaveValue(KEY_USER_POSITION, userTransform.position);
                        dataManager.SaveValue(KEY_USER_ROTATION, userTransform.rotation);
                        dataManager.SaveValue(KEY_USER_EULERANGLES, userTransform.eulerAngles);

                        // Local transforms
                        dataManager.SaveValue(KEY_USER_LOCALPOSITION, userTransform.localPosition);
                        dataManager.SaveValue(KEY_USER_LOCALSCALE, userTransform.localScale);
                        dataManager.SaveValue(KEY_USER_LOCALROTATION, userTransform.localRotation);
                        dataManager.SaveValue(KEY_USER_LOCALEULERANGLES, userTransform.localEulerAngles);

                        // Get the orientation transform
                        Transform orientationTransform = MRET.InputRig.head.transform;
                        if (orientationTransform != null)
                        {
                            // Store the orientation transform into the DataManager
                            dataManager.SaveValue(KEY_USER_ORIENTATION, orientationTransform.forward);
                            dataManager.SaveValue(KEY_USER_EYE_POSITION, orientationTransform.position);
                            dataManager.SaveValue(KEY_USER_EYE_LOCALPOSITION, orientationTransform.localPosition);
                        }
                    }
                }
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="SceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(UserType serialized, SerializationState deserializationState)
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
            serializedUser = serialized;

            // Deserialize the user fields

            // TODO: Deserialize the avatar

            // TODO: User Views
            /*Debug.Log("setting view");
            ViewType viewToGet = (ViewType) serializedProject.Items[i];
            Transform transformToSet = headsetFollower.transform.parent.parent;
            transformToSet.position = new Vector3(viewToGet.Location.X, viewToGet.Location.Y, viewToGet.Location.Z);
            transformToSet.rotation = Quaternion.Euler(viewToGet.Rotation.X, viewToGet.Rotation.Y, viewToGet.Rotation.Z);
            transformToSet.localScale = new Vector3(1 / viewToGet.Zoom, 1 / viewToGet.Zoom, 1 / viewToGet.Zoom);*/

            // Deserialize the alias
            Alias = serializedUser.Alias;

            // Deserialize the color
            Color32 color = UserDefaults.COLOR;
            if (serializedUser.Item is ColorComponentsType)
            {
                SchemaUtil.DeserializeColorComponents(serializedUser.Item as ColorComponentsType, ref color);
            }
            else if (serializedUser.Item is ColorPredefinedType)
            {
                SchemaUtil.DeserializeColorPredefined((ColorPredefinedType)serializedUser.Item, ref color);
            }
            Color = color;

            // Set the user label if remote
            if (!IsLocal && AliasLabel)
            {
                // Assign the alias
                AliasLabel.text = Alias;
                AliasLabel.color = Color;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="SceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(UserType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize out the user fields

            // TODO: Serialize the avatar

            // TODO: User Views

            // Serialize the alias
            serialized.Alias = Alias;

            // Serialize the color
            object serializedColor = serialized.Item;
            if (serializedColor == null)
            {
                // Try to use this color structure from the deserialization process
                serializedColor = (serializedUser != null) ? serializedUser.Item : null;

                // Only serialize if we have a valid serialized color reference
                if (serializedColor != null)
                {
                    // TODO: Serialize the color? Not sure what this means since we really just want
                    // to retain the structure defined in the original serialized form. We don't want to
                    // generate an entirely new serialized color without context to how it was defined.
                    //SchemaUtil.SerializeColor(ref serializedColor);
                }
            }
            serialized.Item = serializedColor;

            // Save the final serialized reference
            serializedUser = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <seealso cref="IUser.SynchronizeHead(UserHeadType)"/>
        public void SynchronizeHead(UserHeadType serializedHead)
        {
            // Check for valid rig
            if (inputRig == null)
            {
                // Log a warning
                LogWarning("Remote user input rig is not initialized", nameof(SynchronizeHead));
                return;
            }

            // Attempt to synchronize the component
            _head = UserHead.SynchronizeComponent(this, serializedHead, _head, inputRig.head);
            if (_head == null)
            {
                LogError("A problem was encountered synchronizing the remote user head for user: " +
                    Alias, nameof(SynchronizeHead));
            }
        }

        /// <seealso cref="IUser.SynchronizeTorso(UserTorsoType)"/>
        public void SynchronizeTorso(UserTorsoType serializedTorso)
        {
            // Check for valid rig
            if (inputRig == null)
            {
                // Log a warning
                LogWarning("Remote user input rig is not initialized", nameof(SynchronizeTorso));
                return;
            }

            // Attempt to synchronize the component
            _torso = UserTorso.SynchronizeComponent(this, serializedTorso, _torso, inputRig.body);
            if (_torso == null)
            {
                LogError("A problem was encountered synchronizing the remote user torso for user: " +
                    Alias, nameof(SynchronizeTorso));
            }
        }

        /// <seealso cref="IUser.SynchronizeHand(UserHandType)"/>
        public void SynchronizeHand(UserHandType serializedHand)
        {
            // We don't allow for forced updates to the local user
            if (IsLocal) return;

            // Check for valid rig
            if (inputRig == null)
            {
                // Log a warning
                LogWarning("Remote user input rig is not initialized", nameof(SynchronizeHand));
                return;
            }

            // Attempt to synchronize the hand
            IUserHand userHand = FindUserHand(serializedHand.UUID);
            if (userHand != null)
            {
                // Synchronize the hand
                userHand.Synchronize(serializedHand);
            }
            else
            {
                // Locate the rig input hand
                InputHand hand = FindInputRigHand(serializedHand.Handedness);
                if (hand != null)
                {
                    // Create the hand and deserialize
                    UserHand newUserHand = UserHand.CreateAndDeserializeComponent(this, serializedHand, hand);
                    if (newUserHand != null)
                    {
                        _hands.Add(newUserHand);
                    }
                    else
                    {
                        LogError("A problem was encountered synchronizing the remote user hand for user: " +
                            Alias, nameof(SynchronizeHand));
                    }
                }
                else
                {
                    // Log a warning
                    LogWarning("Input rig hand could not be located for synchronization", nameof(SynchronizeHand));
                }
            }
        }

        /// <summary>
        /// Locates the input rig hand matching the supplied handedness.
        /// </summary>
        /// <param name="handedness">The handedness of the hand to locate</param>
        /// <returns>The first <code>InputHand</code> in the input rig associated with the supplied handedness</returns>
        protected InputHand FindInputRigHand(InputHand.Handedness handedness)
        {
            InputHand result = null;

            // Check for valid rig
            if (inputRig == null)
            {
                // Log a warning
                LogWarning("Remote user input rig is not initialized", nameof(FindInputRigHand));
                return result;
            }

            // Check each input hand for a matching handedness
            foreach (InputHand hand in inputRig.hands)
            {
                if (hand.handedness == handedness)
                {
                    result = hand;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Locates the hand of this user that matches the supplied ID. The ID can be either
        /// a UUID or a MRET ID.
        /// </summary>
        /// <param name="id">The ID of the hand to locate</param>
        /// <returns>The <code>IUserHand</code> associated with the ID or null</returns>
        protected IUserHand FindUserHand(string id)
        {
            IUserHand userHand = null;

            // Check for a valid registered ID
            IIdentifiable identifiable = MRET.UuidRegistry.Lookup(id);
            if (identifiable == null)
            {
                // Log a warning
                LogWarning("Supplid ID does not exist in the registry: " + id, nameof(FindUserHand));
                return userHand;
            }

            // Check for proper initialization of the hands
            if (_hands.Count == 0)
            {
                // Log a warning
                LogWarning("User hands are not properly initialized", nameof(FindUserHand));
                return userHand;
            }

            // Locate the hand
            foreach (UserHand hand in _hands)
            {
                // Check for a UUID match
                if (hand.uuid.Equals(identifiable.uuid))
                {
                    userHand = hand;
                    break;
                }
            }

            return userHand;
        }

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedUser>();
        }

        /// <summary>
        /// Destroys the hand references
        /// </summary>
        protected void DestroyHands()
        {
            if (_hands != null)
            {
                UserHand[] hands = _hands.ToArray();
                foreach (UserHand hand in hands)
                {
                    Destroy(hand);
                }
                _hands.Clear();
            }
        }

    }

    public class UserDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly string ALIAS = new UserType().Alias;
        public static readonly Color32 COLOR = Color.black;
    }
}

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    public partial class UserType : SceneObjectType
    {
        [XmlIgnore]
        public string Alias = "Default";
    }
}

