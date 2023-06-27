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
	/// ISynchronizedInteractable
	///
	/// An synchronization component of an interactable scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedInteractable : ISynchronizedSceneObject
	{
        /// <seealso cref="ISynchronizedSceneObject.SynchronizedObject"/>
        new public IInteractable SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedSceneObject.Initialize(ISceneObject)"/>
        public void Initialize(IInteractable synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedInteractable
	///
	/// A <generic> synchronization component of an interactable scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedInteractable<T> : ISynchronizedSceneObject<T>, ISynchronizedInteractable
        where T : IInteractable
    {
    }
}
