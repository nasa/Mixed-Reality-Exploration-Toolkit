// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET;

namespace GOV.NASA.GSFC.XR.MRET.UI.Avatar
{
    /// <remarks>
    /// History:
    /// 27 July 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// ClothesSelection
    ///
    /// References OutfitController. Uses button listener to cycle between outfits
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 

    public class ClothesSelection : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ClothesSelection);
            }
        }

        //Assigned on Awake
        public OutfitController[] ocs;

        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // Find reference
            ocs = GetComponentsInChildren<OutfitController>(true);
        }
    
        /// <summary>
        /// CycleForward
        /// 
        /// Calls the SwitchOutfitForward method from the OutfitController
        /// </summary>
        public void CycleForward()
        {
            foreach (OutfitController oc in ocs)
            {
                oc.SwitchOutfitForward();
            }
        }

        /// <summary>
        /// CycleBackward
        /// 
        /// Calls the SwitchOutfitBackward method from the OutfitController
        /// </summary>
        public void CycleBackward()
        {
            foreach (OutfitController oc in ocs)
            {
                oc.SwitchOutfitBackward();
            }
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure)
                ? IntegrityState.Failure   // Fail if base class fails
                : IntegrityState.Success); // Otherwise, our integrity is valid
        }
    }
}
