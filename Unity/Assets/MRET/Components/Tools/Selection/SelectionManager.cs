// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Selection
{
    public class SelectionManager : MRETManager<SelectionManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SelectionManager);

        private List<ISelectable> selectedObjects = new List<ISelectable>();

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            selectedObjects = new List<ISelectable>();
        }

        public void AddToSelection(ISelectable selectedObject)
        {
            if (selectedObject != null)
            {
                if (!selectedObjects.Contains(selectedObject))
                {
                    selectedObjects.Add(selectedObject);
                    selectedObject.Select();
                }
            }
        }

        public void RemoveFromSelection(ISelectable selectedObject)
        {
            if (selectedObject != null)
            {
                if (selectedObjects.Remove(selectedObject))
                {
                    selectedObject.Deselect();
                }
            }
        }

        public void ClearSelection()
        {
            foreach (ISelectable selectedObject in selectedObjects)
            {
                selectedObject.Deselect();
            }
            selectedObjects = new List<ISelectable>();
        }
    }
}