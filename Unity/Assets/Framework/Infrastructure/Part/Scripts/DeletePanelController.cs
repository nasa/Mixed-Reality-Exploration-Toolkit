// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject
{
    /// <remarks>
    /// History:
    /// 9 September 2021: Created
    /// </remarks>
    /// <summary>
    /// DeletePanelController is a class that handles the
    /// deletion of sceneobjects from a GUI in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class DeletePanelController : MRETBehaviour
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
                return nameof(DeletePanelController);
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
                Debug.LogWarning("[DeletePanelController] Selected object not set.");
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
                Debug.LogWarning("[DeletePanelController->Delete] Selected object is not set.");
                return;
            }

            Destroy(selectedObject);
        }
    }
}