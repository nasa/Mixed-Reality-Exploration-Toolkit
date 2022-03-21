using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Infrastructure.Components.Locomotion;
using GSFC.ARVR.MRET;

namespace GSFC.ARVR.MRET.Infrastructure.Components.Locomotion
{
    /// <remarks>
    /// History:
    /// 15 July 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// ClimbingHandController
    ///
    /// Takes user hand inputs and feeds into ClimbingLocomotionController
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 
    public class ClimbingHandController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ClimbingHandController);
            }
        }
        public ClimbingLocomotionController climb;
        public InputHand hand;
        //List of grabbable rungs (colliding with the InputHands)
        public List<GameObject> rungs = new List<GameObject>();
        //The rung that is associated with the hand that is controlling locomotion
        public GameObject currentRung;

        /// <summary>
        /// GrabBegin
        /// 
        /// Called when Grip is pressed (managed under InputHand Inspector)
        /// Calls method to try to grab a rung with this hand
        /// </summary>
        public void GrabBegin()
        {
            GrabRung();
        }

        /// <summary>
        /// GrabComplete
        /// 
        /// Called when Grip is released (managed under InputHand Inspector)
        /// If the hand that is currently controlling locomotion is this hand,
        /// calls method to try to release the rung
        /// </summary>
        public void GrabComplete()
        {
            if (climb.currentHand == this.hand)
            {
                ReleaseRung();
            }
        }

        /// <summary>
        /// GrabRung
        /// 
        /// Assigns the currentRung to the closest rung to this hand from the rungs list
        /// If there is one, then the hand controlling locomotion (currentHand) becomes this hand
        /// </summary>
        void GrabRung()
        {
            currentRung = FindClosestRung();
            if (currentRung)
            {
                climb.currentHand = this.hand;
            }
        }

        /// <summary>
        /// ReleaseRung
        /// 
        /// If there is a currentRung, then the hand controlling locomotion (currentHand) is set to null
        /// </summary>
        public void ReleaseRung()
        {
            if (currentRung)
            {
                climb.currentHand = null;
            }
        }

        /// <summary>
        /// AddRung
        /// 
        /// If newRung has the "Rung" tag, then it will remove any duplicates of the new rung
        /// from the rungs ArrayList, and add the new rung to the rungs ArrayList.
        /// Called when this InputHand trigger collides with a rung.
        /// 
        /// </summary>
        /// <param name="newRung">The GameObject that will try to be added as a grabbable rung</param>
        void AddRung(GameObject newRung)
        {
            if (newRung.CompareTag("Rung"))
            {
                for (int i = 0; i < rungs.Count; i++)
                {
                    if (rungs[i] == newRung)
                    {
                        rungs.RemoveAt(i); //If the rung is already in the list (caused by menu trigger), will remove
                    }
                }
                rungs.Add(newRung); //Then add new rung
            }
        }

        /// <summary>
        /// RemoveRung
        /// 
        /// If newRung has the "Rung" tag, then it will be removed from the rungs ArrayList.
        /// Called when this InputHand trigger exits a rung.
        /// 
        /// </summary>
        /// <param name="newRung">The GameObject that will be removed as a grabbable rung</param>
        void RemoveRung(GameObject newRung)
        {
            if (newRung.CompareTag("Rung"))
            {
                rungs.Remove(newRung);
            }
        }

        /// <summary>
        /// OnTriggerEnter
        /// 
        /// Called when this InputHand trigger collides with another collider
        /// Attempts to add the collider to the rung ArrayList
        /// </summary>
        /// <param name="col">the collider that the InputHand trigger collides with</param>
        void OnTriggerEnter(Collider col)
        {
            AddRung(col.gameObject);
        }

        /// <summary>
        /// OnTriggerExit
        /// 
        /// Called when this InputHand trigger exits another collider
        /// Attempts to remove the collider from the rungArrayList
        /// </summary>
        /// <param name="col">the collider that the InputHand trigger exits</param>
        void OnTriggerExit(Collider col)
        {
            RemoveRung(col.gameObject);
        }

        /// <summary>
        /// FindClosestRung
        /// 
        /// Iterates through the rungs ArrayList and finds the closest rung to this hand
        /// </summary>
        /// <returns>The closest rung to this hand</returns>
        private GameObject FindClosestRung()
        {
            if (rungs.Count == 1)
            {
                return rungs[0];
            }
            else
            {
                GameObject closestRung = null;
                float distance = 0.0f;
                float tempMinDistance = 0.0f;
                foreach (GameObject rung in rungs)
                {
                    distance = (rung.transform.position - transform.position).sqrMagnitude;
                    if (distance <= tempMinDistance)
                    {
                        tempMinDistance = distance;
                        closestRung = rung;
                    }
                }
                return closestRung;
            }

        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (hand == null)
                    ? IntegrityState.Failure   // Fail if base class fails, OR hands are null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }
    }
}

