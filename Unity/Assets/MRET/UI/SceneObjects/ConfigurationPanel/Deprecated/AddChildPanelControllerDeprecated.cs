// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.AddChildPanelController) + " class")]
    public class AddChildPanelControllerDeprecated : MonoBehaviour
    {
        public GameObject selectedObject;
        public ScrollListManager availableChildList;

        private int currentSelection = -1;
        private List<InteractablePartDeprecated> availableParts;

        public void Initialize()
        {
            int i = 0;
            availableParts = new List<InteractablePartDeprecated>();
            availableChildList.ClearScrollList();
            availableChildList.SetTitle("Available Children");
            foreach (InteractablePartDeprecated availablePart in FindObjectsOfType<InteractablePartDeprecated>())
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
}