// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GSFC.ARVR.MRET.Time;
using GSFC.ARVR.UTILITIES;
using GSFC.ARVR.Utilities.Terrain;
using GSFC.ARVR.MRET;

namespace GSFC.ARVR.SOLARSYSTEM.CELESTIALBODIES.LUNAR.MODEL
{
    public enum ProjectionType { Equirectangular, PolarStereographic };

    /**
     * Surface location
     * 
     * This class obtains time and user information from the DataManager and publishes all of
     * the surface related calculations for the user and terrain to the DataManager.<br>
     * 
     * The following keys containing the calculations are written to the DataManager:
     * 
     *  KEY_DATETIME:                 The date/time used as input to the calculations (DateTime)
     *  KEY_LONGITUDE_DEG:            The calculated longitude position of the user in degrees (double)
     *  KEY_LATITUDE_DEG:             The calculated latitude position of the user in degrees (double)
     *  KEY_HEADING_DEG:              The calculated heading of the user in degrees (double)
     *  KEY_ELEVATION_M:              The calculated surface elevation position of the user in meters (double)
     *  KEY_USER_ELEVATION_M:         The calculated relative (eye-level) elevation position of the user in meters (double)
     *  
     *  KEY_TERRAIN_SCALE_M:                    The terrain pixel scale in meters (double)
     *  KEY_TERRAIN_AZIMUTH_DEG:                The terrain azimuth (offset from due North) in degrees [-180,180] (double)
     *  KEY_TERRAIN_UPPERLEFT_LONGITUDE_DEG:    Upper left longitude of the Terrain in degrees [-180,180] (double)
     *  KEY_TERRAIN_UPPERLEFT_LATITUDE_DEG:     Upper left latitude of the Terrain in degrees [-90,90] (double)
     *  KEY_TERRAIN_UPPERLEFT_ELEVATION_M:      Upper left elevation of the Terrain in meters (double)
     *  KEY_TERRAIN_UPPERLEFT_CARTESIAN:        Upper left cartesian coordinates of the Terrain as defined by SPICE (double[3])
     *  KEY_TERRAIN_LOWERLEFT_LONGITUDE_DEG:    Lower left longitude of the Terrain in degrees [-180,180] (double)
     *  KEY_TERRAIN_LOWERLEFT_LATITUDE_DEG:     Lower left latitude of the Terrain in degrees [-90,90] (double)
     *  KEY_TERRAIN_LOWERLEFT_ELEVATION_M:      Lower left elevation of the Terrain in meters (double)
     *  KEY_TERRAIN_LOWERLEFT_CARTESIAN:        Lower left cartesian coordinates of the Terrain as defined by SPICE (double[3])
     *  KEY_TERRAIN_UPPERRIGHT_LONGITUDE_DEG:   Upper right longitude of the Terrain in degrees [-180,180] (double)
     *  KEY_TERRAIN_UPPERRIGHT_LATITUDE_DEG:    Upper right latitude of the Terrain in degrees [-90,90] (double)
     *  KEY_TERRAIN_UPPERRIGHT_ELEVATION_M:     Upper right elevation of the Terrain in meters (double)
     *  KEY_TERRAIN_UPPERRIGHT_CARTESIAN:       Upper right cartesian coordinates of the Terrain as defined by SPICE (double[3])
     *  KEY_TERRAIN_LOWERRIGHT_LONGITUDE_DEG:   Lower right longitude of the Terrain in degrees [-180,180] (double)
     *  KEY_TERRAIN_LOWERRIGHT_LATITUDE_DEG:    Lower right latitude of the Terrain in degrees [-90,90] (double)
     *  KEY_TERRAIN_LOWERRIGHT_ELEVATION_M:     Lower right elevation of the Terrain in meters (double)
     *  KEY_TERRAIN_LOWERRIGHT_CARTESIAN:       Lower right cartesian coordinates of the Terrain as defined by SPICE (double[3])
     *  KEY_TERRAIN_CENTER_LONGITUDE_DEG:       Center longitude of the Terrain in degrees [-180,180] (double)
     *  KEY_TERRAIN_CENTER_LATITUDE_DEG:        Center latitude of the Terrain in degrees [-90,90] (double)
     *  KEY_TERRAIN_CENTER_ELEVATION_M:         Center elevation of the Terrain in meters (double)
     *  KEY_TERRAIN_CENTER_CARTESIAN:           Center cartesian coordinates of the Terrain as defined by SPICE (double[3])
     *                         
     * @author Jeffrey Hosler
     */
    public class SurfaceLocation : MRETUpdateBehaviour
    {
        public override string ClassName => nameof(SurfaceLocation);

