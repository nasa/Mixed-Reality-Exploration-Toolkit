// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET.Schema
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// IUpgradable
	///
	/// TODO: Describe this interface here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public interface IUpgradable
	{
        public object Upgrade();
	}

    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// IUpgradable
	///
	/// TODO: Describe this interface here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public interface IUpgradable<T> : IUpgradable
    {
        new public T Upgrade();
    }
}
