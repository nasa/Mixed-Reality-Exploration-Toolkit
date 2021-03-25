// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.MRET.Telemetry
{
    /**
     * TelemetryBoolean
     * 
     * Extends Telemetry to convert the telemetry value into a boolean result.<br>
     * 
     * @author Jeffrey Hosler
     */
    public class TelemetryBoolean : Telemetry
    {
        public new static readonly string NAME = nameof(TelemetryBoolean);

        // Accessor for the telemetry value as a boolean
        public bool BooleanValue
        {
            get
            {
                return GetBoolean(Value);
            }
        }

        /**
         * Obtains the boolean representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The boolean representation of the telemetry value
         */
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
