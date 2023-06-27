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
	/// ISynchronizedDisplay
	///
	/// A synchronized component for an interactable display in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedDisplay : ISynchronizedInteractable
    {
        /// <seealso cref="ISynchronizedInteractable.SynchronizedObject"/>
        new public IInteractableDisplay SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedInteractable.Initialize(IInteractable)"/>
        public void Initialize(IInteractableDisplay synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedDisplay
	///
	/// A <generic> synchronized component for an interactable display in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedDisplay<T> : ISynchronizedInteractable<T>, ISynchronizedDisplay
        where T : IInteractableDisplay
    {
    }
}
