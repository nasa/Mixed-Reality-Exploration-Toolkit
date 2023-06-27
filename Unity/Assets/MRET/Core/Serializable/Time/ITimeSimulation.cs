// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Time
{
    /// <remarks>
    /// History:
    /// 23 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ITimeSimulation
	///
	/// A time simulation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ITimeSimulation : IVersioned
    {
        /// <summary>
        /// The "long" human readable name of this time simulation.<br>
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The description of this time simulation.<br>
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// The start time for this time simulation
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The end time for this time simulation
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// The update rate in seconds for this time simulation.<br>
        /// </summary>
        public float UpdateRate { get; set; }

        /// <summary>
        /// Indicates if this time simulation is paused.<br>
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// Configures the TimeManager with the current time simulation settings
        /// </summary>
        /// <param name="reset">Indicated whether the time should be reset to the start time</param>
        /// <seealso cref="TimeManager"/>
        public void ConfigureTimeManager(bool reset);

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        new public TimeSimulationType CreateSerializedType();

        /// <seealso cref="IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(TimeSimulationType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(TimeSimulationType serialized, Action<bool, string> onFinished = null);

    }

    /// <remarks>
    /// History:
    /// 23 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ITimeSimulation
	///
	/// A <generic> time simulation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ITimeSimulation<T> : IVersioned<T>, ITimeSimulation
        where T : TimeSimulationType
    {
    }
}
