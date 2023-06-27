// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedPart
	///
	/// A synchronized component for a an interactable part in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedPart : ISynchronizedPhysicalSceneObject
    {
        /// <seealso cref="ISynchronizedPhysicalSceneObject.SynchronizedObject"/>
        new public InteractablePart SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedPhysicalSceneObject.Initialize(IPhysicalSceneObject)"/>
        public void Initialize(InteractablePart synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedPart
	///
	/// A <generic> synchronized component for an interactable part in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedPart<T> : ISynchronizedPhysicalSceneObject<T>, ISynchronizedPart
        where T : InteractablePart
    {
    }
}
