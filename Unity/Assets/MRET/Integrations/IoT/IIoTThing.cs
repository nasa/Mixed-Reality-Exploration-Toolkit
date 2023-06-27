// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    /// <remarks>
    /// History:
    /// 21 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// IIoTThing
	///
	/// An IoT "Thing" in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IIoTThing : IIdentifiable
    {
        /// <summary>
        /// Indicates whether or not this IoT Thing is a set of key/value pairs
        /// </summary>
        public bool IsPairs { get; }

        /// <summary>
        /// Converts the supplied JSON string to a collection of key/type pairs,
        /// i.e. string, float, int, etc.
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <returns>A <code>Dictionary<string, object></code> containing the conversion of
        ///     JSON key/value pairs to key/Type pairs</returns>
        public Dictionary<string, object> PairsJsonToType(string json);

        /// <summary>
        /// Converts the supplied JSON string to an object type instance, i.e. string,
        /// float, int, etc.
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <returns>A <code>Dictionary<string, object></code> containing the conversion of
        ///     JSON key/value pairs to key/Type pairs</returns>
        public object ValueJsonToType(string json);

        /// <summary>
        /// Converts the supplied JSON string to an object type instance, i.e. string,
        /// float, int, etc.
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <param name="timeStamp">The time stamp of the data</param>
        /// <returns>A <code>Dictionary<string, object></code> containing the conversion of
        ///     JSON key/value pairs to key/Type pairs</returns>
        public object ValueJsonToType(string json, out double timeStamp);

        /// <summary>
        /// AddThresholdsToDataValue adds the thresholds of this IoTThing into the datavalue passed in.
        /// This only works for payloads which are values, not pairs since the thresholds are specific
        /// to each element of the pairs list
        /// </summary>
        /// <param name="dataPoint">The <code>DataManager.DataValue</code> to update</param>
        /// <returns></returns>
        public DataManager.DataValue AddThresholdsToDataValue(DataManager.DataValue dataPoint);

        /// <seealso cref="IIdentifiable.CreateSerializedType"/>
        new public IoTThingType CreateSerializedType();

        /// <seealso cref="IIdentifiable.Synchronize(IdentifiableType, Action{bool, string})"/>
        public void Synchronize(IoTThingType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiable.Deserialize(IdentifiableType, Action{bool, string})"/>
        public void Deserialize(IoTThingType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiable.Serialize(IdentifiableType, Action{bool, string})"/>
        public void Serialize(IoTThingType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 21 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// IIoTThing
	///
	/// A <generic> IoT "Thing" in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IIoTThing<T> : IIdentifiable<T>, IIoTThing
        where T : IoTThingType
    {
    }
}
