using UnityEngine;
using System.Collections.Generic;

public class AddChildPanelController : MonoBehaviour
{
    public GameObject selectedObject;
    public ScrollListManager availableChildList;

    private int currentSelection = -1;
    private List<InteractablePart> availableParts;

    public void Initialize()
    {
        int i = 0;
        availableParts = new List<InteractablePart>();
        availableChildList.ClearScrollList();
        availableChildList.SetTitle("Available Children");
        foreach (InteractablePart availablePart in FindObjectsOfType<InteractablePart>())
        {
            // Check if not the object in question, if not already a child, and if not a parent of the object.
            if (!availablePart.transform.IsChildOf(selectedObject.transform)
                && !selectedObject.transform.IsChildOf(availablePart.transform))
            {
                availableParts.Add(availablePart);
                int indexToSelect = i++;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                availableChildList.AddScrollListItem(availablePart.name, clickEvent);
            }
        }
    }

    public void Add()
    {
        if (currentSelection > -1)
        {
            availableParts[currentSelection].transform.parent = selectedObject.transform;
        }
    }

    private void SetActiveSelection(int listID)
    {
        currentSelection = listID;
        availableChildList.HighlightItem(listID);
    }
}