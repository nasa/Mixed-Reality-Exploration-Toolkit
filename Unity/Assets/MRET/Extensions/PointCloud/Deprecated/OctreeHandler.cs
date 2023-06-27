// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Extensions.PointCloud
{
    public class OctreeHandler : MonoBehaviour
    {


        /*
         * HOW TO DIVIDE INTO OCTREE NODES:
         * Create a convention
         * For every level:
         *  Figure out which division of each axis:
         *      For x (then for y, then for z):
         *          DivisionSize = (end - start) / NumberOfDivisions
         *          Divison = ceiling((point - start) / divSize - 1)
         *      Should end up with Vector3 of 3 divisions for x, y, and z    
         *      Then add into the node of this level depending on which number you arrived at for your table lookup
         */

        //TODO: eventually, we're going to want the menu option of measuring to automatically enable the OctreeHandler GameObject


        public List<GameObject> currentColliders = new List<GameObject>();
#if MRET_EXTENSION_UNITYOCTREE
        Vector3 measurePoint1 = new Vector3(0, 0, 0);
        Vector3 measurePoint2 = new Vector3(0, 0, 0);
        bool isFirstMeasurePointFilled = false;
        float distance;
        private PointOctree<Vector3> octree;
        PointOctreeNode<Vector3> currentNode;
        PointOctreeNode<Vector3> rootNode;

        // Start is called before the first frame update
        void Start()
        {
            // Maybe add a value in the editor to be able to quickly change pointcloud gameobject
            //GameObject Pointcloud = GameObject.Find("PointcloudStatic_II");
            //GameObject Pointcloud = GameObject.Find("PointcloudStatic_II_Brennan");
            GameObject Pointcloud = GameObject.Find("PointcloudStatic_II_Example");
            PointCloudRenderer PointCloudScript = Pointcloud.GetComponent<PointCloudRenderer>();
            float[][] pointCloudData = PointCloudScript.GetPointCloudData();
            //Debug.Log("this is a test: " + pointCloudData[0][1]);
            Vector3 PointCloudBound = PointCloudScript.GetPointCloudSize();
            //float PointCloudSizeMeters = Mathf.Max(PointCloudBound.x, PointCloudBound.y, PointCloudBound.z);
            //print("PointCloudBoundMaxMeters: " + PointCloudSizeMeters);
            //print("printing from OctreeHandler");
            // debugging
            octree = new PointOctree<Vector3>((PointCloudScript.GetPointCloudBounds().size.x / 100f), PointCloudScript.GetPointCloudBounds().center, 1f);
            // octree = new PointOctree<Vector3>(PointCloudSizeMeters, PointCloudScript.GetPointCloudPosition(), 1);

            // Initial iteration through points
            // Dividing by grid size to determine x,y,z min/max
            Vector3[] pointCloudCoordinates = new Vector3[pointCloudData[0].Length / 3];
            //print("PointCloudData length: " + pointCloudData[0].Length);
            int j = 0;
            for (int i = 0; i < pointCloudData[0].Length; i++)
            {
                // First 3 are x,y,z, 4th is color. Prune out color
                pointCloudCoordinates[j] = new Vector3(pointCloudData[0][i], pointCloudData[0][++i], pointCloudData[0][++i]);
                //Debug.Log("pointcloudcoordinate at: " + j + " = " + pointCloudCoordinates[j]);
                j++;
                i++;
            }

            Vector3[] startAndEndIndex = FindStartAndEndIndex(pointCloudCoordinates);
            foreach (Vector3 point in pointCloudCoordinates) {
                Vector3 divisionNum = FindDivisionNum(point, startAndEndIndex, pointCloudCoordinates);
                // debugging
                // octree.Add(point, divisionNum);

                // this is weird. right now the object being stored is the point itself
                // should change that to leverage its possible usefulness
                // maybe adding color so that it can keep track of that?
                octree.Add(point, point);
            }

            // creating first collider
            rootNode = octree.GetRootNode();
            currentNode = octree.GetRootNode();
            AddAndDestroyPointColliders(currentNode);



        }

        void OnDrawGizmos()
        {
            // boundsTree.DrawCollisionChecks(); // Draw the last *numCollisionsToSave* collision check boundaries
            //UnityEngine.Debug.Log("OnDrawGizmos() being called");
            octree.DrawAllBounds(); // Draw node boundaries
            //octree.DrawAllObjects(); // Mark object positions

        }

        Vector3 FindDivisionNum(Vector3 point, Vector3[] startAndEndIndex, Vector3[] pointCloudCoordinates)
        {
            // startingPoint - first point for each axis
            // endingPoint - last point for each axis
            Vector3 startingPoint = new Vector3(0, 0, 0);
            Vector3 endingPoint = new Vector3(0, 0, 0);

            int divisionsPerAxis = 10;

            for (int i = 0; i < 3; i++)
            {

                startingPoint[i] = pointCloudCoordinates[(int)startAndEndIndex[0][i]][i];
                endingPoint[i] = pointCloudCoordinates[(int)startAndEndIndex[1][i]][i];
            }
            Vector3 divisionSize = new Vector3(0, 0, 0);
            Vector3 divisionNum = new Vector3(0, 0, 0);
            // TODO: Check if it's within bounds
            for(int i = 0; i < 3; i++)
            {
                
                divisionSize[i] = Convert.ToInt32((endingPoint[i] - startingPoint[i]) / divisionsPerAxis);
                divisionNum[i] = (int)(Math.Ceiling((point[i] - startingPoint[i]) / divisionSize[i] - 1));
            }

            return divisionNum;

        }

        Vector3[] FindStartAndEndIndex(Vector3[] pointCloudCoordinates)
        {
            Vector3 startingIndex = new Vector3(0, 0, 0);
            Vector3 endingIndex = new Vector3(0, 0, 0);
            for (int i = 0; i < pointCloudCoordinates.Length; i++)
            {
                if (pointCloudCoordinates[i].x < pointCloudCoordinates[(int)startingIndex.x].x)
                {
                    startingIndex.x = i;
                }
                if (pointCloudCoordinates[i].y < pointCloudCoordinates[(int)startingIndex.y].y)
                {
                    startingIndex.y = i;
                }
                if (pointCloudCoordinates[i].z < pointCloudCoordinates[(int)startingIndex.z].z)
                {
                    startingIndex.z = i;
                }
                if (pointCloudCoordinates[i].x > pointCloudCoordinates[(int)endingIndex.x].x)
                {
                    endingIndex.x = i;
                }
                if (pointCloudCoordinates[i].y > pointCloudCoordinates[(int)endingIndex.y].y)
                {
                    endingIndex.y = i;
                }
                if (pointCloudCoordinates[i].z < pointCloudCoordinates[(int)endingIndex.z].z)
                {
                    endingIndex.z = i;
                }
            }
            return new[] { startingIndex, endingIndex };
        }

        public PointOctree<Vector3> GetOctree()
        {
            return octree;
        }

        void AddAndDestroyPointColliders(PointOctreeNode<Vector3> currentNode)
        {
            UnityEngine.Debug.Log("AddAndDestroyPointColliders being called");

            // Destroying colliders from previous octree level
            foreach (GameObject pointObject in currentColliders)
            {
                Destroy(pointObject);
            }
            currentColliders.Clear();

            if (currentNode.HasAnyObjects())
            {
                foreach (PointOctreeNode<Vector3> point in currentNode.children)
                {
                    // creating gameobjects to add colliders to
                    GameObject pointObject = new GameObject("point");
                    pointObject.tag = "point";
                    pointObject.transform.position = point.Center;
                    // creating BoxCollider
                    pointObject.AddComponent<BoxCollider>();
                    BoxCollider tempBoxCollider = pointObject.GetComponent<BoxCollider>();
                    Bounds tempBoxBounds = tempBoxCollider.bounds;
                    tempBoxBounds.center = point.Center;
                    tempBoxBounds.size = point.actualBoundsSize;

                    // Adding gameobjects to currentColliders
                    currentColliders.Add(pointObject);

                    //debugging
                    UnityEngine.Debug.Log("point collider has been added");
                }

                //if(currentNode.Center != rootNode.Center)
                //{
                //    GoOctreeLevelDeeper(currentNode.Center);
                //}
                GoOctreeLevelDeeper(currentNode.Center);
            }
            else
            {
                return;
            }
        }

        public void GoOctreeLevelDeeper(Vector3 hit)
        {
            // debugging
            UnityEngine.Debug.Log("GoOctreeLevelDeeper has been called");

            if (!currentNode.HasAnyObjects())
            {
                UnityEngine.Debug.Log("currentNode is a leaf node, running GetNearbyPoints()");
                GetNearbyPoints(hit);
            }
            else
            {
                // debugging
                UnityEngine.Debug.Log("currentNode NOT a leaf node, running rest of GOLD");

                // Getting which node was hit from RaycastHit or AddDestroyPointColliders
                // This is very sloppy, I know. In the future, going to
                // redo octree structure so I don't have to run a search
                bool hitNodeFound = false;
                foreach (PointOctreeNode<Vector3> point in currentNode.children)
                {
                    //Vector3 diff = new Vector3(point.Center.x - hit.point.x, point.Center.y - hit.point.y, point.Center.z - hit.point.z);
                    //if (point.Center == hit.point)
                    if(Vector3.Distance(point.Center, hit) <= 1f)
                    {
                        currentNode = point;
                        hitNodeFound = true;
                        Debug.Log("Node successfully connected with GameObject!! :)");
                        GoOctreeLevelDeeper(currentNode.Center);
                    }
                }
                if (hitNodeFound == false)
                {
                    UnityEngine.Debug.Log("Node in octree not successfully connected with GameObject hit");
                    return;
                }
                AddAndDestroyPointColliders(currentNode);
            }
        }

        void GetNearbyPoints(Vector3 hit)
        {
            List<Vector3> nearbyPoints = new List<Vector3>();
            // There should be 1 object in here only -- the Vector3 position
            // if this changes in the future, will need to update this
            //UnityEngine.Debug.Log("currentNode.GetObjects(): " + currentNode.GetObjects());
            //Vector3 rayPosition = currentNode.GetObjects()[0].Pos;
            currentNode.GetNearby(ref hit, 1, nearbyPoints);
            Vector3 pickedPoint = FindClosestPoint(nearbyPoints, hit);
            if (!isFirstMeasurePointFilled)
            {
                // TODO: this part is inefficient, consolidate it and the following else statement into CreateTextMesh()
                measurePoint1 = pickedPoint;
                isFirstMeasurePointFilled = true;
                CreateTextMesh(pickedPoint);
            }
            else
            {
                measurePoint2 = pickedPoint;
                distance = MeasureDistance(measurePoint1, measurePoint2);
                CreateTextMesh(pickedPoint);
            }
        }

        Vector3 FindClosestPoint(List<Vector3> nearbyPoints, Vector3 hit)
        {
            // TODO: find the closest point. Hardcoded for now
            float leastDistance = Mathf.Infinity;
            int index = -1;
            for(int i = 0; i < nearbyPoints.Count; i++)
            {
                if(Mathf.Abs(Vector3.Distance(hit, nearbyPoints[i])) < leastDistance)
                {
                    leastDistance = Mathf.Abs(Vector3.Distance(hit, nearbyPoints[i]));
                    index = i;

                }
            }
            return nearbyPoints[index];
        }

        float MeasureDistance(Vector3 measurePoint1, Vector3 measurePoint2)
        {
            isFirstMeasurePointFilled = false;
            return Vector3.Distance(measurePoint1, measurePoint2);
        }

        // creating text of point 
        void CreateTextMesh(Vector3 pickedPoint)
        {
            string textObjectName = isFirstMeasurePointFilled ? "MeasurePoint1" : "MeasurePoint2";
            GameObject textObject = new GameObject(textObjectName);
            textObject.tag = "measure";
            textObject.transform.position = pickedPoint;
            textObject.AddComponent<TextMesh>();
            TextMesh tempTextMesh = GetComponent<TextMesh>();
            tempTextMesh.text = pickedPoint.ToString();
        }

#endif
    }
}