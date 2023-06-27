// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Terrains
{
    /// <remarks>
    /// History:
    /// Janaury 2022: V1 Added into MRET
    /// </remarks>
    ///<summary>
    /// DynamicTerrainTile is used for streaming terrain tiles.
    /// Contains functions to activate neighbors and deactivate non neighbors of terrain tile.
    /// Author: Molly T. Goldstein
    ///</summary>
    public class DynamicTerrainTile : MRETBehaviour
    {
        public override string ClassName => nameof(DynamicTerrainTile);

        ///<summary>
        /// Flag to indicate if the stream has started yet. This allows for the first tile
        /// and its neighbors to be set to active.
        ///</summary>
        public bool start = false;

        /// <summary>
        /// If the start flag is true, then this is the first update call.
        /// When the flag is true, the tile checks if it is the starting tile. If it is,
        /// it activates itself and adds its game object's Terrain component to the head of the
        /// active terrains linked list stored in DynamicTerrainStreamingManager. Then it activates all neighbors.
        /// It then sets start to false.
        /// </summary>
        override protected void MRETStart()
        {
            if(start)
            {
                this.gameObject.SetActive(true);
                start = false;
                DynamicTerrainStreamingManager.activeTerrainHead = new ActiveTerrain();
                DynamicTerrainStreamingManager.activeTerrainHead.terrain = this.gameObject.GetComponent<Terrain>();
                DynamicTerrainStreamingManager.activeTerrainTail = DynamicTerrainStreamingManager.activeTerrainHead;
                ActivateNeighbors();
            }
        }

        /// <summary>
        /// The function called in DynamicTerrainCollision when the character controller enters a new terrain.
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
        /// If a neighbor is not yet generated, generate the terrain by calling the DynamicTerrainStreamingManager.CreateTerrain function.
        /// </summary>
        private void ActivateNeighbors()
        {
            foreach (string neighbor in DynamicTerrainStreamingManager.allTerrains[this.gameObject.name])
            {
                if (!string.IsNullOrEmpty(neighbor))
                {
                    if (!DynamicTerrainStreamingManager.generatedTerrains.ContainsKey(neighbor))
                    {
                        DynamicTerrainStreamingManager.CreateTerrain(neighbor);
                    }
                    Terrain terrain = DynamicTerrainStreamingManager.generatedTerrains[neighbor];
                    terrain.gameObject.SetActive(true);
                    InsertTerrain(terrain);
                }
            }
        }

        /// <summary>Check if the terrain is in the active terrains list.</summary>
        /// <param name="terrain">The terrain to search for</param>
        /// <returns>True if the terrain is in the active terrain list, false if not.</returns>
        private bool FindTerrain(Terrain terrain)
        {
            ActiveTerrain current = DynamicTerrainStreamingManager.activeTerrainHead;
            while (current != null)
            {
                if (current.terrain == terrain)
                {
                    return true;
                }
                current = current.next;
            }

            return false;
        }

        /// <summary>Insert the terrain into the active terrains list.</summary>
        /// <param name="">The terrain to insert into the active terrains list.</param>
        /// <returns>True if the terrain is added, false if it is not added.</returns>
        private bool InsertTerrain(Terrain terrain)
        {
            if (!FindTerrain(terrain))
            {
                DynamicTerrainStreamingManager.activeTerrainTail.next = new ActiveTerrain();
                DynamicTerrainStreamingManager.activeTerrainTail.next.terrain = terrain;
                DynamicTerrainStreamingManager.activeTerrainTail = DynamicTerrainStreamingManager.activeTerrainTail.next;
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
            ActiveTerrain current = DynamicTerrainStreamingManager.activeTerrainHead;
            ActiveTerrain prev = current;

            // Iterate through each terrain in the active terrains list
            // and check if each terrain is a neighbor of the current terrain tile.
            while(current != null)
            {
                // If the terrain is not a neighbor and not the current tile, deactivate and remove from the active terrains list.
                if(!DynamicTerrainStreamingManager.allTerrains[this.gameObject.name].Contains(current.terrain.gameObject.name) && 
                    current.terrain.gameObject != this.gameObject)
                {
                    current.terrain.gameObject.SetActive(false);
                    // Check if the terrain is the head of the list and remove for this case if it is
                    if(DynamicTerrainStreamingManager.activeTerrainHead == current)
                    {
                        DynamicTerrainStreamingManager.activeTerrainHead.terrain.gameObject.SetActive(false);
                        DynamicTerrainStreamingManager.activeTerrainHead = DynamicTerrainStreamingManager.activeTerrainHead.next;
                        current = DynamicTerrainStreamingManager.activeTerrainHead;
                        prev = current;
                    }
                    // Check if the terrain is the tail of the list
                    else if(current == DynamicTerrainStreamingManager.activeTerrainTail)
                    {
                        DynamicTerrainStreamingManager.activeTerrainTail.terrain.gameObject.SetActive(false);
                        prev.next = null;
                        DynamicTerrainStreamingManager.activeTerrainTail = prev;
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
            if (!DynamicTerrainStreamingManager.allTerrains[this.gameObject.name].Contains(DynamicTerrainStreamingManager.activeTerrainHead.terrain.gameObject.name) && DynamicTerrainStreamingManager.activeTerrainHead.terrain.gameObject != this.gameObject)
            {
                DynamicTerrainStreamingManager.activeTerrainHead.terrain.gameObject.SetActive(false);
                DynamicTerrainStreamingManager.activeTerrainHead = DynamicTerrainStreamingManager.activeTerrainHead.next;
            }
        }
    }
}