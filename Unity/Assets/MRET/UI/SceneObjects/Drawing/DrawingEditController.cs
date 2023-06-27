// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 28 December 2021: Created
    /// </remarks>
	/// <summary>
	/// DrawingEditController is a class that provides functionality for editing
    /// a LineDrawing.
    /// Author: Dylan Z. Baker
	/// </summary>
	public class DrawingEditController : MRETUpdateBehaviour
	{
        /// <summary>
        /// Minimum width that a line can have.
        /// </summary>
        private static readonly float MINIMUMWIDTH = 0.0001f;

        /// <summary>
        /// Maximum number of points to visualize.
        /// </summary>
        private static readonly int MAXPOINTVISUALS = 8;

		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(DrawingEditController);

        /// <summary>
        /// Button for enabling/disabling menu.
        /// </summary>
        [Tooltip("Button for enabling/disabling menu.")]
        public Button editButton;

        /// <summary>
        /// Color to apply to the edit button when menu is disabled.
        /// </summary>
        [Tooltip("Color to apply to the edit button when menu is disabled.")]
        public Color editMenuDisabledColor;

        /// <summary>
        /// Color to apply to the edit button when menu is enabled.
        /// </summary>
        [Tooltip("Color to apply to the edit button when menu is enabled.")]
        public Color editMenuEnabledColor;

        /// <summary>
        /// Elements to enable with the menu.
        /// </summary>
        [Tooltip("Elements to enable with the menu.")]
        public List<Selectable> menuEnabledElements = new List<Selectable>();

        /// <summary>
        /// Root of the menu.
        /// </summary>
        [Tooltip("Root of the menu.")]
        public GameObject menuRoot;

        /// <summary>
        /// The color palette.
        /// </summary>
        [Tooltip("The color palette.")]
        public GameObject colorPalette;

        /// <summary>
        /// Text for the width label.
        /// </summary>
        [Tooltip("Text for the width label.")]
        public TMP_Text widthLabelText;

        /// <summary>
        /// Text for the width.
        /// </summary>
        [Tooltip("Text for the width.")]
        public TMP_Text widthText;

        /// <summary>
        /// Text for the type.
        /// </summary>
        [Tooltip("Text for the type.")]
        public TMP_Text typeText;

        /// <summary>
        /// Text for the units.
        /// </summary>
        [Tooltip("Text for the units.")]
        public TMP_Text unitsText;

        /// <summary>
        /// Toggle for the measurement state.
        /// </summary>
        [Tooltip("Toggle for the measurement state.")]
        public Toggle measurementStateToggle;

        /// <summary>
        /// Toggle for the grabbing.
        /// </summary>
        [Tooltip("Toggle for the grabbing.")]
        public Toggle grabToggle;

        /// <summary>
        /// Button for loading color context menu.
        /// </summary>
        [Tooltip("Button for loading color context menu.")]
        public Button colorButton;

        /// <summary>
        /// The current drawing.
        /// </summary>
        [Tooltip("The current drawing.")]
        public IInteractable3dDrawing currentDrawing;

        /// <summary>
        /// Whether or not the menu is enabled.
        /// </summary>
        public bool menuEnabled { get; private set; }

        /// <summary>
        /// The current type.
        /// </summary>
        private DrawingRender3dType currentType = DrawingRender3dType.Basic;

        /// <summary>
        /// The current units.
        /// </summary>
        private LengthUnitType currentUnits = LengthUnitType.Meter;

        /// <summary>
        /// Current width (in meters).
        /// </summary>
        private float currentWidth = 1;

        /// <summary>
        /// Current width (in units defined by currentUnits).
        /// </summary>
        private float currentWidthInUnits
        {
            get
            {
                return SchemaUtil.UnityUnitsToLength(currentWidth, currentUnits);
            }
            set
            {
                currentWidth = Mathf.Max(SchemaUtil.LengthToUnityUnits(value, currentUnits), MINIMUMWIDTH);
            }
        }

        /// <summary>
        /// The units abbreviation.
        /// </summary>
        private string unitsAbbreviation
        {
            get
            {
                switch (currentUnits)
                {
                    case LengthUnitType.Meter:
                        return "m";

                    case LengthUnitType.Centimeter:
                        return "cm";

                    case LengthUnitType.Millimeter:
                        return "mm";

                    case LengthUnitType.Micrometer:
                        return "um";

                    case LengthUnitType.Nanometer:
                        return "nm";

                    case LengthUnitType.Kilometer:
                        return "km";

                    case LengthUnitType.Inch:
                        return "in";

                    case LengthUnitType.Foot:
                        return "ft";

                    case LengthUnitType.Yard:
                        return "yd";

                    case LengthUnitType.Mile:
                        return "mi";

                    default:
                        LogError("Invalid units.", nameof(unitsAbbreviation));
                        return "err";
                }
            }
        }

		/// <seealso cref="MRETBehaviour.IntegrityCheck"/>
		protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure || editButton == null
                || widthLabelText == null || widthText == null
                || typeText == null || unitsText == null
                || measurementStateToggle == null || colorButton == null)
				
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

        protected override void MRETAwake()
        {
            base.MRETAwake();

            updateRate = UpdateFrequency.Hz10;
        }

        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (editButton.enabled)
            {
                editButton.transform.rotation = Quaternion.LookRotation(
                    (MRET.InputRig.head.transform.position
                    - editButton.transform.position) * -1, Vector3.up);
            }
        }

        /// <summary>
        /// Initializes the drawing edit controller
        /// </summary>
        public void Initialize(bool editEnabled)
        {
            if (currentDrawing == null)
            {
                LogError("No current drawing.", nameof(Initialize));
                return;
            }

            RefreshState();

            menuEnabled = editEnabled;
        }

        /// <summary>
        /// Refrteshes the internal state with the current drawing
        /// </summary>
        public void RefreshState()
        {
            if (currentDrawing == null)
            {
                LogWarning("No current drawing assigned", nameof(RefreshState));
                return;
            }
            currentType = currentDrawing.RenderType;
            typeText.text = currentType.ToString();
            currentWidth = currentDrawing.width;
            currentUnits = currentDrawing.DesiredUnits;
            unitsText.text = currentUnits.ToString();
            ColorBlock cb = colorButton.colors;
            Color drawingColor = currentDrawing.Color;
            cb.normalColor = drawingColor;
            cb.disabledColor = drawingColor;
            colorButton.colors = cb;

            // Synchronize the toggle states
            measurementStateToggle.isOn = currentDrawing.DisplayMeasurement;
            grabToggle.isOn = currentDrawing.Grabbable;
            //SetGrabbing();
        }

        /// <summary>
        /// Shows the menu button.
        /// </summary>
        public void ShowButton()
        {
            editButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the menu button.
        /// </summary>
        public void HideButton()
        {
            editButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Enable the menu.
        /// </summary>
        public void EnableMenu()
        {
            ColorBlock cb = editButton.colors;
            cb.normalColor = editMenuEnabledColor;
            editButton.colors = cb;
            foreach (Selectable element in menuEnabledElements)
            {
                element.interactable = true;
                element.enabled = true;
            }
            if (currentDrawing != null)
            {
                int numPoints = currentDrawing.points.Length;
                if (numPoints > 0)
                {
                    currentDrawing.EnableEndpointVisuals();
                }
                if (numPoints > 2)
                {
                    currentDrawing.EnableMidpointVisuals(
                        Mathf.Min(MAXPOINTVISUALS - 2, numPoints - 2));
                }

                menuRoot.SetActive(true);
                menuRoot.transform.position = Vector3.Lerp(
                    MRET.InputRig.head.transform.position,
                    menuRoot.transform.TransformPoint(currentDrawing.center), 0.9f);
                menuRoot.transform.rotation = Quaternion.LookRotation(
                    (MRET.InputRig.head.transform.position
                    - menuRoot.transform.position) * -1, Vector3.up);
                menuEnabled = true;
            }
        }

        /// <summary>
        /// Disable the menu.
        /// </summary>
        public void DisableMenu()
        {
            ColorBlock cb = editButton.colors;
            cb.normalColor = editMenuDisabledColor;
            editButton.colors = cb;
            foreach (Selectable element in menuEnabledElements)
            {
                element.interactable = false;
                element.enabled = false;
            }
            if (currentDrawing != null)
            {
                currentDrawing.DisableEndpointVisuals();
                currentDrawing.DisableMidpointVisuals();
            }
            menuRoot.SetActive(false);
            colorPalette.SetActive(false);
            menuEnabled = false;
        }

        /// <summary>
        /// Toggles the menu.
        /// </summary>
        public void ToggleMenu()
        {
            if (menuEnabled)
            {
                DisableMenu();
            }
            else
            {
                EnableMenu();
            }
        }

        /// <summary>
        /// Enable the color palette.
        /// </summary>
        public void EnableColorPalette()
        {
            if (!menuEnabled)
            {
                return;
            }

            menuRoot.SetActive(false);
            colorPalette.SetActive(true);
            colorPalette.transform.position = Vector3.Lerp(
                MRET.InputRig.transform.position,
                colorPalette.transform.TransformPoint(currentDrawing.center), 0.9f);
            colorPalette.transform.rotation = Quaternion.LookRotation(
                (MRET.InputRig.head.transform.position
                - colorPalette.transform.position) * -1, Vector3.up);
        }

        /// <summary>
        /// Disable the color palette.
        /// </summary>
        public void DisableColorPalette()
        {
            if (!menuEnabled)
            {
                return;
            }

            menuRoot.SetActive(true);
            colorPalette.SetActive(false);
        }

        /// <summary>
        /// Set the color on the color palette.
        /// </summary>
        /// <param name="index">Index of the color.</param>
        public void SetPaletteColor(int index)
        {
            switch (index)
            {
                case 0:
                    // Red.
                    SetColor(Color.red);
                    break;

                case 1:
                    // Yellow.
                    SetColor(Color.yellow);
                    break;

                case 2:
                    // Blue.
                    SetColor(Color.blue);
                    break;

                case 3:
                    // Green.
                    SetColor(Color.green);
                    break;

                case 4:
                    // Purple.
                    SetColor(new Color32(169, 0, 255, 255));
                    break;

                case 5:
                    // Orange.
                    SetColor(new Color32(255, 62, 0, 255));
                    break;

                case 6:
                    // Black.
                    SetColor(Color.black);
                    break;

                case 7:
                    // White.
                    SetColor(Color.white);
                    break;

                case 8:
                    // Grey.
                    SetColor(Color.grey);
                    break;

                default:
                    // Red.
                    SetColor(Color.red);
                    break;
            }

            DisableColorPalette();
        }

        /// <summary>
        /// Handle a width decrease.
        /// </summary>
        public void OnWidthDecrease()
        {
            SetWidth(Mathf.Round(currentWidthInUnits - 1));
        }

        /// <summary>
        /// Handle a width increase.
        /// </summary>
        public void OnWidthIncrease()
        {
            SetWidth(Mathf.Round(currentWidthInUnits + 1));
        }

        /// <summary>
        /// Handle a type change.
        /// </summary>
        public void OnTypeChange()
        {
            IncrementType();
        }

        /// <summary>
        /// Handle a units change.
        /// </summary>
        public void OnUnitsChange()
        {
            IncrementUnits();
        }

        /// <summary>
        /// Handle a measurement state change.
        /// </summary>
        public void OnMeasurementStateChange()
        {
            SetMeasurementState();
        }

        /// <summary>
        /// Handle a grab toggle change.
        /// </summary>
        public void OnGrabChange()
        {
            SetGrabbing();
        }

        /// <summary>
        /// Unload the color menu.
        /// </summary>
        public void UnloadColorMenu()
        {

        }

        /// <summary>
        /// Handles a color change.
        /// </summary>
        public void OnColorChange(Color newColor)
        {
            SetColor(newColor);
        }

        /// <summary>
        /// Set the width.
        /// </summary>
        /// <param name="widthInUnits">Width in provided units to use.</param>
        public void SetWidth(float widthInUnits)
        {
            currentWidthInUnits = widthInUnits;
            currentDrawing.width = currentWidth;

            double roundedWidth = Math.Round(currentWidthInUnits, 3);
            widthText.text = roundedWidth.ToString().Length > 4 ?
                roundedWidth.ToString("0.000E0") + unitsAbbreviation : roundedWidth + unitsAbbreviation;
        }

        /// <summary>
        /// Set the units.
        /// </summary>
        /// <param name="units">Units to use.</param>
        public void SetUnits(LengthUnitType units)
        {
            currentUnits = units;
            SetWidth(currentWidthInUnits);
            if (unitsText != null)
            {
                unitsText.text = currentUnits.ToString();
            }
            if ((currentDrawing != null) && (currentDrawing.DesiredUnits != currentUnits))
            {
                currentDrawing.DesiredUnits = currentUnits;
            }
        }

        /// <summary>
        /// Increment the drawing type.
        /// </summary>
        private void IncrementType()
        {
            switch (currentType)
            {
                case DrawingRender3dType.Basic:
                    SetType(DrawingRender3dType.Volumetric);
                    widthLabelText.text = "Diameter";
                    break;

                case DrawingRender3dType.Volumetric:
                    SetType(DrawingRender3dType.Basic);
                    widthLabelText.text = "Width";
                    break;

                default:
                    LogError("Invalid type.", nameof(IncrementType));
                    SetType(DrawingRender3dType.Basic);
                    widthLabelText.text = "Width";
                    break;
            }
        }

        /// <summary>
        /// Set the drawing type.
        /// </summary>
        /// <param name="type">Drawing type to set.</param>
        private void SetType(DrawingRender3dType type)
        {
            if (currentDrawing == null)
            {
                LogError("No current drawing.", nameof(SetType));
                return;
            }

            string name = currentDrawing.name;
            Color color = currentDrawing.Color;
            Vector3[] positions = currentDrawing.points;
            GameObject parent = currentDrawing.parent?.gameObject;
            Vector3 position = currentDrawing.transform.localPosition;
            Quaternion rotation = currentDrawing.transform.localRotation;
            Vector3 scale = currentDrawing.transform.localScale;
            LengthUnitType units = currentUnits;
            float limitLength = currentDrawing.LengthLimit;
            bool showMeasurement = currentDrawing.DisplayMeasurement;
            bool editMenuEnabled = menuEnabled;

            // Destroy the current drawing
            Destroy(currentDrawing.gameObject);

            // Create a new drawing
            IInteractable3dDrawing newDrawing = ProjectManager.DrawingManager.CreateDrawing(name,
                parent, position, rotation, scale, type, currentWidth, color, positions,
                limitLength, showMeasurement);
            newDrawing.DesiredUnits = currentUnits;
            newDrawing.EditingActive = editMenuEnabled;

            // TODO: Need to copy over drawingeditcontroller settings
        }

        /// <summary>
        /// Increment the units.
        /// </summary>
        private void IncrementUnits()
        {
            switch (currentUnits)
            {
                case LengthUnitType.Meter:
                    currentUnits = LengthUnitType.Kilometer;
                    break;

                case LengthUnitType.Centimeter:
                    currentUnits = LengthUnitType.Meter;
                    break;

                case LengthUnitType.Millimeter:
                    currentUnits = LengthUnitType.Centimeter;
                    break;

                case LengthUnitType.Micrometer:
                    currentUnits = LengthUnitType.Millimeter;
                    break;

                case LengthUnitType.Nanometer:
                    currentUnits = LengthUnitType.Micrometer;
                    break;

                case LengthUnitType.Kilometer:
                    currentUnits = LengthUnitType.Inch;
                    break;

                case LengthUnitType.Inch:
                    currentUnits = LengthUnitType.Foot;
                    break;

                case LengthUnitType.Foot:
                    currentUnits = LengthUnitType.Yard;
                    break;

                case LengthUnitType.Yard:
                    currentUnits = LengthUnitType.Mile;
                    break;

                case LengthUnitType.Mile:
                    currentUnits = LengthUnitType.Nanometer;
                    break;

                default:
                    LogError("Invalid units.", nameof(IncrementUnits));
                    return;
            }
            currentDrawing.DesiredUnits = currentUnits;
            SetWidth(currentWidthInUnits);
            unitsText.text = currentUnits.ToString();
        }

        /// <summary>
        /// Set the measurement state.
        /// </summary>
        private void SetMeasurementState()
        {
            if (currentDrawing == null)
            {
                LogError("No current drawing.", nameof(SetMeasurementState));
                return;
            }

            currentDrawing.DisplayMeasurement = measurementStateToggle.isOn;
        }

        /// <summary>
        /// Set grabbing.
        /// </summary>
        private void SetGrabbing()
        {
            if (currentDrawing == null)
            {
                LogError("No current drawing.", nameof(SetGrabbing));
                return;
            }

            if (grabToggle.isOn)
            {
                currentDrawing.Grabbable = true;
            }
            else
            {
                currentDrawing.Grabbable = false;
            }
        }

        /// <summary>
        /// Sets the line color.
        /// </summary>
        /// <param name="color">Color to set the line to.</param>
        private void SetColor(Color color)
        {
            if (currentDrawing == null)
            {
                LogError("No current drawing.", nameof(SetColor));
                return;
            }

            currentDrawing.Color = color;

            ColorBlock cb = colorButton.colors;
            cb.normalColor = color;
            cb.disabledColor = color;
            colorButton.colors = cb;
        }
	}
}