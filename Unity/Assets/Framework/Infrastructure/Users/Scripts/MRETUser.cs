﻿using UnityEngine;

/**
 * Represents a MRET user. For the local user, this class monitors volatile information about the
 * current user and publishes informatio to the DataManager for access by other scripts. This
 * information includes, but is not limited to:
 * 
 *      DataManager Key             Value Description
 *      ----------------------      ----------------------------------------
 *      KEY_USER_HEIGHT_M           User's height in meters (float)
 *      KEY_USER_WIDTH_M            User's width in meters (float)
 *      KEY_USER_ORIENTATION        User's forward vector (Vector3)
 *      KEY_USER_POSITION           User's world position (Vector3)
 *      KEY_USER_ROTATION           User's world rotation (Quaternion)
 *      KEY_USER_EULERANGLES        User's world rotation (Vector3)
 *      KEY_USER_LOCALPOSITION      User's local position (Vector3)
 *      KEY_USER_LOCALSCALE         User's local scale (Vector3)
 *      KEY_USER_LOCALROTATION      User's local rotation (Quaternion)
 *      KEY_USER_LOCALEULERANGLES   User's local rotation (Vector3)
 *      KEY_USER_EYE_POSITION       User's eye world position (Vector3)
 *      KEY_USER_EYE_LOCALPOSITION  User's eye local position (Vector3)
 * 
 * @see DataManager
 */
public class MRETUser : MonoBehaviour
{
    public static readonly string NAME = nameof(MRETUser);

    // Prefix of all keys
    public const string KEY_PREFIX = "GSFC.ARVR.MRET.MRETUser";

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

    [Tooltip("The DataManager to use for storing the user telemetry. If not supplied, one will be located at Start")]
    public DataManager dataManager;

    // Performance Management
    private int updateCounter = 0;
    [Tooltip("Modulates the frequency of model updates published to the DataManager. The value represents a counter modulo to determine how many calls to Update will be skipped before publishing.")]
    public int updateRateModulo = 1;

    [Tooltip("Indicated whether or not this MRET User is local or remote.")]
    public bool isLocal = true;
    [Tooltip("The user's name.")]
    public string userName = "default";

    // Internal SessionManager reference
    private SessionManager sessionManager;

    private bool initialized = false;

    private Collider bodyCollider;
    private Collider footCollider;

    // Properties
    #region PROPERTIES

    /**
     * Indicates that the colliders are initialized
     */
    public bool CollidersInitialized
    {
        get
        {
            return (bodyCollider != null);
        }
    }

    /**
     * The user's head offset from the body collider
     */
    private float headOffset = 0f;
    public float HeadOffset
    {
        get
        {
            return (CollidersInitialized ? headOffset : 0f);
        }
    }

    // Local space properties
    #region LOCAL_SPACE

    // Local space component properties
    #region LOCAL_SPACE_COMPONENTS

    /**
     * The user body height in local space
     */
    public float LocalBodyHeight
    {
        get
        {
            return LocalBodyDimensions.y;
        }
    }

    /**
     * The user body width in local space
     */
    public float LocalBodyWidth
    {
        get
        {
            return LocalBodyDimensions.x;
        }
    }

    /**
     * The user foot height in local space
     */
    public float LocalFootHeight
    {
        get
        {
            return LocalFootDimensions.y;
        }
    }

    /**
     * The user foot width in local space
     */
    public float LocalFootWidth
    {
        get
        {
            return LocalFootDimensions.x;
        }
    }

    /**
     * The user foot dimensions in local space
     */
    public Vector3 LocalBodyDimensions
    {
        get
        {
            float height = 0f;
            float width = 0f;

            if (CollidersInitialized)
            {
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
            }

            return new Vector3(width, height, width);
        }
    }

    /**
     * The user foot dimensions in local space
     */
    public Vector3 LocalFootDimensions
    {
        get
        {
            float height = 0f;
            float width = 0f;

            if (CollidersInitialized)
            {
                if (footCollider is CapsuleCollider)
                {
                    CapsuleCollider capsuleCollider = (footCollider as CapsuleCollider);
                    width = capsuleCollider.radius * 2f;
                    height = capsuleCollider.height;
                }
                else if (footCollider is CharacterController)
                {
                    CharacterController charCollider = (footCollider as CharacterController);
                    width = charCollider.radius * 2f;
                    height = charCollider.height;
                }
            }

            return new Vector3(width, height, width);
        }
    }

