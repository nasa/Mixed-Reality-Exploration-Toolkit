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
	/// ISynchronizedUserTorso
	///
	/// An synchronization component of a user torso in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUserTorso : ISynchronizedUserComponent
    {
        /// <seealso cref="ISynchronizedUserComponent.SynchronizedObject"/>
        new public IUserTorso SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedUserComponent.Initialize(IUserComponent)"/>
        public void Initialize(IUserTorso synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserTorso
	///
	/// A <generic> synchronization component of a user torso in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUserTorso<T> : ISynchronizedUserComponent<T>, ISynchronizedUserTorso
        where T : IUserTorso
    {
    }
}
