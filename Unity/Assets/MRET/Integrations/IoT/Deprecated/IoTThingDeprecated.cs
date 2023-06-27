// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
#if MRET_EXTENSION_SYSTEMTEXTJSON
using System.Text.Json;
#endif
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Integrations.IoT.IoTThing) + " class")]
    public class IoTThingDeprecated
    {
        public string ID;
        public string name;
        public string description;
        public bool pairs;

        public Vector3 location;
        //If the payload is pairs, this dictionary uses keys defined in the xml to find IoTThingDeprecatedValues
        private Dictionary<string, IoTThingValueDeprecated> payloadPairs;
        //If the payload is a value, IoTThingValueDeprecated stores its type and color and thresholds
        private IoTThingValueDeprecated payloadValue;

        //empty constructor
        public IoTThingDeprecated()
        {
            payloadPairs = new Dictionary<string, IoTThingValueDeprecated>();
        }
        //constructor for an IoTThingDeprecated with pairs
        public IoTThingDeprecated(string ID, string name, string description, bool pairs, Dictionary<string, IoTThingValueDeprecated> payloadPairs)
        {
            this.name = name;
            this.ID = ID;
            this.description = description;
            this.pairs = pairs;
            this.payloadPairs = payloadPairs;
        }
        //constructor for an IoTThingDeprecated with values
        public IoTThingDeprecated(string ID, string name, string description, bool pairs, IoTThingValueDeprecated payloadValue)
        {
            this.name = name;
            this.ID = ID;
            this.description = description;
            this.pairs = pairs;
            this.payloadValue = payloadValue;
        }

        public IoTThingPayloadValueTypeType? PairsElementPayloadType(string key)
        {
            IoTThingValueDeprecated value;
            if (payloadPairs.TryGetValue(key, out value))
            {
                return value.GetValueType();
            }
            return null;
        }

        public IoTThingPayloadValueTypeType ValuePayloadType(IoTPayloadDeprecated payload)
        {
            return payloadValue.GetValueType();
        }
        //this method returns the correct color to use for a given key if the payload is a list of pairs
        //If the key does not exist or does not have a color assigned to it (possibly due to not having thresholds) this method returns black
        public Color GetPairsElementColor(string key, IoTPayloadDeprecated payload)
        {
            if (pairs)
            {
                IoTThingValueDeprecated value;
                if (payloadPairs.TryGetValue(key, out value))
                {
                    return PayloadToColor(value, payload);
                }
            }
            return Color.black;
        }
        //This method returns the correct color if the payload is a single value
        public Color GetValueColor(IoTPayloadDeprecated payload)
        {
            if (!pairs)
            {
                return PayloadToColor(payloadValue, payload);
            }
            return Color.black;
        }
        //this method returns the correct color (colorType defined in Project.cs) to use for a given key if the payload is a list of pairs
        //If the key does not exist or does not have a color assigned to it (possibly due to not having thresholds) this method returns black
        public ColorType GetPairsElementIoTColor(string key, IoTPayloadDeprecated payload)
        {
            if (pairs)
            {
                IoTThingValueDeprecated value;
                if (payloadPairs.TryGetValue(key, out value))
                {
                    return PayloadToIoTColor(value, payload);
                }
            }
            return ColorType.Black;
        }
        //This method returns the correct color (colorType defined in Project.cs) if the payload is a single value
        public ColorType GetValueIoTColor(IoTPayloadDeprecated payload)
        {
            if (!pairs)
            {
                return PayloadToIoTColor(payloadValue, payload);
            }
            return ColorType.Black;
        }
        //given a type and an object, get the color of the payload if any.
        //if no color is specified by threshholds return black.
        private Color PayloadToColor(IoTThingValueDeprecated value, IoTPayloadDeprecated payload)
        {
            return thingColorToUnityColor(PayloadToIoTColor(value, payload));
        }
        //given a type and an object, get the color of the payload if any.
        //if no color is specified by threshholds return black.
        private ColorType PayloadToIoTColor(IoTThingValueDeprecated value, IoTPayloadDeprecated payload)
        {
            ColorType color = payloadValue.defaultColor;
            float typedPayload;
            switch (value.GetValueType())
            {
                case IoTThingPayloadValueTypeType.@float:
                    typedPayload = (float)payload.payload;
                    break;
                case IoTThingPayloadValueTypeType.integer:
                    typedPayload = (int)payload.payload;
                    break;
                default:
                    return color;
            }

            foreach ((float, float, ColorType) threshold in value.thresholds)
            {
                if (threshold.Item1 < typedPayload && typedPayload < threshold.Item2)
                {
                    color = threshold.Item3;
                }
            }
            return color;
        }
        public (float, float) GetCurrentPairsElementThresholds(string pairsKey, IoTPayloadDeprecated payload)
        {
            float low = float.MinValue, high = float.MaxValue;
            float typedPayload;
            if (pairs)
            {
                switch (PairsElementPayloadType(pairsKey))
                {
                    case IoTThingPayloadValueTypeType.@float:
                        typedPayload = (float)payload.payload;
                        break;
                    case IoTThingPayloadValueTypeType.integer:
                        typedPayload = (int)payload.payload;
                        break;
                    default:
                        return (low, high);
                }
                IoTThingValueDeprecated value;
                if (payloadPairs.TryGetValue(pairsKey, out value))
                {
                    foreach ((float, float, ColorType) threshold in value.thresholds)
                    {
                        if (threshold.Item1 < typedPayload && typedPayload < threshold.Item2)
                        {
                            low = threshold.Item1;
                            high = threshold.Item2;
                        }
                    }
                }
            }
            return (low, high);
        }

        public (float, float) GetCurrentValueThresholds(IoTPayloadDeprecated payload)
        {
            float low = float.MinValue, high = float.MaxValue;
            float typedPayload;
            if (!pairs)
            {
                switch (payloadValue.GetValueType())
                {
                    case IoTThingPayloadValueTypeType.@float:
                        typedPayload = (float)payload.payload;
                        break;
                    case IoTThingPayloadValueTypeType.integer:
                        typedPayload = (int)payload.payload;
                        break;
                    default:
                        return (low, high);
                }
                foreach ((float, float, ColorType) threshold in payloadValue.thresholds)
                {
                    if (threshold.Item1 < typedPayload && typedPayload < threshold.Item2)
                    {
                        low = threshold.Item1;
                        high = threshold.Item2;
                    }
                }
            }
            return (low, high);
        }

        public object PairsByteToType(byte[] bytes)
        {
            if (pairs)
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                Dictionary<string, JsonElement> split = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes);
                Dictionary<string, object> output = new Dictionary<string, object>();
                foreach (KeyValuePair<string, JsonElement> element in split)
                {
                    output.Add(element.Key, CastFromJsonElementToPairElementType(element.Key, element.Value));
                }
                return output;
#endif
            }
            return null;
        }
        public object ValuesByteToType(byte[] bytes, out double timeStamp)
        {
            string payload = System.Text.Encoding.UTF8.GetString(bytes);
            timeStamp = 0;
#if MRET_EXTENSION_SYSTEMTEXTJSON
            JsonElement jsonTimeStamp = new JsonElement();
            //if the payload is in a json structure, but is still a single value
            if (payload[0] == '{')
            {
                Dictionary<string, JsonElement> split = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes);
                JsonElement singleValue = new JsonElement();
                if (split.Count == 1)
                {
                    if (split.TryGetValue("value", out singleValue))
                    {
                        return CastFromJsonElementToValueType(singleValue);
                    }
                }
                else if (split.Count == 2)
                {
                    if (split.TryGetValue("value", out singleValue))
                    {
                        if (split.TryGetValue("ts", out jsonTimeStamp))
                        {
                            timeStamp = jsonTimeStamp.GetDouble();
                            return CastFromJsonElementToValueType(singleValue);
                        }
                    }

                }
                //if the xml says it's a value, but it's really a json structure of many pairs
                else
                {
                    return singleValue;
                }
            }
