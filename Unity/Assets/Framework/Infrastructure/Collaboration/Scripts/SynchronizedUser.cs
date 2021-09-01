// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.XRC;

public class SynchronizedUser : MonoBehaviour
{
    public enum LabelColor { red = 1, yellow = 2, blue = 3, black = 4, green = 5, purple = 6 }
    public enum UserType { VR = 0, Desktop = 1 }

    public SynchronizationManager synchronizationManager;
    public Object entityLock = new Object();

    public string userAlias = "New User";
    public string avatarName = "default";
    public UserType userType = UserType.VR;
    public GameObject userObject;
    public bool isControlled = false;
    public int throttleFactor = 10;
    public float translationResolution = 0.01f, rotationResolution = 10f, scaleResolution = 0.05f;
    public System.Guid uuid = System.Guid.NewGuid();
    public Text userLabel;
    public LabelColor userLabelColor;
    public SynchronizedController leftController, rightController;

    private CollaborationManager collaborationManager;
    private Vector3 lastRecordedPosition, lastRecordedScale;
    private Quaternion lastRecordedRotation;
    private int positionThrottleCounter = 0, rotationThrottleCounter = 0, scaleThrottleCounter = 0;
    
    public static string GetRandomColor()
    {
        System.Random rand = new System.Random();

        LabelColor color = (LabelColor) rand.Next(1, 6);
        return color.ToString();
    }

    public static LabelColor LabelColorFromString(string colorName)
    {
        switch (colorName)
        {
            case "red":
                return LabelColor.red;

            case "yellow":
                return LabelColor.yellow;

            case "blue":
                return LabelColor.blue;

            case "black":
                return LabelColor.black;

            case "green":
                return LabelColor.green;

            case "purple":
                return LabelColor.purple;

            default:
                return LabelColor.black;
        }
    }

    public static SynchronizedUser Create(string alias, string avatar, UserType type, string color, string uuid = null,
        string lcUUID = null, string rcUUID = null, string lpUUID = null, string rpUUID = null,
        bool controlled = false, int throttle = 10, float transRes = 0.01f, float rotRes = 10f,
        float sclRes = 0.05f)
    {
        CollaborationManager cMan = FindObjectOfType<CollaborationManager>();
        if (cMan == null)
        {
            Debug.LogError("[SynchronizedUser->Create] No Collaboration Manager found. Unable to produce a user.");
        }

        // Set up User Avatar.
        //if (string.IsNullOrEmpty(avatar))
        {
            return CreateAvatar((type == UserType.VR) ? cMan.vrAvatarPrefab : cMan.desktopAvatarPrefab,
                cMan.userContainer.transform, alias, avatar, type, color, uuid,
                lcUUID, rcUUID, lpUUID, rpUUID, controlled, throttle, transRes, rotRes, sclRes);
        }
    }

    private static SynchronizedUser CreateAvatar(GameObject avatarPrefab, Transform avatarParent,
        string userName, string avatarName, UserType type, string userColor,
        string uuid = null, string lcUUID = null, string rcUUID = null, string lpUUID = null, string rpUUID = null,
        bool controlled = false, int throttle = 10, float transRes = 0.01f, float rotRes = 10f, float sclRes = 0.05f)
    {
        if (!controlled)
        {
            GameObject newUserAvatar = Instantiate(avatarPrefab);
            SynchronizedUser newUser = newUserAvatar.AddComponent<SynchronizedUser>();
            newUser.userAlias = newUserAvatar.name = userName;
            newUser.userType = type;
            newUser.isControlled = controlled;
            newUser.throttleFactor = throttle;
            newUser.translationResolution = transRes;
            newUser.rotationResolution = rotRes;
            newUser.scaleResolution = sclRes;
            newUser.avatarName = avatarName;
            newUserAvatar.transform.position = Vector3.zero;
            newUserAvatar.transform.parent = avatarParent;
            newUser.userObject = newUserAvatar;
            newUser.uuid = string.IsNullOrEmpty(uuid) ?
                System.Guid.NewGuid() : System.Guid.Parse(uuid);
            newUser.userLabel = newUserAvatar.GetComponentInChildren<Text>();
            newUser.userLabelColor = LabelColorFromString(userColor);
            if (newUser.userLabel)
            {
                newUser.userLabel.text = newUser.userAlias;
                Color labelColor;
                if (ColorUtility.TryParseHtmlString(userColor, out labelColor))
                {
                    newUser.userLabel.color = labelColor;
                }
            }

            newUser.leftController = newUser.transform.Find("Left").gameObject.AddComponent<SynchronizedController>();
            newUser.leftController.controllerSide = SynchronizedController.ControllerSide.Left;
            newUser.leftController.synchronizedUser = newUser;
            newUser.leftController.uuid = string.IsNullOrEmpty(lcUUID) ?
                System.Guid.NewGuid() : System.Guid.Parse(lcUUID);

            newUser.leftController.pointer = newUser.transform.Find("Left/Laser").gameObject.AddComponent<SynchronizedPointer>();
            newUser.leftController.pointer.synchronizedController = newUser.leftController;
            newUser.leftController.pointer.lineRenderer = newUser.transform.Find("Left/Laser").GetComponent<LineRenderer>();
            newUser.leftController.pointer.uuid = string.IsNullOrEmpty(lpUUID) ?
                System.Guid.NewGuid() : System.Guid.Parse(lpUUID);

            if (newUser.userType == UserType.VR)
            {
                newUser.rightController = newUser.transform.Find("Right").gameObject.AddComponent<SynchronizedController>();
                newUser.rightController.controllerSide = SynchronizedController.ControllerSide.Right;
                newUser.rightController.synchronizedUser = newUser;
                newUser.rightController.uuid = string.IsNullOrEmpty(rcUUID) ?
                    System.Guid.NewGuid() : System.Guid.Parse(rcUUID);

                newUser.rightController.pointer = newUser.transform.Find("Right/Laser").gameObject.AddComponent<SynchronizedPointer>();
                newUser.rightController.pointer.synchronizedController = newUser.rightController;
                newUser.rightController.pointer.lineRenderer = newUser.transform.Find("Right/Laser").GetComponent<LineRenderer>();
                newUser.rightController.pointer.uuid = string.IsNullOrEmpty(rpUUID) ?
                    System.Guid.NewGuid() : System.Guid.Parse(rpUUID);
            }

            return newUser;
        }
        else
        {
            // TODO: Needs to be moved from project loading to here.
            return null;
        }
    }

