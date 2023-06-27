// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    ///<summary>
    /// Terrain Tile is used for streaming terrain tiles.
    /// Contains the list of neighboring terrains and functions to activate
    /// neighbors and deactivate non neighbors
    /// Author: Molly T. Goldstein
    ///</summary>
    public class TerrainTile : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TerrainTile);

        /// <summary>
        /// The list of neighboring terrain tiles to the current tile
        /// </summary>
        public List<Terrain> neighbors = new List<Terrain>();

        /// <summary>
        /// Flag to indicate if the stream has started yet. This allows for the first tile
        /// and its neighbors to be set to active.
        /// </summary>
        bool start = true;

        /// <summary>
        /// If the start flag is true, then this is the first update call.
        /// When the flag is true, the tile checks if it is the starting tile. If it is,
        /// it activates itself and adds its game object's Terrain component to the head of the
        /// active terrains linked list stored in TerrainStreamingManager. Then it activates all neighbors.
        /// It then sets start to false.
        /// </summary>
        override protected void MRETUpdate()
        {
            base.MRETUpdate();

            if (start)
            {
                if (TerrainStreamingManager.start.Equals(this.gameObject.name))
                {
                    this.gameObject.SetActive(true);
                    start = true;
                    TerrainStreamingManager.activeTerrainHead = new ActiveTerrain();
                    TerrainStreamingManager.activeTerrainHead.terrain = this.gameObject.GetComponent<Terrain>();
                    TerrainStreamingManager.activeTerrainTail = TerrainStreamingManager.activeTerrainHead;
                    ActivateNeighbors();
                }
                else
                {
                    this.gameObject.SetActive(false);
                }
                start = false;
            }
        }

        /// <summary>
        /// The function called in TerrainCollision when the character controller enters a new terrain.
        /// This function calls activate neighbors and deactivate far terrains.
        /// </summary>
        public void EnteredNewTerrain()
        {
            Debug.Log("Terrain entered: " + this.gameObject.name);
            ActivateNeighbors();
            DeactivateFarTerrains();
        }

        /// <summary>
        /// Activate all neighboring terrain tile game objects and insert them into the active terrains list.
        /// </summary>
        private void ActivateNeighbors()
        {
            foreach(Terrain neighbor in this.neighbors)
            {
                if(neighbor)
                {
                    neighbor.gameObject.SetActive(true);
                    InsertTerrain(neighbor);
                }
            }
        }

        /// <summary>Check if the terrain is in the active terrains list.</summary>
        /// <param name="terrain">The terrain to search for</param>
        /// <returns>True if the terrain is in the active terrain list, false if not.</returns>
        private bool FindTerrain(Terrain terrain)
        {
            ActiveTerrain current = TerrainStreamingManager.activeTerrainHead;

            while(current != null)
            {
                if(current.terrain == terrain)
                {
                    return true;
                }
                current = current.next;
            }
            return false;
        }

        /// <summary>Insert the terrain into the active terrains list.</summary>
        /// <param name="terrain">The terrain to insert into the active terrains list.</param>
        /// <returns>True if the terrain is added, false if it is not added.</returns>
        private bool InsertTerrain(Terrain terrain)
        {
            if (!FindTerrain(terrain))
            {
                TerrainStreamingManager.activeTerrainTail.next = new ActiveTerrain();
                TerrainStreamingManager.activeTerrainTail.next.terrain = terrain;
                TerrainStreamingManager.activeTerrainTail = TerrainStreamingManager.activeTerrainTail.next;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deactivates all terrains in the active terrains list that are not a neighbor of the current terrain tile.
        /// When deactivated, the terrain's game object is set to inactive and is removed from the active terrains list.
        /// </summary>
        private void DeactivateFarTerrains()
        {
            ActiveTerrain current = TerrainStreamingManager.activeTerrainHead;
            ActiveTerrain prev = current;

            // Iterate through each terrain in the active terrains list
            // and check if each terrain is a neighbor of the current terrain tile.
            while (current != null)
            {
                // If the terrain is not a neighbor and not the current tile, deactivate and remove from the active terrains list.
                if (!neighbors.Contains(current.terrain) &&
                    current.terrain.gameObject != this.gameObject)
                {
                    current.terrain.gameObject.SetActive(false);
                    // Check if the terrain is the head of the list and remove for this case if it is
                    if (TerrainStreamingManager.activeTerrainHead == current)
                    {
                        TerrainStreamingManager.activeTerrainHead.terrain.gameObject.SetActive(false);
                        TerrainStreamingManager.activeTerrainHead = TerrainStreamingManager.activeTerrainHead.next;
                        current = TerrainStreamingManager.activeTerrainHead;
                        prev = current;
                    }
                    // Check if the terrain is the tail of the list
                    else if (current == TerrainStreamingManager.activeTerrainTail)
                    {
                        TerrainStreamingManager.activeTerrainTail.terrain.gameObject.SetActive(false);
                        prev.next = null;
                        TerrainStreamingManager.activeTerrainTail = prev;
                        break;
                    }
                    // Removal if the terrain is neither the head nor the tail
                    else
                    {
                        prev.next = current.next;
                    }
                }
                // If the terrain is a neighbor, skip and move on to the next terrain in the list
                else
                {
                    prev = current;
                }
                current = current.next;
            }

            // Had to add this extra neighbor check and removal for the head of the list.
            if (!neighbors.Contains(TerrainStreamingManager.activeTerrainHead.terrain) && TerrainStreamingManager.activeTerrainHead.terrain.gameObject != this.gameObject)
            {
                TerrainStreamingManager.activeTerrainHead.terrain.gameObject.SetActive(false);
                TerrainStreamingManager.activeTerrainHead = TerrainStreamingManager.activeTerrainHead.next;
            }
        }
    }
}