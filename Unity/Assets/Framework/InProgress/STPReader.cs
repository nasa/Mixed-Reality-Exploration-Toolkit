// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System;
using unitycodercom_PointCloudBinaryViewer;

//This class is a work-in-progress. Designed to load cables in from Creo STP files. 
//Cables are defined as two sets of B_SPLINE features that connect end-to-end, the two sets are separated from each other by a constant distance (which is the cable diameter)
//Very useful resource (has list of entities found in STP files): https://www.steptools.com/stds/stp_aim/html/ 
//to render to a point cloud, have a separate game object in the same scene with a PointCloudViewerDX11 component, then make this script reference that component in the scene
public class STPReader : MonoBehaviour
{
    public float debugPointsSeparation;
    public string path;
    public PointCloudViewerDX11 pointCloudViewer;
    public bool offsetToOrigin;
    float tolerance = 0.1f;
    public DrawLineManager drawLineManager;
    public float maxDistanceBetweenPoints = 0.1f;
    public bool attachSegments = false;

    void OnEnable()
    {
        //List<Point> points = GetCablePointsFromSTP(path);
        List<Point> debugPoints = new List<Point>();
        Debug.Log("everything done before getting cables");
        List<Cable> cables = GetCables(path);
        Debug.Log("got cables");
        ///*
        System.Random random = new System.Random();
        //adds points from collected cables to a single list to render to a point cloud
        foreach (var cable in cables)
        {
            List<Vector3> centerPoints = GetCenterPoints(cable);
            if (centerPoints.Count < 50 || cable.splineList1.Count < 3)//50 3
            {
                continue;
            }
            if (drawLineManager != null && centerPoints.Count > 0)
            {
                SpawnCable(centerPoints);
            }
            Vector3 randomColor = new Vector3(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)) / 255;
            List<Point> cablePoints = new List<Point>();
            for (int i = 0; i < centerPoints.Count; i++)
            {
                cablePoints.Add(new Point(centerPoints[i], randomColor));
            }
            debugPoints.AddRange(cablePoints);
        }
        List<Point> points = debugPoints;
        List<Vector3> pointPositions = GetVector3ListFromPoints(points, "positions");
        List<Vector3> pointColors = GetVector3ListFromPoints(points, "colors");
        if (points.Count > 0)
        {
            if (pointCloudViewer != null)
            {
                InitializePointCloud();
                SpawnPointCloud(pointPositions, pointColors);
            }
            if (drawLineManager != null)
            {
                //SpawnCable(pointPositions);
            }
        }
        else
        {
            Debug.LogWarning("zero points in point cloud");
        }
    }
    //spawns cable using draw line manager
    public void SpawnCable(List<Vector3> positions)
    {
        drawLineManager.AddPredefinedDrawing(positions, LineDrawing.RenderTypes.Cable, LineDrawing.unit.millimeters, "testCable", System.Guid.NewGuid());
    }

    //following 5 methods spawn a point cloud using methods from the PointCloudViewerDX11 script
    public void SpawnPointCloud(List<Vector3> pointPositions, List<Vector3> pointColors)
    {
        UpdatePointCloud(pointPositions.ToArray(), pointColors.ToArray());
    }

    public void SpawnPointCloud(List<Vector3> pointPositions)
    {
        UpdatePointCloud(pointPositions.ToArray());
    }

    void InitializePointCloud()
    {
        pointCloudViewer.containsRGB = true;
        pointCloudViewer.InitDX11Buffers();
        Vector3[] initData = new Vector3[1];
        initData[0] = Vector3.zero;
        UpdatePointCloud(initData);
    }
    //UpdatePointCloud methods set point cloud points and colors
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

        // after you have your own data, send them into viewer
        pointCloudViewer.points = points;
        pointCloudViewer.UpdatePointData();

        pointCloudViewer.pointColors = colors;
        pointCloudViewer.UpdateColorData();
    }

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

        // after you have your own data, send them into viewer
        pointCloudViewer.points = points;
        pointCloudViewer.UpdatePointData();

        pointCloudViewer.pointColors = new Vector3[points.Length];
        pointCloudViewer.UpdateColorData();
    }
    //Used for debug, gets cable points from stp file, separates different cables by color
    List<Point> GetCablePointsFromSTP(string path)
    {
        List<Point> debugPoints = new List<Point>();
        Debug.Log("everything done before getting cables");
        List<Cable> cables = GetCables(path);
        Debug.Log("got cables");
        ///*
        System.Random random = new System.Random();
        //adds points from collected cables to a single list to render to a point cloud
        foreach (var cable in cables)
        {
            List<Vector3> centerPoints = GetCenterPoints(cable);
            if (centerPoints.Count < 50 || cable.splineList1.Count < 3)//50 3
            {
                continue;
            }
            Vector3 randomColor = new Vector3(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)) / 255;
            List<Point> cablePoints = new List<Point>();
            for (int i = 0; i < centerPoints.Count; i++)
            {
                cablePoints.Add(new Point(centerPoints[i], randomColor));
            }
            debugPoints.AddRange(cablePoints);
        }
        //*/
        return debugPoints;
    }

    //struct that carries both a position and a color, used for debug with point cloud rendering
    struct Point
    {
        public Vector3 position;
        public Vector3 color;
        public Point(Vector3 position, Vector3 color)
        {
            this.position = position;
            this.color = color;
        }
    }

    //obtains a list of Vector3 elements from a list of points, used for getting points to render a point cloud
    List<Vector3> GetVector3ListFromPoints(List<Point> points, string feature)
    {
        List<Vector3> list = new List<Vector3>();
        if (feature.ToLower().Equals("positions"))
        {
            foreach (Point point in points)
            {
                list.Add(point.position);
            }
        }
        else if (feature.ToLower().Equals("colors"))
        {
            foreach (Point point in points)
            {
                list.Add(point.color);
            }
        }
        return list;
    }

    //this struct holds data for the B_Spline features, used to define cables
    struct SplineCurve
    {
        public List<Vector3> positions;
        public Vector3 startPosition;
        public Vector3 endPosition;
    }

    List<SplineCurve> GetSplineCurvesFromEntities(OrderedDictionary entities)
    {
        List<SplineCurve> splineCurveList = new List<SplineCurve>();
        for (int i = 0; i < entities.Count; i++)
        {
            string line = entities[i].ToString();
            if (line.StartsWith("B_SPLINE_CURVE"))
            {
                splineCurveList.Add(GetSplineCurveFromSplineCurveEntity(line, entities));
            }
        }
        return splineCurveList;

    }

    SplineCurve GetSplineCurveFromSplineCurveEntity(string line, OrderedDictionary entities)
    {
        SplineCurve splineCurve = new SplineCurve();
        splineCurve.positions = GetCartesianPointsFromEntity(line, entities);
        splineCurve.startPosition = splineCurve.positions[0];
        splineCurve.endPosition = splineCurve.positions[splineCurve.positions.Count - 1];
        return splineCurve;
    }

    List<Vector3> GetCartesianPointsFromEntity(string line, OrderedDictionary entities)
    {
        List<Vector3> cartesianPoints = new List<Vector3>();
        if (line.StartsWith("CARTESIAN_POINT"))
        {
            cartesianPoints.Add(GetVector3FromCartesianPointOrDirection(line));
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
                int id = 0;
                //try parsing int of full section, if that doesn't work start from the beginning and do it until the last time a number is able to be read
                //so if the section is "18231, ", it will get the id 18231
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
                if (id != 0) //if id was found
                {
                    referencedEntities.Add(id);
                }
            }
            foreach(int referencedEntity in referencedEntities)
            {
                if (entities.Contains((object)referencedEntity))
                {
                    cartesianPoints.AddRange(GetCartesianPointsFromEntity(entities[(object)referencedEntity].ToString(), entities));
                }
            }
        }
        return cartesianPoints;
    }

    Vector3 GetVector3FromCartesianPointOrDirection(string line) //old method could be improved
    {
        //example cartesian point: #144815=CARTESIAN_POINT('',(1.001485929265E2,-1.385627396220E1,-6.857835941728E1));
        //example direction: #145715=DIRECTION('',(9.959442326217E-1,8.133015108914E-2,-3.847716246616E-2));
        //splits up the string by comma and reads float from each section to get coords
        string temp = line.Replace(")", string.Empty).Replace(";", string.Empty).Replace("\n", string.Empty).Replace("(", string.Empty).Replace("#", string.Empty); //clean up string
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

    OrderedDictionary GetEntities(string path)
    {
        //reads stp file, collects each feature into an OrderedDictionary where the key is the ID number and the feature is the text after the equals sign (handles features spread over multiple lines)
        //Example: #145717=AXIS2_PLACEMENT_3D('',#145714,#145715,#145716);, key = 145717, value = "AXIS2_PLACEMENT_3D('',#145714,#145715,#145716);"
        string[] lines = File.ReadAllLines(path);
        OrderedDictionary entityDict = new OrderedDictionary();
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("#") && line.Contains("="))
            {
                string currentLine = line;
                while (!currentLine.EndsWith(";"))
                {
                    i++;
                    currentLine = lines[i];
                    line = string.Concat(line, currentLine);
                }
                string[] splitLine = line.Split('=');
                int id = int.Parse(splitLine[0].Replace("#", String.Empty));
                string feature = splitLine[1];

                entityDict.Add(id, feature);
            }
        }
        return entityDict;
    }

    struct Cable
    {
        //the two spline lists form both sides of the cable
        public List<SplineCurve> splineList1;
        public List<SplineCurve> splineList2;
        public List<Vector3> centerPoints;
        public float radius;
        public Cable(List<SplineCurve> splineList1, List<SplineCurve> splineList2)
        {
            this.splineList1 = splineList1;
            this.splineList2 = splineList2;
            this.centerPoints = new List<Vector3>();
            this.radius = Vector3.Distance(splineList1[0].startPosition, splineList2[0].startPosition) / 2f;
        }
    }

    //extracts cables from a STP file. Cables are defined using B_SPLINE features. A spline string is a set of spline features connected end-to-end (have a tolerance value for
    //splines slightly separated). Two spline strings separated by a constant distance (cable diameter) is a cable. One spline feature in one string will correspond to another 
    //spline feature in the other string (both strings will have the same number of spline features)
    List<Cable> GetCables(string path)
    {
        List<Cable> cables = new List<Cable>();
        
        //Reading through all entities right now because there is no guarantee things are defined in order, so features could reference things later in the file
        //GetEntities could be optimized by only reading features having to do with B_SPLINE features however
        OrderedDictionary entities = GetEntities(path);
        List<SplineCurve> splines = GetSplineCurvesFromEntities(entities);

        //partner spline, the spline that is diametrically opposite to the current spline on the cable (if there are multiple) is just the next 
        //nearest one in the list (sometimes cables can be right next to each other and have spline features 
        //that have the same start and end point, the actual partner spline on the cable is more likely to be next in the list rather than further down)
        int numSplines = splines.Count;
        List<int> usedSplines = new List<int>();
        for (int i = 0; i < numSplines; i++)
        {
            bool startsLine = true;
            bool hasPartner = false;
            List<SplineCurve> splines1 = new List<SplineCurve>();
            List<SplineCurve> splines2 = new List<SplineCurve>();
            float diameter = 0;
            //this loop checks if the current spline feature has another spline feature connected to its start point. If it does, it does not start a line or cable, 
            //it is a middle or end section. It also checks if the spline feature has a parter (corresponding spline on another string, meaning it's in a cable). If it 
            //starts a line and has a partner, it is the beginning of a cable, and the cable is then built from the spline and its partner
            for (int j = i + 1; j < numSplines + i + 1; j++)
            {
                int index = j % numSplines;
                if (i != j && !usedSplines.Contains(index) && !usedSplines.Contains(i))
                {
                    if (Vector3.Distance(splines[index].endPosition, splines[i].startPosition) < tolerance) //if another spline connects to its start
                    {
                        startsLine = false;
                    }
                    if (Mathf.Abs(Vector3.Distance(splines[i].startPosition, splines[index].startPosition) - Vector3.Distance(splines[i].endPosition, splines[index].endPosition)) < 0.01) //if the start and end positions are separated by the same distance (constant diameter cable)
                    {
                        hasPartner = true;
                        splines1.Add(splines[i]);
                        splines2.Add(splines[index]);
                        usedSplines.Add(i);
                        usedSplines.Add(index);
                        diameter = Vector3.Distance(splines[i].startPosition, splines[index].startPosition);
                    }
                }
            }
            if (startsLine && hasPartner)
            {
                //this while loop goes through each spline, seeing if it connects to the end splines of current list of splines that forms a cable. Once it finds the end splines,
                //it loops through again to see if splines connect to the new end splines. Once it loops through and does not find new splines to connect to the end, the cable is
                //finished and the loop ends
                bool cableHasNotEnded = true;
                int splineIndex = i; //this is just used for optimization, it starts the loop at a place where the next splines are more likely to be defined
                int counter = 0;
                while (cableHasNotEnded)
                {
                    int index1 = -2;
                    int index2 = -2;
                    bool foundNextSplines = false;
                    for (int l = splineIndex; l < splines.Count + splineIndex; l++) //right now I just take the next two splines if they're defined near each other in the stp file, but I should search the whole list and filter out splines that aren't on the same cable but might have the same start and end
                    {
                        int index = l % (splines.Count);
                        //check if spline start and end points are appropriate for the cable
                        if (splines[index].startPosition == splines1[splines1.Count - 1].endPosition && Mathf.Abs(Vector3.Distance(splines[index].startPosition, splines2[splines2.Count - 1].endPosition) - diameter) < tolerance)
                        {
                            index1 = index;
                        }
                        if (splines[index].startPosition == splines2[splines2.Count - 1].endPosition && Mathf.Abs(Vector3.Distance(splines[index].startPosition, splines1[splines1.Count - 1].endPosition) - diameter) < tolerance)
                        {
                            index2 = index;
                        }
                        
                        if (Mathf.Abs(index1 - index2) <= 4 && index1 >= 0 && index2 >= 0) //if next splines are found and are defined near each other in the stp file
                        {
                            foundNextSplines = true;
                            splineIndex += 2;
                            splines1.Add(splines[index1]);
                            splines2.Add(splines[index2]);
                            usedSplines.Add(index1);
                            usedSplines.Add(index2);
                            break;
                        }
                        
                    }
                    if (!foundNextSplines)
                    {
                        cables.Add(new Cable(splines1, splines2));
                    }
                    cableHasNotEnded = foundNextSplines;

                    //used for debug
                    counter++;
                    if (counter > 300)
                    {
                        Debug.Log("infinite while loop");
                        break;
                    }
                }
            }
        }
        if (attachSegments)
        {
            cables = AttachSegments(cables);
        }
        return cables;
    }

    //since cable segments that are part of the same cable are sometimes detached, this method attaches them together (buggy, still work in progress)
    //two cable segments are attached if drawing a straight line from the end of one cable segment tangent to the splines at the end hits the beginning of another cable segment
    List<Cable> AttachSegments(List<Cable> cables)
    {
        List<Cable> completeCables = new List<Cable>();
        for (int i = 0; i < cables.Count; i++)
        {
            bool beginsCable = true;
            //checks if the current cable is "hit" by any lines coming from other cables. If it is, it does not start a cable. If it does start a cable, the rest of the cable is built from that segment
            for (int j = 0; j < cables.Count; j++)
            {
                List<Vector3> lastSplinePositions1 = cables[j].splineList1[cables[j].splineList1.Count - 1].positions;
                List<Vector3> lastSplinePositions2 = cables[j].splineList2[cables[j].splineList2.Count - 1].positions;
                Vector3 direction = lastSplinePositions1[lastSplinePositions1.Count - 1] - lastSplinePositions1[lastSplinePositions1.Count - 2];

                Vector3 beginSpline1 = cables[i].splineList1[0].positions[0];
                Vector3 beginSpline2 = cables[i].splineList2[0].positions[0];
                if (DistanceFromLine(direction, (lastSplinePositions1[lastSplinePositions1.Count - 1] + lastSplinePositions2[lastSplinePositions2.Count - 1]) / 2, (beginSpline1 + beginSpline2) / 2) < tolerance && j != i) //TODO: make this into helper function
                {
                    beginsCable = false;
                }
            }
            if (beginsCable)
            {
                //this while loop goes through all the cable segments and checks if the end of the current cable hits the beginning of another cable, if it does, the other cable
                //is added to the end of the current cable and the loop is run again. When no cable is hit, the cable is completed
                bool cableHasNotEnded = true;
                List<Cable> cableList = new List<Cable>();
                cableList.Add(cables[i]);
                bool foundNextCable = false;
                int counter = 0;
                while (cableHasNotEnded)
                {
                    foundNextCable = false;
                    for (int j = 0; j < cables.Count; j++)
                    {
                        List<Vector3> lastSplinePositions1 = cableList[cableList.Count - 1].splineList1[cableList[cableList.Count - 1].splineList1.Count - 1].positions;
                        List<Vector3> lastSplinePositions2 = cableList[cableList.Count - 1].splineList2[cableList[cableList.Count - 1].splineList2.Count - 1].positions;
                        Vector3 direction = lastSplinePositions1[lastSplinePositions1.Count - 1] - lastSplinePositions1[lastSplinePositions1.Count - 2];
                        Vector3 beginSpline1 = cables[j].splineList1[0].positions[0];
                        Vector3 beginSpline2 = cables[j].splineList2[0].positions[0];
                        float distanceFromline = DistanceFromLine(direction, (lastSplinePositions2[lastSplinePositions2.Count - 1] + lastSplinePositions1[lastSplinePositions1.Count - 1]) / 2, (beginSpline1 + beginSpline2) / 2);
                        if (distanceFromline < tolerance && j != i) //need condition to check that the point is in the forward direction, otherwise cables with same lines will repeat
                        {
                            foundNextCable = true;
                            cableList.Add(cables[j]);
                        }
                    }
                    if (!foundNextCable)
                    {
                        break;
                    }
                    //this stuff is used when debugging to prevent infinite running while loop
                    counter++;
                    if (counter > 300)
                    {
                        Debug.Log("infinite while loop");
                        break;
                    }
                }
                Cable combinedCable = cables[i];
                for (int j = 1; j < cableList.Count; j++)
                {
                    combinedCable.splineList1.AddRange(cableList[j].splineList1);
                    combinedCable.splineList2.AddRange(cableList[j].splineList2);
                }
                completeCables.Add(combinedCable);
            }
        }
        return completeCables;
    }

    float DistanceFromLine(Vector3 line, Vector3 pointOnLine, Vector3 point)
    {
        return (Vector3.Cross(line, point - pointOnLine).magnitude / line.magnitude);
    }

    //gets the center points of the two spline lists of the cable. The points that actually make up the cable when it is spawned in MRET
    List<Vector3> GetCenterPoints(Cable cable)
    {
        List<Vector3> midpoints = new List<Vector3>();
        if (cable.splineList1.Count != cable.splineList2.Count)
        {
            Debug.LogError("improperly constructed cable");
            return null;
        }
        for (int i = 0; i < cable.splineList1.Count; i++)
        {
            int length = Mathf.Min(cable.splineList1[i].positions.Count, cable.splineList2[i].positions.Count);
            for (int j = 0; j < length; j++)
            {
                int index = j % length;
                Vector3 newPoint = (cable.splineList1[i].positions[index] + cable.splineList2[i].positions[index]) / 2;
                
                if (midpoints.Count > 0)
                {
                    Vector3 lastPoint = midpoints[midpoints.Count - 1];
                    float distance = Vector3.Distance(lastPoint, newPoint);
                    if (distance > maxDistanceBetweenPoints) //if the distance between points is too large, add points in a straight line between the two points in question
                    {
                        int numPointsToAdd = Mathf.CeilToInt(distance / maxDistanceBetweenPoints) - 1;
                        Vector3 connectingLine = newPoint - lastPoint;
                        for (int k = 1; k <= numPointsToAdd; k++)
                        {
                            midpoints.Add(lastPoint + connectingLine.normalized * k * maxDistanceBetweenPoints);
                        }
                    }
                }
                
                midpoints.Add(newPoint);
            }
        }
        return midpoints;
    }
}