    #endregion //LOCAL_SPACE_COMPONENTS

    /**
     * The user height in local space
     */
    public float LocalHeight
    {
        get
        {
            return HeadOffset + LocalBodyHeight + LocalFootHeight;
        }
    }

    /**
     * The user width in local space
     */
    public float LocalWidth
    {
        get
        {
            return (LocalBodyWidth > LocalFootWidth) ? LocalBodyWidth : LocalFootWidth;
        }
    }

    #endregion //LOCAL_SPACE

    // World space properties
    #region WORLD_SPACE

    // World space component properties
    #region WORLD_SPACE_COMPONENTS

    /**
     * The user body height in world space
     */
    public float WorldBodyHeight
    {
        get
        {
            return WorldBodyDimensions.y;
        }
    }

    /**
     * The user body width in world space
     */
    public float WorldBodyWidth
    {
        get
        {
            return WorldBodyDimensions.x;
        }
    }

    /**
     * The user foot height in world space
     */
    public float WorldFootHeight
    {
        get
        {
            return WorldFootDimensions.y;
        }
    }

    /**
     * The user foot width in world space
     */
    public float WorldFootWidth
    {
        get
        {
            return WorldFootDimensions.x;
        }
    }

    /**
     * The user body dimensions in world space
     */
    public Vector3 WorldBodyDimensions
    {
        get
        {
            Vector3 result = Vector3.zero;
            if (bodyCollider != null)
            {
                result = bodyCollider.transform.TransformVector(LocalBodyDimensions);
            }
            return result;
        }
    }

    /**
     * The user foot dimensions in world space
     */
    public Vector3 WorldFootDimensions
    {
        get
        {
            Vector3 result = Vector3.zero;
            if (footCollider != null)
            {
                result = footCollider.transform.TransformVector(LocalFootDimensions);
            }
            return result;
        }
    }

    #endregion //WORLD_SPACE_COMPONENTS

    /**
     * The user height in world space
     */
    public float WorldHeight
    {
        get
        {
            return HeadOffset + WorldBodyHeight + WorldFootHeight;
        }
    }

    /**
     * The user width in world space
     */
    public float WorldWidth
    {
        get
        {
            return (WorldBodyWidth > WorldFootWidth) ? WorldBodyWidth : WorldFootWidth;
        }
    }

    #endregion //WORLD_SPACE

    #endregion //PROPERTIES

    /**
     * Loads the user colliders that define the user's dimensions
     */
    protected void LoadColliders()
    {
        // Obtaining the collider is different in VR vs Desktop mode
        if (VRDesktopSwitcher.isVREnabled())
        {
            // The user collider is generated automatically in VRTK and can be obtained through the body physics script
            if (sessionManager.BodyPhysics != null)
            {
                // Head offset from the body collider
                headOffset = sessionManager.BodyPhysics.headsetYOffset;

                // Body collider
                GameObject bodyPhysicsContainer = sessionManager.BodyPhysics.GetBodyColliderContainer();
                if (bodyPhysicsContainer != null)
                {
                    // Assign the body collider
                    bodyCollider = bodyPhysicsContainer.GetComponent<CapsuleCollider>();
                }

                // Foot collider
                GameObject footPhysicsContainer = sessionManager.BodyPhysics.GetFootColliderContainer();
                if (footPhysicsContainer != null)
                {
                    // Assign the foot collider
                    footCollider = footPhysicsContainer.GetComponent<CapsuleCollider>();
                }
            }
        }
        else
        {
            // Make sure we have a valid headset follower
            if (sessionManager.headsetFollower != null)
            {
                // The user collider is contained within the character controller
                GameObject colliderContainer = sessionManager.headsetFollower.transform.parent.gameObject;
                if (colliderContainer != null)
                {
                    // Assign the body collider
                    bodyCollider = colliderContainer.GetComponent<CharacterController>();

                    // Assign the foot collider
                    footCollider = null;
                }
            }
        }
    }

