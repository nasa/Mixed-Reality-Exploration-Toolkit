// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    public class ChangeParentPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ChangeParentPanelController);

        public ScrollListManager availableParentList;

        public ISceneObject SelectedObject { get; private set; }

        private int currentSelection = -1;
        private List<GameObject> availableObjects;

        public void Initialize(ISceneObject selectedObject)
        {
            if (selectedObject == null)
            {
                LogWarning("Supplied selected object is null", nameof(Initialize));
                return;
            }

            // Assign the reference
            SelectedObject = selectedObject;

            availableObjects = new List<GameObject>();
            availableParentList.ClearScrollList();
            availableParentList.SetTitle("Available Parents");

            // Obtain the current root container for the selected object. We will only allow parent
            // changes within this container.
            GameObject rootContainer = ProjectManager.Project.Content.GetRootContainer(SelectedObject.gameObject);
            if (rootContainer != null)
            {
                // Add the root container
                AddAvailableParent(rootContainer);

                // Add all objects in the container
                foreach (ISceneObject availableObject in rootContainer.GetComponentsInChildren<ISceneObject>())
                {
                    AddAvailableParent(availableObject.gameObject);
                }
            }
            else
            {
                LogWarning("Supplied selected object is not a project object", nameof(Initialize));
            }
        }

        private void AddAvailableParent(GameObject availableParent)
        {
            ISceneObject selectedParent = SelectedObject.parent;

            // Make sure that the selected object is not already assigned as the direct child of
            // the available parent
            if (!availableParent.transform.IsChildOf(SelectedObject.transform) &&
                (selectedParent != null) && (availableParent.gameObject != selectedParent.gameObject))
            {
                availableObjects.Add(availableParent);
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(availableObjects.IndexOf(availableParent)); }));
                availableParentList.AddScrollListItem(availableParent.name, clickEvent);
            }
        }

        public void Set()
        {
            if (currentSelection > -1)
            {
                SelectedObject.transform.SetParent(availableObjects[currentSelection].transform);
            }
        }

        private void SetActiveSelection(int listID)
        {
            currentSelection = listID;
            availableParentList.HighlightItem(listID);
        }
    }
}