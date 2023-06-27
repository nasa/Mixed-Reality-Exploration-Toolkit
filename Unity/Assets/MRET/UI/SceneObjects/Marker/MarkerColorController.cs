// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.ControllerMenu;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 11 February 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// MarkerPrefabController
	///
	/// Controls the active marker color when instantiating markers
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class MarkerColorController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MarkerColorController);

        public ControllerMenuPanel markerColorPanel;

        public Text colorValueText;

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

            // Update the color value text to reflect the current color
            UpdateColorValueText();

            // Initialize the color panel and generate the color buttons
            markerColorPanel.Initialize(false, false, false);
            var colorTypes = Enum.GetValues(typeof(ColorPredefinedType)).Cast<ColorPredefinedType>();
            foreach (ColorPredefinedType colorType in colorTypes)
            {
                string colorName = Enum.GetName(typeof(ColorPredefinedType), colorType);
                Color32 delegateColor = Color.black;
                SchemaUtil.DeserializeColorPredefined(colorType, ref delegateColor);

                // Create the button
                Button button = markerColorPanel.AddButton(colorName, null, null, false, new Vector2(50, 50), ControllerMenuPanel.ButtonSize.Small);
                Image buttonImage = button.GetComponentInChildren<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = delegateColor;
                }
                Text buttonText = button.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.color = Color.black;
                    buttonText.text = colorName;
                }
                button.onClick.AddListener(delegate { UpdateMarkerColor(delegateColor); });
            }
        }

        private void UpdateColorValueText()
        {
            if (colorValueText != null)
            {
                ColorPredefinedType colorType = ColorPredefinedType.Black;
                Color32 currentColor = ProjectManager.MarkerManager.ActiveMarkerColor;
                string colorName;
                if (SchemaUtil.SerializeColorPredefined(currentColor, ref colorType))
                {
                    colorName = Enum.GetName(typeof(ColorPredefinedType), colorType);
                }
                else
                {
                    ColorComponentsType serializedColor = new ColorComponentsType();
                    SchemaUtil.SerializeColorComponents(currentColor, serializedColor);
                    colorName = 
                        "R: 0x" + currentColor.r.ToString("X2") + "; " +
                        "G: 0x" + currentColor.g.ToString("X2") + "; " +
                        "B: 0x" + currentColor.b.ToString("X2");
                }
                colorValueText.text = colorName;
                colorValueText.color = currentColor;
            }
        }

        public void UpdateMarkerColor(Color32 color)
        {
            ProjectManager.MarkerManager.ActiveMarkerColor = color;
            UpdateColorValueText();
        }
    }
}
