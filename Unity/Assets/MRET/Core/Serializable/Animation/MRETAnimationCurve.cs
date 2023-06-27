// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET;
using System;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// MRETAnimationCurve
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public class MRETAnimationCurve : AnimationCurve
	{
        public MRETAnimationCurve(params Keyframe[] keys) : base(keys) { }

        public new static MRETAnimationCurve Linear(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new MRETAnimationCurve(
                AnimationCurve.Linear(timeStart, valueStart, timeEnd, valueEnd).keys);
        }
    }
}
