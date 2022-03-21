using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System;
using unitycodercom_PointCloudBinaryViewer;

//this script is used to render certain features of a Creo STP file to a point cloud. This is used to get a better understanding of how cables are defined in Creo
//to render to a point cloud, have a separate game object in the same scene with a PointCloudViewerDX11 component, then make this script reference that component in the scene
//a lot of this code is outdated, STPReader was written after this and this has a lot of older versions of methods in STPReader
//for the feature input when this is a component on a GameObject, type in the name of a feature as it would first appear past the equals sign in the STP file
//example: for #145717=AXIS2_PLACEMENT_3D('',#145714,#145715,#145716);, to render points associated with this feature, put "AXIS2_PLACEMENT_3D" (script will ignore underscores 
//and is case-insensitive and will work with a substring of this starting from the beginning, so "axis2 placement" will work, but extra features that start with the same text will be rendered if the full feature isn't used)
public class CableDebug : MonoBehaviour
{
    private string[] lines;
    OrderedDictionary points = new OrderedDictionary();
    Dictionary<int, string> entityDict = new Dictionary<int, string>();
    public DrawLineManager drawLineManager;
    public string path;// = @"C:\Users\negan\Documents\newnewMRETclone\MRET_Core\Unity\Assets\Framework\InProgress\vr_harness_sample_asm_STEP_thickharness.stp";
    //C:\Users\negan\Documents\newnewMRETclone\MRET_Core\Unity\Assets\Framework\InProgress\vr_harness_sample_asm.stp
    OrderedDictionary featurePoints;
    public PointCloudViewerDX11 pointCloudViewer;
    public bool offsetToOrigin;
    public string feature;
    List<Vector3> colors = new List<Vector3>();
    void Start()
    {
        lines = File.ReadAllLines(path);
        entityDict = GetEntitiesDictFromSTL(path);
        featurePoints = ExtractFeaturePoints(path, entityDict, feature);
        Debug.Log(featurePoints.Count);
        InitializePointCloud();
    }

    void InitializePointCloud()
    {
        pointCloudViewer.containsRGB = true;
        pointCloudViewer.InitDX11Buffers();
        Vector3[] initData = new Vector3[1];
        initData[0] = Vector3.zero;
        UpdatePointCloud(initData);
    }

    void UpdatePointCloud(Vector3[] points, Vector3[] colors)
    {
        if (offsetToOrigin)
        {
            Vector3 firstPoint = points[0];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = points[i] - firstPoint;
            }
        }

        //data is sent to point cloud viewer
        pointCloudViewer.points = points;
        pointCloudViewer.UpdatePointData();

