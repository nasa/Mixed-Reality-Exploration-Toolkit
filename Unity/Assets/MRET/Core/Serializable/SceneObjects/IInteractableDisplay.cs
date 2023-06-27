// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractableDisplay
	///
	/// Represents an interactable display in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractableDisplay : IInteractable
	{
        /// <seealso cref="IInteractable.CreateSerializedType"/>
        new public DisplayType CreateSerializedType();

        /// <summary>
        /// The display title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The width of the display
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// The height of the display
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// The state of the display
        /// </summary>
        /// <seealso cref="DisplayStateType"/>
        public DisplayStateType State { get; }

        /// <summary>
        /// The Z order of the display
        /// </summary>
        public int Zorder { get; }

        /// <seealso cref="IInteractable.Deserialize(InteractableSceneObjectType, Action{bool, string})"/>
        public void Deserialize(DisplayType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractable.Serialize(InteractableSceneObjectType, Action{bool, string})"/>
        public void Serialize(DisplayType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractableDisplay
	///
	/// Represents a <generic> interactable display in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractableDisplay<T> : IInteractable<T>, IInteractableDisplay
        where T : DisplayType
    {
    }
}
