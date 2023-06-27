// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Color;
using GOV.NASA.GSFC.XR.Utilities.Json;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    public class IoTThing : Identifiable<IoTThingType>, IIoTThing<IoTThingType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IoTThing);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private IoTThingType serializedIoTThing;

        #region IIoTThing
        /// <seealso cref="IIoTThing.IsPairs"/>
        public bool IsPairs { get; private set; }
        #endregion IIoTThing

        //If the payload is pairs, this dictionary uses keys defined in the xml to find IoTThingValues
        private Dictionary<string, IoTThingValue> payloadPairs;
        //If the payload is a value, IoTThingValue stores its type and color and thresholds
        private IoTThingValue payloadValue;

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Initialize defaults (before serialization)
            IsPairs = false;
            payloadPairs = new Dictionary<string, IoTThingValue>();
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Initialize defaults (after serialization)
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="Identifiable{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(IoTThingType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process the object specific configuration

            // Save the serialized reference
            serializedIoTThing = serialized;

            // Clear the old payload references
            payloadPairs.Clear();
            payloadValue = null;

            // Deserialize the payload (required)
            if (serializedIoTThing.Payload != null)
            {
                IoTThingPayloadType serializedPayload = serializedIoTThing.Payload;
                if (serializedPayload.Item is IoTThingPayloadPairsType)
                {
                    IoTThingPayloadPairsType serializedPairs = serializedPayload.Item as IoTThingPayloadPairsType;
                    if ((serializedPairs.Pair != null) || (serializedPairs.Pair.Length > 0))
                    {
                        IoTThingPayloadPairType[] pairArray = serializedPairs.Pair;
                        foreach (IoTThingPayloadPairType pair in pairArray)
                        {
                            // Create and deserialize the value
                            IoTThingValue value = new IoTThingValue();
                            StartCoroutine(payloadValue.Deserialize(pair.Value, deserializationState));

                            // Wait for the coroutine to complete
                            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                            // If the parent failed, there's no point in continuing
                            if (deserializationState.IsError) yield break;

                            // Add the pair to the payload pairs
                            AddPairsElement(pair.Key, value);
                        }
                        IsPairs = true;
                    }
                    else
                    {
                        // Report the error
                        deserializationState.Error("Payload pairs are empty");
                        yield break;
                    }
                }
                else if (serializedPayload.Item is IoTThingPayloadValueType)
                {
                    IoTThingPayloadValueType serializedValue = serializedPayload.Item as IoTThingPayloadValueType;
                    IsPairs = false;

                    // Create the thing value and deserialize
                    payloadValue = new IoTThingValue();
                    StartCoroutine(payloadValue.Deserialize(serializedValue, deserializationState));

                    // Wait for the coroutine to complete
                    while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // If the parent failed, there's no point in continuing
                    if (deserializationState.IsError) yield break;
                }
                else
                {
                    // Report the error
                    deserializationState.Error("Payload definition type is unrecognized");
                    yield break;
                }
            }
            else
            {
                // Report the error
                deserializationState.Error("Payload is not null");
                yield break;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Identifiable{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(IoTThingType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Serialize the object specific configuration

            // Serialize the IoT thing

            // Serialize the payload (required)
            IoTThingPayloadType serializedPayload = new IoTThingPayloadType();
            if (IsPairs)
            {
                if (payloadPairs.Count > 0)
                {
                    IoTThingPayloadPairType[] pairArray = new IoTThingPayloadPairType[payloadPairs.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, IoTThingValue> valuePair in payloadPairs)
                    {
                        // Serialize the pair value
                        IoTThingPayloadValueType serializedPayloadValue = new IoTThingPayloadValueType();
                        StartCoroutine(valuePair.Value.Serialize(serializedPayloadValue, serializationState));

                        // Wait for the coroutine to complete
                        while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                        // If the parent failed, there's no point in continuing
                        if (serializationState.IsError) yield break;

                        // Create the serialized pair
                        IoTThingPayloadPairType serializedPair = new IoTThingPayloadPairType();
                        serializedPair.Key = valuePair.Key;
                        serializedPair.Value = serializedPayloadValue;

                        // Store the serialized pair
                        pairArray[i++] = serializedPair;
                    }

                    // Create the serialized pairs and assign our serialized pairs array
                    IoTThingPayloadPairsType serializedPairs = new IoTThingPayloadPairsType();
                    serializedPairs.Pair = pairArray;

                    // Assign the serialized pairs
                    serializedPayload.Item = serializedPairs;
                }
                else
                {
                    // Report the error
                    serializationState.Error("No payload pairs defined for this IoTThing");
                    yield break;
                }
            }
            else if (payloadValue != null)
            {
                // Serialize the payload value
                IoTThingPayloadValueType serializedPayloadValue = new IoTThingPayloadValueType();
                StartCoroutine(payloadValue.Serialize(serializedPayloadValue, serializationState));

                // Wait for the coroutine to complete
                while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // If the parent failed, there's no point in continuing
                if (serializationState.IsError) yield break;

                // Assign the serialized value
                serializedPayload.Item = serializedPayloadValue;
            }
            else
            {
                // Report the error
                serializationState.Error("No payload defined for this IoTThing");
                yield break;
            }

            // Assign the payload
            serialized.Payload = serializedPayload;

            // Save the final serialized reference
            serializedIoTThing = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        public IoTThingPayloadValueTypeType? PairsElementPayloadType(string key)
        {
            if (payloadPairs.TryGetValue(key, out IoTThingValue value))
            {
                return value.ValueType;
            }
            return null;
        }

        public IoTThingPayloadValueTypeType ValuePayloadType()
        {
            return (payloadValue != null) ? payloadValue.ValueType : IoTThingDefaults.PAYLOAD_VALUE_TYPE;
        }

        //this method returns the correct color to use for a given key if the payload is a list of pairs
        //If the key does not exist or does not have a color assigned to it (possibly due to not having thresholds) this method returns black
        public Color32 GetPairsElementColor(string key, IoTPayload payload)
        {
            if (IsPairs && payloadPairs.TryGetValue(key, out IoTThingValue value))
            {
                return PayloadToColor(value, payload);
            }
            return IoTThingDefaults.COLOR;
        }

        //This method returns the correct color if the payload is a single value
        public Color32 GetValueColor(IoTPayload payload)
        {
            if (!IsPairs && (payloadValue != null))
            {
                return PayloadToColor(payloadValue, payload);
            }
            return IoTThingDefaults.COLOR;
        }

        /// <summary>
        /// Given a type and an object, get the color of the payload if any.
        /// if no color is specified by threshholds returns the default color.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        private Color32 PayloadToColor(IoTThingValue value, IoTPayload payload)
        {
            Color32 color = payloadValue.DefaultColor;
            float typedPayload;
            switch (value.ValueType)
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

            foreach ((float, float, Color32) threshold in value.Thresholds)
            {
                if (threshold.Item1 < typedPayload && typedPayload < threshold.Item2)
                {
                    color = threshold.Item3;
                }
            }
            return color;
        }

        public (float, float) GetCurrentPairsElementThresholds(string pairsKey, IoTPayload payload)
        {
            float low = float.MinValue, high = float.MaxValue;
            float typedPayload;
            if (IsPairs)
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

                if (payloadPairs.TryGetValue(pairsKey, out IoTThingValue value))
                {
                    foreach ((float, float, Color32) threshold in value.Thresholds)
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

        public (float, float) GetCurrentValueThresholds(IoTPayload payload)
        {
            float low = float.MinValue, high = float.MaxValue;
            float typedPayload;
            if (!IsPairs && (payloadValue != null))
            {
                switch (payloadValue.ValueType)
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

                foreach ((float, float, Color32) threshold in payloadValue.Thresholds)
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

        /// <seealso cref="IIoTThing.PairsJsonToType(string)"/>
        public Dictionary<string, object> PairsJsonToType(string json)
        {
            if (IsPairs)
            {
                Dictionary<string, object> split = JsonUtil.Deserialize<Dictionary<string, object>>(json);
                Dictionary<string, object> output = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> element in split)
                {
                    output.Add(element.Key, CastFromJsonElementToPairElementType(element.Key, element.Value));
                }
                return output;
            }
            return null;
        }

        /// <seealso cref="IIoTThing.ValueJsonToType(string, out double)"/>
        public object ValueJsonToType(string json, out double timeStamp)
        {
            timeStamp = 0d;
            //if the payload is in a json structure, but is still a single value
            if (!string.IsNullOrEmpty(json) && json.StartsWith("{"))
            {
                Dictionary<string, object> split = JsonUtil.Deserialize<Dictionary<string, object>>(json);
                object singleValue;
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
                        if (split.TryGetValue("ts", out object jsonTimeStamp))
                        {
                            try
                            {
                                timeStamp = JsonUtil.ToDouble(jsonTimeStamp);
                            }
                            catch (Exception)
                            {
                                LogWarning("There was a problem converting the time stamp: " + jsonTimeStamp);
                                timeStamp = 0d;
                            }
                        }
                        return CastFromJsonElementToValueType(singleValue);
                    }
                }
                //if the xml says it's a value, but it's really a json structure of many pairs
                else
                {
                    return split;
                }
            }

            // Try to convert the payload
            return ConvertJsonToValue(payloadValue.ValueType, json);
        }

        /// <seealso cref="IIoTThing.ValueJsonToType(string)"/>
        public object ValueJsonToType(string json)
        {
            return ValueJsonToType(json, out _);
        }

        protected object ConvertJsonToValue(IoTThingPayloadValueTypeType valueType, object data)
        {
            object result = null;
            try
            {
                switch (payloadValue.ValueType)
                {
                    case IoTThingPayloadValueTypeType.@double:
                        result = JsonUtil.ToDouble(data);
                        break;

                    case IoTThingPayloadValueTypeType.@float:
                        result = JsonUtil.ToSingle(data);
                        break;

                    case IoTThingPayloadValueTypeType.integer:
                        result = JsonUtil.ToInt32(data);
                        break;

                    case IoTThingPayloadValueTypeType.@string:
                    default:
                        result = JsonUtil.ToString(data);
                        break;
                }
            }
            catch (Exception)
            {
                LogWarning("JSON data could not be converted: " + data, nameof(CastFromJsonElementToValueType));
                result = null;
            }

            return result;
        }

        public object CastFromJsonElementToPairElementType(string key, object input)
        {
            object result = null;

            // If we have a value for the key, try to convert it
            IoTThingPayloadValueTypeType? pairElementType = GetPairsElementType(key);
            if (pairElementType.HasValue)
            {
                ConvertJsonToValue(pairElementType.GetValueOrDefault(), input);
            }

            return result;
        }

        public object CastFromJsonElementToValueType(object input)
        {
            return ConvertJsonToValue(payloadValue.ValueType, input);
        }

        //AddPairs returns false if the key already exists in the dictionary
        public bool AddPairsElement(string key, IoTThingValue value)
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

        public bool GetPairsElement(string key, out IoTThingValue value)
        {

            if (payloadPairs.TryGetValue(key, out value))
            {
                return true;
            }
            return false;
        }

        public IoTThingPayloadValueTypeType? GetPairsElementType(string key)
        {
            IoTThingValue value;
            if (payloadPairs.TryGetValue(key, out value))
            {
                return value.ValueType;
            }
            return null;
        }

        public int GetPairsCount()
        {
            return payloadPairs.Count;
        }

        public (float, float, Color32)[] GetPairsElementThresholds(string key)
        {
            //if the element has thresholds
            if (IsPairs && payloadPairs.TryGetValue(key, out IoTThingValue value))
            {
                return value.Thresholds;
            }
            return null;
        }

        public (float, float, Color32)[] GetValueThresholds()
        {
            if (!IsPairs && (payloadValue != null))
            {
                return payloadValue.Thresholds;
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
        //         List<(float, float, ColorPredefinedType)> currentThresholds = GetPairsElementThresholds(payloadPairKey);
        //         if (dataPoint != null && currentThresholds != null)
        //         {
        //             IoTThingValue value = new IoTThingValue();
        //             this.GetPairsElement(payloadPairKey, out value);
        //             dataPoint.arbitraryColorThresholds.Add((IoTColorToLimitState(value.defaultColor), float.MinValue, float.MaxValue));
        //             foreach ((float, float, ColorPredefinedType) threshold in currentThresholds)
        //             {
        //                 dataPoint.arbitraryColorThresholds.Add((IoTColorToLimitState(threshold.Item3), threshold.Item1, threshold.Item2));
        //             }
        //         }
        //     }
        //     return dataPoint;
        // }

        /// <seealso cref="IIoTThing.AddThresholdsToDataValue(DataManager.DataValue)"/>
        public DataManager.DataValue AddThresholdsToDataValue(DataManager.DataValue dataPoint)
        {
            if (!IsPairs && (payloadValue != null))
            {
                (float, float, Color32)[] currentThresholds = GetValueThresholds();
                if (dataPoint != null && currentThresholds != null)
                {
                    dataPoint.clearThresholds();
                    dataPoint.arbitraryColorThresholds.Add((ColorToLimitState(payloadValue.DefaultColor), float.MinValue, float.MaxValue));
                    foreach ((float, float, Color32) threshold in currentThresholds)
                    {
                        dataPoint.arbitraryColorThresholds.Add((ColorToLimitState(threshold.Item3), threshold.Item1, threshold.Item2));
                    }
                }
            }
            return dataPoint;
        }

        //convert ColorPredefinedType defined in project.cs (automatically made from xml) to Datamanager.limitstate
        public DataManager.DataValue.LimitState ColorToLimitState(Color32 color)
        {
            DataManager.DataValue.LimitState result = DataManager.DataValue.LimitState.Undefined;

            // Convert the color to a predefined color
            ColorPredefinedType serializedColor = ColorPredefinedType.Black;
            if (SchemaUtil.SerializeColorPredefined(color, ref serializedColor))
            {
                switch (serializedColor)
                {
                    case ColorPredefinedType.Black:
                        result = DataManager.DataValue.LimitState.Black;
                        break;
                    case ColorPredefinedType.Blue:
                        result = DataManager.DataValue.LimitState.Blue;
                        break;
                    case ColorPredefinedType.Cyan:
                        result = DataManager.DataValue.LimitState.Cyan;
                        break;
                    case ColorPredefinedType.DarkGray:
                        result = DataManager.DataValue.LimitState.DarkGray;
                        break;
                    case ColorPredefinedType.Gray:
                        result = DataManager.DataValue.LimitState.Gray;
                        break;
                    case ColorPredefinedType.Green:
                        result = DataManager.DataValue.LimitState.Green;
                        break;
                    case ColorPredefinedType.LightGray:
                        result = DataManager.DataValue.LimitState.LightGray;
                        break;
                    case ColorPredefinedType.Magenta:
                        result = DataManager.DataValue.LimitState.Magenta;
                        break;
                    case ColorPredefinedType.Orange:
                        result = DataManager.DataValue.LimitState.Orange;
                        break;
                    case ColorPredefinedType.Pink:
                        result = DataManager.DataValue.LimitState.Pink;
                        break;
                    case ColorPredefinedType.Red:
                        result = DataManager.DataValue.LimitState.Red;
                        break;
                    case ColorPredefinedType.White:
                        result = DataManager.DataValue.LimitState.White;
                        break;
                    case ColorPredefinedType.Yellow:
                        result = DataManager.DataValue.LimitState.Yellow;
                        break;
                    default:
                        result = DataManager.DataValue.LimitState.Undefined;
                        break;
                }
            }

            return result;
        }
    }

    public class IoTPayload
    {
        public string thingId;
        public object payload;
        //timeStamp is only used if the payload meets the format {"ts"=__, "value"=__} and it stores the time the measurement was taken
        public DateTime timeStamp;
        //this constructor takes thingID and a byte payload so it can convert that into an object
        public IoTPayload(string thingId, string payload)
        {
            this.thingId = thingId;
            this.payload = payload;
            if (IoTManager.Instance.GetThing(thingId, out IIoTThing thing))
            {
                if (thing.IsPairs)
                {
                    this.payload = thing.PairsJsonToType(payload);
                }
                else
                {
                    this.payload = thing.ValueJsonToType(payload, out double doubleTimeStamp);
                    DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    this.timeStamp = timeStamp.AddMilliseconds(doubleTimeStamp).ToLocalTime();
                }
            }
        }

        //this constructor takes thingID and a payload that has already been converted from bytes to object
        public IoTPayload(string thingId, object payload)
        {
            this.thingId = thingId;
            this.payload = payload;
        }

        public IIoTThing GetIoTThing()
        {
            IoTManager.Instance.GetThing(thingId, out IIoTThing thing);
            return thing;
        }

        //Write the payload and ID to the datamanager seperately so that a user can just use the payload
        //and treat it the same as any other data, or they can also get the thingID to use helper methods
        public void WriteToDataManager(string payloadKey, string IDKey)
        {
            //If the payload matches a specific format, write it as a value instead of a dictionary of pairs
            DataManager.DataValue payloadDataValue = new DataManager.DataValue(payloadKey, payload);
            payloadDataValue.telemetryTS = timeStamp;
            if (IoTManager.Instance.GetThing(thingId, out IIoTThing thing))
            {
                if (!thing.IsPairs)
                {
                    //if the payload is a value, then thresholds make sense
                    thing.AddThresholdsToDataValue(payloadDataValue);
                }
            }
            MRET.DataManager.SaveValue(payloadDataValue);
            MRET.DataManager.SaveValue(IDKey, thingId);
        }
    }

    //This class stores the context for a singular value
    public class IoTThingValue
    {
        public IoTThingPayloadValueTypeType ValueType { get => _valueType; }
        private IoTThingPayloadValueTypeType _valueType;
        public Color32 DefaultColor { get => _defaultColor; }
        private Color32 _defaultColor;
        public (float, float, Color32)[] Thresholds { get => _thresholds.ToArray(); }
        private List<(float, float, Color32)> _thresholds;

        public IoTThingValue()
        {
            _valueType = IoTThingDefaults.PAYLOAD_VALUE_TYPE;
            _defaultColor = IoTThingDefaults.COLOR;
            _thresholds = new List<(float, float, Color32)>();
        }

        /// <summary>
        /// Performs deserialization of the supplied serialized payload value into this class instance
        /// </summary>
        /// <param name="serializedPayloadValue">The serialized <code>IoTThingPayloadValueType</code> class
        ///     instance to read</param>
        /// <param name="deserializationState">The <code>SerializationState</code> to assign the
        ///     results of the deserialization.</param>
        /// <returns>An <code>IEnumerator</code> that can be used for asynchronous reentance</returns>
        public IEnumerator Deserialize(IoTThingPayloadValueType serializedPayloadValue, SerializationState deserializationState)
        {
            if (serializedPayloadValue == null)
            {
                // Record the deserialization state
                deserializationState.Error("Serialized payload value not defined");
                yield break;
            }

            float thingLow = float.MinValue;
            float thingHigh = float.MaxValue;
            _valueType = serializedPayloadValue.Type;

            if ((serializedPayloadValue == null) ||
                (serializedPayloadValue.Thresholds == null) ||
                (serializedPayloadValue.Thresholds.Threshold == null) ||
                (serializedPayloadValue.Thresholds.Threshold.Length == 0))
            {
                // Default threshold
                _thresholds.Add((thingLow, thingHigh, _defaultColor));
            }
            else
            {
                // Check if an alternate default color is specified for these thresholds
                if (serializedPayloadValue.Thresholds.Item is ColorComponentsType)
                {
                    SchemaUtil.DeserializeColorComponents(serializedPayloadValue.Thresholds.Item as ColorComponentsType, ref _defaultColor);
                }
                else if (serializedPayloadValue.Thresholds.Item is ColorPredefinedType)
                {
                    SchemaUtil.DeserializeColorPredefined((ColorPredefinedType)serializedPayloadValue.Thresholds.Item, ref _defaultColor);
                }

                // Deserialize each threshold
                foreach (IoTThingPayloadValueThresholdType threshold in serializedPayloadValue.Thresholds.Threshold)
                {
                    // Assign the threshold range
                    thingLow = float.MinValue;
                    thingHigh = float.MaxValue;
                    if (threshold.LowSpecified)
                    {
                        thingLow = threshold.Low;
                    }
                    if (threshold.HighSpecified)
                    {
                        thingHigh = threshold.High;
                    }

                    // Determine the color for this threshold if defined
                    Color32 color = _defaultColor;
                    if (threshold.Item is ColorComponentsType)
                    {
                        SchemaUtil.DeserializeColorComponents(threshold.Item as ColorComponentsType, ref color);
                    }
                    else if (threshold.Item is ColorPredefinedType)
                    {
                        SchemaUtil.DeserializeColorPredefined((ColorPredefinedType)threshold.Item, ref color);
                    }
                    _thresholds.Add((thingLow, thingHigh, color));
                }
            }

            // Mark as complete
            deserializationState.complete = true;
        }

        /// <summary>
        /// Performs serialization this class instance into the supplied serialized payload value
        /// </summary>
        /// <param name="serializedPayloadValue">The serialized <code>IoTThingPayloadValueType</code> class
        ///     instance to write</param>
        /// <param name="serializationState">The <code>SerializationState</code> to assign the
        ///     results of the serialization.</param>
        /// <returns>An <code>IEnumerator</code> that can be used for asynchronous reentance</returns>
        public IEnumerator Serialize(IoTThingPayloadValueType serializedPayloadValue, SerializationState serializationState)
        {
            if (serializedPayloadValue == null)
            {
                // Record the deserialization state
                serializationState.Error("Serialized payload value not defined");
                yield break;
            }

            // Type
            serializedPayloadValue.Type = ValueType;

            // Determine if we have any thresholds
            serializedPayloadValue.Thresholds = null;
            if (_thresholds.Count > 0)
            {
                // Check for default case
                if ((_thresholds.Count == 1) &&
                    (_thresholds[0].Item1 == float.MinValue) &&
                    (_thresholds[0].Item2 == float.MaxValue) &&
                    (ColorUtil.ColorRGBAEquals(_thresholds[0].Item3, IoTThingDefaults.COLOR)))
                {
                    // No thresholds
                }
                else
                {
                    // Create the serialized struct to populate
                    IoTThingPayloadValueThresholdsType serializedThresholds = new IoTThingPayloadValueThresholdsType();

                    // Alternate default color
                    if (!ColorUtil.ColorRGBAEquals(DefaultColor, IoTThingDefaults.COLOR))
                    {
                        ColorPredefinedType serializedPredefinedColor = ColorPredefinedType.Black;
                        if (SchemaUtil.SerializeColorPredefined(DefaultColor, ref serializedPredefinedColor))
                        {
                            // Predefined color
                            serializedThresholds.Item = serializedPredefinedColor;
                        }
                        else
                        {
                            // Color Components
                            ColorComponentsType serializedColor = new ColorComponentsType();
                            SchemaUtil.SerializeColorComponents(DefaultColor, serializedColor);
                            serializedThresholds.Item = serializedColor;
                        }
                    }

                    // Thresholds
                    List<IoTThingPayloadValueThresholdType> thresholdList = new List<IoTThingPayloadValueThresholdType>();
                    foreach ((float, float, Color32) threshold in _thresholds)
                    {
                        // Create the serialized threshold to populate
                        IoTThingPayloadValueThresholdType serializedThreshold = new IoTThingPayloadValueThresholdType();

                        // Low value
                        if (threshold.Item1 != float.MinValue)
                        {
                            serializedThreshold.Low = threshold.Item1;
                            serializedThreshold.LowSpecified = true;
                        }

                        // High value
                        if (threshold.Item2 != float.MaxValue)
                        {
                            serializedThreshold.High = threshold.Item2;
                            serializedThreshold.HighSpecified = true;
                        }

                        // Color
                        if (!ColorUtil.ColorRGBAEquals(threshold.Item3, IoTThingDefaults.COLOR))
                        {
                            ColorPredefinedType serializedPredefinedColor = ColorPredefinedType.Black;
                            if (SchemaUtil.SerializeColorPredefined(threshold.Item3, ref serializedPredefinedColor))
                            {
                                // Predefined color
                                serializedThreshold.Item = serializedPredefinedColor;
                            }
                            else
                            {
                                // Color Components
                                ColorComponentsType serializedColor = new ColorComponentsType();
                                SchemaUtil.SerializeColorComponents(threshold.Item3, serializedColor);
                                serializedThreshold.Item = serializedColor;
                            }
                        }

                        // Add the serialized threshold to the list
                        thresholdList.Add(serializedThreshold);
                    }

                    // Assign the array of thresholds
                    serializedThresholds.Threshold = thresholdList.ToArray();

                    // Assign the serialized thresholds
                    serializedPayloadValue.Thresholds = serializedThresholds;
                }
            }

            // Mark as complete
            serializationState.complete = true;

            yield return null;
        }
    }

    public class IoTThingDefaults
    {
        // We want to use the default values from the schema to keep in sync
        public static readonly Color32 COLOR = Color.black;
        public static readonly IoTThingPayloadValueTypeType PAYLOAD_VALUE_TYPE = new IoTThingPayloadValueType().Type;
    }
}