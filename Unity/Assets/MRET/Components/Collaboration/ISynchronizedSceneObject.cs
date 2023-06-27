// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedSceneObject
	///
	/// An synchronization component of a scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedSceneObject : ISynchronized
	{
        /// <seealso cref="ISynchronized.SynchronizedObject"/>
        new public ISceneObject SynchronizedObject { get; }

        /// <seealso cref="ISynchronized.Initialize(IIdentifiable)"/>
        public void Initialize(ISceneObject synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedSceneObject
	///
	/// A <generic> synchronization component of a scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedSceneObject<T> : ISynchronized<T>, ISynchronizedSceneObject
        where T : ISceneObject
    {
    }
}
