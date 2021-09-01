// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.GMSEC;
using GSFC.ARVR.UTILITIES;

public class SynchronizationManager : MonoBehaviour
{
    private class EntityStatus
    {
        public GameObject gameObject;
        public System.DateTime occurrenceTime;
        public EntityStateAttributeTypes attributeType;
        public string attributeSubType;
        public Transform transform;
        public string text;

        public EntityStatus(GameObject ob, System.DateTime ti, EntityStateAttributeTypes at, Transform tr, string ast = null, string txt = null)
        {
            gameObject = ob;
            occurrenceTime = ti;
            attributeType = at;
            attributeSubType = ast;
            transform = tr;
            text = txt;
        }
    }

    public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };
    public enum EntityStateAttributeTypes
    {
        create, destroy, reinitialize, localPosition, localRotation, localScale,
        absolutePosition, absoluteRotation, absoluteScale, relativePosition, relativeRotation, relativeScale, edit
    };

    [Tooltip("The middleware type to use.")]
    public ConnectionTypes connectionType = ConnectionTypes.bolt;

    [Tooltip("The server address and port number.")]
    public string server = "localhost:9100";

    [Tooltip("The mission name to use.")]
    public string missionName = "UNSET";

    [Tooltip("The satellite name to use.")]
    public string satName = "UNSET";

    [Tooltip("The group identifier to use.")]
    public string groupID = "UNSET";

    [Tooltip("The project name to use.")]
    public string projectName = "UNSET";

    [Tooltip("The user name to use.")]
    public string userName = "UNSET";

    public float controllerTranslationResolution = 0.01f, controllerRotationResolution = 10f, controllerScaleResolution = 0.05f,
        userTranslationResolution = 0.01f, userRotationResolution = 10f, userScaleResolution = 0.05f,
        objectTranslationResolution = 0.01f, objectRotationResolution = 10f, objectScaleResolution = 0.05f;

    public SessionAdvertiser sessionAdvertiser;
    public SessionMasterNode masterNode;

    // A MonoGMSEC object is needed to interface with GMSEC.
    private MonoGMSEC gmsec;

    // Keep track of if a subscription has been set up.
    private bool subscribed = false;

    // TODO.
    private bool updateSent = false;

    private FixedSizeQueue<GMSECMessage> incomingMessageQueue = new FixedSizeQueue<GMSECMessage>();
    private FixedSizeQueue<EntityStatus> outgoingEntityStatus = new FixedSizeQueue<EntityStatus>();

    void Start()
    {
        gmsec = gameObject.AddComponent<MonoGMSEC>();
    }

    void Update()
    {
        if (subscribed)
        {
            ReceiveMessage();

            if (incomingMessageQueue.GetCount() > 0)
            {
                GMSECMessage msg = incomingMessageQueue.Dequeue();
                if (msg != null)
                {
                    GameObject entityBeingControlled = null;
                    SynchronizedObject synchedObj = null;
                    SynchronizedUser synchedUser = null;
                    SynchronizedController synchedController = null;
                    SynchronizedPointer synchedPointer = null;
                    SynchronizedNote synchedNote = null;

                    string[] subjectFields = msg.GetSubject().Split('.');
                    if (subjectFields.Length > 3 && subjectFields[4] == "ESTATE")
                    {
                        if (subjectFields[3] == "MSG")
                        {
                            string messageUser = subjectFields[8];
                            if (messageUser == userName.ToUpper())
                            {
                                // This was our message; ignore it.
                                return;
                            }

                            string timeStamp = (string)msg.GetField("ATTRIBUTE-TIMESTAMP").GetValue();
                            string entity = (string)msg.GetField("ENTITY").GetValue();
                            int attributeType = (System.Int16)msg.GetField("ATTRIBUTE").GetValue();

                            switch (attributeType)
                            {
                                // Create.
                                case 1:
                                    break;

                                // Destroy.
                                case 2:
                                    break;

                                // Reinitialize.
                                case 3:
                                    break;

                                // Set local position.
                                case 4:
                                    break;

                                // Set local rotation.
                                case 5:
                                    break;

                                // Set local scale.
                                case 6:
                                    break;

                                // Set global position.
                                case 7:
                                    entityBeingControlled = GameObject.Find(entity);
                                    if (entityBeingControlled)
                                    {
                                        synchedObj = entityBeingControlled.GetComponent<SynchronizedObject>();
                                        if (!synchedObj)
                                        {
                                            synchedUser = entityBeingControlled.GetComponent<SynchronizedUser>();
                                            if (!synchedUser)
                                            {
                                                synchedController = entityBeingControlled.GetComponent<SynchronizedController>();
                                                if (!synchedController)
                                                {
                                                    synchedPointer = entityBeingControlled.GetComponent<SynchronizedPointer>();
                                                    if (!synchedPointer)
                                                    {
                                                        synchedNote = entityBeingControlled.GetComponent<SynchronizedNote>();
                                                        if (!synchedNote)
                                                        {
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            lock (synchedNote.entityLock)
                                                            {
                                                                if (entityBeingControlled != null)
                                                                {
                                                                    float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                                    float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                                    float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                                    Vector3 newPos = new Vector3(xValue, yValue, zValue);
                                                                    synchedPointer.SetPosition(newPos);
                                                                    entityBeingControlled.transform.hasChanged = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        lock (synchedPointer.entityLock)
                                                        {
                                                            if (entityBeingControlled != null)
                                                            {
                                                                float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                                float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                                float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                                Vector3 newPos = new Vector3(xValue, yValue, zValue);
                                                                synchedPointer.SetPosition(newPos);
                                                                entityBeingControlled.transform.hasChanged = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    lock (synchedController.entityLock)
                                                    {
                                                        if (entityBeingControlled != null)
                                                        {
                                                            float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                            float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                            float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                            Vector3 newPos = new Vector3(xValue, yValue, zValue);
                                                            if ((newPos - entityBeingControlled.transform.position).magnitude > controllerTranslationResolution)
                                                            {
                                                                entityBeingControlled.transform.position = newPos;
                                                            }
                                                            entityBeingControlled.transform.hasChanged = false;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lock (synchedUser.entityLock)
                                                {
                                                    if (entityBeingControlled != null)
                                                    {
                                                        float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                        float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                        float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                        Vector3 newPos = new Vector3(xValue, yValue, zValue);
                                                        if ((newPos - entityBeingControlled.transform.position).magnitude > userTranslationResolution)
                                                        {
                                                            entityBeingControlled.transform.position = newPos;
                                                        }
                                                        entityBeingControlled.transform.hasChanged = false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            lock (synchedObj.entityLock)
                                            {
                                                if (entityBeingControlled != null)
                                                {
                                                    float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                    float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                    float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                    Vector3 newPos = new Vector3(xValue, yValue, zValue);
                                                    if ((newPos - entityBeingControlled.transform.position).magnitude > objectTranslationResolution)
                                                    {
                                                        entityBeingControlled.transform.position = newPos;
                                                    }
                                                    entityBeingControlled.transform.hasChanged = false;
                                                }
                                            }
                                        }
                                    }
                                    break;

                                // Set global rotation.
                                case 8:
                                    entityBeingControlled = GameObject.Find(entity);
                                    if (entityBeingControlled)
                                    {
                                        synchedObj = entityBeingControlled.GetComponent<SynchronizedObject>();
                                        if (!synchedObj)
                                        {
                                            synchedUser = entityBeingControlled.GetComponent<SynchronizedUser>();
                                            if (!synchedUser)
                                            {
                                                synchedController = entityBeingControlled.GetComponent<SynchronizedController>();
                                                if (!synchedController)
                                                {
                                                    synchedNote = entityBeingControlled.GetComponent<SynchronizedNote>();
                                                    if (!synchedNote)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        lock (synchedNote.entityLock)
                                                        {
                                                            if (entityBeingControlled != null)
                                                            {
                                                                float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                                float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                                float zValue = (float)msg.GetField("Z-VALUE").GetValue();
                                                                float wValue = (float)msg.GetField("W-VALUE").GetValue();

                                                                Quaternion newRot = new Quaternion(xValue, yValue, zValue, wValue);
                                                                if ((newRot.eulerAngles - entityBeingControlled.transform.rotation.eulerAngles).magnitude > controllerRotationResolution)
                                                                {
                                                                    entityBeingControlled.transform.rotation = newRot;
                                                                }
                                                                entityBeingControlled.transform.hasChanged = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    lock (synchedController.entityLock)
                                                    {
                                                        if (entityBeingControlled != null)
                                                        {
                                                            float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                            float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                            float zValue = (float)msg.GetField("Z-VALUE").GetValue();
                                                            float wValue = (float)msg.GetField("W-VALUE").GetValue();

                                                            Quaternion newRot = new Quaternion(xValue, yValue, zValue, wValue);
                                                            if ((newRot.eulerAngles - entityBeingControlled.transform.rotation.eulerAngles).magnitude > controllerRotationResolution)
                                                            {
                                                                entityBeingControlled.transform.rotation = newRot;
                                                            }
                                                            entityBeingControlled.transform.hasChanged = false;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lock (synchedUser.entityLock)
                                                {
                                                    if (entityBeingControlled != null)
                                                    {
                                                        float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                        float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                        float zValue = (float)msg.GetField("Z-VALUE").GetValue();
                                                        float wValue = (float)msg.GetField("W-VALUE").GetValue();

                                                        Quaternion newRot = new Quaternion(xValue, yValue, zValue, wValue);
                                                        if ((newRot.eulerAngles - entityBeingControlled.transform.rotation.eulerAngles).magnitude > userRotationResolution)
                                                        {
                                                            entityBeingControlled.transform.rotation = newRot;
                                                        }
                                                        entityBeingControlled.transform.hasChanged = false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            lock (synchedObj.entityLock)
                                            {
                                                if (entityBeingControlled != null)
                                                {
                                                    float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                    float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                    float zValue = (float)msg.GetField("Z-VALUE").GetValue();
                                                    float wValue = (float)msg.GetField("W-VALUE").GetValue();

                                                    Quaternion newRot = new Quaternion(xValue, yValue, zValue, wValue);
                                                    if ((newRot.eulerAngles - entityBeingControlled.transform.rotation.eulerAngles).magnitude > objectRotationResolution)
                                                    {
                                                        entityBeingControlled.transform.rotation = newRot;
                                                    }
                                                    entityBeingControlled.transform.hasChanged = false;
                                                }
                                            }
                                        }
                                    }
                                    break;

                                // Set global scale.
                                case 9:
                                    entityBeingControlled = GameObject.Find(entity);
                                    if (entityBeingControlled)
                                    {
                                        synchedObj = entityBeingControlled.GetComponent<SynchronizedObject>();
                                        if (!synchedObj)
                                        {
                                            synchedUser = entityBeingControlled.GetComponent<SynchronizedUser>();
                                            if (!synchedUser)
                                            {
                                                synchedController = entityBeingControlled.GetComponent<SynchronizedController>();
                                                if (!synchedController)
                                                {
                                                    synchedNote = entityBeingControlled.GetComponent<SynchronizedNote>();
                                                    if (!synchedNote)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        lock (synchedNote.entityLock)
                                                        {
                                                            if (entityBeingControlled != null)
                                                            {
                                                                float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                                float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                                float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                                Vector3 newScl = new Vector3(xValue, yValue, zValue);
                                                                if ((newScl - entityBeingControlled.transform.localScale).magnitude > controllerScaleResolution)
                                                                {
                                                                    entityBeingControlled.transform.localScale = newScl;
                                                                }
                                                                entityBeingControlled.transform.hasChanged = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    lock (synchedController.entityLock)
                                                    {
                                                        if (entityBeingControlled != null)
                                                        {
                                                            float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                            float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                            float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                            Vector3 newScl = new Vector3(xValue, yValue, zValue);
                                                            if ((newScl - entityBeingControlled.transform.localScale).magnitude > controllerScaleResolution)
                                                            {
                                                                entityBeingControlled.transform.localScale = newScl;
                                                            }
                                                            entityBeingControlled.transform.hasChanged = false;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lock (synchedUser.entityLock)
                                                {
                                                    if (entityBeingControlled != null)
                                                    {
                                                        float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                        float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                        float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                        Vector3 newScl = new Vector3(xValue, yValue, zValue);
                                                        if ((newScl - entityBeingControlled.transform.localScale).magnitude > userScaleResolution)
                                                        {
                                                            entityBeingControlled.transform.localScale = newScl;
                                                        }
                                                        entityBeingControlled.transform.hasChanged = false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            lock (synchedObj.entityLock)
                                            {
                                                if (entityBeingControlled != null)
                                                {
                                                    float xValue = (float)msg.GetField("X-VALUE").GetValue();
                                                    float yValue = (float)msg.GetField("Y-VALUE").GetValue();
                                                    float zValue = (float)msg.GetField("Z-VALUE").GetValue();

                                                    Vector3 newScl = new Vector3(xValue, yValue, zValue);
                                                    if ((newScl - entityBeingControlled.transform.localScale).magnitude > objectScaleResolution)
                                                    {
                                                        entityBeingControlled.transform.localScale = newScl;
                                                    }
                                                    entityBeingControlled.transform.hasChanged = false;
                                                }
                                            }
                                        }
                                    }
                                    break;

                                // Modify position.
                                case 10:
                                    break;

                                // Modify rotation.
                                case 11:
                                    break;

                                // Modify scale.
                                case 12:
                                    break;

                                // Edit.
                                case 13:
                                    entityBeingControlled = GameObject.Find(entity);

                                    if (msg.GetField("EDIT-PROPERTY").GetValueAsString() == "NOTETITLE")
                                    {
                                        synchedNote = entityBeingControlled.GetComponent<SynchronizedNote>();
                                        if (synchedNote)
                                        {
                                            string textToUse = msg.GetField("EDIT-STRING-VALUE").GetValueAsString();
                                            if (textToUse != null)
                                            {
                                                synchedNote.SetTitleText(textToUse);
                                            }
                                        }
                                    }
                                    else if (msg.GetField("EDIT-PROPERTY").GetValueAsString() == "NOTEINFORMATION")
                                    {
                                        synchedNote = entityBeingControlled.GetComponent<SynchronizedNote>();
                                        if (synchedNote)
                                        {
                                            string textToUse = msg.GetField("EDIT-STRING-VALUE").GetValueAsString();
                                            if (textToUse != null)
                                            {
                                                synchedNote.SetInformationText(textToUse);
                                            }
                                        }
                                    }
                                    break;

                                // Unknown.
                                default:
                                    break;
                            }
                        }
                        else if (subjectFields[3] == "REQ")
                        {
                            if (masterNode)
                            {
                                masterNode.SendEntityStateResponse(msg);
                            }
                        }
                    }
                    else if (subjectFields.Length > 4 && subjectFields[5] == "SEEKER")
                    {
                        if (sessionAdvertiser)
                        {
#if !HOLOLENS_BUILD
                            sessionAdvertiser.RespondToListRequest(msg);
#endif
                        }
                    }
                }
            }

            if (outgoingEntityStatus.GetCount() > 0)
            {
                EntityStatus eStatus = outgoingEntityStatus.Dequeue();
                if (eStatus != null)
                {
                    Transform transformToUse = eStatus.transform;
                    string pathToUse = GetFullPath(eStatus.gameObject);
                    string nameToUse = eStatus.gameObject.name.ToUpper();
                    if (eStatus.gameObject.GetComponent<SynchronizedUser>())
                    {
                        pathToUse = "/LoadedProject/Users/" + eStatus.gameObject.GetComponent<SynchronizedUser>().userAlias;
                        nameToUse = eStatus.gameObject.GetComponent<SynchronizedUser>().userAlias.ToUpper();
                    }
                    else if (eStatus.gameObject.GetComponent<SynchronizedController>())
                    {
                        pathToUse = "/LoadedProject/Users/" + eStatus.gameObject.GetComponent<SynchronizedController>().synchronizedUser.userAlias
                            + "/" + ((eStatus.gameObject.GetComponent<SynchronizedController>().controllerSide == SynchronizedController.ControllerSide.Left)
                            ? "Left" : "Right");
                        nameToUse = eStatus.gameObject.GetComponent<SynchronizedController>().synchronizedUser.userAlias.ToUpper();
                    }
                    else if (eStatus.gameObject.GetComponent<SynchronizedPointer>())
                    {
                        pathToUse = "/LoadedProject/Users/" + eStatus.gameObject.GetComponent<SynchronizedPointer>().synchronizedController.synchronizedUser.userAlias
                            + "/" + ((eStatus.gameObject.GetComponent<SynchronizedPointer>().synchronizedController.controllerSide == SynchronizedController.ControllerSide.Left)
                            ? "Left/Laser" : "Right/Laser");
                        nameToUse = eStatus.gameObject.GetComponent<SynchronizedPointer>().synchronizedController.synchronizedUser.userAlias.ToUpper();
                        transformToUse = eStatus.gameObject.GetComponent<SynchronizedPointer>().transformToUse;
                        /*if (!eStatus.gameObject.GetComponent<SynchronizedPointer>().pointer.IsPointerActive())
                        {
                            transformToUse.position = Vector3.zero;
                        }*/
                    }
                    else if (eStatus.gameObject.GetComponentInParent<SynchronizedController>())
                    {
                        pathToUse = eStatus.gameObject.GetComponent<SynchronizedObject>().objectPath;
                    }

                    PublishStateAttributeDataMessage("GMSEC." + missionName.ToUpper() + "." + satName.ToUpper() + ".MSG.ESTATE."
                        + groupID.ToUpper() + "." + projectName.Replace(".mtproj", "").ToUpper() + "." + nameToUse + "." + userName.ToUpper(),
                        eStatus.attributeType, eStatus.attributeSubType, pathToUse, eStatus.occurrenceTime, transformToUse, eStatus.text);
                }
            }
        }
    }

    public void Initialize()
    {
        if (gmsec == null)
        {
            gmsec = gameObject.AddComponent<MonoGMSEC>();
        }

        //Debug.Log("[SynchronizationManager] Initializing GMSEC Object");
        //gmsec.Initialize();
        //Debug.Log("[SynchronizationManager] GMSEC Initialized");

//Debug.Log("[SynchronizationManager] Setting up Config");
//gmsec.CreateConfig();

//gmsec.AddToConfig("connectionType", ConnectionTypeToString(connectionType));
//gmsec.AddToConfig("server", server);
//gmsec.AddToConfig("GMSEC-REQ-RESP", "OPEN-RESP");   // Enable GMSEC open response feature.
//Debug.Log("[SynchronizationManager] Config Initialized");

//Debug.Log("[SynchronizationManager] Connecting");
//gmsec.Connect();
//Debug.Log("[SynchronizationManager] Connected");

#if !HOLOLENS_BUILD
        Debug.Log("[SynchronizationManager] Subscribing");
        gmsec.Subscribe("GMSEC." + missionName.ToUpper() + "." + satName.ToUpper()
            + ".MSG.ESTATE." + groupID.ToUpper() + "." + projectName.Replace(".mtproj", "").ToUpper() + ".*.*");
        subscribed = true;
        Debug.Log("[SynchronizationManager] Subscribed");
#endif
    }

    public void SendPositionChange(GameObject synchronizedObject)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedObject, System.DateTime.Now, EntityStateAttributeTypes.absolutePosition, synchronizedObject.transform));
    }

    public void SendPositionChange(GameObject synchronizedObject, System.DateTime occurrenceTime)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedObject, occurrenceTime, EntityStateAttributeTypes.absolutePosition, synchronizedObject.transform));
    }

    public void SendRotationChange(GameObject synchronizedObject)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedObject, System.DateTime.Now, EntityStateAttributeTypes.absoluteRotation, synchronizedObject.transform));
    }

    public void SendRotationChange(GameObject synchronizedObject, System.DateTime occurrenceTime)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedObject, occurrenceTime, EntityStateAttributeTypes.absoluteRotation, synchronizedObject.transform));
    }

    public void SendScaleChange(GameObject synchronizedObject)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedObject, System.DateTime.Now, EntityStateAttributeTypes.absoluteScale, synchronizedObject.transform));
    }

    public void SendScaleChange(GameObject synchronizedObject, System.DateTime occurrenceTime)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedObject, occurrenceTime, EntityStateAttributeTypes.absoluteScale, synchronizedObject.transform));
    }

    public void SendNoteTitleChange(GameObject synchronizedNote, string newTitle)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedNote, System.DateTime.Now, EntityStateAttributeTypes.edit, synchronizedNote.transform, "NOTETITLE", newTitle));
    }

    public void SendNoteTitleChange(GameObject synchronizedNote, System.DateTime occurrenceTime, string newTitle)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedNote, occurrenceTime, EntityStateAttributeTypes.edit, synchronizedNote.transform, "NOTETITLE", newTitle));
    }

    public void SendNoteInformationChange(GameObject synchronizedNote, string newInfo)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedNote, System.DateTime.Now, EntityStateAttributeTypes.edit, synchronizedNote.transform, "NOTEINFORMATION", newInfo));
    }

    public void SendNoteInformationChange(GameObject synchronizedNote, System.DateTime occurrenceTime, string newInfo)
    {
        outgoingEntityStatus.Enqueue(new EntityStatus(synchronizedNote, occurrenceTime, EntityStateAttributeTypes.edit, synchronizedNote.transform, "NOTEINFORMATION", newInfo));
    }

    void PublishStateAttributeDataMessage(string subjectToPublish, EntityStateAttributeTypes stateAttributeType,
        string stateAttributeSubType, string entityName, System.DateTime dateTime, Transform transformToApply, string text)
    {
        PublishStateAttributeDataMessage(subjectToPublish, stateAttributeType, stateAttributeSubType, entityName,
            dateTime, transformToApply, text, Vector3.zero, Quaternion.identity);
    }

    void PublishStateAttributeDataMessage(string subjectToPublish, EntityStateAttributeTypes stateAttributeType,
        string stateAttributeSubType, string entityName, System.DateTime dateTime, Transform transformToApply,
        string text, Vector3 offset3, Quaternion offset4)
    {
#if !HOLOLENS_BUILD
        // Create Message with Header.
        gmsec.CreateNewMessage(subjectToPublish.Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", ""));
        gmsec.AddF32FieldToMessage("HEADER-VERSION", 2010);
        gmsec.AddStringFieldToMessage("MESSAGE-TYPE", "MSG");
        gmsec.AddStringFieldToMessage("MESSAGE-SUBTYPE", "ESTATE");
        gmsec.AddF32FieldToMessage("CONTENT-VERSION", 2018);

        // Time Stamp.
        dateTime = dateTime.ToUniversalTime();
        string timeStamp = dateTime.Year.ToString() + "-" + dateTime.DayOfYear.ToString("000") + "-" + dateTime.Hour.ToString("00")
            + ":" + dateTime.Minute.ToString("00") + ":" + dateTime.Second.ToString("00") + "." + dateTime.Millisecond.ToString("000");
        gmsec.AddStringFieldToMessage("ATTRIBUTE-TIMESTAMP", timeStamp);

        // Entity Name.
        gmsec.AddStringFieldToMessage("ENTITY", entityName.Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", ""));

        switch (stateAttributeType)
        {
            case EntityStateAttributeTypes.create:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 1);
                break;

            case EntityStateAttributeTypes.destroy:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 2);
                break;

            case EntityStateAttributeTypes.reinitialize:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 3);
                break;

            case EntityStateAttributeTypes.localPosition:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 4);
                gmsec.AddF32FieldToMessage("X-VALUE", transformToApply.localPosition.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", transformToApply.localPosition.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", transformToApply.localPosition.z);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.localRotation:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 5);
                gmsec.AddF32FieldToMessage("X-VALUE", transformToApply.localRotation.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", transformToApply.localRotation.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", transformToApply.localRotation.z);
                gmsec.AddF32FieldToMessage("W-VALUE", transformToApply.localRotation.w);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.localScale:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 6);
                gmsec.AddF32FieldToMessage("X-VALUE", transformToApply.localScale.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", transformToApply.localScale.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", transformToApply.localScale.z);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.absolutePosition:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 7);
                gmsec.AddF32FieldToMessage("X-VALUE", transformToApply.position.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", transformToApply.position.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", transformToApply.position.z);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.absoluteRotation:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 8);
                gmsec.AddF32FieldToMessage("X-VALUE", transformToApply.rotation.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", transformToApply.rotation.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", transformToApply.rotation.z);
                gmsec.AddF32FieldToMessage("W-VALUE", transformToApply.rotation.w);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.absoluteScale:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 9);
                gmsec.AddF32FieldToMessage("X-VALUE", transformToApply.localScale.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", transformToApply.localScale.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", transformToApply.localScale.z);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.relativePosition:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 10);
                gmsec.AddF32FieldToMessage("X-VALUE", offset3.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", offset3.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", offset3.z);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.relativeRotation:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 11);
                gmsec.AddF32FieldToMessage("X-VALUE", offset4.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", offset4.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", offset4.z);
                gmsec.AddF32FieldToMessage("W-VALUE", offset4.w);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.relativeScale:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 12);
                gmsec.AddF32FieldToMessage("X-VALUE", offset3.x);
                gmsec.AddF32FieldToMessage("Y-VALUE", offset3.y);
                gmsec.AddF32FieldToMessage("Z-VALUE", offset3.z);
                gmsec.AddStringFieldToMessage("UNITS", "UNITY");
                break;

            case EntityStateAttributeTypes.edit:
                gmsec.AddI16FieldToMessage("ATTRIBUTE", 13);
                if (stateAttributeSubType != null)
                {
                    if (stateAttributeSubType == "NOTETITLE")
                    {
                        gmsec.AddStringFieldToMessage("EDIT-PROPERTY", "NOTETITLE");
                        gmsec.AddStringFieldToMessage("EDIT-STRING-VALUE", text);
                    }
                    else if (stateAttributeSubType == "NOTEINFORMATION")
                    {
                        gmsec.AddStringFieldToMessage("EDIT-PROPERTY", "NOTEINFORMATION");
                        gmsec.AddStringFieldToMessage("EDIT-STRING-VALUE", text);
                    }
                }
                break;

            default:
                return;
        }
        gmsec.PublishMessage();
