// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CSPICE.LunarModel
{
    public enum DMSFormat { Degrees, DegreesMinutes, DegreesMinutesSeconds };
    public enum DMSDirection { Degrees, DegreesMinutes, DegreesMinutesSeconds };
    public enum DMSOrientation { None, Latitude, Longitude};

    public class Coordinates
    {
        public const string DMS_SEPARATOR = " ";
        //public const char DEGREES_CHAR = '°';
        public const char DEGREES_CHAR = 'd';

        /**
         * Calculates the destination latitude/longitude given a position, bearing and distance.<br>
         * 
         * @param lon1 The starting position longitude in degrees
         * @param lat1 The starting position latitude in degrees
         * @param bearing The bearing in degrees
         * @param distance The distance in meters
         * 
         * @param [Out] lon The destination position longitude in degrees
         * @param [Out] lat The destination position latitude in degrees
         */
        public static void GetDestination(double lon1, double lat1, double bearing, double distance, out double lon, out double lat)
        {
            // TODO: Allow radius to be specified
            double R = LunarModel.MOON_RADIUS * 1000.0; // meters
            double φ1 = LunarModel.ToRadians(lat1);
            double λ1 = LunarModel.ToRadians(lon1);
            bearing = LunarModel.ToRadians(bearing);

            double φ2 = Math.Asin(Math.Sin(φ1) * Math.Cos(distance / R) +
                Math.Cos(φ1) * Math.Sin(distance / R) * Math.Cos(bearing));
            double λ2 = λ1 + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance / R) * Math.Cos(φ1),
                Math.Cos(distance / R) - Math.Sin(φ1) * Math.Sin(φ2));

            lat = LunarModel.ToDegrees(φ2);
            lon = (LunarModel.ToDegrees(λ2) + 540) % 360 - 180; // Normalize to [-180,180]
        }

        /**
         * Calculates the midpoint latitude/longitude given two positions.<br>
         * 
         * @param lon1 The first position longitude in degrees
         * @param lat1 The first position latitude in degrees
         * @param lon2 The second position longitude in degrees
         * @param lat2 The second position latitude in degrees
         * 
         * @param [Out] lon The midpoint position longitude in degrees
         * @param [Out] lat The midpoint position latitude in degrees
         */
        public static void GetMidpoint(double lon1, double lat1, double lon2, double lat2, out double lon, out double lat)
        {
            double φ1 = LunarModel.ToRadians(lat1);
            double φ2 = LunarModel.ToRadians(lat2);
            double λ1 = LunarModel.ToRadians(lon1);
            double λ2 = LunarModel.ToRadians(lon2);
            double Δλ = LunarModel.ToRadians(λ2 - λ1);

            double Bx = Math.Cos(φ2) * Math.Cos(Δλ);
            double By = Math.Cos(φ2) * Math.Sin(Δλ);

            double φ3 = Math.Atan2(Math.Sin(φ1) + Math.Sin(φ2),
                Math.Sqrt((Math.Cos(φ1) + Bx) * (Math.Cos(φ1) + Bx) + By * By));
            double λ3 = λ1 + Math.Atan2(By, Math.Cos(φ1) + Bx);

            lat = LunarModel.ToDegrees(φ3);
            lon = (LunarModel.ToDegrees(λ3) + 540) % 360 - 180; // Normalize to [-180,180]
        }

        /**
         * Calculates the bearing of a destination position from a starting position.<br>
         * 
         * @param lon1 The starting position longitude in degrees
         * @param lat1 The starting position latitude in degrees
         * @param lon2 The destination position longitude in degrees
         * @param lat2 The destination position latitude in degrees
         * 
         * @return The bearing in degrees
         */
        public static double GetBearing(double lon1, double lat1, double lon2, double lat2)
        {
            double φ1 = LunarModel.ToRadians(lat1);
            double φ2 = LunarModel.ToRadians(lat2);
            double Δλ = LunarModel.ToRadians(lon2 - lon1);

            double y = Math.Sin(Δλ) * Math.Cos(φ2);
            double x = (Math.Cos(φ1) * Math.Sin(φ2)) -
                (Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(Δλ));

            return (LunarModel.ToDegrees(Math.Atan2(y, x)) + 360) % 360; // Normalize to [0-360]
        }

        /**
         * Calculates the distance between two positions.<br>
         * 
         * @param lon1 The first position longitude in degrees
         * @param lat1 The first position latitude in degrees
         * @param lon2 The second position longitude in degrees
         * @param lat2 The second position latitude in degrees
         * 
         * @return The distance in meters
         */
        public static double GetDistance(double lon1, double lat1, double lon2, double lat2)
        {
            // TODO: Allow radius to be specified
            double R = LunarModel.MOON_RADIUS * 1000.0; // meters
            double φ1 = LunarModel.ToRadians(lat1);
            double φ2 = LunarModel.ToRadians(lat2);
            double Δφ = LunarModel.ToRadians(lat2 - lat1);
            double Δλ = LunarModel.ToRadians(lon2 - lon1);

            double a = (Math.Sin(Δφ / 2.0) * Math.Sin(Δφ / 2.0)) +
                (Math.Cos(φ1) * Math.Cos(φ2) *
                 Math.Sin(Δλ / 2.0) * Math.Sin(Δλ / 2.0));
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        /**
         * Parses a degrees/minutes/seconds string into decimal degrees.
         *
         * This is very flexible on formats, allowing signed decimal degrees, or deg-min-sec optionally
         * suffixed by compass direction (NSEW); a variety of separators are accepted. Examples -3.62,
         * '3 37 12W', '3°37′12″W'.
         *
         * Thousands/decimal separators must be comma/dot
         *
         * @param dms A string containing the degrees/minutes/seconds in variety of formats
         * 
         * @return The decimal degrees
         *
         * @example
         *   lat = Coordinates.ToDegrees("51° 28′ 40.37N");
         *   lon = Coordinates.ToDegrees("000° 00′ 05.29W");
         */
        public static double ToDegrees(string dms)
        {
            double result;

            // Check for signed decimal degrees without NSEW, if so return it directly
            bool isNumber = double.TryParse(dms, out result);
            if (!double.IsNaN(result) && isNumber) return result;

            // Strip off any sign or compass direction & split out separate d/m/s
            Regex signsRegex = new Regex(@"^-", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex compassRegex = new Regex(@"[NSEW]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex dmsRegex = new Regex(@"[^0-9.,]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Process the input dms
            string tmpDms = dms.Trim();
            tmpDms = signsRegex.Replace(tmpDms, "");
            tmpDms = compassRegex.Replace(tmpDms, "");
            string[] dmsParts = dmsRegex.Split(tmpDms);
            if (dmsParts[dmsParts.Length - 1] == "")
            {
                // from trailing symbol
                List<string> tmp = new List<string>(dmsParts);
                tmp.RemoveAt(dmsParts.Length - 1);
                dmsParts = tmp.ToArray();
            }

            if (dmsParts.Length == 0) return double.NaN;

            // Convert to decimal degrees...
            double deg;
            switch (dmsParts.Length)
            {
                case 3:  // interpret 3-part result as d/m/s
                    deg = double.Parse(dmsParts[0]) / 1 + double.Parse(dmsParts[1]) / 60 + double.Parse(dmsParts[2]) / 3600;
                    break;
                case 2:  // interpret 2-part result as d/m
                    deg = double.Parse(dmsParts[0]) / 1 + double.Parse(dmsParts[1]) / 60;
                    break;
                case 1:  // just d (possibly decimal) or non-separated dddmmss
                    deg = double.Parse(dmsParts[0]);
                    // check for fixed-width unseparated format eg 0033709W
                    //if (/[NS]/i.test(dmsParts)) deg = '0' + deg;  // - normalise N/S to 3-digit degrees
                    //if (/[0-9]{7}/.test(deg)) deg = deg.slice(0,3)/1 + deg.slice(3,5)/60 + deg.slice(5)/3600;
                    break;
                default:
                    return double.NaN;
            }

            Regex wsRegex = new Regex(@"^-|[WS]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (wsRegex.IsMatch(dms.Trim())) deg = -deg; // take '-', west and south as -ve

            return deg;
        }

        /**
         * Converts decimal degrees to deg/min/sec format
         *  - degree, prime, double-prime symbols are added, but sign is discarded and a compass
         *    direction is added if specified
         *  - degrees are zero-padded to 3 digits; for degrees latitude, use .slice(1) to remove leading
         *    zero.
         *
         * @param deg Decimal degrees to be converted
         * @param format The <code>DMSFormat</code> specifying the output format
         * @param position The <code>DMSPosition</code> identifying the orientation of the decimal degrees (used to append a compass)
         * @param dp The number of decimal places to use – default 4 for degrees, 2 for degrees/minutes, 0 for dms
         * 
         * @return {string} Degrees formatted as deg/min/secs according to specified format.
         */
        public static string ToDms(double deg, DMSFormat format = DMSFormat.Degrees, DMSOrientation orientation = DMSOrientation.None, int dp = default)
        {
            // Argument assertions
            if (double.IsNaN(deg) || double.IsInfinity(deg)) return null;

            // Check for default decimal places
            if (dp == default)
            {
                switch (format)
                {
                    case DMSFormat.DegreesMinutesSeconds:
                        dp = 0;
                        break;
                    case DMSFormat.DegreesMinutes:
                        dp = 2;
                        break;
                    case DMSFormat.Degrees:
                    default:
                        dp = 4;
                        break;
                }
            }

            // Remove the sign
            bool negative = deg < 0;
            deg = Math.Abs(deg);

            string dms = "", dStr = "", mStr = "", sStr = "";
            switch (format)
            {
                case DMSFormat.DegreesMinutesSeconds:
                    // Calculate the components
                    double d = Math.Floor(deg);
                    double m = Math.Floor((deg * 3600) / 60) % 60;
                    double s = deg * 3600 % 60;

                    // Check for seconds rollover
                    if (s.Equals(60))                          
                    {
                        s = 0;
                        m++;
                    }

                    // Check for minutes rollover
                    if (m.Equals(60))
                    {
                        m = 0;
                        d++;
                    }

                    // Build the component strings, formatting seconds to the requested decimal place
                    dStr += d;
                    mStr += m;
                    sStr += s.ToString("F" + dp);

                    // Apply leading zero formatting
                    dStr.PadLeft(3, '0');               // left-pad D with leading zeros
                    mStr.PadLeft(2, '0');               // left-pad M with leading zeros
                    if (s < 10)                                  
                    {
                        sStr = '0' + sStr;              // left-pad S with leading zero
                    }

                    // Build the final DMS string
                    dms = dStr + DEGREES_CHAR + DMS_SEPARATOR + mStr + '′' + DMS_SEPARATOR + sStr + '″';
                    break;

                case DMSFormat.DegreesMinutes:
                    // Calculate the components
                    d = Math.Floor(deg);
                    m = ((deg * 60) % 60);

                    // Check for minutes rollover
                    if (m == 60)
                    {
                        m = 0;
                        d++;
                    }

                    // Build the component strings, formatting minutes to the requested decimal place
                    dStr += d;
                    mStr += m.ToString("F" + dp); ;

                    // Apply leading zero formatting
                    dStr.PadLeft(3, '0');               // left-pad D with leading zeros
                    if (m < 10)
                    {
                        mStr = '0' + mStr;              // left-pad M with leading zeros
                    }

                    // Build the final DM string
                    dms = dStr + DEGREES_CHAR + DMS_SEPARATOR + mStr + '′';
                    break;

                case DMSFormat.Degrees:
                default:
                    // Calculate the component
                    d = deg;

                    // Build the component string, formatting degrees to the requested decimal place
                    dStr += d.ToString("F" + dp);

                    // Apply leading zero formatting
                    if (d < 100)
                    {
                        dStr = '0' + dStr; // left-pad D with a leading zero
                    }
                    if (d < 10)
                    {
                        dStr = '0' + dStr; // left-pad D with a leading zero
                    }

                    // Build the final D string
                    dms = dStr + DEGREES_CHAR;
                    break;
            }

            // Add the compass
            switch (orientation)
            {
                case DMSOrientation.Latitude:
                    dms += negative ? DMS_SEPARATOR + "S" : DMS_SEPARATOR + "N";
                    break;

                case DMSOrientation.Longitude:
                    dms += negative ? DMS_SEPARATOR + "W" : DMS_SEPARATOR + "E";
                    break;

                case DMSOrientation.None:
                default:
                    break;
            }

            return dms;
        }

    }
}