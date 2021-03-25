// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.Utilities.Terrain
{
    public class TerrainUtil
    {
        /**
         * Locates the active terrain at the supplied world position
         * 
         * @param worldPosition The inquiry world position
         * 
         * @return The active <code>Terrain</code> located at the x/z of the supplied world position,
         *         or null if not located
         *         
         * @see Terrain
         * @see Terrain.activeTerrains
         */
        public static UnityEngine.Terrain FindActiveTerrain(Vector3 worldPosition)
        {
            UnityEngine.Terrain result = null;

            // Find the active terrain that we need to use; the one below the supplied world space
            UnityEngine.Terrain[] terrains = UnityEngine.Terrain.activeTerrains;
            foreach (UnityEngine.Terrain terrain in terrains)
            {
                // Get the terrain width (X) and length (Z)
                float terrainWidth = terrain.terrainData.bounds.max.x - terrain.terrainData.bounds.min.x;
                float terrainLength = terrain.terrainData.bounds.max.z - terrain.terrainData.bounds.min.z;

                // Get the world space of the terrain. Pivot point should be in the bottom-left
                Vector3 terrainPosition = terrain.gameObject.transform.position;

                // See if the supplied world space within the world space bounds of the terrain
                if ((worldPosition.x >= terrainPosition.x) && (worldPosition.x <= (terrainPosition.x + terrainWidth)) &&
                    (worldPosition.z >= terrainPosition.z) && (worldPosition.z <= (terrainPosition.z + terrainLength)))
                {
                    // Found it
                    result = terrain;
                    break;
                }
            }

            return result;
        }

        /**
         * Obtains the active terrains that are part of the group associated with the supplied
         * group ID
         * 
         * @param groupID The terrain group ID to query
         * 
         * @return An array of active <code>Terrain</code> objects that are part of the supplied group ID
         * 
         * @see Terrain
         */
        public static UnityEngine.Terrain[] GetTerrainGroup(int groupId)
        {
            List<UnityEngine.Terrain> terrainGroup = new List<UnityEngine.Terrain>();

            // Find the active terrain that we need to use; the one below the supplied world space
            UnityEngine.Terrain[] terrains = UnityEngine.Terrain.activeTerrains;
            foreach (UnityEngine.Terrain terrain in terrains)
            {
                if (terrain.groupingID == groupId)
                {
                    terrainGroup.Add(terrain);
                }
            }

            return terrainGroup.ToArray();
        }

        /**
         * Obtains the world space bounds of the supplied terrain array
         * 
         * @param terrains The array of <code>Terrain</code> objects used to perform the calculation
         * 
         * @return A <code>Bounds</code> representing the world space bounds of the supplied Terrain array
         *         
         * @see Terrain
         * @see Terrain.activeTerrains
         * @see #GetTerrainGroup
         */
        public static Bounds GetTerrainGroupBounds(UnityEngine.Terrain[] terrains)
        {
            Bounds result = new Bounds(Vector3.zero, Vector3.zero);

            if (terrains.Length > 0)
            {
                result = new Bounds(terrains[0].transform.position, Vector3.zero);

                // Expand the bounds as we add each terrain
                foreach (UnityEngine.Terrain terrain in terrains)
                {
                    Bounds localBounds = terrain.terrainData.bounds;

                    // The terrains are in local space, so convert to world space
                    Vector3 worldSize = terrain.transform.TransformVector(localBounds.size);
                    Vector3 worldCenter = terrain.transform.TransformPoint(localBounds.center);
                    Bounds worldBounds = new Bounds(worldCenter, worldSize);

                    // Encapsulate the current world bounds
                    result.Encapsulate(worldBounds);
                }
            }

            return result;
        }

    }

}
