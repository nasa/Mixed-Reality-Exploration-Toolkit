// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedNote
	///
	/// A synchronized component for an interactable note in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedNote : ISynchronizedDisplay
    {
        /// <seealso cref="ISynchronizedInteractable.SynchronizedObject"/>
        new public InteractableNote SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedDisplay.Initialize(IInteractableDisplay)"/>
        public void Initialize(InteractableNote synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedNote
	///
	/// A <generic> synchronized component for an interactable note in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedNote<T> : ISynchronizedDisplay<T>, ISynchronizedNote
        where T : InteractableNote
    {
    }
}
