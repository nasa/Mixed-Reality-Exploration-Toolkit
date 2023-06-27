// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Data.Telemetry
{
    /**
    /// TelemetryBoolean
    /// 
    /// Extends Telemetry to convert the telemetry value into a boolean result.<br>
    /// 
    /// @author Jeffrey Hosler
     */
    public class TelemetryBoolean : Telemetry
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryBoolean);

        // Accessor for the telemetry value as a boolean
        public bool BooleanValue { get => GetBoolean(Value); }

        /// <summary>
        /// Obtains the boolean representation of the telemetry value
        /// </summary>
        /// <param name="value">An<code>object</code> representing the telemetry value</param>
        /// <returns>The boolean representation of the telemetry value</returns>
        protected virtual bool GetBoolean(object value)
        {
            bool result = false;

            if (value is bool)
            {
                result = (bool)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                bool.TryParse(value.ToString(), out result);
            }

            return result;
        }
    }
}
