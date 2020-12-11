using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class DataCapsuleController : VRTK_InteractableObject
{
    private enum PointState { redHigh, yellowHigh, green, yellowLow, redLow, unknown };

    public string pointKeyName;
    public Material greenMaterial, redMaterial, yellowMaterial, unknownMaterial;
    public int framesBetweenUpdates = 30;
    public Text label;

    private DataManager dataManager;
    private MeshRenderer meshRenderer;
    private PointState currentState = PointState.unknown;

	void Start ()
    {
        dataManager = FindObjectOfType<DataManager>();
        meshRenderer = GetComponent<MeshRenderer>();

        GetComponentInParent<VRTK_ControllerEvents>().TouchpadPressed += new ControllerInteractionEventHandler(HandleTouchpadPress);
	}

    int frameCounter = 0;
    protected override void Update()
    {
        base.Update();

        if (frameCounter >= framesBetweenUpdates)
        {
            if (pointKeyName != "" && dataManager)
            {
                // Get point.
                DataManager.DataValue pointValue = dataManager.FindCompletePoint(pointKeyName);

                if (pointValue != null)
                {
                    label.text = pointValue.key + ": " + pointValue.value.ToString();

                    // Handle state settings.
                    PointState pointState = DeterminePointState(pointValue);
                    switch (pointState)
                    {
                        case PointState.green:
                            if (pointState != PointState.green) ChangeToGreen();
                            break;

                        case PointState.yellowHigh:
                            if (pointState != PointState.yellowHigh) ChangeToYellow();
                            break;

                        case PointState.yellowLow:
                            if (pointState != PointState.yellowLow) ChangeToYellow();
                            break;

                        case PointState.redHigh:
                            if (pointState != PointState.redHigh) ChangeToRed();
                            break;

                        case PointState.redLow:
                            if (pointState != PointState.redLow) ChangeToRed();
                            break;

                        case PointState.unknown:
                        default:
                            if (pointState != PointState.unknown) ChangeToUnknown();
                            break;
                    }
                }
            }

            frameCounter = 0;
        }
        frameCounter++;
    }

    public void ChangeToGreen()
    {
        meshRenderer.materials = new Material[] { greenMaterial };
    }

    public void ChangeToRed()
    {
        meshRenderer.materials = new Material[] { redMaterial };
    }

    public void ChangeToYellow()
    {
        meshRenderer.materials = new Material[] { yellowMaterial };
    }

    public void ChangeToUnknown()
    {
        meshRenderer.materials = new Material[] { unknownMaterial };
    }

    public InteractablePart currentlyTouchingPart = null;
    private void HandleTouchpadPress(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (currentlyTouchingPart)
        {
            if (pointKeyName != "")
            {
                // Prevents duplicates.
                bool duplicateFound = false;
                foreach (string dataPoint in currentlyTouchingPart.dataPoints)
                {
                    if (dataPoint == pointKeyName)
                    {
                        duplicateFound = true;
                        break;
                    }
                }

                if (!duplicateFound)
                {
                    currentlyTouchingPart.dataPoints.Add(pointKeyName);
                }
            }
            if (gameObject)
            {
                GetComponentInParent<VRTK_ControllerEvents>().UnsubscribeToButtonAliasEvent(VRTK_ControllerEvents.ButtonAlias.TouchpadPress, true, HandleTouchpadPress);
                Destroy(gameObject);
            }
        }
    }
    
    public void OnTriggerEnter(Collider collision)
    {
        InteractablePart iPart = collision.gameObject.GetComponentInParent<InteractablePart>();
        if (iPart)
        {
            currentlyTouchingPart = iPart;
        }
    }

    public void OnTriggerExit(Collider collision)
    {
        InteractablePart iPart = collision.gameObject.GetComponent<InteractablePart>();
        if (iPart)
        {
            if (iPart == currentlyTouchingPart)
            {
                currentlyTouchingPart = null;
            }
        }
    }

#region Helpers
    private PointState DeterminePointState(DataManager.DataValue pointValue)
    {
        if (pointValue != null)
        {
            if (pointValue.redHigh != null && pointValue.yellowHigh != null
                        && pointValue.yellowLow != null && pointValue.redLow != null)
            {
                if ((double)pointValue.value >= (double)pointValue.redHigh)
                {
                    return PointState.redHigh;
                }
                else if ((double)pointValue.value >= (double)pointValue.yellowHigh)
                {
                    return PointState.yellowHigh;
                }
                else if ((double)pointValue.value >= (double)pointValue.yellowLow)
                {
                    return PointState.green;
                }
                else if ((double)pointValue.value >= (double)pointValue.redLow)
                {
                    return PointState.yellowLow;
                }
                else
                {
                    return PointState.redLow;
                }
            }
        }
        return PointState.unknown;
    }
#endregion
}