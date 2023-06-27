// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET.UI.Avatar
{
    /// <remarks>
    /// History:
    /// 27 July 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// ModelSelection
    ///
    /// References ModelController. Uses button listener to select character
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 
    public class ModelSelection : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(ModelSelection);
            }
        }

        private ModelController modelController;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (modelController == null)
                    ? IntegrityState.Failure   // Fail if base class fails, OR required components are null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            modelController = GetComponent<ModelController>();
        }

        /// <summary>
        /// SelectCharacter
        /// 
        /// Disables all base meshes and enables specific model via index character. Uses ModelController methods
        /// Called when model button is pressed under Appearances UI in CharacterCustomizationMenu
        /// </summary>
        public void SelectCharacter(int character)
        {
            if (modelController)
            {
                modelController.DisableAllModels();
                modelController.EnableModel(character);
                //this is just the index number
            }
        }
    }
}
