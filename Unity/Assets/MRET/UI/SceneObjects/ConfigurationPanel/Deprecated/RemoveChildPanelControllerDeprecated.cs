// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.RemoveChildPanelController) + " class")]
    public class RemoveChildPanelControllerDeprecated : MonoBehaviour
    {
        public GameObject selectedObject;
        public ScrollListManager currentChildList;

        private int currentSelection = -1;
        private List<InteractablePartDeprecated> currentParts;
        private Transform rootParent;

        public void Initialize()
        {
            rootParent = FindObjectOfType<UnityProjectDeprecated>().projectObjectContainer.transform;

            int i = 0;
            currentParts = new List<InteractablePartDeprecated>();
            currentChildList.ClearScrollList();
            currentChildList.SetTitle("Children");
            foreach (InteractablePartDeprecated childPart in selectedObject.GetComponentsInChildren<InteractablePartDeprecated>())
            {
                if (childPart.gameObject != selectedObject && childPart.transform.parent.gameObject == selectedObject)
                {
                    currentParts.Add(childPart);
                    int indexToSelect = i++;
                    UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                    clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                    currentChildList.AddScrollListItem(childPart.name, clickEvent);
                }
            }
        }

        public void Remove()
        {
            currentParts[currentSelection].transform.parent = rootParent;
        }

        private void SetActiveSelection(int listID)
        {
            currentSelection = listID;
            currentChildList.HighlightItem(listID);
        }
    }
}