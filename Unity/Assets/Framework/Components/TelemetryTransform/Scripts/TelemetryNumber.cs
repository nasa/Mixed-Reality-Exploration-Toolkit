// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GSFC.ARVR.MRET.Telemetry
{
    /**
     * TelemetryNumber
     * 
     * Extends Telemetry to convert the telemetry value into a number result.<br>
     * 
     * @author Jeffrey Hosler
     */
    public class TelemetryNumber : Telemetry
    {
        public new static readonly string NAME = nameof(TelemetryNumber);

        /**
         * Indicates if the supplied object is a numeric type
         * 
         * @param x The object to test
         * 
         * @return A boolean value indicating whether or not the object is a numeric type
         */
        protected static bool IsNumeric(object x)
        {
            return (x == null ? false : IsNumeric(x.GetType()));
        }

        /**
         * Indicates if the supplied type is numeric
         * 
         * @param type The <code>Type</code> to test
         * 
         * @return A boolean value indicating whether or not the type is numeric
         */
        protected static bool IsNumeric(Type type)
        {
            return IsNumeric(type, Type.GetTypeCode(type));
        }

        /**
         * Indicates if the supplied type is numeric
         * 
         * @param type The <code>Type</code> to test
         * @param typeCode The <code>TypeCode</code> of type
         * 
         * @return A boolean value indicating whether or not the type is numeric
         */
        protected static bool IsNumeric(Type type, TypeCode typeCode)
        {
            return (typeCode == TypeCode.Decimal || (type.IsPrimitive && (typeCode != TypeCode.Object) && (typeCode != TypeCode.Boolean) && (typeCode != TypeCode.Char)));
        }

        // Accessor for the telemetry value as a sbyte
        public sbyte SignedByteValue
        {
            get
            {
                return GetSignedByte(Value);
            }
        }

        // Accessor for the telemetry value as a byte
        public byte ByteValue
        {
            get
            {
                return GetByte(Value);
            }
        }

        // Accessor for the telemetry value as a short
        public short ShortValue
        {
            get
            {
                return GetShort(Value);
            }
        }

        // Accessor for the telemetry value as a ushort
        public ushort UnsignedShortValue
        {
            get
            {
                return GetUnsignedShort(Value);
            }
        }

        // Accessor for the telemetry value as a int
        public int IntValue
        {
            get
            {
                return GetInt(Value);
            }
        }

        // Accessor for the telemetry value as a uint
        public uint UnsignedIntValue
        {
            get
            {
                return GetUnsignedInt(Value);
            }
        }

        // Accessor for the telemetry value as a long
        public long LongValue
        {
            get
            {
                return GetLong(Value);
            }
        }

        // Accessor for the telemetry value as a ulong
        public ulong UnsignedLongValue
        {
            get
            {
                return GetUnsignedLong(Value);
            }
        }

        // Accessor for the telemetry value as a float
        public float FloatValue
        {
            get
            {
                return GetFloat(Value);
            }
        }

        // Accessor for the telemetry value as a double
        public double DoubleValue
        {
            get
            {
                return GetDouble(Value);
            }
        }

        // Accessor for the telemetry value as a decimal
        public decimal DecimalValue
        {
            get
            {
                return GetDecimal(Value);
            }
        }

        /**
         * Obtains the signed byte representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The sbyte representation of the telemetry value
         */
        protected virtual sbyte GetSignedByte(object value)
        {
            sbyte result = 0;

            if (IsNumeric(value))
            {
                result = (sbyte)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                sbyte.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the unsigned byte representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The byte representation of the telemetry value
         */
        protected virtual byte GetByte(object value)
        {
            byte result = 0;

            if (IsNumeric(value))
            {
                result = (byte)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                byte.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the short integer representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The short representation of the telemetry value
         */
        protected virtual short GetShort(object value)
        {
            short result = 0;

            if (IsNumeric(value))
            {
                result = (short)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                short.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the unsigned short integer representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The ushort representation of the telemetry value
         */
        protected virtual ushort GetUnsignedShort(object value)
        {
            ushort result = 0;

            if (IsNumeric(value))
            {
                result = (ushort)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                ushort.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the integer representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The int representation of the telemetry value
         */
        protected virtual int GetInt(object value)
        {
            int result = 0;

            if (IsNumeric(value))
            {
                result = (int)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                int.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the unsigned integer representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The uint representation of the telemetry value
         */
        protected virtual uint GetUnsignedInt(object value)
        {
            uint result = 0;

            if (IsNumeric(value))
            {
                result = (uint)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                uint.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the long integer representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The long representation of the telemetry value
         */
        protected virtual long GetLong(object value)
        {
            long result = 0;

            if (IsNumeric(value))
            {
                result = (long)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                long.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the unsigned integer representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The ulong representation of the telemetry value
         */
        protected virtual ulong GetUnsignedLong(object value)
        {
            ulong result = 0;

            if (IsNumeric(value))
            {
                result = (ulong)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                ulong.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the float (single precision) representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The float representation of the telemetry value
         */
        protected virtual float GetFloat(object value)
        {
            float result = 0;

            if (IsNumeric(value))
            {
                result = (float)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                float.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the double (double precision) representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The double representation of the telemetry value
         */
        protected virtual double GetDouble(object value)
        {
            double result = 0;

            if (IsNumeric(value))
            {
                result = (double)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                double.TryParse(value.ToString(), out result);
            }

            return result;
        }

        /**
         * Obtains the decimal representation of the telemetry value
         * 
         * @param An <code>object</code> representing the telemetry value
         * 
         * @return The decimal representation of the telemetry value
         */
        protected virtual decimal GetDecimal(object value)
        {
            decimal result = 0;

            if (IsNumeric(value))
            {
                result = (decimal)value;
            }
            else if (value != null)
            {
                // Try as a parsable string
                decimal.TryParse(value.ToString(), out result);
            }

            return result;
        }

    }

}