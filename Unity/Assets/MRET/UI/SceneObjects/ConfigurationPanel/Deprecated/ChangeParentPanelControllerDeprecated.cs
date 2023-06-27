// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.ChangeParentPanelController) + " class")]
    public class ChangeParentPanelControllerDeprecated : MonoBehaviour
    {
        public GameObject selectedObject;
        public ScrollListManager availableParentList;

        private int currentSelection = -1;
        private List<InteractablePartDeprecated> availableParts;

        public void Initialize()
        {
            int i = 0;
            availableParts = new List<InteractablePartDeprecated>();
            availableParentList.ClearScrollList();
            availableParentList.SetTitle("Available Parents");
            foreach (InteractablePartDeprecated availablePart in FindObjectsOfType<InteractablePartDeprecated>())
            {
                // Check if not the object in question, if not already the parent, and if not a child of the object.
                if (!availablePart.transform.IsChildOf(selectedObject.transform)
                    && availablePart.gameObject != selectedObject.transform.parent.gameObject)
                {
                    availableParts.Add(availablePart);
                    int indexToSelect = i++;
                    UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                    clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
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
}