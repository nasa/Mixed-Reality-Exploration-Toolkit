// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GOV.NASA.GSFC.XR.MRET.Data.Telemetry
{
    /// <summary>
    /// TelemetryNumber
    /// 
    /// Extends Telemetry to convert the telemetry value into a number result.<br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TelemetryNumber : Telemetry
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryNumber);

        /// <summary>
        /// Indicates if the supplied object is a numeric type
        /// </summary>
        /// <param name="x">The object to test</param>
        /// <returns>A boolean value indicating whether or not the object is a numeric type</returns>
        protected static bool IsNumeric(object x)
        {
            return ((x != null) && IsNumeric(x.GetType()));
        }

        /// <summary>
        /// Indicates if the supplied type is numeric
        /// </summary>
        /// <param name="type">The <code>Type</code> to test</param>
        /// <returns>A boolean value indicating whether or not the type is numeric</returns>
        protected static bool IsNumeric(Type type)
        {
            return IsNumeric(type, Type.GetTypeCode(type));
        }

        /// <summary>
        /// Indicates if the supplied type is numeric
        /// </summary>
        /// <param name="type">The <code>Type</code> to test</param>
        /// <param name="typeCode">The <code>TypeCode</code> of type</param>
        /// <returns>A boolean value indicating whether or not the type is numeric</returns>
        protected static bool IsNumeric(Type type, TypeCode typeCode)
        {
            return (typeCode == TypeCode.Decimal || (type.IsPrimitive && (typeCode != TypeCode.Object) && (typeCode != TypeCode.Boolean) && (typeCode != TypeCode.Char)));
        }

        // Accessor for the telemetry value as a sbyte
        public sbyte SignedByteValue { get => GetSignedByte(Value); }

        // Accessor for the telemetry value as a byte
        public byte ByteValue { get => GetByte(Value); }

        // Accessor for the telemetry value as a short
        public short ShortValue { get => GetShort(Value); }

        // Accessor for the telemetry value as a ushort
        public ushort UnsignedShortValue { get => GetUnsignedShort(Value); }

        // Accessor for the telemetry value as a int
        public int IntValue { get => GetInt(Value); }

        // Accessor for the telemetry value as a uint
        public uint UnsignedIntValue { get => GetUnsignedInt(Value); }

        // Accessor for the telemetry value as a long
        public long LongValue { get => GetLong(Value); }

        // Accessor for the telemetry value as a ulong
        public ulong UnsignedLongValue { get => GetUnsignedLong(Value); }

        // Accessor for the telemetry value as a float
        public float FloatValue { get => GetFloat(Value); }

        // Accessor for the telemetry value as a double
        public double DoubleValue { get => GetDouble(Value); }

        // Accessor for the telemetry value as a decimal
        public decimal DecimalValue { get => GetDecimal(Value); }

        /// <summary>
        /// Obtains the signed byte representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The sbyte representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the unsigned byte representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The byte representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the short integer representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The short representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the unsigned short integer representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The ushort representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the integer representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The int representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the unsigned integer representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The uint representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the long integer representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The long representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the unsigned integer representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The ulong representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the float (single precision) representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The float representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the double (double precision) representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The double representation of the telemetry value</returns>
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

        /// <summary>
        /// Obtains the decimal representation of the telemetry value
        /// </summary>
        /// <param name="value">An <code>object</code> representing the telemetry value</param>
        /// <returns>The decimal representation of the telemetry value</returns>
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