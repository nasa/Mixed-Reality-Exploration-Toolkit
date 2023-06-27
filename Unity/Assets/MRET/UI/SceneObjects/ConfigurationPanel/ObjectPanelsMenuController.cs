// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.UI.Data;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    /// <summary>
    /// FIXME: This should not extend from interactable scene object. This is a controller, not a scene object.
    /// The "grab" functionality should be moved to the interactable panel or some child of interactable panel.
    /// </summary>
    public class ObjectPanelsMenuController : InteractableSceneObject, IInteractableConfigurationPanelController
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ObjectPanelsMenuController);

        public GameObject objectPanel, descriptionPanel, hierarchyPanel, changeParentPanel,
            addChildPanel, removeChildPanel, dataDisplayListPanel, deletePanel, partTelemetryPanel, explodePanel;

        private ObjectPanelController objectPanelController;
        private DescriptionPanelController descriptionPanelController;
        private HierarchyPanelController hierarchyPanelController;
        private ChangeParentPanelController changeParentPanelController;
        private AddChildPanelController addChildPanelController;
        private RemoveChildPanelController removeChildPanelController;
        private DataDisplayListPanelController dataDisplayListPanelController;
        private DeletePanelController deletePanelController;
        private PartTelemetryPanelController partTelemetryPanelController;
        private ExplodePanelController explodePanelController;

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

        /// <seealso cref="IInteractableConfigurationPanelController.ConfiguringInteractable"/>
        public IInteractable ConfiguringInteractable { get; private set; }

        /// <seealso cref="IInteractableConfigurationPanelController.PanelTitle"/>
        public string PanelTitle { get; private set; }

        /// <seealso cref="IInteractableConfigurationPanelController.Initialize(IInteractable, string)"/>
        public void Initialize(IInteractable configuringInteractable, string panelTitle = null)
        {
            if (configuringInteractable == null)
            {
                LogError("Supplied interactable is null");
                return;
            }

            // Get the controller references
            objectPanelController = objectPanel.GetComponent<ObjectPanelController>();
            descriptionPanelController = descriptionPanel.GetComponent<DescriptionPanelController>();
            hierarchyPanelController = hierarchyPanel.GetComponent<HierarchyPanelController>();
            changeParentPanelController = changeParentPanel.GetComponent<ChangeParentPanelController>();
            addChildPanelController = addChildPanel.GetComponent<AddChildPanelController>();
            removeChildPanelController = removeChildPanel.GetComponent<RemoveChildPanelController>();
            dataDisplayListPanelController = dataDisplayListPanel.GetComponent<DataDisplayListPanelController>();
            deletePanelController = deletePanel.GetComponent<DeletePanelController>();
            partTelemetryPanelController = partTelemetryPanel.GetComponent<PartTelemetryPanelController>();
            explodePanelController = explodePanel.GetComponent<ExplodePanelController>();

            // Initialize the key properties
            ConfiguringInteractable = configuringInteractable;
            PanelTitle = panelTitle ?? configuringInteractable.name;

            // Open the main panel
            OpenMainPanel();

            // Mark as initialized
            initialized = true;
        }

        public void Close()
        {
            Destroy(gameObject);
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
                objectPanelController.Initialize(ConfiguringInteractable, PanelTitle);
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
                descriptionPanelController.Initialize(ConfiguringInteractable, PanelTitle);
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
                hierarchyPanelController.Initialize(ConfiguringInteractable, PanelTitle);
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
                changeParentPanelController.Initialize(ConfiguringInteractable);
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
                addChildPanelController.Initialize(ConfiguringInteractable);
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
                removeChildPanelController.Initialize(ConfiguringInteractable);
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
                deletePanelController.Initialize(ConfiguringInteractable);
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

            if ((explodePanelController != null) &&
                (ConfiguringInteractable is IPhysicalSceneObject))
            {
                explodePanelController.Initialize(ConfiguringInteractable as IPhysicalSceneObject);
            }
        }

        protected override void BeforeBeginGrab(InputHand hand)
        {
            base.BeforeBeginGrab(hand);

            positionFollowingSuspended = true;
        }

        protected override void AfterEndGrab(InputHand hand)
        {
            base.AfterEndGrab(hand);

            positionFollowingSuspended = false;
            relativePosition = ConfiguringInteractable.transform.InverseTransformPoint(transform.position);
            relativePositionSet = true;
        }

        // TODO: Cleaner solution.
        private Vector3 relativePosition = Vector3.zero;
        private bool relativePositionSet = false;
        private bool positionFollowingSuspended = false;
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (initialized && !positionFollowingSuspended && ConfiguringInteractable != null)
            {
                if (relativePositionSet && ConfiguringInteractable.transform.hasChanged)
                {
                    Vector3 newPos = ConfiguringInteractable.transform.TransformPoint(relativePosition);
                    transform.position = newPos;
                }
                else
                {
                    relativePosition = ConfiguringInteractable.transform.InverseTransformPoint(transform.position);
                    relativePositionSet = true;
                }
            }
        }
    }
}