        // Prefix of all keys
        public const string KEY_PREFIX = "GSFC.ARVR.SOLARSYSTEM.CELESTIALBODIES.LUNAR.MODEL.SURFACELOCATION";

        [Tooltip("The DataManager to use for storing retrieving and storing telemetry. If not supplied, one will be located at Start")]
        public DataManager dataManager;

        // DataManager keys
        public const string KEY_DATETIME =                          KEY_PREFIX + ".DATETIME";
        public const string KEY_LATITUDE_DEG =                      KEY_PREFIX + ".LATITUDE.DEGREES";
        public const string KEY_LONGITUDE_DEG =                     KEY_PREFIX + ".LONGITUDE.DEGREES";
        public const string KEY_ELEVATION_M =                       KEY_PREFIX + ".ELEVATION.METERS";
        public const string KEY_HEADING_DEG =                       KEY_PREFIX + ".HEADING.DEGREES";
        public const string KEY_USER_ELEVATION_M =                  KEY_PREFIX + ".USER.ELEVATION.METERS";

        public const string KEY_TERRAIN_SCALE_M =                   KEY_PREFIX + ".TERRAIN.SCALE.METERS";
        public const string KEY_TERRAIN_AZIMUTH_DEG =               KEY_PREFIX + ".TERRAIN.AZIMUTH.DEGREES";
        public const string KEY_TERRAIN_UPPERLEFT_LONGITUDE_DEG =   KEY_PREFIX + ".TERRAIN.UPPERLEFT.LONGITUDE.DEGREES";
        public const string KEY_TERRAIN_UPPERLEFT_LATITUDE_DEG =    KEY_PREFIX + ".TERRAIN.UPPERLEFT.LATITUDE.DEGREES";
        public const string KEY_TERRAIN_UPPERLEFT_ELEVATION_M =     KEY_PREFIX + ".TERRAIN.UPPERLEFT.ELEVATION.METERS";
        public const string KEY_TERRAIN_UPPERLEFT_CARTESIAN =       KEY_PREFIX + ".TERRAIN.UPPERLEFT.CARTESIAN";
        public const string KEY_TERRAIN_LOWERLEFT_LONGITUDE_DEG =   KEY_PREFIX + ".TERRAIN.LOWERLEFT.LONGITUDE.DEGREES";
        public const string KEY_TERRAIN_LOWERLEFT_LATITUDE_DEG =    KEY_PREFIX + ".TERRAIN.LOWERLEFT.LATITUDE.DEGREES";
        public const string KEY_TERRAIN_LOWERLEFT_ELEVATION_M =     KEY_PREFIX + ".TERRAIN.LOWERLEFT.ELEVATION.METERS";
        public const string KEY_TERRAIN_LOWERLEFT_CARTESIAN =       KEY_PREFIX + ".TERRAIN.LOWERLEFT.CARTESIAN";
        public const string KEY_TERRAIN_UPPERRIGHT_LONGITUDE_DEG =  KEY_PREFIX + ".TERRAIN.UPPERRIGHT.LONGITUDE.DEGREES";
        public const string KEY_TERRAIN_UPPERRIGHT_LATITUDE_DEG =   KEY_PREFIX + ".TERRAIN.UPPERRIGHT.LATITUDE.DEGREES";
        public const string KEY_TERRAIN_UPPERRIGHT_ELEVATION_M =    KEY_PREFIX + ".TERRAIN.UPPERRIGHT.ELEVATION.METERS";
        public const string KEY_TERRAIN_UPPERRIGHT_CARTESIAN =      KEY_PREFIX + ".TERRAIN.UPPERRIGHT.CARTESIAN";
        public const string KEY_TERRAIN_LOWERRIGHT_LONGITUDE_DEG =  KEY_PREFIX + ".TERRAIN.LOWERRIGHT.LONGITUDE.DEGREES";
        public const string KEY_TERRAIN_LOWERRIGHT_LATITUDE_DEG =   KEY_PREFIX + ".TERRAIN.LOWERRIGHT.LATITUDE.DEGREES";
        public const string KEY_TERRAIN_LOWERRIGHT_ELEVATION_M =    KEY_PREFIX + ".TERRAIN.LOWERRIGHT.ELEVATION.METERS";
        public const string KEY_TERRAIN_LOWERRIGHT_CARTESIAN =      KEY_PREFIX + ".TERRAIN.LOWERRIGHT.CARTESIAN";
        public const string KEY_TERRAIN_CENTER_LONGITUDE_DEG =      KEY_PREFIX + ".TERRAIN.CENTER.LONGITUDE.DEGREES";
        public const string KEY_TERRAIN_CENTER_LATITUDE_DEG =       KEY_PREFIX + ".TERRAIN.CENTER.LATITUDE.DEGREES";
        public const string KEY_TERRAIN_CENTER_ELEVATION_M =        KEY_PREFIX + ".TERRAIN.CENTER.ELEVATION.METERS";
        public const string KEY_TERRAIN_CENTER_CARTESIAN =          KEY_PREFIX + ".TERRAIN.CENTER.CARTESIAN";

