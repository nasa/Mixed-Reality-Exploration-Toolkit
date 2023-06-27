// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedDrawing
	///
	/// A synchronized component for an interactable drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedDrawing : ISynchronizedInteractable
    {
        /// <seealso cref="ISynchronizedInteractable.SynchronizedObject"/>
        new public IInteractableDrawing SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedInteractable.Initialize(IInteractable)"/>
        public void Initialize(IInteractableDrawing synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedDrawing
	///
	/// A <generic> synchronized component for an interactable drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedDrawing<T> : ISynchronizedInteractable<T>, ISynchronizedDrawing
        where T : IInteractableDrawing
    {
    }
}
