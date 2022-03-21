// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.XRC;

public class SynchronizedController : MonoBehaviour
{
    public enum ControllerSide { Left, Right };

    public SynchronizationManager synchronizationManager;
    public SynchronizedUser synchronizedUser;
    public Object entityLock = new Object();
    public ControllerSide controllerSide = ControllerSide.Left;
    public int throttleFactor = 10;
    public float translationResolution = 0.01f, rotationResolution = 10f, scaleResolution = 0.05f;
    public SynchronizedPointer pointer;
    public System.Guid uuid;

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

        if (synchronizedUser.isControlled)
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
                                XRCUnity.UpdateEntityPosition(synchronizedUser.userAlias,
                                    controllerSide == ControllerSide.Left ? XRCManager.LCONTROLLERCATEGORY : XRCManager.RCONTROLLERCATEGORY
                                    , lastRecordedPosition, uuid.ToString(), synchronizedUser.uuid.ToString(), GSFC.ARVR.XRC.UnitType.meter);
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
                                XRCUnity.UpdateEntityRotation(synchronizedUser.userAlias,
                                    controllerSide == ControllerSide.Left ? XRCManager.LCONTROLLERCATEGORY : XRCManager.RCONTROLLERCATEGORY
                                    , lastRecordedRotation, uuid.ToString(), synchronizedUser.uuid.ToString(), GSFC.ARVR.XRC.UnitType.meter);
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
                                XRCUnity.UpdateEntityScale(synchronizedUser.userAlias,
                                    controllerSide == ControllerSide.Left ? XRCManager.LCONTROLLERCATEGORY : XRCManager.RCONTROLLERCATEGORY
                                    , lastRecordedScale, uuid.ToString(), synchronizedUser.uuid.ToString(), GSFC.ARVR.XRC.UnitType.meter);
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