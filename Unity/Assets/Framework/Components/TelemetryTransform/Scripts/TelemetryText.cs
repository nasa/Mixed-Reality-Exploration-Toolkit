// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Globalization;
using UnityEngine;

namespace GSFC.ARVR.MRET.Telemetry
{
    /**
     * TelemetryText
     * 
     * Extends Telemetry to convert the telemetry value into a formatted string.<br>
     * 
     * @author Jeffrey Hosler
     */
    public class TelemetryText : Telemetry
    {
        public new static readonly string NAME = nameof(TelemetryText);

        [Tooltip("The optional telemetry string format. If not specified, a formatting will not be applied to the resultant string.")]
        public string format = "";

        // Accessor for the telemetry value as text
        public string Text
        {
            get
            {
                return GetText(Value);
            }
        }

        /**
         * Obtains the format provider for the supplied telemetry point value.
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The <code>IFormatProvider</code> that will be used to define the formatting.
         *      Default is <code>CultureInfo.CurrentCulture</code>
         * 
         * @see IFormatProvider
         * @see CultureInfo.CurrentCulture
         */
        protected virtual IFormatProvider GetValueFormatProvider(object value)
        {
            return CultureInfo.CurrentCulture;
        }

        /**
         * Obtains the text representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The string representation of the telemetry value
         */
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
