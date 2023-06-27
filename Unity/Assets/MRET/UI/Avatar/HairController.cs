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
    /// HairController
    ///
    /// Ingests values from HairSelection and changes hairstyle that is active
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 

    public class HairController : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(HairController);
            }
        }

        //List of all hairstyles attached to character
        public List<GameObject> hairStyles = new List<GameObject>();
        public int activeHairIndex;

        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            activeHairIndex = 0;
        }

        /// <summary>
        /// SwitchHairForward
        /// 
        /// Checks to see if there are hair gameobjects in List, moves onto next if there are by calling ChangeHair method
        /// Called on when right button is pressed under hair UI in CharacterCustomizationMenu
        /// </summary>    
        public void SwitchHairForward()
        {
            if (hairStyles.Count > 0)
            {
                if (hairStyles.Count > 1 && activeHairIndex < hairStyles.Count - 1)
                {
                    activeHairIndex++;
                }
                else
                {
                    activeHairIndex = 0;
                }

                ChangeHair();
            }
        }

        /// <summary>
        /// SwitchHairBackward
        /// 
        /// Checks to see if there are hair gameobjects in List, moves onto previous if there are by calling ChangeHair method
        /// Called on when left button is pressed under hair UI in CharacterCustomizationMenu
        /// </summary> 
        public void SwitchHairBackward()
        {
            if (hairStyles.Count > 0)
            {
                if (hairStyles.Count > 1)
                {
                    if (activeHairIndex != 0)
                    {
                        activeHairIndex--;
                    }
                    else
                    {
                        activeHairIndex = hairStyles.Count - 1;
                    }
                }
                else
                {
                    activeHairIndex = 0;
                }

                ChangeHair();
            }
        }

        /// <summary>
        /// ChangeHair
        /// 
        /// Deactivates all hair attached to all characters
        /// Activates specific hair attached to all characters (done by reference number)
        /// </summary> 
        void ChangeHair() 
        {
            foreach (GameObject style in hairStyles)
            {
                style.SetActive(false); //deactivate all hair
            }

            hairStyles[activeHairIndex].SetActive(true); //activate chosen hairstyle
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