    /**
     * Called by Unity as part of the MonoBehavior instantiation process.
     * Guaranteed to be called after the Awake methods are called and before the first
     * frame update.
     */
    protected virtual void Start()
    {
        // Obtain a reference to the session manager which should have references to everything we need
        sessionManager = SessionManager.instance;

        // Try to get a reference to the data manager if not explictly set
        if (dataManager == null)
        {
            dataManager = sessionManager.dataManager;
            if (dataManager == null)
            {
                Debug.LogError("[" + NAME + "] Unable to obtain a reference to a DataManager");
            }
        }
    }

    /**
     * Called by Unity once each frame
     */
    protected virtual void Update()
    {
        // Performance management
        updateCounter++;
        if (updateCounter >= updateRateModulo)
        {
            // Reset the update counter
            updateCounter = 0;

            // Make sure we are only updating the local user. We don't want to report local
            // telemetry if this is a remote user. 
            if (isLocal)
            {
                // Make sure the user is initialized
                if (!initialized)
                {
                    // Place the local user into the play area
                    // NOTE: This cannot be done in Start because the transform isn't available during initialization
                    if (VRTK.VRTK_DeviceFinder.PlayAreaTransform() != null)
                    {
                        transform.SetParent(VRTK.VRTK_DeviceFinder.PlayAreaTransform());
                        initialized = true;
                        // TODO: Can MRETUserManager be rewritten to just have an instance variable? The
                        // Get method does a Find on itself?
                        MRETUserManager.Get().UpdateUserList();
                    }
                }

                // Make sure we have a DataManager reference. Otherwise, we will skip this logic altogether
                if (dataManager != null)
                {
                    // Load the user colliders. We need to continually check here because:
                    //   1) VRTK doesn't initialize colliders in Awake, so it can't be guaranteed
                    //      to be set in Start.
                    //   2) If the user enables flying, the colliders are destroyed in MRET. They are recreated
                    //      if flying is disabled. There are other locomotion states that can invalidate colliders,
                    //      as well
                    if (!CollidersInitialized) LoadColliders();

                    // Store the user dimensions
                    dataManager.SaveValue(KEY_USER_HEIGHT_M, WorldHeight);
                    dataManager.SaveValue(KEY_USER_WIDTH_M, WorldWidth);

                    // Make sure the headset follower reference is valid because this is how we will get the user transforms
                    if (sessionManager.headsetFollower != null)
                    {
                        // Get the orientation transform
                        Transform orientationTransform = sessionManager.headsetFollower.transform.parent;
                        if (orientationTransform != null)
                        {
                            // Store the orientation transform into the DataManager
                            dataManager.SaveValue(KEY_USER_ORIENTATION, orientationTransform.forward);
                            dataManager.SaveValue(KEY_USER_EYE_POSITION, orientationTransform.position);
                            dataManager.SaveValue(KEY_USER_EYE_LOCALPOSITION, orientationTransform.localPosition);

                            // Get the user transform. TODO: Different if desktop mode
                            Transform userTransform = VRDesktopSwitcher.isVREnabled() ? orientationTransform.parent : orientationTransform;

                            // Store the user transforms into the DataManager
                            if (userTransform != null)
                            {
                                // World transforms
                                dataManager.SaveValue(KEY_USER_POSITION, userTransform.position);
                                dataManager.SaveValue(KEY_USER_ROTATION, userTransform.rotation);
                                dataManager.SaveValue(KEY_USER_EULERANGLES, userTransform.eulerAngles);

                                // Local transforms
                                dataManager.SaveValue(KEY_USER_LOCALPOSITION, userTransform.localPosition);
                                dataManager.SaveValue(KEY_USER_LOCALSCALE, userTransform.localScale);
                                dataManager.SaveValue(KEY_USER_LOCALROTATION, userTransform.localRotation);
                                dataManager.SaveValue(KEY_USER_LOCALEULERANGLES, userTransform.localEulerAngles);
                            }
                        }
                    }
                }
            }
        }
    }
}