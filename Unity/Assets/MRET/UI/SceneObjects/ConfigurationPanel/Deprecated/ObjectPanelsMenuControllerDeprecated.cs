// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.UI.Data;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.ObjectPanelsMenuController) + " class")]
    public class ObjectPanelsMenuControllerDeprecated : InteractableSceneObjectDeprecated
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ObjectPanelsMenuControllerDeprecated);

        public GameObject objectPanel, descriptionPanel, hierarchyPanel, changeParentPanel,
            addChildPanel, removeChildPanel, dataDisplayListPanel, deletePanel, partTelemetryPanel, explodePanel;
        public GameObject selectedObject;

        private ObjectPanelControllerDeprecated objectPanelController;
        private DescriptionPanelControllerDeprecated descriptionPanelController;
        private HierarchyPanelControllerDeprecated hierarchyPanelController;
        private ChangeParentPanelControllerDeprecated changeParentPanelController;
        private AddChildPanelControllerDeprecated addChildPanelController;
        private RemoveChildPanelControllerDeprecated removeChildPanelController;
        private DataDisplayListPanelControllerDeprecated dataDisplayListPanelController;
        private DeletePanelControllerDeprecated deletePanelController;
        private PartTelemetryPanelControllerDeprecated partTelemetryPanelController;
        private ExplodePanelControllerDeprecated explodePanelController;
        private string objectTitle;

        private bool initialized = false;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        /// <summary>
        /// Initializes this object panels controller
        /// </summary>
        public void Initialize()
        {
            SetSelectedObject();
            objectPanelController.Initialize();
            deletePanelController.Initialize(selectedObject);
            explodePanelController.Initialize(selectedObject);
            OpenMainPanel();
            initialized = true;
        }

        public void Close()
        {
            if (selectedObject == null)
            {
                Destroy(gameObject);
            }
            else
            {
                InteractablePart iPart = selectedObject.GetComponent<InteractablePart>();
                if (iPart)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void SetTitle(string titleToSet)
        {
            objectTitle = titleToSet;
        }

        public void OpenMainPanel()
        {
            objectPanel.SetActive(true);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);

            if (objectPanelController)
            {
                objectPanelController.SetTitle(objectTitle);
            }
        }

        public void OpenDescriptionPanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(true);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);

            if (descriptionPanelController)
            {
                descriptionPanelController.SetTitle(objectTitle);
                descriptionPanelController.Initialize();
            }
        }

        public void OpenHierarchyPanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(true);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);

            if (hierarchyPanelController)
            {
                hierarchyPanelController.SetTitle(objectTitle);
                hierarchyPanelController.Initialize();
            }
        }

        public void OpenChangeParentPanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(true);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);

            if (changeParentPanelController)
            {
                changeParentPanelController.Initialize();
            }
        }

        public void OpenAddChildPanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(true);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);

            if (addChildPanelController)
            {
                addChildPanelController.Initialize();
            }
        }

        public void OpenRemoveChildPanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(true);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);

            if (removeChildPanelController)
            {
                removeChildPanelController.Initialize();
            }
        }

        public void OpenDataDisplayListPanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(true);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);
        }

        public void OpenDeletePanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(true);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(false);

            if (deletePanelController)
            {
                deletePanelController.Initialize(selectedObject);
            }
        }

        public void OpenPartTelemetryPanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(true);
            explodePanel.SetActive(false);
        }

        public void OpenExplodePanel()
        {
            objectPanel.SetActive(false);
            descriptionPanel.SetActive(false);
            hierarchyPanel.SetActive(false);
            changeParentPanel.SetActive(false);
            addChildPanel.SetActive(false);
            removeChildPanel.SetActive(false);
            dataDisplayListPanel.SetActive(false);
            deletePanel.SetActive(false);
            partTelemetryPanel.SetActive(false);
            explodePanel.SetActive(true);

            if (explodePanelController)
            {
                explodePanelController.Initialize(selectedObject);
            }
        }

        public override void BeginGrab(InputHand hand)
        {
            positionFollowingSuspended = true;
            base.BeginGrab(hand);
        }

        public override void EndGrab(InputHand hand)
        {
            positionFollowingSuspended = false;
            relativePosition = selectedObject.transform.InverseTransformPoint(transform.position);
            relativePositionSet = true;
            base.EndGrab(hand);
        }

        private void SetSelectedObject()
        {
            if (selectedObject)
            {
                objectPanelController = objectPanel.GetComponent<ObjectPanelControllerDeprecated>();
                objectPanelController.selectedObject = selectedObject;
                descriptionPanelController = descriptionPanel.GetComponent<DescriptionPanelControllerDeprecated>();
                descriptionPanelController.selectedObject = selectedObject;
                hierarchyPanelController = hierarchyPanel.GetComponent<HierarchyPanelControllerDeprecated>();
                hierarchyPanelController.selectedObject = selectedObject;
                changeParentPanelController = changeParentPanel.GetComponent<ChangeParentPanelControllerDeprecated>();
                changeParentPanelController.selectedObject = selectedObject;
                addChildPanelController = addChildPanel.GetComponent<AddChildPanelControllerDeprecated>();
                addChildPanelController.selectedObject = selectedObject;
                removeChildPanelController = removeChildPanel.GetComponent<RemoveChildPanelControllerDeprecated>();
                removeChildPanelController.selectedObject = selectedObject;
                dataDisplayListPanelController = dataDisplayListPanel.GetComponent<DataDisplayListPanelControllerDeprecated>();
                deletePanelController = deletePanel.GetComponent<DeletePanelControllerDeprecated>();
                partTelemetryPanelController = partTelemetryPanel.GetComponent<PartTelemetryPanelControllerDeprecated>();
                explodePanelController = explodePanel.GetComponent<ExplodePanelControllerDeprecated>();
            }
        }

        // TODO: Cleaner solution.
        private Vector3 relativePosition = Vector3.zero;
        private bool relativePositionSet = false;
        private bool positionFollowingSuspended = false;
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (initialized && !positionFollowingSuspended && selectedObject != null)
            {
                if (relativePositionSet && selectedObject.transform.hasChanged)
                {
                    Vector3 newPos = selectedObject.transform.TransformPoint(relativePosition);
                    transform.position = newPos;
                }
                else
                {
                    relativePosition = selectedObject.transform.InverseTransformPoint(transform.position);
                    relativePositionSet = true;
                }
            }
        }
    }
}