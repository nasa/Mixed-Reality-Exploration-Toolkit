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
	/// IManager
	///
	/// A manager in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IManager : ISingleton
	{
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IManager
	///
	/// A <generic> manager in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IManager<M> : ISingleton<M>, IManager
        where M : IManager
    {
    }

}