//Used to use circle features combined with spline features to define cables but this does not work for every STP file, just using splines is more general
//Kept code in case it would be useful in the future

/*
    struct Circle
    {
        public Vector3 center;
        public Vector3 direction;
        public Vector3 referenceDirection;
        public float radius;
        public Vector3 referencePoint1;
        public Vector3 referencePoint2;
        public Circle(Vector3 center, Vector3 direction, Vector3 referenceDirection, float radius, Vector3 referencePoint1, Vector3 referencePoint2)
        {
            this.center = center;
            this.direction = direction;
            this.referenceDirection = referenceDirection;
            this.radius = radius;
            this.referencePoint1 = referencePoint1;
            this.referencePoint2 = referencePoint2;
        }
    }
*/

/*
    List<Point> GetDebugPoints(List<Circle> circles, List<SplineCurve> splineCurves, float pointSeparation)
    {
        List<Point> points = new List<Point>();
        System.Random random = new System.Random();
        foreach (Circle circle in circles)
        {
            Vector3 randomColor = new Vector3(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)) / 255;
            for (int i = 0; i < 6; i++)
            {
                points.Add(new Point(circle.center + circle.direction * pointSeparation * i, randomColor));
                points.Add(new Point(circle.center + circle.referenceDirection * pointSeparation * i, randomColor));
            }
        }
        foreach (SplineCurve splineCurve in splineCurves)
        {
            Vector3 randomColor = new Vector3(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)) / 255;
            foreach (Vector3 position in splineCurve.positions)
            {
                points.Add(new Point(position, randomColor));
            }
        }
        return points;
    }
*/
/*
    List<Circle> GetCirclesFromEntities(OrderedDictionary entities)
    {
        List<Circle> circleList = new List<Circle>();
        for (int i = 0; i < entities.Count; i++)
        {
            string line = entities[i].ToString();
            if (line.StartsWith("CIRCLE"))
            {
                circleList.Add(GetCircleFromCircleEntity(line, entities));
            }
        }
        return circleList;
    }

    Circle GetCircleFromCircleEntity(string line, OrderedDictionary entities)
    {
        //Debug.Log(line);
        Circle circle = new Circle();
        string[] sections = line.Split(',');
        for (int i = 0; i < sections.Length; i++)
        {
            string section = sections[i];
            //Debug.Log(section);
            if (section.StartsWith("#")) //if it's an entity (will be an axis2_placement_3d for a circle)
            {
                //Debug.Log("section is entity");
                section = section.Replace("#", String.Empty);
                //Debug.Log(section);
                int counter = 0;
                int entityID = 0;
                while (!int.TryParse(section.Substring(0, section.Length - counter), out entityID)) //get id excluding stuff at the end like parentheses and commas and semicolons
                {
                    counter++;
                }
                //Debug.Log(entityID);
                if (entities.Contains((object)entityID)) //this will be a direction or cartesian point
                {
                    string newEntity = entities[(object)entityID].ToString();
                    //Debug.Log(newEntity);
                    if (newEntity.StartsWith("AXIS2_PLACEMENT_3D"))
                    {
                        string[] newSections = newEntity.Split(',');
                        if (int.TryParse(newSections[1].Replace("#", String.Empty), out int cartesianID))
                        {
                            circle.center = GetVector3FromCartesianPointOrDirection(entities[(object)cartesianID].ToString());
                        }
                        if (int.TryParse(newSections[2].Replace("#", String.Empty), out int directionID))
                        {
                            circle.direction = GetVector3FromCartesianPointOrDirection(entities[(object)directionID].ToString());
                        }
                        if (int.TryParse(newSections[3].Replace("#", String.Empty).Replace(")", String.Empty).Replace(";", String.Empty), out int referenceDirectionID))
                        {
                            circle.referenceDirection = GetVector3FromCartesianPointOrDirection(entities[(object)referenceDirectionID].ToString());
                        }

                    }
                    if (newEntity.StartsWith("CARTESIAN_POINT")) //if it's a cartesian point
                    {
                        circle.center = GetVector3FromCartesianPointOrDirection(newEntity);
                    }
                    else if (newEntity.StartsWith("DIRECTION"))
                    {
                        if (circle.direction == Vector3.zero) //should honestly just hardcode this in order it's useless to go through a for loop and test for stuff if I know what everything is
                        {
                            circle.direction = GetVector3FromCartesianPointOrDirection(newEntity);
                        }
                        else
                        {
                            circle.referenceDirection = GetVector3FromCartesianPointOrDirection(newEntity);
                        }
                    }
                }
            }
            else if (char.IsDigit(section[0])) //if it's a number (will be radius)
            {
                int counter = 0;
                float radius = 0;
                while (!float.TryParse(section.Substring(0, section.Length - counter), out radius)) //get radius excluding stuff at the end like parentheses and commas and semicolons
                {
                    counter++;
                }
                circle.radius = radius;
            }
        }
        return circle;
    }
*/
/*
    List<Circle> CombineCircles(List<Circle> circles1)
    {
        List<Circle> circles = circles1;
        List<Circle> combinedCircles = new List<Circle>();
        for (int i = 0; i < circles.Count; i++)
        {
            if (circles[i].radius != 0)
            {
                for (int j = i + 1; j < circles.Count + i + 1; j++)
                {
                    int index = j % circles.Count;
                    if (circles[i].center == circles[index].center && circles[index].radius != 0)
                    {
                        Circle newCircle = new Circle(circles[i].center, circles[i].direction, circles[i].referenceDirection, circles[i].radius, 
                                                    circles[i].referenceDirection * circles[i].radius + circles[i].center, 
                                                    circles[index].referenceDirection * circles[index].radius + circles[index].center);
                        if ((newCircle.referencePoint1 - newCircle.referencePoint2).magnitude - newCircle.radius*2 < tolerance) //if reference points are diametrically opposite like in a cable
                        {
                            combinedCircles.Add(newCircle);
                            //set radii to zero by making them new circles so that I don't reuse the same circles
                            circles[i] = new Circle();
                            circles[index] = new Circle();
                        }
                    }
                }
            }
        }
        return combinedCircles;
    }
*/