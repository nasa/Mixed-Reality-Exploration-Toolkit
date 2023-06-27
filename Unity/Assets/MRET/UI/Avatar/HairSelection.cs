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
    /// HairSelection
    ///
    /// References HairController. Uses button listener to cycle between hairstyles
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 

    public class HairSelection : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(HairSelection);
            }
        }

        public HairController[] hcs;

        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // Find reference
            hcs = GetComponentsInChildren<HairController>(true);
        }
    
        /// <summary>
        /// CycleForward
        /// 
        /// Calls the SwitchHairForward method from the HairController
        /// </summary>
        public void CycleForward()
        {
            foreach (HairController hc in hcs)
            {
                hc.SwitchHairForward();
            }
        }

        /// <summary>
        /// CycleBackward
        /// 
        /// Calls the SwitchHairBackward method from the HairController
        /// </summary>
        public void CycleBackward()
        {
            foreach (HairController hc in hcs)
            {
                hc.SwitchHairBackward();
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
