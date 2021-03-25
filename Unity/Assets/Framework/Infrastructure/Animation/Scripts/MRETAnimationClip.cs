// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Reflection;
using UnityEngine;
using GSFC.ARVR.MRET;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// MRETAnimationClip
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public class MRETAnimationClip
	{
        private AnimationClip animationClip = new AnimationClip();

        public AnimationClip AnimationClip { get => animationClip; protected set => animationClip = value; }
        public string Name { get => animationClip.name; set => animationClip.name = value; }

        public void SetCurve(string relativePath, Type type, string propertyName, AnimationCurve curve)
        {
            animationClip.SetCurve(relativePath, type, propertyName, curve);
            animationClip.legacy = true;
        }

        public void SampleAnimation(GameObject go, float time)
        {
            //Debug.Log("[MRETAnimationClip].SampleAnimation: time:" + time);
            animationClip.SampleAnimation(go, time);
        }
    }
}
