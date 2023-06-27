// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractable
	///
	/// Defines an interactable scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractable : ISceneObject
	{
        /// <seealso cref="ISceneObject.CreateSerializedType"/>
        new public InteractableSceneObjectType CreateSerializedType();

        /// <summary>
        /// The parent <code>IInteractable</code> of this <code>IInteractable</code>.<br>
        /// </summary>
        public IInteractable interactableParent { get; }

        /// <summary>
        /// The children <code>IInteractable</code> of this <code>IInteractable</code>.<br>
        /// </summary>
        public IInteractable[] interactableChildren { get; }

        /// <summary>
        /// Whether or not the physical scene is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// The opacity of the physical scene object. 0 is transparent, 255 is opaque.
        /// </summary>
        public byte Opacity { get; set; }

        /// <summary>
        /// Whether or not the interactable is usable.
        /// </summary>
        public bool Usable { get; set; }

        #region Materials
        /// <summary>
        /// Material to use for highlighting.
        /// </summary>
        public Material HighlightMaterial { get; set; }

        /// <summary>
        /// Material to use for selecting.
        /// </summary>
        public Material SelectionMaterial { get; set; }
        #endregion Materials

        #region Touching
        /// <summary>
        /// Behavior to use on touch.
        /// </summary>
        public enum TouchBehaviors { Highlight, Hold, Custom }

        /// <summary>
        /// Whether or not the interactable is being touched.
        /// </summary>
        public bool IsTouching { get; }

        /// <summary>
        /// Behavior to use on touch.
        /// </summary>
        public TouchBehaviors TouchBehavior { get; set; }

        /// <summary>
        /// Begin the process of touching the <code>IInteractable</code>
        /// </summary>
        /// <param name="hand">The <code>InputHand</code> performing the touch</param>
        /// 
        /// <see cref="InputHand"/> 
        public void BeginTouch(InputHand hand);

        /// <summary>
        /// End the process of touching the <code>IInteractable</code>
        /// </summary>
        public void EndTouch();
        #endregion Touching

        #region Grabbing
        /// <summary>
        /// Behavior to use on grab.
        /// </summary>
        public enum GrabBehaviors { Attach, Constrained, Custom }

        /// <summary>
        /// Whether or not the interactable is grabbable.
        /// </summary>
        public bool Grabbable { get; set; }

        /// <summary>
        /// Whether or not the interactable is being grabbed.
        /// </summary>
        public bool IsGrabbing { get; }

        /// <summary>
        /// Behavior to use on grab.
        /// </summary>
        public GrabBehaviors GrabBehavior { get; set; }

        /// <summary>
        /// Begin the process of grabbing the <code>IInteractable</code>
        /// </summary>
        /// <param name="hand">The <code>InputHand</code> performing the grab</param>
        /// 
        /// <see cref="InputHand"/> 
        public void BeginGrab(InputHand hand);

        /// <summary>
        /// End the process of grabbing the <code>IInteractable</code>
        /// </summary>
        /// <param name="hand">The <code>InputHand</code> performing the grab</param>
        /// 
        /// <see cref="InputHand"/> 
        public void EndGrab(InputHand hand);
        #endregion Grabbing

        #region Placing
        /// <summary>
        /// Whether or not the interactable is being placed.
        /// </summary>
        public bool IsPlacing { get; }

        /// <summary>
        /// Begins the process of placing the <code>IInteractable</code>
        /// </summary>
        /// <param name="placingParent">The <code>GameObject</code> parent</param>
        public void BeginPlacing(GameObject placingParent = null);

        /// <summary>
        /// End the process of placing the <code>IInteractable</code>
        /// </summary>
        public void EndPlacing();
        #endregion Placing

        #region Selection
        /// <summary>
        /// Selects the interactable
        /// </summary>
        /// <param name="hierarchical">Indicates whether the entire hierachy should be selected. Default is true.</param>
        public void Select(bool hierarchical = true);

        /// <summary>
        /// Deselects the interactable
        /// </summary>
        /// <param name="hierarchical">Indicates whether the entire hierachy should be deselected. Default is true.</param>
        public void Deselect(bool hierarchical = true);
        #endregion Selection

        #region Using
        /// <summary>
        /// Use performed by the provided hand.
        /// </summary>
        /// <param name="hand">Hand that performed the use.</param>
        public void Use(InputHand hand);

        /// <summary>
        /// Unuse performed by the provided hand.
        /// </summary>
        /// <param name="hand">Hand that performed the unuse.</param>
        public void Unuse(InputHand hand);
        #endregion Using

        #region Telemetry
        /// <summary>
        /// The data points associated with this interactable
        /// </summary>
        public string[] DataPoints { get; }

        /// <summary>
        /// Adds a telemetry data point to the interactable
        /// </summary>
        /// <param name="dataPoint">The telemetry data point to add</param>
        /// <returns>Indicates successful addition</returns>
        public bool AddDataPoint(string dataPoint);

        /// <summary>
        /// Removes a telemetry data point from the interactable
        /// </summary>
        /// <param name="dataPoint">The telemetry data point to remove</param>
        /// <returns>Indicates sucessful removal</returns>
        public bool RemoveDataPoint(string dataPoint);

        /// <summary>
        /// Indicates whether the interactable should be shaded for limit violations
        /// </summary>
        public bool ShadeForLimitViolations { get; set; }
        #endregion Telemetry

        /// <seealso cref="ISceneObject.Synchronize(SceneObjectType, Action{bool, string})"/>
        public void Synchronize(InteractableSceneObjectType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObject.Deserialize(SceneObjectType, Action{bool, string})"/>
        public void Deserialize(InteractableSceneObjectType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="ISceneObject.Serialize(SceneObjectType, Action{bool, string})"/>
        public void Serialize(InteractableSceneObjectType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IInteractable
	///
	/// Defines a <generic> interactable scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractable<T> : ISceneObject<T>, IInteractable
        where T : InteractableSceneObjectType
    {
    }
}