#endif
    }

    void ReceiveMessage()
    {
#if !HOLOLENS_BUILD
        incomingMessageQueue.Enqueue(gmsec.Receive(0));
#endif
    }

    // Disconnect should be called to clean up connection resources.
    void OnDestroy()
    {
#if !HOLOLENS_BUILD
        if (gmsec)
        {
            gmsec.Disconnect();
        }
#endif
    }

#region Helpers
    private string ConnectionTypeToString(ConnectionTypes rawConnType)
    {
        string connType = "gmsec_bolt";

        switch (rawConnType)
        {
            case ConnectionTypes.amq383:
                connType = "gmsec_activemq383";
                break;

            case ConnectionTypes.amq384:
                connType = "gmsec_activemq384";
                break;

            case ConnectionTypes.bolt:
                connType = "gmsec_bolt";
                break;

            case ConnectionTypes.mb:
                connType = "gmsec_mb";
                break;

            case ConnectionTypes.ws71:
                connType = "gmsec_websphere71";
                break;

            case ConnectionTypes.ws75:
                connType = "gmsec_websphere75";
                break;

            case ConnectionTypes.ws80:
                connType = "gmsec_websphere80";
                break;

            default:
                connType = "gmsec_bolt";
                break;
        }

        return connType;
    }

    private string GetFullPath(GameObject obj)
    {
        GameObject highestObj = obj;
        string fullPath = highestObj.name;

        while (highestObj.transform.parent != null)
        {
            highestObj = highestObj.transform.parent.gameObject;
            fullPath = highestObj.name + "/" + fullPath;
        }

        return fullPath;
    }
#endregion
}