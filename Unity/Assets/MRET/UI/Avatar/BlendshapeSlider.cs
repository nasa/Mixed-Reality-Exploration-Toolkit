// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.UI.Avatar
{
    /// <remarks>
    /// History:
    /// 27 July 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// BlendshapeSlider
    ///
    /// References BlendController. Uses slider listener to change blendvalues
    ///
    /// Author: Caitlin E. Lian
    /// </summary>
    /// 

    public class BlendshapeSlider : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(BlendshapeSlider);

        //All Blendshape sliders under Morphs UI in CharacterCustomizationMenu
        public Slider _weight, _height, _legLength, _armLength;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure)
                    ? IntegrityState.Failure   // Fail if base class fails, OR required components are null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Initialize the slider values to their initial state
            _weight.value = MRET.ConfigurationManager.userPreferences.avatarWeight;
            _height.value = MRET.ConfigurationManager.userPreferences.avatarHeight;
            _legLength.value = MRET.ConfigurationManager.userPreferences.avatarLegLength;
            _armLength.value = MRET.ConfigurationManager.userPreferences.avatarArmLength;

            //Slider listeners below. Sliders are under Morphs UI of CharacterCustomizationMenu
            _weight.onValueChanged.AddListener((v) =>
            {
                SetWeight(v);
            });
            _height.onValueChanged.AddListener((v) =>
            {
                SetHeight(v);
            });
            _legLength.onValueChanged.AddListener((v) =>
            {
                SetLegLength(v);
            });
            _armLength.onValueChanged.AddListener((v) =>
            {
                SetArmLength(v);
            });
        }

        /// <summary>
        /// Sets the weight to the supplied value
        /// </summary>
        /// <param name="weight"></param>
        public void SetWeight(float weight)
        {
            MRET.AvatarManager.SetAvatarWeight(weight);
        }

        /// <summary>
        /// Sets the height to the supplied value
        /// </summary>
        /// <param name="height"></param>
        public void SetHeight(float height)
        {
            MRET.AvatarManager.SetAvatarHeight(height);
        }

        /// <summary>
        /// Sets the leg length to the supplied value
        /// </summary>
        /// <param name="legLength"></param>
        public void SetLegLength(float legLength)
        {
            MRET.AvatarManager.SetAvatarLegLength(legLength);
        }

        /// <summary>
        /// Sets the arm length to the supplied value
        /// </summary>
        /// <param name="armLength"></param>
        public void SetArmLength(float armLength)
        {
            MRET.AvatarManager.SetAvatarArmLength(armLength);
        }

    }
}