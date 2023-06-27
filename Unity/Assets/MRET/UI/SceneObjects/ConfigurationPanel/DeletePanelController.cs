// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
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
        private ISceneObject selectedObject;

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DeletePanelController);

        /// <summary>
        /// Initializes the panel.
        /// </summary>
        /// <param name="selected">The object to work with for deletion.</param>
        public void Initialize(ISceneObject selected)
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

            Destroy(selectedObject.gameObject);
        }
    }
}