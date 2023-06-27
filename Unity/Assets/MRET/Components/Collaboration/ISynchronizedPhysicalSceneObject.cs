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
	/// ISynchronizedPhysicalSceneObject
	///
	/// A synchronized component for a physical scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedPhysicalSceneObject : ISynchronizedInteractable
    {
        /// <seealso cref="ISynchronizedInteractable.SynchronizedObject"/>
        new public IPhysicalSceneObject SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedInteractable.Initialize(IInteractable)"/>
        public void Initialize(IPhysicalSceneObject synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedPhysicalSceneObject
	///
	/// A <generic> synchronized component for a physical scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedPhysicalSceneObject<T> : ISynchronizedInteractable<T>, ISynchronizedPhysicalSceneObject
        where T : IPhysicalSceneObject
    {
    }
}
