// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FemapCSVHandler : MonoBehaviour
{
    [SerializeField] TextAsset csv;

    List<string> headings = new List<string>();
    int numParam;

    public class Row
    {
        public string vert;
        public string rVal;
        public string gVal;
        public string bVal;
        public string aVal;
    }

    List<Row> rowList = new List<Row>();
    bool isLoaded = false;

    void Awake()
    {
        Load();
    }

    public bool IsLoaded()
    {
        return isLoaded;
    }

    public int GetNumParam()
    {
        return numParam;
    }

    public List<string> GetHeadings()
    {
        return headings;
    }

    public List<Row> GetRowList()
    {
        return rowList;
    }

    public void Load()
    {
        rowList.Clear();
        var data = csv.text.Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        this.numParam = data[0].Length;

        foreach (string heading in data[0].Split(','))
        {
            this.headings.Add(heading);
        }

        for (int i =1; i < data.Length; i++)
        {
            var entries = data[i].Split(',');
            Row row = new Row();
            row.vert = entries[0];
            row.rVal = entries[1];
            row.gVal = entries[2];
            row.bVal = entries[3];
            row.aVal = entries[4];

            rowList.Add(row);
        }

        isLoaded = true;
    }

    public int NumRows()
    {
        return rowList.Count;
    }

    public Row GetAt(int i)
    {
        if (rowList.Count <= i)
            return null;
        return rowList[i];
    }

    public Row Find_vert(string find)
    {
        return rowList.Find(x => x.vert == find);
    }
    public List<Row> FindAll_vert(string find)
    {
        return rowList.FindAll(x => x.vert == find);
    }
    public Row Find_rVal(string find)
    {
        return rowList.Find(x => x.rVal == find);
    }
    public List<Row> FindAll_rVal(string find)
    {
        return rowList.FindAll(x => x.rVal == find);
    }
    public Row Find_gVal(string find)
    {
        return rowList.Find(x => x.gVal == find);
    }
    public List<Row> FindAll_gVal(string find)
    {
        return rowList.FindAll(x => x.gVal == find);
    }
    public Row Find_bVal(string find)
    {
        return rowList.Find(x => x.bVal == find);
    }
    public List<Row> FindAll_bVal(string find)
    {
        return rowList.FindAll(x => x.bVal == find);
    }
    public Row Find_aVal(string find)
    {
        return rowList.Find(x => x.aVal == find);
    }
    public List<Row> FindAll_aVal(string find)
    {
        return rowList.FindAll(x => x.aVal == find);
    }
}
