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
    /// ModelController
    ///
    /// Ingests values from ModelSelection and changes which model under CharacterMeshes is active
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 
    public class ModelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ModelController);
            }
        }

        [Tooltip("List of all the available character models")]
        // List of all the available base meshes (not including fallback)
        public List<GameObject> characters = new List<GameObject>();

        /// <summary>
        /// DisableAllModels
        /// 
        /// Finds all children under parent (CharacterMeshes). Sets the child as NOT active
        /// </summary> 
        public void DisableAllModels()
        {
            foreach (GameObject character in characters)
            {
                character.SetActive(false);
            }
        }

        /// <summary>
        /// EnableModel
        /// 
        /// Activates specific base character mesh in characters List via index. activeModel is an int set in ModelSelection script
        /// </summary> 
        public void EnableModel(int activeModel)
        {
            if ((activeModel >= 0) && (activeModel < characters.Count))
            {
                characters[activeModel].SetActive(true);
            }
            else
            {
                LogError("Supplied index is invalid for the available character model array: " + activeModel,
                    nameof(EnableModel));
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
                    ? IntegrityState.Failure   // Fail if base class fails, OR hands are null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }
    }

}
