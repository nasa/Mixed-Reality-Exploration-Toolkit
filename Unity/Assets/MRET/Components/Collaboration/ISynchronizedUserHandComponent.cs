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
	/// ISynchronizedUserHandComponent
	///
	/// An synchronization component of a user hand component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUserHandComponent : ISynchronizedUserComponent
    {
        /// <seealso cref="ISynchronizedUserComponent.SynchronizedObject"/>
        new public IUserHandComponent SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedUserComponent.Initialize(IUserComponent)"/>
        public void Initialize(IUserHandComponent synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserHandComponent
	///
	/// A <generic> synchronization component of a user hand component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUserHandComponent<T> : ISynchronizedUserComponent<T>, ISynchronizedUserHandComponent
        where T : IUserHandComponent
    {
    }
}
