// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRC;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Collaboration.ISynchronized))]
    public class SynchronizedObjectDeprecated : MonoBehaviour
    {
        public SynchronizationManagerDeprecated synchronizationManager;
        public InteractablePartDeprecated interactablePart;
        public Object entityLock = new Object();
        public int throttleFactor = 10;
        public float translationResolution = 0.01f, rotationResolution = 10f, scaleResolution = 0.05f;
        public string objectPath;
        public bool autoSynchronize = false;

        private CollaborationManagerDeprecated collaborationManager;
        private Vector3 lastRecordedPosition, lastRecordedScale;
        private Quaternion lastRecordedRotation;
        private int positionThrottleCounter = 0, rotationThrottleCounter = 0, scaleThrottleCounter = 0;

        void Start()
        {
            if (synchronizationManager == null)
            {
                synchronizationManager = FindObjectOfType<SynchronizationManagerDeprecated>();
            }

            collaborationManager = FindObjectOfType<CollaborationManagerDeprecated>();
            interactablePart = GetComponent<InteractablePartDeprecated>();

            //Vector3 currPos = UnityProject.instance.GlobalToLocalPosition(transform.position);
            lastRecordedPosition = transform.position; //currPos;
            lastRecordedRotation = transform.rotation; //transform.localRotation;
            lastRecordedScale = transform.localScale;

            objectPath = GetFullPath(gameObject);
        }

        void Update()
        {
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
                            if (collaborationManager.engineType == CollaborationManagerDeprecated.EngineType.XRC)
                            {
                                XRCUnityDeprecated.UpdateEntityPosition(transform.name, XRCManager.OBJECTCATEGORY, lastRecordedPosition,
                                        interactablePart.guid.ToString(), null, GOV.NASA.GSFC.XR.XRC.UnitType.meter);
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
                            if (collaborationManager.engineType == CollaborationManagerDeprecated.EngineType.XRC)
                            {
                                XRCUnityDeprecated.UpdateEntityRotation(transform.name, XRCManager.OBJECTCATEGORY, lastRecordedRotation,
                                    interactablePart.guid.ToString(), null, GOV.NASA.GSFC.XR.XRC.UnitType.meter);
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
                            if (collaborationManager.engineType == CollaborationManagerDeprecated.EngineType.XRC)
                            {
                                XRCUnityDeprecated.UpdateEntityScale(transform.name, XRCManager.OBJECTCATEGORY, lastRecordedScale,
                                    interactablePart.guid.ToString(), null, GOV.NASA.GSFC.XR.XRC.UnitType.meter);
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
}