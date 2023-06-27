// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.ControllerMenu;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Marker;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 10 February 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// MarkerPrefabController
	///
	/// Controls the active marker prefab when instantiating markers
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class MarkerPrefabController : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(MarkerPrefabController);

        public ControllerMenuPanel markerPrefabPanel;

        public Text markerPrefabValueText;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Update the marker prefab value text to reflect the current marker prefab
            UpdateMarkerPrefabText();

            // Initialize the marker prefab panel and generate the prefab buttons
            MarkerManager markerManager = ProjectManager.MarkerManager;
            markerPrefabPanel.Initialize(false, false, false);
            for (int i = 0; i < markerManager.markerPrefabs.Count; i++)
            {
                MarkerPrefab markerPrefab = markerManager.markerPrefabs[i];
                Button button = markerPrefabPanel.AddButton(markerPrefab.name, markerPrefab.thumbnail, null,
                    markerPrefab.thumbnail != null, new Vector2(75, 75), ControllerMenuPanel.ButtonSize.Small);
                int delegateIndex = i;
                button.onClick.AddListener(delegate { UpdateActiveMarkerPrefab(delegateIndex); });
            }
        }

        private void UpdateMarkerPrefabText()
        {
            if (markerPrefabValueText != null)
            {
                MarkerPrefab markerPrefab = ProjectManager.MarkerManager.ActiveMarkerPrefab;
                if (markerPrefab != null)
                {
                    markerPrefabValueText.text = markerPrefab.name;
                }
            }
        }

        public void UpdateActiveMarkerPrefab(int index)
        {
            ProjectManager.MarkerManager.ActiveMarkerPrefabIndex = index;
            UpdateMarkerPrefabText();
        }
    }
}
