using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using GSFC.ARVR.UTILITIES;
using GSFC.ARVR.Utilities.Terrain;

namespace GSFC.ARVR.SOLARSYSTEM.CELESTIALBODIES.LUNAR.MODEL
{
    /**
     * Lunar lighting model graphics
     * 
     * This class obtains information from the DataManager, performs calculations to convert to
     * Unity-space and positions the scene objects based upon those calculations.<br>
     * 
     * @author Jeffrey Hosler
     */
    public class LightingModelGraphics : MonoBehaviour
    {
        // Performance Management
        private int updateCounter = 0;
        public int updateRateModulo = 1;

        // Data Manager
        public DataManager dataManager;

        // Labels
        public GameObject timeText;
        public GameObject longitudeText;
        public GameObject latitudeText;
        public GameObject elevationText;
        public GameObject headingText;
        public GameObject sunAzimuthText;
        public GameObject sunAltitudeText;
        public GameObject earthAzimuthText;
        public GameObject earthAltitudeText;

        // Objects
        public GameObject moonObject;
        public GameObject surfaceObject;
        public GameObject sunSpotObject;
        public GameObject astronautObject;

        public GameObject sunlightObject;
        public float sunPositionScaleFactor = 1400;

        public GameObject earthObject;
        public float earthPositionScaleFactor = 3;

        // Terrain Metadata
        private bool terrainMetadataInitialized = false;
        private double terrainScaleM;
        private double terrainAzimuth;
        private double terrainUpperLeftLongitude;
        private double terrainUpperLeftLatitude;
        private double terrainLowerLeftLongitude;
        private double terrainLowerLeftLatitude;
        private double terrainUpperRightLongitude;
        private double terrainUpperRightLatitude;
        private double terrainLowerRightLongitude;
        private double terrainLowerRightLatitude;
        private double terrainCenterLongitude;
        private double terrainCenterLatitude;

        void PositionObjectOnMoon(GameObject theObject, double actualMoonRadius, double actualRelativeObjectElevation, double objectLongitudeRad, double objectLatitudeRad)
        {
            if ((moonObject != null) && (theObject != null))
            {
                Vector3 moonScale = moonObject.transform.localScale;
                double moonObjectRadius = moonScale.x / 2.0;
                double actualObjectElevation = actualMoonRadius + actualRelativeObjectElevation;
                double scaledElevation = ((moonObjectRadius * (actualObjectElevation * 1000.0)) / (actualMoonRadius * 1000.0)); // Convert to meters

                // Get the rectangular coordinate. 
                double[] rectCoord = new double[3];
                LunarModel.Latitudinal2RectangularCoordinates(scaledElevation, objectLongitudeRad, objectLatitudeRad, rectCoord);

                // Calculate the location from the moon origin
                // In Unity, Z is forward/backward, Y is up/down and X is left/right
                // Spice rectangular coordinate, 0(X) is forward/backward, 1(Y) is left/right, 2(Z) is up/down
                // Camera is at -Z is -X, left/right is Y, up/down is Z. so 0,0 lat/lon points toward camera
                Vector3 scaledPosition = new Vector3((float)rectCoord[1], (float)rectCoord[2], (float)-rectCoord[0]);
                theObject.transform.localPosition = scaledPosition;
            }
        }

        void PositionSun(GameObject theSun, double terrainAzimuth, double sunAzimuthDeg, double sunAltitudeDeg, double sunRadius)
        {
            if (theSun != null)
            {
                // Figure out the visual azimuth based upon the orientation of the terrain
                double visualAzimuth = AdjustAzimuth(terrainAzimuth, sunAzimuthDeg);

                // Convert the visual azimuth to [-180,180]
                double visualLongitude = NormalizeAngle180(visualAzimuth);

                // Calculate the position of the sun relative to the azimuth of the terrain
                double[] rectCoord = new double[3];
                LunarModel.Latitudinal2RectangularCoordinates(
                    sunRadius,
                    LunarModel.ToRadians(visualLongitude),
                    LunarModel.ToRadians(sunAltitudeDeg),
                    rectCoord);

                // Position the sun
                double FACTOR = sunPositionScaleFactor;
                Vector3 scaledPosition = new Vector3((float)(rectCoord[1] / FACTOR), (float)(rectCoord[2] / FACTOR), (float)(rectCoord[0] / FACTOR));
                theSun.transform.localPosition = scaledPosition;
            }
        }

