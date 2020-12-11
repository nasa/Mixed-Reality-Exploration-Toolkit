using UnityEngine;

public class RemoteControlMenuManager : MonoBehaviour
{
    public ScrollListManager objectScrollList;

    private DualAxisRotatableObject[] availableObjects = { null };
    private int currentSelection = -1;

    void Start()
    {
        objectScrollList.SetTitle("Object to Control");
    }
	
	void OnEnable()
    {
        SetObjectMenuOptions();
    }

    public void SelectObject()
    {
        if (currentSelection != -1)
        {
            foreach (ControllerMenuManager menu in FindObjectsOfType<ControllerMenuManager>())
            {
                if (menu.IsDimmed())
                {
                    DualAxisRotationControl dar = menu.GetComponentInParent<DualAxisRotationControl>();
                    if (dar)
                    {
                        if (currentSelection == 0)
                        {
                            dar.SelectRotatingObject(null);
                        }
                        else
                        {
                            dar.SelectRotatingObject(availableObjects[currentSelection - 1]);
                        }
                        break;
                    }
                }
            }
        }
    }

    private void SetObjectMenuOptions()
    {
        objectScrollList.ClearScrollList();
        UnityEngine.Events.UnityEvent firstClickEvent = new UnityEngine.Events.UnityEvent();
        firstClickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(0); }));
        objectScrollList.AddScrollListItem("None", firstClickEvent);

        DualAxisRotatableObject[] darObjs = GetDualAxisRotatableObjects();
        for (int i = 0; i < darObjs.Length; i++)
        {
            int indexToSelect = i + 1;
            UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
            clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
            objectScrollList.AddScrollListItem(darObjs[i].objectName, clickEvent);
        }
        availableObjects = darObjs;
    }

    private void SetActiveSelection(int listID)
    {
        currentSelection = listID;
        objectScrollList.HighlightItem(listID);
    }

#region Helpers
    private DualAxisRotatableObject[] GetDualAxisRotatableObjects()
    {
        return FindObjectsOfType<DualAxisRotatableObject>();
    }
#endregion
}