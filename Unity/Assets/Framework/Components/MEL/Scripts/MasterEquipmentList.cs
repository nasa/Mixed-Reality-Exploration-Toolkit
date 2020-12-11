using System.Collections.Generic;
using UnityEngine;

public class MasterEquipmentList : MonoBehaviour
{
    private class MasterEquipmentItem
    {
        public string itemName;
        public float unitMass;
        public int flightUnits;
        public float massContingency;
        public float unitPower;
        public float powerContingency;
        public string description;
        public string other;
        public Vector3 dimensions;
    }

    private List<MasterEquipmentItem> masterEquipmentList = new List<MasterEquipmentItem>();

    public void AddAssembly(GameObject assemblyToAdd)
    {
        foreach (InteractablePart iPart in assemblyToAdd.GetComponentsInChildren<InteractablePart>())
        {
            if (iPart.assetBundle != "NULL")
            {
                Add(iPart);
            }
        }
    }

    private void DeleteAllItems()
    {
        masterEquipmentList = new List<MasterEquipmentItem>();
    }

    public void Add(InteractablePart iPartToAdd)
    {
        // Check if part already exists in MEL.
        foreach (MasterEquipmentItem item in masterEquipmentList)
        {
            if (item.itemName == iPartToAdd.partName)
            {
                item.flightUnits++;
                return;
            }
        }

        MasterEquipmentItem melItem = new MasterEquipmentItem();
        melItem.itemName = iPartToAdd.partName;
        melItem.unitMass = iPartToAdd.minMass;
        melItem.flightUnits = 1;
        melItem.massContingency = iPartToAdd.massContingency;
        melItem.unitPower = iPartToAdd.peakPower;
        melItem.powerContingency = iPartToAdd.powerContingency;
        melItem.description = iPartToAdd.description;
        melItem.other = iPartToAdd.notes;
        melItem.dimensions = iPartToAdd.dimensions;
        masterEquipmentList.Add(melItem);
    }

    public string ToFormattedString()
    {
        string returnString = "";

        foreach (MasterEquipmentItem item in masterEquipmentList)
        {
            float totalMass = item.flightUnits * item.unitMass;
            float totalPower = item.flightUnits * item.unitPower;

            returnString = returnString + "Name: " + item.itemName + "\n"
                + "  Unit Mass (CBE): " + item.unitMass + "\n"
                + "  Flight Units: " + item.flightUnits + "\n"
                + "  Flight Spares: 0\n"
                + "  Total Mass (kg CBE): " + totalMass + "\n"
                + "  Mass Contingency %: " + item.massContingency + "\n"
                + "  Total Mass w/ Contingency: " + totalMass * (1f + item.massContingency) + "\n"
                + "  Unit Power (W CBE): " + item.unitPower + "\n"
                + "  Total Power (W CBE): " + totalPower + "\n"
                + "  Power Contingency %: " + item.powerContingency + "\n"
                + "  Total Power w/ Contingency: " + totalPower * (1f + item.powerContingency) + "\n"
                + "  Description: " + item.description + "\n"
                + "  Other Characteristics/Issues: " + item.dimensions.x + "w x " + item.dimensions.y + "h x " + item.dimensions.z + "d (mm)\n"
                + "\n\n";
        }

        return returnString;
    }

    public string ToCSV()
    {
        string returnString = "Part Name,\"Unit Mass, Current Best Estimate(CBE)\"," +
            "Flight Units,Flight Spares,\"Total Mass, kg CBE\",Mass Contingency %" +
            ",\"Total Mass w / Contingency\",\"Unit Power, W CBE\",\"Total Power, W CBE\",Power Contingency %," +
            "\"Total Power w / Contingency\",Description,Other characteristics/issues\n";

        foreach (MasterEquipmentItem item in masterEquipmentList)
        {
            // TODO: Need to fix the power formulas.
            float totalMass = item.flightUnits * item.unitMass;
            float totalPower = item.flightUnits * item.unitPower;
            returnString = returnString + "\"" + item.itemName + "\"," + item.unitMass + "," + item.flightUnits
                + ",0," + totalMass + "," + item.massContingency + "," + totalMass * (1f + item.massContingency)
                + "," + item.unitPower + "," + totalPower + "," + item.powerContingency + "," + totalPower * (1f + item.powerContingency)
                + ",\"" + item.description + "\",\"" + item.dimensions.x + "w x " + item.dimensions.y
                + "h x " + item.dimensions.z + "d (mm)\"\n";
        }

        return returnString;
    }

    public void ToFile(string filePath)
    {
        System.IO.File.WriteAllText(filePath, ToCSV());
    }
}