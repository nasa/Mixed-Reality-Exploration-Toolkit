// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.XRUI.WorldSpaceMenu;

public class MELMenuManager : MonoBehaviour
{
    public ScrollListManager partListDisplay;
    public Text fileNameText, melDisplayText;

    private List<InteractablePart> interactableParts = new List<InteractablePart>();
    private int currentSelection = -1;

	void OnEnable()
    {
        FindMELItems();
	}

    public void FindMELItems()
    {
        partListDisplay.ClearScrollList();
        partListDisplay.SetTitle("Part/Assembly");

        UnityProject project = FindObjectOfType<UnityProject>();
        if (project)
        {
            interactableParts = new List<InteractablePart>();
            foreach (Transform part in project.projectObjectContainer.transform)
            {
                if (part.GetComponent<InteractablePart>())
                {
                    interactableParts.Add(part.GetComponent<InteractablePart>());
                }
            }

            UnityEngine.Events.UnityEvent firstClickEvent = new UnityEngine.Events.UnityEvent();
            firstClickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(-1); }));
            partListDisplay.AddScrollListItem("All Assemblies", firstClickEvent);
            for (int i = 0; i < interactableParts.Count; i++)
            {
                int indexToSelect = i;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                partListDisplay.AddScrollListItem(interactableParts[i].partName, clickEvent);
            }
        }
    }

    public void Generate()
    {
        MasterEquipmentList mel = new MasterEquipmentList();

        if (currentSelection <= -1)
        {
            foreach (InteractablePart part in interactableParts)
            {
                mel.AddAssembly(part.gameObject);
            }
        }
        else
        {
            if (interactableParts[currentSelection])
            {
                mel.AddAssembly(interactableParts[currentSelection].gameObject);
            }
        }
        melDisplayText.text = mel.ToFormattedString();
    }

    public void Save()
    {
        MasterEquipmentList mel = new MasterEquipmentList();
        string destinationPath = "NewMEL.csv";
        if (fileNameText.text != "")
        {
            destinationPath = fileNameText.text;
        }

        if (currentSelection <= -1)
        {
            foreach (InteractablePart part in interactableParts)
            {
                mel.AddAssembly(part.gameObject);
            }
            mel.ToFile(destinationPath);
        }
        else
        {
            if (interactableParts[currentSelection])
            {
                mel.AddAssembly(interactableParts[currentSelection].gameObject);
            }
            mel.ToFile(destinationPath);
        }

        WorldSpaceMenuManager menuMan = GetComponent<WorldSpaceMenuManager>();
        if (menuMan)
        {
            menuMan.DimMenu();
        }
    }

    private void SetActiveSelection(int listID)
    {
        if (listID == -1)
        {
            currentSelection = -1;
            partListDisplay.HighlightItem(0);
        }
        else
        {
            currentSelection = listID;
            partListDisplay.HighlightItem(listID + 1);
        }
    }
}