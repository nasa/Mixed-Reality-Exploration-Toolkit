// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.CSPICE;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.CSPICE.LunarModel
{
    public class LunarModel : CspiceInterface
    {
        /* SPICE uses these numbers to identify spacecraft and solar system objects */
        public const int    ID_EARTH    = 399;
        public const int    ID_MOON     = 301;
        public const int    ID_SUN      =  10;

        public const double MOON_RADIUS = 1737.4;

        // Bodies
        public const string BODY_EARTH  = "EARTH";
        public const string BODY_MOON   = "MOON";

        // Fixed References
        public const string FIXED_REFERENCE = "MOON_ME";

        // Computation Methods
        public const string METHOD_NADIR            = "NADIR/ELLIPSOID";
        public const string METHOD_INTERCEPT        = "INTERCEPT/ELLIPSOID";
        public const string METHOD_NADIR_DSK        = "NADIR/DSK/UNPRIORITIZED";        // NADIR/DSK/UNPRIORITIZED[/SURFACES = <surface list>]
        public const string METHOD_INTERCEPT_DSK    = "INTERCEPT/DSK/UNPRIORITIZED";    // INTERCEPT/DSK/UNPRIORITIZED[/SURFACES = <surface list>]

        // Aberration Corrections
        public const string ABCORR_ONEWAY_LIGHTTIME                     = "LT";
        public const string ABCORR_ONEWAY_LIGHTTIME_STELLAR             = "LT+S";
        public const string ABCORR_CONVERGEDNEWTONIAN_LIGHTTIME         = "CN";
        public const string ABCORR_CONVERGEDNEWTONIAN_LIGHTTIME_STELLAR = "CN+S";

        /*
        ======================================================================
        SubSolarPoint()

        Calculate the Moon's subsolar lon and lat (radians).  Use to sanity
        check sun_azalt().
        ====================================================================== */
        public static void SubSolarPoint(double et, out double longitude, out double latitude)
        {
            double[] spoint = new double[3];
            double trgepc;
            double[] srfvec = new double[3];
            double radius;

            SubSolarPoint(
                METHOD_INTERCEPT,
                BODY_MOON,
                et,
                FIXED_REFERENCE,
                ABCORR_ONEWAY_LIGHTTIME_STELLAR,
                BODY_EARTH,
                spoint,
                out trgepc,
                srfvec);

            Rectangular2LatitudinalCoordinates(
                spoint,
                out radius,
                out longitude,
                out latitude);
        }

        /*
        ======================================================================
        Calculate Sun vector at location (lon, lat, el) on the Moon at time t

        See naif.jpl.nasa.gov/pipermail/spice_discussion/2010-July/
        000307.html, but with an important caveat.  Instead of finding Y =
        Z cross UP, which isn't right, we find X = UP cross Z.

        UP is (0, 0, 1), the north pole vector.  Z is the surface normal at
        the location defined by (lon, lat).  UP and Z define a plane (think of
        the meridian containing UP and Z), and X is perpendicular to that
        plane, whence X = UP cross Z.  twovec_c() takes care of finding Y as
        the cross of X and Z and then forming the transformation matrix.
        ====================================================================== */
        public static void SunVector(double et,
            double longitude, double latitude, double elevation,
            double[] sunvec)
        {
            double[] locpos = new double[3];
            double[] sunpos = new double[3];
            double lt;
            double[,] m = new double[3,3];
            double[] X = new double[3];
            double[] Z = new double[3];
            double[] up = new double[] { 0.0, 0.0, 1.0 };

            /* the surface point in Moon-fixed cartesian coordinates */
            Latitudinal2RectangularCoordinates(
                MOON_RADIUS + elevation,
                longitude,
                latitude,
                locpos);

            /* the Sun in Moon-fixed, selenocentric coordinates */
            SPKernelEasyPosition(
                ID_SUN,
                et,
                FIXED_REFERENCE,
                ABCORR_ONEWAY_LIGHTTIME_STELLAR,
                ID_MOON,
                sunpos,
                out lt);

            /* the Sun in Moon-fixed, topocentric coordinates */
            VectorSubtraction(
                sunpos,
                locpos,
                sunvec);

            /* Z is just the normalized position vector of the surface point */
            UnitVector(
                locpos,
                Z);

            /* X is perpedicular to the Z,UP plane */
            UnitizedCrossProduct(
                up,
                Z,
                X);

            /* create the transformation matrix */
            TwoVectorTransformation(
                Z,
                3,
                X,
                1,
                m);

            /* transform the Sun vector into local coordinates */
            MatrixTimesVector(
                m,
                sunvec,
                sunvec);
        }

        /*
        ======================================================================
        Calculate Sun azimuth and altitude at location (lon, lat, el) on the
        Moon (el in km, the rest in degrees) at time t.

        This'll fail at lat=+/-90, since UP and Z are coincident. But for
        those two locations, azimuth is arbitrary anyway.  Special handling
        for this corner case is omitted for clarity.
        ====================================================================== */
        public static void SunAzimuthAltitude(double[] sunvec,
            out double radius, out double azimuth, out double altitude)
        {
            /* convert to angles */
            Rectangular2LatitudinalCoordinates(
                sunvec,
                out radius,
                out azimuth,
                out altitude);

            /* polar coordinates are counterclockwise around Z from X, -180 to 180,
               but azimuth is clockwise from north, 0 to 360 */
            azimuth = ToRadians(90.0) - azimuth;
            if (azimuth < 0.0) azimuth += ToRadians(360.0);
        }

        /*
        ======================================================================
        Calculate Earth vector at location (lon, lat, el) on the Moon at time t

        See naif.jpl.nasa.gov/pipermail/spice_discussion/2010-July/
        000307.html, but with an important caveat.  Instead of finding Y =
        Z cross UP, which isn't right, we find X = UP cross Z.

        UP is (0, 0, 1), the north pole vector.  Z is the surface normal at
        the location defined by (lon, lat).  UP and Z define a plane (think of
        the meridian containing UP and Z), and X is perpendicular to that
        plane, whence X = UP cross Z.  twovec_c() takes care of finding Y as
        the cross of X and Z and then forming the transformation matrix.
        ====================================================================== */
        public static void EarthVector(double et,
            double longitude, double latitude, double elevation,
            double[] earthvec)
        {
            double[] locpos = new double[3];
            double[] earthpos = new double[3];
            double lt;
            double[,] m = new double[3, 3];
            double[] X = new double[3];
            double[] Z = new double[3];
            double[] up = new double[] { 0.0, 0.0, 1.0 };

            /* the surface point in Moon-fixed cartesian coordinates */
            Latitudinal2RectangularCoordinates(
                MOON_RADIUS + elevation,
                longitude,
                latitude,
                locpos);

            /* the Sun in Moon-fixed, selenocentric coordinates */
            SPKernelEasyPosition(
                ID_EARTH,
                et,
                FIXED_REFERENCE,
                ABCORR_ONEWAY_LIGHTTIME_STELLAR,
                ID_MOON,
                earthpos,
                out lt);

            /* the Sun in Moon-fixed, topocentric coordinates */
            VectorSubtraction(
                earthpos,
                locpos,
                earthvec);

            /* Z is just the normalized position vector of the surface point */
            UnitVector(
                locpos,
                Z);

            /* X is perpedicular to the Z,UP plane */
            UnitizedCrossProduct(
                up,
                Z,
                X);

            /* create the transformation matrix */
            TwoVectorTransformation(
                Z,
                3,
                X,
                1,
                m);

            /* transform the Sun vector into local coordinates */
            MatrixTimesVector(
                m,
                earthvec,
                earthvec);
        }

        /*
        ======================================================================
        Calculate Earth azimuth and altitude at location (lon, lat, el) on the
        Moon (el in km, the rest in degrees) at time t.

        This'll fail at lat=+/-90, since UP and Z are coincident. But for
        those two locations, azimuth is arbitrary anyway.  Special handling
        for this corner case is omitted for clarity.
        ====================================================================== */
        public static void EarthAzimuthAltitude(double[] earthvec,
            out double radius, out double azimuth, out double altitude)
        {
            /* convert to angles */
            Rectangular2LatitudinalCoordinates(
                earthvec,
                out radius,
                out azimuth,
                out altitude);

            /* polar coordinates are counterclockwise around Z from X, -180 to 180,
               but azimuth is clockwise from north, 0 to 360 */
            azimuth = ToRadians(90.0) - azimuth;
            if (azimuth < 0.0) azimuth += ToRadians(360.0);
        }

    }
}
