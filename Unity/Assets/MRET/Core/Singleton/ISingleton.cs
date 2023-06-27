// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 25 Mar 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// ISingleton
	///
	/// A singleton in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISingleton
    {
        /// <summary>
        /// Called to initialize the manager
        /// </summary>
        public void Initialize();
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ISingleton
	///
	/// A <generic> singleton in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISingleton<T> : ISingleton
        where T : ISingleton
    {
    }

}
