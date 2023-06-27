// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    /// <remarks>
    /// History:
    /// 9 September 2021: Created
    /// </remarks>
    /// <summary>
    /// DeletePanelControllerDeprecated is a class that handles the
    /// deletion of sceneobjects from a GUI in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.DeletePanelController) + " class")]
    public class DeletePanelControllerDeprecated : MRETBehaviour
    {
        /// <summary>
        /// The object of interest.
        /// </summary>
        private GameObject selectedObject;

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(DeletePanelControllerDeprecated);
            }
        }

        /// <summary>
        /// Initializes the panel.
        /// </summary>
        /// <param name="selected">The object to work with for deletion.</param>
        public void Initialize(GameObject selected)
        {
            if (selected == null)
            {
                LogWarning("Selected object not set.", nameof(Initialize));
                return;
            }

            selectedObject = selected;
        }

        /// <summary>
        /// Perform deletion.
        /// </summary>
        public void Delete()
        {
            if (selectedObject == null)
            {
                LogWarning("Selected object is not set.", nameof(Delete));
                return;
            }

            Destroy(selectedObject);
        }
    }
}