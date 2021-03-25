// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

public class DescriptionPanelController : MonoBehaviour
{
    public GameObject selectedObject;
    public Text titleText;
    public Text descriptionText;

	public void Initialize()
    {
		if (selectedObject)
        {
            InteractablePart iPart = selectedObject.GetComponent<InteractablePart>();
            if (iPart && descriptionText != null)
            {
                descriptionText.text = iPart.description;
            }
        }
	}

    public void SetTitle(string titleToSet)
    {
        if (titleText != null)
        {
            titleText.text = titleToSet + " Description";
        }
    }
}