// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects
{
    /// <remarks>
    /// History:
    /// 20 February 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// IInteractableConfigurationPanelController
	///
	/// Describes an interactable configuration panel controller
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IInteractableConfigurationPanelController
	{
        /// <summary>
        /// The interactable scene object being configured by this controller.
        /// </summary>
        public IInteractable ConfiguringInteractable { get; }

        /// <summary>
        /// The title of the panel associated with this controller.
        /// </summary>
        public string PanelTitle { get; }

        /// <summary>
        /// Called to initialize this controller.
        /// </summary>
        /// <param name="configuringInteractable">The <code>IInteractable</code> being configured
        ///     by this controller.</param>
        /// <param name="panelTitle">The optional title of the panel associated with this controller.
        ///     If not specified, the configuring interactable name will be used.</param>
        public void Initialize(IInteractable configuringInteractable, string panelTitle = null);

    }
}
