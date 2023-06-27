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
	/// ISynchronizedUser
	///
	/// An synchronization component of a user in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUser : ISynchronizedSceneObject
    {
        /// <seealso cref="ISynchronizedSceneObject.SynchronizedObject"/>
        new public IUser SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedSceneObject.Initialize(ISceneObject)"/>
        public void Initialize(IUser synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUser
	///
	/// A <generic> synchronization component of a user in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUser<T> : ISynchronizedSceneObject<T>, ISynchronizedUser
        where T : IUser
    {
    }
}
