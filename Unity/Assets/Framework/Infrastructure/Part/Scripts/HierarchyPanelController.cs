// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

public class HierarchyPanelController : MonoBehaviour
{
    public GameObject selectedObject;
    public Text titleText, parentText;
    public ScrollListManager childScrollList;

    public void Initialize()
    {
        InteractablePart parentPart = selectedObject.transform.parent.GetComponentInParent<InteractablePart>();
        parentText.text = (parentPart == null) ? "None" : parentPart.name;

        childScrollList.ClearScrollList();
        childScrollList.SetTitle("Children");
        foreach (InteractablePart childPart in selectedObject.GetComponentsInChildren<InteractablePart>())
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