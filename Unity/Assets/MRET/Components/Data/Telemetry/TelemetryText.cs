// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Globalization;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Data.Telemetry
{
    /// <summary>
    /// TelemetryText
    /// 
    /// Extends Telemetry to convert the telemetry value into a formatted string.<br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TelemetryText : Telemetry
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryText);

        [Tooltip("The optional telemetry string format. If not specified, a formatting will not be applied to the resultant string.")]
        public string format = "";

        // Accessor for the telemetry value as text
        public string Text { get => GetText(Value); }

        /// <summary>
        /// Obtains the format provider for the supplied telemetry point value.
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The <code>IFormatProvider</code> that will be used to define the formatting.
        ///      Default is <code>CultureInfo.CurrentCulture</code></returns>
        /// <seealso cref="IFormatProvider"/>
        /// <seealso cref="CultureInfo.CurrentCulture"/>
        protected virtual IFormatProvider GetValueFormatProvider(object value)
        {
            return CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Obtains the text representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The string representation of the telemetry value</returns>
        protected virtual string GetText(object value)
        {
            string result = "";

            if (value is IFormattable)
            {
                // Format the value
                IFormatProvider formatProvider = GetValueFormatProvider(value);
                result = (value as IFormattable).ToString(format, formatProvider);
            }
            else if (value != null)
            {
                // No formatting
                result = value.ToString();
            }

            return result;
        }

    }
}
