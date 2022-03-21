using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;
using GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing;

namespace GSFC.ARVR.MRET.Components.LineDrawing
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
		public override string ClassName
		{
			get
			{
				return nameof(DrawingEditController);
			}
		}

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
        public LineDrawing currentDrawing;

        /// <summary>
        /// Whether or not the menu is enabled.
        /// </summary>
        public bool menuEnabled { get; private set; }

        /// <summary>
        /// The current type.
        /// </summary>
        private LineDrawingManager.DrawingType currentType = LineDrawingManager.DrawingType.Basic;

        /// <summary>
        /// The current units.
        /// </summary>
        private LineDrawingManager.Units currentUnits = LineDrawingManager.Units.meters;

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
                switch (currentUnits)
                {
                    case LineDrawingManager.Units.meters:
                        return currentWidth;

                    case LineDrawingManager.Units.centimeters:
                        return currentWidth * 100;

                    case LineDrawingManager.Units.millimeters:
                        return currentWidth * 1000;

                    case LineDrawingManager.Units.micrometers:
                        return currentWidth * 1000000;

                    case LineDrawingManager.Units.nanometers:
                        return currentWidth * 1000000000;

                    case LineDrawingManager.Units.kilometers:
                        return currentWidth / 1000;

                    case LineDrawingManager.Units.inches:
                        return currentWidth * 39.3701f;

                    case LineDrawingManager.Units.feet:
                        return currentWidth * 3.28084f;

                    case LineDrawingManager.Units.yards:
                        return currentWidth * 1.09361f;

                    case LineDrawingManager.Units.miles:
                        return currentWidth / 1609.34f;

                    default:
                        Debug.LogError("[DrawingEditController->currentWidthInUnits] Invalid units.");
                        return -1;
                }
            }

            set
            {
                switch (currentUnits)
                {
                    case LineDrawingManager.Units.meters:
                        currentWidth = value;
                        break;

                    case LineDrawingManager.Units.centimeters:
                        currentWidth = value / 100;
                        break;

                    case LineDrawingManager.Units.millimeters:
                        currentWidth = value / 1000;
                        break;

                    case LineDrawingManager.Units.micrometers:
                        currentWidth = value / 1000000;
                        break;

                    case LineDrawingManager.Units.nanometers:
                        currentWidth = value / 1000000000;
                        break;

                    case LineDrawingManager.Units.kilometers:
                        currentWidth = value * 1000;
                        break;

                    case LineDrawingManager.Units.inches:
                        currentWidth = value / 39.3701f;
                        break;

                    case LineDrawingManager.Units.feet:
                        currentWidth = value / 3.28084f;
                        break;

                    case LineDrawingManager.Units.yards:
                        currentWidth = value / 1.09361f;
                        break;

                    case LineDrawingManager.Units.miles:
                        currentWidth = value * 1609.34f;
                        break;

                    default:
                        Debug.LogError("[DrawingEditController->currentWidthInUnits] Invalid units.");
                        return;
                }
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
                    case LineDrawingManager.Units.meters:
                        return "m";

                    case LineDrawingManager.Units.centimeters:
                        return "cm";

                    case LineDrawingManager.Units.millimeters:
                        return "mm";

                    case LineDrawingManager.Units.micrometers:
                        return "um";

                    case LineDrawingManager.Units.nanometers:
                        return "nm";

                    case LineDrawingManager.Units.kilometers:
                        return "km";

                    case LineDrawingManager.Units.inches:
                        return "in";

                    case LineDrawingManager.Units.feet:
                        return "ft";

                    case LineDrawingManager.Units.yards:
                        return "yd";

                    case LineDrawingManager.Units.miles:
                        return "mi";

                    default:
                        Debug.LogError("[DrawingEditController->unitsAbbreviation] Invalid units.");
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
                (Infrastructure.Framework.MRET.InputRig.head.transform.position
                - editButton.transform.position) * -1, Vector3.up);
            }
        }

        /// <summary>
        /// Initializes the drawing edit controller
        /// </summary>
        public void Initialize()
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[DrawingEditController->Initialize] No current drawing.");
                return;
            }
            currentWidth = currentDrawing.GetWidth();
            if (currentDrawing is VolumetricDrawing)
            {
                currentType = LineDrawingManager.DrawingType.Volumetric;
            }
            else
            {
                currentType = LineDrawingManager.DrawingType.Basic;
            }
            currentUnits = LineDrawingManager.Units.meters;
            ColorBlock cb = colorButton.colors;
            Color drawingColor = currentDrawing.GetColor();
            cb.normalColor = drawingColor;
            cb.disabledColor = drawingColor;
            colorButton.colors = cb;
            menuEnabled = true;
            grabToggle.isOn = false;
            SetGrabbing();
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
            if (menuEnabled)
            {
                return;
            }

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
                int numPoints = currentDrawing.numPoints;
                if (numPoints > 0)
                {
                    currentDrawing.EnableEndpointVisuals();
                }
                if (numPoints > 2)
                {
                    currentDrawing.EnableMiddlePointVisuals(
                        Mathf.Min(MAXPOINTVISUALS - 2, numPoints - 2));
                }
            }
            menuRoot.SetActive(true);
            menuRoot.transform.position = Vector3.Lerp(
                Infrastructure.Framework.MRET.InputRig.head.transform.position,
                menuRoot.transform.TransformPoint(currentDrawing.center), 0.9f);
            menuRoot.transform.rotation = Quaternion.LookRotation(
                (Infrastructure.Framework.MRET.InputRig.head.transform.position
                - menuRoot.transform.position) * -1, Vector3.up);
            menuEnabled = true;
        }

        /// <summary>
        /// Disable the menu.
        /// </summary>
        public void DisableMenu()
        {
            if (!menuEnabled)
            {
                return;
            }

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
                currentDrawing.DisableMiddlePointVisuals();
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
                Infrastructure.Framework.MRET.InputRig.transform.position,
                colorPalette.transform.TransformPoint(currentDrawing.center), 0.9f);
            colorPalette.transform.rotation = Quaternion.LookRotation(
                (Infrastructure.Framework.MRET.InputRig.head.transform.position
                - colorPalette.transform.position) * -1, Vector3.up);
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

            menuRoot.SetActive(true);
            colorPalette.SetActive(false);
        }

        /// <summary>
        /// Handle a width decrease.
        /// </summary>
        public void OnWidthDecrease()
        {
            SetWidth(Mathf.Max(Mathf.Round(currentWidthInUnits - 1), MINIMUMWIDTH));
        }

        /// <summary>
        /// Handle a width increase.
        /// </summary>
        public void OnWidthIncrease()
        {
            SetWidth(Mathf.Max(Mathf.Round(currentWidthInUnits + 1), MINIMUMWIDTH));
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
            currentDrawing.SetWidth(currentWidth);

            double roundedWidth = Math.Round(widthInUnits, 3);
            widthText.text = roundedWidth.ToString().Length > 4 ?
                roundedWidth.ToString("0.000E0") + unitsAbbreviation : roundedWidth + unitsAbbreviation;
        }

        /// <summary>
        /// Set the units.
        /// </summary>
        /// <param name="units">Units to use.</param>
        public void SetUnits(LineDrawingManager.Units units)
        {
            currentUnits = units;
            SetWidth(currentWidthInUnits);
            unitsText.text = currentUnits.ToString();
            currentDrawing.SetMeasurementUnits(currentUnits);
        }

        /// <summary>
        /// Increment the drawing type.
        /// </summary>
        private void IncrementType()
        {
            switch (currentType)
            {
                case LineDrawingManager.DrawingType.Basic:
                    SetType(LineDrawingManager.DrawingType.Volumetric);
                    widthLabelText.text = "Diameter";
                    break;

                case LineDrawingManager.DrawingType.Volumetric:
                    SetType(LineDrawingManager.DrawingType.Basic);
                    widthLabelText.text = "Width";
                    break;

                default:
                    Debug.LogError("[DrawingEditController->IncrementType] Invalid type.");
                    SetType(LineDrawingManager.DrawingType.Basic);
                    widthLabelText.text = "Width";
                    break;
            }
        }

        /// <summary>
        /// Set the drawing type.
        /// </summary>
        /// <param name="type">Drawing type to set.</param>
        private void SetType(LineDrawingManager.DrawingType type)
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[DrawingEditController->SetType] No current drawing.");
                return;
            }

            string name = currentDrawing.name;
            Guid uuid = currentDrawing.uuid;
            Color color = currentDrawing.GetColor();
            Vector3[] positions = currentDrawing.points;
            SceneObject parent = currentDrawing.parent;
            Vector3 position = currentDrawing.transform.localPosition;
            Quaternion rotation = currentDrawing.transform.localRotation;
            Vector3 scale = currentDrawing.transform.localScale;
            LineDrawingManager.Units units = currentUnits;
            float width = currentWidthInUnits;
            bool editMenuEnabled = menuEnabled;
            
            Infrastructure.Framework.MRET.SceneObjectManager.DestroySceneObject(uuid);
            LineDrawing newDrawing = Infrastructure.Framework.MRET.SceneObjectManager.CreateLineDrawing(name,
                parent, position, rotation, scale, type, currentWidth, color, positions, uuid);
            if (editMenuEnabled)
            {
                newDrawing.drawingEditController.EnableMenu();
            }
            else
            {
                newDrawing.drawingEditController.DisableMenu();
            }
            newDrawing.drawingEditController.SetMeasurementState();
            newDrawing.drawingEditController.currentUnits = units;
            newDrawing.SetMeasurementUnits(units);
            newDrawing.drawingEditController.SetWidth(width);
            newDrawing.drawingEditController.unitsText.text = units.ToString();
            newDrawing.drawingEditController.currentType = type;
            switch (type)
            {
                case LineDrawingManager.DrawingType.Basic:
                    newDrawing.drawingEditController.typeText.text = "Drawing";
                    break;

                case LineDrawingManager.DrawingType.Volumetric:
                    newDrawing.drawingEditController.typeText.text = "Cable";
                    break;

                default:
                    Debug.LogError("[DrawingEditController->SetType] Invalid type.");
                    break;
            }
        }

        /// <summary>
        /// Increment the units.
        /// </summary>
        private void IncrementUnits()
        {
            switch (currentUnits)
            {
                case LineDrawingManager.Units.meters:
                    currentUnits = LineDrawingManager.Units.kilometers;
                    break;

                case LineDrawingManager.Units.centimeters:
                    currentUnits = LineDrawingManager.Units.meters;
                    break;

                case LineDrawingManager.Units.millimeters:
                    currentUnits = LineDrawingManager.Units.centimeters;
                    break;

                case LineDrawingManager.Units.micrometers:
                    currentUnits = LineDrawingManager.Units.millimeters;
                    break;

                case LineDrawingManager.Units.nanometers:
                    currentUnits = LineDrawingManager.Units.micrometers;
                    break;

                case LineDrawingManager.Units.kilometers:
                    currentUnits = LineDrawingManager.Units.inches;
                    break;

                case LineDrawingManager.Units.inches:
                    currentUnits = LineDrawingManager.Units.feet;
                    break;

                case LineDrawingManager.Units.feet:
                    currentUnits = LineDrawingManager.Units.yards;
                    break;

                case LineDrawingManager.Units.yards:
                    currentUnits = LineDrawingManager.Units.miles;
                    break;

                case LineDrawingManager.Units.miles:
                    currentUnits = LineDrawingManager.Units.nanometers;
                    break;

                default:
                    Debug.LogError("[DrawingEditController->unitsAbbreviation] Invalid units.");
                    return;
            }
            SetWidth(currentWidthInUnits);
            unitsText.text = currentUnits.ToString();
            currentDrawing.SetMeasurementUnits(currentUnits);
        }

        /// <summary>
        /// Set the measurement state.
        /// </summary>
        private void SetMeasurementState()
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[DrawingEditController->SetMeasurementState] No current drawing.");
                return;
            }

            if (measurementStateToggle.isOn)
            {
                currentDrawing.EnableMeasurement();
            }
            else
            {
                currentDrawing.DisableMeasurement();
            }
        }

        /// <summary>
        /// Set grabbing.
        /// </summary>
        private void SetGrabbing()
        {
            if (currentDrawing == null)
            {
                Debug.LogError("[DrawingEditController->SetGrabbing] No current drawing.");
                return;
            }

            if (grabToggle.isOn)
            {
                currentDrawing.grabbable = true;
            }
            else
            {
                currentDrawing.grabbable = false;
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
                Debug.LogError("[DrawingEditController->SetColor] No current drawing.");
                return;
            }

            currentDrawing.SetColor(color);

            ColorBlock cb = colorButton.colors;
            cb.normalColor = color;
            cb.disabledColor = color;
            colorButton.colors = cb;
        }
	}
}