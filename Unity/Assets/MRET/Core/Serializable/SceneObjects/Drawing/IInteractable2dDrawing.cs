// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractable2dDrawing
	///
	/// Represents an interactable 2D drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractable2dDrawing : IInteractableDrawing
	{
        /// <seealso cref="IInteractableDrawing.CreateSerializedType"/>
        new public Drawing2dType CreateSerializedType();

        /// <seealso cref="IInteractableDrawing.Deserialize(DrawingType, Action{bool, string})"/>
        public void Deserialize(Drawing2dType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractableDrawing.Serialize(DrawingType, Action{bool, string})"/>
        public void Serialize(Drawing2dType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractable2dDrawing
	///
	/// Represents a <generic> interactable 2D drawing in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractable2dDrawing<T> : IInteractableDrawing<T>, IInteractable2dDrawing
        where T : Drawing2dType
    {
    }
}
