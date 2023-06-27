// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using TMPro;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing
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
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MeasurementText);

        /// <summary>
        /// Measurement units.
        /// </summary>
        [Tooltip("Measurement units.")]
        public LengthUnitType units;

        /// <summary>
        /// Text field.
        /// </summary>
        [Tooltip("Text field.")]
        public TMP_Text text;

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
                (MRET.InputRig.head.transform.position
                - transform.position) * -1, Vector3.up);
        }

        /// <summary>
        /// Sets measurement text value.
        /// </summary>
        /// <param name="valueInMeters">Value in meters to display in the desired units.</param>
        /// <param name="remaining">Remaining value in meters to display in the desired units.</param>
        public void SetValue(float valueInMeters, float remaining)
        {
            float convertedValue = SchemaUtil.UnityUnitsToLength(valueInMeters, units);
            float convertedRemaining = SchemaUtil.UnityUnitsToLength(remaining, units);

            string unitAbbr = "m";
            switch (units)
            {
                case LengthUnitType.Meter:
                    unitAbbr = "m";
                    break;

                case LengthUnitType.Centimeter:
                    unitAbbr = "cm";
                    break;

                case LengthUnitType.Millimeter:
                    unitAbbr = "mm";
                    break;

                case LengthUnitType.Micrometer:
                    unitAbbr = "um";
                    break;

                case LengthUnitType.Nanometer:
                    unitAbbr = "nm";
                    break;

                case LengthUnitType.Kilometer:
                    unitAbbr = "km";
                    break;

                case LengthUnitType.Inch:
                    unitAbbr = "in";
                    break;

                case LengthUnitType.Foot:
                    unitAbbr = "ft";
                    break;

                case LengthUnitType.Yard:
                    unitAbbr = "yd";
                    break;

                case LengthUnitType.Mile:
                    unitAbbr = "mi";
                    break;
            }

            // Build the measurement text
            text.text = (convertedValue.ToString().Length > 7 ?
                convertedValue.ToString("0.00E0") : convertedValue.ToString()) + unitAbbr;
            if (!float.IsInfinity(convertedRemaining) && !float.IsNaN(convertedRemaining) && (convertedRemaining >= 0))
            {
                text.text += "\nRemaining: " + (convertedRemaining.ToString().Length > 4 ?
                    convertedRemaining.ToString("0.00E0") : convertedRemaining.ToString()) + unitAbbr;
            }

        }
    }
}