        pointCloudViewer.pointColors = colors;
        pointCloudViewer.UpdateColorData();
    }

    //spawns all points black if color is not specified
    void UpdatePointCloud(Vector3[] points)
    {
        if (offsetToOrigin)
        {
            Vector3 firstPoint = points[0];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = points[i] - firstPoint;
            }
        }

        //data is sent to point cloud viewer
        pointCloudViewer.points = points;
        pointCloudViewer.UpdatePointData();

        pointCloudViewer.pointColors = new Vector3[points.Length];
        pointCloudViewer.UpdateColorData();
    }

    public void SpawnCable()
    {
        drawLineManager.AddPredefinedDrawing(GetVector3ListFromOrderedDictionary(points), LineDrawing.RenderTypes.Cable, LineDrawing.unit.millimeters, "testCable", System.Guid.NewGuid());
    }
    public void SpawnPointCloud()
    {
        featurePoints = ExtractFeaturePoints(path, entityDict, feature);
        Vector3[] pointsVec3 = GetVector3ArrayFromOrderedDictionary(featurePoints);
        UpdatePointCloud(pointsVec3, colors.ToArray());
        //UpdatePointCloud(GetVector3ArrayFromOrderedDictionary(points));
    }

    //get cartesian points referenced by each feature of a certain type in an STP file
    OrderedDictionary ExtractFeaturePoints(string path, Dictionary<int, string> entities, string feature)
    {
        colors.Clear();
        var random = new System.Random();
        OrderedDictionary extractedPoints = new OrderedDictionary();

        foreach(KeyValuePair<int, string> kvp in entities)
        {
            string line = kvp.Value;
            if (line.StartsWith(feature.ToUpper().Replace(" ", "_")))
            {
                List<Vector3> points = GetCartesianPointsFromEntity(line, entities); //get cartesian points from an entity

                //this random color stuff is for point clouds
                Vector3 randomColor = new Vector3(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)) / 255;
                foreach(Vector3 point in points)
                {
                    colors.Add(randomColor);
                    extractedPoints.Add(System.Guid.NewGuid(), point);
                }
            }
        }
        return extractedPoints;
    }

    //returns a list of vector3s containing the coordinates of cartesian point features referenced by all instances of a certain entity in an STP file
    List<Vector3> GetCartesianPointsFromEntity(string line, Dictionary<int, string> entities)
    {
        List<Vector3> cartesianPoints = new List<Vector3>();
        if (line.StartsWith("CARTESIAN_POINT"))
        {
            cartesianPoints.Add(GetVector3FromCartesianPoint(line));
        }
        else if (line.Contains("#"))
        {
            //get referenced entities
            //add GetCartesianPointsFromEntity(entity) from each entity to cartesianPoints
            List<int> referencedEntities = new List<int>();

            //get referenced entities
            string[] sections = line.Split('#');
            foreach (string section in sections)
            {
                //get entity ID number, check if the entire section is a number, then keep parsing beginning of the string until the last time it finds a number
                int id = 0;
                if (int.TryParse(section, out id))
                {
                }
                else
                {
                    int index = 1;
                    while (int.TryParse(section.Substring(0, index), out int num))
                    {
                        id = num;
                        index++;
                    }
                }
                if (id != 0)
                {
                    referencedEntities.Add(id);
                }
            }
            foreach(int referencedEntity in referencedEntities)
            {
                if (entities.ContainsKey(referencedEntity))
                {
                    cartesianPoints.AddRange(GetCartesianPointsFromEntity(entities[referencedEntity], entities));
                }
            }
        }
        return cartesianPoints;
    }

    Vector3 GetVector3FromCartesianPoint(string line)
    {
        //cartesian points are of the format "#145722=CARTESIAN_POINT('',(9.252154207798E1,-1.309426581558E1,-7.551180520567E1));", and this method gets the x, y, z coords from that
        string temp = line.Replace(")", string.Empty).Replace(";", string.Empty).Replace("\n", string.Empty).Replace("(", string.Empty).Replace("#", string.Empty);
        string[] sections = temp.Split(',');
        float[] coords = new float[3];
        if (sections.Length == 4)
        {
            for (int j = 1; j < sections.Length; j++)
            {
                string section = sections[j];
                if (char.IsDigit(section[0]) || section.StartsWith("-"))
                {
                    if (float.TryParse(section, out float coord))
                    {
                        coords[j-1] = coord;
                    }
                }
            }
        }
        Vector3 point = new Vector3(coords[0], coords[1], coords[2]);
        return point;
    }

    //reads stp file, collects each feature into a Dictionary where the key is the ID number and the value/feature is the text after the equals sign (handles features spread over multiple lines)
    //Example: #145717=AXIS2_PLACEMENT_3D('',#145714,#145715,#145716);, key = 145717, value = "AXIS2_PLACEMENT_3D('',#145714,#145715,#145716);"
    Dictionary<int, string> GetEntitiesDictFromSTL(string path)
    {
        Dictionary<int, string> entities = new Dictionary<int, string>();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("#") && line.Contains("="))
            {
                string currentLine = line;
                int counter = 1;
                while (!currentLine.EndsWith(";"))
                {
                    currentLine = lines[i+counter];
                    line = string.Concat(line, currentLine);
                    counter++;
                }
                string[] splitLine = line.Split('=');
                int id = int.Parse(splitLine[0].Replace("#", String.Empty));
                string feature = splitLine[1];

                entities.Add(id, feature);
            }
        }
        return entities;
    }

    List<Vector3> GetVector3ListFromOrderedDictionary(OrderedDictionary pointsDict)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < pointsDict.Count; i++)
        {
            points.Add((Vector3)pointsDict[i]);
        }
        return points;
    }
    Vector3[] GetVector3ArrayFromOrderedDictionary(OrderedDictionary pointsDict)
    {
        int numPoints = pointsDict.Count;
        Vector3[] points = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            points[i] = (Vector3)pointsDict[i];
        }
        return points;
    }
}