// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserPointer
	///
	/// An synchronization component of a user hand component pointer in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUserPointer : ISynchronizedUserHandComponent
    {
        /// <seealso cref="ISynchronizedUserHandComponent.SynchronizedObject"/>
        new public IUserPointer SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedUserHandComponent.Initialize(IUserHandComponent)"/>
        public void Initialize(IUserPointer synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserPointer
	///
	/// A <generic> synchronization component of a user hand component pointer in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUserPointer<T> : ISynchronizedUserHandComponent<T>, ISynchronizedUserPointer
        where T : IUserPointer
    {
    }
}
