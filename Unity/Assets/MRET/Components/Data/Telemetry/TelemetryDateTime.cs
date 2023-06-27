// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GOV.NASA.GSFC.XR.MRET.Data.Telemetry
{
    /// <summary>
    /// TelemetryDateTime
    /// 
    /// Extends Telemetry to convert the telemetry value into a DateTime result. If
    /// the telemetry cannot be converted to a DateTime, the DateTime default value
    /// will be used.<br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    /// <see cref="DateTime"/>
    public class TelemetryDateTime : Telemetry
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryDateTime);

        // Accessor for the telemetry DateTime
        public DateTime DateTimeValue { get => GetDateTime(Value); }

        // Accessor for the year component of the DateTime telemetry
        public int Year { get => GetYear(Value); }

        // Accessor for the month component of the DateTime telemetry
        public int Month { get => GetMonth(Value); }

        // Accessor for the day component of the DateTime telemetry
        public int Day { get => GetDay(Value); }

        // Accessor for the hour component of the DateTime telemetry
        public int Hour { get => GetHour(Value); }

        // Accessor for the minute component of the DateTime telemetry
        public int Minute { get => GetMinute(Value); }

        // Accessor for the second component of the DateTime telemetry
        public int Second { get => GetSecond(Value); }

        // Accessor for the number of ticks representing the DateTime telemetry
        public long Ticks { get => GetTicks(Value); }

        /// <summary>
        /// Obtains the telemetry value as a DateTime. If the value cannot be converted to a DateTime,
        /// the default DateTime value will be returned.<br>
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The <code>DateTime</code> representation telemetry value</returns>
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

        /// <summary>
        /// Obtains the year component of the DateTime telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The int representation of the year component of the DateTime telemetry value</returns>
        protected virtual int GetYear(object value)
        {
            return GetDateTime(value).Year;
        }

        /// <summary>
        /// Obtains the month of the DateTime telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The int representation of the month component of the DateTime telemetry value</returns>
        protected virtual int GetMonth(object value)
        {
            return GetDateTime(value).Month;
        }

        /// <summary>
        /// Obtains the day of the DateTime telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The int representation of the day component of the DateTime telemetry value</returns>
        protected virtual int GetDay(object value)
        {
            return GetDateTime(value).Day;
        }

        /// <summary>
        /// Obtains the hour component of the DateTime telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The int representation of the hour component of the DateTime telemetry value</returns>
        protected virtual int GetHour(object value)
        {
            return GetDateTime(value).Hour;
        }

        /// <summary>
        /// Obtains the minutes component of the DateTime telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The int representation of the minutes component of the DateTime telemetry value</returns>
        protected virtual int GetMinute(object value)
        {
            return GetDateTime(value).Minute;
        }

        /// <summary>
        /// Obtains the seconds component of the DateTime telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The int representation of the seconds component of the DateTime telemetry value</returns>
        protected virtual int GetSecond(object value)
        {
            return GetDateTime(value).Second;
        }

        /// <summary>
        /// Obtains the number of ticks representing the DateTime telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The long representation of the number of ticks representing the DateTime telemetry value</returns>
        protected virtual long GetTicks(object value)
        {
            return GetDateTime(value).Ticks;
        }

    }
}
