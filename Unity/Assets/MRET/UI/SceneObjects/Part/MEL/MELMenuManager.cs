// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part.MEL;
using GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Part.MEL
{
    public class MELMenuManager : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MELMenuManager);

        public ScrollListManager partListDisplay;
        public Text fileNameText, melDisplayText;

        private int currentSelection = -1;

        void OnEnable()
        {
            FindMELItems();
        }

        public void FindMELItems()
        {
            partListDisplay.ClearScrollList();
            partListDisplay.SetTitle("Part/Assembly");

            UnityEngine.Events.UnityEvent firstClickEvent = new UnityEngine.Events.UnityEvent();
            firstClickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(-1); }));
            partListDisplay.AddScrollListItem("All Assemblies", firstClickEvent);
            for (int i = 0; i < MRET.UuidRegistry.Parts.Length; i++)
            {
                int indexToSelect = i;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                partListDisplay.AddScrollListItem(MRET.UuidRegistry.Parts[i].name, clickEvent);
            }
        }

        public void Generate()
        {
            MasterEquipmentList mel = GetComponent<MasterEquipmentList>();
            if (mel != null)
            {
                if (currentSelection <= -1)
                {
                    foreach (InteractablePart part in MRET.UuidRegistry.Parts)
                    {
                        mel.AddAssembly(part.gameObject);
                    }
                }
                else
                {
                    if (MRET.UuidRegistry.Parts[currentSelection])
                    {
                        mel.AddAssembly(MRET.UuidRegistry.Parts[currentSelection].gameObject);
                    }
                }
                melDisplayText.text = mel.ToFormattedString();
            }
            else
            {
                LogWarning("MasterEquipmentList component not found", nameof(Generate));
            }
        }

        public void Save()
        {
            MasterEquipmentList mel = GetComponent<MasterEquipmentList>();
            if (mel != null)
            {
                string destinationPath = "NewMEL.csv";
                if (!string.IsNullOrEmpty(fileNameText.text))
                {
                    destinationPath = fileNameText.text;
                }

                if (currentSelection <= -1)
                {
                    foreach (InteractablePart part in MRET.UuidRegistry.Parts)
                    {
                        mel.AddAssembly(part.gameObject);
                    }
                    mel.ToFile(destinationPath);
                }
                else
                {
                    if (MRET.UuidRegistry.Parts[currentSelection])
                    {
                        mel.AddAssembly(MRET.UuidRegistry.Parts[currentSelection].gameObject);
                    }
                    mel.ToFile(destinationPath);
                }
            }
            else
            {
                LogWarning("MasterEquipmentList component not found", nameof(Generate));
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
}