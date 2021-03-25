// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Infrastructure.Framework;

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
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                DualAxisRotationControl dar = hand.GetComponent<DualAxisRotationControl>();
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