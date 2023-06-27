// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MRET_EXTENSION_SYSTEMTEXTJSON
using System.Text.Json;
#endif
#if MRET_EXTENSION_NEWTONSOFTJSON
using Newtonsoft.Json;
#endif

namespace GOV.NASA.GSFC.XR.Utilities.Json
{
    /// <remarks>
    /// History:
    /// 19 June 2023: Created
    /// </remarks>
    ///
    /// <summary>
    /// JsonException
    ///
    /// JSON utility class intended to wrap different JSON implementations
    /// depending on availability in the Unity project
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class JsonUtil
    {
        public static TValue Deserialize<TValue>(string json, params object[] context)
        {
            TValue result = default;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                // Preferred method if available
                JsonSerializerOptions jsonOptions = null;
                if ((context != null) && (context.Length == 1) && (context[0] is JsonSerializerOptions))
                {
                    jsonOptions = context[0] as JsonSerializerOptions;
                }

                // Deserialize
                result = System.Text.Json.JsonSerializer.Deserialize<TValue>(json, jsonOptions);
#elif MRET_EXTENSION_NEWTONSOFTJSON
                // Newtonsoft
                JsonSerializerSettings jsonSettings = null;
                if ((context != null) && (context.Length == 1) && (context[0] is JsonSerializerSettings))
                {
                    jsonSettings = context[0] as JsonSerializerSettings;
                }

                // Deserialize
                result = JsonConvert.DeserializeObject<TValue>(json, jsonSettings);
#else
                // Built in Unity JSON handling relies on the Unity serialization model and doesn't
                // produce desired results. Dictionary is not serializable so just throw an exception
                // if we get here.
                throw new JsonException("JSON deserialization unavailable");
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        public static string Serialize<TValue>(TValue value, params object[] context)
        {
            string result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                // Preferred method if available
                JsonSerializerOptions jsonOptions = null;
                if ((context != null) && (context.Length == 1) && (context[0] is JsonSerializerOptions))
                {
                    jsonOptions = context[0] as JsonSerializerOptions;
                }

                // Serialize
                result = System.Text.Json.JsonSerializer.Serialize(value, jsonOptions);
#elif MRET_EXTENSION_NEWTONSOFTJSON
                // Newtonsoft
                JsonSerializerSettings jsonSettings = null;
                if ((context != null) && (context.Length == 1) && (context[0] is JsonSerializerSettings))
                {
                    jsonSettings = context[0] as JsonSerializerSettings;
                }

                // Serialize
                result = JsonConvert.SerializeObject(value, jsonSettings);
#else
                // Built in Unity JSON handling relies on the Unity serialization model and doesn't
                // produce desired results. Dictionary is not serializable so just throw an exception
                // if we get here.
                throw new JsonException("JSON serialization unavailable");
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        #region Conversions
        /// <summary>
        /// Converts the supplied JSON object to a datetime
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The datetime conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static DateTime ToDateTime(object json)
        {
            DateTime result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetDateTime()
                    : Convert.ToDateTime(json);
#else
                result = Convert.ToDateTime(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a string
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The string conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static string ToString(object json)
        {
            string result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetString()
                    : Convert.ToString(json);
#else
                result = Convert.ToString(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a boolean
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The boolean conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static bool ToBoolean(object json)
        {
            bool result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetBoolean()
                    : Convert.ToBoolean(json);
#else
                result = Convert.ToBoolean(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a sbyte
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The sbyte conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static sbyte ToSByte(object json)
        {
            sbyte result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetSByte()
                    : Convert.ToSByte(json);
#else
                result = Convert.ToSByte(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a byte
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The byte conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static byte ToByte(object json)
        {
            byte result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetByte()
                    : Convert.ToByte(json);
#else
                result = Convert.ToByte(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a int16
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The int16 conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static short ToInt16(object json)
        {
            short result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetInt16()
                    : Convert.ToInt16(json);
#else
                result = Convert.ToInt16(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a uint16
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The uint16 conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static ushort ToUInt16(object json)
        {
            ushort result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetUInt16()
                    : Convert.ToUInt16(json);
#else
                result = Convert.ToUInt16(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a int32
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The int32 conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static int ToInt32(object json)
        {
            int result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetInt32()
                    : Convert.ToInt32(json);
#else
                result = Convert.ToInt32(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a uint32
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The uint32 conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static uint ToUInt32(object json)
        {
            uint result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetUInt32()
                    : Convert.ToUInt32(json);
#else
                result = Convert.ToUInt32(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a int64
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The int64 conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static long ToInt64(object json)
        {
            long result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetInt64()
                    : Convert.ToInt64(json);
#else
                result = Convert.ToInt64(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a uint64
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The uint64 conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static ulong ToUInt64(object json)
        {
            ulong result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetUInt64()
                    : Convert.ToUInt64(json);
#else
                result = Convert.ToUInt64(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a decimal
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The decimal conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static decimal ToDecimal(object json)
        {
            decimal result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetDecimal()
                    : Convert.ToDecimal(json);
#else
                result = Convert.ToDecimal(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a float
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The float conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static float ToSingle(object json)
        {
            float result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetSingle()
                    : Convert.ToSingle(json);
#else
                result = Convert.ToSingle(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }

        /// <summary>
        /// Converts the supplied JSON object to a double
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>The double conversion</returns>
        /// <exception cref="JsonException">If there is an issue converting the JSON</exception>
        public static double ToDouble(object json)
        {
            double result;
            try
            {
#if MRET_EXTENSION_SYSTEMTEXTJSON
                result = (json is JsonElement)
                    ? ((JsonElement)json).GetDouble()
                    : Convert.ToDouble(json);
#else
                result = Convert.ToDouble(json);
#endif
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message, e.InnerException);
            }

            return result;
        }
        #endregion Conversions

        [Serializable]
        protected class JsonKeyValuePair
        {
            [field: SerializeField] public string Key { get; set; }
            [field: SerializeField] public object Value { get; set; }
        }

        public static void TestSerialization()
        {
            // JSON Object Test
            string jsonTest1 = @"{""Key"": ""MyKey"", ""Value"": 2}";
            Debug.Log("JSON Object Test:\nJSON: " + jsonTest1);
            try
            {
                // Deserialize
                JsonKeyValuePair jsonPair = Deserialize<JsonKeyValuePair>(jsonTest1);
                Debug.Log("JSON Deserialized: Key: " + jsonPair.Key + ", Value: " + jsonPair.Value);

                // Serialize
                Debug.Log("JSON Serialize Object Test:\nJSON: " + Serialize(jsonPair));
            }
            catch (JsonException e)
            {
                Debug.Log("JSON Serialize Object Test Failed: " + e.Message);
            }

            // JSON Object List Test
            string jsonTest2 = @"[{""Key"": ""Key1"", ""Value"": 2}, {""Key"": ""Key2"", ""Value"": 4.5}]";
            Debug.Log("JSON Object List Test:\nJSON: " + jsonTest2);
            try
            {
                // Deserialize
                List<JsonKeyValuePair> jsonPairList = Deserialize<List<JsonKeyValuePair>>(jsonTest2);
                foreach (JsonKeyValuePair jsonPair1 in jsonPairList)
                {
                    Debug.Log("JSON Deserialized: Key: " + jsonPair1.Key + ", Value: " + jsonPair1.Value);
                }

                // Serialize
                Debug.Log("JSON Serialize Object List Test:\nJSON: " + Serialize(jsonPairList));
            }
            catch (JsonException e)
            {
                Debug.Log("JSON Serialize Object List Test Failed: " + e.Message);
            }


            // JSON Dictionary Test
            string jsonTest3 = @"{""randomKey1"": ""random_value"" ,""randomKey2"": 0.3, ""randomKey3"": 10, ""randomKey4"": [1, 2, 3]}";
            Debug.Log("JSON Deserialize Dictionary Test:\nJSON: " + jsonTest3);
            try
            {
                // Deserialize
                Dictionary<string, object> jsonPairDictionary = Deserialize<Dictionary<string, object>>(jsonTest3);
                foreach (KeyValuePair<string, object> jsonPair2 in jsonPairDictionary)
                {
                    Debug.Log("Key: " + jsonPair2.Key + ", Value: " + jsonPair2.Value);
                }

                // Serialize
                Debug.Log("JSON Serialize Dictionary Test:\nJSON: " + Serialize(jsonPairDictionary));
            }
            catch (JsonException e)
            {
                Debug.Log("JSON Serialize Dictionary Test Failed: " + e.Message);
            }
        }
    }
}