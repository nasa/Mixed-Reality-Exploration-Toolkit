// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GSFC.ARVR.MRET.Telemetry
{
    /**
     * TelemetryDateTime
     * 
     * Extends Telemetry to convert the telemetry value into a DateTime result. If
     * the telemetry cannot be converted to a DateTime, the DateTime default value
     * will be used.<br>
     * 
     * @see DateTime
     * 
     * @author Jeffrey Hosler
     */
    public class TelemetryDateTime : Telemetry
    {
        public new static readonly string NAME = nameof(TelemetryDateTime);

        // Accessor for the telemetry DateTime
        public DateTime DateTimeValue
        {
            get
            {
                return GetDateTime(Value);
            }
        }

        // Accessor for the year component of the DateTime telemetry
        public int Year
        {
            get
            {
                return GetYear(Value);
            }
        }

        // Accessor for the month component of the DateTime telemetry
        public int Month
        {
            get
            {
                return GetMonth(Value);
            }
        }

        // Accessor for the day component of the DateTime telemetry
        public int Day
        {
            get
            {
                return GetDay(Value);
            }
        }

        // Accessor for the hour component of the DateTime telemetry
        public int Hour
        {
            get
            {
                return GetHour(Value);
            }
        }

        // Accessor for the minute component of the DateTime telemetry
        public int Minute
        {
            get
            {
                return GetMinute(Value);
            }
        }

        // Accessor for the second component of the DateTime telemetry
        public int Second
        {
            get
            {
                return GetSecond(Value);
            }
        }

        // Accessor for the number of ticks representing the DateTime telemetry
        public long Ticks
        {
            get
            {
                return GetTicks(Value);
            }
        }

        /**
         * Obtains the telemetry value as a DateTime. If the value cannot be converted to a DateTime,
         * the default DateTime value will be returned.<br>
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The <code>DateTime</code> representation telemetry value
         */
        protected virtual DateTime GetDateTime(object value)
        {
            DateTime result = default(DateTime);

            if (value is DateTime)
            {
                result = (DateTime)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                DateTime.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the year component of the DateTime telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The int representation of the year component of the DateTime telemetry value
         */
        protected virtual int GetYear(object value)
        {
            return GetDateTime(value).Year;
        }

        /**
         * Obtains the month of the DateTime telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The int representation of the month component of the DateTime telemetry value
         */
        protected virtual int GetMonth(object value)
        {
            return GetDateTime(value).Month;
        }

        /**
         * Obtains the day of the DateTime telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The int representation of the day component of the DateTime telemetry value
         */
        protected virtual int GetDay(object value)
        {
            return GetDateTime(value).Day;
        }

        /**
         * Obtains the hour component of the DateTime telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The int representation of the hour component of the DateTime telemetry value
         */
        protected virtual int GetHour(object value)
        {
            return GetDateTime(value).Hour;
        }

        /**
         * Obtains the minutes component of the DateTime telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The int representation of the minutes component of the DateTime telemetry value
         */
        protected virtual int GetMinute(object value)
        {
            return GetDateTime(value).Minute;
        }

        /**
         * Obtains the seconds component of the DateTime telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The int representation of the seconds component of the DateTime telemetry value
         */
        protected virtual int GetSecond(object value)
        {
            return GetDateTime(value).Second;
        }

        /**
         * Obtains the number of ticks representing the DateTime telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The long representation of the number of ticks representing the DateTime telemetry value
         */
        protected virtual long GetTicks(object value)
        {
            return GetDateTime(value).Ticks;
        }

    }
}
