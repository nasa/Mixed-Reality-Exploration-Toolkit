// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.IO;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities;
using GOV.NASA.GSFC.XR.MRET.Data;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CSPICE.LunarModel
{
    /**
     * Sun azimuth and altitude
     * 
     * This class obtains a time and a location on the Moon from the DataManager and
     * publishes the result calculations to the DataManager.
     * 
     * The following keys containing the calculations are written to the DataManager:
     * 
     *  KEY_DATETIME:           The date/time used as input to the calculations (DateTime)
     *  KEY_EPHEMERISTIME:      The calculated ephemeris time from the input DateTime (double)
     *  KEY_MOON_RADIUS_KM:     The radius of the Moon in kilometers used for the calculations (double)
     *  KEY_MOON_LONGITUDE_RAD: The Moon longitude position in radians used as input to the calculations (double)
     *  KEY_MOON_LONGITUDE_DEG: The Moon longitude position in degrees used as input to the calculations (double)
     *  KEY_MOON_LATITUDE_RAD:  The Moon latitude position in radians used as input to the calculations (double)
     *  KEY_MOON_LATITUDE_DEG:  The Moon latitude position in degrees used as input to the calculations (double)
     *  KEY_MOON_ELEVATION_KM:  The Moon elevation in kilometers used as input to the calculations (double)
     *  KEY_SUN_POSITION_VEC:   The calculated position of the Sun in rectangular coordinates (Vector3d)
     *  KEY_SUN_RADIUS_KM:      The radius of the Sun in kilometers used for the calculations (double)
     *  KEY_SUN_AZIMUTH_RAD:    The calculated azimuth of the Sun in radians (double)
     *  KEY_SUN_AZIMUTH_DEG:    The calculated azimuth of the Sun in degrees (double)
     *  KEY_SUN_ALTITUDE_RAD:   The calculated altitude of the Sun in radians (double)
     *  KEY_SUN_ALTITUDE_DEG:   The calculated altitude of the Sun in degrees (double)
     *  KEY_SUN_LONGITUDE_RAD:  The calculated longitude of the sub-solar point in radians (double)
     *  KEY_SUN_LONGITUDE_DEG:  The calculated longitude of the sub-solar point in degrees (double)
     *  KEY_SUN_LATITUDE_RAD:   The calculated latitude of the sub-solar point in radians (double)
     *  KEY_SUN_LATITUDE_DEG:   The calculated latitude of the sub-solar point in degrees (double)
     *                         
     * @author Jeffrey Hosler
     * 
     * TODO: This should be generalized for any celestial body source and target
     */
    public class SunAzimuthAltitude : MRETUpdateBehaviour
    {
        public override string ClassName => nameof(SunAzimuthAltitude);

//      static string XRCSPICE_ROOT = System.Environment.GetEnvironmentVariable("XRCSPICE");
//      static string XRCSPICE_ROOT = Directory.GetCurrentDirectory();
//      static string XRCSPICE_ROOT = Application.dataPath;
//      static string XRCSPICE_KERNELS = XRCSPICE_ROOT + "\\Assets\\CSPICE\\data";
        static string XRCSPICE_PACKAGE_NAME = "CSPICE";
        static string XRCSPICE_KERNEL_FILE = "moon_kernels.tm";

        // Prefix of all keys
        public const string KEY_PREFIX = "GOV.NASA.GSFC.XR.SOLARSYSTEM.CELESTIALBODIES.LUNAR.MODEL.SUN";

        // DataManager keys
        public const string KEY_DATETIME = KEY_PREFIX + ".DATETIME";
        public const string KEY_EPHEMERISTIME = KEY_PREFIX + ".EPHEMERISTIME";
        public const string KEY_MOON_RADIUS_KM = KEY_PREFIX + ".MOON.RADIUS.KM";
        public const string KEY_MOON_LONGITUDE_RAD = KEY_PREFIX + ".MOON.LONGITUDE.RADIANS";
        public const string KEY_MOON_LONGITUDE_DEG = KEY_PREFIX + ".MOON.LONGITUDE.DEGREES";
        public const string KEY_MOON_LATITUDE_RAD = KEY_PREFIX + ".MOON.LATITUDE.RADIANS";
        public const string KEY_MOON_LATITUDE_DEG = KEY_PREFIX + ".MOON.LATITUDE.DEGREES";
        public const string KEY_MOON_ELEVATION_KM = KEY_PREFIX + ".MOON.ELEVATION.KM";
        public const string KEY_SUN_POSITION_VEC = KEY_PREFIX + ".POSITION.VECTOR";
        public const string KEY_SUN_RADIUS_KM = KEY_PREFIX + ".RADIUS.KM";
        public const string KEY_SUN_AZIMUTH_RAD = KEY_PREFIX + ".AZIMUTH.RADIANS";
        public const string KEY_SUN_AZIMUTH_DEG = KEY_PREFIX + ".AZIMUTH.DEGREES";
        public const string KEY_SUN_ALTITUDE_RAD = KEY_PREFIX + ".ALTITUDE.RADIANS";
        public const string KEY_SUN_ALTITUDE_DEG = KEY_PREFIX + ".ALTITUDE.DEGREES";
        public const string KEY_SUN_LONGITUDE_RAD = KEY_PREFIX + ".LONGITUDE.RADIANS";
        public const string KEY_SUN_LONGITUDE_DEG = KEY_PREFIX + ".LONGITUDE.DEGREES";
        public const string KEY_SUN_LATITUDE_RAD = KEY_PREFIX + ".LATITUDE.RADIANS";
        public const string KEY_SUN_LATITUDE_DEG = KEY_PREFIX + ".LATITUDE.DEGREES";

        [Tooltip("The DataManager to use for storing and retrieving the model calculations. If not supplied, one will be located at Start")]
        public DataManager dataManager;

        private bool f_kernelsLoaded = false;

        /**
         * Calculates the Sun position at a given time and location and places the results in
         * the DataManager.<br>
         * 
         * @param atTime A <code>DateTime</code> containing the cate and time
         * @param lon The longitude in radians
         * @param lat The latitude in radians
         * @param el The elevation in kilometers
         */
        void CalculateSunPosition(DateTime atTime, double lon, double lat, double el)
        {
            double et;
            double r, az, alt;
            double[] sunvec = new double[3];

            // Set the time to the format that SPICE requires
            string timeStr = String.Format("{0:s}", atTime.ToUniversalTime());
//            string timeStr = "2020-02-23T12:00:00";

            // Get the ephemeris time
            LunarModel.UTC2EphemerisTime(timeStr, out et);

            // Calculate the Sun azimuth and altitude
            LunarModel.SunVector(et, lon, lat, el, sunvec);
            Vector3d sunVector = new Vector3d(sunvec[0], sunvec[1], sunvec[2]);

            // Calculate the Sun azimuth and altitude
            LunarModel.SunAzimuthAltitude(sunvec, out r, out az, out alt);

            // Calculate the subsolar point (sun spot)
            double sslat, sslon;
            LunarModel.SubSolarPoint(et, out sslon, out sslat);

            // Place the calculations into the data store
            if (dataManager != null)
            {
                dataManager.SaveValue(KEY_DATETIME, atTime);
                dataManager.SaveValue(KEY_EPHEMERISTIME, et);
                dataManager.SaveValue(KEY_MOON_RADIUS_KM, LunarModel.MOON_RADIUS);
                dataManager.SaveValue(KEY_MOON_LONGITUDE_RAD, lon);
                dataManager.SaveValue(KEY_MOON_LONGITUDE_DEG, LunarModel.ToDegrees(lon));
                dataManager.SaveValue(KEY_MOON_LATITUDE_RAD, lat);
                dataManager.SaveValue(KEY_MOON_LATITUDE_DEG, LunarModel.ToDegrees(lat));
                dataManager.SaveValue(KEY_MOON_ELEVATION_KM, el);
                dataManager.SaveValue(KEY_SUN_POSITION_VEC, sunVector);
                dataManager.SaveValue(KEY_SUN_RADIUS_KM, r);
                dataManager.SaveValue(KEY_SUN_AZIMUTH_RAD, az);
                dataManager.SaveValue(KEY_SUN_AZIMUTH_DEG, LunarModel.ToDegrees(az));
                dataManager.SaveValue(KEY_SUN_ALTITUDE_RAD, alt);
                dataManager.SaveValue(KEY_SUN_ALTITUDE_DEG, LunarModel.ToDegrees(alt));
                dataManager.SaveValue(KEY_SUN_LONGITUDE_RAD, sslon);
                dataManager.SaveValue(KEY_SUN_LONGITUDE_DEG, LunarModel.ToDegrees(sslon));
                dataManager.SaveValue(KEY_SUN_LATITUDE_RAD, sslat);
                dataManager.SaveValue(KEY_SUN_LATITUDE_DEG, LunarModel.ToDegrees(sslat));
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (dataManager == null) ||
                (!f_kernelsLoaded)
                    ? IntegrityState.Failure   // Fail if base class fails, OR data manager is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Initialize the CSPICE DLL
            try
            {
                // Get the CSPICE package path
                string cspicePath = PackageLoader.GetPackagePath(Application.dataPath, XRCSPICE_PACKAGE_NAME);

                // Initialize the CSPICE plugin
                PackageLoader.InitializePackagePlugin(cspicePath);

                // Load the CSPICE Kernels for the Moon
                string kernelPath = MRET.ConfigurationManager.defaultSpiceKernelDirectory;
                Log("Loading CSPICE kernels: " + XRCSPICE_KERNEL_FILE + "' from path: " + kernelPath);
                LunarModel.LoadMetaKernel(kernelPath, XRCSPICE_KERNEL_FILE);

                // Mark as loaded
                f_kernelsLoaded = true;
            }
            catch (Exception e)
            {
                LogError("An issue was encountered initializing the SpiceToolkit: " + e.Message);
            }

            // Located the DataManager if one wasn't assigned to the script in the editor
            if (dataManager == null)
            {
                dataManager = MRET.DataManager;
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            // Check assertions
            if (f_kernelsLoaded && dataManager)
            {
                // Get the data driving the calculations.
                // TODO: Use the terrain center for now because the scale makes the sun fly across the screen
                // as the user moves, and that isn't the effect we want
                var userTimeVar = dataManager.FindPoint(SurfaceLocation.KEY_DATETIME);
//              var userLatitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_LATITUDE_DEG);
                var userLatitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_CENTER_LATITUDE_DEG);
//              var userLongitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_LONGITUDE_DEG);
                var userLongitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_CENTER_LONGITUDE_DEG);
//              var userElevationVar = dataManager.FindPoint(SurfaceLocation.KEY_ELEVATION_M);
                var userElevationVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_CENTER_ELEVATION_M);

                if ((userTimeVar is DateTime) &&
                    (userLatitudeVar is double) &&
                    (userLongitudeVar is double) &&
                    (userElevationVar is double))
                {
                    // Get the current time/lat/lon/el
                    DateTime userTime = (DateTime)userTimeVar;
                    double userLatitude = LunarModel.ToRadians((double)userLatitudeVar); // Radians
                    double userLongitude = LunarModel.ToRadians((double)userLongitudeVar); // Radians
                    double userElevation = (double)userElevationVar / 1000.0; // Kilometers

                    // Set the position of the Sun light
                    CalculateSunPosition(userTime, userLongitude, userLatitude, userElevation);
                }
            }
        }

    }
}