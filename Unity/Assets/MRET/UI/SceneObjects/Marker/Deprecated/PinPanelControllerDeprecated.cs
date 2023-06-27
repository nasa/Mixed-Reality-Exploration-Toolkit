// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Pin
{
    public class PinPanelControllerDeprecated : MRETBehaviour
    {

        /// <remarks>
        /// History:
        /// 3 October 2022: Created
        /// </remarks>
        ///
        /// <summary>
        /// PinPanelController
        ///
        /// Controls the interaction of the UI components within the pin manager
        ///
        /// Author: Sean Letavish
        /// </summary>
        /// 

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(PinPanelControllerDeprecated);
            }
        }

        [Tooltip("Parent Panel. Part of the ObjectsSubmeu Prefab")]
        public GameObject PinPanel;

        [Tooltip("Array of pieces of ObjectsSubmenu that should turn off when PinPanel is active")]
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

        [Tooltip("Text displaying what pin is selected")]
        public Text pinNameText;

        [Tooltip("Button to set pin prefab")]
        public Button PinButton;

        [Tooltip("Button to set flag prefab")]
        public Button FlagButton;

        [Tooltip("Button to set path type as volumetric")]
        public Button Volumetric;

        [Tooltip("Button to set path type as basic")]
        public Button Basic;

        [Tooltip("Prefab for holographic pin")]
        public GameObject PinHolo;

        [Tooltip("Prefab for holographic flag")]
        public GameObject FlagHolo;

        [Tooltip("Text component showing what color pins and path will be")]
        public Text ColorText;

        [Tooltip("Slider setting path size")]
        public Slider PathSizeSlider;

        [Tooltip("Text component showing size of path")]
        public Text PathSizeText;

        [Tooltip("Button to enable measurements")]
        public Button EnableMeasurements;

        public Button segmentedMeasurements;

        public Button placePin;

        public static bool modelInHand = false;
        public static bool setPinEnabled = false;
        Color GreenButton = new Color(45f, 255f, 0f, 255f);
        Color BlueButton = new Color(81f, 105f, 122f, 255f);
        Vector3 rightup = new Vector3(-90f, 0, 0);


        private bool _ignoreEvents = false;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (PinPanel == null) ||
                (PathMode == null) ||
                (pathModeToggle == null) ||
                (PrefabList == null) ||
                (PathSettings == null) ||
                (pinNameText == null) ||
                (PinButton == null) ||
                (FlagButton == null) ||
                (Volumetric == null) ||
                (Basic == null) ||
                (ColorText == null) ||
                (PathSizeSlider == null) ||
                (PathSizeText == null) ||
                (EnableMeasurements == null) ||
                (ColorList == null) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure         // Fail is base class fails or anything is null
                    : IntegrityState.Success);       // Otherwise, our integrity is valid
        }

        protected override void MRETStart()
        {
            Close();
            segmentedMeasurements.image.color = BlueButton;
            placePin.image.color = BlueButton;

            if (PathSizeSlider)
            {
                // Listen for slider changes
                PathSizeSlider.onValueChanged.AddListener((v) =>
                {
                    SetPathScale(v);
                });
            }
            else
            {
                LogError("Path slider reference is not set.", nameof(MRETStart));
            }

            EnableMeasurements.image.color = BlueButton;
            pinNameText.text = "No Pin selected yet";
        }

        /// <summary>
        /// Turns panel off and destroys the holographic tied to the controller
        /// </summary>
        public void Close()
        {
            PinPanel.SetActive(false);
            PrefabList.SetActive(false);
            ColorList.SetActive(false);
            PathSettings.SetActive(false);
            foreach (GameObject panel in ObjectSubMenuParts)
            {
                panel.SetActive(true);
            }
            modelInHand = false;
            ProjectManager.PinManagerDeprecated.DestroyHoloPin();
            setPinEnabled = false;
            placePin.image.color = BlueButton;
            pinNameText.text = "No Pin selected yet";
        }

        /// <summary>
        /// Opens the panel
        /// </summary>
        public void Open()
        {
            PinPanel.SetActive(true);
            foreach (GameObject panel in ObjectSubMenuParts)
            {
                panel.SetActive(false);
            }
        }

        /// <summary>
        /// Instantiates the holographic pin or flag to the hand
        /// </summary>
        public void CreatePin()
        {
            if (MRET.InputRig.GetComponentInChildren<PinMarkerDeprecated>() != null && placePin.image.color == BlueButton)
            {
                placePin.image.color = GreenButton;
                setPinEnabled = true;
            }

            else
            {
                placePin.image.color = BlueButton;
                setPinEnabled = false;
            }
        }

        public void Back()
        {
            PathSettings.SetActive(false);
            PrefabList.SetActive(false);
            ColorList.SetActive(false);
            PinPanel.SetActive(true);
        }

        /// <summary>
        /// Enables pathmode. WORK IN PROGRESS
        /// </summary>
        public void pathmodeEnabled()
        {
            if (pathModeToggle.isOn)
            {
                PathMode.text = "Enabled";
                ProjectManager.PinManagerDeprecated.CreateNewPath();
            }

            else
            {
                PathMode.text = "Disabled";
                ProjectManager.PinManagerDeprecated.EndPath();
            }
        }

        /// <summary>
        /// Turns on prefab panel
        /// </summary>
        public void ChangePrefab()
        {
            PathSettings.SetActive(false);
            PinPanel.SetActive(false);
            ColorList.SetActive(false);
            PrefabList.SetActive(true);
        }

        /// <summary>
        /// Turns on color panel
        /// </summary>
        public void ChangeColor()
        {
            PathSettings.SetActive(false);
            PinPanel.SetActive(false);
            ColorList.SetActive(true);
            PrefabList.SetActive(false);
        }

        /// <summary>
        /// Turns on path settings
        /// </summary>
        public void SetPathSettings()
        {
            PathSettings.SetActive(true);
            PinPanel.SetActive(false);
            ColorList.SetActive(false);
            PrefabList.SetActive(false);
        }

        public void SetWhite()
        {
            ColorText.text = "White";
            ProjectManager.PinManagerDeprecated.SetWhite();
        }

        public void SetGreen()
        {
            ColorText.text = "Green";
            ProjectManager.PinManagerDeprecated.SetGreen();
        }

        public void SetBlue()
        {
            ColorText.text = "Blue";
            ProjectManager.PinManagerDeprecated.SetBlue();
        }

        public void SetRed()
        {
            ColorText.text = "Red";
            ProjectManager.PinManagerDeprecated.SetRed();
        }

        public void DeleteLastPin()
        {
            ProjectManager.PinManagerDeprecated.DeleteLastPin();
        }

        public void SetPathType()
        {
            ProjectManager.PinManagerDeprecated.SetPathType();

            if (ProjectManager.PinManagerDeprecated.PathTypeVolumetric)
            {
                Volumetric.image.color = GreenButton;
                Basic.image.color = BlueButton;
            }

            else
            {
                Volumetric.image.color = BlueButton;
                Basic.image.color = GreenButton;
            }
        }

        public void SetPathScale(float value)
        {
            if (_ignoreEvents) return;
            ProjectManager.PinManagerDeprecated.PathLineSize = value * 0.05f;

            if (PathSizeText)
            {
                PathSizeText.text = PathSizeSlider.value.ToString();
            }
        }

        public void MeasurementEnable()
        {
            if (EnableMeasurements.image.color == BlueButton)
            {
                EnableMeasurements.image.color = GreenButton;
                ProjectManager.PinManagerDeprecated.DisplayMeasurements();
            }

            else
            {
                EnableMeasurements.image.color = BlueButton;
                ProjectManager.PinManagerDeprecated.DisableMeasurements();
            }
        }

        public void SegmentedMeasurement()
        {
            if (segmentedMeasurements.image.color == BlueButton)
            {
                segmentedMeasurements.image.color = GreenButton;
                ProjectManager.PinManagerDeprecated.segmentedMeasurementEnabled = true;
            }

            else
            {
                segmentedMeasurements.image.color = BlueButton;
                ProjectManager.PinManagerDeprecated.segmentedMeasurementEnabled = false;
            }
        }

        public void SetPinName()
        {
            pinNameText.text = PinFileBrowserHelperDeprecated.selectedPinName;
        }
    }
}