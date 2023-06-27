// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
{
    /// <remarks>
    /// History:
    /// 3 Mar 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// IControllerModel
	///
	/// A controller model in the CPIS
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IControllerModel
    {
        /// <summary>
        /// Called to initialize the controller model
        /// </summary>
        /// <param name="hand">The <code>InputHand</code> associated with the controller</param>
        public void Initialize(InputHand hand);

    }
}
