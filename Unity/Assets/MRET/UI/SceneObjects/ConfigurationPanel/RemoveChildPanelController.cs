// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    public class RemoveChildPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(RemoveChildPanelController);

        public ISceneObject SelectedObject { get; private set; }

        public ScrollListManager currentChildList;

        private int currentSelection = -1;
        private List<ISceneObject> availableChildren;
        private GameObject rootContainer;

        public void Initialize(ISceneObject selectedObject)
        {
            if (selectedObject == null)
            {
                LogWarning("Supplied selected object is null", nameof(Initialize));
                return;
            }

            // Assign the reference
            SelectedObject = selectedObject;

            availableChildren = new List<ISceneObject>();
            currentChildList.ClearScrollList();
            currentChildList.SetTitle("Children");

            // Obtain the current root container for the selected object
            rootContainer = ProjectManager.Project.Content.GetRootContainer(SelectedObject.gameObject);
            if (rootContainer != null)
            {
                // Add all the children of the selected object
                foreach (ISceneObject childSceneObject in selectedObject.gameObject.GetComponentsInChildren<ISceneObject>())
                {
                    AddAvailableChild(childSceneObject);
                }
            }
            else
            {
                LogWarning("Supplied selected object is not a project object", nameof(Initialize));
            }
        }

        private void AddAvailableChild(ISceneObject availableChild)
        {
            // Make sure that the available child is not the selected object, and that the
            // selected object is the parent of the child
            if ((availableChild != SelectedObject) &&
                (availableChild.parent == SelectedObject))
            {
                availableChildren.Add(availableChild);
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(availableChildren.IndexOf(availableChild)); }));
                currentChildList.AddScrollListItem(availableChild.name, clickEvent);
            }
        }

        public void Remove()
        {
            availableChildren[currentSelection].transform.parent = (rootContainer != null)
                ? rootContainer.transform
                : ProjectManager.Project.Content.SceneObjects.transform;
        }

        private void SetActiveSelection(int listID)
        {
            currentSelection = listID;
            currentChildList.HighlightItem(listID);
        }
    }
}