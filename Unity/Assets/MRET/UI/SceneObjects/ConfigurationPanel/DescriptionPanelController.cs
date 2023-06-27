// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    public class DescriptionPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DescriptionPanelController);

        public ISceneObject SelectedObject { get; private set; }

        public Text titleText;
        public Text descriptionText;

        public void Initialize(ISceneObject selectedObject, string panelTitle = null)
        {
            if (selectedObject == null)
            {
                LogWarning("Supplied selected object is null", nameof(Initialize));
                return;
            }

            // Assign the reference
            SelectedObject = selectedObject;

            if (descriptionText != null)
            {
                descriptionText.text = SelectedObject.description;
            }

            // Set the title
            SetTitle(panelTitle ?? SelectedObject.name);
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