        void PositionEarth(GameObject theEarth, double terrainAzimuth, double earthAzimuthDeg, double earthAltitudeDeg, double earthRadius)
        {
            if (theEarth != null)
            {
                // Figure out the visual azimuth based upon the orientation of the terrain
                double visualAzimuth = AdjustAzimuth(terrainAzimuth, earthAzimuthDeg);

                // Convert the visual azimuth to [-180,180]
                double visualLongitude = NormalizeAngle180(visualAzimuth);

                // Calculate the position of the Earth relative to the azimuth of the terrain
                double[] rectCoord = new double[3];
                LunarModel.Latitudinal2RectangularCoordinates(
                    earthRadius,
                    LunarModel.ToRadians(visualLongitude),
                    LunarModel.ToRadians(earthAltitudeDeg),
                    rectCoord);

                // Position the Earth
                double FACTOR = earthPositionScaleFactor;
                Vector3 scaledPosition = new Vector3((float)(rectCoord[1] / FACTOR), (float)(rectCoord[2] / FACTOR), (float)(rectCoord[0] / FACTOR));
                theEarth.transform.localPosition = scaledPosition;
            }
        }

        private double AdjustAzimuth(double terrainAzimuth, double azimuth)
        {
            return azimuth + (360.0 - terrainAzimuth);
        }

        private double NormalizeAngle180(double heading)
        {
            return (heading + 540) % 360 - 180;
        }

