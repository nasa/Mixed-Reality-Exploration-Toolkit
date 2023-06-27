// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    public class HierarchyPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(HierarchyPanelController);

        public ISceneObject SelectedObject { get; private set; }

        public Text titleText, parentText;
        public ScrollListManager childScrollList;

        public void Initialize(ISceneObject selectedObject, string panelTitle = null)
        {
            if (selectedObject == null)
            {
                LogWarning("Supplied selected object is null", nameof(Initialize));
                return;
            }

            // Assign the reference
            SelectedObject = selectedObject;

            ISceneObject parentSceneObject = SelectedObject.parent;
            parentText.text = (parentSceneObject == null) ? "None" : parentSceneObject.name;

            childScrollList.ClearScrollList();
            childScrollList.SetTitle("Children");
            foreach (ISceneObject childSceneObject in SelectedObject.gameObject.GetComponentsInChildren<ISceneObject>())
            {
                if ((childSceneObject != SelectedObject) && (childSceneObject.parent == SelectedObject))
                {
                    childScrollList.AddScrollListItem(childSceneObject.name);
                }
            }

            // Set the title
            SetTitle(panelTitle ?? SelectedObject.name);
        }

        public void SetTitle(string titleToSet)
        {
            if (titleText != null)
            {
                // Limit title to 15 characters.
                titleText.text = titleToSet.Substring(0, System.Math.Min(titleToSet.Length, 15));
                if (titleToSet.Length > 15)
                {
                    titleText.text = titleText.text + "...";
                }
            }
        }
    }
}