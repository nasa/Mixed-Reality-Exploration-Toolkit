// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.HierarchyPanelController) + " class")]
    public class HierarchyPanelControllerDeprecated : MonoBehaviour
    {
        public GameObject selectedObject;
        public Text titleText, parentText;
        public ScrollListManager childScrollList;

        public void Initialize()
        {
            InteractablePartDeprecated parentPart = selectedObject.transform.parent.GetComponentInParent<InteractablePartDeprecated>();
            parentText.text = (parentPart == null) ? "None" : parentPart.name;

            childScrollList.ClearScrollList();
            childScrollList.SetTitle("Children");
            foreach (InteractablePartDeprecated childPart in selectedObject.GetComponentsInChildren<InteractablePartDeprecated>())
            {
                if (childPart.gameObject != selectedObject && childPart.transform.parent.gameObject == selectedObject)
                {
                    childScrollList.AddScrollListItem(childPart.name);
                }
            }
        }

        public void SetTitle(string titleToSet)
        {
            if (titleText != null)
            {
                // Limit title to 15 characters.
                titleText.text = titleToSet.Substring(0, System.Math.Min(titleToSet.Length, 15));
                if (titleToSet.Length > 15)
                {
                    titleText.text = titleText.text + "...";
                }
            }
        }
    }
}