#endif
            switch (payloadValue.GetValueType())
            {
                case IoTThingPayloadValueTypeType.@float:
                    return float.Parse(payload);
                case IoTThingPayloadValueTypeType.integer:
                    return int.Parse(payload);
                case IoTThingPayloadValueTypeType.@string:
                    return payload;
                default:
                    return null;
            }
        }
        //this function is overloaded, but either discards the timestamp or returns it through an out parameter
        public object ValuesByteToType(byte[] bytes)
        {
            double discard;
            return ValuesByteToType(bytes, out discard);
        }

#if MRET_EXTENSION_SYSTEMTEXTJSON
        public object CastFromJsonElementToPairElementType(string key, JsonElement input)
        {
            IoTThingPayloadValueTypeType? pairElementType = GetPairsElementType(key);
            if (pairElementType.HasValue)
            {
                switch (pairElementType.GetValueOrDefault())
                {
                    case IoTThingPayloadValueTypeType.@float:
                        return input.GetSingle();
                    case IoTThingPayloadValueTypeType.integer:
                        return input.GetInt32();
                    case IoTThingPayloadValueTypeType.@string:
                        return input.GetString();
                    default:
                        return null;
                }
            }
            return null;
        }
        public object CastFromJsonElementToValueType(JsonElement input)
        {
            switch (payloadValue.GetValueType())
            {
                case IoTThingPayloadValueTypeType.@float:
                    return input.GetSingle();
                case IoTThingPayloadValueTypeType.integer:
                    return input.GetInt32();
                case IoTThingPayloadValueTypeType.@string:
                    return input.GetString();
                default:
                    return null;
            }
        }