        // Terrain Metadata
        [Tooltip("Indicates the map projection type for the Terrain.")]
        public ProjectionType terrainMapProjection = ProjectionType.Equirectangular;
        [Tooltip("The scale of the terrain data in meters.")]
        public double terrainScaleM = 1.0;
        [Tooltip("The angle of due North for the terrain [0-360].")]
        public double terrainAzimuth = 0.00;

        [Tooltip("Upper left longitude of the Terrain in DMS")]
        public string terrainUpperLeftLongitudeDms = Coordinates.ToDms(-180.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
        [Tooltip("Upper left latitude of the Terrain in DMS")]
        public string terrainUpperLeftLatitudeDms = Coordinates.ToDms(90.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
        [Tooltip("Lower left longitude of the Terrain in DMS")]
        public string terrainLowerLeftLongitudeDms = Coordinates.ToDms(-180.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
        [Tooltip("Lower left latitude of the Terrain in DMS")]
        public string terrainLowerLeftLatitudeDms = Coordinates.ToDms(-90.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
        [Tooltip("Upper right longitude of the Terrain in DMS")]
        public string terrainUpperRightLongitudeDms = Coordinates.ToDms(180.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
        [Tooltip("Upper right latitude of the Terrain in DMS")]
        public string terrainUpperRightLatitudeDms = Coordinates.ToDms(90.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
        [Tooltip("Lower right longitude of the Terrain in DMS")]
        public string terrainLowerRightLongitudeDms = Coordinates.ToDms(180.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
        [Tooltip("Lower right latitude of the Terrain in DMS")]
        public string terrainLowerRightLatitudeDms = Coordinates.ToDms(-90.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
        [Tooltip("Center longitude of the Terrain in DMS")]
        public string terrainCenterLongitudeDms = Coordinates.ToDms(0.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.None, 2);
        [Tooltip("Center latitude of the Terrain in DMS")]
        public string terrainCenterLatitudeDms = Coordinates.ToDms(0.00, DMSFormat.DegreesMinutesSeconds, DMSOrientation.None, 2);

        // Used to store the actual degree values
        protected double terrainUpperLeftLongitude = -180.00;
        protected double terrainUpperLeftLatitude = 90.00;
        protected double terrainUpperLeftElevation = 0.00;
        protected double terrainLowerLeftLongitude = -180.00;
        protected double terrainLowerLeftLatitude = -90.00;
        protected double terrainLowerLeftElevation = 0.00;
        protected double terrainUpperRightLongitude = 180.00;
        protected double terrainUpperRightLatitude = 90.00;
        protected double terrainUpperRightElevation = 0.00;
        protected double terrainLowerRightLongitude = 180.00;
        protected double terrainLowerRightLatitude = -90.00;
        protected double terrainLowerRightElevation = 0.00;
        protected double terrainCenterLongitude = 0.00;
        protected double terrainCenterLatitude = 0.00;
        protected double terrainCenterElevation = 0.00;

        // Used to store the cartesian coordinates
        protected double[] terrainUpperLeft = new double[3];
        protected double[] terrainLowerLeft = new double[3];
        protected double[] terrainUpperRight = new double[3];
        protected double[] terrainLowerRight = new double[3];
        protected double[] terrainCenter = new double[3];

        private bool initialized = false;
        private bool terrainInitialized = false;

        public string TerrainUpperLeftLongitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainUpperLeftLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
            }
        }

        public string TerrainUpperLeftLatitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainUpperLeftLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
            }
        }

        public string TerrainLowerLeftLongitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainLowerLeftLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
            }
        }

        public string TerrainLowerLeftLatitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainLowerLeftLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
            }
        }

