// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.Utilities.Color;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Marker;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 3 October 2022: Created (S. Letavish)
    /// 10 February 2023: Renamed and updated to work with the new MRET framework for 22.1 (J. Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// MarkerPanelController
    ///
    /// Controls the interaction of the UI components within the marker panel prefab
    ///
    /// Author: Sean Letavish
    /// </summary>
    /// 
    public class MarkerPanelController : MRETUpdateBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(MarkerPanelController);

        [Tooltip("Parent Panel. Part of the ObjectsSubmeu Prefab")]
        public GameObject MarkerPanel;

        [Tooltip("Array of pieces of ObjectsSubmenu that should turn off when MarkerPanel is active")]
        public GameObject[] ObjectSubMenuParts;

        [Tooltip("Toggle for Path Mode")]
        public Toggle pathModeToggle;

        [Tooltip("Text component displaying path mode")]
        public Text PathMode;

        [Tooltip("Prefab list panel")]
        public GameObject PrefabList;

        [Tooltip("Color list panel")]
        public GameObject ColorList;

        [Tooltip("Path settings panel")]
        public GameObject PathSettings;

        [Tooltip("Text displaying the currently selected marker type")]
        public Text currentMarkerText;

        [Tooltip("Button to set path type as volumetric")]
        public Button VolumetricPathButton;

        [Tooltip("Button to set path type as basic")]
        public Button BasicPathButton;

        [Tooltip("Text component showing what color pins and path will be")]
        public Text ColorText;

        [Tooltip("Slider setting path width")]
        public Slider PathWidthSlider;

        [Tooltip("Text component showing width of path")]
        public Text PathWidthText;

        [Tooltip("Button to enable the total path measurement")]
        public Button EnableTotalMeasurements;

        [Tooltip("Button to enable the path measurements between markers")]
        public Button EnableSegmentMeasurements;

        [Tooltip("Button to create a marker")]
        public Button CreateMarkerButton;

        private bool setPinEnabled = false;

        private Color32 Selected = new Color32(45, 255, 0, 255);
        private Color32 Unselected = new Color32(81, 105, 122, 255);

        public DrawingRender3dType PathType
        {
            get => ProjectManager.MarkerManager.ActiveMarkerPathRenderType;
            set
            {
                ProjectManager.MarkerManager.ActiveMarkerPathRenderType = value;
                VolumetricPathButton.image.color = (value == DrawingRender3dType.Volumetric) ? Selected : Unselected;
                BasicPathButton.image.color = (value == DrawingRender3dType.Basic) ? Selected : Unselected;
            }
        }

        public bool ShowTotalMeasurement
        {
            get => (ColorUtil.ColorRGBAEquals(Selected, EnableTotalMeasurements.image.color));
            set
            {
                ProjectManager.MarkerManager.ActiveMarkerPathDisplayTotalMeasurement = value;
                EnableTotalMeasurements.image.color = value ? Selected : Unselected;
            }
        }

        public bool ShowSegmentMeasurements
        {
            get => (ColorUtil.ColorRGBAEquals(Selected, EnableSegmentMeasurements.image.color));
            set
            {
                ProjectManager.MarkerManager.ActiveMarkerPathDisplaySegmentMeasurements = value;
                EnableSegmentMeasurements.image.color = value ? Selected : Unselected;
            }
        }

        private InteractableMarker markerInHand = null;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (MarkerPanel == null) ||
                (PathMode == null) ||
                (pathModeToggle == null) ||
                (PrefabList == null) ||
                (PathSettings == null) ||
                (currentMarkerText == null) ||
                (VolumetricPathButton == null) ||
                (BasicPathButton == null) ||
                (ColorText == null) ||
                (PathWidthSlider == null) ||
                (PathWidthText == null) ||
                (EnableTotalMeasurements == null) ||
                (ColorList == null) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure         // Fail is base class fails or anything is null
                    : IntegrityState.Success);       // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            if (PathWidthSlider)
            {
                // Listen for slider changes
                PathWidthSlider.onValueChanged.AddListener((v) =>
                {
                    SetPathWidth(v);
                });
            }
            else
            {
                LogError("Path slider reference is not set.", nameof(MRETStart));
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            setPinEnabled = ((markerInHand != null) && (markerInHand.IsPlacing));
            if (!setPinEnabled)
            {
                markerInHand = null;
                CreateMarkerButton.image.color = Unselected;
            }
        }

        private void OnEnable()
        {
            // Initialize the state
            CreateMarkerButton.image.color = Unselected;

            // Update the settings managed by the marker manager
            PathType = ProjectManager.MarkerManager.ActiveMarkerPathRenderType;
            SetPathWidth(ProjectManager.MarkerManager.ActiveMarkerPathWidth);
            ShowTotalMeasurement = ProjectManager.MarkerManager.ActiveMarkerPathDisplayTotalMeasurement;
            ShowSegmentMeasurements = ProjectManager.MarkerManager.ActiveMarkerPathDisplaySegmentMeasurements;
            pathModeToggle.isOn = ProjectManager.MarkerManager.MarkerPathActive;
            PathMode.text = ProjectManager.MarkerManager.MarkerPathActive ? "Enabled" : "Disabled";

            // Open the main panel
            Open();
        }

        /// <summary>
        /// Turns panel off and destroys the holographic tied to the controller
        /// </summary>
        public void Close()
        {
            if (ProjectManager.MarkerManager.MarkerPathActive)
            {
                ProjectManager.MarkerManager.EndPath();
            }
            markerInHand = null;
            setPinEnabled = false;
        }

        /// <summary>
        /// Opens the panel
        /// </summary>
        public void Open()
        {
            MarkerPanel.SetActive(true);
            PrefabList.SetActive(false);
            ColorList.SetActive(false);
            PathSettings.SetActive(false);
            foreach (GameObject panel in ObjectSubMenuParts)
            {
                panel.SetActive(false);
            }

            // Set the current marker type
            RefreshMarkerText();
        }

        /// <summary>
        /// Creates a marker
        /// </summary>
        public void CreateMarker()
        {
            // Delegate the instantiation action for markers
            Action<InteractableMarker> InstantiatedMarkerAction = (InteractableMarker instantiatedMarker) =>
            {
                if (instantiatedMarker != null)
                {
                    markerInHand = instantiatedMarker;
                    CreateMarkerButton.image.color = Selected;
                    setPinEnabled = true;
                }
                else
                {
                    LogError("A problem was encountered creating the marker");
                }
            };

            ProjectManager.MarkerManager.CreateActiveMarker(null, InstantiatedMarkerAction);
        }

        protected void RefreshMarkerText()
        {
            // Set the current marker type
            MarkerPrefab markerPrefab = ProjectManager.MarkerManager.ActiveMarkerPrefab;
            if (markerPrefab != null)
            {
                currentMarkerText.text = markerPrefab.name;
                currentMarkerText.color = ProjectManager.MarkerManager.ActiveMarkerColor;
            }
            else
            {
                currentMarkerText.text = "No Pin selected yet";
                currentMarkerText.color = Color.black;
            }
        }

        /// <summary>
        /// Enables pathmode. WORK IN PROGRESS
        /// </summary>
        public void PathmodeEnabled()
        {
            if (pathModeToggle.isOn)
            {
                ProjectManager.MarkerManager.StartPath();
            }
            else
            {
                ProjectManager.MarkerManager.EndPath();
            }

            PathMode.text = ProjectManager.MarkerManager.MarkerPathActive ? "Enabled" : "Disabled";
        }

        /// <summary>
        /// Turns on prefab panel
        /// </summary>
        public void ChangePrefab()
        {
            PathSettings.SetActive(false);
            MarkerPanel.SetActive(false);
            ColorList.SetActive(false);
            PrefabList.SetActive(true);
        }

        /// <summary>
        /// Turns on color panel
        /// </summary>
        public void ChangeColor()
        {
            PathSettings.SetActive(false);
            MarkerPanel.SetActive(false);
            ColorList.SetActive(true);
            PrefabList.SetActive(false);
        }

        /// <summary>
        /// Turns on path settings
        /// </summary>
        public void SetPathSettings()
        {
            PathSettings.SetActive(true);
            MarkerPanel.SetActive(false);
            ColorList.SetActive(false);
            PrefabList.SetActive(false);
        }

        public void SetPathTypeToBasic()
        {
            PathType = DrawingRender3dType.Basic;
        }

        public void SetPathTypeToVolumetric()
        {
            PathType = DrawingRender3dType.Volumetric;
        }

        public void SetPathWidth(float value)
        {
            ProjectManager.MarkerManager.ActiveMarkerPathWidth = value;

            if (PathWidthText)
            {
                PathWidthText.text = PathWidthSlider.value.ToString();
            }
        }

        public void MeasurementEnable()
        {
            ShowTotalMeasurement = true;
        }

        public void SegmentedMeasurement()
        {
            ShowSegmentMeasurements = true;
        }

        public void SetPinName()
        {
//            pinNameText.text = PinFileBrowserHelperDeprecated.selectedPinName;
        }

    }
}
