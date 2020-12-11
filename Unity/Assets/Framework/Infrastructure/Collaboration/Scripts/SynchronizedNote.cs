using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class SynchronizedNote : MonoBehaviour
{
    public SynchronizationManager synchronizationManager;
    public Object entityLock = new Object();
    public int throttleFactor = 10;
    public float translationResolution = 0.01f, rotationResolution = 10f, scaleResolution = 0.05f;
    public string objectPath;

    private Vector3 lastRecordedPosition, lastRecordedScale;
    private Quaternion lastRecordedRotation;
    private int positionThrottleCounter = 0, rotationThrottleCounter = 0, scaleThrottleCounter = 0;
    private Note note;

    public void SetTitleText(string titleToSet)
    {
        if (note)
        {
            note.titleText.text = titleToSet;
        }
    }

    public void SetInformationText(string infoToSet)
    {
        if (note)
        {
            note.informationText.text = infoToSet;
        }
    }

    public void UpdateTitleText(string newTitle)
    {
        lock (entityLock)
        {
            synchronizationManager.SendNoteTitleChange(gameObject, newTitle);
        }
    }

    public void UpdateInformationText(string newInfo)
    {
        lock (entityLock)
        {
            synchronizationManager.SendNoteInformationChange(gameObject, newInfo);
        }
    }

    void Start()
    {
        if (synchronizationManager == null)
        {
            synchronizationManager = FindObjectOfType<SynchronizationManager>();
        }

        Vector3 currPos = UnityProject.instance.GlobalToLocalPosition(transform.position);
        lastRecordedPosition = transform.position; //currPos;
        lastRecordedRotation = transform.rotation;
        lastRecordedScale = transform.localScale;

        objectPath = GetFullPath(gameObject);

        note = GetComponent<Note>();
    }

    void Update()
    {
        positionThrottleCounter++;
        rotationThrottleCounter++;
        scaleThrottleCounter++;

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
                //Vector3 currPos = UnityProject.instance.GlobalToLocalPosition(transform.position);
                if (transform.position != lastRecordedPosition && positionThrottleCounter >= throttleFactor)
                //if (currPos != lastRecordedPosition && positionThrottleCounter >= throttleFactor)
                {
                    if ((transform.position - lastRecordedPosition).magnitude > translationResolution)
                    //if ((currPos - lastRecordedPosition).magnitude > translationResolution)
                    {
                        lastRecordedPosition = transform.position; //currPos;
                        synchronizationManager.SendPositionChange(gameObject);
                    }
                    positionThrottleCounter = 0;
                }

                // Rotation.
                if (transform.rotation != lastRecordedRotation && rotationThrottleCounter >= throttleFactor)
                {
                    if ((transform.rotation.eulerAngles - lastRecordedRotation.eulerAngles).magnitude > rotationResolution)
                    {
                        lastRecordedRotation = transform.rotation;
                        synchronizationManager.SendRotationChange(gameObject);
                    }
                    rotationThrottleCounter = 0;
                }

                // Scale.
                if (transform.localScale != lastRecordedScale && scaleThrottleCounter >= throttleFactor)
                {
                    if ((transform.localScale - lastRecordedScale).magnitude > scaleResolution)
                    {
                        lastRecordedScale = transform.localScale;
                        synchronizationManager.SendScaleChange(gameObject);
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