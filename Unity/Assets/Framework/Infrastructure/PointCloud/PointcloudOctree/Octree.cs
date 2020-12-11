using System.Collections.Generic;
using UnityEngine;

// Code taken and adapted from https://github.com/Nition/UnityOctree
// Copyright 2014 Nition, BSD licence (see LICENCE file). www.momentstudio.co.nz
// Unity-based, but could be adapted to work in pure C#
namespace Octree
{
    public class Octree<T>
    {
        // Adding an OctreeIndex here. Not sure if we'll need it yet; maybe this library has its own convention.
        public enum OctreeIndex
        {
            // 1st 0 = Bottom/Top
            // 2nd 0 = Left/Right
            // 3rd 0 = Front/Back
            BottomLeftFront = 0, //000
            BottomRightFront = 2, //010
            BottomRightBack = 3, //011
            BottomLeftBack = 1, //001
            TopLeftFront = 4, //100
            TopRightFront = 6, //110
            TopRightBack = 7, //111
            TopLeftBack = 5, //101
        }

        private int GetIndexOfPosition(Vector3 lookupPosition, Vector3 pointPosition)
        {
            int index = 0;
            index |= lookupPosition.y > pointPosition.y ? 4 : 0;
            index |= lookupPosition.x > pointPosition.x ? 2 : 0;
            index |= lookupPosition.z > pointPosition.z ? 1 : 0;

            Debug.Log("index is: " + index);
            return index;
        }



        // The total amount of objects currently in the tree
        public int Count { get; private set; }

        // Root node of the octree
        OctreeNode<T> rootNode;

        // Adding a getter function for root node
        public OctreeNode<T> GetRootNode()
        {
            return rootNode;
        }

        // Size that the octree was on creation
        readonly float initialSize;

        // Minimum side length that a node can be - essentially an alternative to having a max depth
        readonly float minSize;


        // Constructor for the point octree.

        // initialWorldSize = Size of the sides of the initial node. The octree will never shrink smaller than this.
        // initialWorldPos = Position of the centre of the initial node.
        // minNodeSize = Nodes will stop splitting if the new nodes would be smaller than this.
        public Octree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                Debug.LogWarning("Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize + " Adjusted to: " + initialWorldSize);
                minNodeSize = initialWorldSize;
            }
            Count = 0;
            initialSize = initialWorldSize;
            minSize = minNodeSize;
            rootNode = new OctreeNode<T>(initialSize, minSize, initialWorldPos);
        }

        // #### PUBLIC METHODS ####


        // Add an object.
        public void Add(T obj, Vector3 objPos)
        {
            // Add object or expand the octree until it can be added
            int count = 0; // Safety check against infinite/excessive growth
            while (!rootNode.Add(obj, objPos))
            {
                Grow(objPos - rootNode.Center);
                if (++count > 20)
                {
                    Debug.LogError("Aborted Add operation as it seemed to be going on forever (" + (count - 1) + ") attempts at growing the octree.");
                    return;
                }
            }
            Count++;
        }


        // Remove an object. Makes the assumption that the object only exists once in the tree.
        public bool Remove(T obj)
        {
            bool removed = rootNode.Remove(obj);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }


        // Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
        public bool Remove(T obj, Vector3 objPos)
        {
            bool removed = rootNode.Remove(obj, objPos);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }


        // Returns objects that are within maxDistance of the specified ray.
        // If none, returns false. Uses supplied list for results.
        public bool GetNearbyNonAlloc(Ray ray, float maxDistance, List<T> nearBy)
        {
            nearBy.Clear();
            rootNode.GetNearby(ref ray, maxDistance, nearBy);
            if (nearBy.Count > 0)
                return true;
            return false;
        }


        // Returns objects that are within maxDistance of the specified ray.
        // If none, returns an empty array (not null).
        public T[] GetNearby(Ray ray, float maxDistance)
        {
            List<T> collidingWith = new List<T>();
            rootNode.GetNearby(ref ray, maxDistance, collidingWith);
            return collidingWith.ToArray();
        }


        // Returns objects that are within maxDistance of the specified position.
        // If none, returns an empty array (not null).
        public T[] GetNearby(Vector3 position, float maxDistance)
        {
            List<T> collidingWith = new List<T>();
            rootNode.GetNearby(ref position, maxDistance, collidingWith);
            return collidingWith.ToArray();
        }


        // Returns objects that are within maxDistance of the specified position.
        // If none, returns false. Uses supplied list for results.
        public bool GetNearbyNonAlloc(Vector3 position, float maxDistance, List<T> nearBy)
        {
            nearBy.Clear();
            rootNode.GetNearby(ref position, maxDistance, nearBy);
            if (nearBy.Count > 0)
                return true;
            return false;
        }


        // Return all objects in the tree.
        // If none, returns an empty array (not null).
        public ICollection<T> GetAll()
        {
            List<T> objects = new List<T>(Count);
            rootNode.GetAll(objects);
            return objects;
        }


        // Draws node boundaries visually for debugging.
        // Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
        public void DrawAllBounds()
        {
            rootNode.DrawAllBounds();
        }


        // Draws the bounds of all objects in the tree visually for debugging.
        // Must be called from OnDrawGizmos externally. See also: DrawAllBounds.
        public void DrawAllObjects()
        {
            rootNode.DrawAllObjects();
        }

        // #### PRIVATE METHODS ####


        // Grow the octree to fit in all objects.
        // direction = Direction to grow.
        void Grow(Vector3 direction)
        {
            int xDirection = direction.x >= 0 ? 1 : -1;
            int yDirection = direction.y >= 0 ? 1 : -1;
            int zDirection = direction.z >= 0 ? 1 : -1;
            OctreeNode<T> oldRoot = rootNode;
            float half = rootNode.SideLength / 2;
            float newLength = rootNode.SideLength * 2;
            Vector3 newCenter = rootNode.Center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            // Create a new, bigger octree root node
            rootNode = new OctreeNode<T>(newLength, minSize, newCenter);

            if (oldRoot.HasAnyObjects())
            {
                // Create 7 new octree children to go with the old root as children of the new root
                int rootPos = rootNode.BestFitChild(oldRoot.Center);
                OctreeNode<T>[] children = new OctreeNode<T>[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i == rootPos)
                    {
                        children[i] = oldRoot;
                    }
                    else
                    {
                        xDirection = i % 2 == 0 ? -1 : 1;
                        yDirection = i > 3 ? -1 : 1;
                        zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                        children[i] = new OctreeNode<T>(oldRoot.SideLength, minSize, newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                    }
                }

                // Attach the new children to the new root node
                rootNode.SetChildren(children);
            }
        }


        // Shrink the octree if possible, else leave it the same.
        void Shrink()
        {
            rootNode = rootNode.ShrinkIfPossible(initialSize);
        }
    }
}