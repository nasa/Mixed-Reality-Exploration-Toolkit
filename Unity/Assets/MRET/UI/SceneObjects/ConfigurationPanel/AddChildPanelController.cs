// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    public class AddChildPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AddChildPanelController);

        public ISceneObject SelectedObject { get; private set; }

        public ScrollListManager availableChildList;

        private int currentSelection = -1;
        private List<ISceneObject> availableObjects;

        public void Initialize(ISceneObject selectedObject)
        {
            if (selectedObject == null)
            {
                LogWarning("Supplied selected object is null", nameof(Initialize));
                return;
            }

            // Assign the reference
            SelectedObject = selectedObject;

            availableObjects = new List<ISceneObject>();
            availableChildList.ClearScrollList();
            availableChildList.SetTitle("Available Children");

            // Obtain the current root container for the selected object. We will only allow parent
            // changes within this container.
            GameObject rootContainer = ProjectManager.Project.Content.GetRootContainer(SelectedObject.gameObject);
            if (rootContainer != null)
            {
                // Add all objects in the container
                foreach (ISceneObject availableObject in rootContainer.GetComponentsInChildren<ISceneObject>())
                {
                    AddAvailableChild(availableObject);
                }
            }
            else
            {
                LogWarning("Supplied selected object is not a project object", nameof(Initialize));
            }
        }

        private void AddAvailableChild(ISceneObject availableChild)
        {
            // Make sure that the selected object is not already a child, and not the parent
            // of the selected object
            if (!availableChild.transform.IsChildOf(SelectedObject.transform) &&
                !SelectedObject.transform.IsChildOf(availableChild.transform))
            {
                availableObjects.Add(availableChild);
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(availableObjects.IndexOf(availableChild)); }));
                availableChildList.AddScrollListItem(availableChild.name, clickEvent);
            }
        }

        public void Add()
        {
            if (currentSelection > -1)
            {
                availableObjects[currentSelection].transform.parent = SelectedObject.transform;
            }
        }

        private void SetActiveSelection(int listID)
        {
            currentSelection = listID;
            availableChildList.HighlightItem(listID);
        }
    }
}