// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
{
    //This interface guarentees a onTouch method for FingerTracker.cs to call when the user touches their finger to an object in AR
    //Author: Tyler Kitts
    public interface ITouchBehavior
    {
        bool OnTouch(Collider other);
    }
}