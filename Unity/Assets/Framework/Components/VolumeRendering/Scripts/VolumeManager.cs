// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace GSFC.ARVR.MRET.Components.VolumeRendering
{
    /// <remarks>
    /// History:
    /// 20 September 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// VolumeManager
	///
	/// The VolumeManager handles setting of MRET's
    /// rendering volume.
	///
    /// Author: Dylan Z. Baker
	/// </summary>
	/// 
	public class VolumeManager : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(VolumeManager);
			}
		}

        /// <summary>
        /// The volume profile to use within MRET.
        /// </summary>
        public VolumeProfile mretProfile;

		/// <seealso cref="MRETBehaviour.IntegrityCheck"/>
		protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure)  || (mretProfile == null)
				
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

        /// <summary>
        /// Set the sky to use a gradient.
        /// </summary>
        /// <param name="rBottom">Red component for bottom third.</param>
        /// <param name="gBottom">Green component for bottom third.</param>
        /// <param name="bBottom">Blue component for bottom third.</param>
        /// <param name="aBottom">Alpha component for bottom third.</param>
        /// <param name="rMiddle">Red component for middle third.</param>
        /// <param name="gMiddle">Green component for middle third.</param>
        /// <param name="bMiddle">Blue component for middle third.</param>
        /// <param name="aMiddle">Alpha component for middle third.</param>
        /// <param name="rTop">Red component for top third.</param>
        /// <param name="gTop">Green component for top third.</param>
        /// <param name="bTop">Blue component for top third.</param>
        /// <param name="aTop">Alpha component for top third.</param>
        public void SetGradientSky(float rBottom, float gBottom, float bBottom, float aBottom,
            float rMiddle, float gMiddle, float bMiddle, float aMiddle,
            float rTop, float gTop, float bTop, float aTop)
        {
            SetGradientSky(new Color(rBottom, gBottom, bBottom, aBottom),
                new Color(rMiddle, gMiddle, bMiddle, aMiddle),
                new Color(rTop, gTop, bTop, aTop));
        }

        /// <summary>
        /// Set the sky to use a gradient.
        /// </summary>
        /// <param name="bottom">Color for bottom third.</param>
        /// <param name="middle">Color for middle third.</param>
        /// <param name="top">Color for top third.</param>
        public void SetGradientSky(Color bottom, Color middle, Color top)
        {
            // Set MRET volume up.
            Volume mretVolume = GetMRETVolume();
            if (mretVolume == null)
            {
                Debug.LogError("[VolumeManager->SetGradientSky] Unable to get MRET Volume.");
                return;
            }
            mretVolume.profile = mretProfile;

            // Remove existing sky before setting up new one.
            RemoveSkyOverrides();

            // Set sky in visual environment.
            VisualEnvironment visualEnvironment = GetVisualEnvironment();
            visualEnvironment.skyType.overrideState = true;
            visualEnvironment.skyType.value = 3; // Because who needs enumerations?

            // Add gradient sky and set top/middle/bottom.
            GradientSky gradientSky = mretProfile.Add<GradientSky>(true);
            gradientSky.bottom.overrideState =
                gradientSky.middle.overrideState = gradientSky.top.overrideState = true;
            gradientSky.bottom.value = bottom;
            gradientSky.middle.value = middle;
            gradientSky.top.value = top;
        }

        /// <summary>
        /// Set the sky to use a solid color.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <param name="a">Alpha component.</param>
        public void SetSolidColorSky(float r, float g, float b, float a = 1)
        {
            SetSolidColorSky(new Color(r, g, b, a));
        }

        /// <summary>
        /// Set the sky to use a solid color.
        /// </summary>
        /// <param name="skyColor">Color to use.</param>
        public void SetSolidColorSky(Color skyColor)
        {
            SetGradientSky(skyColor, skyColor, skyColor);
        }

        /// <summary>
        /// Set the sky to use an HDRI cubemap.
        /// </summary>
        /// <param name="cubemap">Cubemap to use.</param>
        public void SetHDRISky(Cubemap cubemap)
        {
            // Set MRET volume up.
            Volume mretVolume = GetMRETVolume();
            if (mretVolume == null)
            {
                Debug.LogError("[VolumeManager->SetHDRISky] Unable to get MRET Volume.");
                return;
            }
            mretVolume.profile = mretProfile;

            // Remove existing sky before setting up new one.
            RemoveSkyOverrides();

            // Set sky in visual environment.
            VisualEnvironment visualEnvironment = GetVisualEnvironment();
            visualEnvironment.skyType.overrideState = true;
            visualEnvironment.skyType.value = 1; // Because who needs enumerations?

            // Add HDRI sky and set cubemap.
            HDRISky hdriSky = mretProfile.Add<HDRISky>(true);
            hdriSky.hdriSky.overrideState = true;
            hdriSky.hdriSky.value = cubemap;
        }

        /// <summary>
        /// Remove all volume overrides from the profile.
        /// </summary>
        public void RemoveAllVolumeOverrides()
        {
            mretProfile.components.Clear();
        }

        /// <summary>
        /// Remove all sky overrides from the profile.
        /// </summary>
        public void RemoveSkyOverrides()
        {
            mretProfile.components.RemoveAll(x => x.GetType() == typeof(GradientSky));
            mretProfile.components.RemoveAll(x => x.GetType() == typeof(HDRISky));
            mretProfile.components.RemoveAll(x => x.GetType() == typeof(PhysicallyBasedSky));
            mretProfile.components.RemoveAll(x => x.GetType() == typeof(ProceduralSky));
        }

        /// <summary>
        /// Gets the MRET volume. If not already created, one will be created.
        /// </summary>
        /// <returns>A reference to the MRET volume.</returns>
        private Volume GetMRETVolume()
        {
            Volume mretVolume = gameObject.GetComponent<Volume>();
            if (mretVolume == null)
            {
                mretVolume = gameObject.AddComponent<Volume>();
            }

            return mretVolume;
        }

        /// <summary>
        /// Gets the visual environment override. If not already
        /// created, one will be created.
        /// </summary>
        /// <returns>A reference to the visual environment.</returns>
        private VisualEnvironment GetVisualEnvironment()
        {
            VolumeComponent visualEnvironment =
                mretProfile.components.Find(x => x.GetType() == typeof(VisualEnvironment));
            if (visualEnvironment == null)
            {
                visualEnvironment = mretProfile.Add<VisualEnvironment>(true);
            }

            return (VisualEnvironment) visualEnvironment;
        }
	}
}