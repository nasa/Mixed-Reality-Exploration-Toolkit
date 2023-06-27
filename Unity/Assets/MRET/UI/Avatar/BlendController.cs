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
    /// BlendController
    ///
    /// Ingests values from BlendSlider and alters SkinnedMeshRenderers under CharacterMeshes
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 
    public class BlendController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(BlendController);
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