        public string TerrainUpperRightLongitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainUpperRightLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
            }
        }

        public string TerrainUpperRightLatitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainUpperRightLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
            }
        }

        public string TerrainLowerRightLongitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainLowerRightLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
            }
        }

        public string TerrainLowerRightLatitudeDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainLowerRightLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
            }
        }

        public string TerrainLongitudeCenterDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainCenterLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2);
            }
        }

        public string TerrainLatitudeCenterDms
        {
            get
            {
                return Coordinates.ToDms(this.terrainCenterLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
            }
        }

        private double CalculateHeading(Vector3 forward)
        {
            // Calculate heading (where are we looking)
            double[] headingRect = new double[3];
            headingRect[0] = forward.z;
            headingRect[1] = forward.x;
            headingRect[2] = forward.y;
            double headingDistance, headingLongitude, headingLatitude;
            LunarModel.Rectangular2LatitudinalCoordinates(headingRect, out headingDistance, out headingLongitude, out headingLatitude);

            //TODO: Need to correct for the terrain azimuth (Currently done in LightingModelGraphics but needs to be done here)

            return (LunarModel.ToDegrees(headingLongitude) + 360) % 360; // Normalize to [0-360]
        }

        /**
         * Calculates the user elevation at the supplied position. This is different from the terrain elevation
         * because it factors in the height of the user and also includes flight.
         * 
         * @param eyePosition The <code>Vector3</code> defining the world space eye position from which to base the calculation
         * 
         * @result The calculated elevation of the user
         */
        private float CalculateUserElevation(Vector3 eyePosition)
        {
            float result = 0f;

            Terrain activeTerrain = TerrainUtil.FindActiveTerrain(eyePosition);
            if (activeTerrain != null)
            {
                // Get the interpolated world space Y position on the skin of the terrain at the supplied position
                float normalizedPosX = (eyePosition.x - activeTerrain.transform.position.x) / activeTerrain.terrainData.size.x;
                float normalizedPosZ = (eyePosition.z - activeTerrain.transform.position.z) / activeTerrain.terrainData.size.z;
                float worldPositionY = activeTerrain.terrainData.GetInterpolatedHeight(normalizedPosX, normalizedPosZ) + activeTerrain.transform.position.y;

                // Get the interpolated world space Y position on the skin of the terrain at the center point (arbitrary, but we need two points)
                Vector3 center = activeTerrain.terrainData.bounds.center;
                float normalizedCenterX = (center.x - activeTerrain.transform.position.x) / activeTerrain.terrainData.size.x;
                float normalizedCenterZ = (center.z - activeTerrain.transform.position.z) / activeTerrain.terrainData.size.z;
                float worldCenterY = activeTerrain.terrainData.GetInterpolatedHeight(normalizedCenterX, normalizedCenterZ) + activeTerrain.transform.position.y;

                // Grab the elevation data at our two points
                float elevationPositionY = activeTerrain.SampleHeight(eyePosition);
                float elevationCenterY = activeTerrain.SampleHeight(center);

                // Use the eye location to determine the relative elevation
                float userPositionY = eyePosition.y;

                // Calculate the difference in elevation between the user's eye and the skin of the terrain at the specified location
                // by comparing distance in world units to distance in elevation to get a vertical elevation "pixel" size that we can
                // multiply to the relative world space offset
                float elevationDiff = (Math.Abs(elevationCenterY - elevationPositionY) * (userPositionY - worldPositionY)) / Math.Abs(worldCenterY - worldPositionY);

                // Add the elevation difference to the true elevation for our relative elevation
                result = elevationPositionY + elevationDiff;
            }

            return result;
        }

        /**
         * Calculates the coordinate of the supplied position.
         * 
         * @param position The <code>Vector3</code> definine the world space position from which to base the calculation
         * @return A <code>Vector3</code> containing the [Longitude, Latitude, Elevation] where the elevation is the
         *      terrain elevation at the supplied position
         */
        private Vector3d CalculateCoordinate(Vector3 position)
        {
            Vector3d result = new Vector3d(0, 0, 0);

            Terrain activeTerrain = TerrainUtil.FindActiveTerrain(position);
            if (activeTerrain != null)
            {
                // Get the terrain bounds to be used in the calculations
                //Bounds terrainBounds = activeTerrain.terrainData.bounds;
                Terrain[] terrainGroup = TerrainUtil.GetTerrainGroup(activeTerrain.groupingID);
                Bounds terrainGroupBounds = TerrainUtil.GetTerrainGroupBounds(terrainGroup);

                // Calculate the X ratio on the active terrain group bounds so that we can apply the ratio to the
                // longitude range later
                double terrainGroupExtentX = terrainGroupBounds.max.x - terrainGroupBounds.min.x;
                double ratioX = (position.x - terrainGroupBounds.min.x) / terrainGroupExtentX;

                // Calculate the Z ratio on the active terrain group bounds so that we can apply the ratio to the
                // latitude range later
                double terrainGroupExtentZ = terrainGroupBounds.max.z - terrainGroupBounds.min.z;
                double ratioZ = (position.z - terrainGroupBounds.min.z) / terrainGroupExtentZ;

                // Calculate the rectangular coordinate position of the user
                double[] positionCoord = new double[3];

                // Apply the terrain group Z range ratio to the X rectangular coordinate (different coordinate system) range and add it to the
                // X rectangular coordinate min to get the position X rectangular coordinate (Latitude)
                positionCoord[0] = ((terrainUpperLeft[0] - terrainLowerLeft[0]) * ratioZ) + terrainLowerLeft[0];
                // Apply the terrain group X range ratio to the Z rectangular coordinate (different coordinate system) range and add it to the
                // Z rectangular coordinate min to get the position Z rectangular coordinate (Longitude)
                positionCoord[1] = ((terrainUpperRight[1] - terrainUpperLeft[1]) * ratioX) + terrainUpperLeft[1];

                // Apply the terrain group Z ratio to the Y rectangular coordinate range and add it to the Y rectangular coordinate
                // min to get the position Y radius (R)
                // TODO: This doesn't appear to be completely accurate. Can we do better? We don't have a radius at this position, so how
                // can we get a true value?
                positionCoord[2] = (((terrainUpperLeft[2] - terrainLowerLeft[2]) * ratioZ) + terrainLowerLeft[2]);

                // Convert the rectangular position coordinate to latitudinal coordinates
                double radius, longitude, latitude;
                LunarModel.Rectangular2LatitudinalCoordinates(
                    positionCoord,
                    out radius,
                    out longitude,
                    out latitude);

                // Store the lon/lat/el into the result
                result.x = LunarModel.ToDegrees(longitude);
                result.y = activeTerrain.SampleHeight(position);
                result.z = LunarModel.ToDegrees(latitude);
            }

            return result;
        }

        /**
         * Updates the terrain telemetry in the DataManager
         */
        protected void UpdateTerrainTelemetry()
        {
            // Make sure this class is initialized and the terrain hasn't been. We only want to do
            // this once for static data
            if (initialized && !terrainInitialized)
            {
                Terrain activeTerrain;

                activeTerrain = TerrainUtil.FindActiveTerrain(Vector3.zero);
                if (activeTerrain != null)
                {
                    // Get the terrain bounds to be used in the calculations
                    Terrain[] terrainGroup = TerrainUtil.GetTerrainGroup(activeTerrain.groupingID);
                    Bounds terrainGroupBounds = TerrainUtil.GetTerrainGroupBounds(terrainGroup);

                    // Get the elevations for the terrain points
                    Vector3 position = Vector3.zero;

                    // Center elevation
                    terrainCenterElevation = activeTerrain.SampleHeight(position);

                    // Upper left elevation
                    position = new Vector3(terrainGroupBounds.min.x, 0, terrainGroupBounds.max.z);
                    activeTerrain = TerrainUtil.FindActiveTerrain(position);
                    if (activeTerrain != null)
                    {
                        terrainUpperLeftElevation = activeTerrain.SampleHeight(position);
                    }

                    // Lower left elevation
                    position = new Vector3(terrainGroupBounds.min.x, 0, terrainGroupBounds.min.z);
                    activeTerrain = TerrainUtil.FindActiveTerrain(position);
                    if (activeTerrain != null)
                    {
                        terrainLowerLeftElevation = activeTerrain.SampleHeight(position);
                    }

                    // Upper right elevation
                    position = new Vector3(terrainGroupBounds.max.x, 0, terrainGroupBounds.max.z);
                    activeTerrain = TerrainUtil.FindActiveTerrain(position);
                    if (activeTerrain != null)
                    {
                        terrainUpperRightElevation = activeTerrain.SampleHeight(position);
                    }

                    // Lower right elevation
                    position = new Vector3(terrainGroupBounds.max.x, 0, terrainGroupBounds.min.z);
                    activeTerrain = TerrainUtil.FindActiveTerrain(position);
                    if (activeTerrain != null)
                    {
                        terrainLowerRightElevation = activeTerrain.SampleHeight(position);
                    }

                    // Get the cartesian coordinates of the terrain bounds
                    LunarModel.Latitudinal2RectangularCoordinates(
                        LunarModel.MOON_RADIUS + (terrainUpperLeftElevation / 1000d),
                        LunarModel.ToRadians(terrainUpperLeftLongitude),
                        LunarModel.ToRadians(terrainUpperLeftLatitude),
                        terrainUpperLeft);
                    LunarModel.Latitudinal2RectangularCoordinates(
                        LunarModel.MOON_RADIUS + (terrainLowerLeftElevation / 1000d),
                        LunarModel.ToRadians(terrainLowerLeftLongitude),
                        LunarModel.ToRadians(terrainLowerLeftLatitude),
                        terrainLowerLeft);
                    LunarModel.Latitudinal2RectangularCoordinates(
                        LunarModel.MOON_RADIUS + (terrainUpperRightElevation / 1000d),
                        LunarModel.ToRadians(terrainUpperRightLongitude),
                        LunarModel.ToRadians(terrainUpperRightLatitude),
                        terrainUpperRight);
                    LunarModel.Latitudinal2RectangularCoordinates(
                        LunarModel.MOON_RADIUS + (terrainLowerRightElevation / 1000d),
                        LunarModel.ToRadians(terrainLowerRightLongitude),
                        LunarModel.ToRadians(terrainLowerRightLatitude),
                        terrainLowerRight);
                    LunarModel.Latitudinal2RectangularCoordinates(
                        LunarModel.MOON_RADIUS + (terrainCenterElevation / 1000d),
                        LunarModel.ToRadians(terrainCenterLongitude),
                        LunarModel.ToRadians(terrainCenterLatitude),
                        terrainLowerRight);

                    // Reverse the calculations for sanity
                    double radius, longitude, latitude;
                    LunarModel.Rectangular2LatitudinalCoordinates(
                        terrainUpperLeft,
                        out radius,
                        out longitude,
                        out latitude);
                    Vector3d upperLeftCoord = new Vector3d(radius * 1000d, LunarModel.ToDegrees(longitude), LunarModel.ToDegrees(latitude));
                    LunarModel.Rectangular2LatitudinalCoordinates(
                        terrainLowerLeft,
                        out radius,
                        out longitude,
                        out latitude);
                    Vector3d lowerLeftCoord = new Vector3d(radius * 1000d, LunarModel.ToDegrees(longitude), LunarModel.ToDegrees(latitude));
                    LunarModel.Rectangular2LatitudinalCoordinates(
                        terrainUpperRight,
                        out radius,
                        out longitude,
                        out latitude);
                    Vector3d upperRightCoord = new Vector3d(radius * 1000d, LunarModel.ToDegrees(longitude), LunarModel.ToDegrees(latitude));
                    LunarModel.Rectangular2LatitudinalCoordinates(
                        terrainLowerRight,
                        out radius,
                        out longitude,
                        out latitude);
                    Vector3d lowerRightCoord = new Vector3d(radius * 1000d, LunarModel.ToDegrees(longitude), LunarModel.ToDegrees(latitude));
                    LunarModel.Rectangular2LatitudinalCoordinates(
                        terrainCenter,
                        out radius,
                        out longitude,
                        out latitude);
                    Vector3d centerCoord = new Vector3d(radius * 1000d, LunarModel.ToDegrees(longitude), LunarModel.ToDegrees(latitude));

                    // Store the telemetry into the DataManager
                    dataManager.SaveValue(KEY_TERRAIN_SCALE_M, terrainScaleM);
                    dataManager.SaveValue(KEY_TERRAIN_AZIMUTH_DEG, terrainAzimuth);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERLEFT_LONGITUDE_DEG, terrainUpperLeftLongitude);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERLEFT_LATITUDE_DEG, terrainUpperLeftLatitude);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERLEFT_ELEVATION_M, terrainUpperLeftElevation);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERLEFT_CARTESIAN, terrainUpperLeft);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERLEFT_LONGITUDE_DEG, terrainLowerLeftLongitude);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERLEFT_LATITUDE_DEG, terrainLowerLeftLatitude);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERLEFT_ELEVATION_M, terrainLowerLeftElevation);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERLEFT_CARTESIAN, terrainLowerLeft);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERRIGHT_LONGITUDE_DEG, terrainUpperRightLongitude);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERRIGHT_LATITUDE_DEG, terrainUpperRightLatitude);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERRIGHT_ELEVATION_M, terrainUpperRightElevation);
                    dataManager.SaveValue(KEY_TERRAIN_UPPERRIGHT_CARTESIAN, terrainUpperRight);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERRIGHT_LONGITUDE_DEG, terrainLowerRightLongitude);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERRIGHT_LATITUDE_DEG, terrainLowerRightLatitude);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERRIGHT_ELEVATION_M, terrainLowerRightElevation);
                    dataManager.SaveValue(KEY_TERRAIN_LOWERRIGHT_CARTESIAN, terrainLowerRight);
                    dataManager.SaveValue(KEY_TERRAIN_CENTER_LONGITUDE_DEG, terrainCenterLongitude);
                    dataManager.SaveValue(KEY_TERRAIN_CENTER_LATITUDE_DEG, terrainCenterLatitude);
                    dataManager.SaveValue(KEY_TERRAIN_CENTER_ELEVATION_M, terrainCenterElevation);
                    dataManager.SaveValue(KEY_TERRAIN_CENTER_CARTESIAN, terrainCenter);

                    // Mark as initialized
                    terrainInitialized = true;
                }
            }
        }

        /**
         * Updates the user telemetry in the DataManager
         */
        protected void UpdateUserTelemetry()
        {
            // Make sure the data manager reference is valid
            if (dataManager != null)
            {
                // Obtain the user position to calculate the coordinate
                var userPosition = dataManager.FindPoint(MRETUser.KEY_USER_POSITION);
                if (userPosition is Vector3)
                {
                    // Calculate the surface coordinate
                    Vector3d surfaceCoordinate = CalculateCoordinate((Vector3)userPosition);

                    // Store the values in the DataManager
                    dataManager.SaveValue(KEY_LATITUDE_DEG, surfaceCoordinate.z);
                    dataManager.SaveValue(KEY_LONGITUDE_DEG, surfaceCoordinate.x);
                    dataManager.SaveValue(KEY_ELEVATION_M, surfaceCoordinate.y);

                    // Obtain the user eye position to calculate the user's elevation
                    var userEyePosition = dataManager.FindPoint(MRETUser.KEY_USER_EYE_POSITION);
                    if (userEyePosition is Vector3)
                    {
                        // Calculate the surface coordinate
                        float userElevation = CalculateUserElevation((Vector3)userEyePosition);

                        // Store the value in the DataManager
                        dataManager.SaveValue(KEY_USER_ELEVATION_M, userElevation);
                    }

                }

                // Obtain the user orientation to calculate the heading
                var userOrientation = dataManager.FindPoint(MRETUser.KEY_USER_ORIENTATION);
                if (userOrientation is Vector3)
                {
                    // Calculate the surface object heading
                    double heading = CalculateHeading((Vector3)userOrientation);

                    // Store the initial value in the DataManager
                    dataManager.SaveValue(KEY_HEADING_DEG, heading);
                }

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
                (!initialized)
                    ? IntegrityState.Failure   // Fail if base class fails, OR data manager is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            //Debug.Log("Upper Left:  [" + Coordinates.ToDegrees("3d22'43.43E") + ", " + Coordinates.ToDegrees("26d 9'35.54N") + "]");
            //Debug.Log("Lower Right: [" + Coordinates.ToDegrees("3d45'16.02E") + ", " + Coordinates.ToDegrees("25d49'19.85N") + "]");
            //Debug.Log("Center: [" + Coordinates.ToDegrees("3d33'59.72E") + ", " + Coordinates.ToDegrees("25d59'27.69N") + "]");

            // Track errors encountered during startup
            bool errorEncountered = false;

            // Parse the terrain corners
            // Upper left
            terrainUpperLeftLongitude = Coordinates.ToDegrees(terrainUpperLeftLongitudeDms);
            if (double.IsNaN(terrainUpperLeftLongitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Upper left terrain longitude could not be parsed: " + terrainUpperLeftLongitudeDms);
                errorEncountered = true;
            }
            terrainUpperLeftLatitude = Coordinates.ToDegrees(terrainUpperLeftLatitudeDms);
            if (double.IsNaN(terrainUpperLeftLatitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Upper left terrain latitude could not be parsed: " + terrainUpperLeftLatitudeDms);
                errorEncountered = true;
            }
            // Lower left
            terrainLowerLeftLongitude = Coordinates.ToDegrees(terrainLowerLeftLongitudeDms);
            if (double.IsNaN(terrainLowerLeftLongitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Lower left terrain longitude could not be parsed: " + terrainLowerLeftLongitudeDms);
                errorEncountered = true;
            }
            terrainLowerLeftLatitude = Coordinates.ToDegrees(terrainLowerLeftLatitudeDms);
            if (double.IsNaN(terrainLowerLeftLatitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Lower left terrain latitude could not be parsed: " + terrainLowerLeftLatitudeDms);
                errorEncountered = true;
            }
            // Upper right
            terrainUpperRightLongitude = Coordinates.ToDegrees(terrainUpperRightLongitudeDms);
            if (double.IsNaN(terrainUpperRightLongitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Upper right terrain longitude could not be parsed: " + terrainUpperRightLongitudeDms);
                errorEncountered = true;
            }
            terrainUpperRightLatitude = Coordinates.ToDegrees(terrainUpperRightLatitudeDms);
            if (double.IsNaN(terrainUpperRightLatitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Upper right terrain latitude could not be parsed: " + terrainUpperRightLatitudeDms);
                errorEncountered = true;
            }
            // Lower right
            terrainLowerRightLongitude = Coordinates.ToDegrees(terrainLowerRightLongitudeDms);
            if (double.IsNaN(terrainLowerRightLongitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Lower right terrain longitude could not be parsed: " + terrainLowerRightLongitudeDms);
                errorEncountered = true;
            }
            terrainLowerRightLatitude = Coordinates.ToDegrees(terrainLowerRightLatitudeDms);
            if (double.IsNaN(terrainLowerRightLatitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Lower right terrain latitude could not be parsed: " + terrainLowerRightLatitudeDms);
                errorEncountered = true;
            }
            // Center
            terrainCenterLongitude = Coordinates.ToDegrees(terrainCenterLongitudeDms);
            if (double.IsNaN(terrainCenterLongitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Center terrain longitude could not be parsed: " + terrainCenterLongitudeDms);
                errorEncountered = true;
            }
            terrainCenterLatitude = Coordinates.ToDegrees(terrainCenterLatitudeDms);
            if (double.IsNaN(terrainCenterLatitude))
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Center terrain latitude could not be parsed: " + terrainCenterLatitudeDms);
                errorEncountered = true;
            }

            // Check for any errors
            if (errorEncountered)
            {
                Debug.LogError("[" + ClassName + "->" + nameof(MRETStart) + "; Errors encountered during startup. Processing will not occur.");
            }

            // Try to get a reference to the data manager if not explictly set
            if (dataManager == null)
            {
                dataManager = MRET.Infrastructure.Framework.MRET.DataManager;
            }

            // Indicate if we are initialized properly
            initialized = !errorEncountered && (dataManager != null);
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            // Make sure we are initialized properly
            if (initialized)
            {
                // Get the project time
                var projectTime = dataManager.FindPoint(TimeManager.TIME_KEY_NOW);

                // Store the time stamp in the DataManager
                dataManager.SaveValue(KEY_DATETIME, projectTime);

                // Update the terrain telemetry
                UpdateTerrainTelemetry();

                // Update user telemetry
                UpdateUserTelemetry();
            }

        }

    }

}