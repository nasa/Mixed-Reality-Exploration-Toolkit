// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

public class ObjectPanelsMenuController : MonoBehaviour
{
    public GameObject objectPanel, descriptionPanel, hierarchyPanel, changeParentPanel,
        addChildPanel, removeChildPanel, dataDisplayListPanel;
    public GameObject selectedObject;

    private ObjectPanelController objectPanelController;
    private DescriptionPanelController descriptionPanelController;
    private HierarchyPanelController hierarchyPanelController;
    private ChangeParentPanelController changeParentPanelController;
    private AddChildPanelController addChildPanelController;
    private RemoveChildPanelController removeChildPanelController;
    private DataDisplayListPanelController dataDisplayListPanelController;
    private string objectTitle;

    public void Initialize()
    {
        SetSelectedObject();
        objectPanelController.Initialize();
        OpenMainPanel();
    }

    public void Close()
    {
        if (selectedObject == null)
        {
            Destroy(gameObject);
        }

        InteractablePart iPart = selectedObject.GetComponent<InteractablePart>();
        if (iPart)
        {
            Destroy(gameObject);
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
    }

    private void SetSelectedObject()
    {
        if (selectedObject)
        {
            objectPanelController = objectPanel.GetComponent<ObjectPanelController>();
            objectPanelController.selectedObject = selectedObject;
            descriptionPanelController = descriptionPanel.GetComponent<DescriptionPanelController>();
            descriptionPanelController.selectedObject = selectedObject;
            hierarchyPanelController = hierarchyPanel.GetComponent<HierarchyPanelController>();
            hierarchyPanelController.selectedObject = selectedObject;
            changeParentPanelController = changeParentPanel.GetComponent<ChangeParentPanelController>();
            changeParentPanelController.selectedObject = selectedObject;
            addChildPanelController = addChildPanel.GetComponent<AddChildPanelController>();
            addChildPanelController.selectedObject = selectedObject;
            removeChildPanelController = removeChildPanel.GetComponent<RemoveChildPanelController>();
            removeChildPanelController.selectedObject = selectedObject;
            dataDisplayListPanelController = dataDisplayListPanel.GetComponent<DataDisplayListPanelController>();
        }
    }

    // TODO: Cleaner solution.
    private Vector3 relativePosition = Vector3.zero;
    private void Update()
    {
        if (selectedObject != null)
        {
            if (selectedObject.transform.hasChanged)
            {
                Vector3 newPos = selectedObject.transform.TransformPoint(relativePosition);
                transform.position = newPos;
            }
            else
            {
                relativePosition = selectedObject.transform.InverseTransformPoint(transform.position);
            }
        }
    }
}