#endif

        //AddPairs returns false if the key already exists in the dictionary
        public bool AddPairsElement(string key, IoTThingValueDeprecated value)
        {
            if (!payloadPairs.ContainsKey(key))
            {
                payloadPairs.Add(key, value);
                return true;
            }
            return false;
        }
        //AddPairs returns false if the key already exists in the dictionary
        public bool RemovePairsElement(string key)
        {
            if (payloadPairs.ContainsKey(key))
            {
                payloadPairs.Remove(key);
                return true;
            }
            return false;
        }
        public bool GetPairsElement(string key, out IoTThingValueDeprecated value)
        {

            if (payloadPairs.TryGetValue(key, out value))
            {
                return true;
            }
            return false;
        }
        public IoTThingPayloadValueTypeType? GetPairsElementType(string key)
        {
            IoTThingValueDeprecated value;
            if (payloadPairs.TryGetValue(key, out value))
            {
                return value.GetValueType();
            }
            return null;
        }
        public bool IsPairs()
        {
            return pairs;
        }
        public int GetPairsCount()
        {
            return payloadPairs.Count;
        }
        public List<(float, float, ColorType)> GetPairsElementThresholds(string key)
        {
            IoTThingValueDeprecated value;
            //if the element has threshholds
            if (pairs && payloadPairs.TryGetValue(key, out value))
            {
                return value.thresholds;
            }
            return null;
        }
        public List<(float, float, ColorType)> GetValueThresholds()
        {
            if (!pairs)
            {
                return payloadValue.thresholds;
            }
            else
            {
                return null;
            }
        }

        //TODO: If we want to add threshholds to pairs fix this method
        // public DataManager.DataValue AddPairsElementThresholdsToDataValue(DataManager.DataValue dataPoint, string payloadPairKey)
        // {
        //     if (pairs)
        //     {
        //         List<(float, float, ColorType)> currentThresholds = GetPairsElementThresholds(payloadPairKey);
        //         if (dataPoint != null && currentThresholds != null)
        //         {
        //             IoTThingValueDeprecated value = new IoTThingValueDeprecated();
        //             this.GetPairsElement(payloadPairKey, out value);
        //             dataPoint.arbitraryColorThresholds.Add((IoTColorToLimitState(value.defaultColor), float.MinValue, float.MaxValue));
        //             foreach ((float, float, ColorType) threshold in currentThresholds)
        //             {
        //                 dataPoint.arbitraryColorThresholds.Add((IoTColorToLimitState(threshold.Item3), threshold.Item1, threshold.Item2));
        //             }
        //         }
        //     }
        //     return dataPoint;
        // }

        //AddThresholdsToDataValue adds the thresholds of this IoTThingDeprecated into the datavalue passed in.
        //This only works for payloads which are values, not pairs since the thresholds are specific to each element of the pairs list
        public DataManager.DataValue AddThresholdsToDataValue(DataManager.DataValue dataPoint)
        {
            if (!pairs)
            {
                List<(float, float, ColorType)> currentThresholds = GetValueThresholds();
                if (dataPoint != null && currentThresholds != null)
                {
                    dataPoint.clearThresholds();
                    dataPoint.arbitraryColorThresholds.Add((IoTColorToLimitState(payloadValue.defaultColor), float.MinValue, float.MaxValue));
                    foreach ((float, float, ColorType) threshold in currentThresholds)
                    {
                        dataPoint.arbitraryColorThresholds.Add((IoTColorToLimitState(threshold.Item3), threshold.Item1, threshold.Item2));
                    }
                }
            }
            return dataPoint;
        }

        //convert the color class defined in project.cs (created automatically from xml) to Unity Color
        public static Color thingColorToUnityColor(ColorType thingColor)
        {
            switch (thingColor)
            {
                case ColorType.Blue:
                    return Color.blue;
                case ColorType.Cyan:
                    return Color.cyan;
                case ColorType.DarkGray:
                    return new Color(0.25f, 0.25f, 0.25f);
                case ColorType.Gray:
                    return Color.gray;
                case ColorType.Green:
                    return Color.green;
                case ColorType.LightGray:
                    return new Color(0.75f, 0.75f, 0.75f);
                case ColorType.Magenta:
                    return Color.magenta;
                case ColorType.Orange:
                    return new Color(1f, 0.647058824f, 0f);
                case ColorType.Pink:
                    return new Color(1f, 0.752941176f, 0.796078431f);
                case ColorType.Red:
                    return Color.red;
                case ColorType.White:
                    return Color.white;
                case ColorType.Yellow:
                    return Color.yellow;
                case ColorType.Black:
                    return Color.black;
                default:
                    return Color.black;
            }

        }
        //convert ColorType defined in project.cs (automatically made from xml) to Datamanager.limitstate
        public DataManager.DataValue.LimitState IoTColorToLimitState(ColorType color)
        {
            switch (color)
            {
                case ColorType.Black:
                    return DataManager.DataValue.LimitState.Black;
                case ColorType.Blue:
                    return DataManager.DataValue.LimitState.Blue;
                case ColorType.Cyan:
                    return DataManager.DataValue.LimitState.Cyan;
                case ColorType.DarkGray:
                    return DataManager.DataValue.LimitState.DarkGray;
                case ColorType.Gray:
                    return DataManager.DataValue.LimitState.Gray;
                case ColorType.Green:
                    return DataManager.DataValue.LimitState.Green;
                case ColorType.LightGray:
                    return DataManager.DataValue.LimitState.LightGray;
                case ColorType.Magenta:
                    return DataManager.DataValue.LimitState.Magenta;
                case ColorType.Orange:
                    return DataManager.DataValue.LimitState.Orange;
                case ColorType.Pink:
                    return DataManager.DataValue.LimitState.Pink;
                case ColorType.Red:
                    return DataManager.DataValue.LimitState.Red;
                case ColorType.White:
                    return DataManager.DataValue.LimitState.White;
                case ColorType.Yellow:
                    return DataManager.DataValue.LimitState.Yellow;
                default:
                    return DataManager.DataValue.LimitState.Undefined;
            }
        }
    }

    [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Integrations.IoT.IoTPayload) + " class")]
    public class IoTPayloadDeprecated
    {
        public string ID;
        public object payload;
        //timeStamp is only used if the payload meets the format {"ts"=__, "value"=__} and it stores the time the measurement was taken
        public DateTime timeStamp;
        //this constructor takes thingID and a byte payload so it can convert that into an object
        public IoTPayloadDeprecated(string ID, byte[] payload)
        {
            IoTThingDeprecated thing = IoTManagerDeprecated.instance.IoTThings[ID];
            this.ID = ID;
            this.payload = payload;
            if (thing.pairs)
            {
                this.payload = thing.PairsByteToType(payload);
            }
            else
            {
                double doubleTimeStamp = 0;
                this.payload = thing.ValuesByteToType(payload, out doubleTimeStamp);
                DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                this.timeStamp = timeStamp.AddMilliseconds(doubleTimeStamp).ToLocalTime();
            }
        }
        //this constructor takes thingID and a payload that has already been converted from bytes to object
        public IoTPayloadDeprecated(string ID, object payload)
        {
            this.ID = ID;
            this.payload = payload;
        }
        public IoTThingDeprecated GetIoTThingDeprecated()
        {
            IoTThingDeprecated thing;
            if (IoTManagerDeprecated.instance.IoTThings.TryGetValue(ID, out thing))
            {
                return thing;
            }
            else
            {
                return null;
            }
        }
        //Write the payload and ID to the datamanager seperately so that a user can just use the payload
        //and treat it the same as any other data, or they can also get the thingID to use helper methods
        public void WriteToDataManager(string payloadKey, string IDKey)
        {
            //If the payload matches a specific format, write it as a value instead of a dictionary of pairs
            DataManager.DataValue payloadDataValue = new DataManager.DataValue(payloadKey, payload);
            payloadDataValue.telemetryTS = timeStamp;
            IoTThingDeprecated thing = new IoTThingDeprecated();
            if (IoTManagerDeprecated.instance.GetThing(ID, out thing))
            {
                if (!thing.pairs)
                {
                    //if the payload is a value, then thresholds make sense
                    thing.AddThresholdsToDataValue(payloadDataValue);
                }
            }
            MRET.DataManager.SaveValue(payloadDataValue);
            MRET.DataManager.SaveValue(IDKey, ID);
        }
    }

    //This class stores the context for a singular value
    [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Integrations.IoT.IoTThingValue) + " class")]
    public class IoTThingValueDeprecated
    {
        private IoTThingPayloadValueTypeType type;
        public ColorType defaultColor;
        public List<(float, float, ColorType)> thresholds;

        public string GetAsString(object payload)
        {
            IoTThingPayloadValueTypeType type = GetValueType();
            return type.ToString();
        }
        public IoTThingPayloadValueTypeType GetValueType()
        {
            return type;
        }
        public void SetType(IoTThingPayloadValueTypeType type)
        {
            this.type = type;
        }
    }
}