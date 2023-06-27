// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 26 Mar 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// MRETManager
	///
	/// Provides an abstract implementation of the singleton management for
    /// MRET managers that DO NOT require update behavior.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="MRETUpdateManager{M}"/>
	/// 
	public abstract class MRETManager<M> : MRETSingleton<M>, IManager<M>
        where M : IManager
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(MRETManager<M>);
    }

    /// <remarks>
    /// History:
    /// 26 Mar 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// MRETUpdateManager
	///
	/// Provides an abstract implementation of the singleton management for
    /// MRET managers requiring update behavior.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="MRETManager{M}"/>
	/// 
	public abstract class MRETUpdateManager<M> : MRETUpdateSingleton<M>, IManager<M>
        where M : IManager
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETManager<M>);
    }
}
