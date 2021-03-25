// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas.TimeSimulationTypes;

namespace GSFC.ARVR.MRET.Time.Simulation
{
    /**
    * Time simulation
    * 
    * Represents the information associated with a time simulation. This class has the ability to
    * serialize/deserialize time simulations to/from TimeSimulationType and XML.<br>
    * 
    * @author Jeffrey Hosler
    */
    public class TimeSimulation : MonoBehaviour
    {
        public static readonly string NAME = nameof(TimeSimulation);
        public static readonly string DEFAULT_NAME= "Time Simulation";

        public string simulationName = DEFAULT_NAME;
        public string simulationDescription = "";
        public DateTime startTime = default(DateTime);
        public float updateRate = 1f;
        public bool paused = false;

        /**
         * Only called once after enabled
         */
        private void Start()
        {
            startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
        }

        /**
         * Loads this object instance with the deserialized time simulation file contents
         * 
         * @param filePath The time simulation file to deserialize
         */
        public void LoadFromXML(string filePath)
        {
            try
            {
                // Deserialize the file into our deserialized type class
                TimeSimulationType timeSimulationType = (TimeSimulationFileSchema.FromXML(filePath) as TimeSimulationType);

                // Deserialize the deserialized type class to produce our TimeSimulation
                StartCoroutine(Deserialize(timeSimulationType));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + NAME + "->LoadFromXML] A problem was encountered loading the XML file: " + e.ToString());
            }
        }

        /**
         * Serializes a time simulation object to a file
         * 
         * @param filePath The time simulation file to store the serialized object
         */
        public void SaveToXML(string filePath)
        {
            // Serialize this object instance into the serialized time simulation type
            TimeSimulationType timeSimulationType = Serialize();

            try
            {
                // Serialize the time simulation type to file
                TimeSimulationFileSchema.ToXML(filePath, timeSimulationType);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + NAME + "->SaveToXML] A problem was encountered saving the XML file: " + e.ToString());
            }
        }

        /**
         * Serializes this object to a TimeSimulationType<br>
         * 
         * @return The <code>TimeSimulationType</code> representing the serialized contents
         *      of this time simulation instance
         *      
         * @see TimeSimulationType
         */
        public TimeSimulationType Serialize()
        {
            TimeSimulationType serialized = new TimeSimulationType();

            serialized.Name = simulationName;
            serialized.Description = simulationDescription;
            serialized.UpdateRate = updateRate;
            serialized.Paused = paused;

            // Serialize out the time
            if (startTime != default(DateTime))
            {
                serialized.StartTime = new TimeType();
                serialized.StartTime.ItemsElementName = new ItemsChoiceType[1] { ItemsChoiceType.TimeString };

                // Keep it simple and just use the string format, but convert to UTC
                TimeStringType timeStrType = new TimeStringType();
                timeStrType.Value = startTime.ToUniversalTime().ToString(timeStrType.format);
                serialized.StartTime.Items = new object[1] { timeStrType };
            }
            else
            {
                // If not set, we don't want to serialize out a time
                serialized.StartTime = null;
            }

            return serialized;
        }

        /**
         * Deserializes the supplied TimeSimulationType into this time simulation instance<br>
         * 
         * @return The <code>TimeSimulationType</code> representing the serialized information
         *      used to initialize this time simulation instance
         *      
         * @see TimeSimulationType
         */
        public IEnumerator Deserialize(TimeSimulationType timeSimulationType)
        {
            if (timeSimulationType == null)
            {
                return null;
            }

            if (String.IsNullOrEmpty(timeSimulationType.Name))
            {
                return null;
            }

            simulationName = timeSimulationType.Name;
            simulationDescription = timeSimulationType.Description;
            updateRate = timeSimulationType.UpdateRate;
            paused = timeSimulationType.Paused;

            // Make sure we have an optional start time to process
            if (timeSimulationType.StartTime != null)
            {
                int year = 0, month = 0, day = 0;
                int hour = 0, minute = 0, second = 0, millis = 0;
                DateTimeKind timeZone = DateTimeKind.Utc;
                TimeStringType timeStrType = null;
                for (int i = 0; i < timeSimulationType.StartTime.Items.Length; i++)
                {
                    ItemsChoiceType itemsChoice = timeSimulationType.StartTime.ItemsElementName[i];
                    switch (itemsChoice)
                    {
                        case ItemsChoiceType.TimeString:
                            timeStrType = (TimeStringType)timeSimulationType.StartTime.Items[i];
                            break;
                        case ItemsChoiceType.Year:
                            int.TryParse(timeSimulationType.StartTime.Items[i].ToString(), out year);
                            break;
                        case ItemsChoiceType.Month:
                            int.TryParse(timeSimulationType.StartTime.Items[i].ToString(), out month);
                            break;
                        case ItemsChoiceType.Day:
                            int.TryParse(timeSimulationType.StartTime.Items[i].ToString(), out day);
                            break;
                        case ItemsChoiceType.Hour:
                            int.TryParse(timeSimulationType.StartTime.Items[i].ToString(), out hour);
                            break;
                        case ItemsChoiceType.Minute:
                            int.TryParse(timeSimulationType.StartTime.Items[i].ToString(), out minute);
                            break;
                        case ItemsChoiceType.Second:
                            int.TryParse(timeSimulationType.StartTime.Items[i].ToString(), out second);
                            break;
                        case ItemsChoiceType.Millisecond:
                            int.TryParse(timeSimulationType.StartTime.Items[i].ToString(), out millis);
                            break;
                        case ItemsChoiceType.TimeZone:
                            // Check for explicit local timezone, otherwise just leave the default UTC
                            if (((TimeZoneType)timeSimulationType.StartTime.Items[i]) == TimeZoneType.Local)
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
                    DateTime.TryParse(timeStrType.Value, out startTime);
                    if (startTime.Kind == DateTimeKind.Unspecified)
                    {
                        startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                    }
                }
                else
                {
                    // Default to the time components
                    startTime = new DateTime(year, month, day, hour, minute, second, millis, timeZone);
                }
            }
            else
            {
                // None specified so use the current time
                startTime = DateTime.UtcNow;
            }

            return null;
        }


    }

}