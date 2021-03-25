// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;

public class ChangeParentPanelController : MonoBehaviour
{
    public GameObject selectedObject;
    public ScrollListManager availableParentList;

    private int currentSelection = -1;
    private List<InteractablePart> availableParts;

    public void Initialize()
    {
        int i = 0;
        availableParts = new List<InteractablePart>();
        availableParentList.ClearScrollList();
        availableParentList.SetTitle("Available Parents");
        foreach (InteractablePart availablePart in FindObjectsOfType<InteractablePart>())
        {
            // Check if not the object in question, if not already the parent, and if not a child of the object.
            if (!availablePart.transform.IsChildOf(selectedObject.transform)
                && availablePart.gameObject != selectedObject.transform.parent.gameObject)
            {
                availableParts.Add(availablePart);
                int indexToSelect = i++;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(()=> { SetActiveSelection(indexToSelect); }));
                availableParentList.AddScrollListItem(availablePart.name, clickEvent);
            }
        }
    }

    public void Set()
    {
        if (currentSelection > -1)
        {
            selectedObject.transform.SetParent(availableParts[currentSelection].transform);
        }
    }

    private void SetActiveSelection(int listID)
    {
        currentSelection = listID;
        availableParentList.HighlightItem(listID);
    }
}