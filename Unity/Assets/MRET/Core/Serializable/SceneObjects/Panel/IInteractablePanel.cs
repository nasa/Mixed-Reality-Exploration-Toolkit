// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Panel
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractablePanel
	///
	/// Represents an interactable panel in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractablePanel : IInteractableDisplay
    {
        /// <summary>
        /// The feed source for this panel
        /// </summary>
        /// <seealso cref="FeedSource"/>
        public FeedSource feedSource { get; }

        /// <seealso cref="IInteractableDisplay.CreateSerializedType"/>
        new public PanelType CreateSerializedType();

        /// <seealso cref="IInteractableDisplay.Deserialize(DisplayType, Action{bool, string})"/>
        public void Deserialize(PanelType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractableDisplay.Serialize(DisplayType, Action{bool, string})"/>
        public void Serialize(PanelType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractablePanel
	///
	/// Represents a <generic> interactable panel in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractablePanel<T> : IInteractableDisplay<T>, IInteractablePanel
        where T : PanelType
    {
    }
}
