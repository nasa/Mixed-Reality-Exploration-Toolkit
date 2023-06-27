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
	/// ISynchronizedUserHand
	///
	/// An synchronization component of a user hand in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUserHand : ISynchronizedUserComponent
    {
        /// <seealso cref="ISynchronizedUserComponent.SynchronizedObject"/>
        new public IUserHand SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedUserComponent.Initialize(IUserComponent)"/>
        public void Initialize(IUserHand synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserHand
	///
	/// A <generic> synchronization component of a user hand in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUserHand<T> : ISynchronizedUserComponent<T>, ISynchronizedUserHand
        where T : IUserHand
    {
    }
}
