// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Concurrent;

namespace GOV.NASA.GSFC.XR.MRET.Data
{
    public class DataManager : MRETUpdateManager<DataManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DataManager);

        public int defaultMaximumPreviousValues = 5;

        [System.Serializable]
        public class DataValueEvent : UnityEvent<DataValue>
        {

        }

        public class DataValue
        {
            /// <summary>
            /// A state for data point limits.
            /// </summary>
            //TODO: revisit Undefined, if we don't need Undefined then we can just replace the LimitState enum with Unity Color (and modify IoTThing to use Unity Color as well)
            public enum LimitState
            {
                Undefined,
                Black,
                Blue,
                Cyan,
                DarkGray,
                Gray,
                Green,
                LightGray,
                Magenta,
                Orange,
                Pink,
                Red,
                White,
                Yellow
            }

            public string key;
            public object value;
            public object minimum, maximum;
            public string category;
            public int MaxEntriesWithSameKey = Instance.defaultMaximumPreviousValues;
            //Time is the time stamp for when the data was entered into the datamanager
            public DateTime time;
            //telemetryTS is the time stamp passed in along with the telemetry data, presumably the time stamp for when the measurement was taken
            public DateTime telemetryTS;
            public List<(LimitState, object, object)> arbitraryColorThresholds = new List<(LimitState, object, object)>();

            public List<DataValue> previousValues;

            /// <summary>
            /// The current limit state corresponding to the data point's value.
            /// </summary>
            //TODO: consider revisiting this: passing thresholds to the datamanager for every DataValue is inefficient. Maybe just pass the current color instead.
            public LimitState limitState
            {
                get
                {
                    if (IsNumber() == false)
                    {
                        return LimitState.Undefined;
                    }

                    if (arbitraryColorThresholds == null || arbitraryColorThresholds.Count == 0)
                    {
                        return LimitState.Undefined;
                    }

                    LimitState output = LimitState.Undefined;
                    foreach ((LimitState, object, object) threshold in arbitraryColorThresholds)
                    {
                        if (Convert.ToDouble(value) >= Convert.ToDouble(threshold.Item2) && Convert.ToDouble(value) <= Convert.ToDouble(threshold.Item3))
                        {
                            output = threshold.Item1;
                        }
                    }

                    return output;
                }
            }

            public DataValue(string _key, object _value, string _category = "none", DateTime? _time = null,
                object _oldMin = null, object _oldMax = null)
            {
                previousValues = new List<DataValue>();
                key = _key;
                value = _value;
                category = _category;
                time = _time ?? DateTime.Now;

                if (_oldMin != null)
                {
                    minimum = _oldMin;
                    UpdateMin(_value);
                }
                else
                {
                    minimum = _value;
                }

                if (_oldMax != null)
                {
                    maximum = _oldMax;
                    UpdateMax(_value);
                }
                else
                {
                    maximum = _value;
                }
            }
            public void UpdateValue(DataValue newValue)
            {
                UpdateMinandMax(newValue.value);
                previousValues.Add(new DataValue(key, value, category, time));
                if (previousValues.Count > MaxEntriesWithSameKey)
                {
                    previousValues.RemoveAt(0);
                }
                key = newValue.key;
                value = newValue.value;
                category = newValue.category;
                time = newValue.time;
            }
            public void UpdateValue(string _key, object _value, string _category = "none", DateTime? _time = null)
            {
                UpdateMinandMax(_value);
                previousValues.Add(new DataValue(key, value, category, time));
                if (previousValues.Count > MaxEntriesWithSameKey)
                {
                    previousValues.RemoveAt(0);
                }
                key = _key;
                value = _value;
                category = _category;
                time = _time ?? DateTime.Now;
            }

            public bool IsNumber()
            {
                if (value is double || value is float || value is int || value is uint || value is short
                    || value is ushort || value is long || value is ulong || value is decimal
                    || value is byte || value is sbyte)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private void UpdateMinandMax(object newValue)
            {
                UpdateMin(newValue);
                UpdateMax(newValue);
            }

            private void UpdateMin(object newValue)
            {
                // Update minimum value.
                if (newValue is double && minimum is double)
                {
                    if ((double)newValue < (double)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is float && minimum is float)
                {
                    if ((float)newValue < (float)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is int && minimum is int)
                {
                    if ((int)newValue < (int)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is uint && minimum is uint)
                {
                    if ((uint)newValue < (uint)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is short && minimum is short)
                {
                    if ((short)newValue < (short)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is ushort && minimum is ushort)
                {
                    if ((ushort)newValue < (ushort)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is long && minimum is long)
                {
                    if ((long)newValue < (long)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is ulong && minimum is ulong)
                {
                    if ((ulong)newValue < (ulong)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is decimal && minimum is decimal)
                {
                    if ((decimal)newValue < (decimal)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is byte && minimum is byte)
                {
                    if ((byte)newValue < (byte)minimum)
                    {
                        minimum = newValue;
                    }
                }
                else if (newValue is sbyte && minimum is sbyte)
                {
                    if ((sbyte)newValue < (sbyte)minimum)
                    {
                        minimum = newValue;
                    }
                }
            }

            private void UpdateMax(object newValue)
            {
                // Update maximum value.
                if (newValue is double && maximum is double)
                {
                    if ((double)newValue > (double)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is float && maximum is float)
                {
                    if ((float)newValue > (float)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is int && maximum is int)
                {
                    if ((int)newValue > (int)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is uint && maximum is uint)
                {
                    if ((uint)newValue > (uint)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is short && maximum is short)
                {
                    if ((short)newValue > (short)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is ushort && maximum is ushort)
                {
                    if ((ushort)newValue > (ushort)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is long && maximum is long)
                {
                    if ((long)newValue > (long)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is ulong && maximum is ulong)
                {
                    if ((ulong)newValue > (ulong)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is decimal && maximum is decimal)
                {
                    if ((decimal)newValue > (decimal)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is byte && maximum is byte)
                {
                    if ((byte)newValue > (byte)maximum)
                    {
                        maximum = newValue;
                    }
                }
                else if (newValue is sbyte && maximum is sbyte)
                {
                    if ((sbyte)newValue > (sbyte)maximum)
                    {
                        maximum = newValue;
                    }
                }
            }
            public void clearThresholds()
            {
                arbitraryColorThresholds.Clear();
            }
        }

        public bool archiveData = false;

        // These are the saved data points.
        private ConcurrentDictionary<string, DataValue> dataPoints;

        // These are events that correspond to each data point.
        private Dictionary<string, List<DataValueEvent>> dataPointEvents = new Dictionary<string, List<DataValueEvent>>();

        //These are events that have triggered on other threads, so the main thread needs to go through them
        private ConcurrentQueue<(DataValueEvent, DataValue)> otherThreadEvents;

        private static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public object FindPoint(string pointName)
        {
            return SearchValue(pointName);
        }

        public DataValue FindCompletePoint(string pointName)
        {
            return SearchPoint(pointName);
        }

        public void SubscribeToPoint(string pointKey, DataValueEvent eventToInvoke)
        {
            if (!dataPointEvents.ContainsKey(pointKey))
            {
                dataPointEvents[pointKey] = new List<DataValueEvent>();
            }

            if (dataPointEvents[pointKey] == null)
            {
                dataPointEvents[pointKey] = new List<DataValueEvent>();
            }

            dataPointEvents[pointKey].Add(eventToInvoke);
        }

        public void UnsubscribeFromPoint(string pointKey, DataValueEvent eventToInvoke)
        {
            if (dataPointEvents.ContainsKey(pointKey))
            {
                dataPointEvents[pointKey].RemoveAll(x => x == eventToInvoke);

                if (dataPointEvents.ContainsKey(pointKey))
                {
                    if (dataPointEvents[pointKey].Count == 0)
                    {
                        dataPointEvents.Remove(pointKey);
                        return;
                    }
                }
            }

            LogWarning("Subscription not found.", nameof(UnsubscribeFromPoint));
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            Log("Initializing empty Data Manager...", nameof(Initialize));
            dataPoints = new ConcurrentDictionary<string, DataValue>();
            dataPointEvents = new Dictionary<string, List<DataValueEvent>>();
            otherThreadEvents = new ConcurrentQueue<(DataValueEvent, DataValue)>();
            Log("Empty Data Manager Initialized.", nameof(Initialize));
        }


        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();
            (DataValueEvent, DataValue) task = (null, null);
            while (otherThreadEvents.TryDequeue(out task))
            {
                //invoke the event using the included datavalue
                task.Item1.Invoke(task.Item2);
            }
        }

        public void LoadFromCSV(string fileName)
        {
            try
            {
                StreamReader saveFile = new StreamReader(fileName);

                while (!saveFile.EndOfStream)
                {
                    DataValue newValue = null;
                    string line = saveFile.ReadLine();
                    bool currentValueSet = false;
                    foreach (string savedValue in line.Split(';'))
                    {
                        string[] attributes = savedValue.Split(',');
                        string name = "";
                        if (!currentValueSet)
                        {
                            name = attributes[0];
                            DateTime pointTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            pointTime = pointTime.AddMilliseconds(double.Parse(attributes[4])).ToLocalTime();
                            newValue = new DataValue(name, TypeNameToType(attributes[3], attributes[1]), attributes[2],
                                pointTime);
                            currentValueSet = true;
                        }
                        else if (newValue != null)
                        {
                            DateTime pointTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            pointTime = pointTime.AddMilliseconds(double.Parse(attributes[3])).ToLocalTime();
                            newValue.previousValues.Add(new DataValue(name, TypeNameToType(attributes[2], attributes[0]), attributes[1],
                                pointTime));
                        }
                    }
                    SaveValue(newValue);
                }
            }
            catch (Exception e)
            {
                LogError("A problem occured loading from CSV: " + fileName + "; " + e.ToString(), nameof(LoadFromCSV));
            }
        }

        public void SaveToCSV(string fileName)
        {
            try
            {
                using (StreamWriter saveFile = new StreamWriter(fileName))
                {
                    saveFile.Write("");

                    //foreach (DataValue dataPoint in dataPoints)
                    foreach (KeyValuePair<string, DataValue> dataPoint in dataPoints)
                    {
                        string csvString = "";

                        // Add data point.
                        csvString = csvString + dataPoint.Value.key + "," + TypeToTypeName(dataPoint.Value.value)
                            + "," + dataPoint.Value.category + "," + dataPoint.Value.value + ","
                            + dataPoint.Value.time.ToUniversalTime().Subtract(epochStart).TotalMilliseconds
                            + "," + dataPoint.Value.arbitraryColorThresholds;
                        if (archiveData)
                        {
                            foreach (DataValue savedPoint in dataPoint.Value.previousValues)
                            {
                                csvString = csvString + ";" + TypeToTypeName(savedPoint.value) + ","
                                    + savedPoint.category + "," + savedPoint.value + ","
                                    + savedPoint.time.ToUniversalTime().Subtract(epochStart).TotalMilliseconds
                                    + "," + savedPoint.arbitraryColorThresholds;
                            }
                        }

                        // Write data point to file.
                        saveFile.WriteLine(csvString);
                    }
                }
            }
            catch (Exception e)
            {
                LogError("A problem occured saving to CSV: " + fileName + "; " + e.ToString(), nameof(SaveToCSV));
            }
        }


        //TODO: convert unix time and slap that in the datavalue
        public void SaveValue(DataValue valueToSave, int timestamp = 0)
        {
            string key = valueToSave.key;

            if (dataPoints.ContainsKey(key))
            {
                // Point exists.
                if (archiveData)
                {   // Add current value to point.
                    dataPoints[key].UpdateValue(valueToSave);
                }
                else
                {
                    // Overwrite point value;
                    valueToSave.minimum = dataPoints[key].minimum;
                    valueToSave.maximum = dataPoints[key].maximum;
                    dataPoints[key] = valueToSave;
                }
            }
            else
            {
                // Point does not already exist.
                dataPoints[key] = valueToSave;
            }
            if (dataPointEvents.ContainsKey(valueToSave.key))
            {
                foreach (DataValueEvent dve in dataPointEvents[valueToSave.key])
                {
                    otherThreadEvents.Enqueue((dve, valueToSave));
                }
            }
        }

        public void SaveValue(string key, object value, string category = "none", DateTime? time = null,
            object redLow = null, object yellowLow = null, object yellowHigh = null, object redHigh = null)
        {
            if (dataPoints.ContainsKey(key))
            {
                // Point exists.
                if (archiveData)
                {   // Add current value to point.
                    dataPoints[key].UpdateValue(key, value, category, time);
                }
                else
                {
                    // Overwrite point value;
                    object oldMin = dataPoints[key].minimum;
                    object oldMax = dataPoints[key].maximum;
                    dataPoints[key] = new DataValue(key, value, category, time, oldMin, oldMax);
                }
            }
            else
            {
                // Point does not already exist.
                dataPoints[key] = new DataValue(key, value, category, time);
            }

            if (dataPointEvents.ContainsKey(key))
            {
                foreach (DataValueEvent dve in dataPointEvents[key])
                {
                    otherThreadEvents.Enqueue((dve, dataPoints[key]));
                    //dve.Invoke(dataPoints[key]);
                }
            }
        }

        private object SearchValue(string key)
        {
            DataValue val = null;
            dataPoints.TryGetValue(key, out val);
            return (val == null) ? null : val.value;
        }

        private DataValue SearchPoint(string key)
        {
            DataValue val = null;
            dataPoints.TryGetValue(key, out val);
            return (val == null) ? null : val;
        }

        private object SearchValueAtTime(string key, DateTime time)
        {
            int i = 0;
            DataValue pointOfInterest = dataPoints[key];
            foreach (DataValue val in pointOfInterest.previousValues)
            {
                if (val.time > time)
                {
                    // We have passed the time we are looking for.
                    if (i > 0)
                    {
                        // There is a valid previous value that would have applied at the specified time.
                        return pointOfInterest.previousValues[i - 1].value;
                    }
                    else
                    {
                        // There are no valid previous values.
                        if (pointOfInterest.time <= time)
                        {
                            // The current value would have been valid.
                            return pointOfInterest.value;
                        }
                        else
                        {
                            // There have been no valid values.
                            return null;
                        }
                    }
                }
                i++;
            }

            // There are no valid previous values.
            if (pointOfInterest.time <= time)
            {
                // The current value would have been valid.
                return pointOfInterest.value;
            }
            else
            {
                // There have been no valid values.
                return null;
            }
        }

        public DataValue[] GetAllDataPoints()
        {
            DataValue[] dVs = new DataValue[dataPoints.Count];
            dataPoints.Values.CopyTo(dVs, 0);
            return dVs;
        }

        #region HELPERS
        private string TypeToTypeName(object objectOfType)
        {
            string typeName = "str";
            Type t = objectOfType.GetType();
            if (t == typeof(int))
            {
                typeName = "int";
            }
            else if (t == typeof(long))
            {
                typeName = "lng";
            }
            else if (t == typeof(float))
            {
                typeName = "flt";
            }
            else if (t == typeof(double))
            {
                typeName = "dbl";
            }
            else if (t == typeof(byte))
            {
                typeName = "byt";
            }
            else if (t == typeof(bool))
            {
                typeName = "bol";
            }

            return typeName;
        }

        private object TypeNameToType(string objectString, string typeName)
        {
            if (objectString == "")
            {
                return null;
            }

            switch (typeName)
            {
                case "int":
                    return int.Parse(objectString);

                case "lng":
                    return long.Parse(objectString);

                case "flt":
                    return float.Parse(objectString);

                case "dbl":
                    return double.Parse(objectString);

                case "byt":
                    return byte.Parse(objectString);

                case "bol":
                    return bool.Parse(objectString);

                case "str":
                default:
                    return objectString;
            }
        }
        #endregion Helpers
    }
}