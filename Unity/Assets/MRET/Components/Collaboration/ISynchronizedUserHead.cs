// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedHead
	///
	/// An synchronization component of a user head in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUserHead : ISynchronizedUserComponent
    {
        /// <seealso cref="ISynchronizedUserComponent.SynchronizedObject"/>
        new public IUserHead SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedUserComponent.Initialize(IUserComponent)"/>
        public void Initialize(IUserHead synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedHead
	///
	/// A <generic> synchronization component of a user head in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUserHead<T> : ISynchronizedUserComponent<T>, ISynchronizedUserHead
        where T : IUserHead
    {
    }
}