        private double NormalizeAngle360(double angle)
        {
            return (angle + 360) % 360;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Make a sunlight object
            //            lightSunObject = new GameObject("Light Sun");

            // Add the light component
            //            Light lightSunComponent = lightSunObject.AddComponent<Light>();
            //            lightSunComponent.type = LightType.Directional;
            //            lightSunComponent.shadows = LightShadows.Hard;
            //            lightSunComponent.color = Color.white;
            //            lightSunComponent.intensity = (float) 2.5;

            // Add a lens flare effect
            //            LensFlare lensFlare = lightSunObject.GetComponent<LensFlare>();
            //            lensFlare.color = Color.white;

            if (sunlightObject != null)
            {
                // Add the look at constraint
                if (moonObject != null)
                {
                    //                    MeshFilter meshFilter = moonObject.GetComponent<MeshFilter>();
                    //                    meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                    LookAtConstraint lookAtConstraint = sunlightObject.AddComponent<LookAtConstraint>();
                    ConstraintSource lookAtSource = new ConstraintSource();
                    lookAtSource.sourceTransform = moonObject.transform;
                    lookAtSource.weight = 1;
                    lookAtConstraint.AddSource(lookAtSource);
                    lookAtConstraint.locked = true;
                    lookAtConstraint.constraintActive = true;

                    // Place the astronaut on the surface
                    if (astronautObject != null)
                    {
                        // We want to move the feet of the astronaut to the surface, so
                        // get the height of the astronaut, and divide by 2 to use that
                        // value as a vertical offset from the terrain
                        double heightOffset = (astronautObject.transform.localScale.y / 2.0) + 0.35;

                        // Figure out where to move the astronaut on the Y axis to place it on the terrain
                        Vector3 astronautPos = astronautObject.transform.position;

                        // Find the active terrain that we need to use; the one below the astronaut
                        Terrain activeTerrain = TerrainUtil.FindActiveTerrain(astronautPos);
                        if (activeTerrain != null)
                        {
                            astronautPos.y = activeTerrain.SampleHeight(astronautPos);// + (float)heightOffset;
                        }

                        // Move the astronaut
                        astronautObject.transform.position = astronautPos;
                    }
                }

                if (surfaceObject != null)
                {
                }

                if (sunSpotObject != null)
                {
                    // Make the direction object look at the sun light
                    LookAtConstraint lookAtConstraint = sunSpotObject.AddComponent<LookAtConstraint>();
                    ConstraintSource lookAtSource = new ConstraintSource();
                    lookAtSource.sourceTransform = sunlightObject.transform;
                    lookAtSource.weight = 1;
                    lookAtConstraint.AddSource(lookAtSource);
                    lookAtConstraint.rotationOffset = new Vector3(0, 90, 0);
                    //                lookAtConstraint.locked = true;
                    lookAtConstraint.constraintActive = true;
                }
            }

            if (earthObject != null)
            {
            }

            // Located the DataManager if one wasn't assigned to the script in the editor
            if (dataManager == null)
            {
                dataManager = FindObjectOfType<DataManager>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                string filename = SceneManager.GetActiveScene().name + "_" + String.Format("{0:s}", DateTime.Now).Replace(":","") + ".png";
                ScreenCapture.CaptureScreenshot(filename);
                print("Print key was pressed");
            }

            // Performance management
            updateCounter++;
            if (updateCounter >= updateRateModulo)
            {
                // Reset the update counter
                updateCounter = 0;

                if (dataManager != null)
                {
                    // Grab the terrain metadata if we don't already have it
                    if (!terrainMetadataInitialized)
                    {
                        // Extract the metadata from the data store
                        var terrainScaleMVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_SCALE_M);
                        var terrainAzimuthVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_AZIMUTH_DEG);
                        var terrainUpperLeftLongitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_UPPERLEFT_LONGITUDE_DEG);
                        var terrainUpperLeftLatitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_UPPERLEFT_LATITUDE_DEG);
                        var terrainLowerLeftLongitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_LOWERLEFT_LONGITUDE_DEG);
                        var terrainLowerLeftLatitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_LOWERLEFT_LATITUDE_DEG);
                        var terrainUpperRightLongitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_UPPERRIGHT_LONGITUDE_DEG);
                        var terrainUpperRightLatitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_UPPERRIGHT_LATITUDE_DEG);
                        var terrainLowerRightLongitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_LOWERRIGHT_LONGITUDE_DEG);
                        var terrainLowerRightLatitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_LOWERRIGHT_LATITUDE_DEG);
                        var terrainCenterLongitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_CENTER_LONGITUDE_DEG);
                        var terrainCenterLatitudeVar = dataManager.FindPoint(SurfaceLocation.KEY_TERRAIN_CENTER_LATITUDE_DEG);

                        // Check that we have valid data
                        if ((terrainScaleMVar is double) &&
                            (terrainAzimuthVar is double) &&
                            (terrainUpperLeftLongitudeVar is double) &&
                            (terrainUpperLeftLatitudeVar is double) &&
                            (terrainLowerLeftLongitudeVar is double) &&
                            (terrainLowerLeftLatitudeVar is double) &&
                            (terrainUpperRightLongitudeVar is double) &&
                            (terrainUpperRightLatitudeVar is double) &&
                            (terrainLowerRightLongitudeVar is double) &&
                            (terrainLowerRightLatitudeVar is double) &&
                            (terrainCenterLatitudeVar is double) &&
                            (terrainCenterLatitudeVar is double))
                        {
                            // Store the values in our private fields. These shouldn't change, so we only need to do it once.
                            terrainScaleM = (double)terrainScaleMVar;
                            terrainAzimuth = (double)terrainAzimuthVar;
                            terrainUpperLeftLongitude = (double)terrainUpperLeftLongitudeVar;
                            terrainUpperLeftLatitude = (double)terrainUpperLeftLatitudeVar;
                            terrainLowerLeftLongitude = (double)terrainLowerLeftLongitudeVar;
                            terrainLowerLeftLatitude = (double)terrainLowerLeftLatitudeVar;
                            terrainUpperRightLongitude = (double)terrainUpperRightLongitudeVar;
                            terrainUpperRightLatitude = (double)terrainUpperRightLatitudeVar;
                            terrainLowerRightLongitude = (double)terrainLowerRightLongitudeVar;
                            terrainLowerRightLatitude = (double)terrainLowerRightLatitudeVar;
                            terrainCenterLongitude = (double)terrainCenterLongitudeVar;
                            terrainCenterLatitude = (double)terrainCenterLatitudeVar;

                            double terrainZ = Coordinates.GetDistance(terrainUpperLeftLongitude, terrainUpperLeftLatitude, terrainLowerLeftLongitude, terrainLowerLeftLatitude);
                            Debug.Log("Terrain Hight (m): " + terrainZ.ToString("F3"));

                            double terrainX = Coordinates.GetDistance(terrainUpperLeftLongitude, terrainUpperLeftLatitude, terrainUpperRightLongitude, terrainUpperRightLatitude);
                            Debug.Log("Terrain Width (m): " + terrainX.ToString("F3"));

                            string terrainBoundsStr = "Terrain Bounds:\n";
                            terrainBoundsStr += "\tUpper Left Longitude:   " + terrainUpperLeftLongitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainUpperLeftLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2) + "\n";
                            terrainBoundsStr += "\tUpper Left Latitude:    " + terrainUpperLeftLatitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainUpperLeftLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2) + "\n";
                            terrainBoundsStr += "\tLower Left Longitude:   " + terrainLowerLeftLongitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainLowerLeftLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2) + "\n";
                            terrainBoundsStr += "\tLower Left Latitude:    " + terrainLowerLeftLatitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainLowerLeftLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2) + "\n";
                            terrainBoundsStr += "\tUpper Right Longitude:   " + terrainUpperRightLongitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainUpperRightLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2) + "\n";
                            terrainBoundsStr += "\tUpper Right Latitude:    " + terrainUpperRightLatitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainUpperRightLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2) + "\n";
                            terrainBoundsStr += "\tLower Right Longitude:   " + terrainLowerRightLongitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainLowerRightLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2) + "\n";
                            terrainBoundsStr += "\tLower Right Latitude:    " + terrainLowerRightLatitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainLowerRightLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2) + "\n";
                            terrainBoundsStr += "\tCenter Longitude:   " + terrainCenterLongitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainCenterLongitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2) + "\n";
                            terrainBoundsStr += "\tCenter Latitude:  " + terrainCenterLatitude.ToString("F3") + "; " +
                                Coordinates.ToDms(terrainCenterLatitude, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2);
                            Debug.Log(terrainBoundsStr);

                            // Flag as initialized
                            terrainMetadataInitialized = true;
                        }
                    }

                    // Get the changing model data from the data store
                    var timeVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_DATETIME);
                    var ephemerisTimeVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_EPHEMERISTIME);
                    var moonRadiusVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_MOON_RADIUS_KM);
                    var moonLongitudeVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_MOON_LONGITUDE_RAD);
                    var moonLatitudeVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_MOON_LATITUDE_RAD);
                    var moonElevationVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_MOON_ELEVATION_KM);
                    var objectHeadingVar = dataManager.FindPoint(SurfaceLocation.KEY_HEADING_DEG);

                    if ((timeVar is DateTime) &&
                        (ephemerisTimeVar is double) &&
                        (moonRadiusVar is double) &&
                        (moonLongitudeVar is double) &&
                        (moonLatitudeVar is double) &&
                        (moonElevationVar is double) &&
                        (objectHeadingVar is double))
                    {
                        DateTime time = (DateTime)timeVar;
                        double et = (double)ephemerisTimeVar;
                        double moonRadius = (double)moonRadiusVar;
                        double moonLongitude = (double)moonLongitudeVar;
                        double moonLongitudeDeg = LunarModel.ToDegrees((double)moonLongitudeVar);
                        double moonLatitude = (double)moonLatitudeVar;
                        double moonLatitudeDeg = LunarModel.ToDegrees((double)moonLatitudeVar);
                        double moonElevation = (double)moonElevationVar;
                        double objectHeading = (double)objectHeadingVar;

                        Debug.Log("Location [Lon,Lat,El]: " + moonLongitude + ", " + moonLatitude + ", " + moonElevation + "km\n");
                        Debug.Log("Location Deg [Lon,Lat]: [" + moonLongitudeDeg.ToString("F3") + ", " + moonLatitudeDeg.ToString("F3") + "]");
                        Debug.Log("Location DMS [Lon, Lat]: [" +
                            Coordinates.ToDms(moonLongitudeDeg, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Longitude, 2) + ", " +
                            Coordinates.ToDms(moonLatitudeDeg, DMSFormat.DegreesMinutesSeconds, DMSOrientation.Latitude, 2) + "]");
                        Debug.Log("Date/Time: " + time.ToString("u") + "\n");

                        if (timeText != null)
                        {
                            Text text = timeText.GetComponent<Text>();
                            text.text = String.Format("{0:u}", time);
                        }

                        if (longitudeText != null)
                        {
                            Text text = longitudeText.GetComponent<Text>();
                            text.text = moonLongitudeDeg.ToString("F3");
                        }

                        if (latitudeText != null)
                        {
                            Text text = latitudeText.GetComponent<Text>();
                            text.text = moonLatitudeDeg.ToString("F3");
                        }

                        if (elevationText != null)
                        {
                            Text text = elevationText.GetComponent<Text>();
                            text.text = (moonElevation * 1000.0).ToString("F3"); // meters
                        }

                        if (headingText != null)
                        {
                            Text text = headingText.GetComponent<Text>();
                            double correctedHeading = AdjustAzimuth(terrainAzimuth, objectHeading);
                            correctedHeading = NormalizeAngle360(correctedHeading);
                            text.text = correctedHeading.ToString("F3");
                        }

                        if (surfaceObject != null)
                        {
                            // Position the surface object
                            PositionObjectOnMoon(surfaceObject, moonRadius, moonElevation, moonLongitude, moonLatitude);
                        }

                        if (sunlightObject != null)
                        {
                            var sunPositionVecVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_SUN_POSITION_VEC);
                            var sunRadiusVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_SUN_RADIUS_KM);
                            var sunAzimuthVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_SUN_AZIMUTH_RAD);
                            var sunAltitudeVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_SUN_ALTITUDE_RAD);
                            var sunLongitudeVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_SUN_LONGITUDE_RAD);
                            var sunLatitudeVar = dataManager.FindPoint(SunAzimuthAltitude.KEY_SUN_LATITUDE_RAD);

                            if ((sunPositionVecVar is Vector3d) &&
                                (sunRadiusVar is double) &&
                                (sunAzimuthVar is double) &&
                                (sunAltitudeVar is double) &&
                                (sunLongitudeVar is double) &&
                                (sunLatitudeVar is double))
                            {
                                Vector3d sunPositionVec = (Vector3d)sunPositionVecVar;
                                double sunRadius = (double)sunRadiusVar;
                                double sunAzimuthDeg = LunarModel.ToDegrees((double)sunAzimuthVar);
                                double sunAltitudeDeg = LunarModel.ToDegrees((double)sunAltitudeVar);
                                double sunLongitude = (double)sunLongitudeVar;
                                double sunLatitude = (double)sunLatitudeVar;

                                Debug.Log("Sun Vector [X,Y,Z]: " + sunPositionVec.x + ", " + sunPositionVec.y + ", " + sunPositionVec.z + ", " + "\n");
                                Debug.Log("Sun Distance, Azimuth, Altitude: " +
                                    sunRadius + "km, " +
                                    sunAzimuthDeg + ", " +
                                    sunAltitudeDeg + "\n");

                                Debug.Log("Subsolar point: " +
                                    LunarModel.ToDegrees(sunLongitude) + ", " +
                                    LunarModel.ToDegrees(sunLatitude) + "\n\n");

                                if (sunAzimuthText != null)
                                {
                                    Text text = sunAzimuthText.GetComponent<Text>();
                                    double correctedAzimuth = AdjustAzimuth(terrainAzimuth, sunAzimuthDeg);
                                    correctedAzimuth = NormalizeAngle360(correctedAzimuth);
                                    text.text = correctedAzimuth.ToString("F3");
                                }

                                if (sunAltitudeText != null)
                                {
                                    Text text = sunAltitudeText.GetComponent<Text>();
                                    text.text = sunAltitudeDeg.ToString("F3");
                                }

                                if (sunSpotObject != null)
                                {
                                    // Position the sun spot object on the surface
                                    PositionObjectOnMoon(sunSpotObject, moonRadius, 0, sunLongitude, sunLatitude);
                                }

                                // Position the light source
                                if (Terrain.activeTerrain != null)
                                {
                                    // Use sun azimuth/altitude
                                    PositionSun(sunlightObject, terrainAzimuth, sunAzimuthDeg, sunAltitudeDeg, sunRadius);
                                }
                                else
                                {
                                    // Use the sun spot
                                    PositionObjectOnMoon(sunlightObject, moonRadius, 100000, sunLongitude, sunLatitude);
                                }
                            }
                        }

                        if (earthObject != null)
                        {
                            var earthPositionVecVar = dataManager.FindPoint(EarthAzimuthAltitude.KEY_EARTH_POSITION_VEC);
                            var earthRadiusVar = dataManager.FindPoint(EarthAzimuthAltitude.KEY_EARTH_RADIUS_KM);
                            var earthAzimuthVar = dataManager.FindPoint(EarthAzimuthAltitude.KEY_EARTH_AZIMUTH_RAD);
                            var earthAltitudeVar = dataManager.FindPoint(EarthAzimuthAltitude.KEY_EARTH_ALTITUDE_RAD);

                            if ((earthPositionVecVar is Vector3d) &&
                                (earthRadiusVar is double) &&
                                (earthAzimuthVar is double) &&
                                (earthAltitudeVar is double))
                            {
                                Vector3d earthPositionVec = (Vector3d)earthPositionVecVar;
                                double earthRadius = (double)earthRadiusVar;
                                double earthAzimuthDeg = LunarModel.ToDegrees((double)earthAzimuthVar);
                                double earthAltitudeDeg = LunarModel.ToDegrees((double)earthAltitudeVar);

                                Debug.Log("Earth Vector [X,Y,Z]: " + earthPositionVec.x + ", " + earthPositionVec.y + ", " + earthPositionVec.z + ", " + "\n");
                                Debug.Log("Earth Distance, Azimuth, Altitude: " +
                                    earthRadius + "km, " +
                                    earthAzimuthDeg + ", " +
                                    earthAltitudeDeg + "\n");

                                if (earthAzimuthText != null)
                                {
                                    Text text = earthAzimuthText.GetComponent<Text>();
                                    double correctedAzimuth = AdjustAzimuth(terrainAzimuth, earthAzimuthDeg);
                                    correctedAzimuth = NormalizeAngle360(correctedAzimuth);
                                    text.text = correctedAzimuth.ToString("F3");
                                }

                                if (earthAltitudeText != null)
                                {
                                    Text text = earthAltitudeText.GetComponent<Text>();
                                    text.text = earthAltitudeDeg.ToString("F3");
                                }

                                // Position the light source
                                if (Terrain.activeTerrain != null)
                                {
                                    // Use sun azimuth/altitude
                                    PositionEarth(earthObject, terrainAzimuth, earthAzimuthDeg, earthAltitudeDeg, earthRadius);
                                }
                            }


                        }

                    }
                }
            }

        }
    }
}
