// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Color;
using GOV.NASA.GSFC.XR.Utilities.Renderer;
using GOV.NASA.GSFC.XR.Utilities.Transforms;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Helpers;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;
using GOV.NASA.GSFC.XR.MRET.Integrations.Matlab;

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_9
{
    /// <remarks>
    /// History:
    /// 09 Sep 2021: Created (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// SchemaUtil
    /// 
    /// A utility class containing a collection of functions for deserializing and
    /// serializing Unity classes into the schema v0.9 types.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class SchemaUtil
    {
        public static string ClassName => nameof(SchemaUtil);

        #region Unit Conversion
        /// <summary>
        /// Converts the length value in the supplied units to a Unity unit value 
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="units">The <code>LengthUnitType</code> units of the value</param>
        /// <returns>The converted value in Unity units</returns>
        /// 
        public static float LengthToUnityUnits(float value, LengthUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            float unitDivisor;
            switch (units)
            {
                case LengthUnitType.Nanometer:
                    unitDivisor = 1000000000.0f;
                    break;
                case LengthUnitType.Micrometer:
                    unitDivisor = 1000000.0f;
                    break;
                case LengthUnitType.Millimeter:
                    unitDivisor = 1000.0f;
                    break;
                case LengthUnitType.Centimeter:
                    unitDivisor = 100.0f;
                    break;
                case LengthUnitType.Decimeter:
                    unitDivisor = 10.0f;
                    break;
                case LengthUnitType.Kilometer:
                    unitDivisor = 0.001f;
                    break;
                case LengthUnitType.Inch:
                    unitDivisor = 39.37007874f;
                    break;
                case LengthUnitType.Foot:
                    unitDivisor = 3.280839895f;
                    break;
                case LengthUnitType.Yard:
                    unitDivisor = 1.093613298f;
                    break;
                case LengthUnitType.Mile:
                    unitDivisor = 0.0006213711922f;
                    break;
                case LengthUnitType.Meter:
                default:
                    // Unity units
                    unitDivisor = 1.0f;
                    break;
            }

            // Adjust the units
            return value / unitDivisor;
        }

        /// <summary>
        /// Converts the supplied Unity unit value to a length value in the supplied units
        /// </summary>
        /// <param name="value">Unity value to convert</param>
        /// <param name="units">The desired <code>LengthUnitType</code> units</param>
        /// <returns>The converted value in the specified units</returns>
        /// 
        public static float UnityUnitsToLength(float value, LengthUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            // Serialize the length
            float unitMultiplier = 1f;
            // TODO: Need unit conversion library
            switch (units)
            {
                case LengthUnitType.Nanometer:
                    unitMultiplier = 1000000000.0f;
                    break;
                case LengthUnitType.Micrometer:
                    unitMultiplier = 1000000.0f;
                    break;
                case LengthUnitType.Millimeter:
                    unitMultiplier = 1000.0f;
                    break;
                case LengthUnitType.Centimeter:
                    unitMultiplier = 100.0f;
                    break;
                case LengthUnitType.Decimeter:
                    unitMultiplier = 10.0f;
                    break;
                case LengthUnitType.Kilometer:
                    unitMultiplier = 0.001f;
                    break;
                case LengthUnitType.Inch:
                    unitMultiplier = 39.37007874f;
                    break;
                case LengthUnitType.Foot:
                    unitMultiplier = 3.280839895f;
                    break;
                case LengthUnitType.Yard:
                    unitMultiplier = 1.093613298f;
                    break;
                case LengthUnitType.Mile:
                    unitMultiplier = 0.0006213711922f;
                    break;
                case LengthUnitType.Meter:
                default:
                    // Unity units
                    unitMultiplier = 1.0f;
                    break;
            }

            // Serialize out the value
            return value * unitMultiplier;
        }

        public static void DeserializeLength(LengthType serializedLength, ref float value)
        {
            // Check assertions
            if (serializedLength is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized length is null");
                value = float.NaN;
                return;
            }

            // Convert the units
            value = LengthToUnityUnits(serializedLength.Value, serializedLength.units);
        }

        public static void SerializeLength(float value, LengthType serializedLength)
        {
            // Check assertions
            if (serializedLength is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized length is null");
                return;
            }

            // Serialize out the value
            serializedLength.Value = UnityUnitsToLength(value, serializedLength.units);
        }

        /// <summary>
        /// Converts the mass value in the supplied units to a Unity unit value 
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="units">The <code>MassUnitType</code> units of the value</param>
        /// <returns>The converted value in Unity units</returns>
        /// 
        public static float MassToUnityUnits(float value, MassUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            float unitDivisor;
            switch (units)
            {
                case MassUnitType.Milligram:
                    unitDivisor = 1000.0f;
                    break;
                case MassUnitType.Centigram:
                    unitDivisor = 100.0f;
                    break;
                case MassUnitType.Decigram:
                    unitDivisor = 10.0f;
                    break;
                case MassUnitType.Kilogram:
                    unitDivisor = 0.001f;
                    break;
                case MassUnitType.Pound:
                    unitDivisor = 0.002204622622f;
                    break;
                case MassUnitType.Ounce:
                    unitDivisor = 0.03527396195f;
                    break;
                case MassUnitType.Gram:
                default:
                    // Unity units
                    unitDivisor = 1.0f;
                    break;
            }

            // Adjust the units
            return value / unitDivisor;
        }

        /// <summary>
        /// Converts the supplied Unity unit value to a mass value in the supplied units
        /// </summary>
        /// <param name="value">Unity value to convert</param>
        /// <param name="units">The desired <code>MassUnitType</code> units</param>
        /// <returns>The converted value in the specified units</returns>
        /// 
        public static float UnityUnitsToMass(float value, MassUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            // Serialize the mass
            float unitMultiplier = 1f;
            // TODO: Need unit conversion library
            switch (units)
            {
                case MassUnitType.Milligram:
                    unitMultiplier = 1000.0f;
                    break;
                case MassUnitType.Centigram:
                    unitMultiplier = 100.0f;
                    break;
                case MassUnitType.Decigram:
                    unitMultiplier = 10.0f;
                    break;
                case MassUnitType.Kilogram:
                    unitMultiplier = 0.001f;
                    break;
                case MassUnitType.Ounce:
                    unitMultiplier = 28.34952312f;
                    break;
                case MassUnitType.Pound:
                    unitMultiplier = 0.0625f;
                    break;
                case MassUnitType.Gram:
                default:
                    // Unity units
                    unitMultiplier = 1.0f;
                    break;
            }

            // Serialize out the value
            return value * unitMultiplier;
        }

        public static void DeserializeMass(MassType serializedMass, ref float value)
        {
            // Check assertions
            if (serializedMass is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized mass is null");
                value = float.NaN;
                return;
            }

            // Convert the units
            value = MassToUnityUnits(serializedMass.Value, serializedMass.units);
        }

        public static void SerializeMass(float value, MassType serializedMass)
        {
            // Check assertions
            if (serializedMass is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized mass is null");
                return;
            }

            // Serialize out the value
            serializedMass.Value = UnityUnitsToMass(value, serializedMass.units);
        }

        /// <summary>
        /// Converts the power value in the supplied units to a Unity unit value 
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="units">The <code>PowerUnitType</code> units of the value</param>
        /// <returns>The converted value in Unity units</returns>
        /// 
        public static float PowerToUnityUnits(float value, PowerUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            float unitDivisor;
            switch (units)
            {
                case PowerUnitType.Milliwatt:
                    unitDivisor = 1000.0f;
                    break;
                case PowerUnitType.Kilowatt:
                    unitDivisor = 0.001f;
                    break;
                case PowerUnitType.Watt:
                default:
                    // Unity units
                    unitDivisor = 1.0f;
                    break;
            }

            // Adjust the units
            return value / unitDivisor;
        }

        /// <summary>
        /// Converts the supplied Unity unit value to a power value in the supplied units
        /// </summary>
        /// <param name="value">Unity value to convert</param>
        /// <param name="units">The desired <code>PowerUnitType</code> units</param>
        /// <returns>The converted value in the specified units</returns>
        /// 
        public static float UnityUnitsToPower(float value, PowerUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            // Serialize the power
            float unitMultiplier = 1f;
            // TODO: Need unit conversion library
            switch (units)
            {
                case PowerUnitType.Milliwatt:
                    unitMultiplier = 1000.0f;
                    break;
                case PowerUnitType.Kilowatt:
                    unitMultiplier = 0.001f;
                    break;
                case PowerUnitType.Watt:
                default:
                    // Unity units
                    unitMultiplier = 1.0f;
                    break;
            }

            // Serialize out the value
            return value * unitMultiplier;
        }

        public static void DeserializePower(PowerType serializedPower, ref float value)
        {
            // Check assertions
            if (serializedPower is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized power is null");
                value = float.NaN;
                return;
            }

            // Convert the units
            value = PowerToUnityUnits(serializedPower.Value, serializedPower.units);
        }

        public static void SerializePower(float value, PowerType serializedPower)
        {
            // Check assertions
            if (serializedPower is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized power is null");
                return;
            }

            // Serialize out the value
            serializedPower.Value = UnityUnitsToPower(value, serializedPower.units);
        }

        /* FIXME: Defined in schema but not converted by the XSD tool because it's not used yet
         
        /// <summary>
        /// Converts the speed value in the supplied units to a Unity unit value 
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="units">The <code>SpeedUnitType</code> units of the value</param>
        /// <returns>The converted value in Unity units</returns>
        /// 
        public static float SpeedToUnityUnits(float value, SpeedUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            float unitDivisor;
            switch (units)
            {
                case SpeedUnitType.FeetperSecond:
                    unitDivisor = 3.280839895f;
                    break;
                case SpeedUnitType.MileperHour:
                    unitDivisor = 2.236936292f;
                    break;
                case SpeedUnitType.MeterperSecond:
                default:
                    // Unity units
                    unitDivisor = 1.0f;
                    break;
            }

            // Adjust the units
            return value / unitDivisor;
        }

        /// <summary>
        /// Converts the supplied Unity unit value to a speed value in the supplied units
        /// </summary>
        /// <param name="value">Unity value to convert</param>
        /// <param name="units">The desired <code>SpeedUnitType</code> units</param>
        /// <returns>The converted value in the specified units</returns>
        /// 
        public static float UnityUnitsToSpeed(float value, SpeedUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            // Serialize the length
            float unitMultiplier = 1f;
            // TODO: Need unit conversion library
            switch (units)
            {
                case SpeedUnitType.FeetperSecond:
                    unitMultiplier = 3.280839895f;
                    break;
                case SpeedUnitType.MileperHour:
                    unitMultiplier = 2.236936292f;
                    break;
                case SpeedUnitType.MeterperSecond:
                default:
                    // Unity units
                    unitMultiplier = 1.0f;
                    break;
            }

            // Serialize out the value
            return value * unitMultiplier;
        }

        public static void DeserializeSpeed(SpeedType serializedSpeed, ref float value)
        {
            // Check assertions
            if (serializedSpeed is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized speed is null");
                value = float.NaN;
                return;
            }

            // Convert the units
            value = SpeedToUnityUnits(serializedSpeed.Value, serializedSpeed.units);
        }

        public static void SerializeSpeed(float value, SpeedType serializedSpeed)
        {
            // Check assertions
            if (serializedSpeed is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized speed is null");
                return;
            }

            // Serialize out the value
            serializedSpeed.Value = UnityUnitsToSpeed(value, serializedSpeed.units);
        }
        */

        /// <summary>
        /// Converts the acceleration value in the supplied units to a Unity unit value 
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="units">The <code>AccelerationUnitType</code> units of the value</param>
        /// <returns>The converted value in Unity units</returns>
        /// 
        public static float AccelerationToUnityUnits(float value, AccelerationUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            float unitDivisor;
            switch (units)
            {
                case AccelerationUnitType.FeetperSecondSquared:
                    unitDivisor = 3.280839895f;
                    break;
                case AccelerationUnitType.MileperHourSquared:
                    unitDivisor = 2.236936292f;
                    break;
                case AccelerationUnitType.MeterperSecondSquared:
                case AccelerationUnitType.NewtonperKilogram:
                default:
                    // Unity units
                    unitDivisor = 1.0f;
                    break;
            }

            // Adjust the units
            return value / unitDivisor;
        }

        /// <summary>
        /// Converts the supplied Unity unit value to a acceleration value in the supplied units
        /// </summary>
        /// <param name="value">Unity value to convert</param>
        /// <param name="units">The desired <code>AccelerationUnitType</code> units</param>
        /// <returns>The converted value in the specified units</returns>
        /// 
        public static float UnityUnitsToAcceleration(float value, AccelerationUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            // Serialize the length
            float unitMultiplier = 1f;
            // TODO: Need unit conversion library
            switch (units)
            {
                case AccelerationUnitType.FeetperSecondSquared:
                    unitMultiplier = 3.280839895f;
                    break;
                case AccelerationUnitType.MileperHourSquared:
                    unitMultiplier = 2.236936292f;
                    break;
                case AccelerationUnitType.MeterperSecondSquared:
                case AccelerationUnitType.NewtonperKilogram:
                default:
                    // Unity units
                    unitMultiplier = 1.0f;
                    break;
            }

            // Serialize out the value
            return value * unitMultiplier;
        }

        public static void DeserializeAcceleration(AccelerationType serializedAcceleration, ref float value)
        {
            // Check assertions
            if (serializedAcceleration is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized acceleration is null");
                value = float.NaN;
                return;
            }

            // Convert the units
            value = AccelerationToUnityUnits(serializedAcceleration.Value, serializedAcceleration.units);
        }

        public static void SerializeAcceleration(float value, AccelerationType serializedAcceleration)
        {
            // Check assertions
            if (serializedAcceleration is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized acceleration is null");
                return;
            }

            // Serialize out the value
            serializedAcceleration.Value = UnityUnitsToAcceleration(value, serializedAcceleration.units);
        }

        /// <summary>
        /// Converts the frequency value in the supplied units to a Unity unit value 
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="units">The <code>FrequencyUnitType</code> units of the value</param>
        /// <returns>The converted value in Unity units</returns>
        /// 
        public static float FrequencyToUnityUnits(float value, FrequencyUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            float unitDivisor;
            switch (units)
            {
                case FrequencyUnitType.Millihertz:
                    unitDivisor = 1000.0f;
                    break;
                case FrequencyUnitType.Kilohertz:
                    unitDivisor = 0.001f;
                    break;
                case FrequencyUnitType.Hertz:
                default:
                    // Unity units
                    unitDivisor = 1.0f;
                    break;
            }

            // Adjust the units
            return 1f / (value / unitDivisor);
        }

        /// <summary>
        /// Converts the supplied Unity unit value to a frequency value in the supplied units
        /// </summary>
        /// <param name="value">Unity value to convert</param>
        /// <param name="units">The desired <code>FrequencyUnitType</code> units</param>
        /// <returns>The converted value in the specified units</returns>
        /// 
        public static float UnityUnitsToFrequency(float value, FrequencyUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            // Serialize the length
            float unitMultiplier = 1f;
            switch (units)
            {
                case FrequencyUnitType.Millihertz:
                    unitMultiplier = 1000.0f;
                    break;
                case FrequencyUnitType.Kilohertz:
                    unitMultiplier = 0.001f;
                    break;
                case FrequencyUnitType.Hertz:
                default:
                    // Unity units
                    unitMultiplier = 1.0f;
                    break;
            }

            // Adjust the units
            return 1f / (value * unitMultiplier);
        }

        public static void DeserializeFrequency(FrequencyType serializedFrequency, ref float value)
        {
            // Check assertions
            if (serializedFrequency is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized frequency is null");
                value = float.NaN;
                return;
            }

            // Convert the units
            value = FrequencyToUnityUnits(serializedFrequency.Value, serializedFrequency.units);
        }

        public static void SerializeFrequency(float value, FrequencyType serializedFrequency)
        {
            // Check assertions
            if (serializedFrequency is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized frequency is null");
                return;
            }

            // Serialize out the value
            serializedFrequency.Value = UnityUnitsToFrequency(value, serializedFrequency.units);
        }

        /// <summary>
        /// Converts the duration value in the supplied units to a Unity unit value 
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="units">The <code>DurationUnitType</code> units of the value</param>
        /// <returns>The converted value in Unity units</returns>
        /// 
        public static float DurationToUnityUnits(float value, DurationUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            float unitDivisor;
            switch (units)
            {
                case DurationUnitType.Year:
                    unitDivisor = 1f/31536000f;
                    break;
                case DurationUnitType.Month:
                    unitDivisor = 1f/2592000f; // 30 days
                    break;
                case DurationUnitType.Day:
                    unitDivisor = 1f/86400f;
                    break;
                case DurationUnitType.Hour:
                    unitDivisor = 1f/3600f;
                    break;
                case DurationUnitType.Minute:
                    unitDivisor = 1f/60f;
                    break;
                case DurationUnitType.Millisecond:
                    unitDivisor = 1000.0f;
                    break;

                case DurationUnitType.Second:
                default:
                    // Unity units
                    unitDivisor = 1.0f;
                    break;
            }

            // Adjust the units
            return value / unitDivisor;
        }

        /// <summary>
        /// Converts the supplied Unity unit value to a duration value in the supplied units
        /// </summary>
        /// <param name="value">Unity value to convert</param>
        /// <param name="units">The desired <code>DurationUnitType</code> units</param>
        /// <returns>The converted value in the specified units</returns>
        /// 
        public static float UnityUnitsToDuration(float value, DurationUnitType units)
        {
            if (float.IsInfinity(value) || float.IsNaN(value) || (value < 0))
            {
                return float.NaN;
            }

            // Serialize the length
            float unitMultiplier = 1f;
            // TODO: Need unit conversion library
            switch (units)
            {
                case DurationUnitType.Year:
                    unitMultiplier = 1f / 31536000f;
                    break;
                case DurationUnitType.Month:
                    unitMultiplier = 1f / 2592000f; // 30 days
                    break;
                case DurationUnitType.Day:
                    unitMultiplier = 1f / 86400f;
                    break;
                case DurationUnitType.Hour:
                    unitMultiplier = 1f / 3600f;
                    break;
                case DurationUnitType.Minute:
                    unitMultiplier = 1f / 60f;
                    break;
                case DurationUnitType.Millisecond:
                    unitMultiplier = 1000.0f;
                    break;

                case DurationUnitType.Second:
                default:
                    // Unity units
                    unitMultiplier = 1.0f;
                    break;
            }

            // Serialize out the value
            return value * unitMultiplier;
        }

        public static void DeserializeDuration(DurationType serializedDuration, ref float value)
        {
            // Check assertions
            if (serializedDuration is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized duration is null");
                value = float.NaN;
                return;
            }

            // Convert the units
            value = DurationToUnityUnits(serializedDuration.Value, serializedDuration.units);
        }

        public static void SerializeDuration(float value, DurationType serializedDuration)
        {
            // Check assertions
            if (serializedDuration is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized duration is null");
                return;
            }

            // Serialize out the value
            serializedDuration.Value = UnityUnitsToDuration(value, serializedDuration.units);
        }
        #endregion Unit Conversion

        #region Time
        public static void DeserializeTime(TimeType serializedTime, ref DateTime time)
        {
            if (serializedTime is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied time is null");
                time = default;
            }

            int year = 0, month = 0, day = 0;
            int hour = 0, minute = 0, second = 0, millis = 0;
            DateTimeKind timeZone = DateTimeKind.Utc;
            TimeStringType timeStrType = null;
            for (int i = 0; i < serializedTime.Items.Length; i++)
            {
                ItemsChoiceType3 itemsChoice = serializedTime.ItemsElementName[i];
                switch (itemsChoice)
                {
                    case ItemsChoiceType3.TimeString:
                        timeStrType = (TimeStringType)serializedTime.Items[i];
                        break;
                    case ItemsChoiceType3.Year:
                        int.TryParse(serializedTime.Items[i].ToString(), out year);
                        break;
                    case ItemsChoiceType3.Month:
                        int.TryParse(serializedTime.Items[i].ToString(), out month);
                        break;
                    case ItemsChoiceType3.Day:
                        int.TryParse(serializedTime.Items[i].ToString(), out day);
                        break;
                    case ItemsChoiceType3.Hour:
                        int.TryParse(serializedTime.Items[i].ToString(), out hour);
                        break;
                    case ItemsChoiceType3.Minute:
                        int.TryParse(serializedTime.Items[i].ToString(), out minute);
                        break;
                    case ItemsChoiceType3.Second:
                        int.TryParse(serializedTime.Items[i].ToString(), out second);
                        break;
                    case ItemsChoiceType3.Millisecond:
                        int.TryParse(serializedTime.Items[i].ToString(), out millis);
                        break;
                    case ItemsChoiceType3.TimeZone:
                        // Check for explicit local timezone, otherwise just leave the default UTC
                        if (((TimeZoneType)serializedTime.Items[i]) == TimeZoneType.Local)
                        {
                            timeZone = DateTimeKind.Local;
                        }
                        break;
                }
            }

            // First see if we were supplied a time string
            if (timeStrType != null)
            {
                // Parse the string
                DateTime.TryParse(timeStrType.Value, out time);
                if (time.Kind == DateTimeKind.Unspecified)
                {
                    time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
                }
            }
            else
            {
                // Default to the time components
                time = new DateTime(year, month, day, hour, minute, second, millis, timeZone);
            }
        }

        public static void SerializeTime(DateTime time, TimeType serializedTime)
        {
            // Check assertions
            if (serializedTime is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized time is null");
                return;
            }

            // Keep it simple and just use the string format, but convert to UTC
            serializedTime.ItemsElementName = new ItemsChoiceType3[1] { ItemsChoiceType3.TimeString };
            TimeStringType timeStrType = new TimeStringType();
            timeStrType.Value = time.ToUniversalTime().ToString(timeStrType.format);
            serializedTime.Items = new object[1] { timeStrType };
        }
        #endregion Time

        #region Points
        public static void DeserializePoint(PointType serializedPoint, ref Vector3 point)
        {
            // Check assertions
            if (serializedPoint is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized point is null");
                point = Vector3.zero;
                return;
            }

            // Create the point
            point = new Vector3(serializedPoint.X, serializedPoint.Y, serializedPoint.Z);
        }

        public static void SerializePoint(Vector3 point, PointType serializedPoint)
        {
            // Check assertions
            if (serializedPoint is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized point is null");
                return;
            }

            // Serialize out the point
            serializedPoint.X = point.x;
            serializedPoint.Y = point.y;
            serializedPoint.Z = point.z;
        }

        public static void DeserializePoints(PointsType serializedPoints, ref Vector3[] points)
        {
            // Check assertions
            if ((serializedPoints is null)||
                (serializedPoints.Items is null))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized points is null");
                points = null;
                return;
            }

            // Create the point array
            List<Vector3> pointList = new List<Vector3>();
            foreach (PointType serializedPoint in serializedPoints.Items)
            {
                Vector3 point = Vector3.zero;
                DeserializePoint(serializedPoint, ref point);
            }

            // Return the array of points
            points = pointList.ToArray();
        }

        public static void SerializePoints(Vector3[] points, PointsType serializedPoints)
        {
            // Check assertions
            if (points is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied point array is null");
                return;
            }

            // Make sure we have a valid serialized reference
            if (serializedPoints is null)
            {
                serializedPoints = new PointsType();
            }

            // Serialize out the points
            List<PointType> serializedPointList = new List<PointType>();
            foreach (Vector3 point in points)
            {
                PointType serializedPoint = new PointType();
                SerializePoint(point, serializedPoint);
            }

            // Return the serialized points list
            serializedPoints.Items = serializedPointList.ToArray();
        }
        #endregion Points

        #region Colors and Materials
        public static void DeserializeColorPredefined(ColorPredefinedType serializedColor, ref Color32 color)
        {
            // Predefined colors
            switch (serializedColor)
            {
                case ColorPredefinedType.Blue:
                    color = Color.blue;
                    break;
                case ColorPredefinedType.Cyan:
                    color = Color.cyan;
                    break;
                case ColorPredefinedType.DarkGray:
                    color.r = 169;
                    color.g = 169;
                    color.b = 169;
                    break;
                case ColorPredefinedType.Gray:
                    color = Color.gray;
                    break;
                case ColorPredefinedType.Green:
                    color = Color.green;
                    break;
                case ColorPredefinedType.LightGray:
                    color.r = 105;
                    color.g = 105;
                    color.b = 105;
                    break;
                case ColorPredefinedType.Magenta:
                    color = Color.magenta;
                    break;
                case ColorPredefinedType.Orange:
                    color.r = 255;
                    color.g = 165;
                    color.b =   0;
                    break;
                case ColorPredefinedType.Pink:
                    color.r = 255;
                    color.g = 192;
                    color.b = 203;
                    break;
                case ColorPredefinedType.Red:
                    color = Color.red;
                    break;
                case ColorPredefinedType.White:
                    color = Color.white;
                    break;
                case ColorPredefinedType.Yellow:
                    color = Color.yellow;
                    break;

                case ColorPredefinedType.Black:
                default:
                    color = Color.black;
                    break;
            }
        }

        public static bool SerializeColorPredefined(Color32 color, ref ColorPredefinedType serializedColor)
        {
            bool matched = true;

            // Black
            if (ColorUtil.ColorRGBAEquals(color, Color.black))
            {
                serializedColor = ColorPredefinedType.Black;
            }

            // Blue
            else if (ColorUtil.ColorRGBAEquals(color, Color.blue))
            {
                serializedColor = ColorPredefinedType.Blue;
            }

            // Cyan
            else if (ColorUtil.ColorRGBAEquals(color, Color.cyan))
            {
                serializedColor = ColorPredefinedType.Cyan;
            }

            // Dark Gray
            else if (ColorUtil.ColorRGBAEquals(color,  new Color32(169, 169, 169, 255)))
            {
                serializedColor = ColorPredefinedType.DarkGray;
            }

            // Gray
            else if (ColorUtil.ColorRGBAEquals(color, Color.gray))
            {
                serializedColor = ColorPredefinedType.Gray;
            }

            // Green
            else if (ColorUtil.ColorRGBAEquals(color, Color.green))
            {
                serializedColor = ColorPredefinedType.Green;
            }

            // Light Gray
            else if (ColorUtil.ColorRGBAEquals(color, new Color32(105, 105, 105, 255))) 
            {
                serializedColor = ColorPredefinedType.LightGray;
            }

            // Magenta
            else if (ColorUtil.ColorRGBAEquals(color, Color.magenta))
            {
                serializedColor = ColorPredefinedType.Magenta;
            }

            // Orange
            else if (ColorUtil.ColorRGBAEquals(color, new Color32(255, 165, 0, 255)))
            {
                serializedColor = ColorPredefinedType.Orange;
            }

            // Pink
            else if (ColorUtil.ColorRGBAEquals(color, new Color32(255, 192, 203, 255)))
            {
                serializedColor = ColorPredefinedType.Pink;
            }

            // Red
            else if (ColorUtil.ColorRGBAEquals(color, Color.red))
            {
                serializedColor = ColorPredefinedType.Red;
            }

            // White
            else if (ColorUtil.ColorRGBAEquals(color, Color.white))
            {
                serializedColor = ColorPredefinedType.White;
            }

            // Yellow
            else if (ColorUtil.ColorRGBAEquals(color, Color.yellow))
            {
                serializedColor = ColorPredefinedType.Yellow;
            }

            // No match
            else
            {
                matched = false;
            }

            return matched;
        }

        public static void DeserializeColorComponents(ColorComponentsType serializedColor, ref Color32 color)
        {
            // Check assertions
            if (serializedColor is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized color components is null");
                return;
            }

            // Build the color from the components
            color.r = serializedColor.R;
            color.g = serializedColor.G;
            color.b = serializedColor.B;
            color.a = serializedColor.A;
        }

        public static void SerializeColorComponents(Color32 color, ColorComponentsType serializedColor)
        {
            // Check assertions
            if (serializedColor is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized color components is null");
                return;
            }

            // Serialize out the components
            serializedColor.R = color.r;
            serializedColor.G = color.g;
            serializedColor.B = color.b;
            serializedColor.A = color.a;
        }

        public static void DeserializeGradient(ColorGradientType serializedGradient, Gradient gradient)
        {
            // Check assertions
            if ((serializedGradient is null) ||
                (serializedGradient.Items is null))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized gradient is null");
                return;
            }
            if (serializedGradient.Items.Length <= 0)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized gradient has no color elements");
                return;
            }
            if (serializedGradient.Items.Length != serializedGradient.Time.Length)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized gradient requires mating time and color entries");
                return;
            }

            // Build a list of colors
            gradient.colorKeys = new GradientColorKey[serializedGradient.Items.Length];
            gradient.alphaKeys = new GradientAlphaKey[serializedGradient.Items.Length];
            for (int i=0; i < serializedGradient.Items.Length; i++)
            {
                float time = serializedGradient.Time[i];
                object serializedColor = serializedGradient.Items[i];
                if (serializedColor != null)
                {
                    Color32 color = new Color32();
                    if (serializedColor is ColorPredefinedType)
                    {
                        // Predefined
                        DeserializeColorPredefined((ColorPredefinedType)serializedColor, ref color);
                    }
                    else if (serializedColor is ColorComponentsType)
                    {
                        // Components
                        DeserializeColorComponents(serializedColor as ColorComponentsType, ref color);
                    }

                    // Assign the keys
                    gradient.colorKeys[i].color = color;
                    gradient.colorKeys[i].time = time;
                    gradient.alphaKeys[i].alpha = color.a;
                    gradient.alphaKeys[i].time = time;
                }
            }
        }

        public static bool SerializeGradient(Gradient gradient, ColorGradientType serializedGradient)
        {
            // Check assertions
            if (serializedGradient is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized gradient is null");
                return false;
            }
            if (gradient.colorKeys.Length != gradient.alphaKeys.Length)
            {
                Debug.LogError("[" + ClassName + "] Supplied gradient contains different length keys");
                return false;
            }

            List<object> colors = new List<object>();
            List<float> times = new List<float>();

            // Load the serialized colors, alphas and times
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                GradientColorKey colorKey = gradient.colorKeys[i];
                GradientAlphaKey alphaKey = gradient.alphaKeys[i];
                float time = colorKey.time;

                // Color
                ColorPredefinedType serializedPredefinedColor = ColorPredefinedType.Black;
                if (SerializeColorPredefined(colorKey.color, ref serializedPredefinedColor))
                {
                    // Predefined color
                    colors.Add(serializedPredefinedColor);
                }
                else
                {
                    // Color components
                    ColorComponentsType serializedColorComponents = new ColorComponentsType();
                    SerializeColorComponents(colorKey.color, serializedColorComponents);
                    colors.Add(serializedColorComponents);
                }

                // Time
                times.Add(colorKey.time);
            }

            // Serialize out the gradient
            serializedGradient.Items = colors.ToArray();
            serializedGradient.Time = times.ToArray();

            return true;
        }

        public static void DeserializeShaderName(MaterialShaderType serializedShader, ref string shaderName)
        {
            // Check assertions
            if (serializedShader is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized shader is null");
                return;
            }

            // Predefined shaders
            switch (serializedShader.shader)
            {
                case MaterialShaderPredefinedType.Lit:
                    shaderName = "Universal Render Pipeline/Lit";
                    break;
                case MaterialShaderPredefinedType.Unlit:
                    shaderName = "Universal Render Pipeline/Unlit";
                    break;
                case MaterialShaderPredefinedType.Specular:
                    shaderName = "Specular";
                    break;
                case MaterialShaderPredefinedType.Skybox:
                    shaderName = "Skybox/6 Sided";
                    break;
                case MaterialShaderPredefinedType.Custom:
                    shaderName = serializedShader?.Value;
                    break;

                case MaterialShaderPredefinedType.Standard:
                default:
                    shaderName = "Standard";
                    break;
            }
        }

        public static void DeserializeMaterialFile(MaterialFileType serializedFile, Action<Material> onFinished)
        {
            if (serializedFile is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized material file is null");
                onFinished?.Invoke(null);
            }
            if (string.IsNullOrEmpty(serializedFile.Value))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized material filename is invalid");
                onFinished?.Invoke(null);
            }

            // Deserialize the material file based upon the type
            Material material = null;
            switch (serializedFile.format)
            {
                case MaterialFormatType.Texture:
                    // TODO: Need an material loader class that supports asychronous loading
                    Texture texture = Resources.Load(serializedFile.Value, typeof(Texture)) as Texture;
                    if (texture)
                    {
                        // Create a default shader
                        string shaderName = "";
                        DeserializeShaderName(new MaterialShaderType(), ref shaderName);
                        Shader shader = Shader.Find(shaderName);
                        if (shader)
                        {
                            material = new Material(shader);
                            material.mainTexture = texture;
                        }
                    }
                    break;

                case MaterialFormatType.Material:
                    // TODO: Need an material loader class that supports asychronous loading
                    material = Resources.Load(serializedFile.Value, typeof(Material)) as Material;
                    break;
            }

            onFinished?.Invoke(material);
        }

        /// <summary>
        /// Deserializes a material from the supplied serialized material.
        /// </summary>
        /// <param name="serializedMaterial">The serialized <code>MaterialType</code></param>
        /// <param name="onFinished">The <code>Action</code> called upon completion. Will
        ///     contain a <code>Material</code> on success. null on failure.</param>
        public static void DeserializeMaterial(MaterialType serializedMaterial, Action<Material> onFinished)
        {
            // Check assertions
            if ((serializedMaterial is null) ||
                (serializedMaterial.Items is null))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized material is null");
                onFinished?.Invoke(null);
            }

            // Deserialize the material depending on how it is serialized
            Material material = null;
            string shaderName = "";
            Color32 color = new Color32();

            // Generated code for schema allows for inconsistent interpretation of material type. The rules are these:
            // One of the following can occur:
            //      - AssetType, or
            //      - MaterialFileType, or
            //      - MaterialDefinitionGroup
            //          - MaterialShaderType (optional)
            //          - ColorTypeGroup
            //              - PredefintedColorType, or
            //              - ColorComponentType
            foreach (object serializedMaterialItem in serializedMaterial.Items)
            {
                if (serializedMaterialItem is AssetType)
                {
                    DeserializeAsset(serializedMaterialItem as AssetType, onFinished);
                    return; // Exit immediately
                }
                else if (serializedMaterialItem is MaterialFileType)
                {
                    DeserializeMaterialFile(serializedMaterialItem as MaterialFileType, onFinished);
                    return; // Exit immediately
                }
                else if (serializedMaterialItem is MaterialShaderType)
                {
                    DeserializeShaderName(serializedMaterialItem as MaterialShaderType, ref shaderName);
                }
                else if (serializedMaterialItem is ColorPredefinedType)
                {
                    DeserializeColorPredefined(((ColorPredefinedType)serializedMaterialItem), ref color);
                }
                else if (serializedMaterialItem is ColorComponentsType)
                {
                    DeserializeColorComponents(serializedMaterialItem as ColorComponentsType, ref color);
                }
                else
                {
                    Debug.LogWarning("[" + ClassName + "] Unsupported material type item: " + serializedMaterialItem);
                    onFinished?.Invoke(null);
                }
            }

            // If we get here, a material definition must have been specified
            if (string.IsNullOrEmpty(shaderName))
            {
                // Shader not specified so get the default shader
                DeserializeShaderName(new MaterialShaderType(), ref shaderName);
            }

            // See if we can obtain the shader
            Shader shader = Shader.Find(shaderName);
            if (shader)
            {
                // Build the material and assign the color
                material = new Material(shader) { color = color };
                if (shaderName.Equals("Skybox/6 Sided"))
                {
                    material.SetColor("_Tint", color);
                }
            }

            // Invoke the finish action
            onFinished?.Invoke(material);
        }
        #endregion // Colors and Materials

        #region Transforms
        /// <summary>
        /// Deserializes a serialized transform position into a Unity Vector3.<br>
        /// 
        /// NOTE: The reference space is lost by using this function. To retain the reference space,
        /// <code>DeserializeTransformPosition(TransformPositionType, Transform)</code> should be used.<br>
        /// </summary>
        /// <param name="serializedPosition">The <code>TransformPositionType</code> representing the serialized transform position</param>
        /// <param name="position">The Unity <code>Vector3</code> to assign</param>
        /// 
        /// <seealso cref="Vector3"/>
        /// <seealso cref="TransformPositionType"/>
        /// <see cref="DeserializeTransformPosition(TransformPositionType, Transform)"/>
        /// 
        public static void DeserializeTransformPosition(TransformPositionType serializedPosition,
            ref Vector3 position)
        {
            // Create a default serialized position if not defined
            if (serializedPosition is null)
            {
                serializedPosition = new TransformPositionType();
            }

            // Make sure we have a valid position reference
            if (position == null)
            {
                position = new Vector3();
            }

            // Position
            position.x = serializedPosition.X;
            position.y = serializedPosition.Y;
            position.z = serializedPosition.Z;
        }

        /// <summary>
        /// Deserializes a serialized transform position into a Unity game object transform<br>
        /// </summary>
        /// <param name="serializedPosition">The <code>TransformPositionType</code> representing the serialized transform position</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> transform to update</param>
        /// 
        /// <seealso cref="GameObject"/>
        /// <seealso cref="TransformPositionType"/>
        /// 
        public static void DeserializeTransformPosition(TransformPositionType serializedPosition, GameObject gameObject)
        {
            // Check assertions
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Create a default serialized position if not defined
            if (serializedPosition is null)
            {
                serializedPosition = new TransformPositionType();
            }

            // Position
            Vector3 newPosition = new Vector3();
            DeserializeTransformPosition(serializedPosition, ref newPosition);
            switch (serializedPosition.referenceSpace)
            {
                case ReferenceSpaceType.Relative:
                    gameObject.transform.localPosition = newPosition;
                    break;

                case ReferenceSpaceType.Global:
                default:
                    gameObject.transform.position = newPosition;
                    break;
            }
        }

        /// <summary>
        /// Deserializes a serialized transform Euler rotation into a Unity Vector3.<br>
        /// 
        /// NOTE: The reference space is lost by using this function. To retain the reference space,
        /// <code>DeserializeTransformRotation(TransformRotationType, Transform)</code> should be used.<br>
        /// </summary>
        /// <param name="serializedRotation">The <code>TransformEulerRotationType</code> representing the serialized transform Euler rotation</param>
        /// <param name="rotation">The Unity <code>Vector3</code> to assign</param>
        /// 
        /// <seealso cref="Vector3"/>
        /// <seealso cref="TransformEulerRotationType"/>
        /// <see cref="DeserializeTransformRotation(TransformRotationType, Transform)"/>
        /// 
        public static void DeserializeTransformRotation(TransformEulerRotationType serializedRotation,
            ref Vector3 rotation)
        {
            // Create a default serialized Euler rotation if not defined
            if (serializedRotation is null)
            {
                serializedRotation = new TransformEulerRotationType();
            }

            // Make sure we have a valid rotation reference
            if (rotation == null)
            {
                rotation = new Vector3();
            }

            // Create the Euler quaternion
            Quaternion eulerRotation = Quaternion.Euler(
                serializedRotation.X,
                serializedRotation.Y,
                serializedRotation.Z);

            // Rotation
            rotation.x = eulerRotation.eulerAngles.x;
            rotation.y = eulerRotation.eulerAngles.y;
            rotation.z = eulerRotation.eulerAngles.z;
        }

        /// <summary>
        /// Deserializes a serialized transform quaternion rotation into a Unity Quaternion.<br>
        /// 
        /// NOTE: The reference space is lost by using this function. To retain the reference space,
        /// <code>DeserializeTransformRotation(TransformRotationType, Transform)</code> should be used.<br>
        /// </summary>
        /// <param name="serializedRotation">The <code>TransformQRotationType</code> representing the serialized transform quaternion rotation</param>
        /// <param name="rotation">The Unity <code>Quaternion</code> to assign</param>
        /// 
        /// <seealso cref="Quaternion"/>
        /// <seealso cref="TransformQRotationType"/>
        /// <see cref="DeserializeTransformRotation(TransformRotationType, Transform)"/>
        /// 
        public static void DeserializeTransformRotation(TransformQRotationType serializedRotation,
            ref Quaternion rotation)
        {
            // Create a default serialized quaternion rotation if not defined
            if (serializedRotation is null)
            {
                serializedRotation = new TransformQRotationType();
            }

            // Make sure we have a valid position reference
            if (rotation == null)
            {
                rotation = new Quaternion();
            }

            // Rotation
            rotation.x = serializedRotation.X;
            rotation.y = serializedRotation.Y;
            rotation.z = serializedRotation.Z;
            rotation.w = serializedRotation.W;
        }

        /// <summary>
        /// Deserializes a serialized transform rotation into a Unity game object transform<br>
        /// </summary>
        /// <param name="serializedRotation">The <code>TransformRotationType</code> representing the serialized transform rotation</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> transform to update</param>
        /// 
        /// <seealso cref="TransformRotationType"/>
        /// <seealso cref="GameObject"/>
        /// 
        public static void DeserializeTransformRotation(TransformRotationType serializedRotation, GameObject gameObject)
        {
            // Check assertions
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Create a default serialized quaternion rotation if not defined
            if (serializedRotation is null)
            {
                serializedRotation = new TransformQRotationType();
            }

            // Rotation
            if (serializedRotation is TransformEulerRotationType)
            {
                TransformEulerRotationType serializedEulerRotation = serializedRotation as TransformEulerRotationType;

                // Euler rotation
                Vector3 eulerRotation = new Vector3();
                Quaternion qRotation = new Quaternion();
                DeserializeTransformRotation(serializedEulerRotation, ref eulerRotation);
                switch (serializedRotation.referenceSpace)
                {
                    case ReferenceSpaceType.Relative:
                        qRotation.eulerAngles = eulerRotation;
                        gameObject.transform.localRotation = qRotation;
                        break;

                    case ReferenceSpaceType.Global:
                    default:
                        qRotation.eulerAngles = eulerRotation;
                        gameObject.transform.rotation = qRotation;
                        break;
                }
            }
            else if (serializedRotation is TransformQRotationType)
            {
                TransformQRotationType serializedQRotation = serializedRotation as TransformQRotationType;

                // Quaternion rotation
                Quaternion qRotation = new Quaternion();
                DeserializeTransformRotation(serializedQRotation, ref qRotation);
                switch (serializedRotation.referenceSpace)
                {
                    case ReferenceSpaceType.Relative:
                        gameObject.transform.localRotation = qRotation;
                        break;

                    case ReferenceSpaceType.Global:
                    default:
                        gameObject.transform.rotation = qRotation;
                        break;
                }
            }
        }

        /// <summary>
        /// Deserializes a serialized transform scale into a Unity Vector3.<br>
        /// 
        /// NOTE: The reference space is lost by using this function. To retain the reference space,
        /// <code>DeserializeTransformScale(TransformScaleType, Transform)</code> should be used.<br>
        /// </summary>
        /// <param name="serializedScale">The <code>TransformScaleType</code> representing the serialized transform scale</param>
        /// <param name="scale">The Unity <code>Vector3</code> to assign</param>
        /// 
        /// <seealso cref="Vector3"/>
        /// <seealso cref="TransformScaleType"/>
        /// <see cref="DeserializeTransformScale(TransformScaleType, Transform)"/>
        /// 
        public static void DeserializeTransformScale(TransformScaleType serializedScale,
            ref Vector3 scale)
        {
            // Create a default serialized quaternion rotation if not defined
            if (serializedScale is null)
            {
                serializedScale = new TransformScaleType();
            }

            // Make sure we have a valid scale reference
            if (scale == null)
            {
                scale = Vector3.one;
            }

            // Check for a size in units
            float x = serializedScale.X;
            float y = serializedScale.Y;
            float z = serializedScale.Z;
            if (serializedScale is TransformSizeType)
            {
                TransformSizeType serializedSize = serializedScale as TransformSizeType;

                // Adjust for the units
                x = LengthToUnityUnits(x, serializedSize.units);
                y = LengthToUnityUnits(y, serializedSize.units);
                z = LengthToUnityUnits(z, serializedSize.units);
            }

            // Scale
            scale.x = x;
            scale.y = y;
            scale.z = z;
        }

        /// <summary>
        /// Deserializes a serialized transform scale into a Unity game object transform<br>
        /// </summary>
        /// <param name="serializedScale">The <code>TransformScaleType</code> representing the serialized transform scale</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> transform to update</param>
        /// 
        /// <seealso cref="TransformScaleType"/>
        /// <seealso cref="GameObject"/>
        /// 
        public static void DeserializeTransformScale(TransformScaleType serializedScale, GameObject gameObject)
        {
            // Check assertions
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Create a default serialized quaternion rotation if not defined
            if (serializedScale is null)
            {
                serializedScale = new TransformScaleType();
            }

            // Deserialize the scale
            Vector3 value = Vector3.one;
            DeserializeTransformScale(serializedScale, ref value);

            // Determine how to apply the scale
            if (serializedScale is TransformSizeType)
            {
                // Convert the size to a scale
                Vector3 size = TransformUtil.GetSizeAsScale(gameObject, value);
                gameObject.transform.localScale = ((size.x <= 0f) || (size.y <= 0f) || (size.z <= 0f)) ?
                    gameObject.transform.localScale :
                    size;
            }
            else
            {
                // Scale
                gameObject.transform.localScale = value;
            }
        }

        /// <summary>
        /// Deserializes a serialized transform into a Unity game object transform<br>
        /// </summary>
        /// <param name="serializedTransform">The <code>TransformType</code> representing the serialized transform</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> transform to update</param>
        /// 
        /// <seealso cref="TransformType"/>
        /// <seealso cref="GameObject"/>
        /// 
        public static void DeserializeTransform(TransformType serializedTransform, GameObject gameObject)
        {
            // Check assertions
            if (serializedTransform is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform is null");
                return;
            }
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Position
            DeserializeTransformPosition(serializedTransform.Position, gameObject);

            // Rotation
            DeserializeTransformRotation(serializedTransform.Item, gameObject);

            // Scale
            DeserializeTransformScale(serializedTransform.Item1, gameObject);
        }

        /// <summary>
        /// Serializes a Unity Vector3 into a serialized transform position<br>
        /// </summary>
        /// <param name="serializedPosition">The serialized <code>TransformPositionType</code> to assign</param>
        /// <param name="position">The Unity <code>Vector3</code> position to serialize</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> identifying the reference space for the position</param>
        /// 
        /// <seealso cref="Vector3"/>
        /// <seealso cref="TransformPositionType"/>
        /// 
        public static void SerializeTransformPosition(TransformPositionType serializedPosition,
            Vector3 position, ReferenceSpaceType reference = ReferenceSpaceType.Global)
        {
            // Check assertions
            if (serializedPosition is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform position is null");
                return;
            }

            // Serialize the reference space
            serializedPosition.referenceSpace = reference;

            // Serialize the transform position
            serializedPosition.X = position.x;
            serializedPosition.Y = position.y;
            serializedPosition.Z = position.z;
        }

        /// <summary>
        /// Serializes a Unity game object transform into a serialized transform position<br>
        /// </summary>
        /// <param name="serializedPosition">The serialized <code>TransformPositionType</code> to assign</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> containing the transform to serialize</param>
        /// 
        /// <seealso cref="GameObject"/>
        /// <seealso cref="TransformPositionType"/>
        /// 
        public static void SerializeTransformPosition(TransformPositionType serializedPosition, GameObject gameObject)
        {
            // Check assertions
            if (serializedPosition is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform position is null");
                return;
            }
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Serialize the transform position
            switch (serializedPosition.referenceSpace)
            {
                case ReferenceSpaceType.Relative:
                    SerializeTransformPosition(serializedPosition,
                        gameObject.transform.localPosition, ReferenceSpaceType.Relative);
                    break;
                case ReferenceSpaceType.Global:
                default:
                    SerializeTransformPosition(serializedPosition,
                        gameObject.transform.position, ReferenceSpaceType.Global);
                    break;
            }
        }

        /// <summary>
        /// Serializes a Unity Vector3 into a serialized transform Euler rotation<br>
        /// </summary>
        /// <param name="serializedRotation">The serialized <code>TransformEulerRotationType</code> to assign</param>
        /// <param name="rotation">The Unity <code>Vector3</code> rotation to serialize</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> identifying the reference space for the rotation</param>
        /// 
        /// <seealso cref="Vector3"/>
        /// <seealso cref="TransformEulerRotationType"/>
        /// 
        public static void SerializeTransformRotation(TransformEulerRotationType serializedRotation,
            Vector3 rotation, ReferenceSpaceType reference = ReferenceSpaceType.Global)
        {
            // Check assertions
            if (serializedRotation is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform Euler rotation is null");
                return;
            }

            // Serialize the reference space
            serializedRotation.referenceSpace = reference;

            // Serialize the transform rotation
            serializedRotation.X = rotation.x;
            serializedRotation.Y = rotation.y;
            serializedRotation.Z = rotation.z;
        }

        /// <summary>
        /// Serializes a Unity Quaternion into a serialized transform quternion rotation<br>
        /// </summary>
        /// <param name="serializedRotation">The serialized <code>TransformQRotationType</code> to assign</param>
        /// <param name="rotation">The Unity <code>Quaternion</code> rotation to serialize</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> identifying the reference space for the rotation</param>
        /// 
        /// <seealso cref="Quaternion"/>
        /// <seealso cref="TransformQRotationType"/>
        /// 
        public static void SerializeTransformRotation(TransformQRotationType serializedRotation,
            Quaternion rotation, ReferenceSpaceType reference = ReferenceSpaceType.Global)
        {
            // Check assertions
            if (serializedRotation is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform Euler rotation is null");
                return;
            }

            // Serialize the reference space
            serializedRotation.referenceSpace = reference;

            // Serialize the transform rotation
            serializedRotation.X = rotation.x;
            serializedRotation.Y = rotation.y;
            serializedRotation.Z = rotation.z;
            serializedRotation.W = rotation.w;
        }

        /// <summary>
        /// Serializes a Unity game object transform into a serialized transform rotation<br>
        /// </summary>
        /// <param name="serializedRotation">The serialized <code>TransformRotationType</code> to assign</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> containing the transform to serialize</param>
        /// 
        /// <seealso cref="GameObject"/>
        /// <seealso cref="TransformRotationType"/>
        /// 
        public static void SerializeTransformRotation(TransformRotationType serializedRotation, GameObject gameObject)
        {
            // Check assertions
            if (serializedRotation is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform rotation is null");
                return;
            }
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Serialize the transform rotation
            if (serializedRotation is TransformEulerRotationType)
            {
                // Euler rotation
                TransformEulerRotationType serializedEulerRotation = serializedRotation as TransformEulerRotationType;

                switch (serializedRotation.referenceSpace)
                {
                    case ReferenceSpaceType.Relative:
                        SerializeTransformRotation(serializedEulerRotation,
                            gameObject.transform.localRotation.eulerAngles, ReferenceSpaceType.Relative);
                        break;
                    case ReferenceSpaceType.Global:
                    default:
                        SerializeTransformRotation(serializedEulerRotation,
                            gameObject.transform.rotation.eulerAngles, ReferenceSpaceType.Global);
                        break;
                }
            }
            else
            {
                // Quaternion rotation
                TransformQRotationType serializedQRotation = serializedRotation as TransformQRotationType;

                switch (serializedRotation.referenceSpace)
                {
                    case ReferenceSpaceType.Relative:
                        SerializeTransformRotation(serializedQRotation,
                            gameObject.transform.localRotation, ReferenceSpaceType.Relative);
                        break;
                    case ReferenceSpaceType.Global:
                    default:
                        SerializeTransformRotation(serializedQRotation,
                            gameObject.transform.rotation, ReferenceSpaceType.Global);
                        break;
                }
            }
        }

        /// <summary>
        /// Serializes a Unity Vector3 into a serialized transform scale<br>
        /// </summary>
        /// <param name="serializedScale">The serialized <code>TransformScaleType</code> to assign</param>
        /// <param name="scale">The Unity <code>Vector3</code> scale to serialize</param>
        /// <param name="reference">The <code>ReferenceSpaceType</code> identifying the reference space for the scale</param>
        /// 
        /// <seealso cref="Vector3"/>
        /// <seealso cref="TransformScaleType"/>
        /// 
        public static void SerializeTransformScale(TransformScaleType serializedScale,
            Vector3 scale, ReferenceSpaceType reference = ReferenceSpaceType.Relative)
        {
            // Check assertions
            if (serializedScale is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform scale is null");
                return;
            }

            // Serialize the reference space
            serializedScale.referenceSpace = reference;

            // Start with the local scale
            float x = scale.x;
            float y = scale.y;
            float z = scale.z;

            // Adjust for units
            if (serializedScale is TransformSizeType)
            {
                // Size
                TransformSizeType serializedSize = serializedScale as TransformSizeType;

                // Adjust the multiplier based upon the units
                LengthUnitType units = serializedSize.units;
                x = UnityUnitsToLength(x, units);
                y = UnityUnitsToLength(y, units);
                z = UnityUnitsToLength(z, units);
            }

            // Serialize the transform scale
            serializedScale.X = x;
            serializedScale.Y = y;
            serializedScale.Z = z;
        }

        /// <summary>
        /// Serializes a Unity game object transform into a serialized transform scale<br>
        /// </summary>
        /// <param name="serializedScale">The serialized <code>TransformScaleType</code> to assign</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> containing the transform to serialize</param>
        /// 
        /// <seealso cref="GameObject"/>
        /// <seealso cref="TransformScaleType"/>
        /// 
        public static void SerializeTransformScale(TransformScaleType serializedScale, GameObject gameObject)
        {
            // Check assertions
            if (serializedScale is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform scale is null");
                return;
            }
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Determine how to interpret the scale
            Vector3 scale = Vector3.one;
            ReferenceSpaceType referenceSpace;
            if (serializedScale is TransformSizeType)
            {
                // Size
                scale = TransformUtil.GetSize(gameObject);
                referenceSpace = ReferenceSpaceType.Global;
            }
            else
            {
                // Scale
                scale = gameObject.transform.localScale;
                referenceSpace = ReferenceSpaceType.Relative; // Only relative for scale
            }

            // Serialize out the transform scale
            SerializeTransformScale(serializedScale,
                scale, referenceSpace);
        }

        /// <summary>
        /// Serializes a Unity game object transform into a serialized transform<br>
        /// </summary>
        /// <param name="serializedTransform">The serialized <code>TransformType</code> to assign</param>
        /// <param name="gameObject">The Unity <code>GameObject</code> containing the transform to serialize</param>
        /// <param name="useSizeAsDefault">Indicates whether or not size is used for scale if the serialized
        ///     scale structure is not defined.</param>
        /// 
        /// <seealso cref="GameObject"/>
        /// <seealso cref="TransformType"/>
        /// 
        public static void SerializeTransform(TransformType serializedTransform, GameObject gameObject, bool useSizeAsDefault = false)
        {
            // Check assertions
            if (serializedTransform is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized transform is null");
                return;
            }
            if (gameObject is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied game object is null");
                return;
            }

            // Serialize the entire transform

            // *** Position ***
            {
                // Make sure we have a valid serialized transform position reference
                TransformPositionType serializedPosition = serializedTransform.Position;
                if (serializedPosition is null)
                {
                    // Default to basic transform structure
                    serializedPosition = new TransformPositionType();
                }

                // Serialize the position
                SerializeTransformPosition(serializedPosition, gameObject);

                // Only assign the position if anything deviates from the defaults
                TransformPositionType defaults = new TransformPositionType();
                if ((serializedPosition.referenceSpace == defaults.referenceSpace) &&
                    Mathf.Approximately(serializedPosition.X, defaults.X) &&
                    Mathf.Approximately(serializedPosition.Y, defaults.Y) &&
                    Mathf.Approximately(serializedPosition.Z, defaults.Z))
                {
                    // Set to null for defaults
                    serializedPosition = null;
                }
                serializedTransform.Position = serializedPosition;
            }

            // *** Rotation ***
            {
                // Make sure we have a valid serialized transform rotation reference
                TransformRotationType serializedRotation = serializedTransform.Item;
                if (serializedRotation is null)
                {
                    // Default to basic transform structure
                    serializedRotation = new TransformQRotationType();
                }

                // Serialize the rotation
                SerializeTransformRotation(serializedRotation, gameObject);

                // Only assign the rotation if anything deviates from the defaults
                if (serializedRotation is TransformEulerRotationType)
                {
                    TransformEulerRotationType defaults = new TransformEulerRotationType();
                    TransformEulerRotationType serialized = serializedRotation as TransformEulerRotationType;
                    if ((serialized.referenceSpace == defaults.referenceSpace) &&
                        Mathf.Approximately(serialized.X, defaults.X) &&
                        Mathf.Approximately(serialized.Y, defaults.Y) &&
                        Mathf.Approximately(serialized.Z, defaults.Z))
                    {
                        // Set to null for defaults
                        serializedRotation = null;
                    }
                }
                else
                {
                    TransformQRotationType defaults = new TransformQRotationType();
                    TransformQRotationType serialized = serializedRotation as TransformQRotationType;
                    if ((serialized.referenceSpace == defaults.referenceSpace) &&
                        Mathf.Approximately(serialized.X, defaults.X) &&
                        Mathf.Approximately(serialized.Y, defaults.Y) &&
                        Mathf.Approximately(serialized.Z, defaults.Z) &&
                        Mathf.Approximately(serialized.W, defaults.W))
                    {
                        // Set to null for defaults
                        serializedRotation = null;
                    }
                }
                serializedTransform.Item = serializedRotation;
            }

            // *** Scale ***
            {
                // Make sure we have a valid serialized transform scale reference
                TransformScaleType serializedScale = serializedTransform.Item1;
                if (serializedScale is null)
                {
                    // Default to the correct transform structure
                    serializedScale = useSizeAsDefault ? new TransformSizeType() : new TransformScaleType();
                }

                // Serialize the scale
                SerializeTransformScale(serializedScale, gameObject);

                // Only assign the scale if anything deviates from the defaults
                if (serializedScale is TransformSizeType)
                {
                    TransformSizeType defaults = new TransformSizeType();
                    TransformSizeType serializedSize = serializedScale as TransformSizeType;
                    if ((serializedSize.referenceSpace == defaults.referenceSpace) &&
                        (serializedSize.units == defaults.units) &&
                        Mathf.Approximately(serializedSize.X, defaults.X) &&
                        Mathf.Approximately(serializedSize.Y, defaults.Y) &&
                        Mathf.Approximately(serializedSize.Z, defaults.Z))
                    {
                        // Set to null for defaults
                        serializedScale = null;
                    }
                }
                else
                {
                    TransformScaleType defaults = new TransformScaleType();
                    if ((serializedScale.referenceSpace == defaults.referenceSpace) &&
                        Mathf.Approximately(serializedScale.X, defaults.X) &&
                        Mathf.Approximately(serializedScale.Y, defaults.Y) &&
                        Mathf.Approximately(serializedScale.Z, defaults.Z))
                    {
                        // Set to null for defaults
                        serializedScale = null;
                    }
                }
                serializedTransform.Item1 = serializedScale;
            }
        }
        #endregion Transforms

        #region Assets
        public static void DeserializeAsset<T>(AssetType serializedAsset, Action<T> onFinished)
            where T : UnityEngine.Object
        {
            // Check assertions
            if (serializedAsset is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized asset is null");
                onFinished?.Invoke(null);
            }
            if (string.IsNullOrEmpty(serializedAsset.AssetName) ||
                string.IsNullOrEmpty(serializedAsset.AssetBundle))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized asset fields are invalid");
                onFinished?.Invoke(null);
            }

            Action<object> AssetLoadedAction = (object loadedAsset) =>
            {
                if (loadedAsset == null)
                {
                    Debug.LogError("[" + ClassName + "] There was a problem loading the asset");
                }
                onFinished?.Invoke(loadedAsset as T);
            };

            // Load the asset asynchronously
            AssetBundleHelper.Instance.LoadAssetAsync<T>(
                serializedAsset.AssetBundle,
                serializedAsset.AssetName,
                AssetLoadedAction);
        }

        public static void DeserializeScene(AssetType serializedScene, bool isAdditive, Action<bool> onFinished)
        {
            // Check assertions
            if (serializedScene is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized scene is null");
                onFinished?.Invoke(false);
            }
            if (string.IsNullOrEmpty(serializedScene.AssetName) ||
                string.IsNullOrEmpty(serializedScene.AssetBundle))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized scene fields are invalid");
                onFinished?.Invoke(false);
            }

            // Load the scene asynchronously
            AssetBundleHelper.Instance.LoadSceneAsync(
                serializedScene.AssetBundle,
                serializedScene.AssetName, isAdditive,
                onFinished);
        }
        #endregion Assets

        #region Models
        public static void DeserializePrimitiveShape(PrimitiveShapeType serializedShape, Action<GameObject> onFinished)
        {
            // Load the primitive shape
            GameObject primitive = null;
            switch (serializedShape)
            {
                case PrimitiveShapeType.Sphere:
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;

                case PrimitiveShapeType.Plane:
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    break;

                case PrimitiveShapeType.Cube:
                default:
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
            }

            onFinished?.Invoke(primitive);
        }

        public static void DeserializeModelFile(ModelFileType serializedFile, Action<GameObject> onFinished)
        {
            if (serializedFile is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized model file is null");
                onFinished?.Invoke(null);
            }
            if (string.IsNullOrEmpty(serializedFile.Value))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized model filename is invalid");
                onFinished?.Invoke(null);
            }

            Action<GameObject, AnimationClip[]> action = (GameObject model, AnimationClip[] anims) =>
            {
                // Invoke the finish action
                onFinished?.Invoke(model);
            };

            try
            {
                // Load the file asynchronously depending on the file type
                switch (serializedFile.format)
                {
                    case ModelFormatType.GLTF:
                    default:
                        // Load the GLTF
                        ModelLoading.ImportGLTFAsync(serializedFile.Value, action);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[" + ClassName + "] There was a problem importing the mode: " + e.Message);
                onFinished?.Invoke(null);
            }
        }

        public static void DeserializeModel(ModelType serializedModel, Action<GameObject> onFinished)
        {
            // Check assertions
            if (serializedModel is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized model is null. Aborting model deserialization.");
                onFinished?.Invoke(null);
            }

            GameObject model = null;

            void DeserializeModelAction(GameObject deserializedModel)
            {
                // Assign the model
                model = deserializedModel;

                // Assign the material if provided
                if (model != null)
                {
                    // Load the optional material
                    MaterialType serializedMaterial = serializedModel.Material;
                    if (serializedMaterial != null)
                    {
                        Action<Material> MaterialLoadedAction = (Material loadedMaterial) =>
                        {
                            if (loadedMaterial != null)
                            {
                                RendererUtil.ApplyMaterial(model, loadedMaterial);
                            }
                            else
                            {
                                Debug.LogWarning("[" + ClassName + "] Model material deserialization failed");
                            }
                        };

                        // Load the material
                        DeserializeMaterial(serializedMaterial, MaterialLoadedAction);
                    }
                }

                // Notify the caller
                onFinished?.Invoke(model);
            };

            // Determine how we are deserializing the model
            if (serializedModel.Item != null)
            {
                if (serializedModel.Item is AssetType)
                {
                    // Deserialize the asset
                    DeserializeAsset<GameObject>(serializedModel.Item as AssetType, DeserializeModelAction);
                }
                else if (serializedModel.Item is ModelFileType)
                {
                    // Deserialize the model file
                    DeserializeModelFile(serializedModel.Item as ModelFileType, DeserializeModelAction);
                }
                else if (serializedModel.Item is PrimitiveShapeType)
                {
                    // Deserialize the model shape
                    DeserializePrimitiveShape((PrimitiveShapeType)serializedModel.Item, DeserializeModelAction);
                }
                else
                {
                    Debug.LogWarning("[" + ClassName + "] Unsupported model type item: " + serializedModel.Item.GetType());
                    onFinished?.Invoke(null);
                }
            }
        }
        #endregion Models

        #region Annotations
        public static void DeserializeAnnotationSourceFile(AnnotationSourceFileType serializedFile, Action<object> onFinished)
        {
            if (serializedFile is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized annotation source file is null");
                onFinished?.Invoke(null);
            }
            if (string.IsNullOrEmpty(serializedFile.Value))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized annotation source filename is invalid");
                onFinished?.Invoke(null);
            }

            // Load the file asynchronously depending on the file type
            switch (serializedFile.format)
            {
                case AnnotationSourceFormatType.M4A:
                case AnnotationSourceFormatType.FLAC:
                case AnnotationSourceFormatType.MP3:
                case AnnotationSourceFormatType.WAV:
                case AnnotationSourceFormatType.WMA:
                case AnnotationSourceFormatType.AAC:
                case AnnotationSourceFormatType.MOV:
                case AnnotationSourceFormatType.WMV:
                case AnnotationSourceFormatType.AVI:
                case AnnotationSourceFormatType.AVCHD:
                case AnnotationSourceFormatType.FLV:
                case AnnotationSourceFormatType.F4V:
                case AnnotationSourceFormatType.SWF:
                case AnnotationSourceFormatType.MP4:
                default:
                    // FIXME: implement
                    onFinished?.Invoke(serializedFile.Value);
                    break;
            }
        }

        public static void LoadAnnotationSource(AnnotationSourceType serializedSource, Action<object> onFinished)
        {
            // Check assertions
            if ((serializedSource is null) ||
                (serializedSource.Item is null))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized annotation source is null");
                onFinished?.Invoke(null);
            }

            // Load the source depending on how it is serialized
            if (serializedSource.Item is AssetType)
            {
                Action<GameObject> AssetLoadedAction = (GameObject loadedAsset) =>
                {
                    onFinished?.Invoke(loadedAsset);
                };

                // Deserialize the asset
                DeserializeAsset(serializedSource.Item as AssetType, AssetLoadedAction);
            }
            else if (serializedSource.Item is AnnotationSourceFileType)
            {
                // Deserialize the file
                AnnotationSourceFileType serializedFile = serializedSource.Item as AnnotationSourceFileType;
                DeserializeAnnotationSourceFile(serializedFile, onFinished);
            }
            else if (serializedSource.Item is string)
            {
                Uri uri = new Uri(serializedSource.Item as string);
                onFinished?.Invoke(uri);
            }
            else
            {
                Debug.LogWarning("[" + ClassName + "] Unsupported annotation source type item: " + serializedSource.Item.GetType());
                onFinished?.Invoke(null);
            }
        }
        #endregion Annotations

        #region PointCloud
        public static void DeserializePointCloudFile(PointCloudFileType serializedFile, Action<object> onFinished)
        {
            if (serializedFile is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized point cloud file is null");
                onFinished?.Invoke(null);
            }
            if (string.IsNullOrEmpty(serializedFile.Value))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized point cloud filename is invalid");
                onFinished?.Invoke(null);
            }

            // Load the file asynchronously depending on the file type
            switch (serializedFile.format)
            {
                case PointCloudFormatType.Binary:
                case PointCloudFormatType.ASCII:
                case PointCloudFormatType.pcache:
                case PointCloudFormatType.potree:
                case PointCloudFormatType.NetCDF:
                case PointCloudFormatType.LAS:
                default:
                    // FIXME: implement
                    onFinished?.Invoke(serializedFile.Value);
                    break;
            }
        }

        public static void LoadPointCloudSource(PointCloudSourceType serializedSource, Action<object> onFinished)
        {
            // Check assertions
            if ((serializedSource is null) ||
                (serializedSource.Item is null))
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized point cloud source is null");
                onFinished?.Invoke(null);
            }

            // Load the source depending on how it is serialized
            if (serializedSource.Item is AssetType)
            {
                Action<GameObject> AssetLoadedAction = (GameObject loadedAsset) =>
                {
                    onFinished?.Invoke(loadedAsset);
                };

                // Deserialize the asset
                DeserializeAsset(serializedSource.Item as AssetType, AssetLoadedAction);
            }
            else if (serializedSource.Item is PointCloudFileType)
            {
                // Deserialize the file
                PointCloudFileType serializedFile = serializedSource.Item as PointCloudFileType;
                DeserializePointCloudFile(serializedFile, onFinished);
            }
            else if (serializedSource.Item is string)
            {
                Uri uri = new Uri(serializedSource.Item as string);
                onFinished?.Invoke(uri);
            }
            else
            {
                Debug.LogWarning("[" + ClassName + "] Unsupported point cloud source type item: " + serializedSource.Item.GetType());
                onFinished?.Invoke(null);
            }
        }
        #endregion PointCloud

        #region Interactions
        /// <summary>
        /// Deserializes the supplied serialized interation settings and assigns the values
        /// to the supplied reference parameters.
        /// </summary>
        /// <param name="serializedInteractions">The serialized <code>InteractionSettingsType</code> interaction settings</param>
        /// <param name="usable">The usable setting</param>
        /// <param name="grabbable">The grabbable setting</param>
        /// 
        /// <see cref="InteractionSettingsType"/>
        /// 
        public static void DeserializeInteractions(
            InteractionSettingsType serializedInteractions,
            ref bool usable,
            ref bool grabbable)
        {
            // Check assertions
            if (serializedInteractions is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized interactions are null");
                return;
            }

            // Deserialize interactions
            usable = serializedInteractions.EnableUsability;
            grabbable = serializedInteractions.EnableInteraction;
        }

        /// <summary>
        /// Serializes the supplied interactions into a serialized interaction settings<br>
        /// </summary>
        /// <param name="serializedInteractions">The serialized <code>InteractionSettingsType</code> to assign</param>
        /// <param name="usable">The usable setting</param>
        /// <param name="grabbable">The grabbable setting</param>
        /// 
        /// <seealso cref="InteractionSettingsType"/>
        /// 
        public static void SerializeInteractions(InteractionSettingsType serializedInteractions,
            bool usable, bool grabbable)
        {
            // Check assertions
            if (serializedInteractions is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized interactions is null");
                return;
            }

            // Serialize the interactions
            serializedInteractions.EnableUsability = usable;
            serializedInteractions.EnableInteraction = grabbable;
        }
        #endregion Interactions

        #region Physics
        /// <summary>
        /// Deserializes the supplied serialized physics settings and assigns the values
        /// to the supplied reference parameters.
        /// </summary>
        /// <param name="serializedPhysics">The serialized <code>PhysicsSettingsType</code> physics settings</param>
        /// <param name="enableCollisions">The enable collisions setting</param>
        /// <param name="enableGravity">The enable gravity setting</param>
        /// <param name="mass">The mass setting</param>
        /// 
        /// <see cref="PhysicsSettingsType"/>
        /// 
        public static void DeserializePhysics(
            PhysicsSettingsType serializedPhysics,
            ref bool enableCollisions,
            ref bool enableGravity)
        {
            // Check assertions
            if (serializedPhysics is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized physics settings are null");
                return;
            }

            // Deserialize collisions
            enableCollisions = serializedPhysics.EnableCollisions;

            // Deserialize gravity
            enableGravity = serializedPhysics.EnableGravity;
        }

        /// <summary>
        /// Serializes the supplied physics settings into a serialized physics settings<br>
        /// </summary>
        /// <param name="serializedPhysics">The serialized <code>PhysicsSettingsType</code> to assign</param>
        /// <param name="enableCollisions">The enable collisions setting</param>
        /// <param name="enableGravity">The enable gravity setting</param>
        /// <param name="mass">The mass setting</param>
        /// 
        /// <seealso cref="PhysicsSettingsType"/>
        /// 
        public static void SerializePhysics(PhysicsSettingsType serializedPhysics,
            bool enableCollisions, bool enableGravity)
        {
            // Check assertions
            if (serializedPhysics is null)
            {
                Debug.LogError("[" + ClassName + "] Supplied serialized physics settings is null");
                return;
            }

            // Serialize collisions
            serializedPhysics.EnableCollisions = enableCollisions;

            // Serialize gravity
            serializedPhysics.EnableGravity = enableGravity;
        }
        #endregion Physics

        #region Locomotion Constraints
        public static void DeserializeLocomotionGravityConstraint(
            LocomotionGravityConstraintType serializedGravityConstraint,
            ref GravityConstraint gravityConstraint)
        {
            // Locomotion gravity constraints
            switch (serializedGravityConstraint)
            {
                case LocomotionGravityConstraintType.Prohibited:
                    gravityConstraint = GravityConstraint.Prohibited;
                    break;
                case LocomotionGravityConstraintType.Required:
                    gravityConstraint = GravityConstraint.Required;
                    break;

                case LocomotionGravityConstraintType.Allowed:
                default:
                    gravityConstraint = GravityConstraint.Allowed;
                    break;
            }
        }

        public static void SerializeLocomotionGravityConstraint(
            GravityConstraint gravityConstraint,
            LocomotionGravityConstraintType serializedGravityConstraint)
        {
            // Gravity constraints
            switch (gravityConstraint)
            {
                case GravityConstraint.Prohibited:
                    serializedGravityConstraint = LocomotionGravityConstraintType.Prohibited;
                    break;
                case GravityConstraint.Required:
                    serializedGravityConstraint = LocomotionGravityConstraintType.Required;
                    break;

                case GravityConstraint.Allowed:
                default:
                    serializedGravityConstraint = LocomotionGravityConstraintType.Allowed;
                    break;
            }
        }

        public static void DeserializeLocomotionConstraints(
            LocomotionConstraintsType serializedLocomotionConstraint,
            ref GravityConstraint gravityConstraint, ref float slow, ref float normal, ref float fast)
        {
            // Gravity
            SchemaUtil.DeserializeLocomotionGravityConstraint(serializedLocomotionConstraint.gravityConstraint, ref gravityConstraint);

            // Slow multipler
            slow = float.NaN;
            if (serializedLocomotionConstraint.SlowMultiplierSpecified)
            {
                slow = serializedLocomotionConstraint.SlowMultiplier;
            }

            // Normal multipler
            normal = float.NaN;
            if (serializedLocomotionConstraint.NormalMultiplierSpecified)
            {
                slow = serializedLocomotionConstraint.NormalMultiplier;
            }

            // Fast multipler
            fast = float.NaN;
            if (serializedLocomotionConstraint.FastMultiplierSpecified)
            {
                slow = serializedLocomotionConstraint.FastMultiplier;
            }
        }

        public static void SerializeLocomotionConstraints(
            GravityConstraint gravityConstraint, float slow, float normal, float fast,
            LocomotionConstraintsType serializedLocomotionConstraint)
        {
            // Gravity
            SchemaUtil.SerializeLocomotionGravityConstraint(gravityConstraint, serializedLocomotionConstraint.gravityConstraint);

            // Slow multipler
            serializedLocomotionConstraint.SlowMultiplierSpecified = !float.IsNaN(slow);
            if (serializedLocomotionConstraint.SlowMultiplierSpecified)
            {
                serializedLocomotionConstraint.SlowMultiplier = slow;
            }

            // Normal multipler
            serializedLocomotionConstraint.NormalMultiplierSpecified = !float.IsNaN(normal);
            if (serializedLocomotionConstraint.NormalMultiplierSpecified)
            {
                serializedLocomotionConstraint.NormalMultiplier = normal;
            }

            // Fast multipler
            serializedLocomotionConstraint.FastMultiplierSpecified = !float.IsNaN(fast);
            if (serializedLocomotionConstraint.FastMultiplierSpecified)
            {
                serializedLocomotionConstraint.FastMultiplier = fast;
            }
        }
        #endregion Locomotion Constraints

        #region Interfaces
        public static void DeserializeGMSECConnectionType(
            GMSECConnectionType serializedConnectionType,
            ref GMSECBusToDataManager.ConnectionTypes connectionType)
        {
            // GMSEC connection
            switch (serializedConnectionType)
            {
                case GMSECConnectionType.mb:
                    connectionType = GMSECBusToDataManager.ConnectionTypes.mb;
                    break;
                case GMSECConnectionType.amq383:
                    connectionType = GMSECBusToDataManager.ConnectionTypes.amq383;
                    break;
                case GMSECConnectionType.amq384:
                    connectionType = GMSECBusToDataManager.ConnectionTypes.amq384;
                    break;
                case GMSECConnectionType.ws71:
                    connectionType = GMSECBusToDataManager.ConnectionTypes.ws71;
                    break;
                case GMSECConnectionType.ws75:
                    connectionType = GMSECBusToDataManager.ConnectionTypes.ws75;
                    break;
                case GMSECConnectionType.ws80:
                    connectionType = GMSECBusToDataManager.ConnectionTypes.ws80;
                    break;

                case GMSECConnectionType.bolt:
                default:
                    connectionType = GMSECBusToDataManager.ConnectionTypes.bolt;
                    break;
            }
        }

        public static void SerializeGMSECConnectionType(
            GMSECBusToDataManager.ConnectionTypes connectionType,
            ref GMSECConnectionType serializedConnectionType)
        {
            // GMSEC connection
            switch (connectionType)
            {
                case GMSECBusToDataManager.ConnectionTypes.mb:
                    serializedConnectionType = GMSECConnectionType.mb;
                    break;
                case GMSECBusToDataManager.ConnectionTypes.amq383:
                    serializedConnectionType = GMSECConnectionType.amq383;
                    break;
                case GMSECBusToDataManager.ConnectionTypes.amq384:
                    serializedConnectionType = GMSECConnectionType.amq384;
                    break;
                case GMSECBusToDataManager.ConnectionTypes.ws71:
                    serializedConnectionType = GMSECConnectionType.ws71;
                    break;
                case GMSECBusToDataManager.ConnectionTypes.ws75:
                    serializedConnectionType = GMSECConnectionType.ws75;
                    break;
                case GMSECBusToDataManager.ConnectionTypes.ws80:
                    serializedConnectionType = GMSECConnectionType.ws80;
                    break;

                case GMSECBusToDataManager.ConnectionTypes.bolt:
                default:
                    serializedConnectionType = GMSECConnectionType.bolt;
                    break;
            }
        }

        public static void DeserializeMatlabCommandType(
            MatlabCommandType serializedCommandType,
            ref MATLABCommandHandler.MatlabCommandType commandType)
        {
            // Matlab command
            switch (serializedCommandType)
            {
                case MatlabCommandType.Legacy:
                    commandType = MATLABCommandHandler.MatlabCommandType.Legacy;
                    break;

                case MatlabCommandType.JSON:
                default:
                    commandType = MATLABCommandHandler.MatlabCommandType.JSON;
                    break;
            }
        }

        public static void SerializeMatlabCommandType(
            MATLABCommandHandler.MatlabCommandType commandType,
            ref MatlabCommandType serializedCommandType)
        {
            // Matlab command
            switch (commandType)
            {
                case MATLABCommandHandler.MatlabCommandType.Legacy:
                    serializedCommandType = MatlabCommandType.Legacy;
                    break;

                case MATLABCommandHandler.MatlabCommandType.JSON:
                default:
                    serializedCommandType = MatlabCommandType.JSON;
                    break;
            }
        }
        #endregion Interfaces

        #region Feeds
        public static void DeserializeFeedSpriteType(
            SpriteFeedTypeSpriteType serializedSpriteType,
            ref FeedSource.SpriteType spriteType)
        {
            // Feed sprite type
            switch (serializedSpriteType)
            {
                case SpriteFeedTypeSpriteType.Number:
                    spriteType = FeedSource.SpriteType.number;
                    break;

                case SpriteFeedTypeSpriteType.Toggle:
                default:
                    spriteType = FeedSource.SpriteType.toggle;
                    break;
            }
        }

        public static void SerializeFeedSpriteType(
            FeedSource.SpriteType spriteType,
            SpriteFeedTypeSpriteType serializedSpriteType)
        {
            // Gravity constraints
            switch (spriteType)
            {
                case FeedSource.SpriteType.number:
                    serializedSpriteType = SpriteFeedTypeSpriteType.Number;
                    break;

                case FeedSource.SpriteType.toggle:
                default:
                    serializedSpriteType = SpriteFeedTypeSpriteType.Toggle;
                    break;
            }
        }
        #endregion Feeds
    }
}