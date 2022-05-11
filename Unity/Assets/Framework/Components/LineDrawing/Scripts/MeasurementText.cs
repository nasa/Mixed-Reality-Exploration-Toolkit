// Copyright � 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using TMPro;
using GSFC.ARVR.MRET.Infrastructure.Framework.LineDrawing;

namespace GSFC.ARVR.MRET.Components.LineDrawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
	/// <summary>
	/// MeasurementText is a class for managing the display of LineDrawing measurements.
    /// Author: Dylan Z. Baker
	/// </summary>
	public class MeasurementText : MRETUpdateBehaviour
	{
        /// <summary>
        /// Measurement units.
        /// </summary>
        [Tooltip("Measurement units.")]
        public LineDrawingManager.Units units;

        /// <summary>
        /// Text field.
        /// </summary>
        [Tooltip("Text field.")]
        public TMP_Text text;

		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(MeasurementText);
			}
		}

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

        protected override void MRETAwake()
        {
            base.MRETAwake();
            updateRate = UpdateFrequency.Hz10;
        }

        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            transform.rotation = Quaternion.LookRotation(
                (Infrastructure.Framework.MRET.InputRig.head.transform.position
                - transform.position) * -1, Vector3.up);
        }

        /// <summary>
        /// Sets measurement text value.
        /// </summary>
        /// <param name="valueInMeters">Value to show in meters.</param>
        public void SetValue(float valueInMeters, float remaining)
        {
            switch (units)
            {
                case LineDrawingManager.Units.meters:
                    text.text = (valueInMeters.ToString().Length > 7 ?
                        valueInMeters.ToString("0.00E0") : valueInMeters.ToString()) + "m";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + (remaining.ToString().Length > 4 ?
                            remaining.ToString("0.00E0") : remaining.ToString()) + "m";
                    }
                    break;

                case LineDrawingManager.Units.centimeters:
                    text.text = ((valueInMeters * 100).ToString().Length > 7 ?
                        (valueInMeters * 100).ToString("0.00E0") : (valueInMeters * 100).ToString()) + "cm";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 100).ToString().Length > 4 ?
                            (remaining * 100).ToString("0.00E0") : (remaining * 100).ToString()) + "cm";
                    }
                    break;

                case LineDrawingManager.Units.millimeters:
                    text.text = ((valueInMeters * 1000).ToString().Length > 7 ?
                        (valueInMeters * 1000).ToString("0.00E0") : (valueInMeters * 1000).ToString()) + "mm";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 1000).ToString().Length > 4 ?
                            (remaining * 1000).ToString("0.00E0") : (remaining * 1000).ToString()) + "mm";
                    }
                    break;

                case LineDrawingManager.Units.micrometers:
                    text.text = ((valueInMeters * 1000000).ToString().Length > 7 ?
                        (valueInMeters * 1000000).ToString("0.00E0") : (valueInMeters * 1000000).ToString()) + "um";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 1000000).ToString().Length > 4 ?
                            (remaining * 1000000).ToString("0.00E0") : (remaining * 1000000).ToString()) + "um";
                    }
                    break;

                case LineDrawingManager.Units.nanometers:
                    text.text = ((valueInMeters * 1000000000).ToString().Length > 7 ?
                        (valueInMeters * 1000000000).ToString("0.00E0") : (valueInMeters * 1000000000).ToString()) + "nm";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 1000000000).ToString().Length > 4 ?
                            (remaining * 1000000000).ToString("0.00E0") : (remaining * 1000000000).ToString()) + "nm";
                    }
                    break;

                case LineDrawingManager.Units.kilometers:
                    text.text = ((valueInMeters / 1000).ToString().Length > 7 ?
                        (valueInMeters / 1000).ToString("0.00E0") : (valueInMeters / 1000).ToString()) + "km";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 1000000000).ToString().Length > 4 ?
                            (remaining / 1000).ToString("0.00E0") : (remaining * 1000000000).ToString()) + "km";
                    }
                    break;

                case LineDrawingManager.Units.inches:
                    text.text = ((valueInMeters * 39.3701).ToString().Length > 7 ?
                        (valueInMeters * 39.3701).ToString("0.00E0") : (valueInMeters * 39.3701).ToString()) + "in";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 39.3701).ToString().Length > 4 ?
                            (remaining * 39.3701).ToString("0.00E0") : (remaining * 39.3701).ToString()) + "in";
                    }
                    break;

                case LineDrawingManager.Units.feet:
                    text.text = ((valueInMeters * 3.28084).ToString().Length > 7 ?
                        (valueInMeters * 3.28084).ToString("0.00E0") : (valueInMeters * 3.28084).ToString()) + "ft";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 3.28084).ToString().Length > 4 ?
                            (remaining * 3.28084).ToString("0.00E0") : (remaining * 3.28084).ToString()) + "ft";
                    }
                    break;

                case LineDrawingManager.Units.yards:
                    text.text = ((valueInMeters * 1.09361).ToString().Length > 7 ?
                        (valueInMeters * 1.09361).ToString("0.00E0") : (valueInMeters * 1.09361).ToString()) + "yd";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining * 1.09361).ToString().Length > 4 ?
                            (remaining * 1.09361).ToString("0.00E0") : (remaining * 1.09361).ToString()) + "yd";
                    }
                    break;

                case LineDrawingManager.Units.miles:
                    text.text = ((valueInMeters / 1609.34).ToString().Length > 7 ?
                        (valueInMeters / 1609.34).ToString("0.00E0") : (valueInMeters / 1609.34).ToString()) + "mi";
                    if (remaining >= 0)
                    {
                        text.text = text.text + "\nRemaining: " + ((remaining / 1609.34).ToString().Length > 4 ?
                            (remaining / 1609.34).ToString("0.00E0") : (remaining / 1609.34).ToString()) + "mi";
                    }
                    break;

                default:
                    Debug.LogError("[MeasurementText->SetValue] Invalid units.");
                    return;
            }
        }
	}
}