    void Start()
    {
        if (synchronizationManager == null)
        {
            synchronizationManager = FindObjectOfType<SynchronizationManager>();
        }

        collaborationManager = FindObjectOfType<CollaborationManager>();

        //Vector3 currPos = UnityProject.instance.GlobalToLocalPosition(transform.position);
        lastRecordedPosition = transform.position; //currPos;
        lastRecordedRotation = transform.rotation; //transform.localRotation;
        lastRecordedScale = transform.localScale;
    }

    void Update()
    {
#if !HOLOLENS_BUILD
        positionThrottleCounter++;
        rotationThrottleCounter++;
        scaleThrottleCounter++;

        if (isControlled)
        {
            // Send Transform (position, rotation, scale) changes.
            lock (entityLock)
            {
                if (transform.hasChanged && synchronizationManager != null)
                {
                    Rigidbody rBody = GetComponent<Rigidbody>();
                    if (rBody)
                    {
                        if (!rBody.IsSleeping())
                        {
                            return;
                        }
                    }

                    // Position.
                    if (transform.position != lastRecordedPosition && positionThrottleCounter >= throttleFactor)
                    //Vector3 currPos = UnityProject.instance.GlobalToLocalPosition(transform.position);
                    //if (currPos != lastRecordedPosition && positionThrottleCounter >= throttleFactor)
                    {
                        if ((transform.position - lastRecordedPosition).magnitude > translationResolution)
                        //if ((currPos - lastRecordedPosition).magnitude > translationResolution)
                        {
                            lastRecordedPosition = transform.position; // currPos;
                            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
                            {
                                XRCUnity.UpdateEntityPosition(userAlias, XRCManager.USERCATEGORY, lastRecordedPosition,
                                    uuid.ToString(), null, GSFC.ARVR.XRC.UnitType.meter);
                            }
                            else
                            {
                                synchronizationManager.SendPositionChange(gameObject);
                            }
                        }
                        positionThrottleCounter = 0;
                    }

                    // Rotation.
                    if (transform.rotation != lastRecordedRotation && rotationThrottleCounter >= throttleFactor)
                    //if (transform.localRotation != lastRecordedRotation && rotationThrottleCounter >= throttleFactor)
                    {
                        if ((transform.rotation.eulerAngles - lastRecordedRotation.eulerAngles).magnitude > rotationResolution)
                        //if ((transform.localRotation.eulerAngles - lastRecordedRotation.eulerAngles).magnitude > rotationResolution)
                        {
                            lastRecordedRotation = transform.rotation; //transform.localRotation;
                            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
                            {
                                XRCUnity.UpdateEntityRotation(userAlias, XRCManager.USERCATEGORY, lastRecordedRotation,
                                    uuid.ToString(), null, GSFC.ARVR.XRC.UnitType.degrees);
                            }
                            else
                            {
                                synchronizationManager.SendRotationChange(gameObject);
                            }
                        }
                        rotationThrottleCounter = 0;
                    }

                    // Scale.
                    if (transform.localScale != lastRecordedScale && scaleThrottleCounter >= throttleFactor)
                    {
                        if ((transform.localScale - lastRecordedScale).magnitude > scaleResolution)
                        {
                            lastRecordedScale = transform.localScale;
                            if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
                            {
                                XRCUnity.UpdateEntityScale(userAlias, XRCManager.USERCATEGORY, lastRecordedScale,
                                    uuid.ToString(), null, GSFC.ARVR.XRC.UnitType.unitless);
                            }
                            else
                            {
                                synchronizationManager.SendScaleChange(gameObject);
                            }
                        }
                        scaleThrottleCounter = 0;
                    }
                    transform.hasChanged = false;
                }
            }
        }
#endif
    }
}