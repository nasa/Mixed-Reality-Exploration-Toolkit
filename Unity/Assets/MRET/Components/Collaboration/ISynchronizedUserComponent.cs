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
	/// ISynchronizedUserComponent
	///
	/// An synchronization component of a user component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUserComponent : ISynchronizedSceneObject
    {
        /// <seealso cref="ISynchronizedSceneObject.SynchronizedObject"/>
        new public IUserComponent SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedSceneObject.Initialize(ISceneObject)"/>
        public void Initialize(IUserComponent synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserComponent
	///
	/// A <generic> synchronization component of a user component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUserComponent<T> : ISynchronizedSceneObject<T>, ISynchronizedUserComponent
        where T : IUserComponent
    {
    }
}
