// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CSPICE
{
    public class CspiceInterface
    {
        public const char NULL_CHAR = char.MinValue;
        public static char[] TRIM_CHARS = { ' ', NULL_CHAR };

        public enum ErrorMessageType
        {
            Short,
            Explain,
            Long
        }

        public enum ErrorOperationType
        {
            Set,
            Get
        }

        /* SPICE data files */
        public const string META_KERNEL = "kernels.tm";
        public const string ERROR_DEVICE = "NULL";

#if !HOLOLENS_BUILD
        /********************************************************************
         * CSPICE Functions
         ********************************************************************/
        [DllImport("kernel32.dll", EntryPoint = "SetDllDirectory", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool I_SetDllDirectory(string lpPathName);

        [DllImport("cspice.dll", EntryPoint = "dpr_c")]
        private static extern double I_DegreesPerRadian();

        [DllImport("cspice.dll", EntryPoint = "erract_c")]
        private static extern void I_ErrorAction(
            [MarshalAs(UnmanagedType.LPArray)] byte[] _op,
            int _lenout,
            [MarshalAs(UnmanagedType.LPArray)] [In, Out] byte[] _action);

        [DllImport("cspice.dll", EntryPoint = "errdev_c")]
        private static extern void I_ErrorDevice(
            [MarshalAs(UnmanagedType.LPArray)] byte[] _op,
            int _lenout,
            [MarshalAs(UnmanagedType.LPArray)] [In, Out] byte[] _device);

        [DllImport("cspice.dll", EntryPoint = "errprt_c")]
        private static extern void I_ErrorPrint(
            [MarshalAs(UnmanagedType.LPArray)] byte[] _op,
            int _lenout,
            [MarshalAs(UnmanagedType.LPArray)] [In, Out] byte[] _list);

        [DllImport("cspice.dll", EntryPoint = "failed_c")]
        private static extern bool I_Failed();

        [DllImport("cspice.dll", EntryPoint = "furnsh_c")]
        private static extern void I_FurnishSpiceKernel(
            [MarshalAs(UnmanagedType.LPArray)] byte[] _kernel);

        [DllImport("cspice.dll", EntryPoint = "getmsg_c")]
        private static extern void I_GetErrorMessage(
            [MarshalAs(UnmanagedType.LPArray)] byte[] _option,
            int _lenout,
            [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] _msg);

        [DllImport("cspice.dll", EntryPoint = "latrec_c")]
        private static extern void I_Latitudinal2RectangularCoordinates(
            double _radius,
            double _longitude,
            double _latitude,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _rectan);

        [DllImport("cspice.dll", EntryPoint = "mxv_c")]
        private static extern void I_MatrixTimesVector(
            [MarshalAs(UnmanagedType.LPArray)] double[,] _m1,
            [MarshalAs(UnmanagedType.LPArray)] double[] _vin,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _vout);

        [DllImport("cspice.dll", EntryPoint = "reclat_c")]
        private static extern void I_Rectangular2LatitudinalCoordinates(
            [MarshalAs(UnmanagedType.LPArray)] double[] _rectan,
            out double _radius,
            out double _longitude,
            out double _latitude);

        [DllImport("cspice.dll", EntryPoint = "reset_c")]
        private static extern bool I_ErrorStatusReset();

        [DllImport("cspice.dll", EntryPoint = "rpd_c")]
        private static extern double I_RadiansPerDegree();

        [DllImport("cspice.dll", EntryPoint = "spkezp_c")]
        private static extern void I_SPKernelEasyPosition(
            long _targ,
            double _et,
            [MarshalAs(UnmanagedType.LPArray)] byte[] _ref,
            [MarshalAs(UnmanagedType.LPArray)] byte[] _abcorr,
            long _obs,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _ptarg,
            out double _lt);

        [DllImport("cspice.dll", EntryPoint = "subslr_c")]
        private static extern void I_SubSolarPoint(
            [MarshalAs(UnmanagedType.LPArray)] byte[] _method,
            [MarshalAs(UnmanagedType.LPArray)] byte[] _target,
            double _et,
            [MarshalAs(UnmanagedType.LPArray)] byte[] _fixref,
            [MarshalAs(UnmanagedType.LPArray)] byte[] _abcorr,
            [MarshalAs(UnmanagedType.LPArray)] byte[] _obsrvr,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _spoint,
            out double _trgepc,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _srfvec);

        [DllImport("cspice.dll", EntryPoint = "timout_c")]
        private static extern void I_TimeOutput(
            double _et,
            [MarshalAs(UnmanagedType.LPArray)] byte[] _pictur,
            long _lenout,
            [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] _output);

        [DllImport("cspice.dll", EntryPoint = "twovec_c")]
        private static extern void I_TwoVectorTransformation(
            [MarshalAs(UnmanagedType.LPArray)] double[] _axdef,
            long _indexa,
            [MarshalAs(UnmanagedType.LPArray)] double[] _plndef,
            long _indexp,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[,] _mout);

        [DllImport("cspice.dll", EntryPoint = "ucrss_c")]
        private static extern void I_UnitizedCrossProduct(
            [MarshalAs(UnmanagedType.LPArray)] double[] _v1,
            [MarshalAs(UnmanagedType.LPArray)] double[] _v2,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _vout);

        [DllImport("cspice.dll", EntryPoint = "utc2et_c")]
        private static extern void I_UTC2EphemerisTime(
            [MarshalAs(UnmanagedType.LPArray)] byte[] _utcstr,
            out double _et);

        [DllImport("cspice.dll", EntryPoint = "vhat_c")]
        private static extern void I_UnitVector(
            [MarshalAs(UnmanagedType.LPArray)] double[] _v1,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _vout);

        [DllImport("cspice.dll", EntryPoint = "vsub_c")]
        private static extern void I_VectorSubtraction(
            [MarshalAs(UnmanagedType.LPArray)] double[] _v1,
            [MarshalAs(UnmanagedType.LPArray)] double[] _v2,
            [MarshalAs(UnmanagedType.LPArray)] [Out] double[] _vout);


        /*
        ======================================================================
        Constructor()
        ====================================================================== */
        static CspiceInterface()
        {
            string device = ERROR_DEVICE;
            ErrorDevice(ErrorOperationType.Set, ref device);

            string list = "ALL";
            ErrorPrint(ErrorOperationType.Set, ref list);

            string action = "REPORT";
            ErrorAction(ErrorOperationType.Set, ref action);

            // Reset the error status
            I_ErrorStatusReset();
        }

        /*
        ======================================================================
        CheckForError()
        ====================================================================== */
        static void CheckForError()
        {
            // Check for failure code
            if (I_Failed())
            {
                string message;
                CspiceException e = new CspiceException();

                // Extract the short message
                GetErrorMessage(ErrorMessageType.Short, out message);
                e.ShortMessage = message;

                // Extract the short message explanation
                GetErrorMessage(ErrorMessageType.Explain, out message);
                e.ExplainMessage = message;

                // Extract the long message
                GetErrorMessage(ErrorMessageType.Long, out message);
                e.LongMessage = message;

                // Reset the error status
                I_ErrorStatusReset();

                throw e;
            }

        }

#region Angle functions
        /*
        ======================================================================
        DegreesPerRadian()

        Degrees per radian.
        ====================================================================== */
        public static double DegreesPerRadian()
        {
            return I_DegreesPerRadian();
        }

        /*
        ======================================================================
        ToRadians()

        Degrees to radians.
        ====================================================================== */
        public static double ToRadians(double degrees)
        {
            return (degrees * I_RadiansPerDegree());
        }

        /*
        ======================================================================
        RadiansPerDegree()

        Radians per degree.
        ====================================================================== */
        public static double RadiansPerDegree()
        {
            return I_RadiansPerDegree();
        }

        /*
        ======================================================================
        ToDegrees()

        Radians to degrees.
        ====================================================================== */
        public static double ToDegrees(double radians)
        {
            return (radians * I_DegreesPerRadian());
        }
#endregion

#region Error handling functions
        /*
        ======================================================================
        ErrorAction()

        Retrieve or set the default error action.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        op         I   Operation -- "GET" or "SET"
        action    I/O  Error response action
        ====================================================================== */
        public static void ErrorAction(ErrorOperationType op, ref string action)
        {
            byte[] dstBytes = new byte[256];
            byte[] srcBytes = Encoding.ASCII.GetBytes(action + char.MinValue);
            System.Buffer.BlockCopy(srcBytes, 0, dstBytes, 0, srcBytes.Length);

            // Set default error action
            I_ErrorAction(
                Encoding.ASCII.GetBytes(op.ToString() + char.MinValue),
                dstBytes.Length,
                dstBytes);

            // Check for error
            CheckForError();

            action = Encoding.ASCII.GetString(dstBytes).TrimEnd(TRIM_CHARS);
        }

        /*
        ======================================================================
        ErrorDevice()

        Retrieve or set the name of the current output device for error
        messages.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        op         I   The operation:  "GET" or "SET".
        device    I/O  The device name.
        ====================================================================== */
        public static void ErrorDevice(ErrorOperationType op, ref string device)
        {
            byte[] dstBytes = new byte[256];
            byte[] srcBytes = Encoding.ASCII.GetBytes(device + char.MinValue);
            System.Buffer.BlockCopy(srcBytes, 0, dstBytes, 0, srcBytes.Length);

            // Set default error device
            I_ErrorDevice(
                Encoding.ASCII.GetBytes(op.ToString() + char.MinValue),
                dstBytes.Length,
                dstBytes);

            // Check for error
            CheckForError();

            device = Encoding.ASCII.GetString(dstBytes).TrimEnd(TRIM_CHARS);
        }

        /*
        ======================================================================
        ErrorPrint()

        Retrieve or set the list of error message items to be output when an
        error is detected.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        op         I   The operation:  "GET" or "SET".
        list      I/O  Specification of error messages to be output.
        ====================================================================== */
        public static void ErrorPrint(ErrorOperationType op, ref string list)
        {
            byte[] dstBytes = new byte[256];
            byte[] srcBytes = Encoding.ASCII.GetBytes(list + char.MinValue);
            System.Buffer.BlockCopy(srcBytes, 0, dstBytes, 0, srcBytes.Length);

            // Set default error output
            I_ErrorPrint(
                Encoding.ASCII.GetBytes(op.ToString() + char.MinValue),
                dstBytes.Length,
                dstBytes);

            // Check for error
            CheckForError();

            list = Encoding.ASCII.GetString(dstBytes).TrimEnd(TRIM_CHARS);
        }

        /*
        ======================================================================
        GetErrorMessage()

        Retrieve or set the list of error message items to be output when an
        error is detected.

        VARIABLE  I/O  DESCRIPTION 
        --------  ---  -------------------------------------------------- 
        option     I   Indicates type of error message
        message    O   The error message to be retrieved
        ====================================================================== */
        public static void GetErrorMessage(ErrorMessageType option, out string message)
        {
            byte[] dstBytes = new byte[512];

            // Get Error Message
            I_GetErrorMessage(
                Encoding.ASCII.GetBytes(option.ToString() + char.MinValue),
                dstBytes.Length,
                dstBytes);

            message = Encoding.ASCII.GetString(dstBytes).TrimEnd(TRIM_CHARS);
        }
#endregion

#region Time functions
        /*
        ======================================================================
        TimeOutput()

        This routine converts an input epoch represented in TDB seconds past
        the TDB epoch of J2000 to a character string formatted to the
        specifications of a user's format picture.

        Variable  I/O  Description
        --------  ---  --------------------------------------------------
        et         I   An epoch in seconds past the ephemeris epoch J2000.
        format     I   A format specification for the output string.
        lenout     I   The length of the output string plus 1.
        output     O   A string representation of the input epoch.
        ====================================================================== */
        public static void TimeOutput(double et, string format, out string output)
        {
            byte[] tmpOutput = new byte[128];

            // Convert the time to a string
            I_TimeOutput(
                et,
                Encoding.ASCII.GetBytes(format + char.MinValue),
                tmpOutput.Length,
                tmpOutput);

            // Check for error
            CheckForError();

            output = Encoding.ASCII.GetString(tmpOutput).TrimEnd(TRIM_CHARS);
        }

        /*
        ======================================================================
        UTC2EphemerisTime()

        Convert an input time from Calendar or Julian Date format, UTC, to
        ephemeris seconds past J2000.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        utc        I   Input time string, UTC.
        et         O   Output epoch, ephemeris seconds past J2000.
        ====================================================================== */
        public static void UTC2EphemerisTime(string utc, out double et)
        {
            // Convert the UTC string to an ephemeris time
            I_UTC2EphemerisTime(
                Encoding.ASCII.GetBytes(utc + char.MinValue),
                out et);

            // Check for error
            CheckForError();
        }
#endregion

        /*
        ======================================================================
        LoadMetaKernel()

        Load the meta kernel with the SPICE kernels to use for the program.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        path       I   Path of the meta kernel file
        file       I   Meta kernel file name. Default is defined by the
                       constant: META_KERNEL
        ====================================================================== */
        public static bool LoadMetaKernel(string path, string file = META_KERNEL)
        {
            bool result = false;
            if (path != "")
            {
                // Fix the path separators
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    
                // Create the meta kernel file reference
                string kernel_file = path + "\\" + file;

                // Load the meta kernel file
                I_FurnishSpiceKernel(Encoding.ASCII.GetBytes(kernel_file + char.MinValue));

                // Check for error
                CheckForError();

                result = true;
            }

            return result;
        }

#region Coordinate functions
        /*
        ======================================================================
        Latitudinal2RectangularCoordinates()

        Convert from latitudinal coordinates to rectangular coordinates.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        radius     I   Distance of a point from the origin.
        longitude  I   Longitude of point in radians [-pi, pi]
        latitude   I   Latitude of point in radians [-pi/2, pi/2]
        coords     O   Rectangular coordinates of the point.
        ====================================================================== */
        public static void Latitudinal2RectangularCoordinates(double radius, double longitude, double latitude,
            /*out*/ double[] coords)
        {
            /* the surface point in Moon-fixed cartesian coordinates */
            I_Latitudinal2RectangularCoordinates(
                radius,
                longitude,
                latitude,
                coords);
        }

        /*
        ======================================================================
        Rectangular2LatitudinalCoordinates()

        Convert from rectangular coordinates to latitudinal coordinates.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        coords     I   Rectangular coordinates of a point.
        radius     O   Distance of the point from the origin.
        longitude  O   Longitude of the point in radians [-pi, pi]
        latitude   O   Latitude of the point in radians [-pi/2, pi/2]
        ====================================================================== */
        public static void Rectangular2LatitudinalCoordinates(double[] coords,
            out double radius, out double longitude, out double latitude)
        {
            /* change rectangular coordinates into radians */
            I_Rectangular2LatitudinalCoordinates(
                coords,
                out radius,
                out longitude,
                out latitude);
        }
#endregion

#region Matrix Functions
        /*
        ======================================================================
        MatrixTimesVector()

        Multiply a 3x3 matrix with a 3-dimensional vector.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        min        I   3x3 double precision matrix.
        vin        I   3-dimensional double precision vector.
        vout       O   3-dimensional double precision vector. vout is
                        the product m1*vin.
        ====================================================================== */
        public static void MatrixTimesVector(
            double[,] min,
            double[] vin,
            /*out*/ double[] vout)
        {
            /* Matrix times vector, 3x3 */
            I_MatrixTimesVector(
                min,
                vin,
                vout);

            // Check for error
            CheckForError();
        }

        /*
        ======================================================================
        TwoVectorTransformation()

        Compute the normalized cross product of two 3-vectors.

        VARIABLE  I/O  DESCRIPTION 
        --------  ---  ------------------------------------------------- 
        vaxis      I   Vector defining a principal axis. 
        indexa     I   Principal axis number of axdef (X=1, Y=2, Z=3). 
        vplane     I   Vector defining (with axdef) a principal plane. 
        indexp     I   Second axis number (with indexa) of principal 
                       plane. 
        mout       O   Output rotation matrix. 
        ====================================================================== */
        public static void TwoVectorTransformation(
            double[] vaxis,
            long indexa,
            double[] vplane,
            long indexp,
            /*out*/double[,] mout)
        {
            /* Two vectors defining an orthonormal frame */
            I_TwoVectorTransformation(
                vaxis,
                indexa,
                vplane,
                indexp,
                mout);

            // Check for error
            CheckForError();
        }
#endregion

#region Vector Functions
        /*
        ======================================================================
        UnitizedCrossProduct()

        Compute the normalized cross product of two 3-vectors.

        VARIABLE  I/O  DESCRIPTION 
        --------  ---  -------------------------------------------------- 
        v1         I     Left vector for cross product. 
        v2         I     Right vector for cross product. 
        vout       O     Normalized cross product (v1xv2) / |v1xv2|. 
        ====================================================================== */
        public static void UnitizedCrossProduct(
            double[] v1,
            double[] v2,
            /*out*/ double[] vout)
        {
            /* Unitized cross product, 3 dimensions */
            I_UnitizedCrossProduct(
                v1,
                v2,
                vout);
        }

        /*
        ======================================================================
        UnitVector()

        Find the "V-Hat" unit vector along a double precision 3-dimensional
        vector.

        VARIABLE  I/O  DESCRIPTION
        --------  ---  --------------------------------------------------
        v1         I   Vector to be unitized.
        vout       O   Unit vector v1 / |v1|.
        ====================================================================== */
        public static void UnitVector(double[] v1, /*out*/ double[] vout)
        {
            /* "V-Hat", unit vector along V, 3 dimensions */
            I_UnitVector(
                v1,
                vout);
        }

        /*
        ======================================================================
        VectorSubtraction()

        Compute the difference between two 3-dimensional, double precision
        vectors.

        VARIABLE  I/O  DESCRIPTION 
        --------  ---  -------------------------------------------------- 
        v1         I   First vector (minuend). 
        v2         I   Second vector (subtrahend). 
        vout       O   Difference vector, v1 - v2. vout can overwrite 
                       either v1 or v2. 
        ====================================================================== */
        public static void VectorSubtraction(double[] v1, double[] v2,
            /*out*/ double[] vout)
        {
            /* Vector subtraction, 3 dimensions */
            I_VectorSubtraction(
                v1,
                v2,
                vout);
        }
#endregion

        /*
        ======================================================================
        SPKernelEasyPosition()

        Return the position of a target body relative to an observing body,
        optionally corrected for light time (planetary aberration) and stellar
        aberration.

        Variable  I/O  Description
        --------  ---  -------------------------------------------------- 
        target     I   Target body NAIF ID code. 
        et         I   Observer epoch. 
        refframe   I   Reference frame of output position vector. 
        abcorr     I   Aberration correction flag. 
        observer   I   Observing body NAIF ID code. 
        position   O   Position of target. 
        lt         O   One way light time between observer and target. 
        ====================================================================== */
        public static void SPKernelEasyPosition(
            long target,
            double et,
            string refframe,
            string abcorr,
            long observer,
            /*out*/double[] position,
            out double lt)
        {
            /* S/P Kernel, easy position */
            I_SPKernelEasyPosition(
                target,
                et,
                Encoding.ASCII.GetBytes(refframe + char.MinValue),
                Encoding.ASCII.GetBytes(abcorr + char.MinValue),
                observer,
                position,
                out lt);

            // Check for error
            CheckForError();
        }

        /*
        ======================================================================
        SubSolarPoint()

        Compute the rectangular coordinates of the sub-solar point on a target
        body at a specified epoch, optionally corrected for light time and
        stellar aberration. 
 
        The surface of the target body may be represented by a triaxial 
        ellipsoid or by topographic data provided by DSK files. 
 
        Variable  I/O  Description 
        --------  ---  -------------------------------------------------- 
        method     I   Computation method
        target     I   Name of target body
        et         I   Epoch in ephemeris seconds past J2000 TDB
        fixref     I   Body-fixed, body-centered target body frame
        abcorr     I   Aberration correction
        observer   I   Name of observing body
        spoint     O   Sub-solar point on the target body
        trgepc     O   Sub-solar point epoch
        srfvec     O   Vector from observer to sub-solar point
        ====================================================================== */
        public static void SubSolarPoint(
            string method,
            string target,
            double et,
            string fixref,
            string abcorr,
            string observer,
            /*out*/ double[] spoint,
            out double trgepc,
            /*out*/ double[] srfvec)
        {
            /* Sub-solar point */
            I_SubSolarPoint(
                Encoding.ASCII.GetBytes(method + char.MinValue),
                Encoding.ASCII.GetBytes(target + char.MinValue),
                et,
                Encoding.ASCII.GetBytes(fixref + char.MinValue),
                Encoding.ASCII.GetBytes(abcorr + char.MinValue),
                Encoding.ASCII.GetBytes(observer + char.MinValue),
                spoint,
                out trgepc,
                srfvec);

            // Check for error
            CheckForError();
        }
#endif
    }

}
