// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.Avatar
{
    /// <remarks>
    /// History:
    /// 27 July 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// OutfitController
    ///
    /// Ingests values from ClothesSelection and changes outfit that is active
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 
    public class OutfitController : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(OutfitController);
            }
        }

        //An attempt to make a nested List. Doesn't work, needs to be fixed (see ChangeClothes method)
        [System.Serializable]
            public class serializableClass
            {
                public List<GameObject> outfit;
            }
            public List<serializableClass> outfits = new List<serializableClass>();
        public int activeOutfitIndex;

        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            activeOutfitIndex = 0;
        }

        /// <summary>
        /// SwitchOutfitForward
        /// 
        /// Checks to see if there outfit List contains an outfit, moves onto next if there are by calling ChangeClothes method
        /// Called on when right button is pressed under clothes UI in CharacterCustomizationMenu
        /// </summary> 
        public void SwitchOutfitForward()
        {
            if (outfits.Count > 1 && activeOutfitIndex < outfits.Count - 1)
            {
                activeOutfitIndex++;
            }
            else 
            {
                activeOutfitIndex = 0;
            }
        
            ChangeClothes();
        }

        /// <summary>
        /// SwitchOutfitBackward
        /// 
        /// Checks to see if there outfit List contains an outfit, moves onto previous if there are by calling ChangeClothes method
        /// Called on when left button is pressed under clothes UI in CharacterCustomizationMenu
        /// </summary> 
        public void SwitchOutfitBackward()
        {
            if (outfits.Count > 1)
            {
                if (activeOutfitIndex != 0)
                {
                    activeOutfitIndex--;
                }
                else
                {
                    activeOutfitIndex = outfits.Count - 1;
                }
            }
            else
            {
                activeOutfitIndex = 0;
            }

            ChangeClothes();
        }

        /// <summary>
        /// ChangeClothes
        /// 
        /// Deactivates all clothes GameObjects attached to all characters
        /// Activates specific group of clothes in outfit list attached to all characters (done by reference number)
        /// </summary> 
        //TODO: This method is unfinished. Need to find a way to access nest List
        void ChangeClothes()
        {
            /*foreach (serializableClass outfit in outfits)
            {
                foreach (GameObject clothingPiece in outfit)
                {
                    clothingPiece.SetActive(false); //disable all clothes
                }
            }
            foreach (GameObject clothingPiece in outfits[activeOutfitIndex])
            {
                clothingPiece.SetActive(true); //enable clothes in active outfit
            }*/
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

