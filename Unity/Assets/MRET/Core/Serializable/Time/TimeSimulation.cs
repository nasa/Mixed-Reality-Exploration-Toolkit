// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Time
{
    /// <summary>
    /// Time simulation
    /// 
    /// Represents the information associated with a time simulation. This class has the ability to
    /// serialize/deserialize time simulations to/from TimeSimulationType and XML.<br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TimeSimulation : Versioned<TimeSimulationType>, ITimeSimulation<TimeSimulationType>
    {
        /// <seealso cref="Versioned{T}.ClassName"/>
        public override string ClassName => nameof(TimeSimulation);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private TimeSimulationType serializedTimeSimulation;

        public static readonly string DEFAULT_NAME= "Time Simulation";


        #region ITimeSimulation

        /// <seealso cref="ITimeSimulation.name"/>
        public string name { get; set; }

        /// <seealso cref="ITimeSimulation.description"/>
        public string description { get; set; }

        /// <seealso cref="ITimeSimulation.StartTime"/>
        public DateTime StartTime { get; set; }

        /// <seealso cref="ITimeSimulation.StartTime"/>
        public DateTime EndTime { get; set; }

        /// <seealso cref="ITimeSimulation.UpdateRate"/>
        public float UpdateRate { get; set; }

        /// <seealso cref="ITimeSimulation.Paused"/>
        public bool Paused { get; set; }

        /// <seealso cref="ITimeSimulation.ConfigureTimeManager(bool)"/>
        public void ConfigureTimeManager(bool reset)
        {
            if (MRET.TimeManager)
            {
                MRET.TimeManager.startTime = StartTime;
                MRET.TimeManager.endTime = EndTime;
                MRET.TimeManager.timeUpdateRate = UpdateRate;
                MRET.TimeManager.pause = Paused;
                if (reset) MRET.TimeManager.ResetTime();
            }
        }
        #endregion ITimeSimulation

        /// <summary>
        /// Loads this object instance with the deserialized time simulation file contents
        /// </summary>
        /// <param name="filePath">The time simulation file to deserialize</param>
        public void LoadFromXML(string filePath)
        {
            try
            {
                // Deserialize the file into our deserialized type class
                TimeSimulationType timeSimulationType = (TimeSimulationFileSchema.FromXML(filePath) as TimeSimulationType);

                // Deserialize the deserialized type class to produce our TimeSimulation
                Deserialize(timeSimulationType);
            }
            catch (Exception e)
            {
                LogWarning("A problem was encountered loading the XML file: " + e.Message, nameof(LoadFromXML));
            }
        }

        /// <summary>
        /// Serializes a time simulation object to a file
        /// </summary>
        /// <param name="filePath">The time simulation file to store the serialized object</param>
        public bool SaveToXML(string filePath)
        {
            bool result = false;

            try
            {
                // Setup our serialized action which will write the result to a file if successful
                TimeSimulationType serializedTimeSimulation = CreateSerializedType();
                Action<bool, string> SerializedAction = (bool serialized, string message) =>
                {
                    if (serialized)
                    {
                        // Write out the XML file
                        TimeSimulationFileSchema.ToXML(filePath, serializedTimeSimulation);
                        result = true;
                    }
                    else
                    {
                        string logMessage = "A problem occurred serializing the time simulation";
                        if (!string.IsNullOrEmpty(message))
                        {
                            logMessage += ": " + message;
                        }
                        LogWarning(logMessage, nameof(SaveToXML));
                    }
                };

                // Serialize this object instance into the serialized time simulation type
                Serialize(serializedTimeSimulation, SerializedAction);
            }
            catch (Exception e)
            {
                LogWarning("A problem was encountered saving the XML file: " + e.Message, nameof(SaveToXML));
            }

            return result;
        }

        #region Serializable
        /// <seealso cref="Versioned{T}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(TimeSimulationType serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization
            serializedTimeSimulation = serialized;

            // Deserialize the fields
            name = serializedTimeSimulation.Name;
            description = serializedTimeSimulation.Description;
            UpdateRate = serializedTimeSimulation.UpdateRate;
            Paused = serializedTimeSimulation.Paused;

            // Make sure we have an optional start time to process
            DateTime _startTime = DateTime.UtcNow;
            if (serializedTimeSimulation.StartTime != null)
            {
                SchemaUtil.DeserializeTime(serializedTimeSimulation.StartTime, ref _startTime);
            }
            StartTime = _startTime;

            // See if we have an optional end time to process
            DateTime _endTime = default;
            if (serializedTimeSimulation.EndTime != null)
            {
                SchemaUtil.DeserializeTime(serializedTimeSimulation.EndTime, ref _endTime);
            }
            EndTime = _endTime;

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <seealso cref="Versioned{T}.Serialize(T, SerializationState)"/>
        protected override void Serialize(TimeSimulationType serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization
            serialized.Name = name;
            serialized.Description = description;
            serialized.UpdateRate = UpdateRate;
            serialized.Paused = Paused;

            // Serialize out the start time
            serialized.StartTime = null;
            if (StartTime != default)
            {
                serialized.StartTime = new TimeType();
                SchemaUtil.SerializeTime(StartTime, serialized.StartTime);
            }

            // Serialize out the end time
            serialized.EndTime = null;
            if (EndTime != default)
            {
                serialized.EndTime = new TimeType();
                SchemaUtil.SerializeTime(EndTime, serialized.EndTime);
            }

            // Save the final serialized reference
            serializedTimeSimulation = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serializable

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            name = DEFAULT_NAME;
            description = "";
            StartTime = DateTime.SpecifyKind(default(DateTime), DateTimeKind.Utc);
            UpdateRate = 1f;
            Paused = false;
        }

        /// <summary>
        /// Constructor for the <code>TimeSimulation</code>
        /// </summary>
        public TimeSimulation() : base()
        {
        }

    }

}