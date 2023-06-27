// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.DescriptionPanelController) + " class")]
    public class DescriptionPanelControllerDeprecated : MonoBehaviour
    {
        public GameObject selectedObject;
        public Text titleText;
        public Text descriptionText;

        public void Initialize()
        {
            if (selectedObject)
            {
                InteractablePartDeprecated iPart = selectedObject.GetComponent<InteractablePartDeprecated>();
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
}