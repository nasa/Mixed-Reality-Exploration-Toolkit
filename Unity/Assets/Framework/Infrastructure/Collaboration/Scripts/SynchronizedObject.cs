// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.XRC;

public class SynchronizedObject : MonoBehaviour
{
    public SynchronizationManager synchronizationManager;
    public InteractablePart interactablePart;
    public Object entityLock = new Object();
    public int throttleFactor = 10;
    public float translationResolution = 0.01f, rotationResolution = 10f, scaleResolution = 0.05f;
    public string objectPath;
    public bool autoSynchronize = false;

    private CollaborationManager collaborationManager;
    private Vector3 lastRecordedPosition, lastRecordedScale;
    private Quaternion lastRecordedRotation;
    private int positionThrottleCounter = 0, rotationThrottleCounter = 0, scaleThrottleCounter = 0;

    void Start()
    {
        if (synchronizationManager == null)
        {
            synchronizationManager = FindObjectOfType<SynchronizationManager>();
        }

        collaborationManager = FindObjectOfType<CollaborationManager>();
        interactablePart = GetComponent<InteractablePart>();

        //Vector3 currPos = UnityProject.instance.GlobalToLocalPosition(transform.position);
        lastRecordedPosition = transform.position; //currPos;
        lastRecordedRotation = transform.rotation; //transform.localRotation;
        lastRecordedScale = transform.localScale;

        objectPath = GetFullPath(gameObject);
    }

    void Update()
    {
#if !HOLOLENS_BUILD
        positionThrottleCounter++;
        rotationThrottleCounter++;
        scaleThrottleCounter++;

        // Send Transform (position, rotation, scale) changes.
        lock (entityLock)
        {
            if (transform.hasChanged && autoSynchronize && synchronizationManager != null)
            {
                Rigidbody rBody = GetComponent<Rigidbody>();
                if (rBody)
                {
                    if (!rBody.IsSleeping())
                    {
                        //return;
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
                        lastRecordedPosition = transform.position; //currPos;
                        if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
                        {
                            XRCUnity.UpdateEntityPosition(transform.name, XRCManager.OBJECTCATEGORY, lastRecordedPosition,
                                    interactablePart.guid.ToString(), null, GSFC.ARVR.XRC.UnitType.meter);
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
                {
                    if ((transform.rotation.eulerAngles - lastRecordedRotation.eulerAngles).magnitude > rotationResolution)
                    {
                        lastRecordedRotation = transform.rotation;
                        if (collaborationManager.engineType == CollaborationManager.EngineType.XRC)
                        {
                            XRCUnity.UpdateEntityRotation(transform.name, XRCManager.OBJECTCATEGORY, lastRecordedRotation,
                                interactablePart.guid.ToString(), null, GSFC.ARVR.XRC.UnitType.meter);
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
                            XRCUnity.UpdateEntityScale(transform.name, XRCManager.OBJECTCATEGORY, lastRecordedScale,
                                interactablePart.guid.ToString(), null, GSFC.ARVR.XRC.UnitType.meter);
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
#endif
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
}